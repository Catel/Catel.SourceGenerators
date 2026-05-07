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
    public partial class BehaviorCtorGeneratorTests
    {
        [Test]
        public async Task Generates_Ctors_For_Behavior_With_Injected_Services()
        {
            var driver = BuildBehaviorWithInjectedServicesDriver();

            await Verifier.Verify(driver)
                .ScrubAssemblyVersion();
        }

        private GeneratorDriver BuildBehaviorWithInjectedServicesDriver()
        {
            var behaviorSource = @"
namespace MyNamespace
{
    public partial class MyBehavior : Microsoft.Xaml.Behaviors.Behavior<System.Windows.FrameworkElement>
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

            var stubSource = @"
namespace Microsoft.Xaml.Behaviors
{
    public abstract class Behavior { }
    public abstract class Behavior<T> : Behavior { }
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
                behaviorSource,
                stubSource
            }.Select(x => CSharpSyntaxTree.ParseText(x)));

            var generator = new BehaviorConstructorsSourceGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            return driver.RunGenerators(compilation);
        }
    }
}
