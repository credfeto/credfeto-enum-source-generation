using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Models;

[DebuggerDisplay("{AccessType} {Namespace}.{Name}")]
public readonly record struct ClassEnumGeneration
{
    public ClassEnumGeneration(
        AccessType accessType,
        string name,
        string @namespace,
        in IReadOnlyList<EnumGeneration> enums,
        Location location
    )
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

    public bool Equals(ClassEnumGeneration other)
    {
        return this.AccessType == other.AccessType
            && StringComparer.Ordinal.Equals(this.Name, other.Name)
            && StringComparer.Ordinal.Equals(this.Namespace, other.Namespace)
            && this.Enums.SequenceEqual(other.Enums);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = this.AccessType.GetHashCode();
            hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(this.Name);
            hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(this.Namespace);
            hashCode = (hashCode * 397) ^ this.Enums.Count;

            foreach (EnumGeneration e in this.Enums)
            {
                hashCode = (hashCode * 397) ^ e.GetHashCode();
            }

            return hashCode;
        }
    }
}
