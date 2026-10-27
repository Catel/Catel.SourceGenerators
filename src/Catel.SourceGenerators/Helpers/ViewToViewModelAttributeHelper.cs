namespace Catel.SourceGenerators.XamlConstructors;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

internal static class ViewToViewModelAttributeHelper
{
    internal const string AttributeFullName = "Catel.MVVM.Views.ViewToViewModelAttribute";
    private const string DependencyPropertyTypeName = "System.Windows.DependencyProperty";

    internal static List<string> GetViewToViewModelProperties(INamedTypeSymbol classSymbol)
    {
        var properties = new List<string>();

        var currentType = (INamedTypeSymbol?)classSymbol;
        while (currentType is not null)
        {
            var displayString = currentType.GetFullTypeName();
            if (displayString.StartsWith("Catel.") ||
                displayString.StartsWith("System.") ||
                displayString.StartsWith("Microsoft."))
            {
                break;
            }

            foreach (var member in currentType.GetMembers())
            {
                if (member is IPropertySymbol propertySymbol)
                {
                    var hasAttr = propertySymbol.GetAttributes()
                        .Any(a => a.AttributeClass?.IsType(AttributeFullName) ?? false);
                    if (hasAttr && IsDependencyProperty(currentType, propertySymbol.Name))
                    {
                        properties.Add(propertySymbol.Name);
                    }
                }
            }

            currentType = currentType.BaseType;
        }

        return properties;
    }

    private static bool IsDependencyProperty(INamedTypeSymbol classSymbol, string propertyName)
    {
        var dependencyPropertyFieldName = propertyName + "Property";

        foreach (var member in classSymbol.GetMembers(dependencyPropertyFieldName))
        {
            if (member is IFieldSymbol fieldSymbol &&
                fieldSymbol.IsStatic &&
                fieldSymbol.Type.IsType(DependencyPropertyTypeName))
            {
                return true;
            }
        }

        return false;
    }
}
