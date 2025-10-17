using System.Collections.Generic;
using System.Linq;
using Credfeto.Enumeration.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Credfeto.Enumeration.Source.Generation.Extensions;

internal static class EnumGenerationExtensions
{
    public static string FormatMember(
        this in EnumGeneration enumDeclaration,
        IFormatConfig formatConfig,
        IFieldSymbol member,
        string attributeText
    )
    {
        return formatConfig.ClassName + "." + member.Name + " => " + attributeText + ",";
    }

    public static IReadOnlyList<string> GetDescriptionCaseOptions(
        this EnumGeneration enumDeclaration,
        IFormatConfig formatConfig
    )
    {
        HashSet<string> names = enumDeclaration.UniqueEnumMemberNames();

        return
        [
            .. enumDeclaration
               .Members.Where(member =>
                                  !member.IsSkipEnumValue(names: names) && !member.HasObsoleteAttribute()
               )
               .Select(member =>
                       (
                           member,
                           description: member.GetAttributes().FirstOrDefault(SymbolExtensions.IsDescriptionAttribute)
                       )
               )
               .Where(item => item.description is not null)
               .Select(EnsureNotNullDescription)
               .Select(item => (item.member, typedConstant: item.description.ConstructorArguments.FirstOrDefault()))
               .Select(item =>
                           (item.member, attributeText: ((TypedConstant?)item.typedConstant).Value.ToCSharpString())
               )
               .Where(item => !string.IsNullOrWhiteSpace(item.attributeText))
               .Select(item =>
                           enumDeclaration.FormatMember(
                               formatConfig: formatConfig,
                               member: item.member,
                               attributeText: item.attributeText
                           )
               ),
        ];

        static (IFieldSymbol member, AttributeData description) EnsureNotNullDescription(
            (IFieldSymbol member, AttributeData? description) item
        )
        {
            // ! item.description nullability has been excluded in the where before this.
            return (item.member, description: item.description!);
        }
    }

}