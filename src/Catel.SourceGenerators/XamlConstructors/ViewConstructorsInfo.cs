namespace Catel.SourceGenerators.XamlConstructors
{
    using System;
    using System.Collections.Generic;

    public readonly record struct ViewConstructorsInfo : IEquatable<ViewConstructorsInfo>
    {
        public readonly string FileName;
        public readonly string NamespaceName;
        public readonly string ClassName;
        public readonly bool CreateStaticConstructor;
        public readonly EquatableArray<ConstructorInfo> Constructors;

        public ViewConstructorsInfo(string fileName, string namespaceName, string className, 
            bool createStaticConstructor, IReadOnlyList<ConstructorInfo> constructors)
        {
            FileName = fileName;
            NamespaceName = namespaceName;
            ClassName = className;
            CreateStaticConstructor = createStaticConstructor;
            Constructors = new(constructors);
        }

        public bool Equals(ViewConstructorsInfo other)
        {
            return NamespaceName == other.NamespaceName &&
                   ClassName == other.ClassName &&
                   Constructors == other.Constructors;
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
