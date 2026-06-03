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
    public async Task Generates_Ctors_For_Non_Catel_UserControl_With_Injected_Services()
    {
        var driver = BuildNonCatelUserControlWithInjectedServicesDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    [Test]
    public async Task Generates_Ctors_For_Non_Catel_UserControl_Without_Injected_Services()
    {
        var driver = BuildNonCatelUserControlWithoutInjectedServicesDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    [Test]
    public async Task Generates_Ctors_For_Non_Catel_Window_With_Injected_Services()
    {
        var driver = BuildNonCatelWindowWithInjectedServicesDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildNonCatelUserControlWithInjectedServicesDriver()
    {
        var userControlSource = @"
namespace MyNamespace
{
    public partial class MyUserControl : System.Windows.Controls.UserControl
    {
        [Catel.InjectedService]
        private readonly IMyService _myService;
    }

    public partial class MyUserControl
    {
        public void InitializeComponent()
        {
        }
    }

    public interface IMyService {}
}
";

        var stubSource = @"
namespace Catel
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal sealed class InjectedServiceAttribute : System.Attribute { }
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

namespace System.Windows.Controls
{
    public class Control
    {
    }

    public class UserControl : Control
    {
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

    private GeneratorDriver BuildNonCatelUserControlWithoutInjectedServicesDriver()
    {
        var userControlSource = @"
namespace MyNamespace
{
    public partial class MyUserControl : System.Windows.Controls.UserControl
    {

    }

    public partial class MyUserControl
    {
        public void InitializeComponent()
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

namespace System.Windows.Controls
{
    public class Control
    {
    }

    public class UserControl : Control
    {
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

    private GeneratorDriver BuildNonCatelWindowWithInjectedServicesDriver()
    {
        var windowSource = @"
namespace MyNamespace
{
    public partial class MyWindow : System.Windows.Window
    {
        [Catel.InjectedService]
        private readonly IMyService _myService;
    }

    public partial class MyWindow
    {
        public void InitializeComponent()
        {
        }
    }

    public interface IMyService {}
}
";

        var stubSource = @"
namespace Catel
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal sealed class InjectedServiceAttribute : System.Attribute { }
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

namespace System.Windows
{
    public class Window
    {
    }
}
";

        var compilation = CSharpCompilation.Create("name", new[]
        {
            windowSource,
            stubSource
        }.Select(x => CSharpSyntaxTree.ParseText(x)));

        var generator = new ViewConstructorsSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }
}
