using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Models.Tests;

[SuppressMessage(
    category: "FunFair.CodeAnalysis",
    checkId: "FFS0013: Test classes should be derived from TestBase",
    Justification = "Not in this case"
)]
public sealed class EnumGetNameTests
{
    [Theory]
    [InlineData(ExampleEnumValues.ZERO, nameof(ExampleEnumValues.ZERO))]
    [InlineData(ExampleEnumValues.ONE, nameof(ExampleEnumValues.ONE))]
    [InlineData(ExampleEnumValues.THREE, nameof(ExampleEnumValues.THREE))]
    public void GetNameReturnsExpectedName(ExampleEnumValues value, string expected)
    {
        string name = value.GetName();

        Assert.Equal(expected: expected, actual: name);
    }

    [Fact]
    public void GetNameForAliasedMatchesOriginal()
    {
        Assert.Equal(expected: ExampleEnumValues.ONE.GetName(), actual: ExampleEnumValues.SAME_AS_ONE.GetName());
    }

    [Fact]
    public void GetNameForObsoleteValueThrows()
    {
        // ExampleEnumValues.TWO (value 2) is not included in the generated switch — treat as out-of-range
        const ExampleEnumValues twoByValue = (ExampleEnumValues)2;
#if NET7_0_OR_GREATER
        Assert.Throws<UnreachableException>(() => twoByValue.GetName());
#else
        Assert.Throws<ArgumentOutOfRangeException>(() => twoByValue.GetName());
#endif
    }

    [Fact]
    public void GetNameForUnknownValueThrows()
    {
        const ExampleEnumValues unknown = (ExampleEnumValues)72;

#if NET7_0_OR_GREATER
        Assert.Throws<UnreachableException>(() => unknown.GetName());
#else
        Assert.Throws<ArgumentOutOfRangeException>(() => unknown.GetName());
#endif
    }

