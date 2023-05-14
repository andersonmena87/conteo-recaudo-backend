using ConteoRecaudo.Models;

namespace ConteoRecaudo.Services.Interfaces
{
    public interface IConteoApi
    {
        Task<List<ConteoModel>> GetConteos(string token, string fecha);
    }
}
