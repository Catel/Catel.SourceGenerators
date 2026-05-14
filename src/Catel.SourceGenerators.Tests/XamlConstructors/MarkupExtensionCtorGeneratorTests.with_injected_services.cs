namespace Catel.SourceGenerators.Tests.XamlConstructors;

using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using VerifyNUnit;

[TestFixture]
public partial class MarkupExtensionCtorGeneratorTests
{
    [Test]
    public async Task Generates_Ctors_For_MarkupExtension_With_Injected_Services()
    {
        var driver = BuildMarkupExtensionWithInjectedServicesDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildMarkupExtensionWithInjectedServicesDriver()
    {
        var markupExtensionSource = @"
namespace MyNamespace
{
    public partial class MyMarkupExtension : System.Windows.Markup.MarkupExtension
    {
        [Catel.InjectedService]
        private readonly IMyService1 _myService1;

        [Catel.InjectedService]
        private readonly IMyService2 _myService2;

        public override object ProvideValue(System.IServiceProvider serviceProvider) => null;
    }

    public interface IMyService1 {}
    public interface IMyService2 {}
}
";

        var stubSource = @"
namespace System.Windows.Markup
{
    public abstract class MarkupExtension
    {
        public abstract object ProvideValue(System.IServiceProvider serviceProvider);
    }
}

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
";

        var compilation = CSharpCompilation.Create("name", new[]
        {
            markupExtensionSource,
            stubSource
        }.Select(x => CSharpSyntaxTree.ParseText(x)));

        var generator = new MarkupExtensionConstructorsSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }
}