    [Theory]
    [InlineData(HttpStatusCode.Continue, nameof(HttpStatusCode.Continue))]
    [InlineData(HttpStatusCode.SwitchingProtocols, nameof(HttpStatusCode.SwitchingProtocols))]
    [InlineData(HttpStatusCode.Processing, nameof(HttpStatusCode.Processing))]
    [InlineData(HttpStatusCode.EarlyHints, nameof(HttpStatusCode.EarlyHints))]
    [InlineData(HttpStatusCode.OK, nameof(HttpStatusCode.OK))]
    [InlineData(HttpStatusCode.Created, nameof(HttpStatusCode.Created))]
    [InlineData(HttpStatusCode.Accepted, nameof(HttpStatusCode.Accepted))]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation, nameof(HttpStatusCode.NonAuthoritativeInformation))]
    [InlineData(HttpStatusCode.NoContent, nameof(HttpStatusCode.NoContent))]
    [InlineData(HttpStatusCode.ResetContent, nameof(HttpStatusCode.ResetContent))]
    [InlineData(HttpStatusCode.PartialContent, nameof(HttpStatusCode.PartialContent))]
    [InlineData(HttpStatusCode.MultiStatus, nameof(HttpStatusCode.MultiStatus))]
    [InlineData(HttpStatusCode.AlreadyReported, nameof(HttpStatusCode.AlreadyReported))]
    [InlineData(HttpStatusCode.IMUsed, nameof(HttpStatusCode.IMUsed))]
    [InlineData(HttpStatusCode.Ambiguous, nameof(HttpStatusCode.Ambiguous))]
    [InlineData(HttpStatusCode.Moved, nameof(HttpStatusCode.Moved))]
    [InlineData(HttpStatusCode.Found, nameof(HttpStatusCode.Found))]
    [InlineData(HttpStatusCode.RedirectMethod, nameof(HttpStatusCode.RedirectMethod))]
    [InlineData(HttpStatusCode.NotModified, nameof(HttpStatusCode.NotModified))]
    [InlineData(HttpStatusCode.UseProxy, nameof(HttpStatusCode.UseProxy))]
    [InlineData(HttpStatusCode.Unused, nameof(HttpStatusCode.Unused))]
    [InlineData(HttpStatusCode.RedirectKeepVerb, nameof(HttpStatusCode.RedirectKeepVerb))]
    [InlineData(HttpStatusCode.PermanentRedirect, nameof(HttpStatusCode.PermanentRedirect))]
    [InlineData(HttpStatusCode.BadRequest, nameof(HttpStatusCode.BadRequest))]
    [InlineData(HttpStatusCode.Unauthorized, nameof(HttpStatusCode.Unauthorized))]
    [InlineData(HttpStatusCode.PaymentRequired, nameof(HttpStatusCode.PaymentRequired))]
    [InlineData(HttpStatusCode.Forbidden, nameof(HttpStatusCode.Forbidden))]
    [InlineData(HttpStatusCode.NotFound, nameof(HttpStatusCode.NotFound))]
    [InlineData(HttpStatusCode.MethodNotAllowed, nameof(HttpStatusCode.MethodNotAllowed))]
    [InlineData(HttpStatusCode.NotAcceptable, nameof(HttpStatusCode.NotAcceptable))]
    [InlineData(HttpStatusCode.ProxyAuthenticationRequired, nameof(HttpStatusCode.ProxyAuthenticationRequired))]
    [InlineData(HttpStatusCode.RequestTimeout, nameof(HttpStatusCode.RequestTimeout))]
    [InlineData(HttpStatusCode.Conflict, nameof(HttpStatusCode.Conflict))]
    [InlineData(HttpStatusCode.Gone, nameof(HttpStatusCode.Gone))]
    [InlineData(HttpStatusCode.LengthRequired, nameof(HttpStatusCode.LengthRequired))]
    [InlineData(HttpStatusCode.PreconditionFailed, nameof(HttpStatusCode.PreconditionFailed))]
    [InlineData(HttpStatusCode.RequestEntityTooLarge, nameof(HttpStatusCode.RequestEntityTooLarge))]
    [InlineData(HttpStatusCode.RequestUriTooLong, nameof(HttpStatusCode.RequestUriTooLong))]
    [InlineData(HttpStatusCode.UnsupportedMediaType, nameof(HttpStatusCode.UnsupportedMediaType))]
    [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable, nameof(HttpStatusCode.RequestedRangeNotSatisfiable))]
    [InlineData(HttpStatusCode.ExpectationFailed, nameof(HttpStatusCode.ExpectationFailed))]
    [InlineData(HttpStatusCode.MisdirectedRequest, nameof(HttpStatusCode.MisdirectedRequest))]
    [InlineData(HttpStatusCode.UnprocessableEntity, nameof(HttpStatusCode.UnprocessableEntity))]
    [InlineData(HttpStatusCode.Locked, nameof(HttpStatusCode.Locked))]
    [InlineData(HttpStatusCode.FailedDependency, nameof(HttpStatusCode.FailedDependency))]
    [InlineData(HttpStatusCode.UpgradeRequired, nameof(HttpStatusCode.UpgradeRequired))]
    [InlineData(HttpStatusCode.PreconditionRequired, nameof(HttpStatusCode.PreconditionRequired))]
    [InlineData(HttpStatusCode.TooManyRequests, nameof(HttpStatusCode.TooManyRequests))]
    [InlineData(HttpStatusCode.RequestHeaderFieldsTooLarge, nameof(HttpStatusCode.RequestHeaderFieldsTooLarge))]
    [InlineData(HttpStatusCode.UnavailableForLegalReasons, nameof(HttpStatusCode.UnavailableForLegalReasons))]
    [InlineData(HttpStatusCode.InternalServerError, nameof(HttpStatusCode.InternalServerError))]
    [InlineData(HttpStatusCode.NotImplemented, nameof(HttpStatusCode.NotImplemented))]
    [InlineData(HttpStatusCode.BadGateway, nameof(HttpStatusCode.BadGateway))]
    [InlineData(HttpStatusCode.ServiceUnavailable, nameof(HttpStatusCode.ServiceUnavailable))]
    [InlineData(HttpStatusCode.GatewayTimeout, nameof(HttpStatusCode.GatewayTimeout))]
    [InlineData(HttpStatusCode.HttpVersionNotSupported, nameof(HttpStatusCode.HttpVersionNotSupported))]
    [InlineData(HttpStatusCode.VariantAlsoNegotiates, nameof(HttpStatusCode.VariantAlsoNegotiates))]
    [InlineData(HttpStatusCode.InsufficientStorage, nameof(HttpStatusCode.InsufficientStorage))]
    [InlineData(HttpStatusCode.LoopDetected, nameof(HttpStatusCode.LoopDetected))]
    [InlineData(HttpStatusCode.NotExtended, nameof(HttpStatusCode.NotExtended))]
    [InlineData(HttpStatusCode.NetworkAuthenticationRequired, nameof(HttpStatusCode.NetworkAuthenticationRequired))]
    public void GetNameForExternalEnumReturnsMemberName(HttpStatusCode value, string expected)
    {
        Assert.Equal(expected: expected, actual: value.GetName());
    }

    [Fact]
    public void GetNameForOutOfRangeHttpStatusCodeThrows()
    {
        const HttpStatusCode unknown = (HttpStatusCode)9999;
        Assert.Throws<UnreachableException>(() => unknown.GetName());
    }
}
