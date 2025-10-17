using System.Collections.Generic;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class EnumGenerationExtensions
{
    public static IReadOnlyList<string> GetDescriptionCaseOptions(in this EnumGeneration enumDeclaration, IFormatConfig formatConfig)
    {
        return GetDescriptionCaseOptionsQuery(enumDeclaration: enumDeclaration, formatConfig: formatConfig, enumDeclaration.UniqueEnumMemberNames());
    }

    private static IReadOnlyList<string> GetDescriptionCaseOptionsQuery(in EnumGeneration enumDeclaration, IFormatConfig formatConfig, HashSet<string> names)
    {
        return
        [
            .. enumDeclaration.Members.Where(member => IsUsable(member: member, names: names))
                              .Select(MemberDescription)
                              .Where(item => item.description is not null)
                              .Select(EnsureNotNullDescription)
                              .Select(MemberDescriptionText)
                              .Select(MemberAttributeText)
                              .Where(IsValidText)
                              .Select(item => item.member.FormatMember(attributeText: item.attributeText, formatConfig: formatConfig))
        ];
    }

    private static bool IsValidText((IFieldSymbol member, string attributeText) item)
    {
        return !string.IsNullOrWhiteSpace(item.attributeText);
    }

    private static (IFieldSymbol member, string attributeText) MemberAttributeText((IFieldSymbol member, TypedConstant typedConstant) item)
    {
        return (item.member, attributeText: ((TypedConstant?)item.typedConstant).Value.ToCSharpString());
    }

    private static (IFieldSymbol member, TypedConstant typedConstant) MemberDescriptionText((IFieldSymbol member, AttributeData description) item)
    {
        return (item.member, typedConstant: item.description.ConstructorArguments.FirstOrDefault());
    }

    private static bool IsUsable(IFieldSymbol member, HashSet<string> names)
    {
        return !member.IsSkipEnumValue(names: names) && !member.HasObsoleteAttribute();
    }

    private static (IFieldSymbol member, AttributeData? description) MemberDescription(IFieldSymbol member)
    {
        return (member, description: member.GetAttributes()
                                           .FirstOrDefault(SymbolExtensions.IsDescriptionAttribute));
    }

    private static (IFieldSymbol member, AttributeData description) EnsureNotNullDescription((IFieldSymbol member, AttributeData? description) item)
    {
        // ! item.description nullability has been excluded in the where before this.
        return (item.member, description: item.description!);
    }
}