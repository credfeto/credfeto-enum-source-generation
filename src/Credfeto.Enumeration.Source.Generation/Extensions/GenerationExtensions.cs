using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Formatting;
using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class GenerationExtensions
{
    private const string IS_DEFINED_METHOD_NAME = nameof(Enum.IsDefined);
    private const string GET_NAME_METHOD_NAME = "GetName";
    private const string GET_DESCRIPTION_METHOD_NAME = "GetDescription";

    private const string INVALID_ENUM_MEMBER_METHOD_NAME = "ThrowInvalidEnumMemberException";

    public static CodeBuilder GenerateMethods(
        this CodeBuilder source,
        in EnumGeneration attribute,
        IFormatConfig formatConfig
    )
    {
        return source
            .GenerateGetName(enumDeclaration: attribute, formatConfig: formatConfig)
            .AppendBlankLine()
            .GenerateGetDescription(enumDeclaration: attribute, formatConfig: formatConfig)
            .AppendBlankLine()
            .GenerateIsDefined(enumDeclaration: attribute, formatConfig: formatConfig)
            .AppendBlankLine()
            .GenerateThrowNotFound(enumDeclaration: attribute, formatConfig: formatConfig);
    }

    private static CodeBuilder GenerateIsDefined(
        this CodeBuilder source,
        in EnumGeneration enumDeclaration,
        IFormatConfig formatConfig
    )
    {
        string className = formatConfig.ClassName;

        using (
            source
                .AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                .StartBlock($"public static bool {IS_DEFINED_METHOD_NAME}(this {className} value)")
        )
        {
            IReadOnlyList<string> members = enumDeclaration.GetUniqueMemberNames(className: className);

            return source.AppendLine(
                members.Count switch
                {
                    0 => "return false;",
                    1 => $"return value == {members[0]};",
                    _ => $"return value is {string.Join(separator: " or ", values: members)};",
                }
            );
        }
    }

    private static CodeBuilder GenerateGetName(
        this CodeBuilder source,
        in EnumGeneration enumDeclaration,
        IFormatConfig formatConfig
    )
    {
        string className = formatConfig.ClassName;

        using (
            source
                .AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                .StartBlock($"public static string {GET_NAME_METHOD_NAME}(this {className} value)")
        )
        {
            IReadOnlyList<string> members = enumDeclaration.GetUniqueMemberNames();

            if (members.Count == 0)
            {
                return source.AppendLine($"return {INVALID_ENUM_MEMBER_METHOD_NAME}(value: value);");
            }

            using (source.StartBlock(text: "return value switch", start: "{", end: "};"))
            {
                return members
                    .Aggregate(
                        seed: source,
                        func: (current, memberName) =>
                            current.AppendLine($"{className}.{memberName} => nameof({className}.{memberName}),")
                    )
                    .AppendLine($"_ => {INVALID_ENUM_MEMBER_METHOD_NAME}(value: value)");
            }
        }
    }

    private static CodeBuilder GenerateThrowNotFound(
        this CodeBuilder source,
        in EnumGeneration enumDeclaration,
        IFormatConfig formatConfig
    )
    {
        if (enumDeclaration.Options.HasDoesNotReturnAttribute)
        {
            source = source.AppendLine("[DoesNotReturn]");
        }

        string className = formatConfig.ClassName;
        using (source.StartBlock($"public static string {INVALID_ENUM_MEMBER_METHOD_NAME}(this {className} value)"))
        {
            return enumDeclaration.Options.SupportsUnreachableException
                ? source.IssueUnreachableException(enumDeclaration)
                : source
                    .AppendLine("#if NET7_0_OR_GREATER")
                    .IssueUnreachableException(enumDeclaration: enumDeclaration)
                    .AppendLine("#else")
                    .IssueArgumentOutOfRangeException()
                    .AppendLine("#endif");
        }
    }

    private static CodeBuilder IssueArgumentOutOfRangeException(this CodeBuilder source)
    {
        return source.AppendLine(
            "throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: \"Unknown enum member\");"
        );
    }

    private static CodeBuilder IssueUnreachableException(this CodeBuilder source, in EnumGeneration enumDeclaration)
    {
        return source.AppendLine(
            $"throw new UnreachableException(message: \"{enumDeclaration.Name}: Unknown enum member\");"
        );
    }

    private static CodeBuilder GenerateGetDescription(
        this CodeBuilder source,
        in EnumGeneration enumDeclaration,
        IFormatConfig formatConfig
    )
    {
        string className = formatConfig.ClassName;

        using (
            source
                .AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                .StartBlock($"public static string {GET_DESCRIPTION_METHOD_NAME}(this {className} value)")
        )
        {
            IReadOnlyList<string> items = enumDeclaration.GetDescriptionCaseOptions(formatConfig);

            if (items.Count == 0)
            {
                return source.AppendLine($"return {GET_NAME_METHOD_NAME}(value);");
            }

            using (source.StartBlock(text: "return value switch", start: "{", end: "};"))
            {
                return items
                    .Aggregate(seed: source, func: (current, line) => current.AppendLine(line))
                    .AppendLine($"_ => {GET_NAME_METHOD_NAME}(value)");
            }
        }
    }

    private static IReadOnlyList<string> GetUniqueMemberNames(this in EnumGeneration enumDeclaration)
    {
        return [.. UniqueMembers(enumDeclaration).Select(member => member.Name)];
    }

    private static IReadOnlyList<string> GetUniqueMemberNames(this in EnumGeneration enumDeclaration, string className)
    {
        return [.. UniqueMembers(enumDeclaration).Select(member => member.BuildClassMemberName(className: className))];
    }

    private static IEnumerable<IFieldSymbol> UniqueMembers(in EnumGeneration enumDeclaration)
    {
        HashSet<string> names = enumDeclaration.UniqueEnumMemberNames();

        return enumDeclaration.Members.Where(member => !IsSkippedOrObsolete(fieldSymbol: member, names: names));

        static bool IsSkippedOrObsolete(IFieldSymbol fieldSymbol, HashSet<string> names)
        {
            return fieldSymbol.IsSkipEnumValue(names: names) || fieldSymbol.HasObsoleteAttribute();
        }
    }

    public static HashSet<string> UniqueEnumMemberNames(this in EnumGeneration enumDeclaration)
    {
        return new(
            enumDeclaration.Members.Select(m => m.Name).Distinct(StringComparer.Ordinal),
            comparer: StringComparer.Ordinal
        );
    }
}
