using ConteoRecaudo.Models;

namespace ConteoRecaudo.BLL.Interfaces
{
    public interface IRecaudoBL
    {
        Task<List<ConteoRecaudoModel>> GetRecaudos();

        Task<bool> GuardarRecaudos(string token, DateTime fechaIncio, DateTime fechaFin);
    }
}
