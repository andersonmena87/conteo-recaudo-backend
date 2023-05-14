using ConteoRecaudo.Models;

namespace ConteoRecaudo.Services.Interfaces
{
    public interface IConteoApi
    {
        Task<List<RecaudoModel>> GetConteos(string token, string fecha);
    }
}
