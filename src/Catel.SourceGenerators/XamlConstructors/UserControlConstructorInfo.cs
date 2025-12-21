namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Collections.Generic;

    public readonly record struct UserControlConstructorInfo
    {
        public readonly string FileName;
        public readonly string NamespaceName;
        public readonly string ClassName;
        public readonly EquatableArray<string> ParameterTypeNames;

        public UserControlConstructorInfo(string fileName, string namespaceName, string className, IReadOnlyList<string> parameterTypeNames)
        {
            FileName = fileName;
            NamespaceName = namespaceName;
            ClassName = className;
            ParameterTypeNames = new(parameterTypeNames);
        }
    }
}
