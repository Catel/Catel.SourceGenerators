namespace Catel.SourceGenerators.XamlConstructors
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [Generator]
    public class UserControlConstructorsSourceGenerator : IIncrementalGenerator
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

            // Collect all values, then deduplicate
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
            if (node is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return false;
            }

            return true;
        }

        private static UserControlConstructorsInfo? Transform(GeneratorSyntaxContext context)
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
            if (classSymbol is null)
            {
                return null;
            }

            if (classSymbol.IsAbstract)
            {
                return null;
            }

            var isCatelView = false;
            var baseType = classSymbol.BaseType;
            while (baseType is not null)
            {
                var displayString = baseType.ToDisplayString();
                if (!isCatelView)
                {
                    if (displayString.Contains("Catel.Windows.Controls.UserControl") ||
                        displayString.Contains("Catel.Windows.DataWindow"))
                    {
                        isCatelView = true;
                    }
                }

                if (displayString == "System.Windows.Controls.Control" ||
                    displayString == "System.Windows.Controls.UserControl" ||
                    displayString == "System.Windows.Window")
                {
                    break;
                }

                baseType = baseType.BaseType;
            }

            if (baseType is null)
            {
                return null;
            }

            var emptyClassConstructor = classSymbol.InstanceConstructors.FirstOrDefault(x => x.Parameters.Length == 0);
            if (emptyClassConstructor is not null)
            {
                // Has parameterless ctor already, but could be implicit

                if (emptyClassConstructor.IsImplicitlyDeclared)
                {
                    // Generate 2 ctors: parameterless and DI, check base
                    var baseClassConstructors = classSymbol.BaseType!.GetMembers()
                        .Where(x => x is IMethodSymbol ctor)
                        .Select(x => (IMethodSymbol)x)
                        .OrderBy(x => x.Parameters.Length)
                        .Where(x => x.Parameters.Any())
                        .ToArray();

                    var ctors = new List<ConstructorInfo>();

                    foreach (var baseCtor in baseClassConstructors)
                    {
                        if (baseCtor.Name != ".ctor")
                        {
                            // Not a ctor
                            continue;
                        }

                        var isViewModelInjectionCtor = baseCtor.Parameters[0].Type.ImplementsInterface("Catel.MVVM.IViewModel");

                        ctors.Add(new ConstructorInfo(classSymbol.Name,
                            baseCtor.Parameters.Select(x => new ParameterInfo(x.Name, x.Type.ToDisplayString(), x.IsNullable())).ToArray(),
                            true,
                            !isViewModelInjectionCtor));

                        // Only generate empty ctor for non-view model injection ctor
                        if (!isViewModelInjectionCtor)
                        {
                            ctors.Add(new ConstructorInfo(classSymbol.Name,
                                baseCtor.Parameters.Select(x => new ParameterInfo(x.Name, x.Type.ToDisplayString(), x.IsNullable())).ToArray(),
                                false, false));
                        }
                    }

                    return new UserControlConstructorsInfo(
                        classDeclarationSyntax.SyntaxTree.FilePath,
                        classSymbol.ContainingNamespace.ToDisplayString(),
                        classSymbol.Name,
                        isCatelView && !classSymbol.HasStaticConstructorWithContent(),
                        ctors);
                }

                return null;
            }

            // TODO: Figure out how to filter the right constructor with the view model

            var classConstructors = classSymbol.InstanceConstructors
                .Where(c => c.Parameters.Length > 0)
                .Where(c => !c.Parameters[0].Type.ImplementsInterface("Catel.MVVM.IViewModel"))
                .Select(x => new ConstructorInfo(classSymbol.Name,
                        x.Parameters.Select(x => new ParameterInfo(x.Name, x.Type.ToDisplayString(), x.IsNullable())).ToArray(),
                        false, false))
                .ToArray();

            var info = new UserControlConstructorsInfo(
                classDeclarationSyntax.SyntaxTree.FilePath,
                classSymbol.ContainingNamespace.ToDisplayString(),
                classSymbol.Name,
                isCatelView && !classSymbol.HasStaticConstructorWithContent(),
                classConstructors);
            return info;
        }

        private static void Execute(SourceProductionContext sourceProductionContext, UserControlConstructorsInfo? constructorInfo)
        {
            if (constructorInfo is null)
            {
                return;
            }

            var ctorsInfo = constructorInfo.Value;
            var hasGeneratedPartialMethods = false;

            var sourceBuilder = new IndentedStringBuilder();
            sourceBuilder.AppendLine("using System;");
            sourceBuilder.AppendLine("using System.Runtime.CompilerServices;");
            sourceBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sourceBuilder.AppendLine("using Catel;");
            sourceBuilder.AppendLine("using Catel.IoC;");
            sourceBuilder.AppendLine("using Catel.MVVM.Views;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("#nullable enable");
            sourceBuilder.AppendLine();

            sourceBuilder.AppendLine($"namespace {ctorsInfo.NamespaceName}");
            sourceBuilder.StartBlock();
            sourceBuilder.AppendLine($"partial class {ctorsInfo.ClassName}");
            sourceBuilder.StartBlock();

            sourceBuilder.AppendResolveServiceMethod("UserControlConstructors");

            sourceBuilder.AppendGeneratedCodeAttribute("UserControlConstructors");
            sourceBuilder.AppendLine("private static void InitializeViewPropertyMappings()");
            sourceBuilder.StartBlock();
            sourceBuilder.AppendLine("if (CatelEnvironment.IsInDesignMode)");
            sourceBuilder.StartBlock();
            sourceBuilder.AppendLine("return;");
            sourceBuilder.EndBlock();
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine($"typeof({ctorsInfo.ClassName}).AutoDetectViewPropertiesToSubscribe(IoCContainer.ServiceProvider.GetRequiredService<IViewPropertySelector>());");
            sourceBuilder.EndBlock();

            sourceBuilder.AppendLine();

            if (ctorsInfo.CreateStaticConstructor)
            {
                sourceBuilder.AppendGeneratedCodeAttribute("UserControlConstructors");
                sourceBuilder.AppendLine($"static {ctorsInfo.ClassName}()");
                sourceBuilder.StartBlock();
                sourceBuilder.AppendLine("InitializeViewPropertyMappings();");
                sourceBuilder.EndBlock();
            }

            sourceBuilder.AppendLine();

            foreach (var ctorInfo in ctorsInfo.Constructors)
            {
                if (ctorInfo.CallBase)
                {
                    if (!hasGeneratedPartialMethods)
                    {
                        hasGeneratedPartialMethods = true;

                        sourceBuilder.AppendLine("partial void OnInitializingComponent();");
                        sourceBuilder.AppendLine();
                        sourceBuilder.AppendLine("partial void OnInitializedComponent();");
                        sourceBuilder.AppendLine();
                    }

                    sourceBuilder.AppendGeneratedCodeAttribute("UserControlConstructors");

                    if (ctorInfo.IsActivatorUtilitiesConstructor)
                    {
                        sourceBuilder.AppendLine("[ActivatorUtilitiesConstructor]");
                    }

                    sourceBuilder.AppendLine($"public {ctorsInfo.ClassName}({string.Join(", ", ctorInfo.Parameters.Select(p =>
                        $"{p.ParameterTypeName} {p.Name}"))})");
                    sourceBuilder.Append("    : base(");
                    sourceBuilder.Append(string.Join(", ", ctorInfo.Parameters.Select(p => p.Name)));
                    sourceBuilder.AppendLine(")");
                    sourceBuilder.StartBlock();
                    sourceBuilder.AppendLine("OnInitializingComponent();");
                    sourceBuilder.AppendLine("InitializeComponent();");
                    sourceBuilder.AppendLine("OnInitializedComponent();");
                    sourceBuilder.EndBlock();
                }
                else
                {
                    // Generate empty constructor
                    sourceBuilder.AppendGeneratedCodeAttribute("UserControlConstructors");
                    sourceBuilder.AppendLine($"public {ctorsInfo.ClassName}()");
                    sourceBuilder.Append("    : this(");
                    sourceBuilder.Append(string.Join(", ", ctorInfo.Parameters.Select(p =>
                        $"GetService<{p.ParameterTypeName}>()")));
                    sourceBuilder.AppendLine(")");
                    sourceBuilder.StartBlock();
                    sourceBuilder.EndBlock();
                }

                sourceBuilder.AppendLine();
            }

            sourceBuilder.EndBlock();
            sourceBuilder.EndBlock();

            //#if DEBUG
            //            if (!Debugger.IsAttached)
            //            {
            //                Debugger.Launch();
            //            }
            //#endif

            var fileName = ctorsInfo.FileName;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                fileName = fileName.Replace(".xaml.", ".");
                fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            }
            else
            {
                fileName = ctorsInfo.ClassName;
            }

            sourceProductionContext.AddSource($"{fileName}_UserControlConstructors.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}
