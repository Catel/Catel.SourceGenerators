namespace Catel.SourceGenerators.XamlConstructors;

using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class ValueConverterConstructorsSourceGenerator : IIncrementalGenerator
{
    //private bool _isIoCContainerAvailable = false;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntax = context.SyntaxProvider;

        var compilation = context.CompilationProvider;

        //// Check for Catel.IoC.IoCContainer
        //var iocContainerType = compilation.GetTypeByMetadataName("Catel.IoC.IoCContainer");
        //if (iocContainerType is null)
        //{
        //    return;
        //}

        var constructorsToGenerate = syntax.CreateSyntaxProvider(
            predicate: static (s, _) =>
            {
                return IsSyntaxTargetForGeneration(s);
            },
            transform: static (ctx, _) =>
            {
                return Transform(ctx);
            })
            .Where(static m => m is not null);

        context.RegisterSourceOutput(constructorsToGenerate,
            static (spc, source) => Execute(spc, source));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return false;
        }

        return true;
    }

    private static ValueConverterConstructorInfo? Transform(GeneratorSyntaxContext context)
    {
        var semanticModel = context.SemanticModel;
        var classDeclarationSyntax = context.Node as ClassDeclarationSyntax;
        if (classDeclarationSyntax is null)
        {
            return null;
        }

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
        if (classSymbol is null ||
            classSymbol.IsAbstract)
        {
            return null;
        }

        if (!classSymbol.ImplementsInterface("System.Windows.Data.IValueConverter") &&
            !classSymbol.ImplementsInterface("System.Windows.Data.IMultiValueConverter"))
        {
            return null;
        }

        // Already handled by markup extension constructor source generator
        if (classSymbol.DerivesFromBaseClass("System.Windows.Markup.MarkupExtension"))
        {
            return null;
        }

        var emptyClassConstructor = classSymbol.Constructors.FirstOrDefault(x => !x.IsStatic && x.Parameters.Length == 0);
        if (emptyClassConstructor is not null)
        {
            // Has parameterless ctor already
            return null;
        }

        // Note: instead of using the *class* to get the ctors, we use the node. This is important since
        // a partial class may be defined in multiple nodes, and we want to get the ctor defined in this specific node.

        //if (!Debugger.IsAttached)
        //{
        //    Debugger.Launch();
        //}

        var constructors = classDeclarationSyntax.Members
            .Where(x => x is ConstructorDeclarationSyntax ctor)
            .Select(x => (ConstructorDeclarationSyntax)x)
            .Where(x => x.ParameterList.ChildNodes().Any())
            .ToArray();
        if (constructors.Length == 0 || constructors.Length > 1)
        {
            return null;
        }

        var classConstructor = classSymbol.InstanceConstructors
            .Where(c => c.Parameters.Length > 0)
            .FirstOrDefault();

        var info = new ValueConverterConstructorInfo(
            classDeclarationSyntax.SyntaxTree.FilePath,
            classSymbol.ContainingNamespace.ToDisplayString(), classSymbol.Name,
            classConstructor.Parameters.Select(x => x.Type.ToDisplayString()).ToArray());
        return info;
    }

    private static void Execute(SourceProductionContext sourceProductionContext, ValueConverterConstructorInfo? constructorInfo)
    {
        if (constructorInfo is null)
        {
            return;
        }

        var ctorInfo = constructorInfo.Value;

        var sourceBuilder = new IndentedStringBuilder();
        sourceBuilder.AppendLine("using System;");
        sourceBuilder.AppendLine("using System.Runtime.CompilerServices;");
        sourceBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sourceBuilder.AppendLine("using Catel.IoC;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("#nullable enable");
        sourceBuilder.AppendLine();

        sourceBuilder.AppendLine($"namespace {ctorInfo.NamespaceName}");
        sourceBuilder.StartBlock();
        sourceBuilder.AppendLine($"partial class {ctorInfo.ClassName}");
        sourceBuilder.StartBlock();

        sourceBuilder.AppendResolveServiceMethod("ValueConverterConstructors");

        // Generate empty constructor
        sourceBuilder.AppendGeneratedCodeAttribute("ValueConverterConstructors");
        sourceBuilder.AppendLine($"public {ctorInfo.ClassName}()");
        sourceBuilder.Append("    : this(");
        sourceBuilder.Append(string.Join(", ", ctorInfo.ParameterTypeNames.Select(p =>
            $"GetService<{p}>()")));
        sourceBuilder.AppendLine(")");
        sourceBuilder.StartBlock();
        sourceBuilder.EndBlock();

        sourceBuilder.EndBlock();
        sourceBuilder.EndBlock();

        //#if DEBUG
        //            if (!Debugger.IsAttached)
        //            {
        //                Debugger.Launch();
        //            }
        //#endif

        var fileName = ctorInfo.FileName;
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            fileName = fileName.Replace(".xaml.", ".");
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
        }
        else
        {
            fileName = ctorInfo.ClassName;
        }

        sourceProductionContext.AddSource($"{fileName}_BehaviorConstructors.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
}
