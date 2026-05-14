namespace Catel.SourceGenerators;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    internal static readonly DiagnosticDescriptor ConflictingConstructorsAndInjectedService = new DiagnosticDescriptor(
        id: "CTLSG001",
        title: "Cannot combine constructors with injection attributes",
        messageFormat: "You cannot combine existing constructors and the `InjectedService` or `InjectedModel` attribute",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor ClassShouldBePartial = new DiagnosticDescriptor(
        id: "CTLSG002",
        title: "Class should be partial",
        messageFormat: "Make {1} '{0}' partial so Catel can generate constructors",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
