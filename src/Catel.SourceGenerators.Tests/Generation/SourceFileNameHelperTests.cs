namespace Catel.SourceGenerators.Tests.Generation;

using Catel.SourceGenerators.Generation;
using NUnit.Framework;

[TestFixture]
public class SourceFileNameHelperTests
{
    [Test]
    public void Returns_Namespace_And_Class_When_Short_Enough()
    {
        var result = SourceFileNameHelper.GetGeneratedFileName("MyNamespace", "MyClass");

        Assert.That(result, Is.EqualTo("MyNamespace_MyClass"));
    }

    [Test]
    public void Replaces_Dots_With_Underscores_In_Namespace()
    {
        var result = SourceFileNameHelper.GetGeneratedFileName("My.Deep.Namespace", "MyClass");

        Assert.That(result, Is.EqualTo("My_Deep_Namespace_MyClass"));
    }

    [Test]
    public void Returns_Hash_Prefix_When_Combined_Name_Exceeds_Max_Length()
    {
        var longNamespace = new string('A', 95) + ".Sub";
        var result = SourceFileNameHelper.GetGeneratedFileName(longNamespace, "MyClass");

        // Should not use the full namespace when combined name > 100 chars
        Assert.That(result, Does.Not.StartWith(longNamespace.Replace('.', '_')));
        Assert.That(result, Does.EndWith("_MyClass"));
        // Hash prefix is 8 hex chars
        Assert.That(result.Length, Is.EqualTo(8 + 1 + "MyClass".Length));
    }

    [Test]
    public void Hash_Is_Deterministic_For_Same_Inputs()
    {
        var ns = "Very.Long.Namespace.That.Exceeds.The.Limit.For.File.Names.In.The.Source.Generator.Output.Files";
        var result1 = SourceFileNameHelper.GetGeneratedFileName(ns, "SomeClass");
        var result2 = SourceFileNameHelper.GetGeneratedFileName(ns, "SomeClass");

        Assert.That(result1, Is.EqualTo(result2));
    }

    [Test]
    public void Different_Namespaces_Produce_Different_File_Names_For_Same_Class()
    {
        var result1 = SourceFileNameHelper.GetGeneratedFileName("MyNamespace1", "WelcomeWizardPage");
        var result2 = SourceFileNameHelper.GetGeneratedFileName("MyNamespace2", "WelcomeWizardPage");

        Assert.That(result1, Is.Not.EqualTo(result2));
    }

    [Test]
    public void Long_Different_Namespaces_Produce_Different_File_Names_For_Same_Class()
    {
        var ns1 = "Company.Product.Feature.SubFeature.Level1.Level2.Level3.Part1";
        var ns2 = "Company.Product.Feature.SubFeature.Level1.Level2.Level3.Part2";
        var result1 = SourceFileNameHelper.GetGeneratedFileName(ns1, "WelcomeWizardPage");
        var result2 = SourceFileNameHelper.GetGeneratedFileName(ns2, "WelcomeWizardPage");

        Assert.That(result1, Is.Not.EqualTo(result2));
    }

    [Test]
    public void Result_At_Exactly_Max_Length_Uses_Full_Namespace()
    {
        // Construct a namespace so that namespace_class is exactly 100 chars
        // "A...A_MyClass" where namespace = 'A' * (100 - 1 - "MyClass".Length) = 'A' * 92
        var ns = new string('A', 92);
        var className = "MyClass";
        var result = SourceFileNameHelper.GetGeneratedFileName(ns, className);

        Assert.That(result.Length, Is.EqualTo(100));
        Assert.That(result, Is.EqualTo($"{ns}_{className}"));
    }
}
