namespace Catel.SourceGenerators.XamlConstructors;

using System.Collections.Generic;

public readonly record struct BehaviorConstructorInfo
{
    public readonly string FileName;
    public readonly string NamespaceName;
    public readonly string ClassName;
    public readonly string ClassDeclarationName;
    public readonly EquatableArray<string> ParameterTypeNames;
    public readonly EquatableArray<InjectedServiceInfo> InjectedServices;
    public readonly bool HasConflictingConstructors;
    public readonly bool GenerateGetServiceOnly;

    public BehaviorConstructorInfo(string fileName, string namespaceName, string className, string classDeclarationName,
        IReadOnlyList<string> parameterTypeNames, IReadOnlyList<InjectedServiceInfo> injectedServices,
        bool hasConflictingConstructors = false, bool generateGetServiceOnly = false)
    {
        FileName = fileName;
        NamespaceName = namespaceName;
        ClassName = className;
        ClassDeclarationName = classDeclarationName;
        ParameterTypeNames = new(parameterTypeNames);
        InjectedServices = new(injectedServices);
        HasConflictingConstructors = hasConflictingConstructors;
        GenerateGetServiceOnly = generateGetServiceOnly;
    }

    public bool Equals(BehaviorConstructorInfo other)
    {
        return NamespaceName == other.NamespaceName &&
               ClassName == other.ClassName &&
               ClassDeclarationName == other.ClassDeclarationName &&
               ParameterTypeNames == other.ParameterTypeNames &&
               InjectedServices == other.InjectedServices &&
               HasConflictingConstructors == other.HasConflictingConstructors &&
               GenerateGetServiceOnly == other.GenerateGetServiceOnly;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(NamespaceName);
        hashCode.Add(ClassName);
        hashCode.Add(ClassDeclarationName);

        return hashCode.ToHashCode();
    }
}
