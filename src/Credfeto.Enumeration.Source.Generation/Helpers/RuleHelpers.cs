using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Helpers;

internal static class RuleHelpers
{
    public static DiagnosticDescriptor CreateRule(
        string code,
        string category,
        string title,
        string message
    )
    {
        LiteralString translatableTitle = new(title);
        LiteralString translatableMessage = new(message);

        return new(
            id: code,
            title: translatableTitle,
            messageFormat: translatableMessage,
            category: category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: translatableMessage
        );
    }
}
