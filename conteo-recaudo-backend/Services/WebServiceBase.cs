namespace ConteoRecaudo.Services
{
    public class WebServiceBase
    {
        public IConfiguration Configuration { get; }

        public WebServiceBase(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string ObtenerUrlApi(string ApiTag)
        {
            string urlBase = Configuration.GetValue<string>(ApiTag + ":Url");
            string endPoint = Configuration.GetValue<string>(ApiTag + ":EndPoint");
            string urlApi = urlBase + endPoint;
            return urlApi;
        }
    }
}
