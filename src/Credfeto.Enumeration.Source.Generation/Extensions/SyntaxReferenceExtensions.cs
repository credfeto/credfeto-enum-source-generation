using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class SyntaxReferenceExtensions
{
    [SuppressMessage(
        category: "Meziantou.Analyzer",
        checkId: "MA0045:Use async override",
        Justification = "Calling code is not async"
    )]
    public static EnumMemberDeclarationSyntax? GetDeclaredSyntax(this SyntaxReference syntaxReference)
    {
        return syntaxReference.GetSyntax(CancellationToken.None) as EnumMemberDeclarationSyntax;
    }

}