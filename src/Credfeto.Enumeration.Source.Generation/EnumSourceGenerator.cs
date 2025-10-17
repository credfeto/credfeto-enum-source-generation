using System;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Formatting;
using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation;

public static class EnumSourceGenerator
{
    public static string GenerateClassForEnum(in EnumGeneration enumDeclaration, out CodeBuilder source)
    {
        string className = enumDeclaration.Name + "GeneratedExtensions";

        source = AddUsingDeclarations(new CodeBuilder().AppendFileHeader());

        using (
            source
                .AppendLine("namespace " + enumDeclaration.Namespace + ";")
                .AppendBlankLine()
                .AppendGeneratedCodeAttribute()
                .StartBlock(enumDeclaration.AccessType.ConvertAccessType() + " static class " + className)
        )
        {
            ClassNameOnlyFormatter classNameOnlyFormatter = new(enumDeclaration);

            source = source.GenerateMethods(attribute: enumDeclaration, formatConfig: classNameOnlyFormatter);
        }

        return className;
    }

    private static CodeBuilder AddUsingDeclarations(CodeBuilder source)
    {
        return AddUsingDeclarations(
            source: source,
            "System",
            "System.CodeDom.Compiler",
            "System.Diagnostics",
            "System.Diagnostics.CodeAnalysis",
            "System.Runtime.CompilerServices"
        );
    }

    private static CodeBuilder AddUsingDeclarations(CodeBuilder source, params string[] namespaces)
    {
        return namespaces
            .OrderBy(keySelector: n => n, comparer: StringComparer.OrdinalIgnoreCase)
            .Aggregate(seed: source, func: (current, ns) => current.AppendLine($"using {ns};"))
            .AppendBlankLine();
    }

    public static string GenerateClassForClass(in ClassEnumGeneration classDeclaration, out CodeBuilder source)
    {
        string className = classDeclaration.Name;

        source = AddUsingDeclarations(new CodeBuilder().AppendFileHeader())
            .AppendLine($"namespace {classDeclaration.Namespace};")
            .AppendBlankLine()
            .AppendGeneratedCodeAttribute();

        using (source.StartBlock($"{classDeclaration.AccessType.ConvertAccessType()} static partial class {className}"))
        {
            bool isFirst = true;

            source = classDeclaration.Enums.Aggregate(
                seed: source,
                func: (builder, enumGeneration) =>
                {
                    try
                    {
                        return GenerateMethodsForEnum(
                            builder: builder,
                            enumGeneration: enumGeneration,
                            isFirst: isFirst
                        );
                    }
                    finally
                    {
                        isFirst = false;
                    }
                }
            );
        }

        return className;
    }

    private static CodeBuilder GenerateMethodsForEnum(
        CodeBuilder builder,
        in EnumGeneration enumGeneration,
        bool isFirst
    )
    {
        IFormatConfig formatConfig = new ClassWithNamespaceFormatter(enumGeneration);

        return isFirst
            ? builder.AppendBlankLine().GenerateMethods(attribute: enumGeneration, formatConfig: formatConfig)
            : builder.GenerateMethods(attribute: enumGeneration, formatConfig: formatConfig);
    }
}
