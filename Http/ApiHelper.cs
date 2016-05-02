﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PrintNode.Net
{
    internal static class ApiHelper
    {
        private const string BaseUri = "https://api.printnode.com";

        private static readonly JsonSerializerSettings DefaultSerializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        internal static async Task<string> Get(string relativeUri, string clientId = null)
        {
            using (var http = BuildHttpClient(clientId))
            {
                var result = await http.GetAsync(BaseUri + relativeUri, CancellationToken.None);

                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception(result.StatusCode.ToString());
                }

                return await result.Content.ReadAsStringAsync();
            }
        }

        internal static async Task<string> Post<T>(string relativeUri, T parameters, string clientId = null)
        {
            using (var http = BuildHttpClient(clientId))
            {
                var json = JsonConvert.SerializeObject(parameters, DefaultSerializationSettings);

                var response = await http.PostAsync(BaseUri + relativeUri, new StringContent(json, Encoding.UTF8, "application/json"), CancellationToken.None);

                if (!response.IsSuccessStatusCode)
                {
                    throw new PrintNodeException(response);
                }

                return await response.Content.ReadAsStringAsync();
            }
        }

        internal static async Task<string> Patch<T>(string relativeUri, T parameters, Dictionary<string, string> headers, string clientId = null)
        {
            using (var http = BuildHttpClient(clientId, headers))
            {
                var json = JsonConvert.SerializeObject(parameters, DefaultSerializationSettings);
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), BaseUri + relativeUri) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

                var response = await http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new PrintNodeException(response);
                }

                return await response.Content.ReadAsStringAsync();
            }
        }

        internal static async Task<string> Delete(string relativeUri, Dictionary<string, string> headers, string clientId = null)
        {
            using (var http = BuildHttpClient(clientId, headers))
            {
                var request = new HttpRequestMessage(new HttpMethod("DELETE"), BaseUri + relativeUri);

                var response = await http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new PrintNodeException(response);
                }

                return await response.Content.ReadAsStringAsync();
            }
        }

        private static HttpClient BuildHttpClient(string clientId = null, Dictionary<string, string> headers = null)
        {
            headers = headers ?? new Dictionary<string, string>();
            clientId = clientId ?? PrintNodeConfiguration.GetApiKey();

            var http = new HttpClient();

            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(clientId)));
            http.DefaultRequestHeaders.Add("Accept-Version", "~3");

            foreach (var kv in headers)
            {
                http.DefaultRequestHeaders.Add(kv.Key, kv.Value);
            }

            return http;
        }
    }
}
