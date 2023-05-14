using ConteoRecaudo.Helpers.HttpClientHelper;
using ConteoRecaudo.Models;
using ConteoRecaudo.Services.Interfaces;

namespace ConteoRecaudo.Services
{
    public class ConteoApi : WebServiceBase, IConteoApi
    {
        public ConteoApi(IConfiguration configuration) : base(configuration)
        {
     
        }

        public async Task<List<RecaudoModel>> GetConteos(string token, string fecha)
        {
            string url = ObtenerUrlApi("ConteoApi") + fecha;
            return await HttpClientHelper
                .GetAsync<List<RecaudoModel>>(token, url);
        }
    }
}
