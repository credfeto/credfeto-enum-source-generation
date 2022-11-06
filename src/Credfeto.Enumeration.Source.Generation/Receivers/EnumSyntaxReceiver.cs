using System.Collections.Generic;
using Credfeto.Enumeration.Source.Generation.Extensions;
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
        this.Classes = new();
    }

    public List<EnumGeneration> Enums { get; }

    public List<ClassEnumGeneration> Classes { get; }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
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

        IReadOnlyList<INamedTypeSymbol> attributesForGeneration = GetEnumsToGenerateForClass(context: context, classDeclarationSyntax: classDeclarationSyntax);

        if (attributesForGeneration.Count == 0)
        {
            return;
        }

        INamedTypeSymbol classSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax)!;

        AccessType accessType = classDeclarationSyntax.GetAccessType();

        this.Classes.Add(item: new(accessType: accessType, name: classSymbol.Name, classSymbol.ContainingNamespace.ToDisplayString(), attributes: attributesForGeneration));
    }

    private static IReadOnlyList<INamedTypeSymbol> GetEnumsToGenerateForClass(in GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
    {
        List<INamedTypeSymbol> attributesForGeneration = new();

        foreach (AttributeListSyntax attributeList in classDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                INamedTypeSymbol attributeSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax)!;

                if (attributeSymbol.Name == "EnumTextAttribute" && attributeSymbol.ContainingNamespace.Name == "Credfeto.Enumeration.Source.Generation.Attributes")
                {
                    AttributeArgumentSyntax? arg = attribute.ArgumentList?.Arguments.FirstOrDefault();

                    if (arg != null)
                    {
                        TypeInfo attributeType = context.SemanticModel.GetTypeInfo(arg.Expression);

                        if (!attributeType.IsEnum())
                        {
                            continue;
                        }

                        INamedTypeSymbol enumType = (INamedTypeSymbol)attributeType.Type!;

                        attributesForGeneration.Add(item: enumType);
                    }
                }
            }
        }

        return attributesForGeneration;
    }

    private void AddDefinedEnums(in GeneratorSyntaxContext context, EnumDeclarationSyntax enumDeclarationSyntax)
    {
        INamedTypeSymbol enumSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: enumDeclarationSyntax)!;

        AccessType accessType = enumDeclarationSyntax.GetAccessType();

        if (accessType == AccessType.PRIVATE)
        {
            // skip privates
            return;
        }

        SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDeclarationSyntax.Members;

        this.Enums.Add(new(accessType: accessType, name: enumSymbol.Name, enumSymbol.ContainingNamespace.ToDisplayString(), members: members));
    }
}