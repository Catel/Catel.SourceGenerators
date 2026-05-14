namespace Catel.SourceGenerators.XamlConstructors;

public readonly record struct ParameterInfo
{
    public readonly string Name;
    public readonly string ParameterTypeName;
    public readonly bool IsNullable;

    public ParameterInfo(string name, string parameterTypeName, bool isNullable)
    {
        Name = name;
        ParameterTypeName = parameterTypeName;
        IsNullable = isNullable;
    }
}
