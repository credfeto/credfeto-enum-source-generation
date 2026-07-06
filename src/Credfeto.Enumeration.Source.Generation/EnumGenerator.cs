using System;
using System.Collections.Generic;
using System.Linq;
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
        IncrementalValueProvider<GenerationOptions> optionsProvider = context.CompilationProvider.Select(
            static (compilation, _) => SyntaxExtractor.DetectGenerationOptions(compilation)
        );

        context.RegisterSourceOutput(
            ExtractEnums(context).Combine(optionsProvider).Select(ApplyOptionsToEnum),
            action: GenerateEnums
        );

        context.RegisterSourceOutput(
            ExtractClasses(context).Combine(optionsProvider).Select(ApplyOptionsToClass),
            action: GenerateClasses
        );
    }

    private static IncrementalValuesProvider<(EnumGeneration? enumInfo, ErrorInfo? errorInfo)> ExtractEnums(
        in IncrementalGeneratorInitializationContext context
    )
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (n, _) => n is EnumDeclarationSyntax,
            transform: GetEnumDetails
        );
    }

    private static IncrementalValuesProvider<(ClassEnumGeneration? classInfo, ErrorInfo? errorInfo)> ExtractClasses(
        in IncrementalGeneratorInitializationContext context
    )
    {
        return context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: SyntaxExtractor.EnumTextAttributeMetadataName,
            predicate: static (n, _) => n is ClassDeclarationSyntax,
            transform: GetClassDetails
        );
    }

    private static (EnumGeneration? enumInfo, ErrorInfo? errorInfo) GetEnumDetails(
        GeneratorSyntaxContext generatorSyntaxContext,
        CancellationToken cancellationToken
    )
    {
        EnumDeclarationSyntax enumDeclarationSyntax = (EnumDeclarationSyntax)generatorSyntaxContext.Node;

        try
        {
            EnumGeneration? enumInfo = SyntaxExtractor.ExtractEnum(
                semanticModel: generatorSyntaxContext.SemanticModel,
                enumDeclarationSyntax: enumDeclarationSyntax,
                options: default,
                cancellationToken: cancellationToken
            );

            return (enumInfo, null);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return (null, new ErrorInfo(enumDeclarationSyntax.GetLocation(), exception: exception));
        }
    }

    private static (ClassEnumGeneration? classInfo, ErrorInfo? errorInfo) GetClassDetails(
        GeneratorAttributeSyntaxContext generatorAttributeSyntaxContext,
        CancellationToken cancellationToken
    )
    {
        ClassDeclarationSyntax classDeclarationSyntax = (ClassDeclarationSyntax)
            generatorAttributeSyntaxContext.TargetNode;

        try
        {
            ClassEnumGeneration? classInfo = SyntaxExtractor.ExtractClass(
                semanticModel: generatorAttributeSyntaxContext.SemanticModel,
                classDeclarationSyntax: classDeclarationSyntax,
                options: default,
                cancellationToken: cancellationToken
            );

            return (classInfo, null);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return (null, new ErrorInfo(classDeclarationSyntax.GetLocation(), exception: exception));
        }
    }

    private static (EnumGeneration? enumInfo, ErrorInfo? errorInfo) ApplyOptionsToEnum(
        ((EnumGeneration? enumInfo, ErrorInfo? errorInfo) result, GenerationOptions options) pair,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (pair.result.enumInfo is null)
        {
            return pair.result;
        }

        EnumGeneration e = pair.result.enumInfo.Value;

        return (
            new EnumGeneration(
                accessType: e.AccessType,
                name: e.Name,
                @namespace: e.Namespace,
                members: e.Members,
                location: e.Location,
                options: pair.options,
                equalsValueIdentifiers: e.EqualsValueIdentifiers
            ),
            pair.result.errorInfo
        );
    }

    private static (ClassEnumGeneration? classInfo, ErrorInfo? errorInfo) ApplyOptionsToClass(
        ((ClassEnumGeneration? classInfo, ErrorInfo? errorInfo) result, GenerationOptions options) pair,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (pair.result.classInfo is null)
        {
            return pair.result;
        }

        ClassEnumGeneration c = pair.result.classInfo.Value;

        IReadOnlyList<EnumGeneration> updatedEnums =
        [
            .. c.Enums.Select(e =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                return new EnumGeneration(
                    accessType: e.AccessType,
                    name: e.Name,
                    @namespace: e.Namespace,
                    members: e.Members,
                    location: e.Location,
                    options: pair.options,
                    equalsValueIdentifiers: e.EqualsValueIdentifiers
                );
            }),
        ];

        return (
            new ClassEnumGeneration(
                accessType: c.AccessType,
                name: c.Name,
                @namespace: c.Namespace,
                enums: updatedEnums,
                location: c.Location
            ),
            pair.result.errorInfo
        );
    }

    private static void GenerateClasses(
        SourceProductionContext sourceProductionContext,
        (ClassEnumGeneration? classInfo, ErrorInfo? errorInfo) classEnumGeneration
    )
    {
        if (classEnumGeneration.errorInfo is not null)
        {
            ErrorInfo ei = classEnumGeneration.errorInfo.Value;
            ReportException(location: ei.Location, context: sourceProductionContext, exception: ei.Exception);

            return;
        }

        if (classEnumGeneration.classInfo is null)
        {
            return;
        }

        try
        {
            string className = EnumSourceGenerator.GenerateClassForClass(
                classDeclaration: classEnumGeneration.classInfo.Value,
                out CodeBuilder? codeBuilder
            );

            sourceProductionContext.AddSource(
                $"{classEnumGeneration.classInfo.Value.Namespace}.{className}.generated.cs",
                sourceText: codeBuilder.Text
            );
        }
        catch (Exception exception)
        {
            ReportException(
                location: classEnumGeneration.classInfo.Value.Location,
                context: sourceProductionContext,
                exception: exception
            );
        }
    }

    private static void GenerateEnums(
        SourceProductionContext sourceProductionContext,
        (EnumGeneration? enumInfo, ErrorInfo? errorInfo) enumGeneration
    )
    {
        if (enumGeneration.errorInfo is not null)
        {
            ErrorInfo ei = enumGeneration.errorInfo.Value;
            ReportException(location: ei.Location, context: sourceProductionContext, exception: ei.Exception);

            return;
        }

        if (enumGeneration.enumInfo is null)
        {
            return;
        }

        try
        {
            string className = EnumSourceGenerator.GenerateClassForEnum(
                enumDeclaration: enumGeneration.enumInfo.Value,
                out CodeBuilder codeBuilder
            );

            sourceProductionContext.AddSource(
                $"{enumGeneration.enumInfo.Value.Namespace}.{className}.generated.cs",
                sourceText: codeBuilder.Text
            );
        }
        catch (Exception exception)
        {
            ReportException(
                location: enumGeneration.enumInfo.Value.Location,
                context: sourceProductionContext,
                exception: exception
            );
        }
    }

    private static void ReportException(Location location, in SourceProductionContext context, Exception exception)
    {
        context.ReportDiagnostic(
            diagnostic: Diagnostic.Create(
                new(
                    id: "ENUM002",
                    title: "Unhandled Exception",
                    $"{exception.Message} {exception.StackTrace}",
                    category: VersionInformation.Product,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: location
            )
        );
    }
}
