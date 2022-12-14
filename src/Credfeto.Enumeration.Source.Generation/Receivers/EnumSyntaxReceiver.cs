using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation.Receivers;

public sealed class EnumSyntaxReceiver : ISyntaxContextReceiver
{
    private bool? _hasDoesNotReturnAttribute;
    private bool? _hasUnreachableException;

    public EnumSyntaxReceiver()
    {
        this.Enums = new();
        this.Classes = new();
    }

    public List<EnumGeneration> Enums { get; }

    public List<ClassEnumGeneration> Classes { get; }

    public bool HasDoesNotReturnAttribute => this._hasDoesNotReturnAttribute.GetValueOrDefault(false);

    public bool SupportsUnreachableException => this._hasUnreachableException.GetValueOrDefault(false);


    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (!this._hasDoesNotReturnAttribute.HasValue)
        {
            this._hasDoesNotReturnAttribute = !context.SemanticModel.LookupNamespacesAndTypes(position: 0, container: null, name: "System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute")
                                                      .IsEmpty;
        }

        if (!this._hasUnreachableException.HasValue)
        {
            this._hasUnreachableException = !context.SemanticModel.LookupNamespacesAndTypes(position: 0, container: null, name: "System.Diagnostics.UnreachableException")
                                                    .IsEmpty;
        }

        if (context.Node is EnumDeclarationSyntax enumDeclarationSyntax)
        {
            this.AddDefinedEnums(context: context, enumDeclarationSyntax: enumDeclarationSyntax);

            return;
        }

        if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
        {
            this.AddEnumExtensionHolder(context: context, classDeclarationSyntax: classDeclarationSyntax);
        }
    }

    private void AddEnumExtensionHolder(in GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
    {
        bool isStatic = classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword);

        if (!isStatic)
        {
            return;
        }

        bool isPartial = classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);

        if (!isPartial)
        {
            return;
        }

        AccessType accessType = classDeclarationSyntax.GetAccessType();

        if (accessType == AccessType.PRIVATE)
        {
            return;
        }

        IReadOnlyList<EnumGeneration> attributesForGeneration = GetEnumsToGenerateForClass(context: context, classDeclarationSyntax: classDeclarationSyntax);

        if (attributesForGeneration.Count == 0)
        {
            return;
        }

        INamedTypeSymbol classSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax)!;

        this.Classes.Add(item: new(accessType: accessType, name: classSymbol.Name, classSymbol.ContainingNamespace.ToDisplayString(), enums: attributesForGeneration));
    }

    private static IReadOnlyList<EnumGeneration> GetEnumsToGenerateForClass(in GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
    {
        List<EnumGeneration> attributesForGeneration = new();

        INamedTypeSymbol classSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax)!;

        ImmutableArray<AttributeData> attributes = classSymbol.GetAttributes();

        foreach (AttributeData? item in attributes)
        {
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

                EnumGeneration enumGen = new(accessType: AccessType.PUBLIC, name: type.Name, type.ContainingNamespace.ToDisplayString(), members: members);

                attributesForGeneration.Add(item: enumGen);
            }
        }

        return attributesForGeneration;
    }

    private static bool IsCodeGenerationAttribute(AttributeData item)
    {
        if (item.AttributeClass == null)
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

    private void AddDefinedEnums(in GeneratorSyntaxContext context, EnumDeclarationSyntax enumDeclarationSyntax)
    {
        INamedTypeSymbol enumSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: enumDeclarationSyntax)!;

        if (enumSymbol.HasObsoleteAttribute())
        {
            // no point in generating code for obsolete enums
            return;
        }

        AccessType accessType = enumDeclarationSyntax.GetAccessType();

        if (accessType == AccessType.PRIVATE)
        {
            // skip privates
            return;
        }

        List<IFieldSymbol> members = new();

        foreach (EnumMemberDeclarationSyntax member in enumDeclarationSyntax.Members)
        {
            IFieldSymbol fieldSymbol = (IFieldSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: member)!;

            members.Add(item: fieldSymbol);
        }

        this.Enums.Add(new(accessType: accessType, name: enumSymbol.Name, enumSymbol.ContainingNamespace.ToDisplayString(), members: members));
    }
}