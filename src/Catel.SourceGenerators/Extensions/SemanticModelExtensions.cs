namespace Catel.SourceGenerators
{
    using Microsoft.CodeAnalysis;

    internal static class SemanticModelExtensions
    {
        public static bool IsCatelAssembly(this SemanticModel? semanticModel)
        {
            if (semanticModel is null)
            {
                return false;
            }

            var assemblyName = semanticModel.Compilation.AssemblyName;
            return assemblyName == "Catel.Core" ||
                   assemblyName == "Catel.MVVM";
        }
    }
}
