using ConteoRecaudo.BLL.Interfaces;
using ConteoRecaudo.Infraestructure.Interfaces;
using ConteoRecaudo.Models;

namespace ConteoRecaudo.BLL
{
    public class RecaudoBL: IRecaudoBL
    {
        private IRecaudoRepository _recuadoRepository;
        public RecaudoBL(IRecaudoRepository recaudoRepository)
        {
            _recuadoRepository = recaudoRepository;
        }

        public async Task<List<RecaudoModel>> GetRecaudos() {
            List<RecaudoModel> recaudos = await _recuadoRepository.GetRecaudos();
            return recaudos;
        }
    }
}
