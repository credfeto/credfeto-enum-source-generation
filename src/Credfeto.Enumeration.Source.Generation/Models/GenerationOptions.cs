using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Credfeto.Enumeration.Source.Generation.Models;

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("DoesNotReturn: {HasDoesNotReturnAttribute}, UnreachableException: {SupportsUnreachableException}")]
public readonly record struct GenerationOptions
{
    public GenerationOptions(bool hasDoesNotReturnAttribute, bool supportsUnreachableException)
    {
        this.HasDoesNotReturnAttribute = hasDoesNotReturnAttribute;
        this.SupportsUnreachableException = supportsUnreachableException;
    }

    public bool HasDoesNotReturnAttribute { get; }

    public bool SupportsUnreachableException { get; }
}