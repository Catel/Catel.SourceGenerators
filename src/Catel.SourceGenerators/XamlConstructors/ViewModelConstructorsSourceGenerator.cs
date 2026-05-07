namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [Generator]
    public class ViewModelConstructorsSourceGenerator : IIncrementalGenerator
    {
        private const string ViewModelBaseTypeName = "Catel.MVVM.ViewModelBase";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var syntax = context.SyntaxProvider;

            var constructorsToGenerate = syntax.CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => Transform(ctx))
                .Where(static m => m is not null);

            var collected = constructorsToGenerate.Collect()
                .Select((infos, _) => infos.Distinct().ToImmutableArray());

            context.RegisterSourceOutput(collected,
                static (spc, sources) =>
                {
                    foreach (var source in sources)
                    {
                        Execute(spc, source);
                    }
                });
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax;
        }

        private static ViewModelConstructorInfo? Transform(GeneratorSyntaxContext context)
        {
            var semanticModel = context.SemanticModel;

            if (semanticModel.IsCatelAssembly())
            {
                return null;
            }

            var classDeclarationSyntax = context.Node as ClassDeclarationSyntax;
            if (classDeclarationSyntax is null)
            {
                return null;
            }

            if (!classDeclarationSyntax.IsPartialType())
            {
                return null;
            }

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
            if (classSymbol is null || classSymbol.IsAbstract)
            {
                return null;
            }

            if (!classSymbol.DerivesFromBaseClass(ViewModelBaseTypeName))
            {
                return null;
            }

            var injectedServices = InjectedServiceAttributeHelper.GetInjectedServiceFields(classSymbol);
            var injectedModel = InjectedModelAttributeHelper.GetInjectedModelMember(classSymbol);

            if (injectedServices.Count == 0 && injectedModel is null)
            {
                return null;
            }

            // Check for conflicting explicit constructors
            var hasExplicitInstanceCtors = classSymbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared);
            if (hasExplicitInstanceCtors && injectedServices.Count > 0)
            {
                return new ViewModelConstructorInfo(
                    classDeclarationSyntax.SyntaxTree.FilePath,
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    classSymbol.Name,
                    System.Array.Empty<InjectedServiceInfo>(),
                    injectedModel: null,
                    hasConflictingConstructors: true);
            }

            return new ViewModelConstructorInfo(
                classDeclarationSyntax.SyntaxTree.FilePath,
                classSymbol.ContainingNamespace.ToDisplayString(),
                classSymbol.Name,
                injectedServices,
                injectedModel);
        }

        private static void Execute(SourceProductionContext sourceProductionContext, ViewModelConstructorInfo? constructorInfo)
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
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("#nullable enable");
            sourceBuilder.AppendLine();

            sourceBuilder.AppendLine($"namespace {ctorInfo.NamespaceName}");
            sourceBuilder.StartBlock();
            sourceBuilder.AppendLine($"partial class {ctorInfo.ClassName}");
            sourceBuilder.StartBlock();

            sourceBuilder.AppendLine("partial void OnConstructing();");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("partial void OnConstructed();");
            sourceBuilder.AppendLine();

            var hasModel = ctorInfo.HasInjectedModel;
            var model = ctorInfo.InjectedModel;

            if (hasModel && model.IsNullable)
            {
                // Generate 2 constructors: one with model, one without
                GenerateViewModelConstructor(sourceBuilder, ctorInfo, includeModel: true);
                sourceBuilder.AppendLine();
                GenerateViewModelConstructor(sourceBuilder, ctorInfo, includeModel: false);
            }
            else
            {
                // Generate 1 constructor
                GenerateViewModelConstructor(sourceBuilder, ctorInfo, includeModel: hasModel);
            }

            sourceBuilder.EndBlock();
            sourceBuilder.EndBlock();

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

            sourceProductionContext.AddSource($"{fileName}_ViewModelConstructors.g.cs",
                SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private static void GenerateViewModelConstructor(IndentedStringBuilder sourceBuilder,
            ViewModelConstructorInfo ctorInfo, bool includeModel)
        {
            // Build parameter list: [model,] IServiceProvider serviceProvider, [services...]
            var parameters = new System.Collections.Generic.List<string>();

            if (includeModel)
            {
                parameters.Add($"{ctorInfo.InjectedModel.TypeName} {ctorInfo.InjectedModel.ParameterName}");
            }

            parameters.Add("System.IServiceProvider serviceProvider");

            foreach (var service in ctorInfo.InjectedServices)
            {
                parameters.Add($"{service.TypeName} {service.ParameterName}");
            }

            sourceBuilder.AppendGeneratedCodeAttribute("ViewModelConstructors");
            sourceBuilder.AppendLine($"public {ctorInfo.ClassName}({string.Join(", ", parameters)})");
            sourceBuilder.AppendLine("    : base(serviceProvider)");
            sourceBuilder.StartBlock();

            sourceBuilder.AppendLine("OnConstructing();");

            if (includeModel)
            {
                sourceBuilder.AppendLine($"{ctorInfo.InjectedModel.MemberName} = {ctorInfo.InjectedModel.ParameterName};");
            }

            foreach (var service in ctorInfo.InjectedServices)
            {
                sourceBuilder.AppendLine($"{service.FieldName} = {service.ParameterName};");
            }

            sourceBuilder.AppendLine("OnConstructed();");

            sourceBuilder.EndBlock();
        }
    }
}
