namespace Catel.SourceGenerators.Tests
{
    using System.Runtime.CompilerServices;
    using VerifyTests;

    public class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            VerifySourceGenerators.Initialize();
        }
    }
}
