using System;
using Credfeto.Enumeration.Source.Generation.Models;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Models;

public sealed class ErrorInfoTests : TestBase
{
    [Fact]
    public void ConstructorSetsLocationAndException()
    {
        Location location = Location.None;
        Exception exception = new InvalidOperationException("test error");

        ErrorInfo errorInfo = new(location: location, exception: exception);

        Assert.Same(expected: location, actual: errorInfo.Location);
        Assert.Same(expected: exception, actual: errorInfo.Exception);
    }

    [Fact]
    public void EqualityReturnsTrueForSameValues()
    {
        Location location = Location.None;
        Exception exception = new InvalidOperationException("test error");

        ErrorInfo first = new(location: location, exception: exception);
        ErrorInfo second = new(location: location, exception: exception);

        Assert.Equal(expected: first, actual: second);
    }

    [Fact]
    public void EqualityReturnsFalseForDifferentException()
    {
        Location location = Location.None;
        Exception first = new InvalidOperationException("first");
        Exception second = new InvalidOperationException("second");

        ErrorInfo firstInfo = new(location: location, exception: first);
        ErrorInfo secondInfo = new(location: location, exception: second);

        Assert.NotEqual(expected: firstInfo, actual: secondInfo);
    }
}
