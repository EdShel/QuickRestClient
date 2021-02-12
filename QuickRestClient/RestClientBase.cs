using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace QuickRestClient
{
    public abstract class RestClientBase
    {
        internal HttpClient Client;

        protected internal HttpResponseMessage GetResponse(string requestString)
        {
            bool correctUri = Uri.TryCreate(
                requestString, UriKind.RelativeOrAbsolute, out Uri requestUri);
            if (!correctUri)
            {
                throw new ArgumentException(nameof(requestString));
            }

            return Client.GetAsync(requestUri).Result;
        }

        protected internal string ReturnRawStringResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return response.Content.ReadAsStringAsync().Result;
        }

        protected internal T ReturnParsedObject<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            var jsonString = response.Content.ReadAsStringAsync().Result;
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (JsonSerializationException ex)
            {
                throw new InvalidOperationException(
                    $"Can't parse response string to the type {typeof(T).FullName}. " +
                    $"See inner exception for more details.", ex);
            }
        }
    }
}
