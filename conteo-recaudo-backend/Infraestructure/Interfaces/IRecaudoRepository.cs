using ConteoRecaudo.Entities;
using ConteoRecaudo.Models;

namespace ConteoRecaudo.Infraestructure.Interfaces
{
    public interface IRecaudoRepository
    {
        Task<int> GuardarRecaudo(RecaudoEntity recaudo);
        Task<List<RecaudoModel>> GetRecaudos();
    }
}
