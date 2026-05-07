namespace Catel.SourceGenerators.XamlConstructors
{
    using System;
    using System.Collections.Generic;

    public readonly record struct ViewModelConstructorInfo : IEquatable<ViewModelConstructorInfo>
    {
        public readonly string FileName;
        public readonly string NamespaceName;
        public readonly string ClassName;
        public readonly EquatableArray<InjectedServiceInfo> InjectedServices;
        public readonly bool HasInjectedModel;
        public readonly InjectedModelInfo InjectedModel;
        public readonly bool HasConflictingConstructors;

        public ViewModelConstructorInfo(string fileName, string namespaceName, string className,
            IReadOnlyList<InjectedServiceInfo> injectedServices,
            InjectedModelInfo? injectedModel = null,
            bool hasConflictingConstructors = false)
        {
            FileName = fileName;
            NamespaceName = namespaceName;
            ClassName = className;
            InjectedServices = new(injectedServices);
            HasInjectedModel = injectedModel.HasValue;
            InjectedModel = injectedModel ?? default;
            HasConflictingConstructors = hasConflictingConstructors;
        }

        public bool Equals(ViewModelConstructorInfo other)
        {
            return NamespaceName == other.NamespaceName &&
                   ClassName == other.ClassName &&
                   InjectedServices == other.InjectedServices &&
                   HasInjectedModel == other.HasInjectedModel &&
                   (!HasInjectedModel || InjectedModel == other.InjectedModel) &&
                   HasConflictingConstructors == other.HasConflictingConstructors;
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
