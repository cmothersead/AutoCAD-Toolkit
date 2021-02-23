using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
