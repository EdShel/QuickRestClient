using System;

namespace QuickRestClient
{
    public enum EndpointHttpMethod
    {
        Get,
        Post,
        Delete,
        Patch,
        Put,
        Head
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
