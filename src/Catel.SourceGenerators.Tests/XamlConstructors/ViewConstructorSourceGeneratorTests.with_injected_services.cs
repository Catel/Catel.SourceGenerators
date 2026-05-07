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
    public partial class ViewCtorGeneratorTests
    {
        [Test]
        public async Task Generates_Ctors_For_View_With_Injected_Services()
        {
            var driver = BuildViewWithInjectedServicesDriver();

            await Verifier.Verify(driver)
                .ScrubAssemblyVersion();
        }

        private GeneratorDriver BuildViewWithInjectedServicesDriver()
        {
            var userControlSource = @"
using Catel.IoC;
using Catel.MVVM;

namespace MyNamespace
{
    public partial class MyUserControl : UserControlBase
    {
        [Catel.InjectedService]
        private readonly IMyService1 _myService1;

        [Catel.InjectedService]
        private readonly IMyService2 _myService2;
    }

    public partial class MyUserControl
    {
        public void InitializeComponent()
        {
        }
    }

    public interface ILogger<T> {}
    public interface IUserControlWrapperService {}
    public interface IMyService1 {}
    public interface IMyService2 {}

    public abstract class UserControlBase : Catel.Windows.Controls.UserControl
    {
        protected UserControlBase(ILogger<MyUserControl> logger, IUserControlWrapperService userControlWrapperService)
        {
        }
    }
}
";

            var stubSource = @"
namespace Catel
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal sealed class InjectedServiceAttribute : System.Attribute { }
}

namespace Catel.MVVM
{
    public interface IViewModel
    {
    }
}

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
                stubSource
            }.Select(x => CSharpSyntaxTree.ParseText(x)));

            var generator = new ViewConstructorsSourceGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            return driver.RunGenerators(compilation);
        }
    }
}
