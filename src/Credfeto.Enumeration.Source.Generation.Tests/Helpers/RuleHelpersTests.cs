using System.Globalization;
using Credfeto.Enumeration.Source.Generation.Helpers;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Helpers;

public sealed class RuleHelpersTests : TestBase
{
    [Fact]
    public void CreateRuleReturnsDescriptorWithCorrectId()
    {
        DiagnosticDescriptor rule = RuleHelpers.CreateRule(
            code: "TEST001",
            category: "TestCategory",
            title: "Test Title",
            message: "Test Message"
        );

        Assert.Equal(expected: "TEST001", actual: rule.Id);
    }

    [Fact]
    public void CreateRuleReturnsDescriptorWithCorrectCategory()
    {
        DiagnosticDescriptor rule = RuleHelpers.CreateRule(
            code: "TEST001",
            category: "MyCategory",
            title: "Test Title",
            message: "Test Message"
        );

        Assert.Equal(expected: "MyCategory", actual: rule.Category);
    }

    [Fact]
    public void CreateRuleReturnsDescriptorWithErrorSeverity()
    {
        DiagnosticDescriptor rule = RuleHelpers.CreateRule(
            code: "TEST001",
            category: "TestCategory",
            title: "Test Title",
            message: "Test Message"
        );

        Assert.Equal(expected: DiagnosticSeverity.Error, actual: rule.DefaultSeverity);
    }

    [Fact]
    public void CreateRuleReturnsDescriptorThatIsEnabledByDefault()
    {
        DiagnosticDescriptor rule = RuleHelpers.CreateRule(
            code: "TEST001",
            category: "TestCategory",
            title: "Test Title",
            message: "Test Message"
        );

        Assert.True(rule.IsEnabledByDefault, "Rule should be enabled by default");
    }

    [Fact]
    public void CreateRuleReturnsDescriptorWithCorrectTitle()
    {
        const string expectedTitle = "My Rule Title";
        DiagnosticDescriptor rule = RuleHelpers.CreateRule(
            code: "TEST001",
            category: "TestCategory",
            title: expectedTitle,
            message: "Test Message"
        );

        Assert.Equal(expected: expectedTitle, actual: rule.Title.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void CreateRuleReturnsDescriptorWithCorrectMessage()
    {
        const string expectedMessage = "My Rule Message";
        DiagnosticDescriptor rule = RuleHelpers.CreateRule(
            code: "TEST001",
            category: "TestCategory",
            title: "Test Title",
            message: expectedMessage
        );

        Assert.Equal(expected: expectedMessage, actual: rule.MessageFormat.ToString(CultureInfo.InvariantCulture));
    }
}
