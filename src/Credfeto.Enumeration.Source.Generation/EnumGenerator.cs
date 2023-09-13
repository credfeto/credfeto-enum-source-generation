using System;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Models;
using Credfeto.Enumeration.Source.Generation.Receivers;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation;

[Generator(LanguageNames.CSharp)]
public sealed class EnumGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not EnumSyntaxReceiver receiver)
        {
            return;
        }

        try
        {
            ExecuteInternal(context: context, receiver: receiver);
        }
        catch (Exception exception)
        {
            context.ReportDiagnostic(diagnostic: Diagnostic.Create(new(id: "ENUM004",
                                                                       title: "Unhandled exception",
                                                                       messageFormat: exception.Message,
                                                                       category: "Credfeto.Enumerationc.Source.Generation",
                                                                       defaultSeverity: DiagnosticSeverity.Error,
                                                                       isEnabledByDefault: true),
                                                                   context.Compilation.Assembly.Locations.FirstOrDefault()));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new EnumSyntaxReceiver());
    }

    private static void ExecuteInternal(in GeneratorExecutionContext context, EnumSyntaxReceiver receiver)
    {
        bool hasDoesNotReturn = receiver.HasDoesNotReturnAttribute;
        bool supportsUnreachableException = receiver.SupportsUnreachableException;

        foreach (EnumGeneration enumDeclaration in receiver.Enums)
        {
            try
            {
                GenerateClassForEnum(context: context, enumDeclaration: enumDeclaration, hasDoesNotReturn: hasDoesNotReturn, supportsUnreachableException: supportsUnreachableException);
            }
            catch (Exception exception)
            {
                context.ReportDiagnostic(diagnostic: Diagnostic.Create(new(id: "ENUM002",
                                                                           title: "Unhandled exception",
                                                                           messageFormat: exception.Message,
                                                                           category: "Credfeto.Enumerationc.Source.Generation",
                                                                           defaultSeverity: DiagnosticSeverity.Error,
                                                                           isEnabledByDefault: true),
                                                                       location: enumDeclaration.Location));
            }
        }

        foreach (ClassEnumGeneration classDeclaration in receiver.Classes)
        {
            try
            {
                GenerateClassForClass(context: context, classDeclaration: classDeclaration, hasDoesNotReturn: hasDoesNotReturn, supportsUnreachableException: supportsUnreachableException);
            }
            catch (Exception exception)
            {
                context.ReportDiagnostic(diagnostic: Diagnostic.Create(new(id: "ENUM003",
                                                                           title: "Unhandled exception",
                                                                           messageFormat: exception.Message,
                                                                           category: "Credfeto.Enumerationc.Source.Generation",
                                                                           defaultSeverity: DiagnosticSeverity.Error,
                                                                           isEnabledByDefault: true),
                                                                       location: classDeclaration.Location));
            }
        }
    }

    private static void GenerateClassForEnum(in GeneratorExecutionContext context, in EnumGeneration enumDeclaration, bool hasDoesNotReturn, bool supportsUnreachableException)
    {
        string className = EnumSourceGenerator.GenerateClassForEnum(enumDeclaration: enumDeclaration,
                                                                hasDoesNotReturn: hasDoesNotReturn,
                                                                supportsUnreachableException: supportsUnreachableException,
                                                                out CodeBuilder source);

        context.AddSource(enumDeclaration.Namespace + "." + className + ".generated.cs", sourceText: source.Text);
    }

    private static void GenerateClassForClass(in GeneratorExecutionContext context, in ClassEnumGeneration classDeclaration, bool hasDoesNotReturn, bool supportsUnreachableException)
    {
        string className = EnumSourceGenerator.GenerateClassForClass(classDeclaration: classDeclaration,
                                                                 hasDoesNotReturn: hasDoesNotReturn,
                                                                 supportsUnreachableException: supportsUnreachableException,
                                                                 out CodeBuilder source);

        context.AddSource(classDeclaration.Namespace + "." + className + ".generated.cs", sourceText: source.Text);
    }
}