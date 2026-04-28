namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Collections.Generic;

    public readonly record struct MarkupExtensionConstructorInfo
    {
        public readonly string FileName;
        public readonly string NamespaceName;
        public readonly string ClassName;
        public readonly EquatableArray<string> ParameterTypeNames;

        public MarkupExtensionConstructorInfo(string fileName, string namespaceName, string className, IReadOnlyList<string> parameterTypeNames)
        {
            FileName = fileName;
            NamespaceName = namespaceName;
            ClassName = className;
            ParameterTypeNames = new(parameterTypeNames);
        }

        public bool Equals(BehaviorConstructorInfo other)
        {
            return NamespaceName == other.NamespaceName &&
                   ClassName == other.ClassName &&
                   ParameterTypeNames == other.ParameterTypeNames;
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
