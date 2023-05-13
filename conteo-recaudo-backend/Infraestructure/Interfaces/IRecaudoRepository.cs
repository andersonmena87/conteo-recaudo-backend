using ConteoRecaudo.Models;

namespace ConteoRecaudo.Infraestructure.Interfaces
{
    public interface IRecaudoRepository
    {
        Task<int> GuardarRecaudo(RecaudoModel recaudo);
        Task<List<RecaudoModel>> GetRecaudos();
    }
}
