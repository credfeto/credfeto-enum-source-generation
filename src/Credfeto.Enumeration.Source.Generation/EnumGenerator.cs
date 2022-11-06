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

        // foreach (EnumGeneration enumDeclaration in receiver.Enums)
        // {
        //     GenerateClassForEnum(context: context, enumDeclaration: enumDeclaration);
        // }

        foreach (ClassEnumGeneration classDeclaration in receiver.Classes)
        {
            GenerateClassForClass(context: context, classDeclaration: classDeclaration);
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

    private static void GenerateClassForClass(in GeneratorExecutionContext context, ClassEnumGeneration classDeclaration)
    {
        string className = classDeclaration.Name + "GeneratedExtensions";

        static string ClassNameFormatter(EnumGeneration2 d)
        {
            return string.Join(separator: ".", d.Namespace, d.Name);
        }

        CodeBuilder source = new();

        using (source.AppendLine("using System;")
                     .AppendLine("using System.CodeDom.Compiler;")
                     .AppendLine("using System.Diagnostics.CodeAnalysis;")
                     .AppendLine("using System.Runtime.CompilerServices;")
                     .AppendBlankLine()
                     .AppendLine("namespace " + classDeclaration.Namespace + ";")
                     .AppendBlankLine()
                     .AppendLine($"[GeneratedCode(tool: \"{typeof(EnumGenerator).FullName}\", version: \"{ExecutableVersionInformation.ProgramVersion()}\")]")
                     .StartBlock(ConvertAccessType(classDeclaration.AccessType) + " static partial class " + className))
        {
            foreach (EnumGeneration2 attribute in classDeclaration.Enums)
            {
                source.AppendLine($"// {attribute.Namespace}.{attribute.Name}");

                GenerateGetName(source: source, enumDeclaration: attribute, classNameFormatter: ClassNameFormatter);
                GenerateGetDescription(source: source, enumDeclaration: attribute, classNameFormatter: ClassNameFormatter);
            }
        }

        context.AddSource(classDeclaration.Namespace + "." + className, sourceText: source.Text);
    }

    private static void GenerateGetName(CodeBuilder source, EnumGeneration2 enumDeclaration, Func<EnumGeneration2, string> classNameFormatter)
    {
        string className = classNameFormatter(enumDeclaration);

        using (source.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                     .StartBlock("public static string GetName(this " + className + " value)"))
        {
            using (source.StartBlock(text: "return value switch", start: "{", end: "};"))
            {
                ImmutableHashSet<string> names = UniqueEnumMemberNames(enumDeclaration);

                foreach (IFieldSymbol member in enumDeclaration.Members)
                {
                    if (IsSkipEnumValue(member: member, names: names) || IsObsolete(member))
                    {
                        continue;
                    }

                    source.AppendLine(className + "." + member.Name + " => \"" + member.Name + "\",");
                }

                source.AppendLine("_ => throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: \"Unknown enum member\")");
            }
        }
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

    private static void GenerateGetDescription(CodeBuilder source, EnumGeneration2 enumDeclaration, Func<EnumGeneration2, string> classNameFormatter)
    {
        string className = classNameFormatter(enumDeclaration);

        using (source.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                     .StartBlock("public static string GetDescription(this " + className + " value)"))
        {
            IReadOnlyList<string> items = GetDescriptionCaseOptions(enumDeclaration: enumDeclaration, classNameFormatter: classNameFormatter);

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

    private static IReadOnlyList<string> GetDescriptionCaseOptions(EnumGeneration2 enumDeclaration, Func<EnumGeneration2, string> classNameFormatter)
    {
        List<string> items = new();

        ImmutableHashSet<string> names = UniqueEnumMemberNames(enumDeclaration);

        foreach (IFieldSymbol? member in enumDeclaration.Members)
        {
            if (IsSkipEnumValue(member: member, names: names) || IsObsolete(member))
            {
                continue;
            }

            AttributeData? description = member.GetAttributes()
                                               .FirstOrDefault(IsDescriptionAttribute);

            if (description == null)
            {
                continue;
            }

            TypedConstant? tc = description.ConstructorArguments.FirstOrDefault();

            string attributeText = tc.Value.ToCSharpString();

            if (string.IsNullOrWhiteSpace(attributeText))
            {
                continue;
            }

            items.Add(classNameFormatter(enumDeclaration) + "." + member.Name + " => " + attributeText + ",");
        }

        return items;
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

    private static bool IsSkipEnumValue(ISymbol member, ImmutableHashSet<string> names)
    {
        EnumMemberDeclarationSyntax? syntax = FindEnumMemberDeclarationSyntax(member);

        if (syntax == null)
        {
            return false;
        }

        return IsSkipEnumValue(member: syntax, names: names);
    }

    private static EnumMemberDeclarationSyntax? FindEnumMemberDeclarationSyntax(ISymbol member)
    {
        EnumMemberDeclarationSyntax? syntax = null;

        foreach (SyntaxReference dsr in member.DeclaringSyntaxReferences)
        {
            syntax = dsr.GetSyntax() as EnumMemberDeclarationSyntax;

            if (syntax != null)
            {
                break;
            }
        }

        return syntax;
    }

    private static bool IsSkipEnumValue(EnumMemberDeclarationSyntax member, ImmutableHashSet<string> names)
    {
        return member.EqualsValue?.Value.Kind() == SyntaxKind.IdentifierName && names.Contains(member.EqualsValue.Value.ToString());
    }

    private static bool IsDescriptionAttribute(AttributeData attribute)
    {
        return StringComparer.Ordinal.Equals(x: attribute.AttributeClass?.Name, y: "Description") || StringComparer.Ordinal.Equals(x: attribute.AttributeClass?.Name, y: "DescriptionAttribute");
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

    private static bool IsObsolete(ISymbol member)
    {
        List<string> a = member.GetAttributes()
                               .Select(x => x.AttributeClass!.Name)
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

    private static ImmutableHashSet<string> UniqueEnumMemberNames(EnumGeneration2 enumDeclaration)
    {
        return enumDeclaration.Members.Select(m => m.Name)
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