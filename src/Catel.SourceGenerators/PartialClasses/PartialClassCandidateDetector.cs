namespace Catel.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

internal static class PartialClassCandidateDetector
{
    private static readonly HashSet<string> CatelViewBaseTypeNames =
    [
        "Catel.Windows.Controls.UserControl",
        "Catel.Windows.DataWindow",
        "Catel.Windows.Window"
    ];

    private static readonly HashSet<string> ViewTerminalTypeNames =
    [
        "System.Windows.Controls.Control",
        "System.Windows.Controls.UserControl",
        "System.Windows.Window"
    ];

    public static string? GetSupportedTypeDescription(SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax)
    {
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
        if (classSymbol is null || classSymbol.IsAbstract)
        {
            return null;
        }

        if (!semanticModel.IsCatelAssembly() &&
            classSymbol.DerivesFromBaseClass("Catel.MVVM.ViewModelBase"))
        {
            return "view model";
        }

        if (IsBehavior(classSymbol))
        {
            return "behavior";
        }

        if (classSymbol.DerivesFromBaseClass("System.Windows.Markup.MarkupExtension"))
        {
            return "markup extension";
        }

        if (!semanticModel.IsCatelAssembly() &&
            IsView(classSymbol))
        {
            return "view";
        }

        return null;
    }

    private static bool IsBehavior(INamedTypeSymbol classSymbol)
    {
        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            if (baseType.IsType("Microsoft.Xaml.Behaviors.Behavior"))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool IsView(INamedTypeSymbol classSymbol)
    {
        var hasCatelViewBase = false;
        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            var typeName = baseType.GetFullTypeName();
            if (!hasCatelViewBase && CatelViewBaseTypeNames.Contains(typeName))
            {
                hasCatelViewBase = true;
            }

            if (ViewTerminalTypeNames.Contains(typeName))
            {
                return hasCatelViewBase;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }
}
