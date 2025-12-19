namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Text;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

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

            var constructors = classSymbol.Constructors.Where(c => c.Parameters.Length > 0).ToArray();
            if (constructors.Length == 0 || constructors.Length > 1)
            {
                return null;
            }

            var info = new XamlConstructorInfo(classSymbol.ContainingNamespace.Name, classSymbol.Name,
                constructors[0].Parameters.Select(x => x.Type.ToDisplayString()).ToArray());
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
                $"IoCContainer.Provider.GetRequiredService<{p}>()")));
            sourceBuilder.AppendLine(")");
            sourceBuilder.AppendLine("        {");
            sourceBuilder.AppendLine("        }");

            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");

            sourceProductionContext.AddSource($"{ctorInfo.ClassName}_XamlConstructors.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}
