using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace QuickRestClient.ILGeneration
{
    internal static class HttpRequestBuilderIL
    {
        public static void EmitHttpRequest(this ILGenerator il, EndpointAttribute endpoint, ParameterInfo[] parameters)
        {
            string urlTemplate = endpoint.RelativePath;
            MatchCollection parametersInUrl = GetUrlParameters(urlTemplate);

            if (parameters.Length < parametersInUrl.Count)
            {
                throw new InvalidOperationException(
                    $"Endpoint Url '{urlTemplate}' has more parameters than method's arguments count.");
            }

            ParameterInfo[] orderOfSubstitution = GetMethodParamsForUrlInSubstitutionOrder(parameters, parametersInUrl);
            string formatString = GetUrlFormatString(urlTemplate, parametersInUrl.Select(p => p.Value));

            ParameterInfo[] unusedParams = parameters.Except(orderOfSubstitution).ToArray();
            ParameterInfo contentParameter = unusedParams.FirstOrDefault();
            if (contentParameter != null && !endpoint.HttpMethod.HasContent())
            {
                throw new InvalidOperationException(
                    $"Endpoint '{urlTemplate}' has Http method {endpoint.HttpMethod} which doesn't allow " +
                    $"having request body. Remove unnecessary parameter(s) to avoid this exception.");
            }
            if (unusedParams.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Method '{unusedParams[0].Member}' has {unusedParams.Length - 1} unused parameter(s): " +
                    $"{string.Join(", ", unusedParams.Skip(1).Select(p => p.Name))}. " +
                    $"Remove unnecessary parameter(s) to avoid this exception.");
            }
            if (contentParameter != null && contentParameter.ParameterType.IsValueType)
            {
                throw new InvalidOperationException(
                    $"Parameter '{contentParameter.Name}' in '{urlTemplate}' must be a reference type " +
                    $"(currently it's a value type). If you really want to keep it, just box it.");
            }

            il.Emit(OpCodes.Newobj, typeof(HttpRequestMessage).GetConstructor(new Type[0]));

            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Call, typeof(HttpMethod).GetProperty(Enum.GetName(typeof(EndpointHttpMethod), endpoint.HttpMethod)).GetMethod);
            il.Emit(OpCodes.Callvirt, typeof(HttpRequestMessage).GetProperty(nameof(HttpRequestMessage.Method)).SetMethod);

            if (contentParameter != null)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_S, contentParameter.Position + 1);
                il.Emit(OpCodes.Call, typeof(RestClientBase).GetMethod(nameof(RestClientBase.CreateRequestBody), BindingFlags.Instance | BindingFlags.NonPublic));
                il.Emit(OpCodes.Callvirt, typeof(HttpRequestMessage).GetProperty(nameof(HttpRequestMessage.Content)).SetMethod);
            }

            il.Emit(OpCodes.Dup);
            EmitUrlComposition(il, orderOfSubstitution, formatString);
            il.Emit(OpCodes.Ldc_I4_S, (int)UriKind.RelativeOrAbsolute);
            il.Emit(OpCodes.Newobj, typeof(Uri).GetConstructor(new[] { typeof(string), typeof(UriKind) }));
            il.Emit(OpCodes.Callvirt, typeof(HttpRequestMessage).GetProperty(nameof(HttpRequestMessage.RequestUri)).SetMethod);
        }

        private static MatchCollection GetUrlParameters(string urlTemplate)
        {
            var parameterRegex = new Regex("{(.+?)}");
            var parametersInUrl = parameterRegex.Matches(urlTemplate);
            return parametersInUrl;
        }

        private static ParameterInfo[] GetMethodParamsForUrlInSubstitutionOrder(ParameterInfo[] allMethodParams, MatchCollection paramsOfUrl)
        {
            var urlParamNames = paramsOfUrl.Select(p => p.Groups[1].Value);
            var methodParamNames = allMethodParams.Select(p => p.Name);

            bool methodHasAllUrlParams = urlParamNames.All(p => methodParamNames.Contains(p));
            if (methodHasAllUrlParams)
            {
                return paramsOfUrl
                    .Select(urlParam => allMethodParams.First(methodParam => methodParam.Name == urlParam.Groups[1].Value))
                    .ToArray();
            }
            return allMethodParams.Take(paramsOfUrl.Count).ToArray();
        }

        private static string GetUrlFormatString(string urlTemplate, IEnumerable<string> urlParameters)
        {
            var formatStringBuilder = new StringBuilder(urlTemplate);
            int i = 0;
            foreach (var parameterInUrl in urlParameters)
            {
                formatStringBuilder.Replace(parameterInUrl, $"{{{i}}}");
                i++;
            }
            return formatStringBuilder.ToString();
        }

        private static void EmitUrlComposition(ILGenerator il, ParameterInfo[] orderOfSubstitution, string formatString)
        {
            if (orderOfSubstitution.Length == 0)
            {
                il.EmitString(formatString);
                return;
            }

            il.EmitString(formatString);

            il.EmitEmptyArray<string>(orderOfSubstitution.Length);

            for (int i = 0; i < orderOfSubstitution.Length; i++)
            {
                ParameterInfo param = orderOfSubstitution[i];
                var emitArgRef = param.ParameterType.IsPrimitive
                    ? (Action<int>)il.EmitLoadArgAddress
                    : (Action<int>)il.EmitLoadArg;

                il.SetArrayReferenceElement(i, ilGen =>
                {
                    // +1 to argument position, because
                    // 0-argument is "this"
                    emitArgRef.Invoke(param.Position + 1);
                    ilGen.EmitToString(param.ParameterType);
                });
            }
            il.Emit(OpCodes.Call, typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) }));
        }
    }
}
