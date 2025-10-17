using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation;

internal sealed class ClassNameOnlyFormatter : IFormatConfig
{
    public ClassNameOnlyFormatter(in EnumGeneration enumGeneration)
    {
        this.ClassName = enumGeneration.Name;
    }

    public string ClassName { get; }
}