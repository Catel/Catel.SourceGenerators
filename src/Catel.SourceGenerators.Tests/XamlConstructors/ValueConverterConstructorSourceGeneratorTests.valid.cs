namespace Catel.SourceGenerators.Tests.XamlConstructors
{
    using Catel.SourceGenerators.XamlConstructors;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;
    using System.Linq;
    using System.Threading.Tasks;
    using VerifyNUnit;

    [TestFixture]
    public partial class ValueConverterConstructorSourceGeneratorTests
    {
        [Test]
        public async Task Generates_Empty_Constructor_For_Generic_ValueConverter()
        {
            var driver = BuildGenericValueConverterConstructorDriver();

            await Verifier.Verify(driver);
        }

        private GeneratorDriver BuildGenericValueConverterConstructorDriver()
        {
            var userControlSource = @"
using Catel.IoC;

namespace MyNamespace
{
    public class MyConverter : ValueConverterBase<string>
    {
        public MyConverter(ILanguageService languageService)
        {
        }
    }

    public abstract class ValueConverterBase<T> : IValueConverter {}

    public interface IValueConverter : System.Windows.Data.IValueConverter {}

    public interface LanguageService {}
}
";

            var iocContainerSource = @"
namespace Catel.IoC
{
    public static class IoCContainer
    {
        public static ProviderType Provider { get; } = new ProviderType();
        public class ProviderType
        {
            public T GetRequiredService<T>() => default(T);
        }
    }
}
";

            var compilation = CSharpCompilation.Create("name", new[]
            {
                userControlSource,
                iocContainerSource
            }.Select(x => CSharpSyntaxTree.ParseText(x)));

            var generator = new ValueConverterConstructorsSourceGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            return driver.RunGenerators(compilation);
        }
    }
}
