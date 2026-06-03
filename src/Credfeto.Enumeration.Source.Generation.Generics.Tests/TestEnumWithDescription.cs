using System.ComponentModel;

namespace Credfeto.Enumeration.Source.Generation.Generics.Tests;

public enum TestEnumWithDescription
{
    WITHOUT_DESCRIPTION = 0,

    [Description("First described value")]
    WITH_DESCRIPTION = 1,
}
