namespace Catel.SourceGenerators.Tests.XamlConstructors;

using System.Linq;
using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

public partial class ValueConvCtorGeneratorTests
{
    [TestCase("public MyConverter() { }")]
    [TestCase("public MyConverter(string service) { }")]
    public void Reports_Error_When_Converter_Has_Explicit_Constructor_And_Injected_Service(string constructor)
    {
        var source = $@"
namespace System.Windows.Data {{ public interface IValueConverter {{ }} }}
namespace Catel {{ public sealed class InjectedServiceAttribute : System.Attribute {{ }} }}
namespace MyNamespace
{{
    public partial class MyConverter : System.Windows.Data.IValueConverter
    {{
        [Catel.InjectedService] private readonly string _service;
        {constructor}
    }}
}}";
        var compilation = CSharpCompilation.Create("Tests", new[] { CSharpSyntaxTree.ParseText(source) });
        var driver = CSharpGeneratorDriver.Create(new ValueConverterConstructorsSourceGenerator()).RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.That(result.Diagnostics.Select(x => x.Id), Is.EqualTo(new[] { "CTLSG001" }));
        Assert.That(result.GeneratedTrees, Is.Empty);
    }

    [Test]
    public void Generates_Service_Constructors_Once_For_Generic_Partial_Converter()
    {
        var source = @"
namespace System.Windows.Data { public interface IValueConverter { } }
namespace Catel { public sealed class InjectedServiceAttribute : System.Attribute { } }
namespace MyNamespace
{
    public partial class MyConverter<T> : System.Windows.Data.IValueConverter
    {
        [Catel.InjectedService] private readonly string _service;
    }
    public partial class MyConverter<T> { }
}";
        var compilation = CSharpCompilation.Create("Tests", new[] { CSharpSyntaxTree.ParseText(source) });
        var driver = CSharpGeneratorDriver.Create(new ValueConverterConstructorsSourceGenerator()).RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.That(result.Diagnostics, Is.Empty);
        var generatedClass = result.GeneratedTrees.Single().GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Single();
        Assert.That(generatedClass.TypeParameterList!.Parameters.Single().Identifier.Text, Is.EqualTo("T"));
        Assert.That(generatedClass.Members.OfType<ConstructorDeclarationSyntax>().Count(), Is.EqualTo(2));
    }

    [Test]
    public void Leaves_Injected_MarkupExtension_Converters_To_MarkupExtension_Generator()
    {
        var source = @"
namespace System.Windows.Data { public interface IValueConverter { } }
namespace System.Windows.Markup { public abstract class MarkupExtension { } }
namespace Catel { public sealed class InjectedServiceAttribute : System.Attribute { } }
namespace MyNamespace
{
    public partial class MyConverter : System.Windows.Markup.MarkupExtension, System.Windows.Data.IValueConverter
    {
        [Catel.InjectedService] private readonly string _service;
    }
}";
        var compilation = CSharpCompilation.Create("Tests", new[] { CSharpSyntaxTree.ParseText(source) });
        var driver = CSharpGeneratorDriver.Create(
            new ValueConverterConstructorsSourceGenerator().AsSourceGenerator(),
            new MarkupExtensionConstructorsSourceGenerator().AsSourceGenerator()).RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.That(result.Diagnostics, Is.Empty);
        Assert.That(result.Results[0].GeneratedSources, Is.Empty);
        var constructors = result.Results[1].GeneratedSources.Single().SyntaxTree.GetRoot()
            .DescendantNodes().OfType<ConstructorDeclarationSyntax>();
        Assert.That(constructors.Count(), Is.EqualTo(2));
    }
}
