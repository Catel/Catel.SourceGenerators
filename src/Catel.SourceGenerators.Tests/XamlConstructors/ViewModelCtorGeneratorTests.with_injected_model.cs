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
        public async Task Generates_Ctor_For_ViewModel_With_Injected_Model()
        {
            var driver = BuildViewModelWithInjectedModelDriver();

            await Verifier.Verify(driver)
                .ScrubAssemblyVersion();
        }

        private GeneratorDriver BuildViewModelWithInjectedModelDriver()
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

        [Catel.InjectedModel]
        public MyModel Model { get; private set; }
    }

    public class MyModel {}
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
    }
}
