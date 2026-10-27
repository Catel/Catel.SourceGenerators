namespace Catel.SourceGenerators.XamlConstructors;

using System.Collections.Generic;

public readonly record struct MarkupExtensionConstructorInfo
{
    public readonly string FileName;
    public readonly string NamespaceName;
    public readonly string ClassName;
    public readonly EquatableArray<string> ParameterTypeNames;
    public readonly EquatableArray<InjectedServiceInfo> InjectedServices;
    public readonly bool HasConflictingConstructors;
    public readonly bool GenerateGetServiceOnly;

    internal readonly EquatableArray<ParameterInfo> BaseParameters;

    internal MarkupExtensionConstructorInfo(string fileName, string namespaceName, string className,
        IReadOnlyList<string> parameterTypeNames, IReadOnlyList<InjectedServiceInfo> injectedServices,
        IReadOnlyList<ParameterInfo> baseParameters)
        : this(fileName, namespaceName, className, parameterTypeNames, injectedServices)
    {
        BaseParameters = new(baseParameters);
    }

    public MarkupExtensionConstructorInfo(string fileName, string namespaceName, string className,
        IReadOnlyList<string> parameterTypeNames, IReadOnlyList<InjectedServiceInfo> injectedServices,
        bool hasConflictingConstructors = false, bool generateGetServiceOnly = false)
    {
        FileName = fileName;
        NamespaceName = namespaceName;
        ClassName = className;
        ParameterTypeNames = new(parameterTypeNames);
        InjectedServices = new(injectedServices);
        HasConflictingConstructors = hasConflictingConstructors;
        GenerateGetServiceOnly = generateGetServiceOnly;
    }

    public bool Equals(MarkupExtensionConstructorInfo other)
    {
        return NamespaceName == other.NamespaceName &&
               ClassName == other.ClassName &&
               ParameterTypeNames == other.ParameterTypeNames &&
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
