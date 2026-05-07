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
    public partial class ViewModelCtorGeneratorTests
    {
        [Test]
        public async Task Reports_Error_When_ViewModel_Has_Existing_Ctor_And_Injected_Model()
        {
            var driver = BuildViewModelWithExistingCtorAndInjectedModelDriver();

            await Verifier.Verify(driver)
                .ScrubAssemblyVersion();
        }

        private GeneratorDriver BuildViewModelWithExistingCtorAndInjectedModelDriver()
        {
            var viewModelSource = @"
namespace MyNamespace
{
    public partial class MyViewModel : Catel.MVVM.ViewModelBase
    {
        [Catel.InjectedModel]
        public MyModel Model { get; private set; }

        public MyViewModel(MyModel model, System.IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Model = model;
        }
    }

    public class MyModel {}
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
}
