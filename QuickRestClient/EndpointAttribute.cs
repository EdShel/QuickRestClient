using System;

namespace QuickRestClient
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class EndpointAttribute : Attribute
    {
        public EndpointAttribute(string relativePath)
        {
            this.RelativePath = relativePath;
        }

        public string RelativePath { get; }
    }

}
