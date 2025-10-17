using System;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Models;

namespace Credfeto.Enumeration.Source.Generation;

public static class EnumSourceGenerator
{
    public static string GenerateClassForEnum(in EnumGeneration enumDeclaration, out CodeBuilder source)
    {
        string className = enumDeclaration.Name + "GeneratedExtensions";

        source = AddUsingDeclarations(new CodeBuilder().AppendFileHeader());

        using (source.AppendLine("namespace " + enumDeclaration.Namespace + ";")
                     .AppendBlankLine()
                     .AppendGeneratedCodeAttribute()
                     .StartBlock(enumDeclaration.AccessType.ConvertAccessType() + " static class " + className))
        {
            source = source.GenerateMethods(attribute: enumDeclaration, classNameFormatter: ClassNameOnlyFormatter);
        }

        return className;
    }

    private static string ClassNameOnlyFormatter(EnumGeneration d)
    {
        return d.Name;
    }

    private static string ClassWithNamespaceFormatter(EnumGeneration d)
    {
        return string.Join(separator: ".", d.Namespace, d.Name);
    }

    private static CodeBuilder AddUsingDeclarations(CodeBuilder source)
    {
        return AddUsingDeclarations(source: source, "System", "System.CodeDom.Compiler", "System.Diagnostics", "System.Diagnostics.CodeAnalysis", "System.Runtime.CompilerServices");
    }

    private static CodeBuilder AddUsingDeclarations(CodeBuilder source, params string[] namespaces)
    {
        return namespaces.OrderBy(keySelector: n => n, comparer: StringComparer.OrdinalIgnoreCase)
                         .Aggregate(seed: source, func: (current, ns) => current.AppendLine($"using {ns};"))
                         .AppendBlankLine();
    }

    public static string GenerateClassForClass(in ClassEnumGeneration classDeclaration, bool hasDoesNotReturn, bool supportsUnreachableException, out CodeBuilder source)
    {
        string className = classDeclaration.Name;

        source = AddUsingDeclarations(new CodeBuilder().AppendFileHeader());

        using (source.AppendLine("namespace " + classDeclaration.Namespace + ";")
                     .AppendBlankLine()
                     .AppendGeneratedCodeAttribute()
                     .StartBlock(classDeclaration.AccessType.ConvertAccessType() + " static partial class " + className))
        {
            Func<EnumGeneration, string> classNameFormatter = ClassWithNamespaceFormatter;

            bool isFirst = true;

            foreach (EnumGeneration attribute in classDeclaration.Enums)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    source = source.AppendBlankLine();
                }

                source = source.GenerateMethods(attribute: attribute, classNameFormatter: classNameFormatter);
            }
        }

        return className;
    }
}