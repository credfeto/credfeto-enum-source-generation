using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Models;

[DebuggerDisplay("{AccessType} {Namespace}.{Name}")]
public readonly record struct ClassEnumGeneration
{
    public ClassEnumGeneration(AccessType accessType, string name, string @namespace, in IReadOnlyList<EnumGeneration> enums, Location location)
    {
        this.AccessType = accessType;
        this.Name = name;
        this.Namespace = @namespace;
        this.Enums = enums;
        this.Location = location;
    }

    public AccessType AccessType { get; }

    public string Name { get; }

    public string Namespace { get; }

    public IReadOnlyList<EnumGeneration> Enums { get; }

    public Location Location { get; }
}