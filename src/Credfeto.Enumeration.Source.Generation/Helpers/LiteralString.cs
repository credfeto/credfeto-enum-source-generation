using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Helpers;

internal sealed class LiteralString : LocalizableString
{
    private readonly string _value;

    public LiteralString(string value)
    {
        this._value = value;
    }

    protected override string GetText(IFormatProvider? formatProvider)
    {
        return this._value;
    }

    [SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0021:Use String Comparer to compute hash codes", Justification = "Not in net stabdard 2.0")]
    protected override int GetHash()
    {
        return this._value.GetHashCode();
    }

    protected override bool AreEqual(object? other)
    {
        return other is LiteralString otherResourceString && StringComparer.OrdinalIgnoreCase.Equals(x: this._value, y: otherResourceString._value);
    }
}