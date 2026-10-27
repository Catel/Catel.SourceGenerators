namespace Catel.SourceGenerators.Tests.XamlConstructors;

using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    [TestCase("IUserControlWrapperService", "_userControlWrapperService", "userControlWrapperService")]
    [TestCase("IUserControlWrapperService", "_wrapperService", "userControlWrapperService")]
    [TestCase("ILogger<MyUserControl>", "_logger", "logger")]
    [TestCase("ILogger<MyUserControl>", "_viewLogger", "logger")]
    public void Reuses_Base_Constructor_Parameter_For_Injected_Service(string serviceType, string fieldName, string parameterName)
    {
        var driver = BuildViewWithInjectedServicesDriver($@"
        [Catel.InjectedService]
        private readonly {serviceType} {fieldName};");

        var generatedTree = driver.GetRunResult().GeneratedTrees.Single();
        var constructors = generatedTree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>()
            .Where(x => !x.Modifiers.Any(SyntaxKind.StaticKeyword)).ToArray();
        var injectingConstructor = constructors.Single(x => x.ParameterList.Parameters.Count > 0);
        var emptyConstructor = constructors.Single(x => x.ParameterList.Parameters.Count == 0);

        Assert.That(injectingConstructor.ParameterList.Parameters.Select(x => x.Identifier.Text),
            Is.EqualTo(new[] { "logger", "userControlWrapperService", "myService1", "myService2" }));
        Assert.That(injectingConstructor.Initializer!.ArgumentList.Arguments.Select(x => x.ToString()),
            Is.EqualTo(new[] { "logger", "userControlWrapperService" }));
        Assert.That(injectingConstructor.Body!.Statements.Select(x => x.ToString()),
            Does.Contain($"{fieldName} = {parameterName};"));
        Assert.That(emptyConstructor.Initializer!.ArgumentList.Arguments.Select(x => x.ToString()),
            Is.EqualTo(new[]
            {
                "GetService<MyNamespace.ILogger<MyNamespace.MyUserControl>>()",
                "GetService<MyNamespace.IUserControlWrapperService>()",
                "GetService<MyNamespace.IMyService1>()",
                "GetService<MyNamespace.IMyService2>()"
            }));
    }

    [Test]
    public void Reuses_Base_Constructor_Parameter_For_Multiple_Injected_Fields()
    {
        var driver = BuildViewWithInjectedServicesDriver(@"
        [Catel.InjectedService]
        private readonly IUserControlWrapperService _wrapperService;

        [Catel.InjectedService]
        private readonly IUserControlWrapperService _otherWrapperService;");

        var generatedTree = driver.GetRunResult().GeneratedTrees.Single();
        var constructor = generatedTree.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>()
            .Single(x => x.ParameterList.Parameters.Count > 0);

        Assert.That(constructor.ParameterList.Parameters.Count, Is.EqualTo(4));
        Assert.That(constructor.Body!.Statements.Select(x => x.ToString()),
            Does.Contain("_wrapperService = userControlWrapperService;")
                .And.Contain("_otherWrapperService = userControlWrapperService;"));
    }

    private GeneratorDriver BuildViewWithInjectedServicesDriver(string additionalInjectedFields = "")
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
            stubSource,
            "namespace MyNamespace { public partial class MyUserControl { " + additionalInjectedFields + " } }"
        }.Select(x => CSharpSyntaxTree.ParseText(x)));

        var generator = new ViewConstructorsSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }
}
