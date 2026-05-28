namespace Catel.SourceGenerators.Tests.XamlConstructors;

using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Linq;

[TestFixture]
public partial class ViewModelCtorGeneratorTests
{
    [Test]
    public void Generates_Unique_FileNames_For_ViewModels_With_Same_Class_Name_In_Different_Namespaces()
    {
        var driver = BuildDuplicateClassNameAcrossNamespacesDriver();

        var runResult = driver.GetRunResult();
        var sources = runResult.Results[0].GeneratedSources;

        Assert.That(sources.Length, Is.EqualTo(2), "Expected two generated sources, one per namespace.");

        var hintNames = sources.Select(s => s.HintName).ToArray();
        Assert.That(hintNames, Is.Unique, "Each generated file must have a unique hint name.");
        Assert.That(hintNames, Does.Contain("MyNamespace1_WelcomeWizardPage_ViewModelConstructors.g.cs"));
        Assert.That(hintNames, Does.Contain("MyNamespace2_WelcomeWizardPage_ViewModelConstructors.g.cs"));
    }

    private GeneratorDriver BuildDuplicateClassNameAcrossNamespacesDriver()
    {
        var viewModelSource = @"
namespace MyNamespace1
{
    public partial class WelcomeWizardPage : Catel.MVVM.ViewModelBase
    {
    }
}

namespace MyNamespace2
{
    public partial class WelcomeWizardPage : Catel.MVVM.ViewModelBase
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
}
