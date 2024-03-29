﻿using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace ICA.Schematic.Data
{
    public class FamilyProcessor
    {
        public static async Task<int> GetFamilyIdAsync(string familyCode)
        {
            using (HttpResponseMessage response = await APIHelper.APIClient.GetAsync("https://localhost:44364/api/families/" + familyCode))
            {
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsAsync<int>().Result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

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
