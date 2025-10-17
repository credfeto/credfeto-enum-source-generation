using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class ExpressionSyntaxExtensions
{
    public static bool IsSkipEnumValue(this ExpressionSyntax expressionSyntax, HashSet<string> names)
    {
        return expressionSyntax.IsKind(SyntaxKind.IdentifierName) && names.Contains(expressionSyntax.ToString());
    }
   
}