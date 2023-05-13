using ConteoRecaudo.Models;

namespace ConteoRecaudo.BLL.Interfaces
{
    public interface IRecaudoBL
    {
        Task<List<RecaudoModel>> GetRecaudos();
    }
}
