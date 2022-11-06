using System.Diagnostics.CodeAnalysis;
using Credfeto.Enumeration.Source.Generation.Attributes;

namespace Credfeto.Enumeration.Source.Generation.Models;

// [EnumText(typeof(HttpStatusCode))]
[EnumText(typeof(ExampleEnumValues))]
[SuppressMessage(category: "ReSharper", checkId: "PartialTypeWithSinglePart", Justification = "Needed for generated code")]
public static partial class EnumExtensions
{
}