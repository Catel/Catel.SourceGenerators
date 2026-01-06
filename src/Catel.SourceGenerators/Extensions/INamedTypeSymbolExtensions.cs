namespace Catel.SourceGenerators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
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
    }
}
