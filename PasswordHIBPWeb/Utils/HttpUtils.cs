using Newtonsoft.Json;
using PasswordHIBPWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PasswordHIBPWeb.Utils
{
    public class HttpUtils
    {
        private static HttpClient client = new HttpClient();
        private readonly string baseUrl = string.Empty;

        public HttpUtils()
        {
            baseUrl = "https://api.pwnedpasswords.com";

            client.BaseAddress = new Uri(baseUrl);
        }

        public async Task<List<PasswordHashEntry>> GetPasswordsByRange(string hashedPassword)
        {
            List<PasswordHashEntry> entries = null;
            string hashPrefix = hashedPassword.Substring(0, 5);

            try
            {
                string uriString = $"{baseUrl}/range/{hashPrefix}";
                Uri uri = new Uri(uriString);
                string content = await GetJsonFromUri(uri);
                entries = ParsePasswordsRangeFromApi(content);
            }
            catch (NullReferenceException)
            {
                System.Diagnostics.Debug.WriteLine("HttpUtils:GetPasswordsByRange: Cannot get ranges");
            }

            return entries;
        }

        private List<PasswordHashEntry> ParsePasswordsRangeFromApi(string passwordsRange)
        {
            List<PasswordHashEntry> entries = null;

            if (!string.IsNullOrEmpty(passwordsRange))
            {
                entries = new List<PasswordHashEntry>();
            }

            foreach (var hashAndOccurences in passwordsRange.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                string[] items = hashAndOccurences.Split(':');

                PasswordHashEntry passwordHashEntry = new PasswordHashEntry()
                {
                    Hash = items[0],
                    Occurences = int.Parse(items[1])
                };

                entries.Add(passwordHashEntry);
            }

            return entries;
        }

        private async Task<string> GetJsonFromUri(Uri uri)
        {
            try
            {
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException)
            {
                // TODO: Temp workaround to API unavailable. Log/pass through exception somewhere. 
                return string.Empty;
            }

            return string.Empty;
        }

        private async Task<string> PostJsonToUri(Uri uri, object jsonObject)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string postBody = JsonConvert.SerializeObject(jsonObject);

            try
            {
                var response = await client.PostAsync(uri, new StringContent(postBody, System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException)
            {
                //TODO: Temp workaround to API unavailable. Log/pass through exception somewhere. 
                return string.Empty;
            }

            return string.Empty;
        }

        private async Task<HttpStatusCode> PutJsonToUri(Uri uri, object jsonObject)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string postBody = JsonConvert.SerializeObject(jsonObject);
            HttpStatusCode status;

            try
            {
                var response = await client.PutAsync(uri, new StringContent(postBody, System.Text.Encoding.UTF8, "application/json"));
                status = response.StatusCode;
            }
            catch (HttpRequestException)
            {
                //TODO: Temp workaround to API unavailable. Log/pass through exception somewhere. 
                return HttpStatusCode.InternalServerError;
            }

            return status;
        }

        private async Task<bool> DeleteToUri(Uri uri)
        {
            HttpResponseMessage response;

            try
            {
                response = await client.DeleteAsync(uri);
            }
            catch (HttpRequestException)
            {
                //TODO: Temp workaround to API unavailable. Log/pass through exception somewhere. 
                return false;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }
    }
}
