namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Collections.Generic;

    public readonly record struct XamlConstructorInfo
    {
        public readonly string NamespaceName;
        public readonly string ClassName;
        public readonly IReadOnlyList<string> ParameterTypeNames;

        public XamlConstructorInfo(string namespaceName, string className, IReadOnlyList<string> parameterTypeNames)
        {
            NamespaceName = namespaceName;
            ClassName = className;
            ParameterTypeNames = parameterTypeNames;
        }
    }
}
