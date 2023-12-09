using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Extensions;
using Credfeto.Enumeration.Source.Generation.Helpers;
using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Enumeration.Source.Generation;

public static class EnumSourceGenerator
{
    private const string IS_DEFINED_METHOD_NAME = nameof(Enum.IsDefined);
    private const string GET_NAME_METHOD_NAME = "GetName";
    private const string GET_DESCRIPTION_METHOD_NAME = "GetDescription";
    private const string INVALID_ENUM_MEMBER_METHOD_NAME = "ThrowInvalidEnumMemberException";

    public static string GenerateClassForEnum(in EnumGeneration enumDeclaration, out CodeBuilder source)
    {
        string className = enumDeclaration.Name + "GeneratedExtensions";

        source = AddUsingDeclarations(new());

        using (source.AppendLine("namespace " + enumDeclaration.Namespace + ";")
                     .AppendBlankLine()
                     .AppendLine($"[GeneratedCode(tool: \"{typeof(EnumGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]")
                     .StartBlock(ConvertAccessType(enumDeclaration.AccessType) + " static class " + className))
        {
            GenerateMethods(source: source, attribute: enumDeclaration, classNameFormatter: ClassNameOnlyFormatter);
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
        return AddUsingDeclarations(source: source,
                                    "System",
                                    "System.CodeDom.Compiler",
                                    "System.Diagnostics",
                                    "System.Diagnostics.CodeAnalysis",
                                    "System.Runtime.CompilerServices");
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

        source = AddUsingDeclarations(new());

        using (source.AppendLine("namespace " + classDeclaration.Namespace + ";")
                     .AppendBlankLine()
                     .AppendLine($"[GeneratedCode(tool: \"{typeof(EnumGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]")
                     .StartBlock(ConvertAccessType(classDeclaration.AccessType) + " static partial class " + className))
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
                    source.AppendBlankLine();
                }

                GenerateMethods(source: source, attribute: attribute, classNameFormatter: classNameFormatter);
            }
        }

        return className;
    }

    private static void GenerateMethods(CodeBuilder source, in EnumGeneration attribute, Func<EnumGeneration, string> classNameFormatter)
    {
        GenerateGetName(source: source, enumDeclaration: attribute, classNameFormatter: classNameFormatter);
        source.AppendBlankLine();
        GenerateGetDescription(source: source, enumDeclaration: attribute, classNameFormatter: classNameFormatter);
        source.AppendBlankLine();
        GenerateIsDefined(source: source, enumDeclaration: attribute, classNameFormatter: classNameFormatter);
        source.AppendBlankLine();
        GenerateThrowNotFound(source: source, enumDeclaration: attribute, classNameFormatter: classNameFormatter);
    }

    private static void GenerateThrowNotFound(CodeBuilder source, in EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter)
    {
        string className = classNameFormatter(enumDeclaration);

        if (enumDeclaration.Options.HasDoesNotReturnAttribute)
        {
            source.AppendLine("[DoesNotReturn]");
        }

        using (source.StartBlock("public static string " + INVALID_ENUM_MEMBER_METHOD_NAME + "(this " + className + " value)"))
        {
            if (enumDeclaration.Options.SupportsUnreachableException)
            {
                IssueUnreachableException(source: source, enumDeclaration: enumDeclaration);
            }
            else
            {
                source.AppendLine("#if NET7_0_OR_GREATER");
                IssueUnreachableException(source: source, enumDeclaration: enumDeclaration);
                source.AppendLine("#else");
                IssueArgumentOutOfRangeException(source);
                source.AppendLine("#endif");
            }
        }
    }

    private static void IssueArgumentOutOfRangeException(CodeBuilder source)
    {
        source.AppendLine("throw new ArgumentOutOfRangeException(nameof(value), actualValue: value, message: \"Unknown enum member\");");
    }

    private static void IssueUnreachableException(CodeBuilder source, in EnumGeneration enumDeclaration)
    {
        source.AppendLine("throw new UnreachableException(message: \"" + enumDeclaration.Name + ": Unknown enum member\");");
    }

    private static void GenerateGetName(CodeBuilder source, in EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter)
    {
        string className = classNameFormatter(enumDeclaration);

        using (source.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                     .StartBlock("public static string " + GET_NAME_METHOD_NAME + "(this " + className + " value)"))
        {
            IReadOnlyList<string> members = UniqueMembers(enumDeclaration)
                                            .Select(member => member.Name)
                                            .ToArray();

            if (members.Count == 0)
            {
                source.AppendLine("return " + INVALID_ENUM_MEMBER_METHOD_NAME + "(value: value);");
            }
            else
            {
                using (source.StartBlock(text: "return value switch", start: "{", end: "};"))
                {
                    members.Aggregate(seed: source,
                                      func: (current, memberName) => current.AppendLine(className + "." + memberName + " => nameof(" + className + "." + memberName + "),"))
                           .AppendLine("_ => " + INVALID_ENUM_MEMBER_METHOD_NAME + "(value: value)");
                }
            }
        }
    }

    private static void GenerateIsDefined(CodeBuilder source, in EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter)
    {
        string className = classNameFormatter(enumDeclaration);

        using (source.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                     .StartBlock("public static bool " + IS_DEFINED_METHOD_NAME + "(this " + className + " value)"))
        {
            IReadOnlyList<string> members = UniqueMembers(enumDeclaration)
                                            .Select(member => className + "." + member.Name)
                                            .ToArray();

            switch (members.Count)
            {
                case 0:
                    source.AppendLine("return false;");

                    break;
                case 1:
                    source.AppendLine("return value == " + members[0] + ";");

                    break;
                default:
                {
                    source.AppendLine("return value is " + string.Join(separator: " or ", values: members) + ";");

                    break;
                }
            }
        }
    }

    private static IEnumerable<IFieldSymbol> UniqueMembers(EnumGeneration enumDeclaration)
    {
        HashSet<string> names = UniqueEnumMemberNames(enumDeclaration);

        foreach (IFieldSymbol member in enumDeclaration.Members)
        {
            if (IsSkipEnumValue(member: member, names: names) || member.HasObsoleteAttribute())
            {
                continue;
            }

            yield return member;
        }
    }

    private static void GenerateGetDescription(CodeBuilder source, in EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter)
    {
        string className = classNameFormatter(enumDeclaration);

        using (source.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                     .StartBlock("public static string " + GET_DESCRIPTION_METHOD_NAME + "(this " + className + " value)"))
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

                    source.AppendLine("_ => " + GET_NAME_METHOD_NAME + "(value)");
                }
            }
        }
    }

    private static IReadOnlyList<string> GetDescriptionCaseOptions(EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter)
    {
        HashSet<string> names = UniqueEnumMemberNames(enumDeclaration);

        return enumDeclaration.Members.Where(member => !IsSkipEnumValue(member: member, names: names) && !member.HasObsoleteAttribute())
                              .Select(member => (member, description: member.GetAttributes()
                                                                            .FirstOrDefault(SymbolExtensions.IsDescriptionAttribute)))
                              .Where(item => item.description is not null)
                              .Select(EnsureNotNullDescription)
                              .Select(item => (item.member, typedConstant: item.description.ConstructorArguments.FirstOrDefault()))
                              .Select(item => (item.member, attributeText: ((TypedConstant?)item.typedConstant).Value.ToCSharpString()))
                              .Where(item => !string.IsNullOrWhiteSpace(item.attributeText))
                              .Select(item => FormatMember(enumDeclaration: enumDeclaration,
                                                           classNameFormatter: classNameFormatter,
                                                           member: item.member,
                                                           attributeText: item.attributeText))
                              .ToArray();

        static (IFieldSymbol member, AttributeData description) EnsureNotNullDescription((IFieldSymbol member, AttributeData? description) item)
        {
            // ! item.description nullability has been excluded in the where before this.
            return (item.member, description: item.description!);
        }
    }

    private static string FormatMember(in EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter, IFieldSymbol member, string attributeText)
    {
        return classNameFormatter(enumDeclaration) + "." + member.Name + " => " + attributeText + ",";
    }

    private static bool IsSkipEnumValue(IFieldSymbol member, HashSet<string> names)
    {
        EnumMemberDeclarationSyntax? syntax = FindEnumMemberDeclarationSyntax(member);

        if (syntax?.EqualsValue is not null)
        {
            if (syntax.EqualsValue.Value.Kind() == SyntaxKind.IdentifierName)
            {
                bool found = names.Contains(syntax.EqualsValue.Value.ToString());

                if (found)
                {
                    // note deliberately ignoring the return value here as we need to record the integer value as being skipped too
                    IsSkipConstantValue(member: member, names: names);

                    return true;
                }
            }
        }

        return IsSkipConstantValue(member: member, names: names);
    }

    private static bool IsSkipConstantValue(IFieldSymbol member, HashSet<string> names)
    {
        object? cv = member.ConstantValue;

        if (cv is null)
        {
            return false;
        }

        return !names.Add(cv.ToString());
    }

    private static EnumMemberDeclarationSyntax? FindEnumMemberDeclarationSyntax(ISymbol member)
    {
        EnumMemberDeclarationSyntax? syntax = null;

        foreach (SyntaxReference dsr in member.DeclaringSyntaxReferences)
        {
            syntax = GetSyntax(dsr);

            if (syntax is not null)
            {
                break;
            }
        }

        return syntax;
    }

    [SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0045:Use async override", Justification = "Calling code is not async")]
    private static EnumMemberDeclarationSyntax? GetSyntax(SyntaxReference dsr)
    {
        return dsr.GetSyntax(CancellationToken.None) as EnumMemberDeclarationSyntax;
    }

    private static HashSet<string> UniqueEnumMemberNames(in EnumGeneration enumDeclaration)
    {
        return new(enumDeclaration.Members.Select(m => m.Name)
                                  .Distinct(StringComparer.Ordinal),
                   comparer: StringComparer.Ordinal);
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