namespace Catel.SourceGenerators.Tests.PartialClasses;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

[TestFixture]
public class ClassShouldBePartialAnalyzerTests
{
    private static readonly MetadataReference[] MetadataReferences = AppDomain.CurrentDomain.GetAssemblies()
        .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
        .Select(assembly => assembly.Location)
        .Distinct()
        .Select(location => MetadataReference.CreateFromFile(location))
        .ToArray();

    [TestCase("public class MyViewModel : Catel.MVVM.ViewModelBase { }", "MyViewModel", "view model")]
    [TestCase("public class MyBehavior : Microsoft.Xaml.Behaviors.Behavior { }", "MyBehavior", "behavior")]
    [TestCase("public class MyMarkupExtension : System.Windows.Markup.MarkupExtension { }", "MyMarkupExtension", "markup extension")]
    [TestCase("public class MyUserControl : Catel.Windows.Controls.UserControl { }", "MyUserControl", "view")]
    [TestCase("public class MyWindow : Catel.Windows.Window { }", "MyWindow", "view")]
    public async Task Reports_Diagnostic_For_Supported_Class_Without_Partial(string classSource, string className, string supportedTypeDescription)
    {
        var diagnostics = await GetDiagnosticsAsync(WrapInSource(classSource));

        Assert.That(diagnostics, Has.Length.EqualTo(1));

        var diagnostic = diagnostics[0];

        Assert.Multiple(() =>
        {
            Assert.That(diagnostic.Id, Is.EqualTo("CTLSG002"));
            Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Info));
            Assert.That(diagnostic.GetMessage(), Is.EqualTo($"Make {supportedTypeDescription} '{className}' partial so Catel can generate constructors"));
            Assert.That(diagnostic.Location.SourceSpan.Length, Is.EqualTo(className.Length));
            Assert.That(WrapInSource(classSource).Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length), Is.EqualTo(className));
        });
    }

    [Test]
    public async Task Does_Not_Report_Diagnostic_For_Partial_Supported_Class()
    {
        var diagnostics = await GetDiagnosticsAsync(WrapInSource(
            "public partial class MyViewModel : Catel.MVVM.ViewModelBase { }"));

        Assert.That(diagnostics, Is.Empty);
    }

    [Test]
    public async Task Does_Not_Report_Diagnostic_For_Unsupported_Class()
    {
        var diagnostics = await GetDiagnosticsAsync(WrapInSource(
            "public class MyClass { }"));

        Assert.That(diagnostics, Is.Empty);
    }

    [TestCase(
        "public class MyViewModel : Catel.MVVM.ViewModelBase { }",
        "public partial class MyViewModel : Catel.MVVM.ViewModelBase { }")]
    [TestCase(
        "public sealed class MyBehavior : Microsoft.Xaml.Behaviors.Behavior { }",
        "public sealed partial class MyBehavior : Microsoft.Xaml.Behaviors.Behavior { }")]
    [TestCase(
        "internal class MyMarkupExtension : System.Windows.Markup.MarkupExtension { }",
        "internal partial class MyMarkupExtension : System.Windows.Markup.MarkupExtension { }")]
    [TestCase(
        "public class MyUserControl : Catel.Windows.Controls.UserControl { }",
        "public partial class MyUserControl : Catel.Windows.Controls.UserControl { }")]
    public async Task Applies_Code_Fix_To_Add_Partial_Modifier(string originalClassSource, string expectedClassSource)
    {
        var updatedSource = await ApplyCodeFixAsync(WrapInSource(originalClassSource));

        Assert.That(updatedSource, Is.EqualTo(NormalizeLineEndings(WrapInSource(expectedClassSource))));
    }

    [Test]
    public void Uses_Batch_Fixer_For_Fix_All()
    {
        var codeFixProvider = new ClassShouldBePartialCodeFixProvider();

        Assert.That(codeFixProvider.GetFixAllProvider(), Is.SameAs(WellKnownFixAllProviders.BatchFixer));
    }

    private static async Task<Diagnostic[]> GetDiagnosticsAsync(string source)
    {
        using var workspace = new AdhocWorkspace();
        var document = CreateDocument(workspace, source);
        var compilation = await document.Project.GetCompilationAsync();
        Assert.That(compilation, Is.Not.Null);

        var analyzer = new ClassShouldBePartialAnalyzer();
        var diagnostics = await compilation!
            .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer))
            .GetAnalyzerDiagnosticsAsync();

        return diagnostics
            .OrderBy(x => x.Location.SourceSpan.Start)
            .ToArray();
    }

    private static async Task<string> ApplyCodeFixAsync(string source)
    {
        using var workspace = new AdhocWorkspace();
        var document = CreateDocument(workspace, source);
        var diagnostics = await GetDiagnosticsAsync(source);
        Assert.That(diagnostics, Has.Length.EqualTo(1));

        var codeFixProvider = new ClassShouldBePartialCodeFixProvider();
        var codeActions = new List<CodeAction>();

        var context = new CodeFixContext(
            document,
            diagnostics[0],
            (action, _) => codeActions.Add(action),
            CancellationToken.None);

        await codeFixProvider.RegisterCodeFixesAsync(context);

        Assert.That(codeActions, Has.Count.EqualTo(1));

        var operations = await codeActions[0].GetOperationsAsync(CancellationToken.None);
        var applyChangesOperation = operations.OfType<ApplyChangesOperation>().Single();
        var updatedDocument = applyChangesOperation.ChangedSolution.GetDocument(document.Id);
        var updatedText = await updatedDocument!.GetTextAsync();

        return NormalizeLineEndings(updatedText.ToString());
    }

    private static Document CreateDocument(AdhocWorkspace workspace, string source)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var solution = workspace.CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .WithProjectParseOptions(projectId, new CSharpParseOptions(LanguageVersion.Latest))
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (var metadataReference in MetadataReferences)
        {
            solution = solution.AddMetadataReference(projectId, metadataReference);
        }

        solution = solution.AddDocument(documentId, "Test.cs", SourceText.From(source));

        return solution.GetDocument(documentId)!;
    }

    private static string WrapInSource(string classSource)
    {
        return NormalizeLineEndings($$"""
namespace Catel.MVVM
{
    public class ViewModelBase
    {
    }
}

namespace Microsoft.Xaml.Behaviors
{
    public class Behavior
    {
    }
}

namespace System.Windows.Markup
{
    public abstract class MarkupExtension
    {
    }
}

namespace System.Windows.Controls
{
    public class Control
    {
    }

    public class UserControl : Control
    {
    }
}

namespace System.Windows
{
    public class Window
    {
    }
}

namespace Catel.Windows.Controls
{
    public class UserControl : System.Windows.Controls.UserControl
    {
    }
}

namespace Catel.Windows
{
    public class DataWindow : System.Windows.Window
    {
    }

    public class Window : System.Windows.Window
    {
    }
}

namespace TestNamespace
{
    {{classSource}}
}
""");
    }

    private static string NormalizeLineEndings(string value)
    {
        return value.Replace("\r\n", "\n");
    }
}
