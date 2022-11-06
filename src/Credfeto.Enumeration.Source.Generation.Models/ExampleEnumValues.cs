using System;
using System.ComponentModel;

namespace Credfeto.Enumeration.Source.Generation.Models;

public enum ExampleEnumValues
{
    ZERO = 0,

    [Description("One \"1\"")]
    ONE = 1,

    SAME_AS_ONE = ONE,

    [Obsolete("This value is deprecated, use " + nameof(THREE) + " instead.")]
    TWO = 2,

    [Description("Two but one better!")]
    THREE = 3
}