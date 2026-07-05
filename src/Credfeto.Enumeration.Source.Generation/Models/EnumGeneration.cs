using System;
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

    public bool Equals(EnumGeneration other)
    {
        return this.AccessType == other.AccessType
            && StringComparer.Ordinal.Equals(this.Name, other.Name)
            && StringComparer.Ordinal.Equals(this.Namespace, other.Namespace)
            && this.Options == other.Options
            && MembersEqual(this.Members, other.Members)
            && DictionariesEqual(this.EqualsValueIdentifiers, other.EqualsValueIdentifiers);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = this.AccessType.GetHashCode();
            hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(this.Name);
            hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(this.Namespace);
            hashCode = (hashCode * 397) ^ this.Members.Count;
            hashCode = (hashCode * 397) ^ this.Options.GetHashCode();

            foreach (IFieldSymbol member in this.Members)
            {
                hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(member.Name);
                hashCode = (hashCode * 397) ^ (member.ConstantValue?.GetHashCode() ?? 0);
            }

            return hashCode;
        }
    }

    private static bool MembersEqual(IReadOnlyList<IFieldSymbol> a, IReadOnlyList<IFieldSymbol> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        for (int i = 0; i < a.Count; i++)
        {
            if (!StringComparer.Ordinal.Equals(a[i].Name, b[i].Name) || !Equals(a[i].ConstantValue, b[i].ConstantValue))
            {
                return false;
            }
        }

        return true;
    }

    private static bool DictionariesEqual(
        IReadOnlyDictionary<string, string>? a,
        IReadOnlyDictionary<string, string>? b
    )
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (KeyValuePair<string, string> kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out string? value) || !StringComparer.Ordinal.Equals(kvp.Value, value))
            {
                return false;
            }
        }

        return true;
    }
}
