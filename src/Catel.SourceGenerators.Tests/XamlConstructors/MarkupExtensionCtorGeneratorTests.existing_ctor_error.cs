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
    public partial class MarkupExtensionCtorGeneratorTests
    {
        [Test]
        public async Task Reports_Error_When_MarkupExtension_Has_Existing_Ctor_And_Injected_Services()
        {
            var driver = BuildMarkupExtensionWithExistingCtorAndInjectedServicesDriver();

            await Verifier.Verify(driver)
                .ScrubAssemblyVersion();
        }

        private GeneratorDriver BuildMarkupExtensionWithExistingCtorAndInjectedServicesDriver()
        {
            var markupExtensionSource = @"
namespace MyNamespace
{
    public partial class MyMarkupExtension : System.Windows.Markup.MarkupExtension
    {
        [Catel.InjectedService]
        private readonly IMyService1 _myService1;

        public MyMarkupExtension(IMyService1 myService1)
        {
            _myService1 = myService1;
        }

        public override object ProvideValue(System.IServiceProvider serviceProvider) => null;
    }

    public interface IMyService1 {}
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
}
