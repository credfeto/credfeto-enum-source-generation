using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

public static class TypeDeclarationSyntaxExtensions
{
    public static AccessType GetAccessType(this EnumDeclarationSyntax generatorSyntaxContext)
    {
        return GetAccessType(generatorSyntaxContext.Modifiers);
    }

    public static AccessType GetAccessType(this ClassDeclarationSyntax generatorSyntaxContext)
    {
        return GetAccessType(generatorSyntaxContext.Modifiers);
    }

    private static AccessType GetAccessType(in SyntaxTokenList modifiers)
    {
        bool isPublic = modifiers.Any(SyntaxKind.PublicKeyword);

        if (isPublic)
        {
            return AccessType.PUBLIC;
        }

        bool isPrivate = modifiers.Any(SyntaxKind.PrivateKeyword);

        if (isPrivate)
        {
            return AccessType.PRIVATE;
        }

        bool isInternal = modifiers.Any(SyntaxKind.InternalKeyword);

        bool isProtected = modifiers.Any(SyntaxKind.ProtectedKeyword);

        if (isProtected)
        {
            return isInternal
                ? AccessType.PROTECTED_INTERNAL
                : AccessType.PROTECTED;
        }

        return AccessType.INTERNAL;
    }
}