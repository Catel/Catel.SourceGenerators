namespace Catel.SourceGenerators.Tests.XamlConstructors;

using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using VerifyNUnit;

[TestFixture]
public partial class ValueConvCtorGeneratorTests
{
    [Test]
    public async Task Generates_GetService_For_Partial_ValueConverter_With_Existing_Ctor()
    {
        var driver = BuildPartialValueConverterWithExistingCtorDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildPartialValueConverterWithExistingCtorDriver()
    {
        var source = @"
namespace MyNamespace
{
    public partial class MyConverter : System.Windows.Data.IValueConverter
    {
        public MyConverter()
        {
        }

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) => null;
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture) => null;
    }
}
";

        var stubSource = @"
namespace System.Windows.Data
{
    public interface IValueConverter
    {
        object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture);
        object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture);
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
            source,
            stubSource
        }.Select(x => CSharpSyntaxTree.ParseText(x)));

        var generator = new ValueConverterConstructorsSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }
}
