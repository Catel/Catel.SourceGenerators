namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Collections.Generic;

    public readonly record struct UserControlConstructorsInfo
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
    }
}
