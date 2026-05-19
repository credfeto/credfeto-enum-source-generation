using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Models;

[DebuggerDisplay("{AccessType} {Namespace}.{Name}")]
public readonly record struct EnumGeneration
{
    public EnumGeneration(
        AccessType accessType,
        string name,
        string @namespace,
        IReadOnlyList<IFieldSymbol> members,
        Location location,
        in GenerationOptions options,
        IReadOnlyDictionary<string, string>? equalsValueIdentifiers = null
    )
    {
        this.AccessType = accessType;
        this.Name = name;
        this.Namespace = @namespace;
        this.Members = members;
        this.Location = location;
        this.Options = options;
        this.EqualsValueIdentifiers = equalsValueIdentifiers;
    }

    public AccessType AccessType { get; }

    public string Name { get; }

    public string Namespace { get; }

    public IReadOnlyList<IFieldSymbol> Members { get; }

    public Location Location { get; }

    public GenerationOptions Options { get; }

    public IReadOnlyDictionary<string, string>? EqualsValueIdentifiers { get; }
}
