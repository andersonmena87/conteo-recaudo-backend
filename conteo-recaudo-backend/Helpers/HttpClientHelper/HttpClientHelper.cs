using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ConteoRecaudo.Helpers.HttpClientHelper
{
    public class HttpClientHelper
    {
        public static async Task<T> GetAsync<T>(string token, string url)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token vacío o nulo", nameof(token));
            }

            T data;
            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using HttpResponseMessage response = await client.GetAsync(url);
                using HttpContent content = response.Content;
                string d = await content.ReadAsStringAsync();
                try
                {
                    if (d != null)
                    {
                        data = JsonConvert.DeserializeObject<T>(d);
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            object o = new();
            return (T)o;
        }

    }
}
