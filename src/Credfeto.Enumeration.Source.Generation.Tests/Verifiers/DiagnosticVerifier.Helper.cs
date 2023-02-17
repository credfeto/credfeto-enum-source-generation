using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Enumeration.Source.Generation.Tests.Exceptions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Credfeto.Enumeration.Source.Generation.Tests.Verifiers;

/// <summary>
///     Class for turning strings into documents and getting the diagnostics on them
///     All methods are static
/// </summary>
public abstract partial class DiagnosticVerifier
{
    private const string DEFAULT_FILE_PATH_PREFIX = "Test";
    private const string C_SHARP_DEFAULT_FILE_EXT = "cs";
    private const string VISUAL_BASIC_DEFAULT_EXT = "vb";
    private const string TEST_PROJECT_NAME = "TestProject";
    private static readonly string? AssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
    private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
    private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
    private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
    private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

    private static readonly MetadataReference SystemRuntimeReference = MetadataReference.CreateFromFile(Path.Combine(AssemblyPath ?? string.Empty, path2: "System.Runtime.dll"));

    private static readonly MetadataReference SystemReference = MetadataReference.CreateFromFile(Path.Combine(AssemblyPath ?? string.Empty, path2: "System.dll"));
    private static readonly MetadataReference SystemConsoleReference = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);

    #region Get Diagnostics

    /// <summary>
    ///     Given classes in the form of strings, their language, and an IDiagnosticAnalyzer to apply to it, return the diagnostics found in the string after converting it to a document.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <param name="references">Metadata references</param>
    /// <param name="language">The language the source classes are in</param>
    /// <param name="analyzer">The analyzer to be run on the sources</param>
    /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
    private static Task<IReadOnlyList<Diagnostic>> GetSortedDiagnosticsAsync(string[] sources, MetadataReference[] references, string language, DiagnosticAnalyzer analyzer)
    {
        return GetSortedDiagnosticsFromDocumentsAsync(analyzer: analyzer, GetDocuments(sources: sources, references: references, language: language));
    }

    /// <summary>
    ///     Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics found in it.
    ///     The returned diagnostics are then ordered by location in the source document.
    /// </summary>
    /// <param name="analyzer">The analyzer to run on the documents</param>
    /// <param name="documents">The Documents that the analyzer will be run on</param>
    /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
    protected static async Task<IReadOnlyList<Diagnostic>> GetSortedDiagnosticsFromDocumentsAsync(DiagnosticAnalyzer analyzer, IReadOnlyList<Document> documents)
    {
        HashSet<Project> projects = new();

        foreach (Document document in documents)
        {
            projects.Add(document.Project);
        }

        IReadOnlyList<Diagnostic> diagnostics = Array.Empty<Diagnostic>();

        foreach (Project project in projects)
        {
            Compilation? compilation = await project.GetCompilationAsync(CancellationToken.None);

            if (compilation == null)
            {
                continue;
            }

            EnsureNoCompilationErrors(compilation);

            CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(IaHelper.For(analyzer), options: null, cancellationToken: CancellationToken.None);
            diagnostics = await CollectDiagnosticsAsync(documents: documents, compilationWithAnalyzers: compilationWithAnalyzers);
        }

        return SortDiagnostics(diagnostics);
    }

    private static async Task<IReadOnlyList<Diagnostic>> CollectDiagnosticsAsync(IReadOnlyList<Document> documents, CompilationWithAnalyzers compilationWithAnalyzers)
    {
        IReadOnlyList<Diagnostic> diags = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(CancellationToken.None);

        return await ExtractDiagnosticsAsync(documents: documents, diags: diags);
    }

    private static async Task<IReadOnlyList<Diagnostic>> ExtractDiagnosticsAsync(IReadOnlyList<Document> documents, IReadOnlyList<Diagnostic> diags)
    {
        List<Diagnostic> diagnostics = new();

        foreach (Diagnostic diag in diags)
        {
            bool add = diag.Location == Location.None || diag.Location.IsInMetadata || await ShouldAddDocumentDiagnosticAsync(documents: documents, diag: diag);

            if (add)
            {
                diagnostics.Add(diag);
            }
        }

        return diagnostics;
    }

    private static async Task<bool> ShouldAddDocumentDiagnosticAsync(IReadOnlyList<Document> documents, Diagnostic diag)
    {
        foreach (Document document in documents)
        {
            bool add = await ShouldAddDiagnosticAsync(document: document, diag: diag);

            if (add)
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<bool> ShouldAddDiagnosticAsync(Document document, Diagnostic diag)
    {
        SyntaxTree? tree = await document.GetSyntaxTreeAsync(CancellationToken.None);
        bool add = tree != null && tree == diag.Location.SourceTree;

        return add;
    }

    private static void EnsureNoCompilationErrors(Compilation compilation)
    {
        IReadOnlyList<Diagnostic> compilerErrors = compilation.GetDiagnostics(CancellationToken.None);

        if (compilerErrors.Count != 0)
        {
            StringBuilder errors = compilerErrors.Where(IsReportableCSharpError)
                                                 .Aggregate(new StringBuilder(), func: (current, compilerError) => current.Append(compilerError));

            if (errors.Length != 0)
            {
                throw new UnitTestSourceException("Please correct following compiler errors in your unit test source:" + errors);
            }
        }
    }

    private static bool IsReportableCSharpError(Diagnostic compilerError)
    {
        return !compilerError.ToString()
                             .Contains(value: "netstandard", comparisonType: StringComparison.Ordinal) && !compilerError.ToString()
                                                                                                                        .Contains(value: "static 'Main' method",
                                                                                                                                  comparisonType: StringComparison.Ordinal) && !compilerError.ToString()
            .Contains(value: "CS1002", comparisonType: StringComparison.Ordinal) && !compilerError.ToString()
                                                                                                  .Contains(value: "CS1702", comparisonType: StringComparison.Ordinal);
    }

    /// <summary>
    ///     Sort diagnostics by location in source document
    /// </summary>
    /// <param name="diagnostics">The list of Diagnostics to be sorted</param>
    /// <returns>An IEnumerable containing the Diagnostics in order of Location</returns>
    private static IReadOnlyList<Diagnostic> SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        return diagnostics.OrderBy(keySelector: d => d.Location.SourceSpan.Start)
                          .ToArray();
    }

    #endregion

    #region Set up compilation and documents

    /// <summary>
    ///     Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <param name="references">Metadata references.</param>
    /// <param name="language">The language the source code is in</param>
    /// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
    private static Document[] GetDocuments(string[] sources, MetadataReference[] references, string language)
    {
        if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic)
        {
            throw new ArgumentException(message: "Unsupported Language", nameof(language));
        }

        Project project = CreateProject(sources: sources, references: references, language: language);
        Document[] documents = project.Documents.ToArray();

        if (sources.Length != documents.Length)
        {
            throw new InvalidOperationException(message: "Amount of sources did not match amount of Documents created");
        }

        return documents;
    }

    /// <summary>
    ///     Create a project using the inputted strings as sources.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <param name="references">Metadata References.</param>
    /// <param name="language">The language the source code is in</param>
    /// <returns>A Project created out of the Documents created from the source strings</returns>
    private static Project CreateProject(string[] sources, MetadataReference[] references, string language = LanguageNames.CSharp)
    {
        const string fileNamePrefix = DEFAULT_FILE_PATH_PREFIX;
        string fileExt = language == LanguageNames.CSharp
            ? C_SHARP_DEFAULT_FILE_EXT
            : VISUAL_BASIC_DEFAULT_EXT;

        ProjectId projectId = ProjectId.CreateNewId(TEST_PROJECT_NAME);

        Solution solution = references.Aggregate(BuildSolutionWithStandardReferences(language: language, projectId: projectId),
                                                 func: (current, reference) => current.AddMetadataReference(projectId: projectId, metadataReference: reference));

        int count = 0;

        foreach (string source in sources)
        {
            string newFileName = fileNamePrefix + count.ToString(CultureInfo.InvariantCulture) + "." + fileExt;
            DocumentId documentId = DocumentId.CreateNewId(projectId: projectId, debugName: newFileName);
            solution = solution.AddDocument(documentId: documentId, name: newFileName, SourceText.From(source));
            count++;
        }

        Project? proj = solution.GetProject(projectId);
        Assert.NotNull(proj);

        return proj;
    }

    [SuppressMessage(category: "codecracker.CSharp", checkId: "CC0022:DisposeObjectsBeforeLosingScope", Justification = "Test code")]
    [SuppressMessage(category: "Microsoft.Reliability", checkId: "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Test code")]
    private static Solution BuildSolutionWithStandardReferences(string language, ProjectId projectId)
    {
        return new AdhocWorkspace().CurrentSolution.AddProject(projectId: projectId, name: TEST_PROJECT_NAME, assemblyName: TEST_PROJECT_NAME, language: language)
                                   .AddMetadataReference(projectId: projectId, metadataReference: CorlibReference)
                                   .AddMetadataReference(projectId: projectId, metadataReference: SystemCoreReference)
                                   .AddMetadataReference(projectId: projectId, metadataReference: CSharpSymbolsReference)
                                   .AddMetadataReference(projectId: projectId, metadataReference: CodeAnalysisReference)
                                   .AddMetadataReference(projectId: projectId, metadataReference: SystemRuntimeReference)
                                   .AddMetadataReference(projectId: projectId, metadataReference: SystemReference)
                                   .AddMetadataReference(projectId: projectId, metadataReference: SystemConsoleReference);
    }

    #endregion
}