using System.Collections.Generic;
using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation.Receivers;

public sealed class EnumSyntaxReceiver : ISyntaxContextReceiver
{
    public EnumSyntaxReceiver()
    {
        this.Enums = new();
    }

    public List<EnumGeneration> Enums { get; }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not EnumDeclarationSyntax enumDeclarationSyntax)
        {
            return;
        }

        INamedTypeSymbol enumSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: enumDeclarationSyntax)!;

        AccessType accessType = GetAccessType(enumDeclarationSyntax);

        if (accessType == AccessType.PRIVATE)
        {
            // skip privates
            return;
        }

        SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclarationSyntax.Members;

        this.Enums.Add(new(accessType: accessType, name: enumSymbol.Name, enumSymbol.ContainingNamespace.ToDisplayString(), members: members));
    }

    private static AccessType GetAccessType(EnumDeclarationSyntax generatorSyntaxContext)
    {
        bool isPublic = generatorSyntaxContext.Modifiers.Any(SyntaxKind.PublicKeyword);

        if (isPublic)
        {
            return AccessType.PUBLIC;
        }

        bool isPrivate = generatorSyntaxContext.Modifiers.Any(SyntaxKind.PrivateKeyword);

        if (isPrivate)
        {
            return AccessType.PRIVATE;
        }

        bool isInternal = generatorSyntaxContext.Modifiers.Any(SyntaxKind.InternalKeyword);

        bool isProtected = generatorSyntaxContext.Modifiers.Any(SyntaxKind.ProtectedKeyword);

        if (isProtected)
        {
            return isInternal
                ? AccessType.PROTECTED_INTERNAL
                : AccessType.PROTECTED;
        }

        return AccessType.INTERNAL;
    }
}