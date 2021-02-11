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

        protected internal string GetResponseString(string requestString)
        {
            bool correctUri = Uri.TryCreate(
                requestString, UriKind.RelativeOrAbsolute, out Uri requestUri);
            if (!correctUri)
            {
                throw new ArgumentException(nameof(requestString));
            }

            var response = Client.GetAsync(requestUri).Result;
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

        private ModuleBuilder moduleBuilder;

        private readonly object monitor = new object();

        public RestClientsFactory(HttpClient client)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
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

        private static void ImplementContractByClientClass(Type contractType, TypeBuilder clientClass)
        {
            clientClass.AddInterfaceImplementation(contractType);

            var allMethods = contractType.GetMethods(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.FlattenHierarchy);

            foreach (var method in allMethods)
            {
                ImplementMethodByClientClass(method, clientClass);
            }
        }

        private static void ImplementMethodByClientClass(MethodInfo method, TypeBuilder type)
        {
            var endpointAttribute = method.GetCustomAttribute<EndpointAttribute>(true);
            if (endpointAttribute == null)
            {
                throw new InvalidOperationException(
                    $"Method '{method}' has no {nameof(EndpointAttribute)}.");
            }

            MethodBuilder methodImpl = AddInterfaceMethodToClientClass(method, type);



            var realImplementation = typeof(RestClientBase)
                .GetMethod(nameof(RestClientBase.GetResponseString),
                    BindingFlags.Instance | BindingFlags.NonPublic);

            var endpointPath = endpointAttribute.RelativePath;
            var il = methodImpl.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, endpointPath);
            il.Emit(OpCodes.Call, realImplementation);
            il.Emit(OpCodes.Ret);
        }

        private static MethodBuilder AddInterfaceMethodToClientClass(MethodInfo method, TypeBuilder type)
        {
            var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var methodImpl = type.DefineMethod(method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameters);
            type.DefineMethodOverride(methodImpl, method);
            return methodImpl;
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

            return client as T;
        }
    }
}
