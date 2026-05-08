namespace Catel.SourceGenerators;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassShouldBePartialCodeFixProvider)), Shared]
public class ClassShouldBePartialCodeFixProvider : CodeFixProvider
{
    private const string Title = "Make class partial";

    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(Diagnostics.ClassShouldBePartial.Id);

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var classDeclarationSyntax = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDeclarationSyntax is null || classDeclarationSyntax.IsPartialType())
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                Title,
                cancellationToken => AddPartialModifierAsync(context.Document, classDeclarationSyntax, cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> AddPartialModifierAsync(Document document,
        ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword)
            .WithTrailingTrivia(SyntaxFactory.Space);

        var newClassDeclarationSyntax = classDeclarationSyntax.WithModifiers(
            classDeclarationSyntax.Modifiers.Insert(GetPartialInsertionIndex(classDeclarationSyntax.Modifiers), partialToken));

        return document.WithSyntaxRoot(root.ReplaceNode(classDeclarationSyntax, newClassDeclarationSyntax));
    }

    private static int GetPartialInsertionIndex(SyntaxTokenList modifiers)
    {
        return modifiers.Count;
    }
}
