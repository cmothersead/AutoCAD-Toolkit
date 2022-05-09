using System.Net.Http;
using System.Net.Http.Headers;

namespace ICA.Schematic.Data
{
    public class APIHelper
    {
        public static HttpClient APIClient { get; set; } = new HttpClient();

        public static void InitializeClient()
        {
            APIClient = new HttpClient();
            APIClient.DefaultRequestHeaders.Accept.Clear();
            APIClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //APIClient.BaseAddress = new Uri("https://localhost:44364/api");
        }
    }
}
