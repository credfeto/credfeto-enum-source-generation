using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Formatting;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using NSubstitute;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Extensions;

public sealed class EnumGenerationExtensionsTests : TestBase
{
    private static IFieldSymbol MakeField(string name, int value, AttributeData? descriptionAttr = null)
    {
        IFieldSymbol field = GetSubstitute<IFieldSymbol>();
        field.Name.Returns(name);
        field.ConstantValue.Returns(value);

        field.GetAttributes().Returns(descriptionAttr is not null ? [descriptionAttr] : []);

        return field;
    }

    private static AttributeData MakeDescriptionAttributeData(string text)
    {
        // Use real Roslyn to get an actual AttributeData for DescriptionAttribute
        string source = $$"""
            using System.ComponentModel;
            public enum TestEnum { [Description("{{text}}")] A }
            """;

        Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation = CompilationHelpers.CreateCompilation(source);
        INamedTypeSymbol? type = compilation.GetTypeByMetadataName("TestEnum");
        return (type ?? throw new InvalidOperationException("TestEnum type not found"))
            .GetMembers("A")
            .OfType<IFieldSymbol>()
            .First()
            .GetAttributes()
            .First();
    }

    [Fact]
    public void GetDescriptionCaseOptionsReturnsEmptyWhenNoMembersHaveDescriptions()
    {
        IFieldSymbol memberA = MakeField("A", 0);
        IFieldSymbol memberB = MakeField("B", 1);
        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);

        EnumGeneration enumGeneration = new(
            accessType: AccessType.PUBLIC,
            name: "TestEnum",
            @namespace: "Test",
            members: [memberA, memberB],
            location: Location.None,
            options: options
        );

        ClassNameOnlyFormatter formatter = new(enumGeneration);
        IReadOnlyList<string> result = enumGeneration.GetDescriptionCaseOptions(formatter);

        Assert.Empty(result);
    }

    [Fact]
    public void GetDescriptionCaseOptionsReturnsItemsForMembersWithDescriptions()
    {
        AttributeData descAttr = MakeDescriptionAttributeData("First Member");
        IFieldSymbol memberA = MakeField("A", 0, descAttr);
        IFieldSymbol memberB = MakeField("B", 1);

        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);

        EnumGeneration enumGeneration = new(
            accessType: AccessType.PUBLIC,
            name: "TestEnum",
            @namespace: "Test",
            members: [memberA, memberB],
            location: Location.None,
            options: options
        );

        ClassNameOnlyFormatter formatter = new(enumGeneration);
        IReadOnlyList<string> result = enumGeneration.GetDescriptionCaseOptions(formatter);

        Assert.Single(result);
        Assert.Contains("\"First Member\"", result[0], System.StringComparison.Ordinal);
    }

    [Fact]
    public void GetDescriptionCaseOptionsSkipsObsoleteMembers()
    {
        AttributeData descAttr = MakeDescriptionAttributeData("Something");

        IFieldSymbol obsoleteMember = GetSubstitute<IFieldSymbol>();
        obsoleteMember.Name.Returns("OBSOLETE_A");
        obsoleteMember.ConstantValue.Returns(0);

        // Get a real ObsoleteAttribute AttributeData
        const string obsoleteSource = """
            using System;
            public enum ObsEnum { [Obsolete] A }
            """;
        Microsoft.CodeAnalysis.CSharp.CSharpCompilation obsoleteComp = CompilationHelpers.CreateCompilation(
            obsoleteSource
        );
        INamedTypeSymbol? obsType = obsoleteComp.GetTypeByMetadataName("ObsEnum");
        AttributeData obsoleteAttr = (obsType ?? throw new InvalidOperationException("ObsEnum type not found"))
            .GetMembers("A")
            .OfType<IFieldSymbol>()
            .First()
            .GetAttributes()
            .First();
        obsoleteMember.GetAttributes().Returns([obsoleteAttr]);

        IFieldSymbol memberB = MakeField("B", 1, descAttr);

        GenerationOptions options = new(hasDoesNotReturnAttribute: false, supportsUnreachableException: false);

        EnumGeneration enumGeneration = new(
            accessType: AccessType.PUBLIC,
            name: "TestEnum",
            @namespace: "Test",
            members: [obsoleteMember, memberB],
            location: Location.None,
            options: options
        );

        ClassNameOnlyFormatter formatter = new(enumGeneration);
        IReadOnlyList<string> result = enumGeneration.GetDescriptionCaseOptions(formatter);

        // Only memberB (B) should appear — obsoleteMember is filtered out
        Assert.Single(result);
        Assert.Contains("B", result[0], System.StringComparison.Ordinal);
    }
}
