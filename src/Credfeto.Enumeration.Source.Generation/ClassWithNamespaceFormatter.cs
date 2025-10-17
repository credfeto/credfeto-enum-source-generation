using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation;

internal sealed class ClassWithNamespaceFormatter : IFormatConfig
{
    public ClassWithNamespaceFormatter(in EnumGeneration enumGeneration)
    {
        this.ClassName = string.Join(separator: ".", enumGeneration.Namespace, enumGeneration.Name);
    }

    public string ClassName { get; }
}