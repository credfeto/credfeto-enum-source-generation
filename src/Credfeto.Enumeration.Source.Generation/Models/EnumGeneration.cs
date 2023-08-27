using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Models;

[DebuggerDisplay("{AccessType} {Namespace}.{Name}")]
public readonly record struct EnumGeneration
{
    public EnumGeneration(AccessType accessType, string name, string @namespace, IReadOnlyList<IFieldSymbol> members)
    {
        this.AccessType = accessType;
        this.Name = name;
        this.Namespace = @namespace;
        this.Members = members;
    }

    public AccessType AccessType { get; }

    public string Name { get; }

    public string Namespace { get; }

    public IReadOnlyList<IFieldSymbol> Members { get; }
}