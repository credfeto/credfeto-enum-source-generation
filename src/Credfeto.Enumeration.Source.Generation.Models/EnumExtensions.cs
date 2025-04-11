using System.Diagnostics.CodeAnalysis;
using System.Net;
using Credfeto.Enumeration.Source.Generation.Attributes;

namespace Credfeto.Enumeration.Source.Generation.Models;

[EnumText(typeof(HttpStatusCode))]
[SuppressMessage(
    category: "ReSharper",
    checkId: "PartialTypeWithSinglePart",
    Justification = "Needed for generated code"
)]
public static partial class EnumExtensions;
