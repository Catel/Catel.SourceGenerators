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
            var displayString = baseType.ToDisplayString();
            if (displayString == typeName)
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }
}
