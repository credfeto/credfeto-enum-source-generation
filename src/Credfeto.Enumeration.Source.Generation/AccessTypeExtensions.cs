using System;
using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation;

internal static class AccessTypeExtensions
{
    public static string ConvertAccessType(this AccessType accessType)
    {
        return accessType switch
        {
            AccessType.PUBLIC => "public",
            AccessType.PRIVATE => "private",
            AccessType.PROTECTED => "protected",
            AccessType.PROTECTED_INTERNAL => "protected internal",
            AccessType.INTERNAL => "internal",
            _ => throw new ArgumentOutOfRangeException(
                nameof(accessType),
                actualValue: accessType,
                message: "Unknown access type"
            ),
        };
    }
}