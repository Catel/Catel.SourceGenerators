namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Collections.Generic;

    public readonly record struct MarkupExtensionConstructorInfo
    {
        public readonly string FileName;
        public readonly string NamespaceName;
        public readonly string ClassName;
        public readonly EquatableArray<string> ParameterTypeNames;
        public readonly EquatableArray<InjectedServiceInfo> InjectedServices;

        public MarkupExtensionConstructorInfo(string fileName, string namespaceName, string className,
            IReadOnlyList<string> parameterTypeNames, IReadOnlyList<InjectedServiceInfo> injectedServices)
        {
            FileName = fileName;
            NamespaceName = namespaceName;
            ClassName = className;
            ParameterTypeNames = new(parameterTypeNames);
            InjectedServices = new(injectedServices);
        }

        public bool Equals(BehaviorConstructorInfo other)
        {
            return NamespaceName == other.NamespaceName &&
                   ClassName == other.ClassName &&
                   ParameterTypeNames == other.ParameterTypeNames &&
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
}
