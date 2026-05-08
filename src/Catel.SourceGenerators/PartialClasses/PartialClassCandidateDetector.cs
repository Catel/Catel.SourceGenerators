namespace Catel.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class PartialClassCandidateDetector
{
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
            if (baseType.ToDisplayString() == "Microsoft.Xaml.Behaviors.Behavior")
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool IsView(INamedTypeSymbol classSymbol)
    {
        var isCatelView = false;
        var baseType = classSymbol.BaseType;
        while (baseType is not null)
        {
            var displayString = baseType.ToDisplayString();
            if (!isCatelView)
            {
                if (displayString.Contains("Catel.Windows.Controls.UserControl") ||
                    displayString.Contains("Catel.Windows.DataWindow") ||
                    displayString.Contains("Catel.Windows.Window"))
                {
                    isCatelView = true;
                }
            }

            if (displayString == "System.Windows.Controls.Control" ||
                displayString == "System.Windows.Controls.UserControl" ||
                displayString == "System.Windows.Window")
            {
                return isCatelView;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }
}
