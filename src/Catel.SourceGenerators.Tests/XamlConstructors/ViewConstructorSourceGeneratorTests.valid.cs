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
    public partial class ViewConstructorSourceGeneratorTests
    {
        [Test]
        public async Task Generates_Empty_Constructor_And_Attributes_For_UserControl()
        {
            var driver = BuildConstructorWithParametersDriver();

            await Verifier.Verify(driver)
                .ScrubAssemblyVersion();
        }

        private GeneratorDriver BuildConstructorWithParametersDriver()
        {
            var userControlSource = @"
using System.Windows.Controls;
using Catel.IoC;

namespace MyNamespace
{
    public partial class MyUserControl : Catel.Windows.Controls.UserControl
    {
        public MyUserControl(ILogger<MyUserControl> logger, IUserControlWrapperService userControlWrapperService)
            : base(logger, userControlWrapperService)
        {
            InitializeComponent();
        }
    }

    public partial class MyUserControl
    {
        public void InitializeComponent()
        {
        }
    }

    public interface ILogger<T> {}
    public interface IUserControlWrapperService {}
}
";

            var iocContainerSource = @"
namespace Catel.Windows.Controls
{
    public class UserControl : System.Windows.Controls.UserControl
    {
        
    }
}

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

            var generator = new ViewConstructorsSourceGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            return driver.RunGenerators(compilation);
        }
    }
}
