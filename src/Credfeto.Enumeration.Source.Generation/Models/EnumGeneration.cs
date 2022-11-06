using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation.Models;

[DebuggerDisplay("{AccessType} {Namespace}.{Name}")]
public sealed class EnumGeneration
{
    public EnumGeneration(AccessType accessType, string name, string @namespace, in SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
    {
        this.AccessType = accessType;
        this.Name = name;
        this.Namespace = @namespace;
        this.Members = members;
    }

    public SeparatedSyntaxList<EnumMemberDeclarationSyntax> Members { get; }

    public AccessType AccessType { get; }

    public string Name { get; }

    public string Namespace { get; }
}