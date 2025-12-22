namespace Catel.SourceGenerators.XamlConstructors
{
    using System;
    using System.Collections.Generic;

    public readonly record struct UserControlConstructorsInfo : IEquatable<UserControlConstructorsInfo>
    {
        public readonly string FileName;
        public readonly string NamespaceName;
        public readonly string ClassName;
        public readonly EquatableArray<ConstructorInfo> Constructors;

        public UserControlConstructorsInfo(string fileName, string namespaceName, string className, 
            IReadOnlyList<ConstructorInfo> constructors)
        {
            FileName = fileName;
            NamespaceName = namespaceName;
            ClassName = className;
            Constructors = new(constructors);
        }

        public bool Equals(UserControlConstructorsInfo other)
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
