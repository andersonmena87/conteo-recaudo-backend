using ClosedXML.Excel;
using ConteoRecaudo.BLL.Interfaces;
using ConteoRecaudo.Helpers;
using ConteoRecaudo.Helpers.Converts;
using ConteoRecaudo.Helpers.HttpExeptionHelper;
using ConteoRecaudo.Infraestructure.Interfaces;
using ConteoRecaudo.Models;
using ConteoRecaudo.Properties;
using ConteoRecaudo.Services.Interfaces;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using System.Numerics;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;

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

        private async Task<byte[]> ObtenerBytesArchivo(string Archivo)
        {
            byte[]? bytesArchivo = null;
            if (File.Exists(Archivo))
            {
                bytesArchivo = await File.ReadAllBytesAsync(Archivo);
                // File.Delete(Archivo);
                Helper.CrearDirectorio(Path.GetDirectoryName(Archivo));
            }
            else
            {
                throw new HttpException(404, string.Format(Resources.ErrorArchivo, Archivo));
            }

            return bytesArchivo;
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
            List<ReporteRecaudoExcel> recaudos = await _recuadoRepository.ObtenerRecaudosxFechas(fechaInicial, fechaFinal);

            DataTable dataTable = ConvertToDataTable(recaudos);

            string nombreArchivo = ObtenerRutaArchivo(fechaInicial, fechaFinal);
            string nombreHoja = Path.GetFileNameWithoutExtension(nombreArchivo);

            string archivo = Path.Combine(ObtenerRutaDirectorio(), nombreArchivo);
            
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


                        // Formatear el campo "FechaRecaudo" como fecha
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
                rowIndex++;

                worksheet.Cells[rowIndex, 1].Value = "Total";

                foreach (DataColumn column in dataTable.Columns.Cast<DataColumn>().Skip(1))
                {
                    double sum = dataTable.AsEnumerable().Sum(row =>
                    {
                        if (row[column.ColumnName] is BigInteger bigInteger)
                        {
                            return (double)bigInteger;
                        }
                        return Convert.ToDouble(row[column.ColumnName]);
                    });

                    var cell = worksheet.Cells[rowIndex, columnIndex];
                    cell.Value = sum;
                    cell.Style.Numberformat.Format = worksheet.Cells[2, columnIndex].Style.Numberformat.Format;
                    columnIndex++;
                }

                // Calcular y escribir las sumatorias
                rowIndex++;

                // Filas de totales
                worksheet.Cells[rowIndex, 1].Value = "Totales";
                worksheet.Cells[rowIndex, 1, rowIndex + 1, 1].Merge = true;

                // Calcular y escribir las sumatorias
                columnIndex = 2;
                rowIndex++;

                double sumColumn2 = 0;
                for (int i = 1; i <= dataTable.Columns.Count - 1; i += 2)
                {
                    double sumColumn = 0;
                    // Calcular la sumatoria de la columna
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row[i] is BigInteger bigInteger)
                            sumColumn += (double)bigInteger;
                    }
                    sumColumn2 += sumColumn;
                }

                var cellColumn2 = worksheet.Cells[rowIndex - 1, columnIndex];
                cellColumn2.Value = sumColumn2;
                cellColumn2.Style.Numberformat.Format = worksheet.Cells[2, columnIndex].Style.Numberformat.Format;

                // Fila Totales: sumatoria de la fila total columnas 3, 5, 7
                rowIndex++;
                columnIndex = 2;
                

                double sumColumn3 = 0;
                for (int i = 2; i <= dataTable.Columns.Count - 1; i += 2)
                {
                    double sumColumn = 0;
                    // Calcular la sumatoria de la columna
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row[i] is BigInteger bigInteger)
                            sumColumn += (double)bigInteger;
                    }
                    sumColumn3 += sumColumn;
                }

                var cellColumn3 = worksheet.Cells[rowIndex - 1, columnIndex];
                cellColumn3.Value = sumColumn3;
                cellColumn3.Style.Numberformat.Format = "$#,##0";

                // Establecer el formato de las celdas
                worksheet.Cells.AutoFitColumns();
                worksheet.Cells.Style.Font.Size = 11;
                worksheet.Cells.Style.Font.Name = "Arial";
                worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

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

        private string ObtenerRutaDirectorio()
        {
            string rutaArchivos = "RutaArchivos";
            string entorno = string.Format("{0}:{1}", "RutaArchivosTemporales", rutaArchivos);
            string RutaTemporales = _configuracion.GetValue<string>(entorno);
            return RutaTemporales;
        }

    }
}
