using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Models;

public sealed class EnumGeneration2
{
    public EnumGeneration2(AccessType accessType, string name, string @namespace, IReadOnlyList<IFieldSymbol> members)
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