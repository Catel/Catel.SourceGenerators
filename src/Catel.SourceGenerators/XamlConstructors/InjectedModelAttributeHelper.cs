namespace Catel.SourceGenerators.XamlConstructors;

using Microsoft.CodeAnalysis;

internal static class InjectedModelAttributeHelper
{
    internal const string AttributeFullName = "Catel.InjectedModelAttribute";

    internal static InjectedModelInfo? GetInjectedModelMember(INamedTypeSymbol classSymbol)
    {
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IFieldSymbol fieldSymbol)
            {
                var hasAttr = false;
                foreach (var attr in fieldSymbol.GetAttributes())
                {
                    if (attr.AttributeClass?.ToDisplayString() == AttributeFullName)
                    {
                        hasAttr = true;
                        break;
                    }
                }

                if (!hasAttr)
                {
                    continue;
                }

                var fieldName = fieldSymbol.Name;
                var parameterName = GetParameterName(fieldName);
                var typeName = fieldSymbol.Type.ToDisplayString();
                var isNullable = fieldSymbol.NullableAnnotation == NullableAnnotation.Annotated;
                var baseTypeName = typeName.TrimEnd('?');

                return new InjectedModelInfo(fieldName, parameterName, baseTypeName, isNullable);
            }

            if (member is IPropertySymbol propertySymbol)
            {
                var hasAttr = false;
                foreach (var attr in propertySymbol.GetAttributes())
                {
                    if (attr.AttributeClass?.ToDisplayString() == AttributeFullName)
                    {
                        hasAttr = true;
                        break;
                    }
                }

                if (!hasAttr)
                {
                    continue;
                }

                var propName = propertySymbol.Name;
                var parameterName = GetParameterName(propName);
                var typeName = propertySymbol.Type.ToDisplayString();
                var isNullable = propertySymbol.NullableAnnotation == NullableAnnotation.Annotated;
                var baseTypeName = typeName.TrimEnd('?');

                return new InjectedModelInfo(propName, parameterName, baseTypeName, isNullable);
            }
        }

        return null;
    }

    private static string GetParameterName(string memberName)
    {
        var name = memberName.TrimStart('_');
        if (name.Length == 0)
        {
            return memberName;
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
