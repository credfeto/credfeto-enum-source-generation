using System.Collections.Immutable;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Credfeto.Enumeration.Source.Generation;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProhibitEnumToStringsDiagnosticsAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = RuleHelpers.CreateRule(code: "ENUM001",
                                                                               category: "Do not use ToString() on an enum use EnumHelpers.GetName(this Enum value) instead",
                                                                               title: "Do not use ToString() on an enum use EnumHelpers.GetName(this Enum value) instead",
                                                                               message: "Do not use ToString() on an enum use EnumHelpers.GetName(this Enum value) instead");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        new[]
        {
            Rule
        }.ToImmutableArray();

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(PerformCheck);
    }

    private static void PerformCheck(CompilationStartAnalysisContext compilationStartContext)
    {
        compilationStartContext.RegisterSyntaxNodeAction(action: LookForBannedMethods,
                                                         SyntaxKind.PointerMemberAccessExpression,
                                                         SyntaxKind.SimpleMemberAccessExpression,
                                                         SyntaxKind.InterpolatedStringExpression,
                                                         SyntaxKind.AddExpression);
    }

    private static void LookForBannedMethods(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        LookForExplicitBannedMethods(syntaxNodeAnalysisContext);
        LookForBannedInInterpolatedStrings(syntaxNodeAnalysisContext);
        LookForBannedInStringConcatenation(syntaxNodeAnalysisContext);
    }

    private static void LookForExplicitBannedMethods(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        if (syntaxNodeAnalysisContext.Node is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            LookForBannedMethod(memberAccessExpressionSyntax: memberAccessExpressionSyntax, syntaxNodeAnalysisContext: syntaxNodeAnalysisContext);
        }
    }

    private static void LookForBannedMethod(MemberAccessExpressionSyntax memberAccessExpressionSyntax, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        INamedTypeSymbol? typeInfo = ExtractExpressionSyntax(invocation: memberAccessExpressionSyntax, syntaxNodeAnalysisContext: syntaxNodeAnalysisContext);

        if (typeInfo == null)
        {
            return;
        }

        if (typeInfo.EnumUnderlyingType == null)
        {
            // not an enum
            return;
        }

        if (memberAccessExpressionSyntax.Name.Identifier.ToString() == nameof(ToString))
        {
            ReportDiagnostic(expressionSyntax: memberAccessExpressionSyntax, context: syntaxNodeAnalysisContext);
        }
    }

    private static INamedTypeSymbol? ExtractExpressionSyntax(MemberAccessExpressionSyntax invocation, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        ExpressionSyntax e;

        if (invocation.Expression is MemberAccessExpressionSyntax syntax)
        {
            e = syntax;
        }
        else if (invocation.Expression is IdentifierNameSyntax expression)
        {
            e = expression;
        }
        else
        {
            return null;
        }

        INamedTypeSymbol? typeInfo = syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(e)
                                                              .Type as INamedTypeSymbol;

        if (typeInfo?.ConstructedFrom == null)
        {
            return null;
        }

        return typeInfo;
    }

    private static void LookForBannedInInterpolatedStrings(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        if (syntaxNodeAnalysisContext.Node is InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax)
        {
            LookForBannedInInterpolatedStrings(interpolatedStringExpressionSyntax: interpolatedStringExpressionSyntax, syntaxNodeAnalysisContext: syntaxNodeAnalysisContext);
        }
    }

    private static void LookForBannedInInterpolatedStrings(InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        foreach (InterpolationSyntax part in interpolatedStringExpressionSyntax.Contents.OfType<InterpolationSyntax>())
        {
            if (!IsEnum(syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(part.Expression)))
            {
                // not an enum
                continue;
            }

            ReportDiagnostic(expressionSyntax: interpolatedStringExpressionSyntax, context: syntaxNodeAnalysisContext);
        }
    }

    private static void ReportDiagnostic(ExpressionSyntax expressionSyntax, SyntaxNodeAnalysisContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(descriptor: Rule, expressionSyntax.GetLocation()));
    }

    private static bool IsAddExpression(BinaryExpressionSyntax node)
    {
        return node.Kind() == SyntaxKind.AddExpression;
    }

    private static void LookForBannedInStringConcatenation(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        if (syntaxNodeAnalysisContext.Node is BinaryExpressionSyntax binaryExpressionSyntax && IsAddExpression(binaryExpressionSyntax))
        {
            LookForBannedInStringConcatenation(binaryExpressionSyntax: binaryExpressionSyntax, syntaxNodeAnalysisContext: syntaxNodeAnalysisContext);
        }
    }

    private static void LookForBannedInStringConcatenation(BinaryExpressionSyntax binaryExpressionSyntax, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        TypeInfo left = syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(binaryExpressionSyntax.Left);
        TypeInfo right = syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(binaryExpressionSyntax.Right);

        if (IsString(left) && IsEnum(right))
        {
            ReportDiagnostic(expressionSyntax: binaryExpressionSyntax.Left, context: syntaxNodeAnalysisContext);
        }
        else if (IsString(right))
        {
            ReportDiagnostic(expressionSyntax: binaryExpressionSyntax.Right, context: syntaxNodeAnalysisContext);
        }
    }

    private static bool IsString(in TypeInfo typeInfo)
    {
        return typeInfo.Type?.SpecialType == SpecialType.System_String;
    }

    private static bool IsEnum(in TypeInfo typeInfo)
    {
        if (typeInfo.Type is not INamedTypeSymbol type)
        {
            return false;
        }

        return type.EnumUnderlyingType != null;
    }
}