using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Helpers;
using Credfeto.Enumeration.Source.Generation.Models;
using Credfeto.Enumeration.Source.Generation.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation;

[Generator]
public sealed class EnumGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not EnumSyntaxReceiver receiver)
        {
            return;
        }

        foreach (EnumGeneration enumDeclaration in receiver.Enums)
        {
            GenerateClassForEnum(context: context, enumDeclaration: enumDeclaration);
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new EnumSyntaxReceiver());
    }

    private static void GenerateClassForEnum(in GeneratorExecutionContext context, EnumGeneration enumDeclaration)
    {
        string className = enumDeclaration.Name + "GeneratedExtensions";

        CodeBuilder source = new();

        using (source.AppendLine("using System;")
                     .AppendLine("using System.CodeDom.Compiler;")
                     .AppendLine("using System.Diagnostics.CodeAnalysis;")
                     .AppendLine("using System.Runtime.CompilerServices;")
                     .AppendBlankLine()
                     .AppendLine("namespace " + enumDeclaration.Namespace + ";")
                     .AppendBlankLine()
                     .AppendLine($"[GeneratedCode(tool: \"{typeof(EnumGenerator).FullName}\", version: \"{ExecutableVersionInformation.ProgramVersion()}\")]")
                     .StartBlock(ConvertAccessType(enumDeclaration.AccessType) + " static class " + className))
        {
            GenerateGetName(source: source, enumDeclaration: enumDeclaration);
            GenerateGetDescription(source: source, enumDeclaration: enumDeclaration);
        }

        context.AddSource(enumDeclaration.Namespace + "." + className, sourceText: source.Text);
    }

    private static void GenerateGetName(CodeBuilder source, EnumGeneration enumDeclaration)
    {
        using (source.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                     .StartBlock("public static string GetName(this " + enumDeclaration.Name + " value)"))
        {
            using (source.StartBlock(text: "return value switch", start: "{", end: "};"))
            {
                ImmutableHashSet<string> names = UniqueEnumMemberNames(enumDeclaration);

                foreach (EnumMemberDeclarationSyntax member in enumDeclaration.Members)
                {
                    if (IsSkipEnumValue(member: member, names: names) || IsObsolete(member))
                    {
                        continue;
                    }

                    source.AppendLine(enumDeclaration.Name + "." + member.Identifier.Text + " => \"" + member.Identifier.Text + "\",");
                }

                source.AppendLine("_ => throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: \"Unknown enum member\")");
            }
        }
    }

    private static void GenerateGetDescription(CodeBuilder source, EnumGeneration enumDeclaration)
    {
        using (source.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                     .StartBlock("public static string GetDescription(this " + enumDeclaration.Name + " value)"))
        {
            IReadOnlyList<string> items = GetDescriptionCaseOptions(enumDeclaration);

            if (items.Count == 0)
            {
                source.AppendLine("return GetName(value);");
            }
            else
            {
                using (source.StartBlock(text: "return value switch", start: "{", end: "};"))
                {
                    foreach (string line in items)
                    {
                        source.AppendLine(line);
                    }

                    source.AppendLine("_ => GetName(value)");
                }
            }
        }
    }

    private static IReadOnlyList<string> GetDescriptionCaseOptions(EnumGeneration enumDeclaration)
    {
        List<string> items = new();

        ImmutableHashSet<string> names = UniqueEnumMemberNames(enumDeclaration);

        foreach (EnumMemberDeclarationSyntax member in enumDeclaration.Members)
        {
            if (IsSkipEnumValue(member: member, names: names) || IsObsolete(member))
            {
                continue;
            }

            AttributeSyntax? description = member.AttributeLists.SelectMany(x => x.Attributes)
                                                 .FirstOrDefault(IsDescriptionAttribute);

            if (description == null)
            {
                continue;
            }

            string? attributeText = description.ArgumentList?.Arguments.FirstOrDefault()
                                               ?.Expression.ToString();

            if (string.IsNullOrWhiteSpace(attributeText))
            {
                continue;
            }

            items.Add(enumDeclaration.Name + "." + member.Identifier.Text + " => " + attributeText + ",");
        }

        return items;
    }

    private static bool IsSkipEnumValue(EnumMemberDeclarationSyntax member, ImmutableHashSet<string> names)
    {
        return member.EqualsValue?.Value.Kind() == SyntaxKind.IdentifierName && names.Contains(member.EqualsValue.Value.ToString());
    }

    private static bool IsDescriptionAttribute(AttributeSyntax attribute)
    {
        return StringComparer.Ordinal.Equals(attribute.Name.ToString(), y: "Description") || StringComparer.Ordinal.Equals(attribute.Name.ToString(), y: "DescriptionAttribute");
    }

    private static bool IsObsolete(EnumMemberDeclarationSyntax member)
    {
        List<string> a = member.AttributeLists.SelectMany(x => x.Attributes)
                               .Select(x => x.Name.ToString())
                               .ToList();
        bool isObsolete = a.Contains(value: "Obsolete", comparer: StringComparer.Ordinal) || a.Contains(value: "ObsoleteAttribute", comparer: StringComparer.Ordinal);

        return isObsolete;
    }

    private static ImmutableHashSet<string> UniqueEnumMemberNames(EnumGeneration enumDeclaration)
    {
        return enumDeclaration.Members.Select(m => m.Identifier.Text)
                              .Distinct(StringComparer.Ordinal)
                              .ToImmutableHashSet(StringComparer.Ordinal);
    }

    private static string ConvertAccessType(AccessType accessType)
    {
        return accessType switch
        {
            AccessType.PUBLIC => "public",
            AccessType.PRIVATE => "private",
            AccessType.PROTECTED => "protected",
            AccessType.PROTECTED_INTERNAL => "protected internal",
            AccessType.INTERNAL => "internal",
            _ => throw new ArgumentOutOfRangeException(nameof(accessType), actualValue: accessType, message: "Unknown access type")
        };
    }
}