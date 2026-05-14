namespace Catel.SourceGenerators.Tests.XamlConstructors;

using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using VerifyNUnit;

[TestFixture]
public partial class ViewModelCtorGeneratorTests
{
    [Test]
    public async Task Generates_Ctor_For_ViewModel_With_Injected_Services()
    {
        var driver = BuildViewModelWithInjectedServicesDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildViewModelWithInjectedServicesDriver()
    {
        var viewModelSource = @"
namespace MyNamespace
{
    public partial class MyViewModel : Catel.MVVM.ViewModelBase
    {
        [Catel.InjectedService]
        private readonly IMyService1 _myService1;

        [Catel.InjectedService]
        private readonly IMyService2 _myService2;
    }

    public interface IMyService1 {}
    public interface IMyService2 {}
}
";

        var stubSource = BuildStubSource();

        var compilation = CSharpCompilation.Create("name", new[]
        {
            viewModelSource,
            stubSource
        }.Select(x => CSharpSyntaxTree.ParseText(x)));

        var generator = new ViewModelConstructorsSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }

    private static string BuildStubSource()
    {
        return @"
namespace Catel
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    internal sealed class InjectedServiceAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    internal sealed class InjectedModelAttribute : System.Attribute { }
}

namespace Catel.MVVM
{
    public abstract class ViewModelBase
    {
        protected ViewModelBase(System.IServiceProvider serviceProvider)
        {
        }
    }
}
";
    }
}
