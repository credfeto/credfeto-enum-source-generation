using System.Threading;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Models;
using Credfeto.Enumeration.Source.Generation.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation;

[Generator(LanguageNames.CSharp)]
public sealed class EnumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(ExtractEnums(context), action: GenerateEnums);

        context.RegisterSourceOutput(ExtractClasses(context), action: GenerateClasses);
    }

    private static IncrementalValuesProvider<ClassEnumGeneration?> ExtractClasses(in IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(predicate: static (n, _) => n is ClassDeclarationSyntax, transform: GetClassDetails);
    }

    private static IncrementalValuesProvider<EnumGeneration?> ExtractEnums(in IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(predicate: static (n, _) => n is EnumDeclarationSyntax, transform: GetEnumDetails);
    }

    private static ClassEnumGeneration? GetClassDetails(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        return generatorSyntaxContext.Node is ClassDeclarationSyntax classDeclarationSyntax
            ? EnumSyntaxReceiver.ExtractClass(context: generatorSyntaxContext, classDeclarationSyntax: classDeclarationSyntax, cancellationToken: cancellationToken)
            : null;
    }

    private static EnumGeneration? GetEnumDetails(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        return generatorSyntaxContext.Node is EnumDeclarationSyntax enumDeclarationSyntax
            ? EnumSyntaxReceiver.ExtractEnum(context: generatorSyntaxContext, enumDeclarationSyntax: enumDeclarationSyntax, cancellationToken: cancellationToken)
            : null;
    }

    private static void GenerateClasses(SourceProductionContext sourceProductionContext, ClassEnumGeneration? classEnumGeneration)
    {
        if (classEnumGeneration is null)
        {
            return;
        }

        string className = EnumSourceGenerator.GenerateClassForClass(classDeclaration: classEnumGeneration.Value,
                                                                     hasDoesNotReturn: false,
                                                                     supportsUnreachableException: false,
                                                                     out CodeBuilder? codeBuilder);

        sourceProductionContext.AddSource(classEnumGeneration.Value.Namespace + "." + className + ".generated.cs", sourceText: codeBuilder.Text);
    }

    private static void GenerateEnums(SourceProductionContext sourceProductionContext, EnumGeneration? enumGeneration)
    {
        if (enumGeneration is null)
        {
            return;
        }

        string className = EnumSourceGenerator.GenerateClassForEnum(enumDeclaration: enumGeneration.Value, hasDoesNotReturn: false, supportsUnreachableException: false, out CodeBuilder? codeBuilder);

        sourceProductionContext.AddSource(enumGeneration.Value.Namespace + "." + className + ".generated.cs", sourceText: codeBuilder.Text);
    }
}