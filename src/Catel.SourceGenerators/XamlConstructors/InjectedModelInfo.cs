namespace Catel.SourceGenerators.XamlConstructors;

using System;

public readonly record struct InjectedModelInfo : IEquatable<InjectedModelInfo>
{
    public readonly string MemberName;
    public readonly string ParameterName;
    public readonly string TypeName;
    public readonly bool IsNullable;

    public InjectedModelInfo(string memberName, string parameterName, string typeName, bool isNullable)
    {
        MemberName = memberName;
        ParameterName = parameterName;
        TypeName = typeName;
        IsNullable = isNullable;
    }
}
