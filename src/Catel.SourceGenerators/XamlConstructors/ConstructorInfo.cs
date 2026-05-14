namespace Catel.SourceGenerators.XamlConstructors;

using System.Collections.Generic;

public readonly record struct ConstructorInfo
{
    public readonly string ClassName;
    public readonly EquatableArray<ParameterInfo> Parameters;
    public readonly bool CallBase;
    public readonly bool IsActivatorUtilitiesConstructor;

    public ConstructorInfo(string className, IReadOnlyList<ParameterInfo> parameters, 
        bool callBase, bool isActivatorUtilitiesConstructor)
    {
        ClassName = className;
        Parameters = new(parameters);
        CallBase = callBase;
        IsActivatorUtilitiesConstructor = isActivatorUtilitiesConstructor;
    }
}
