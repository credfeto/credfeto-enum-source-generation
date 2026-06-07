using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

public sealed class AccessTypeTests : TestBase
{
    [Fact]
    public void PublicEqualsPublicKeywordSyntaxKind()
    {
        Assert.Equal(expected: (int)SyntaxKind.PublicKeyword, actual: (int)AccessType.PUBLIC);
    }

    [Fact]
    public void PrivateEqualsPrivateKeywordSyntaxKind()
    {
        Assert.Equal(expected: (int)SyntaxKind.PrivateKeyword, actual: (int)AccessType.PRIVATE);
    }

    [Fact]
    public void ProtectedEqualsProtectedKeywordSyntaxKind()
    {
        Assert.Equal(expected: (int)SyntaxKind.ProtectedKeyword, actual: (int)AccessType.PROTECTED);
    }

    [Fact]
    public void InternalEqualsInternalKeywordSyntaxKind()
    {
        Assert.Equal(expected: (int)SyntaxKind.InternalKeyword, actual: (int)AccessType.INTERNAL);
    }

    [Fact]
    public void ProtectedInternalEqualsProtectedKeywordOrInternalKeywordSyntaxKind()
    {
        Assert.Equal(
            expected: (int)SyntaxKind.ProtectedKeyword | (int)SyntaxKind.InternalKeyword,
            actual: (int)AccessType.PROTECTED_INTERNAL
        );
    }
}
