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
        public RecaudoBL(IRecaudoRepository recaudoRepository, IConteoApi conteoApi)
        {
            _recuadoRepository = recaudoRepository;
            _conteoApi = conteoApi;

        }

        public async Task<List<RecaudoModel>> GetRecaudos() 
             => await _recuadoRepository.GetRecaudos();

        public async Task<bool> GuardarRecaudos(string token, DateTime fechaIncio, DateTime fechaFin)
        {

            List<RecaudoModel> recaudos = new List<RecaudoModel>();
            DateTime FechaInicio = fechaIncio;

            while (FechaInicio <= fechaFin)
            {
                string fecha = FechaInicio.ToString("yyyy-MM-dd");
                List<RecaudoModel> recaudosDelDia = await _conteoApi.GetConteos(token, fecha);
                recaudos.AddRange(recaudosDelDia);
                FechaInicio = FechaInicio.AddDays(1);
            }

            return true;

        }
        

    }
}
