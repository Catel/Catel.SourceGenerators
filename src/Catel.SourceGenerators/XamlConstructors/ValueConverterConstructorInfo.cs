namespace Catel.SourceGenerators.XamlConstructors;

using System.Collections.Generic;

public readonly record struct ValueConverterConstructorInfo
{
    public readonly string FileName;
    public readonly string NamespaceName;
    public readonly string ClassName;
    public readonly EquatableArray<string> ParameterTypeNames;
    public readonly bool GenerateGetServiceOnly;

    internal readonly string? ClassDeclarationName;
    internal readonly EquatableArray<ParameterInfo> BaseParameters;
    internal readonly EquatableArray<InjectedServiceInfo> InjectedServices;
    internal readonly bool HasConflictingConstructors;

    internal ValueConverterConstructorInfo(string fileName, string namespaceName, string className, string classDeclarationName,
        IReadOnlyList<ParameterInfo> baseParameters, IReadOnlyList<InjectedServiceInfo> injectedServices, bool hasConflictingConstructors)
        : this(fileName, namespaceName, className, System.Array.Empty<string>())
    {
        ClassDeclarationName = classDeclarationName;
        BaseParameters = new(baseParameters);
        InjectedServices = new(injectedServices);
        HasConflictingConstructors = hasConflictingConstructors;
    }

    public ValueConverterConstructorInfo(string fileName, string namespaceName, string className, IReadOnlyList<string> parameterTypeNames, bool generateGetServiceOnly = false)
    {
        FileName = fileName;
        NamespaceName = namespaceName;
        ClassName = className;
        ParameterTypeNames = new(parameterTypeNames);
        GenerateGetServiceOnly = generateGetServiceOnly;
    }

    public bool Equals(ValueConverterConstructorInfo other)
    {
        return NamespaceName == other.NamespaceName &&
               ClassName == other.ClassName &&
               ParameterTypeNames == other.ParameterTypeNames &&
               ClassDeclarationName == other.ClassDeclarationName &&
               BaseParameters == other.BaseParameters &&
               InjectedServices == other.InjectedServices &&
               HasConflictingConstructors == other.HasConflictingConstructors &&
               GenerateGetServiceOnly == other.GenerateGetServiceOnly;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(NamespaceName);
        hashCode.Add(ClassName);

        return hashCode.ToHashCode();
    }
}
