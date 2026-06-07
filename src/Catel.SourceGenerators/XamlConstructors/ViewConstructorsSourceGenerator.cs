namespace Catel.SourceGenerators.XamlConstructors;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

internal static class ViewToViewModelAttributeHelper
{
    internal const string AttributeFullName = "Catel.MVVM.Views.ViewToViewModelAttribute";
    private const string DependencyPropertyTypeName = "System.Windows.DependencyProperty";

    internal static List<string> GetViewToViewModelProperties(INamedTypeSymbol classSymbol)
    {
        var properties = new List<string>();

        var currentType = (INamedTypeSymbol?)classSymbol;
        while (currentType is not null)
        {
            var displayString = currentType.GetFullTypeName();
            if (displayString.StartsWith("Catel.") ||
                displayString.StartsWith("System.") ||
                displayString.StartsWith("Microsoft."))
            {
                break;
            }

            foreach (var member in currentType.GetMembers())
            {
                if (member is IPropertySymbol propertySymbol)
                {
                    var hasAttr = propertySymbol.GetAttributes()
                        .Any(a => a.AttributeClass?.IsType(AttributeFullName) ?? false);
                    if (hasAttr && IsDependencyProperty(currentType, propertySymbol.Name))
                    {
                        properties.Add(propertySymbol.Name);
                    }
                }
            }

            currentType = currentType.BaseType;
        }

        return properties;
    }

    private static bool IsDependencyProperty(INamedTypeSymbol classSymbol, string propertyName)
    {
        var dependencyPropertyFieldName = propertyName + "Property";

        foreach (var member in classSymbol.GetMembers(dependencyPropertyFieldName))
        {
            if (member is IFieldSymbol fieldSymbol &&
                fieldSymbol.IsStatic &&
                fieldSymbol.Type.IsType(DependencyPropertyTypeName))
            {
                return true;
            }
        }

        return false;
    }
}

