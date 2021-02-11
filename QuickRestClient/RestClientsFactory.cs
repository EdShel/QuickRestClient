using System;
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

        private ModuleBuilder moduleBuilder;

        private readonly object monitor = new object();

        public RestClientsFactory(HttpClient client, Uri host)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public TContract CreateClient<TContract>()
            where TContract : class
        {
            var contractType = typeof(TContract);
            if (!contractType.IsInterface)
            {
                throw new ArgumentException(
                    "Contract type must be an interface.", nameof(TContract));
            }

            Type clientClass = CreateClientClass(contractType);
            return CreateClientInstance<TContract>(clientClass);
        }


        private Type CreateClientClass(Type contractType)
        {
            TypeBuilder clientBuilder = CreateClientClassWithInheritance(contractType);
            ImplementContractByClientClass(contractType, clientBuilder);
            var createdType = clientBuilder.CreateType();
            return createdType;
        }

        private TypeBuilder CreateClientClassWithInheritance(Type contractType)
        {
            ModuleBuilder module = GetModuleForCustomTypes();

            var typeName = $"{contractType.FullName}Impl";
            var type = module.DefineType(typeName,
                TypeAttributes.Class
                | TypeAttributes.AnsiClass
                | TypeAttributes.Sealed
                | TypeAttributes.NotPublic);
            type.SetParent(typeof(RestClientBase));
            type.DefineDefaultConstructor(MethodAttributes.Public);
            return type;
        }

        private static void ImplementContractByClientClass(Type contractType, TypeBuilder type)
        {
            type.AddInterfaceImplementation(contractType);

            var allMethods = contractType.GetMethods(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.FlattenHierarchy);

            foreach (var method in allMethods)
            {
                ImplementMethodByClientClass(method, type);
            }
        }

        private static void ImplementMethodByClientClass(MethodInfo method, TypeBuilder type)
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

        private ModuleBuilder GetModuleForCustomTypes()
        {
            lock (this.monitor)
            {
                if (this.moduleBuilder != null)
                {
                    return this.moduleBuilder;
                }
                var assemblyName = new AssemblyName($"{nameof(QuickRestClient)}Implementations");
                var assembly = AssemblyBuilder.DefineDynamicAssembly(
                    assemblyName, AssemblyBuilderAccess.RunAndCollect);

                var moduleName = $"{assemblyName}.dll";
                this.moduleBuilder = assembly.DefineDynamicModule(moduleName);
                return moduleBuilder;
            }
        }

        private T CreateClientInstance<T>(Type createdType) where T : class
        {
            var client = (RestClientBase)Activator.CreateInstance(createdType, new object[0]);
            client.Client = this.client;
            client.Host = this.host;

            return client as T;
        }
    }
}
