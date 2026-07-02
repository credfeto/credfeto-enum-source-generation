using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests;

internal static class CompilationHelpers
{
    public static CSharpCompilation CreateCompilation(
        string source,
        IEnumerable<MetadataReference>? additionalReferences = null
    )
    {
        HashSet<string> addedPaths = new(StringComparer.OrdinalIgnoreCase);
        List<MetadataReference> references = [];

        AddReference(references, addedPaths, typeof(object));
        AddReference(references, addedPaths, typeof(Attribute));
        AddReference(references, addedPaths, typeof(System.ComponentModel.DescriptionAttribute));
        AddReference(references, addedPaths, typeof(System.Diagnostics.UnreachableException));
        AddReference(references, addedPaths, typeof(System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute));

        // Add System.Runtime which provides core types
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string? name = assembly.GetName().Name;

            if (
                StringComparer.Ordinal.Equals(name, "System.Runtime")
                || StringComparer.Ordinal.Equals(name, "netstandard")
            )
            {
                TryAddReference(references, addedPaths, assembly.Location);
            }
        }

        if (additionalReferences is not null)
        {
            references.AddRange(additionalReferences);
        }

        return CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    public static CSharpCompilation CreateMinimalCompilation(string source)
    {
        return CSharpCompilation.Create(
            assemblyName: "MinimalTestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)],
            references: [],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
    }

    private static void AddReference(List<MetadataReference> references, HashSet<string> addedPaths, Type type)
    {
        TryAddReference(references, addedPaths, type.Assembly.Location);
    }

    private static void TryAddReference(List<MetadataReference> references, HashSet<string> addedPaths, string path)
    {
        if (!string.IsNullOrEmpty(path) && addedPaths.Add(path))
        {
            references.Add(MetadataReference.CreateFromFile(path));
        }
    }

    public static CSharpCompilation CreateCompilationWithEnumTextAttribute(string source)
    {
        MetadataReference attributesRef = MetadataReference.CreateFromFile(
            typeof(Credfeto.Enumeration.Source.Generation.Attributes.EnumTextAttribute).Assembly.Location
        );

        return CreateCompilation(source: source, additionalReferences: [attributesRef]);
    }
}
