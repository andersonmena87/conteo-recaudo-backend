using ConteoRecaudo.Models;

namespace ConteoRecaudo.Services.Interfaces
{
    public interface IRecaudoApi
    {
        Task<List<RecaudoModel>> GetRecaudos(string token, string fecha);
    }
}
