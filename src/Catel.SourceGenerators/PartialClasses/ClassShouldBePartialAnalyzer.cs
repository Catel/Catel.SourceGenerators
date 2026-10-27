namespace Catel.SourceGenerators;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS1038 // Compiler extensions should be implemented in assemblies with compiler-provided references
[DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1038 // Compiler extensions should be implemented in assemblies with compiler-provided references
public class ClassShouldBePartialAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Diagnostics.ClassShouldBePartial);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        if (classDeclarationSyntax.IsPartialType())
        {
            return;
        }

        var supportedTypeDescription = PartialClassCandidateDetector.GetSupportedTypeDescription(context.SemanticModel, classDeclarationSyntax);
        if (supportedTypeDescription is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.ClassShouldBePartial,
            classDeclarationSyntax.Identifier.GetLocation(),
            classDeclarationSyntax.Identifier.Text,
            supportedTypeDescription));
    }
}
