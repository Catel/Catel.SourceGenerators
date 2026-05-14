namespace Catel.SourceGenerators.Tests.XamlConstructors;

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
    public async Task Generates_Ctors_When_No_Ctors()
    {
        var driver = BuildClassWithoutConstructorsDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildClassWithoutConstructorsDriver()
    {
        var userControlSource = @"
using System.Windows.Controls;
using Catel.IoC;
using Catel.MVVM;

namespace MyNamespace
{
    public partial class MyUserControl : UserControlBase
    {
        static MyUserControl()
        {
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

    public abstract class UserControlBase : Catel.Windows.Controls.UserControl
    {
        protected MyUserControl(ILogger<MyUserControl> logger, IUserControlWrapperService userControlWrapperService)
        {
        }

        protected MyUserControl(IViewModel? viewModel, ILogger<MyUserControl> logger, IUserControlWrapperService userControlWrapperService)
        {
        }
    }
}
";

        var iocContainerSource = @"
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
