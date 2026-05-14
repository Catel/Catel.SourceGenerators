namespace Catel.SourceGenerators;

using Microsoft.CodeAnalysis;

internal static class IParameterSymbolExtensions
{
    public static bool IsNullable(this Microsoft.CodeAnalysis.IParameterSymbol parameterSymbol)
    {
        return parameterSymbol.NullableAnnotation == NullableAnnotation.Annotated;
    }
}
