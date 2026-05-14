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
    public async Task Reports_Error_When_ViewModel_Has_Existing_Ctor_And_Injected_Services()
    {
        var driver = BuildViewModelWithExistingCtorAndInjectedServicesDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildViewModelWithExistingCtorAndInjectedServicesDriver()
    {
        var viewModelSource = @"
namespace MyNamespace
{
    public partial class MyViewModel : Catel.MVVM.ViewModelBase
    {
        [Catel.InjectedService]
        private readonly IMyService1 _myService1;

        public MyViewModel(System.IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    public interface IMyService1 {}
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
}
