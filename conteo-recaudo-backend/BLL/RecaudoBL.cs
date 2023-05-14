using ConteoRecaudo.BLL.Interfaces;
using ConteoRecaudo.Helpers.Converts;
using ConteoRecaudo.Infraestructure.Interfaces;
using ConteoRecaudo.Models;
using ConteoRecaudo.Services.Interfaces;

namespace ConteoRecaudo.BLL
{
    public class RecaudoBL: IRecaudoBL
    {
        private IRecaudoRepository _recuadoRepository;
        private IConteoApi _conteoApi;
        private IRecaudoApi _recaudoApi;

        public RecaudoBL(IRecaudoRepository recaudoRepository, IConteoApi conteoApi, IRecaudoApi recaudoApi)
        {
            _recuadoRepository = recaudoRepository;
            _conteoApi = conteoApi;
            _recaudoApi = recaudoApi;
        }

        public async Task<List<ConteoRecaudoModel>> GetRecaudos() 
             => await _recuadoRepository.GetRecaudos();

        public async Task<bool> GuardarRecaudos(string token, DateTime fechaIncio, DateTime fechaFin)
        {

            List<ConteoRecaudoModel> recaudos = new List<ConteoRecaudoModel>();
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

    }
}
