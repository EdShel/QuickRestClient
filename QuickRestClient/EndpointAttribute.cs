using System;
using System.Net.Http;

namespace QuickRestClient
{
    public enum EndpointHttpMethod
    {
        Get,
        Post,
        Delete,
        Patch,
        Put,
        Head,
        Trace,
        Options
    }

    public static class HttpMethodHelper
    {
        public static bool HasContent(this EndpointHttpMethod method)
        {
            return method == EndpointHttpMethod.Post
                || method == EndpointHttpMethod.Put
                || method == EndpointHttpMethod.Delete
                || method == EndpointHttpMethod.Patch;
        }

        public static HttpMethod ToSystemNetHttpHttpMethod(this EndpointHttpMethod method)
        {
            return method switch
            {
                EndpointHttpMethod.Get => HttpMethod.Get,
                EndpointHttpMethod.Post => HttpMethod.Post,
                EndpointHttpMethod.Delete => HttpMethod.Delete,
                EndpointHttpMethod.Patch => HttpMethod.Patch,
                EndpointHttpMethod.Put => HttpMethod.Put,
                EndpointHttpMethod.Head => HttpMethod.Head,
                EndpointHttpMethod.Trace => HttpMethod.Trace,
                EndpointHttpMethod.Options => HttpMethod.Options,
                _ => throw new ArgumentException("Unknown Http method", nameof(method)),
            };
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class EndpointAttribute : Attribute
    {
        public EndpointAttribute(string relativePath)
        {
            this.RelativePath = relativePath;
        }

        public string RelativePath { get; }

        public EndpointHttpMethod HttpMethod { get; set; } = EndpointHttpMethod.Get;
    }
}
