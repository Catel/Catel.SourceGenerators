namespace Catel.SourceGenerators.XamlConstructors;

using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class MarkupExtensionConstructorsSourceGenerator : IIncrementalGenerator
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

    private static MarkupExtensionConstructorInfo? Transform(GeneratorSyntaxContext context)
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

        if (!classDeclarationSyntax.IsPartialType())
        {
            return null;
        }

        if (!classSymbol.DerivesFromBaseClass("System.Windows.Markup.MarkupExtension"))
        {
            return null;
        }

        var injectedServices = InjectedServiceAttributeHelper.GetInjectedServiceFields(classSymbol);

        var emptyClassConstructor = classSymbol.Constructors.FirstOrDefault(x => !x.IsStatic && x.Parameters.Length == 0);
        if (emptyClassConstructor is not null && !emptyClassConstructor.IsImplicitlyDeclared)
        {
            if (injectedServices.Count > 0)
            {
                return new MarkupExtensionConstructorInfo(
                    classDeclarationSyntax.SyntaxTree.FilePath,
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    classSymbol.Name,
                    System.Array.Empty<string>(),
                    injectedServices,
                    hasConflictingConstructors: true);
            }

            return new MarkupExtensionConstructorInfo(
                classDeclarationSyntax.SyntaxTree.FilePath,
                classSymbol.ContainingNamespace.ToDisplayString(),
                classSymbol.Name,
                System.Array.Empty<string>(),
                System.Array.Empty<InjectedServiceInfo>(),
                generateGetServiceOnly: true);
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

        if (constructors.Length == 0)
        {
            if (injectedServices.Count == 0)
            {
                return new MarkupExtensionConstructorInfo(
                    classDeclarationSyntax.SyntaxTree.FilePath,
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    classSymbol.Name,
                    System.Array.Empty<string>(),
                    System.Array.Empty<InjectedServiceInfo>(),
                    generateGetServiceOnly: true);
            }

            // No user-written constructor but has [InjectedService] fields: generate from injected services
            return new MarkupExtensionConstructorInfo(
                classDeclarationSyntax.SyntaxTree.FilePath,
                classSymbol.ContainingNamespace.ToDisplayString(), classSymbol.Name,
                System.Array.Empty<string>(), injectedServices);
        }

        if (injectedServices.Count > 0)
        {
            // User has explicit constructors combined with [InjectedService] fields — report an error
            return new MarkupExtensionConstructorInfo(
                classDeclarationSyntax.SyntaxTree.FilePath,
                classSymbol.ContainingNamespace.ToDisplayString(),
                classSymbol.Name,
                System.Array.Empty<string>(),
                injectedServices,
                hasConflictingConstructors: true);
        }

        if (constructors.Length > 1)
        {
            return new MarkupExtensionConstructorInfo(
                classDeclarationSyntax.SyntaxTree.FilePath,
                classSymbol.ContainingNamespace.ToDisplayString(),
                classSymbol.Name,
                System.Array.Empty<string>(),
                System.Array.Empty<InjectedServiceInfo>(),
                generateGetServiceOnly: true);
        }

        var classConstructor = classSymbol.InstanceConstructors
            .Where(c => c.Parameters.Length > 0)
            .FirstOrDefault();

        var info = new MarkupExtensionConstructorInfo(
            classDeclarationSyntax.SyntaxTree.FilePath,
            classSymbol.ContainingNamespace.ToDisplayString(), classSymbol.Name,
            classConstructor.Parameters.Select(x => x.Type.ToDisplayString()).ToArray(),
            injectedServices);
        return info;
    }

    private static void Execute(SourceProductionContext sourceProductionContext, MarkupExtensionConstructorInfo? constructorInfo)
    {
        if (constructorInfo is null)
        {
            return;
        }

        var ctorInfo = constructorInfo.Value;

        if (ctorInfo.HasConflictingConstructors)
        {
            sourceProductionContext.ReportDiagnostic(
                Diagnostic.Create(Diagnostics.ConflictingConstructorsAndInjectedService, Location.None));
            return;
        }

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

        sourceBuilder.AppendResolveServiceMethod("MarkupExtensionConstructors");

        if (!ctorInfo.GenerateGetServiceOnly)
        {
            if (ctorInfo.ParameterTypeNames.Count == 0 && ctorInfo.InjectedServices.Count > 0)
            {
                // No user-written constructor: generate DI ctor + empty ctor from injected services
                sourceBuilder.AppendLine("partial void OnConstructing();");
                sourceBuilder.AppendLine();
                sourceBuilder.AppendLine("partial void OnConstructed();");
                sourceBuilder.AppendLine();

                sourceBuilder.AppendGeneratedCodeAttribute("MarkupExtensionConstructors");
                sourceBuilder.AppendLine($"public {ctorInfo.ClassName}({string.Join(", ", ctorInfo.InjectedServices.Select(s => $"{s.TypeName} {s.ParameterName}"))})");
                sourceBuilder.StartBlock();
                foreach (var service in ctorInfo.InjectedServices)
                {
                    sourceBuilder.AppendLine($"{service.FieldName} = {service.ParameterName};");
                }
                sourceBuilder.AppendLine("OnConstructing();");
                sourceBuilder.AppendLine("OnConstructed();");
                sourceBuilder.EndBlock();

                sourceBuilder.AppendLine();

                sourceBuilder.AppendGeneratedCodeAttribute("MarkupExtensionConstructors");
                sourceBuilder.AppendLine($"public {ctorInfo.ClassName}()");
                sourceBuilder.Append("    : this(");
                sourceBuilder.Append(string.Join(", ", ctorInfo.InjectedServices.Select(s =>
                    $"GetService<{s.TypeName}>()")));
                sourceBuilder.AppendLine(")");
                sourceBuilder.StartBlock();
                sourceBuilder.EndBlock();
            }
            else
            {
                // Generate empty constructor delegating to user-written ctor
                sourceBuilder.AppendLine("partial void OnConstructing();");
                sourceBuilder.AppendLine();
                sourceBuilder.AppendLine("partial void OnConstructed();");
                sourceBuilder.AppendLine();

                var allServiceCalls = ctorInfo.ParameterTypeNames.Select(p => $"GetService<{p}>()")
                    .Concat(ctorInfo.InjectedServices.Select(s => $"GetService<{s.TypeName}>()"));

                sourceBuilder.AppendGeneratedCodeAttribute("MarkupExtensionConstructors");
                sourceBuilder.AppendLine($"public {ctorInfo.ClassName}()");
                sourceBuilder.Append("    : this(");
                sourceBuilder.Append(string.Join(", ", allServiceCalls));
                sourceBuilder.AppendLine(")");
                sourceBuilder.StartBlock();
                sourceBuilder.EndBlock();
            }
        }

        sourceBuilder.EndBlock();
        sourceBuilder.EndBlock();

        //#if DEBUG
        //            if (!Debugger.IsAttached)
        //            {
        //                Debugger.Launch();
        //            }
        //#endif

        var fileName = Generation.SourceFileNameHelper.GetGeneratedFileName(ctorInfo.NamespaceName, ctorInfo.ClassName);

        sourceProductionContext.AddSource($"{fileName}_MarkupExtensionConstructors.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
}
