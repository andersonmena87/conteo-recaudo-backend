using ConteoRecaudo.BLL.Interfaces;
using ConteoRecaudo.Helpers;
using ConteoRecaudo.Helpers.Converts;
using ConteoRecaudo.Helpers.HttpExeptionHelper;
using ConteoRecaudo.Infraestructure.Interfaces;
using ConteoRecaudo.Models;
using ConteoRecaudo.Properties;
using ConteoRecaudo.Services.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Numerics;

namespace ConteoRecaudo.BLL
{
    public class RecaudoBL : IRecaudoBL
    {
        private IRecaudoRepository _recuadoRepository;
        private IConteoApi _conteoApi;
        private IRecaudoApi _recaudoApi;
        private IConfiguration _configuracion { get; }

        public RecaudoBL(IRecaudoRepository recaudoRepository, IConteoApi conteoApi, IRecaudoApi recaudoApi, IConfiguration configuracion)
        {
            _recuadoRepository = recaudoRepository;
            _conteoApi = conteoApi;
            _recaudoApi = recaudoApi;
            _configuracion = configuracion;
        }

        public async Task<List<ConteoRecaudoModel>> GetRecaudos()
             => await _recuadoRepository.GetRecaudos();

        public async Task<bool> GuardarRecaudos(string token, DateTime fechaIncio, DateTime fechaFin)
        {
            List<ConteoRecaudoModel> recaudos = new();
            DateTime FechaInicio = fechaIncio;
            bool datosAlmacenados = false;

            try
            {
                while (FechaInicio <= fechaFin)
                {
                    string fecha = FechaInicio.ToString("yyyy-MM-dd");
                    List<ConteoModel> conteosDelDia = await _conteoApi.GetConteos(token, fecha);
                    List<RecaudoModel> recaudosDelDia = await _recaudoApi.GetRecaudos(token, fecha);
                    recaudos.AddRange(UnirListas(conteosDelDia, recaudosDelDia, FechaInicio));
                    FechaInicio = FechaInicio.AddDays(1);
                }

                if (recaudos.Count > 0)
                {
                    foreach (var recaudo in recaudos)
                    {
                        await _recuadoRepository.GuardarRecaudo(ConvertRecaudo.ToEntity(recaudo));
                    }
                    datosAlmacenados = true;

                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return datosAlmacenados;

        }

        private List<ConteoRecaudoModel> UnirListas(List<ConteoModel> conteosDelDia, List<RecaudoModel> recaudosDelDia, DateTime FechaRecaudo)
        {
            List<ConteoRecaudoModel> listaUnida = (
                from conteo in conteosDelDia
                join recaudo in recaudosDelDia on new { conteo.Estacion, conteo.Sentido, conteo.Hora, conteo.Categoria }
                                                equals new { recaudo.Estacion, recaudo.Sentido, recaudo.Hora, recaudo.Categoria }
                select new ConteoRecaudoModel
                {
                    Estacion = conteo.Estacion,
                    Sentido = conteo.Sentido,
                    Hora = conteo.Hora,
                    Categoria = conteo.Categoria,
                    Cantidad = conteo.Cantidad,
                    ValorTabulado = recaudo.ValorTabulado,
                    FechaRecaudo = FechaRecaudo
                }
            ).ToList();

            return listaUnida;
        }

        public string ObtenerRutaArchivo(DateTime fechaInicial, DateTime fechaFinal)
        {
            string NombreArchivo;
            string Nombre = _configuracion.GetValue<string>("ArchivoRecaudo:NombreArchivo");
            string Extension = _configuracion.GetValue<string>("ArchivoRecaudo:Extension");
            if (string.IsNullOrEmpty(Nombre))
            {
                throw new HttpException(500, Resources.ErrorNombreArchivo);
            }
            if (string.IsNullOrEmpty(Extension))
            {
                throw new HttpException(500, Resources.ErrorExtensionArchivo);
            }
            NombreArchivo = string.Format("{0}{1}-{2}", Nombre, fechaInicial.ToShortDateString().Replace("/", ""), fechaFinal.ToShortDateString().Replace("/", ""));
            NombreArchivo = string.Format("{0}.{1}", NombreArchivo, Extension);
            return NombreArchivo;
        }

        public async Task<ArchivoRecaudoExcel> ExportarExcel(DateTime fechaInicial, DateTime fechaFinal)
        {
            if (!Helper.FechasValidas(fechaInicial, fechaFinal)) {
                throw new HttpException(500, Resources.ErrorRangoFechas);
            }

            List<ReporteRecaudoExcel> recaudos = await _recuadoRepository.ObtenerRecaudosxFechas(fechaInicial, fechaFinal);

            DataTable dataTable = ConvertToDataTable(recaudos);

            string nombreArchivo = ObtenerRutaArchivo(fechaInicial, fechaFinal);
            string nombreHoja = Path.GetFileNameWithoutExtension(nombreArchivo);

            string archivo = Path.Combine(Path.GetTempPath(), nombreArchivo);
            
            if (File.Exists(archivo))
            {
                File.Delete(archivo);
            }

            using (ExcelPackage package = new ExcelPackage(new FileInfo(archivo)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(nombreHoja);

                // Escribir la columna vacía en la fila 1
                worksheet.Cells[1, 1].Value = string.Empty;

                // Escribir las estaciones en la fila 1
                int columnIndex = 2;
                foreach (DataColumn column in dataTable.Columns.Cast<DataColumn>())
                {

                    string columnName = column.ColumnName;
                    if (columnName != "FechaRecaudo" && !columnName.Contains("_TotalValorTabulado"))
                    {
                        worksheet.Cells[1, columnIndex].Value = columnName;
                        worksheet.Cells[1, columnIndex, 1, columnIndex + 1].Merge = true;
                        columnIndex += 2;
                    }
                    
                }

                // Escribir los datos en el archivo Excel
                int rowIndex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    
                    columnIndex = 1;
                    foreach (var item in row.ItemArray)
                    {
                        var cell = worksheet.Cells[rowIndex, columnIndex];

                        if (item is DateTime fechaRecaudo)
                        {
                            cell.Value = fechaRecaudo.ToShortDateString();
                            cell.Style.Numberformat.Format = "yyyy-MM-dd";
                        }

                        else if (columnIndex % 2 == 0 && item is BigInteger bigInteger)
                        {
                            double valorNumerico = (double)bigInteger;
                            cell.Value = valorNumerico;
                            cell.Style.Numberformat.Format = "#";
                        }

                        else if (columnIndex % 2 != 0 && item is BigInteger bigInteger1)
                        {
                            double valorNumerico = (double)bigInteger1;
                            cell.Value = valorNumerico;
                            cell.Style.Numberformat.Format = "$#,##0";
                        }
                        else {
                            cell.Value = item;
                        }

                        columnIndex++;
                    }

                    rowIndex++;
                }

                // Calcular y escribir las sumatorias
                columnIndex = 2;
                
                worksheet.Cells[rowIndex, 1].Value = "Total";

                foreach (DataColumn column in dataTable.Columns.Cast<DataColumn>().Skip(1))
                {
                    var cell = worksheet.Cells[rowIndex, columnIndex];
                    cell.Formula = "SUM(" + GetExcelColumnLetter(columnIndex) + "2:" + GetExcelColumnLetter(columnIndex) + (rowIndex - 1) + ")";
                    cell.Style.Numberformat.Format = worksheet.Cells[2, columnIndex].Style.Numberformat.Format;
                    columnIndex++;
                }

                rowIndex += 2;

                // Filas de totales
                worksheet.Cells[rowIndex, 1].Value = "Totales";
                worksheet.Cells[rowIndex, 1, rowIndex + 1, 1].Merge = true;

                // Fila Totales: sumatoria de la fila total columnas pares
                columnIndex = 2;
                rowIndex++;

                string sumFormulaColumn = string.Empty;

                for (int i = 2; i <= dataTable.Columns.Count - 1; i += 2)
                {
                    string columnLetter = GetExcelColumnLetter(i);
                    sumFormulaColumn += (sumFormulaColumn == string.Empty ? string.Empty : "+") + columnLetter + (rowIndex - 3);
                }

                var cellColumn2 = worksheet.Cells[rowIndex - 1, columnIndex];
                cellColumn2.Formula = "SUM(" + sumFormulaColumn + ")";
                cellColumn2.Style.Numberformat.Format = worksheet.Cells[2, columnIndex].Style.Numberformat.Format;


                // Fila Totales: sumatoria de la fila total columnas impares
                rowIndex++;
                columnIndex = 2;

                sumFormulaColumn = string.Empty;
                for (int i = 3; i <= dataTable.Columns.Count; i += 2)
                {
                    string columnLetter = GetExcelColumnLetter(i);
                    sumFormulaColumn += (sumFormulaColumn == string.Empty ? string.Empty : "+") + columnLetter + (rowIndex - 4);
                }

                var cellColumn3 = worksheet.Cells[rowIndex - 1, columnIndex];
                cellColumn3.Formula = "SUM(" + sumFormulaColumn + ")";
                cellColumn3.Style.Numberformat.Format = "$#,##0";

                // Obtener el rango de la tabla
                ExcelRange tableRange = worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];

                // Obtener las filas que contienen datos
                IEnumerable<ExcelRangeBase> dataRows = tableRange
                    .Where(cell => !string.IsNullOrWhiteSpace(cell.Text) || !string.IsNullOrEmpty(cell.Formula))
                    .Select(cell => cell.Start.Row)
                    .Distinct()
                    .Select(row => worksheet.Cells[row, 1, row, worksheet.Dimension.End.Column]);

                // Aplicar bordes adicionales a las filas con datos
                foreach (ExcelRangeBase row in dataRows)
                {
                    row.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    row.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    row.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    row.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Aplicar bordes a la primera fila
                ExcelRange headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];
                headerRow.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                int lastRow = worksheet.Dimension.End.Row;
                int lastColumn = worksheet.Dimension.End.Column;

                // Quitar los bordes de las últimas dos filas desde la columna 3 en adelante
                ExcelRangeBase rangeToRemoveBorders = worksheet.Cells[lastRow - 1, 3, lastRow, lastColumn];
                rangeToRemoveBorders.Style.Border.Top.Style = ExcelBorderStyle.None;
                rangeToRemoveBorders.Style.Border.Bottom.Style = ExcelBorderStyle.None;
                rangeToRemoveBorders.Style.Border.Left.Style = ExcelBorderStyle.None;
                rangeToRemoveBorders.Style.Border.Right.Style = ExcelBorderStyle.None;

                // Establecer el formato de las celdas
                worksheet.Cells.Style.Font.Size = 10;
                worksheet.Cells.Style.Font.Name = "Arial";
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Calculate();
                worksheet.Cells.AutoFitColumns();

                // Guardar el archivo Excel
                package.Save();
            }

            byte[] bytesArchivo = await File.ReadAllBytesAsync(archivo);
            ArchivoRecaudoExcel descarga = new()
            {
                Archivo = bytesArchivo,
                NombreArchivo = nombreArchivo
            };

            return descarga;
        }

        private string GetExcelColumnLetter(int columnNumber)
        {
            int dividend = columnNumber;
            string columnLetter = string.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
                dividend = (dividend - modulo) / 26;
            }

            return columnLetter;
        }


        private DataTable ConvertToDataTable(List<ReporteRecaudoExcel> recaudos)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("FechaRecaudo", typeof(DateTime));

            var estaciones = recaudos.SelectMany(r => r.Estaciones.Select(e => e.Estacion)).Distinct();

            foreach (var estacion in estaciones)
            {
                dataTable.Columns.Add(estacion, typeof(BigInteger));
                dataTable.Columns.Add(estacion + "_TotalValorTabulado", typeof(BigInteger));
            }

            foreach (var recaudo in recaudos)
            {
                DataRow row = dataTable.NewRow();

                row["FechaRecaudo"] = recaudo.FechaRecaudo;

                foreach (var estacion in recaudo.Estaciones)
                {
                    row[estacion.Estacion] = estacion.TotalCantidad;
                    row[estacion.Estacion + "_TotalValorTabulado"] = estacion.TotalValorTabulado;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

    }
}
