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
    public partial class UserControlConstructorSourceGeneratorTests
    {
        [Test]
        public async Task Generates_Constructors_When_No_Constructors()
        {
            var driver = BuildClassWithoutConstructorsDriver();

            await Verifier.Verify(driver);
        }

        private GeneratorDriver BuildClassWithoutConstructorsDriver()
        {
            var userControlSource = @"
using System.Windows.Controls;
using Catel.IoC;

namespace MyNamespace
{
    public partial class MyUserControl : UserControlBase
    {

    }

    public partial class MyUserControl
    {
        public void InitializeComponent()
        {
        }
    }

    public interface ILogger<T> {}
    public interface IUserControlWrapperService {}

    public abstract class UserControlBase : System.Windows.Controls.UserControl
    {
        protected MyUserControl(ILogger<MyUserControl> logger, IUserControlWrapperService userControlWrapperService)
        {
        }
    }
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

            var generator = new UserControlConstructorsSourceGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            return driver.RunGenerators(compilation);
        }
    }
}
