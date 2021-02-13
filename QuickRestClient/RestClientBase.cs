using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;

namespace QuickRestClient
{
    public abstract class RestClientBase
    {
        internal HttpClient Client;

        protected internal HttpContent CreateRequestBody(object content)
        {
            var json = JsonConvert.SerializeObject(content);
            return new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        protected internal HttpResponseMessage GetResponse(HttpRequestMessage request)
        {
            return Client.SendAsync(request).Result;
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
