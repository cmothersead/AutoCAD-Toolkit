using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data
{
    public class PartProcessor
    {
        public static async Task<ObservableCollection<Part>> GetPartNumbersAsync(int familyId, int manufacturerId)
        {
            using (HttpResponseMessage response = await APIHelper.APIClient.GetAsync("https://localhost:44364/api/families/" + familyId + "/manufacturers/" + manufacturerId + "/parts"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<ObservableCollection<Part>>();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<Part> CreatePartAsync(int familyId, int manufacturerId, string partNumber)
        {
            var test = new StringContent(JsonConvert.SerializeObject(new { familyId, manufacturerId, partNumber }), Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await APIHelper.APIClient.PostAsync("https://localhost:44364/api/parts/", test))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<Part>();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}

