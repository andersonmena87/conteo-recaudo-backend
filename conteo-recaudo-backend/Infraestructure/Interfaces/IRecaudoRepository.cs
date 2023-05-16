using ConteoRecaudo.Entities;
using ConteoRecaudo.Models;

namespace ConteoRecaudo.Infraestructure.Interfaces
{
    public interface IRecaudoRepository
    {
        Task<int> GuardarRecaudo(RecaudoEntity recaudo);
        Task<List<ConteoRecaudoModel>> GetRecaudos();
        Task<List<ReporteRecaudoExcel>> ObtenerRecaudosxFechas(DateTime fechaInicial, DateTime fechaFinal);
    }
}
