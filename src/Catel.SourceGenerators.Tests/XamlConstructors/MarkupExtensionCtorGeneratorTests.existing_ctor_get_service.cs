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
    public async Task Generates_GetService_For_MarkupExtension_With_Existing_Ctor()
    {
        var driver = BuildMarkupExtensionWithExistingCtorDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildMarkupExtensionWithExistingCtorDriver()
    {
        var markupExtensionSource = @"
namespace MyNamespace
{
    public partial class MyMarkupExtension : System.Windows.Markup.MarkupExtension
    {
        public MyMarkupExtension()
        {
        }

        public override object ProvideValue(System.IServiceProvider serviceProvider) => null;
    }
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
