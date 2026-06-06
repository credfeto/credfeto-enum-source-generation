using System.Linq;
using System.Threading.Tasks;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Extensions;

public sealed class TypeDeclarationSyntaxExtensionsTests : TestBase
{
    private static async Task<EnumDeclarationSyntax> ParseEnumAsync(string source)
    {
        return (
            await CSharpSyntaxTree
                .ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
                .GetRootAsync(TestContext.Current.CancellationToken)
        )
            .DescendantNodes()
            .OfType<EnumDeclarationSyntax>()
            .First();
    }

    private static async Task<ClassDeclarationSyntax> ParseClassAsync(string source)
    {
        return (
            await CSharpSyntaxTree
                .ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
                .GetRootAsync(TestContext.Current.CancellationToken)
        )
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Last();
    }

    [Fact]
    public async Task EnumWithPublicKeywordReturnsPublic()
    {
        EnumDeclarationSyntax decl = await ParseEnumAsync("public enum E { }");
        Assert.Equal(expected: AccessType.PUBLIC, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task EnumWithPrivateKeywordReturnsPrivate()
    {
        EnumDeclarationSyntax decl = await ParseEnumAsync("class Outer { private enum E { } }");
        Assert.Equal(expected: AccessType.PRIVATE, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task EnumWithProtectedKeywordReturnsProtected()
    {
        EnumDeclarationSyntax decl = await ParseEnumAsync("class Outer { protected enum E { } }");
        Assert.Equal(expected: AccessType.PROTECTED, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task EnumWithProtectedInternalKeywordReturnsProtectedInternal()
    {
        EnumDeclarationSyntax decl = await ParseEnumAsync("class Outer { protected internal enum E { } }");
        Assert.Equal(expected: AccessType.PROTECTED_INTERNAL, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task EnumWithInternalKeywordReturnsInternal()
    {
        EnumDeclarationSyntax decl = await ParseEnumAsync("internal enum E { }");
        Assert.Equal(expected: AccessType.INTERNAL, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task EnumWithNoModifiersReturnsInternal()
    {
        EnumDeclarationSyntax decl = await ParseEnumAsync("enum E { }");
        Assert.Equal(expected: AccessType.INTERNAL, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task ClassWithPublicKeywordReturnsPublic()
    {
        ClassDeclarationSyntax decl = await ParseClassAsync("public class C { }");
        Assert.Equal(expected: AccessType.PUBLIC, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task ClassWithPrivateKeywordReturnsPrivate()
    {
        ClassDeclarationSyntax decl = await ParseClassAsync("class Outer { private class C { } }");
        Assert.Equal(expected: AccessType.PRIVATE, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task ClassWithProtectedKeywordReturnsProtected()
    {
        ClassDeclarationSyntax decl = await ParseClassAsync("class Outer { protected class C { } }");
        Assert.Equal(expected: AccessType.PROTECTED, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task ClassWithProtectedInternalKeywordReturnsProtectedInternal()
    {
        ClassDeclarationSyntax decl = await ParseClassAsync("class Outer { protected internal class C { } }");
        Assert.Equal(expected: AccessType.PROTECTED_INTERNAL, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task ClassWithInternalKeywordReturnsInternal()
    {
        ClassDeclarationSyntax decl = await ParseClassAsync("internal class C { }");
        Assert.Equal(expected: AccessType.INTERNAL, actual: decl.GetAccessType());
    }

    [Fact]
    public async Task ClassWithNoModifiersReturnsInternal()
    {
        ClassDeclarationSyntax decl = await ParseClassAsync("class C { }");
        Assert.Equal(expected: AccessType.INTERNAL, actual: decl.GetAccessType());
    }
}