[Generator]
public class ViewConstructorsSourceGenerator : IIncrementalGenerator
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

    private static ViewConstructorsInfo? Transform(GeneratorSyntaxContext context)
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
            var displayString = baseType.GetFullTypeName();
            if (!isCatelView)
            {
                if (displayString.Contains("Catel.Windows.Controls.UserControl") ||
                    displayString.Contains("Catel.Windows.DataWindow") ||
                    displayString.Contains("Catel.Windows.Window"))
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

        var viewToViewModelProperties = ViewToViewModelAttributeHelper.GetViewToViewModelProperties(classSymbol);
        var injectedServices = InjectedServiceAttributeHelper.GetInjectedServiceFields(classSymbol);

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

                if (baseClassConstructors.Length > 0)
                {
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
                            false));

                        // Only generate empty ctor for non-view model injection ctor
                        if (!isViewModelInjectionCtor)
                        {
                            ctors.Add(new ConstructorInfo(classSymbol.Name,
                                baseCtor.Parameters.Select(x => new ParameterInfo(x.Name, x.Type.ToDisplayString(), x.IsNullable())).ToArray(),
                                false, false));
                        }
                    }
                }
                else if (injectedServices.Count > 0)
                {
                    ctors.Add(new ConstructorInfo(classSymbol.Name,
                        System.Array.Empty<ParameterInfo>(),
                        true,
                        false));
                    ctors.Add(new ConstructorInfo(classSymbol.Name,
                        System.Array.Empty<ParameterInfo>(),
                        false,
                        false));
                }
                else
                {
                    // Regular view without any specific base calls, simply
                    // generate empty ctor with OnInitializingComponent and OnInitializedComponent
                    ctors.Add(new ConstructorInfo(classSymbol.Name,
                        System.Array.Empty<ParameterInfo>(),
                        true, // Need to call base to generate InitializeComponent call
                        true));
                }

                return new ViewConstructorsInfo(
                    classDeclarationSyntax.SyntaxTree.FilePath,
                    classSymbol.ContainingNamespace.ToDisplayString(),
                    classSymbol.Name,
                    isCatelView && !classSymbol.HasStaticConstructorWithContent(),
                    ctors,
                    viewToViewModelProperties,
                    injectedServices);
            }

            // Has explicit parameterless ctor; generate GetService<T> + property mappings but no instance ctors
            return new ViewConstructorsInfo(
                classDeclarationSyntax.SyntaxTree.FilePath,
                classSymbol.ContainingNamespace.ToDisplayString(),
                classSymbol.Name,
                isCatelView && !classSymbol.HasStaticConstructorWithContent(),
                System.Array.Empty<ConstructorInfo>(),
                viewToViewModelProperties,
                injectedServices);
        }

        // TODO: Figure out how to filter the right constructor with the view model

        var classConstructors = classSymbol.InstanceConstructors
            .Where(c => c.Parameters.Length > 0)
            .Where(c => !c.Parameters[0].Type.ImplementsInterface("Catel.MVVM.IViewModel"))
            .Select(x => new ConstructorInfo(classSymbol.Name,
                    x.Parameters.Select(x => new ParameterInfo(x.Name, x.Type.ToDisplayString(), x.IsNullable())).ToArray(),
                    false, false))
            .ToArray();

        var info = new ViewConstructorsInfo(
            classDeclarationSyntax.SyntaxTree.FilePath,
            classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.Name,
            isCatelView && !classSymbol.HasStaticConstructorWithContent(),
            classConstructors,
            viewToViewModelProperties,
            injectedServices);
        return info;
    }

    private static void Execute(SourceProductionContext sourceProductionContext, ViewConstructorsInfo? constructorInfo)
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

        sourceBuilder.AppendResolveServiceMethod("ViewConstructors");

        sourceBuilder.AppendGeneratedCodeAttribute("ViewConstructors");
        sourceBuilder.AppendLine("private static void InitializeViewPropertyMappings()");
        sourceBuilder.StartBlock();
        sourceBuilder.AppendLine("if (CatelEnvironment.IsInDesignMode)");
        sourceBuilder.StartBlock();
        sourceBuilder.AppendLine("return;");
        sourceBuilder.EndBlock();

        if (ctorsInfo.ViewToViewModelProperties.Count > 0)
        {
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("var viewPropertySelector = GetService<IViewPropertySelector>();");
            foreach (var propertyName in ctorsInfo.ViewToViewModelProperties)
            {
                sourceBuilder.AppendLine($"viewPropertySelector.AddPropertyToSubscribe(\"{propertyName}\", typeof({ctorsInfo.ClassName}));");
            }
        }

        sourceBuilder.EndBlock();

        sourceBuilder.AppendLine();

        if (ctorsInfo.CreateStaticConstructor)
        {
            sourceBuilder.AppendGeneratedCodeAttribute("ViewConstructors");
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

                sourceBuilder.AppendGeneratedCodeAttribute("ViewConstructors");

                if (ctorInfo.IsActivatorUtilitiesConstructor)
                {
                    sourceBuilder.AppendLine("[ActivatorUtilitiesConstructor]");
                }

                // Combine base parameters with injected service parameters
                var allParams = ctorInfo.Parameters.Select(p => $"{p.ParameterTypeName} {p.Name}")
                    .Concat(ctorsInfo.InjectedServices.Select(s => $"{s.TypeName} {s.ParameterName}"));
                sourceBuilder.AppendLine($"public {ctorsInfo.ClassName}({string.Join(", ", allParams)})");
                sourceBuilder.Append("    : base(");
                sourceBuilder.Append(string.Join(", ", ctorInfo.Parameters.Select(p => p.Name)));
                sourceBuilder.AppendLine(")");
                sourceBuilder.StartBlock();
                foreach (var service in ctorsInfo.InjectedServices)
                {
                    sourceBuilder.AppendLine($"{service.FieldName} = {service.ParameterName};");
                }
                sourceBuilder.AppendLine("OnInitializingComponent();");
                sourceBuilder.AppendLine("InitializeComponent();");
                sourceBuilder.AppendLine("OnInitializedComponent();");
                sourceBuilder.EndBlock();
            }
            else
            {
                // Generate empty constructor
                sourceBuilder.AppendGeneratedCodeAttribute("ViewConstructors");
                sourceBuilder.AppendLine($"public {ctorsInfo.ClassName}()");
                var allServiceCalls = ctorInfo.Parameters.Select(p => $"GetService<{p.ParameterTypeName}>()")
                    .Concat(ctorsInfo.InjectedServices.Select(s => $"GetService<{s.TypeName}>()"));
                sourceBuilder.Append("    : this(");
                sourceBuilder.Append(string.Join(", ", allServiceCalls));
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

        var fileName = Generation.SourceFileNameHelper.GetGeneratedFileName(ctorsInfo.NamespaceName, ctorsInfo.ClassName);

        sourceProductionContext.AddSource($"{fileName}_ViewConstructors.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
}
