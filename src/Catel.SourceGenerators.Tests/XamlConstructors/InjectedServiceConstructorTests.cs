namespace Catel.SourceGenerators.Tests.XamlConstructors;

using System;
using System.IO;
using System.Linq;
using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

[TestFixture]
public class InjectedServiceConstructorTests
{
    [TestCase("Behavior", "_languageService")]
    [TestCase("Behavior", "_localLanguageService")]
    [TestCase("Behavior", "languageService")]
    [TestCase("MarkupExtension", "_languageService")]
    [TestCase("MarkupExtension", "_localLanguageService")]
    [TestCase("MarkupExtension", "languageService")]
    [TestCase("GenerateEmptyConstructor", "_languageService")]
    [TestCase("GenerateEmptyConstructor", "_localLanguageService")]
    [TestCase("GenerateEmptyConstructor", "languageService")]
    [TestCase("ValueConverter", "_languageService")]
    [TestCase("ValueConverter", "_localLanguageService")]
    [TestCase("MultiValueConverter", "languageService")]
    public void Reuses_Base_Service_And_Resolves_It_Only_Once(string generatorKind, string fieldName)
    {
        var rootType = generatorKind == "Behavior" ? "Microsoft.Xaml.Behaviors.Behavior"
            : generatorKind == "MarkupExtension" ? "System.Windows.Markup.MarkupExtension"
            : generatorKind == "ValueConverter" ? "System.Windows.Data.IValueConverter"
            : generatorKind == "MultiValueConverter" ? "System.Windows.Data.IMultiValueConverter" : "object";
        var attribute = generatorKind == "GenerateEmptyConstructor" ? "[Catel.GenerateEmptyConstructor]" : string.Empty;
        var source = $@"
namespace MyNamespace
{{
    public interface ILanguageService<T> {{ }}
    public interface IOtherService {{ }}
    public abstract class BaseClass : {rootType}
    {{
        private BaseClass() {{ }}
        protected BaseClass(ILanguageService<int> languageService, string otherService) {{ }}
    }}
    {attribute}
    public partial class MyClass : BaseClass
    {{
        [Catel.InjectedService] private readonly ILanguageService<int> {fieldName};
        [Catel.InjectedService] private readonly ILanguageService<int> _secondService;
        [Catel.InjectedService] private readonly IOtherService _otherService;
    }}
}}";
        IIncrementalGenerator generator = generatorKind == "Behavior" ? new BehaviorConstructorsSourceGenerator()
            : generatorKind == "MarkupExtension" ? new MarkupExtensionConstructorsSourceGenerator()
            : generatorKind.EndsWith("ValueConverter") ? new ValueConverterConstructorsSourceGenerator()
            : new GenerateEmptyConstructorSourceGenerator();

        var output = Generate(source, generator);
        var constructors = output.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToArray();
        var injectingConstructor = constructors.Single(x => x.ParameterList.Parameters.Count > 0);
        var emptyConstructor = constructors.Single(x => x.ParameterList.Parameters.Count == 0);

        Assert.That(injectingConstructor.ParameterList.Parameters.Select(x => x.Identifier.Text),
            Is.EqualTo(new[] { "languageService", "otherService", "otherService1" }));
        Assert.That(injectingConstructor.Initializer!.ToString(), Is.EqualTo(": base(languageService, otherService)"));
        var assignedField = fieldName == "languageService" ? "this.languageService" : fieldName;
        Assert.That(injectingConstructor.Body!.Statements.Select(x => x.ToString()),
            Does.Contain($"{assignedField} = languageService;").And.Contain("_secondService = languageService;")
                .And.Contain("_otherService = otherService1;"));
        Assert.That(emptyConstructor.Initializer!.ArgumentList.Arguments.Select(x => x.ToString()),
            Is.EqualTo(new[] { "GetService<MyNamespace.ILanguageService<int>>()", "GetService<string>()", "GetService<MyNamespace.IOtherService>()" }));
    }

    [TestCase("Behavior")]
    [TestCase("MarkupExtension")]
    [TestCase("GenerateEmptyConstructor")]
    [TestCase("ValueConverter")]
    public void Preserves_Optional_Base_Parameter_Default(string generatorKind)
    {
        var rootType = generatorKind == "Behavior" ? "Microsoft.Xaml.Behaviors.Behavior"
            : generatorKind == "MarkupExtension" ? "System.Windows.Markup.MarkupExtension"
            : generatorKind == "ValueConverter" ? "System.Windows.Data.IValueConverter" : "object";
        var attribute = generatorKind == "GenerateEmptyConstructor" ? "[Catel.GenerateEmptyConstructor]" : string.Empty;
        var source = $@"
namespace MyNamespace
{{
    public abstract class BaseClass : {rootType}
    {{
        protected BaseClass(int count = 42) {{ }}
    }}
    {attribute}
    public partial class MyClass : BaseClass
    {{
        [Catel.InjectedService] private readonly string _service;
    }}
}}";
        IIncrementalGenerator generator = generatorKind == "Behavior" ? new BehaviorConstructorsSourceGenerator()
            : generatorKind == "MarkupExtension" ? new MarkupExtensionConstructorsSourceGenerator()
            : generatorKind == "ValueConverter" ? new ValueConverterConstructorsSourceGenerator()
            : new GenerateEmptyConstructorSourceGenerator();

        var output = Generate(source, generator);
        var constructor = output.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>()
            .Single(x => x.ParameterList.Parameters.Count == 0);

        Assert.That(constructor.Initializer!.ArgumentList.Arguments.Select(x => x.ToString()),
            Is.EqualTo(new[] { "(int)42", "GetService<string>()" }));
    }

