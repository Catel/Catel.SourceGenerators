namespace Catel.SourceGenerators.XamlConstructors;

public readonly record struct InjectedServiceInfo
{
    public readonly string FieldName;
    public readonly string ParameterName;
    public readonly string TypeName;

    public InjectedServiceInfo(string fieldName, string parameterName, string typeName)
    {
        FieldName = fieldName;
        ParameterName = parameterName;
        TypeName = typeName;
    }
}
