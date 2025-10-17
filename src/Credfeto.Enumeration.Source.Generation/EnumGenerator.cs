using System;
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

    private static IncrementalValuesProvider<(ClassEnumGeneration? classInfo, ErrorInfo? errorInfo)> ExtractClasses(
        in IncrementalGeneratorInitializationContext context
    )
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (n, _) => n is ClassDeclarationSyntax,
            transform: GetClassDetails
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

    private static (ClassEnumGeneration? classInfo, ErrorInfo? errorInfo) GetClassDetails(
        GeneratorSyntaxContext generatorSyntaxContext,
        CancellationToken cancellationToken
    )
    {
        if (generatorSyntaxContext.Node is ClassDeclarationSyntax classDeclarationSyntax)
        {
            try
            {
                ClassEnumGeneration? classInfo = SyntaxExtractor.ExtractClass(
                    context: generatorSyntaxContext,
                    classDeclarationSyntax: classDeclarationSyntax,
                    cancellationToken: cancellationToken
                );

                return (classInfo, null);
            }
            catch (Exception exception)
            {
                return (null, new ErrorInfo(classDeclarationSyntax.GetLocation(), exception: exception));
            }
        }

        return (null, null);
    }

    private static (EnumGeneration? enumInfo, ErrorInfo? errorInfo) GetEnumDetails(
        GeneratorSyntaxContext generatorSyntaxContext,
        CancellationToken cancellationToken
    )
    {
        if (generatorSyntaxContext.Node is EnumDeclarationSyntax enumDeclarationSyntax)
        {
            try
            {
                EnumGeneration? enumInfo = SyntaxExtractor.ExtractEnum(
                    context: generatorSyntaxContext,
                    enumDeclarationSyntax: enumDeclarationSyntax,
                    cancellationToken: cancellationToken
                );

                return (enumInfo, null);
            }
            catch (Exception exception)
            {
                return (null, new ErrorInfo(enumDeclarationSyntax.GetLocation(), exception: exception));
            }
        }

        return (null, null);
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
                classEnumGeneration.classInfo.Value.Namespace + "." + className + ".generated.cs",
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
                enumGeneration.enumInfo.Value.Namespace + "." + className + ".generated.cs",
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
                    exception.Message + ' ' + exception.StackTrace,
                    category: VersionInformation.Product,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: location
            )
        );
    }
}
