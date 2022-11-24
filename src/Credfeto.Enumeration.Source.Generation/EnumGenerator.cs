using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Builders;
using Credfeto.Enumeration.Source.Generation.Extensions;
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
    private const string IS_DEFINED_METHOD_NAME = nameof(Enum.IsDefined);
    private const string GET_NAME_METHOD_NAME = "GetName";
    private const string GET_DESCRIPTION_METHOD_NAME = "GetDescription";
    private const string INVALID_ENUM_MEMBER_METHOD_NAME = "ThrowInvalidEnumMemberException";

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not EnumSyntaxReceiver receiver)
        {
            return;
        }

        bool hasDoesNotReturn = receiver.HasDoesNotReturnAttribute;
        bool supportsUnreachableException = receiver.SupportsUnreachableException;

        foreach (EnumGeneration enumDeclaration in receiver.Enums)
        {
            GenerateClassForEnum(context: context, enumDeclaration: enumDeclaration, hasDoesNotReturn: hasDoesNotReturn, supportsUnreachableException: supportsUnreachableException);
        }

        foreach (ClassEnumGeneration classDeclaration in receiver.Classes)
        {
            GenerateClassForClass(context: context, classDeclaration: classDeclaration, hasDoesNotReturn: hasDoesNotReturn, supportsUnreachableException: supportsUnreachableException);
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new EnumSyntaxReceiver());
    }

    private static void GenerateClassForEnum(in GeneratorExecutionContext context, EnumGeneration enumDeclaration, bool hasDoesNotReturn, bool supportsUnreachableException)
    {
        string className = enumDeclaration.Name + "GeneratedExtensions";

        CodeBuilder source = AddUsingDeclarations(new());

        using (source.AppendLine("namespace " + enumDeclaration.Namespace + ";")
                     .AppendBlankLine()
                     .AppendLine($"[GeneratedCode(tool: \"{typeof(EnumGenerator).FullName}\", version: \"{VersionInformation.Version()}\")]")
                     .StartBlock(ConvertAccessType(enumDeclaration.AccessType) + " static class " + className))
        {
            GenerateMethods(hasDoesNotReturn: hasDoesNotReturn,
                            supportsUnreachableException: supportsUnreachableException,
                            source: source,
                            attribute: enumDeclaration,
                            classNameFormatter: ClassNameOnlyFormatter);
        }

        context.AddSource(enumDeclaration.Namespace + "." + className + ".generated.cs", sourceText: source.Text);
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
        return namespaces.OrderBy(n => n.ToLowerInvariant())
                         .Aggregate(seed: source, func: (current, ns) => current.AppendLine($"using {ns};"))
                         .AppendBlankLine();
    }

    private static void GenerateClassForClass(in GeneratorExecutionContext context, ClassEnumGeneration classDeclaration, bool hasDoesNotReturn, bool supportsUnreachableException)
    {
        string className = classDeclaration.Name;

        CodeBuilder source = AddUsingDeclarations(new());

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

                GenerateMethods(hasDoesNotReturn: hasDoesNotReturn,
                                supportsUnreachableException: supportsUnreachableException,
                                source: source,
                                attribute: attribute,
                                classNameFormatter: classNameFormatter);
            }
        }

        context.AddSource(classDeclaration.Namespace + "." + className + ".generated.cs", sourceText: source.Text);
    }

    private static void GenerateMethods(bool hasDoesNotReturn, bool supportsUnreachableException, CodeBuilder source, EnumGeneration attribute, Func<EnumGeneration, string> classNameFormatter)
    {
        GenerateGetName(source: source, enumDeclaration: attribute, classNameFormatter: classNameFormatter);
        source.AppendBlankLine();
        GenerateGetDescription(source: source, enumDeclaration: attribute, classNameFormatter: classNameFormatter);
        source.AppendBlankLine();
        GenerateIsDefined(source: source, enumDeclaration: attribute, classNameFormatter: classNameFormatter);
        source.AppendBlankLine();
        GenerateThrowNotFound(source: source,
                              enumDeclaration: attribute,
                              classNameFormatter: classNameFormatter,
                              hasDoesNotReturn: hasDoesNotReturn,
                              supportsUnreachableException: supportsUnreachableException);
    }

    private static void GenerateThrowNotFound(CodeBuilder source,
                                              EnumGeneration enumDeclaration,
                                              bool supportsUnreachableException,
                                              Func<EnumGeneration, string> classNameFormatter,
                                              bool hasDoesNotReturn)
    {
        string className = classNameFormatter(enumDeclaration);

        if (hasDoesNotReturn)
        {
            source.AppendLine("[DoesNotReturn]");
        }

        using (source.StartBlock("public static string " + INVALID_ENUM_MEMBER_METHOD_NAME + "(this " + className + " value)"))
        {
            if (supportsUnreachableException)
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

    private static void IssueUnreachableException(CodeBuilder source, EnumGeneration enumDeclaration)
    {
        source.AppendLine("throw new UnreachableException(message: \"" + enumDeclaration.Name + ": Unknown enum member\");");
    }

    private static void GenerateGetName(CodeBuilder source, EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter)
    {
        string className = classNameFormatter(enumDeclaration);

        using (source.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                     .StartBlock("public static string " + GET_NAME_METHOD_NAME + "(this " + className + " value)"))
        {
            using (source.StartBlock(text: "return value switch", start: "{", end: "};"))
            {
                UniqueMembers(enumDeclaration)
                    .Select(member => member.Name)
                    .Aggregate(seed: source, func: (current, memberName) => current.AppendLine(className + "." + memberName + " => nameof(" + className + "." + memberName + "),"))
                    .AppendLine("_ => " + INVALID_ENUM_MEMBER_METHOD_NAME + "(value: value)");
            }
        }
    }

    private static void GenerateIsDefined(CodeBuilder source, EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter)
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

    private static void GenerateGetDescription(CodeBuilder source, EnumGeneration enumDeclaration, Func<EnumGeneration, string> classNameFormatter)
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
        List<string> items = new();

        HashSet<string> names = UniqueEnumMemberNames(enumDeclaration);

        foreach (IFieldSymbol? member in enumDeclaration.Members)
        {
            if (IsSkipEnumValue(member: member, names: names) || member.HasObsoleteAttribute())
            {
                continue;
            }

            AttributeData? description = member.GetAttributes()
                                               .FirstOrDefault(SymbolExtensions.IsDescriptionAttribute);

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

    private static bool IsSkipEnumValue(IFieldSymbol member, HashSet<string> names)
    {
        EnumMemberDeclarationSyntax? syntax = FindEnumMemberDeclarationSyntax(member);

        if (syntax?.EqualsValue != null)
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

        if (cv == null)
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
            syntax = dsr.GetSyntax() as EnumMemberDeclarationSyntax;

            if (syntax != null)
            {
                break;
            }
        }

        return syntax;
    }

    private static HashSet<string> UniqueEnumMemberNames(EnumGeneration enumDeclaration)
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