using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data
{
    class ManufacturerProcessor
    {
        public static async Task<ObservableCollection<Manufacturer>> GetManufacturersAsync(int familyId)
        {
            using (HttpResponseMessage response = await APIHelper.APIClient.GetAsync("https://localhost:44364/api/families/" + familyId + "/manufacturers"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsAsync<ObservableCollection<Manufacturer>>();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public static async Task<ObservableCollection<Manufacturer>> GetManufacturersUppercaseAsync(int familyId)
        {
            using (HttpResponseMessage response = await APIHelper.APIClient.GetAsync("https://localhost:44364/api/families/" + familyId + "/manufacturers"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var manufacturers = await response.Content.ReadAsAsync<ObservableCollection<Manufacturer>>();
                    foreach(var manufacturer in manufacturers)
                    {
                        manufacturer.ManufacturerName = manufacturer.ManufacturerName.ToUpper();
                    }
                    return manufacturers;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
