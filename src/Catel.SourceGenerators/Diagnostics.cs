namespace Catel.SourceGenerators
{
    using Microsoft.CodeAnalysis;

    internal static class Diagnostics
    {
        internal static readonly DiagnosticDescriptor ConflictingConstructorsAndInjectedService = new DiagnosticDescriptor(
            id: "CTL0001",
            title: "Cannot combine constructors with injection attributes",
            messageFormat: "You cannot combine existing constructors and the `InjectedService` or `InjectedModel` attribute",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}
