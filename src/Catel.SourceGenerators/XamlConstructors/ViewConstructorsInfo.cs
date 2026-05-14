namespace Catel.SourceGenerators.XamlConstructors;

using System;
using System.Collections.Generic;

public readonly record struct ViewConstructorsInfo : IEquatable<ViewConstructorsInfo>
{
    public readonly string FileName;
    public readonly string NamespaceName;
    public readonly string ClassName;
    public readonly bool CreateStaticConstructor;
    public readonly EquatableArray<ConstructorInfo> Constructors;
    public readonly EquatableArray<string> ViewToViewModelProperties;
    public readonly EquatableArray<InjectedServiceInfo> InjectedServices;

    public ViewConstructorsInfo(string fileName, string namespaceName, string className,
        bool createStaticConstructor, IReadOnlyList<ConstructorInfo> constructors,
        IReadOnlyList<string> viewToViewModelProperties,
        IReadOnlyList<InjectedServiceInfo> injectedServices)
    {
        FileName = fileName;
        NamespaceName = namespaceName;
        ClassName = className;
        CreateStaticConstructor = createStaticConstructor;
        Constructors = new(constructors);
        ViewToViewModelProperties = new(viewToViewModelProperties);
        InjectedServices = new(injectedServices);
    }

    public bool Equals(ViewConstructorsInfo other)
    {
        return NamespaceName == other.NamespaceName &&
               ClassName == other.ClassName &&
               Constructors == other.Constructors &&
               ViewToViewModelProperties == other.ViewToViewModelProperties &&
               InjectedServices == other.InjectedServices;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(NamespaceName);
        hashCode.Add(ClassName);

        return hashCode.ToHashCode();
    }
}
