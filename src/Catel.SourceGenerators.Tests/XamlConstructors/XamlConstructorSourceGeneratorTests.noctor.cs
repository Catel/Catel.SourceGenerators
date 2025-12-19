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
    public partial class XamlConstructorSourceGeneratorTests
    {
        [Test]
        public async Task Generates_Nothing_When_No_Constructors_With_Parameters()
        {
            var driver = BuildNoOverloadsDriver();

            await Verifier.Verify(driver);
        }

        private GeneratorDriver BuildNoOverloadsDriver()
        {
            var userControlSource = @"
using System.Windows.Controls;
using Catel.IoC;

namespace MyNamespace
{
    public partial class MyUserControl : System.Windows.Controls.UserControl
    {
        public MyUserControl()
        {
            InitializeComponent();
        }
    }

    public interface ILogger<T> {}
    public interface IUserControlWrapperService {}
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

            var generator = new XamlConstructorSourceGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            return driver.RunGenerators(compilation);
        }
    }
}
