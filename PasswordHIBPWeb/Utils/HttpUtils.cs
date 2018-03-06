using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// HaveIBeenPwned API URL
        /// </summary>
        private readonly string baseUrl = string.Empty;

        public HttpUtils()
        {
            baseUrl = "https://api.pwnedpasswords.com";
        }

        /// <summary>
        /// Gets a list of PasswordHashEntry objects by given SHA1 hashed password from HIBP API v2.
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <returns>A list of PasswordHashEntry objects, based on parsed HIBP API return data.</returns>
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
            catch (NullReferenceException e)
            {
                System.Diagnostics.Debug.WriteLine($"HttpUtils: GetPasswordsByRange: Cannot get ranges. Message: {e}");
            }

            return entries;
        }

        /// <summary>
        /// Gets the amount of occurences based on hashed password and store it in a PasswordHashEntry.
        /// </summary>
        /// <param name="hashedPassword">SHA-1 hashed password.</param>
        /// <returns>Filled PasswordHashEntry with total amount of occurences.</returns>
        public async Task<PasswordHashEntry> GetPasswordOccurences(string hashedPassword)
        {
            PasswordHashEntry passwordHashEntry = null;

            try
            {
                string uriString = $"{baseUrl}/pwnedpassword/{hashedPassword}";
                Uri uri = new Uri(uriString);
                string content = await GetJsonFromUri(uri);

                if (int.TryParse(content, out int occurences))
                {
                    passwordHashEntry = new PasswordHashEntry()
                    {
                        Hash = hashedPassword,
                        Occurences = occurences
                    };
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"HttpUtils: GetPasswordOccurences: Cannot get password occurences. Message: {e}");
            }

            return passwordHashEntry;
        }

        /// <summary>
        /// Parses the return data from the HIBP API into a list of PasswordHashEntry object for simplification purposes.
        /// </summary>
        /// <param name="passwordsRange">One long string containing multiple newline-seperated entries. The entries are semicolom seperated hash:occurences object, e.g. 00D4F6E8FA6EECAD2A3AA415EEC418D38EC:2</param>
        /// <returns>A list of parsed PasswordHashEntry objects, based on HIBP API return data.</returns>
        private List<PasswordHashEntry> ParsePasswordsRangeFromApi(string passwordsRange)
        {
            List<PasswordHashEntry> entries = null;

            if (!string.IsNullOrEmpty(passwordsRange))
            {
                entries = new List<PasswordHashEntry>();
            }

            // Split entries on NewLine character (\r\n).
            // PasswordsRange contains for example: 00D4F6E8FA6EECAD2A3AA415EEC418D38EC:2\r\n011053FD0102E94D6AE2F8B83D76FAF94F6:1
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
                System.Diagnostics.Debug.WriteLine("HttpUtils: GetJsonFromUri: API unavailable.");

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
                System.Diagnostics.Debug.WriteLine("HttpUtils: PostJsonToUri: API unavailable.");

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
                System.Diagnostics.Debug.WriteLine("HttpUtils: PutJsonToUri: API unavailable.");

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
                System.Diagnostics.Debug.WriteLine("HttpUtils: DeleteToUri: API unavailable.");

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
