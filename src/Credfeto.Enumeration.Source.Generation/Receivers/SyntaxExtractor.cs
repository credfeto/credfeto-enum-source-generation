using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation.Receivers;

public static class SyntaxExtractor
{
    private const string EnumTextAttributeNamespace = "Credfeto.Enumeration.Source.Generation.Attributes";
    private const string EnumTextAttributeShortName = "EnumTextAttribute";
    public const string EnumTextAttributeMetadataName = EnumTextAttributeNamespace + "." + EnumTextAttributeShortName;

    public static GenerationOptions DetectGenerationOptions(Compilation compilation)
    {
        bool hasDoesNotReturnAttribute =
            compilation.GetTypesByMetadataName("System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute").Length > 0;
        bool supportsUnreachableException =
            compilation.GetTypesByMetadataName("System.Diagnostics.UnreachableException").Length > 0;

        return new(
            hasDoesNotReturnAttribute: hasDoesNotReturnAttribute,
            supportsUnreachableException: supportsUnreachableException
        );
    }

    public static ClassEnumGeneration? ExtractClass(
        SemanticModel semanticModel,
        ClassDeclarationSyntax classDeclarationSyntax,
        in GenerationOptions options,
        CancellationToken cancellationToken
    )
    {
        bool isStatic = classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword);

        if (!isStatic)
        {
            return null;
        }

        bool isPartial = classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);

        if (!isPartial)
        {
            return null;
        }

        AccessType accessType = classDeclarationSyntax.GetAccessType();

        if (accessType == AccessType.PRIVATE)
        {
            return null;
        }

        IReadOnlyList<EnumGeneration> attributesForGeneration = GetEnumsToGenerateForClass(
            semanticModel: semanticModel,
            classDeclarationSyntax: classDeclarationSyntax,
            options: options,
            cancellationToken: cancellationToken
        );

        if (attributesForGeneration.Count == 0)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (
            semanticModel.GetDeclaredSymbol(
                declaration: classDeclarationSyntax,
                cancellationToken: CancellationToken.None
            )
            is INamedTypeSymbol classSymbol
        )
        {
            return new(
                accessType: accessType,
                name: classSymbol.Name,
                classSymbol.ContainingNamespace.ToDisplayString(),
                enums: attributesForGeneration,
                classDeclarationSyntax.GetLocation()
            );
        }

        return null;
    }

    private static IReadOnlyList<EnumGeneration> GetEnumsToGenerateForClass(
        SemanticModel semanticModel,
        ClassDeclarationSyntax classDeclarationSyntax,
        in GenerationOptions options,
        CancellationToken cancellationToken
    )
    {
        if (
            semanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax, cancellationToken: cancellationToken)
            is not INamedTypeSymbol classSymbol
        )
        {
            return [];
        }

        List<EnumGeneration> attributesForGeneration = [];

        ImmutableArray<AttributeData> attributes = classSymbol.GetAttributes();

        foreach (AttributeData? item in attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsCodeGenerationAttribute(item))
            {
                continue;
            }

            if (item.ConstructorArguments.Length != 1)
            {
                continue;
            }

            TypedConstant constructorArguments = item.ConstructorArguments[0];

            if (constructorArguments is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol type })
            {
                IReadOnlyList<IFieldSymbol> members = [.. type.GetMembers().OfType<IFieldSymbol>()];

                EnumGeneration enumGen = new(
                    accessType: AccessType.PUBLIC,
                    name: type.Name,
                    type.ContainingNamespace.ToDisplayString(),
                    members: members,
                    classDeclarationSyntax.GetLocation(),
                    options: options
                );

                attributesForGeneration.Add(item: enumGen);
            }
        }

        return attributesForGeneration;
    }

    private static bool IsCodeGenerationAttribute(AttributeData item)
    {
        if (item.AttributeClass is null)
        {
            return false;
        }

        return StringComparer.Ordinal.Equals(
                item.AttributeClass.ContainingNamespace.ToDisplayString(),
                EnumTextAttributeNamespace
            ) && StringComparer.Ordinal.Equals(item.AttributeClass.Name, EnumTextAttributeShortName);
    }

    public static EnumGeneration? ExtractEnum(
        SemanticModel semanticModel,
        EnumDeclarationSyntax enumDeclarationSyntax,
        in GenerationOptions options,
        CancellationToken cancellationToken
    )
    {
        if (
            semanticModel.GetDeclaredSymbol(
                declaration: enumDeclarationSyntax,
                cancellationToken: CancellationToken.None
            )
            is not INamedTypeSymbol enumSymbol
        )
        {
            return null;
        }

        if (enumSymbol.HasObsoleteAttribute())
        {
            // no point in generating code for obsolete enums
            return null;
        }

        AccessType accessType = enumDeclarationSyntax.GetAccessType();

        if (accessType == AccessType.PRIVATE)
        {
            // skip privates
            return null;
        }

        IReadOnlyList<IFieldSymbol> members =
        [
            .. enumDeclarationSyntax
                .Members.Select(member =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return semanticModel.GetDeclaredSymbol(declaration: member, cancellationToken: cancellationToken)
                        as IFieldSymbol;
                })
                .RemoveNulls(),
        ];

        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyDictionary<string, string>? equalsValueIdentifiers = ExtractEqualsValueIdentifiers(
            enumDeclarationSyntax.Members
        );

        return new(
            accessType: accessType,
            name: enumSymbol.Name,
            enumSymbol.ContainingNamespace.ToDisplayString(),
            members: members,
            enumDeclarationSyntax.GetLocation(),
            options: options,
            equalsValueIdentifiers: equalsValueIdentifiers
        );
    }

    private static IReadOnlyDictionary<string, string>? ExtractEqualsValueIdentifiers(
        in SeparatedSyntaxList<EnumMemberDeclarationSyntax> members
    )
    {
        Dictionary<string, string>? result = null;

        foreach (EnumMemberDeclarationSyntax member in members)
        {
            if (member.EqualsValue?.Value is not IdentifierNameSyntax identifierName)
            {
                continue;
            }

            result ??= new(StringComparer.Ordinal);
            result[member.Identifier.ValueText] = identifierName.Identifier.ValueText;
        }

        return result;
    }
}
