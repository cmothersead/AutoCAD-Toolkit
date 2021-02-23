using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data
{
    class FamilyProcessor
    {
        public static async Task<Family> GetFamilyAsync(int familyId)
        {
            using (HttpResponseMessage response = await APIHelper.APIClient.GetAsync("https://localhost:44364/api/families/" + familyId))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<Family>();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<Family> GetFamilyAsync(string familyCode)
        {
            using (HttpResponseMessage response = await APIHelper.APIClient.GetAsync("https://localhost:44364/api/families/" + familyCode))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<Family>();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<ObservableCollection<Family>> GetFamiliesAsync()
        {
            using (HttpResponseMessage response = await APIHelper.APIClient.GetAsync("https://localhost:44364/api/families/"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<ObservableCollection<Family>>();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
