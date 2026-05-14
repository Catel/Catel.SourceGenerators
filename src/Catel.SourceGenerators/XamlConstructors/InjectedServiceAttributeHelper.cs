namespace Catel.SourceGenerators.XamlConstructors;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

internal static class InjectedServiceAttributeHelper
{
    internal const string AttributeFullName = "Catel.InjectedServiceAttribute";

    internal static List<InjectedServiceInfo> GetInjectedServiceFields(INamedTypeSymbol classSymbol)
    {
        var fields = new List<InjectedServiceInfo>();

        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            var hasAttr = fieldSymbol.GetAttributes()
                .Any(a => a.AttributeClass?.IsType(AttributeFullName) ?? false);
            if (!hasAttr)
            {
                continue;
            }

            var fieldName = fieldSymbol.Name;
            var parameterName = GetParameterName(fieldName);
            var typeName = fieldSymbol.Type.ToDisplayString();

            fields.Add(new InjectedServiceInfo(fieldName, parameterName, typeName));
        }

        return fields;
    }

    private static string GetParameterName(string fieldName)
    {
        var name = fieldName.TrimStart('_');
        if (name.Length == 0)
        {
            return fieldName;
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