    [TestCase("Behavior")]
    [TestCase("MarkupExtension")]
    [TestCase("GenerateEmptyConstructor")]
    [TestCase("ValueConverter")]
    public void Preserves_Parameterless_Base_Constructor(string generatorKind)
    {
        var rootType = generatorKind == "Behavior" ? "Microsoft.Xaml.Behaviors.Behavior"
            : generatorKind == "MarkupExtension" ? "System.Windows.Markup.MarkupExtension"
            : generatorKind == "ValueConverter" ? "System.Windows.Data.IValueConverter" : "object";
        var attribute = generatorKind == "GenerateEmptyConstructor" ? "[Catel.GenerateEmptyConstructor]" : string.Empty;
        var source = $@"
namespace MyNamespace
{{
    public abstract class BaseClass : {rootType}
    {{
        protected BaseClass() {{ }}
        private BaseClass(string service) {{ }}
    }}
    {attribute}
    public partial class MyClass : BaseClass
    {{
        [Catel.InjectedService] private readonly string _service;
    }}
}}";
        IIncrementalGenerator generator = generatorKind == "Behavior" ? new BehaviorConstructorsSourceGenerator()
            : generatorKind == "MarkupExtension" ? new MarkupExtensionConstructorsSourceGenerator()
            : generatorKind == "ValueConverter" ? new ValueConverterConstructorsSourceGenerator()
            : new GenerateEmptyConstructorSourceGenerator();

        var output = Generate(source, generator);
        var constructor = output.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>()
            .Single(x => x.ParameterList.Parameters.Count > 0);

        Assert.That(constructor.Initializer, Is.Null);
        Assert.That(constructor.ParameterList.Parameters.Select(x => x.Identifier.Text), Is.EqualTo(new[] { "service" }));
    }

    [TestCase("_serviceProvider")]
    [TestCase("_provider")]
    public void Reuses_ViewModel_ServiceProvider(string fieldName)
    {
        var output = Generate($@"
namespace MyNamespace
{{
    public partial class MyClass : Catel.MVVM.ViewModelBase
    {{
        [Catel.InjectedService] private readonly System.IServiceProvider {fieldName};
    }}
}}", new ViewModelConstructorsSourceGenerator());
        var constructor = output.GetRoot().DescendantNodes().OfType<ConstructorDeclarationSyntax>().Single();

        Assert.That(constructor.ParameterList.Parameters.Select(x => x.Identifier.Text),
            Is.EqualTo(new[] { "serviceProvider" }));
        Assert.That(constructor.Body!.Statements.Select(x => x.ToString()),
            Does.Contain($"{fieldName} = serviceProvider;"));
    }

    private static SyntaxTree Generate(string source, IIncrementalGenerator generator)
    {
        var stubs = @"
namespace Catel
{
    public static class CatelEnvironment { public static bool IsInDesignMode => false; }
    public sealed class InjectedServiceAttribute : System.Attribute { }
    public sealed class GenerateEmptyConstructorAttribute : System.Attribute { }
}
namespace Microsoft.Xaml.Behaviors { public abstract class Behavior { } }
namespace System.Windows.Markup { public abstract class MarkupExtension { } }
namespace System.Windows.Data
{
    public interface IValueConverter { }
    public interface IMultiValueConverter { }
}
namespace Catel.MVVM
{
    public abstract class ViewModelBase
    {
        protected ViewModelBase(System.IServiceProvider serviceProvider) { }
    }
}
namespace Catel.IoC
{
    public static class IoCContainer
    {
        public static System.IServiceProvider ServiceProvider { get; set; }
    }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceProviderServiceExtensions
    {
        public static T GetRequiredService<T>(this System.IServiceProvider provider) => default(T);
    }
}";
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator).Select(x => MetadataReference.CreateFromFile(x));
        var compilation = CSharpCompilation.Create("Tests",
            new[] { CSharpSyntaxTree.ParseText(source), CSharpSyntaxTree.ParseText(stubs) },
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

        Assert.That(diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error), Is.Empty);
        Assert.That(output.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error), Is.Empty);
        return driver.GetRunResult().GeneratedTrees.Single();
    }
}
