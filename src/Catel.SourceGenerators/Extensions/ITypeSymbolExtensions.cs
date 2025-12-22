namespace Catel.SourceGenerators
{
    using Microsoft.CodeAnalysis;

    internal static class ITypeSymbolExtensions
    {
        public static bool ImplementsInterface(this ITypeSymbol typeSymbol, string interfaceMetadataName)
        {
            foreach (var interfaceType in typeSymbol.AllInterfaces)
            {
                if (interfaceType.ToDisplayString() == interfaceMetadataName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
