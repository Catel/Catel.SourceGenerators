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
    public async Task Generates_Ctor_For_Empty_ViewModel()
    {
        var driver = BuildEmptyViewModelDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    [Test]
    public async Task Generates_Nothing_When_Empty_ViewModel_Has_Existing_Ctor()
    {
        var driver = BuildEmptyViewModelWithExistingCtorDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildEmptyViewModelDriver()
    {
        var viewModelSource = @"
namespace MyNamespace
{
    public partial class MyViewModel : Catel.MVVM.ViewModelBase
    {
    }
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

    private GeneratorDriver BuildEmptyViewModelWithExistingCtorDriver()
    {
        var viewModelSource = @"
namespace MyNamespace
{
    public partial class MyViewModel : Catel.MVVM.ViewModelBase
    {
        public MyViewModel(System.IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
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
