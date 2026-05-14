namespace Catel.SourceGenerators;

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class INamedTypeSymbolExtensions
{
    public static bool HasStaticConstructorWithContent(this INamedTypeSymbol namedTypeSymbol)
    {
        var staticCtor = namedTypeSymbol.Constructors.FirstOrDefault(x => x.IsStatic);
        if (staticCtor is null)
        {
            return false;
        }

        var syntaxReferences = staticCtor.DeclaringSyntaxReferences;

        foreach (var syntaxReference in syntaxReferences)
        {
            var constructorDeclarationSyntax = syntaxReference.GetSyntax() as ConstructorDeclarationSyntax;
            if (constructorDeclarationSyntax is null)
            {
                continue;
            }

            var body = constructorDeclarationSyntax.Body;
            if (body is null)
            {
                return false;
            }

            foreach (var childNode in body.ChildNodes())
            {
                // We found at least 1 child, thus body
                return true;
            }
        }

        return false;
    }

    public static bool DerivesFromBaseClass(this INamedTypeSymbol classSymbol, string typeName)
    {
        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            var displayString = baseType.GetFullTypeName();
            if (displayString == typeName)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    public static string GetFullTypeName(this INamedTypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return string.Empty;
        }

        return typeSymbol.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", string.Empty);
    }
}
