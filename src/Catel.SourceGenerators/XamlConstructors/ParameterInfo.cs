namespace Catel.SourceGenerators.XamlConstructors
{
    public readonly record struct ParameterInfo
    {
        public readonly string Name;
        public readonly string ParameterTypeName;

        public ParameterInfo(string name, string parameterTypeName)
        {
            Name = name;
            ParameterTypeName = parameterTypeName;
        }
    }
}
