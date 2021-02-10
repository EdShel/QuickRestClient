﻿using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;

namespace QuickRestClient
{
    public abstract class RestClientBase
    {
        internal HttpClient Client;

        internal Uri Host;

        protected internal string GetResponseString(string relativeUri)
        {
            bool correctUri = Uri.TryCreate(Host, relativeUri, out Uri absoluteUri);
            if (!correctUri)
            {
                throw new ArgumentException(nameof(relativeUri));
            }

            var response = Client.GetAsync(absoluteUri).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return null;
        }
    }

    public class RestClientsFactory
    {
        private readonly HttpClient client;

        private readonly Uri host;

        public RestClientsFactory(HttpClient client, Uri host)
        {
            this.client = client;
            this.host = host;
        }

        public T CreateClient<T>()
            where T : class
        {
            var contractType = typeof(T);
            if (!contractType.IsInterface)
            {
                throw new ArgumentException(
                    "Contract type must be an interface.", nameof(T));
            }

            var allMethods = contractType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            var assemblyName = new AssemblyName(contractType.FullName);
            var moduleName = $"{assemblyName}.dll";
            var namesapce = contractType.Namespace;
            var typeName = $"{namesapce}.{contractType.Name}Impl";

            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var module = assembly.DefineDynamicModule(moduleName);
            var type = module.DefineType(typeName,
                TypeAttributes.Class
                | TypeAttributes.AnsiClass
                | TypeAttributes.Sealed
                | TypeAttributes.NotPublic);
            type.SetParent(typeof(RestClientBase));
            type.AddInterfaceImplementation(contractType);

            type.DefineDefaultConstructor(MethodAttributes.Public);

            foreach (var method in allMethods)
            {
                var endpointAttribute = method.GetCustomAttribute<EndpointAttribute>(true);
                if (endpointAttribute != null)
                {
                    var methodName = method.Name;
                    var returnType = method.ReturnType;
                    var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
                    var endpointPath = endpointAttribute.RelativePath;
                    var methodImpl = type.DefineMethod(
                        methodName, 
                        MethodAttributes.Public | MethodAttributes.Virtual, returnType, parameters);
                    var realImplementation = typeof(RestClientBase)
                        .GetMethod(nameof(RestClientBase.GetResponseString), BindingFlags.Instance | BindingFlags.NonPublic);

                    var il = methodImpl.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, endpointPath);
                    il.Emit(OpCodes.Call, realImplementation);
                    il.Emit(OpCodes.Ret);

                    type.DefineMethodOverride(methodImpl, method);
                }
            }

            var createdType = type.CreateType();
            var client = (RestClientBase)Activator.CreateInstance(createdType, new object[0]);
            client.Client = this.client;
            client.Host = this.host;

            return client as T;
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
    }

}