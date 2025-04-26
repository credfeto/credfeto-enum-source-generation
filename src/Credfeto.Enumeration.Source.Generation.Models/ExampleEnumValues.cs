using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Models;

public enum ExampleEnumValues
{
    ZERO = 0,

    [Description("One \"1\"")]
    ONE = 1,

    SAME_AS_ONE = ONE,

    [SuppressMessage(
        category: "SonarAnalyzer.CSharp",
        checkId: "S1133: Remove deprecated code",
        Justification = "Used in tests"
    )]
    [Obsolete("This value is deprecated, use " + nameof(THREE) + " instead.")]
    [SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global", Justification = "Used in tests")]
    TWO = 2,

    [Description("Two but one better!")]
    THREE = 3,
}
