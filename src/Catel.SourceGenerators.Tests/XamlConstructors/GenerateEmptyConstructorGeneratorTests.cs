namespace Catel.SourceGenerators.Tests.XamlConstructors;

using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using VerifyNUnit;

[TestFixture]
public partial class GenerateEmptyConstructorGeneratorTests
{
    [Test]
    public async Task Generates_GetService_Only_For_Class_With_Attribute_And_No_Ctors()
    {
        var driver = BuildDriverWithNoCtors();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    [Test]
    public async Task Generates_Empty_Ctor_For_Class_With_Attribute_And_DI_Ctor()
    {
        var driver = BuildDriverWithDiCtor();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    [Test]
    public async Task Generates_Ctors_For_Class_With_Attribute_And_Injected_Services()
    {
        var driver = BuildDriverWithInjectedServices();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    [Test]
    public async Task Reports_Error_When_Class_Has_Existing_Ctor_And_Injected_Services()
    {
        var driver = BuildDriverWithConflictingCtor();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    [Test]
    public async Task Does_Not_Generate_For_Non_Partial_Class()
    {
        var source = @"
namespace MyNamespace
{
    [Catel.GenerateEmptyConstructor]
    public class MyClass
    {
        public MyClass(IMyService service) { }
    }

    public interface IMyService {}
}
";
        var compilation = CSharpCompilation.Create("name",
            new[] { source, BuildStubSource() }.Select(x => CSharpSyntaxTree.ParseText(x)));

        var generator = new GenerateEmptyConstructorSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.That(result.GeneratedTrees, Is.Empty);
    }

    private GeneratorDriver BuildDriverWithNoCtors()
    {
        var source = @"
namespace MyNamespace
{
    [Catel.GenerateEmptyConstructor]
    public partial class MyClass
    {
    }
}
";
        return BuildDriver(source);
    }

    private GeneratorDriver BuildDriverWithDiCtor()
    {
        var source = @"
namespace MyNamespace
{
    [Catel.GenerateEmptyConstructor]
    public partial class MyClass
    {
        public MyClass(IMyService1 service1, IMyService2 service2) { }
    }

    public interface IMyService1 {}
    public interface IMyService2 {}
}
";
        return BuildDriver(source);
    }

    private GeneratorDriver BuildDriverWithInjectedServices()
    {
        var source = @"
namespace MyNamespace
{
    [Catel.GenerateEmptyConstructor]
    public partial class MyClass
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
        return BuildDriver(source);
    }

    private GeneratorDriver BuildDriverWithConflictingCtor()
    {
        var source = @"
namespace MyNamespace
{
    [Catel.GenerateEmptyConstructor]
    public partial class MyClass
    {
        [Catel.InjectedService]
        private readonly IMyService1 _myService1;

        public MyClass(IMyService1 service1) { }
    }

    public interface IMyService1 {}
}
";
        return BuildDriver(source);
    }

    private GeneratorDriver BuildDriver(string classSource)
    {
        var compilation = CSharpCompilation.Create("name",
            new[] { classSource, BuildStubSource() }.Select(x => CSharpSyntaxTree.ParseText(x)));

        var generator = new GenerateEmptyConstructorSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }

    private static string BuildStubSource() => @"
namespace Catel
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal sealed class GenerateEmptyConstructorAttribute : System.Attribute { }

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
}
