using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace QuickRestClient.ILGeneration
{
    internal static class UrlBuilderIL
    {
        public static void EmitUrlString(this ILGenerator il, string urlTemplate, ParameterInfo[] parameters)
        {
            var parameterRegex = new Regex("{(.+?)}");
            var parametersInUrl = parameterRegex.Matches(urlTemplate);
            if (parametersInUrl.Count == 0)
            {
                il.EmitString(urlTemplate);
                return;
            }
            if (parameters.Length < parametersInUrl.Count)
            {
                throw new InvalidOperationException(
                    $"Endpoint Url '{urlTemplate}' has more parameters than method's arguments count.");
            }
            var urlParamNames = parametersInUrl.Select(p => p.Groups[1].Value);
            var methodParamNames = parameters.Select(p => p.Name);

            ParameterInfo[] orderOfSubstitution;
            bool methodHasAllUrlParams = urlParamNames.All(p => methodParamNames.Contains(p));
            if (methodHasAllUrlParams)
            {
                orderOfSubstitution = parametersInUrl
                    .Select(urlParam => parameters.First(methodParam => methodParam.Name == urlParam.Groups[1].Value))
                    .ToArray();
            }
            else
            {
                orderOfSubstitution = parameters.Take(parametersInUrl.Count).ToArray();
            }

            var formatStringBuilder = new StringBuilder(urlTemplate);
            for (int i = 0; i < parametersInUrl.Count; i++)
            {
                var parameterInUrl = parametersInUrl[i];
                formatStringBuilder.Replace(parameterInUrl.Value, $"{{{i}}}");
            }
            var formatString = formatStringBuilder.ToString();

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
