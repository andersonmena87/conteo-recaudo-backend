using ConteoRecaudo.Helpers.HttpClientHelper;
using ConteoRecaudo.Models;
using ConteoRecaudo.Services.Interfaces;

namespace ConteoRecaudo.Services
{
    public class RecaudoApi : WebServiceBase, IRecaudoApi
    {
        public RecaudoApi(IConfiguration configuration) : base(configuration)
        {
     
        }

        public async Task<List<RecaudoModel>> GetRecaudos(string token, string fecha)
        {
            string url = ObtenerUrlApi("RecaudoApi") + fecha;
            return await HttpClientHelper
                .GetAsync<List<RecaudoModel>>(token, url);
        }
    }
}
