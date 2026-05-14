namespace Catel.SourceGenerators;

using Microsoft.CodeAnalysis;

internal static class ITypeSymbolExtensions
{
    public static bool ImplementsInterface(this ITypeSymbol typeSymbol, string interfaceMetadataName)
    {
        if (typeSymbol.IsType(interfaceMetadataName))
        {
            return true;
        }

        foreach (var interfaceType in typeSymbol.AllInterfaces)
        {
            if (interfaceType.IsType(interfaceMetadataName))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsType(this ITypeSymbol? typeSymbol, string fullTypeName)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        var typeName = typeSymbol.GetFullTypeName().TrimEnd('?');
        return typeName == fullTypeName;
    }

    public static string GetFullTypeName(this ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return string.Empty;
        }

        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", string.Empty);
    }
}
