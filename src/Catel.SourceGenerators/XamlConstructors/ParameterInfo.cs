namespace Catel.SourceGenerators.XamlConstructors;

public readonly record struct ParameterInfo
{
    public readonly string Name;
    public readonly string ParameterTypeName;
    public readonly bool IsNullable;

    internal readonly string? DefaultValueExpression;

    internal ParameterInfo(string name, string parameterTypeName, bool isNullable, string? defaultValueExpression)
        : this(name, parameterTypeName, isNullable)
    {
        DefaultValueExpression = defaultValueExpression;
    }

    public ParameterInfo(string name, string parameterTypeName, bool isNullable)
    {
        Name = name;
        ParameterTypeName = parameterTypeName;
        IsNullable = isNullable;
    }
}
