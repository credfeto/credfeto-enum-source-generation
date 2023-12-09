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

internal static class SyntaxExtractor
{
    public static ClassEnumGeneration? ExtractClass(in GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
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

        GenerationOptions options = DetectGenerationOptions(context: context, cancellationToken: cancellationToken);

        IReadOnlyList<EnumGeneration> attributesForGeneration =
            GetEnumsToGenerateForClass(context: context, classDeclarationSyntax: classDeclarationSyntax, options: options, cancellationToken: cancellationToken);

        if (attributesForGeneration.Count == 0)
        {
            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (context.SemanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax, cancellationToken: CancellationToken.None) is INamedTypeSymbol classSymbol)
        {
            return new(accessType: accessType, name: classSymbol.Name, classSymbol.ContainingNamespace.ToDisplayString(), enums: attributesForGeneration, classDeclarationSyntax.GetLocation());
        }

        return null;
    }

    private static IReadOnlyList<EnumGeneration> GetEnumsToGenerateForClass(in GeneratorSyntaxContext context,
                                                                            ClassDeclarationSyntax classDeclarationSyntax,
                                                                            in GenerationOptions options,
                                                                            CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax, cancellationToken: cancellationToken) is not INamedTypeSymbol classSymbol)
        {
            return Array.Empty<EnumGeneration>();
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

            TypedConstant constructorArguments = item.ConstructorArguments[0];

            if (constructorArguments.Kind == TypedConstantKind.Type && constructorArguments.Value is INamedTypeSymbol type)
            {
                IReadOnlyList<IFieldSymbol> members = type.GetMembers()
                                                          .OfType<IFieldSymbol>()
                                                          .ToArray();

                EnumGeneration enumGen = new(accessType: AccessType.PUBLIC,
                                             name: type.Name,
                                             type.ContainingNamespace.ToDisplayString(),
                                             members: members,
                                             classDeclarationSyntax.GetLocation(),
                                             options: options);

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

        if (item.AttributeClass.ContainingNamespace.ToDisplayString() != "Credfeto.Enumeration.Source.Generation.Attributes")
        {
            return false;
        }

        if (item.AttributeClass.Name != "EnumTextAttribute")
        {
            return false;
        }

        return true;
    }

    public static EnumGeneration? ExtractEnum(in GeneratorSyntaxContext context, EnumDeclarationSyntax enumDeclarationSyntax, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetDeclaredSymbol(declaration: enumDeclarationSyntax, cancellationToken: CancellationToken.None) is not INamedTypeSymbol enumSymbol)
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

        List<IFieldSymbol> members = [];

        foreach (EnumMemberDeclarationSyntax member in enumDeclarationSyntax.Members)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (context.SemanticModel.GetDeclaredSymbol(declaration: member, cancellationToken: CancellationToken.None) is IFieldSymbol fieldSymbol)
            {
                members.Add(item: fieldSymbol);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        GenerationOptions options = DetectGenerationOptions(context: context, cancellationToken: cancellationToken);

        return new(accessType: accessType, name: enumSymbol.Name, enumSymbol.ContainingNamespace.ToDisplayString(), members: members, enumDeclarationSyntax.GetLocation(), options: options);
    }

    private static GenerationOptions DetectGenerationOptions(in GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        bool hasDoesNotReturnAttribute = !context.SemanticModel.LookupNamespacesAndTypes(position: 0, container: null, name: "System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute")
                                                 .IsEmpty;

        cancellationToken.ThrowIfCancellationRequested();
        bool hasUnreachableException = !context.SemanticModel.LookupNamespacesAndTypes(position: 0, container: null, name: "System.Diagnostics.UnreachableException")
                                               .IsEmpty;

        cancellationToken.ThrowIfCancellationRequested();

        return new(hasDoesNotReturnAttribute: hasDoesNotReturnAttribute, supportsUnreachableException: hasUnreachableException);
    }
}