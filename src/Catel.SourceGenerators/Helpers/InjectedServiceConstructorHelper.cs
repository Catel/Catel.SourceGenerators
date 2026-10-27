namespace Catel.SourceGenerators.XamlConstructors;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

internal static class InjectedServiceConstructorHelper
{
    internal static ParameterInfo[] GetBaseParameters(INamedTypeSymbol classSymbol, Compilation compilation)
    {
        var constructor = classSymbol.BaseType?.InstanceConstructors
            .Where(c => compilation.IsSymbolAccessibleWithin(c, classSymbol))
            .OrderBy(c => c.Parameters.Length)
            .FirstOrDefault();

        return constructor?.Parameters
            .Select(p => new ParameterInfo(p.Name, p.Type.ToDisplayString(), p.IsNullable(),
                p.HasExplicitDefaultValue
                    ? p.ExplicitDefaultValue is null ? $"default({p.Type.ToDisplayString()})"
                        : $"({p.Type.ToDisplayString()}){Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatPrimitive(p.ExplicitDefaultValue, true, false)}"
                    : null)).ToArray()
            ?? Array.Empty<ParameterInfo>();
    }

    internal static void AppendConstructors(IndentedStringBuilder sourceBuilder, string generatorName,
        string className, EquatableArray<ParameterInfo> baseParameters, EquatableArray<InjectedServiceInfo> injectedServices)
    {
        var parameterNames = new HashSet<string>(baseParameters.Select(p => p.Name));
        var additionalServices = injectedServices
            .Where(s => !baseParameters.Any(p => p.ParameterTypeName == s.TypeName))
            .Select(s =>
            {
                var name = s.ParameterName;
                var suffix = 1;
                while (!parameterNames.Add(name))
                {
                    name = s.ParameterName + suffix++;
                }
                return new InjectedServiceInfo(s.FieldName, name, s.TypeName);
            }).ToArray();
        var parameters = baseParameters.Select(p => $"{p.ParameterTypeName} {p.Name}")
            .Concat(additionalServices.Select(s => $"{s.TypeName} {s.ParameterName}"));

        sourceBuilder.AppendLine("partial void OnConstructing();");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("partial void OnConstructed();");
        sourceBuilder.AppendLine();

        sourceBuilder.AppendGeneratedCodeAttribute(generatorName);
        sourceBuilder.AppendLine($"public {className}({string.Join(", ", parameters)})");
        if (baseParameters.Count > 0)
        {
            sourceBuilder.AppendLine($"    : base({string.Join(", ", baseParameters.Select(p => p.Name))})");
        }
        sourceBuilder.StartBlock();
        foreach (var service in injectedServices)
        {
            var parameterName = baseParameters.FirstOrDefault(p => p.ParameterTypeName == service.TypeName).Name
                ?? additionalServices.First(s => s.FieldName == service.FieldName).ParameterName;
            var fieldName = parameterNames.Contains(service.FieldName) ? $"this.{service.FieldName}" : service.FieldName;
            sourceBuilder.AppendLine($"{fieldName} = {parameterName};");
        }
        sourceBuilder.AppendLine("OnConstructing();");
        sourceBuilder.AppendLine("OnConstructed();");
        sourceBuilder.EndBlock();

        sourceBuilder.AppendLine();

        var serviceCalls = baseParameters.Select(p =>
                p.DefaultValueExpression is not null && !injectedServices.Any(s => s.TypeName == p.ParameterTypeName)
                    ? p.DefaultValueExpression : $"GetService<{p.ParameterTypeName}>()")
            .Concat(additionalServices.Select(s => $"GetService<{s.TypeName}>()"));
        sourceBuilder.AppendGeneratedCodeAttribute(generatorName);
        sourceBuilder.AppendLine($"public {className}()");
        sourceBuilder.AppendLine($"    : this({string.Join(", ", serviceCalls)})");
        sourceBuilder.StartBlock();
        sourceBuilder.EndBlock();
    }
}
