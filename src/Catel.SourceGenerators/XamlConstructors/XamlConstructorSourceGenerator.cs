namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Text;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System.Diagnostics;

    [Generator]
    public class XamlConstructorSourceGenerator : IIncrementalGenerator
    {
        //private bool _isIoCContainerAvailable = false;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var syntax = context.SyntaxProvider;
            //syntax.

            var compilation = context.CompilationProvider;

            //// Check for Catel.IoC.IoCContainer
            //var iocContainerType = compilation.GetTypeByMetadataName("Catel.IoC.IoCContainer");
            //if (iocContainerType is null)
            //{
            //    return;
            //}

            var constructorsToGenerate = syntax.CreateSyntaxProvider<XamlConstructorInfo?>(
                predicate: static (s, _) =>
                {
                    return IsSyntaxTargetForGeneration(s);
                },
                transform: static (ctx, _) =>
                {
                    return Transform(ctx);
                });

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

        private static XamlConstructorInfo? Transform(GeneratorSyntaxContext context)
        {
            var semanticModel = context.SemanticModel;
            var classDeclarationSyntax = context.Node as ClassDeclarationSyntax;
            if (classDeclarationSyntax is null)
            {
                return null;
            }

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
            if (classSymbol is null)
            {
                return null;
            }

            var baseType = classSymbol.BaseType;
            while (baseType is not null)
            {
                if (baseType.ToDisplayString() == "System.Windows.Controls.UserControl")
                {
                    break;
                }

                baseType = baseType.BaseType;
            }

            if (baseType is null)
            {
                return null;
            }

            var emptyClassConstructor = classSymbol.Constructors.FirstOrDefault(x => x.Parameters.Length == 0);
            if (emptyClassConstructor is not null)
            {
                // Has parameterless ctor already
                return null;
            }

            // Note: instead of using the *class* to get the ctors, we use the node. This is important since
            // a partial class may be defined in multiple nodes, and we want to get the ctor defined in this specific node.

            var constructors = classDeclarationSyntax.Members
                .Where(x => x is ConstructorDeclarationSyntax ctor)
                .Select(x => (ConstructorDeclarationSyntax)x)
                .Where(x => x.ParameterList.ChildNodes().Any())
                .ToArray();
            if (constructors.Length == 0 || constructors.Length > 1)
            {
                return null;
            }

            var classConstructor = classSymbol.Constructors.Where(c => c.Parameters.Length > 0).Single();

            var info = new XamlConstructorInfo(
                classDeclarationSyntax.SyntaxTree.FilePath,
                classSymbol.ContainingNamespace.ToDisplayString(), classSymbol.Name,
                classConstructor.Parameters.Select(x => x.Type.ToDisplayString()).ToArray());
            return info;
        }

        private static void Execute(SourceProductionContext sourceProductionContext, XamlConstructorInfo? xamlConstructorInfo)
        {
            if (xamlConstructorInfo is null)
            {
                return;
            }

            var ctorInfo = xamlConstructorInfo.Value;

            var sourceBuilder = new StringBuilder();
            sourceBuilder.AppendLine("using System;");
            sourceBuilder.AppendLine("using System.Runtime.CompilerServices;");
            sourceBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sourceBuilder.AppendLine("using Catel.IoC;");
            sourceBuilder.AppendLine();

            sourceBuilder.AppendLine($"namespace {ctorInfo.NamespaceName}");
            sourceBuilder.AppendLine("{");
            sourceBuilder.AppendLine($"    partial class {ctorInfo.ClassName}");
            sourceBuilder.AppendLine("    {");

            // Generate empty constructor
            sourceBuilder.AppendLine("        [CompilerGenerated]");
            sourceBuilder.AppendLine($"        public {ctorInfo.ClassName}()");
            sourceBuilder.Append("            : this(");
            sourceBuilder.Append(string.Join(", ", ctorInfo.ParameterTypeNames.Select(p =>
                $"IoCContainer.ServiceProvider.GetRequiredService<{p}>()")));
            sourceBuilder.AppendLine(")");
            sourceBuilder.AppendLine("        {");
            sourceBuilder.AppendLine("        }");

            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");

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

            sourceProductionContext.AddSource($"{fileName}_XamlConstructors.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}
