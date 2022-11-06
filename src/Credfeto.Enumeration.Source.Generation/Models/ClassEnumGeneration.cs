using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Models;

[DebuggerDisplay("{AccessType} {Namespace}.{Name}")]
public sealed class ClassEnumGeneration
{
    public ClassEnumGeneration(AccessType accessType, string name, string @namespace, in IReadOnlyList<INamedTypeSymbol> attributes)
    {
        this.AccessType = accessType;
        this.Name = name;
        this.Namespace = @namespace;
        this.Attributes = attributes;
    }

    public AccessType AccessType { get; }

    public string Name { get; }

    public string Namespace { get; }

    public IReadOnlyList<INamedTypeSymbol> Attributes { get; }
}