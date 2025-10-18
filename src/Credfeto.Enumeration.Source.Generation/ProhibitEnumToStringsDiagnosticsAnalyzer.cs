using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Extensions;
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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => SupportedDiagnosticsList.Build(Rule);

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
        switch (syntaxNodeAnalysisContext.Node.Kind())
        {
            case SyntaxKind.SimpleMemberAccessExpression:
            case SyntaxKind.PointerMemberAccessExpression: CheckMemberAccessExpression(syntaxNodeAnalysisContext); break;

            case SyntaxKind.InterpolatedStringExpression: CheckInterpolatedStringExpression(syntaxNodeAnalysisContext); break;

            case SyntaxKind.AddExpression: CheckAddExpression(syntaxNodeAnalysisContext); break;

            default:
                Debug.WriteLine("Unexpected syntax kind in ProhibitEnumToStringsDiagnosticsAnalyzer: " + syntaxNodeAnalysisContext.Node.Kind());
                break;
        }
    }

    private static void CheckAddExpression(in SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        if (syntaxNodeAnalysisContext.Node is not BinaryExpressionSyntax binary || !binary.IsKind(SyntaxKind.AddExpression) || binary.Ancestors()
                                                                                                               .OfType<AttributeArgumentSyntax>()
                                                                                                               .Any())
        {
            return;
        }

        TypeInfo leftInfo = syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(node: binary.Left, cancellationToken: syntaxNodeAnalysisContext.CancellationToken);
        TypeInfo rightInfo = syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(node: binary.Right, cancellationToken: syntaxNodeAnalysisContext.CancellationToken);

        if (leftInfo.IsString() && rightInfo.IsEnum())
        {
            RaiseError(syntaxNodeAnalysisContext: syntaxNodeAnalysisContext, syntaxNode: binary.Left);
        }
        else if (rightInfo.IsString() && leftInfo.IsEnum())
        {
            RaiseError(syntaxNodeAnalysisContext: syntaxNodeAnalysisContext, syntaxNode: binary.Right);
        }
    }

    private static void RaiseError(in SyntaxNodeAnalysisContext syntaxNodeAnalysisContext, SyntaxNode syntaxNode)
    {
        RaiseError(syntaxNodeAnalysisContext: syntaxNodeAnalysisContext, location: syntaxNode.GetLocation());
    }

    private static void RaiseError(in SyntaxNodeAnalysisContext syntaxNodeAnalysisContext, Location location)
    {
        syntaxNodeAnalysisContext.ReportDiagnostic(Diagnostic.Create(descriptor: Rule, location: location));
    }

    private static void CheckInterpolatedStringExpression(in SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        if (syntaxNodeAnalysisContext.Node is not InterpolatedStringExpressionSyntax interpolated)
        {
            return;
        }

        if (HasEnumInterpolation(syntaxNodeAnalysisContext: syntaxNodeAnalysisContext, interpolated: interpolated))
        {
            RaiseError(syntaxNodeAnalysisContext: syntaxNodeAnalysisContext, syntaxNode: interpolated);
        }
    }

    private static bool HasEnumInterpolation(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext, InterpolatedStringExpressionSyntax interpolated)
    {
        return interpolated.Contents.OfType<InterpolationSyntax>()
                           .Any(i => syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(node: i.Expression, cancellationToken: syntaxNodeAnalysisContext.CancellationToken)
                                        .IsEnum());
    }

    private static void CheckMemberAccessExpression(in SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
    {
        if (syntaxNodeAnalysisContext.Node is not MemberAccessExpressionSyntax memberAccess || !StringComparer.Ordinal.Equals(x: memberAccess.Name?.Identifier.ValueText, nameof(ToString)))
        {
            return;
        }

        INamedTypeSymbol? type = syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(node: memberAccess.Expression, cancellationToken: syntaxNodeAnalysisContext.CancellationToken)
                                    .Type as INamedTypeSymbol;

        if (type?.EnumUnderlyingType is not null)
        {
            RaiseError(syntaxNodeAnalysisContext: syntaxNodeAnalysisContext, syntaxNode: memberAccess);
        }
    }
}