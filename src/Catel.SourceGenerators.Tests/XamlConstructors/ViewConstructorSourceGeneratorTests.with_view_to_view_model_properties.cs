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
        public async Task Generates_Direct_Property_Subscriptions_For_ViewToViewModel_Properties()
        {
            var driver = BuildViewWithViewToViewModelPropertiesDriver();

            await Verifier.Verify(driver)
                .ScrubAssemblyVersion();
        }

        private GeneratorDriver BuildViewWithViewToViewModelPropertiesDriver()
        {
            var userControlSource = @"
using System.Windows.Controls;
using Catel.IoC;
using Catel.MVVM;

namespace MyNamespace
{
    public partial class MyUserControl : Catel.Windows.Controls.UserControl
    {
        [ViewToViewModel]
        public string Title { get; set; }

        [ViewToViewModel]
        public int Count { get; set; }

        public string NotMapped { get; set; }
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
namespace Catel.MVVM
{
    public interface IViewModel
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ViewToViewModelAttribute : System.Attribute
    {
    }
}

namespace Catel.Windows.Controls
{
    public class UserControl : System.Windows.Controls.UserControl
    {
        protected UserControl(ILogger logger, IUserControlWrapperService service) {}
    }

    public interface ILogger {}
    public interface IUserControlWrapperService {}
}

namespace Catel.IoC
{
    public static class IoCContainer
    {
        public static ProviderType ServiceProvider { get; } = new ProviderType();
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
