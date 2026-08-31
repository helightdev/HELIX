#if RIDER
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Parts;
using JetBrains.ProjectModel;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Feature.Services.Protocol;
using JetBrains.ReSharper.Resources.Shell;

using WireRange = HelixRider.Protocol.MixinSourceRange;
using CoreRange = Mixins.Compiler.MixinSourceRange;

namespace HelixRider.Protocol;

/// <summary>
/// Stateless external-analysis endpoint for the IntelliJ frontend. It parses a complete batch
/// from the supplied text only; it never reads editor documents or owns mixin PSI.
/// </summary>
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
public sealed class HelixMixinLanguageHost
{
    private readonly ISymbolCache _symbolCache;

    public HelixMixinLanguageHost(ISolution solution, ISymbolCache symbolCache)
    {
        _symbolCache = symbolCache;
        solution.GetProtocolSolution().GetHelixExpressionModel().ParseMixinFiles.SetAsync(
            (_, request) => RdTask.Successful(Parse(request, CompleteTypes, ResolveType)));
        solution.GetProtocolSolution().GetHelixExpressionModel().GetMixinLanguageCatalog.SetAsync(
            (_, _) => RdTask.Successful(LanguageCatalog()));
    }

    internal static MixinParseResponse Parse(MixinParseRequest request) => Parse(request, null, null);
    internal static MixinLanguageCatalog LanguageCatalog() => new(Definitions());

    private static MixinParseResponse Parse(MixinParseRequest request,
        Func<string, MixinCompletionItem[]> completeTypes, Func<string, SemanticTarget> resolveType)
    {
        var completionCache = new Dictionary<string, MixinCompletionItem[]>(StringComparer.Ordinal);
        MixinCompletionItem[] CachedCompletion(string prefix)
        {
            if (completeTypes == null) return Array.Empty<MixinCompletionItem>();
            if (!completionCache.TryGetValue(prefix ?? string.Empty, out var result))
                completionCache[prefix ?? string.Empty] = result = completeTypes(prefix ?? string.Empty);
            return result;
        }
        var typeCache = new Dictionary<string, SemanticTarget>(StringComparer.Ordinal);
        SemanticTarget CachedType(string name)
        {
            if (resolveType == null) return null;
            if (!typeCache.TryGetValue(name ?? string.Empty, out var result))
                typeCache[name ?? string.Empty] = result = resolveType(name ?? string.Empty);
            return result;
        }
        var inputs = request?.Files ?? Array.Empty<MixinFileInput>();
        var analyses = inputs.Select(input => new AnalyzedInput(input,
            MixinEditorAnalyzer.Analyze(input.SourceText ?? string.Empty))).ToArray();
        return new MixinParseResponse(analyses.Select(input => Snapshot(input, analyses,
            completeTypes == null ? null : CachedCompletion,
            resolveType == null ? null : CachedType)).ToArray());
    }

    private static MixinFileSnapshot Snapshot(AnalyzedInput input, IReadOnlyList<AnalyzedInput> batch,
        Func<string, MixinCompletionItem[]> completeTypes, Func<string, SemanticTarget> resolveType)
    {
        // Syntax and highlighting are produced synchronously by the close Kotlin editor-parser
        // port. Keep the legacy wire fields empty until the next protocol schema cleanup; RD is
        // now exclusively the semantic enrichment channel.
        var syntax = Array.Empty<MixinSyntaxNode>();
        var tokens = Array.Empty<MixinToken>();
        var declarations = input.Analysis.Declarations.Select(declaration => new MixinDeclaration(
            declaration.Name, declaration.Kind.ToString(), Range(declaration.Range),
            Range(declaration.ScopeStart, declaration.ScopeEnd))).ToArray();
        var references = input.Analysis.References.Select(reference =>
        {
            if (reference.Kind == MixinEditorReferenceKind.CSharpType)
            {
                var semantic = resolveType?.Invoke(reference.Name);
                return new MixinReference(reference.Name, reference.Kind.ToString(), Range(reference.Range),
                    Range(reference.ScopeStart, reference.ScopeEnd), semantic?.FilePath ?? string.Empty,
                    semantic == null ? Range(0, 0) : Range(semantic.StartOffset, semantic.EndOffset));
            }
            var target = Resolve(input, reference, batch);
            return new MixinReference(reference.Name, reference.Kind.ToString(), Range(reference.Range),
                Range(reference.ScopeStart, reference.ScopeEnd), target?.Input.Input.FilePath ?? string.Empty,
                target == null ? Range(0, 0) : Range(target.Declaration.Range));
        }).ToArray();
        var resolvedFunctions = references.Where(reference =>
                reference.Kind == MixinEditorReferenceKind.Function.ToString() &&
                !string.IsNullOrEmpty(reference.TargetFilePath))
            .Select(reference => (reference.Range.StartOffset, reference.Range.EndOffset))
            .ToHashSet();
        var directoryFunctions = batch
            .Where(candidate => SameDirectory(input.Input.FilePath, candidate.Input.FilePath))
            .SelectMany(candidate => candidate.Analysis.Declarations
                .Where(declaration => declaration.Kind == MixinEditorSymbolKind.Function)
                .Select(declaration => (Input: candidate, Declaration: declaration)))
            .GroupBy(item => item.Declaration.Name, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var diagnostics = input.Analysis.Diagnostics
            .Where(diagnostic => !IsResolvedFunctionDiagnostic(diagnostic, resolvedFunctions) &&
                !IsDirectoryDuplicateFunctionDiagnostic(diagnostic, directoryFunctions))
            .Select(diagnostic => new MixinDiagnostic(
                diagnostic.Message, diagnostic.Severity.ToString(), Range(diagnostic.Range))).ToList();
        foreach (var declaration in input.Analysis.Declarations.Where(declaration =>
                     declaration.Kind == MixinEditorSymbolKind.Function &&
                     directoryFunctions.ContainsKey(declaration.Name)))
            diagnostics.Add(new MixinDiagnostic("duplicate function '" + declaration.Name +
                "' in mixin directory", "Error", Range(declaration.Range)));
        if (resolveType != null)
        {
            diagnostics.AddRange(references.Where(reference =>
                    reference.Kind == MixinEditorReferenceKind.CSharpType.ToString() &&
                    string.IsNullOrEmpty(reference.TargetFilePath))
                .Select(reference => new MixinDiagnostic(
                    "unresolved C# type '" + reference.Name + "'", "Error", reference.Range)));
        }
        var completionSites = CompletionSites(input.Input.SourceText ?? string.Empty, completeTypes, resolveType);
        return new MixinFileSnapshot(input.Input.FilePath ?? string.Empty, input.Input.Revision,
            SourceHash(input.Input.SourceText ?? string.Empty), syntax.ToArray(), tokens,
            declarations, references, diagnostics.ToArray(), completionSites);
    }

    private static bool IsResolvedFunctionDiagnostic(MixinEditorDiagnostic diagnostic,
        ISet<(int Start, int End)> resolvedFunctions) =>
        diagnostic.Message.StartsWith("unresolved function '", StringComparison.Ordinal) &&
        resolvedFunctions.Contains((diagnostic.Range.Start, diagnostic.Range.End));

    private static bool IsDirectoryDuplicateFunctionDiagnostic(MixinEditorDiagnostic diagnostic,
        IReadOnlyDictionary<string, (AnalyzedInput Input, MixinEditorSymbol Declaration)[]> duplicates) =>
        diagnostic.Message.StartsWith("duplicate function '", StringComparison.Ordinal) &&
        duplicates.Keys.Any(name => diagnostic.Message.Contains("'" + name + "'"));

    private static MixinCompletionSite[] CompletionSites(string source,
        Func<string, MixinCompletionItem[]> completeTypes, Func<string, SemanticTarget> resolveType)
    {
        return MixinEditorAnalyzer.GetCompletionSites(source).Select(site =>
        {
            var items = site.Context.Kind == MixinEditorCompletionKind.CSharpType && completeTypes != null &&
                        (resolveType == null || resolveType(site.Context.Prefix ?? string.Empty) == null)
                ? completeTypes(site.Context.Prefix ?? string.Empty)
                : Array.Empty<MixinCompletionItem>();
            return new MixinCompletionSite(site.Context.Kind.ToString(),
                Range(Clamp(site.ActivationRange, source.Length)),
                Range(Clamp(site.Context.ReplacementRange, source.Length)),
                site.Context.ReceiverKind.ToString(), items);
        }).ToArray();
    }

    private MixinCompletionItem[] CompleteTypes(string prefix)
    {
        return ReadLockCookie.Execute(() =>
        {
            var normalizedPrefix = (prefix ?? string.Empty).Trim().Replace("global::", string.Empty);
            var shortPrefix = normalizedPrefix.Substring(normalizedPrefix.LastIndexOf('.') + 1);
            var scope = _symbolCache.GetSymbolScope(LibrarySymbolScope.FULL, true);
            return scope.GetAllShortNames()
                .Where(name => name.StartsWith(shortPrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Take(256)
                .SelectMany(name => scope.GetElementsByShortName(name).OfType<ITypeElement>())
                .Select(type => type.GetClrName().FullName.Replace('+', '.'))
                .Where(name => string.IsNullOrEmpty(normalizedPrefix) ||
                               name.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase) ||
                               name.Substring(name.LastIndexOf('.') + 1)
                                   .StartsWith(shortPrefix, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Take(128)
                .Select(name => new MixinCompletionItem(name.Substring(name.LastIndexOf('.') + 1), name,
                    "CSharpType", name, string.Empty, Range(0, 0)))
                .ToArray();
        });
    }

    private SemanticTarget ResolveType(string name)
    {
        return ReadLockCookie.Execute(() =>
        {
            var normalized = (name ?? string.Empty).Trim().Replace("global::", string.Empty);
            if (string.IsNullOrEmpty(normalized)) return null;
            var shortName = normalized.Substring(normalized.LastIndexOf('.') + 1);
            var scope = _symbolCache.GetSymbolScope(LibrarySymbolScope.FULL, true);
            var type = scope.GetElementsByShortName(shortName).OfType<ITypeElement>()
                .FirstOrDefault(candidate => string.Equals(
                    candidate.GetClrName().FullName.Replace('+', '.'), normalized,
                    StringComparison.Ordinal));
            var declaration = type?.GetDeclarations().FirstOrDefault();
            var sourceFile = declaration?.GetSourceFile();
            var range = declaration?.GetNavigationRange();
            if (sourceFile == null || range == null || !range.Value.IsValid()) return null;
            var textRange = range.Value.TextRange;
            return new SemanticTarget(sourceFile.GetLocation().FullPath,
                textRange.StartOffset, textRange.EndOffset);
        });
    }

    private static ResolvedDeclaration Resolve(AnalyzedInput origin, MixinEditorReference reference,
        IReadOnlyList<AnalyzedInput> batch)
    {
        var declarationKind = reference.Kind switch
        {
            MixinEditorReferenceKind.Function => MixinEditorSymbolKind.Function,
            MixinEditorReferenceKind.Label => MixinEditorSymbolKind.Label,
            MixinEditorReferenceKind.Local => MixinEditorSymbolKind.Local,
            MixinEditorReferenceKind.Variable => MixinEditorSymbolKind.Variable,
            MixinEditorReferenceKind.TargetVariable => MixinEditorSymbolKind.TargetVariable,
            MixinEditorReferenceKind.Carry => MixinEditorSymbolKind.Carry,
            _ => (MixinEditorSymbolKind?)null
        };
        if (declarationKind == null) return null;

        IEnumerable<AnalyzedInput> files = declarationKind == MixinEditorSymbolKind.Function
            ? batch.Where(candidate => SameDirectory(origin.Input.FilePath, candidate.Input.FilePath))
            : new[] { origin };
        foreach (var file in files)
        foreach (var declaration in file.Analysis.Declarations)
        {
            if (declaration.Kind != declarationKind || declaration.Name != reference.Name) continue;
            if (declarationKind != MixinEditorSymbolKind.Function &&
                (declaration.ScopeStart != reference.ScopeStart || declaration.ScopeEnd != reference.ScopeEnd))
                continue;
            return new ResolvedDeclaration(file, declaration);
        }
        return null;
    }

    private static bool SameDirectory(string left, string right) => string.Equals(
        System.IO.Path.GetDirectoryName(left ?? string.Empty),
        System.IO.Path.GetDirectoryName(right ?? string.Empty), StringComparison.OrdinalIgnoreCase);

    private static void Flatten(MixinSyntaxNode node, int parent, string source,
        ICollection<MixinSyntaxNode> destination)
    {
        var index = destination.Count;
        var range = Clamp(node.SourceRange, source.Length);
        var name = node.Kind is MixinSyntaxKind.DirectiveName or MixinSyntaxKind.Root or
            MixinSyntaxKind.Member or MixinSyntaxKind.Path or MixinSyntaxKind.FunctionCall
            ? source.Substring(range.Start, range.Length)
            : string.Empty;
        destination.Add(new MixinSyntaxNode(parent, node.Kind.ToString(), Range(range), name));
        foreach (var child in node.Children) Flatten(child, index, source, destination);
    }

    private static MixinLanguageDefinition[] Definitions()
    {
        var directives = global::Helix.MixinLanguage.Language.DirectiveLibrary.EnumerateLanguageDefinitions()
          .Select(definition => new MixinLanguageDefinition(
            definition.Name, "Directive", definition.MinimumArguments, definition.MaximumArguments,
            definition.OperandType.ToString(), "None", definition.OperandType.ToString(),
            definition.ArgumentTypes.Select(role => role.ToString()).ToArray(), definition.Documentation));
        var functions = global::Helix.MixinLanguage.Language.FunctionLibrary.Enumerate()
          .Select(definition => new MixinLanguageDefinition(
            definition.Name, definition.IsPredicate ? "Predicate" : "Function",
            definition.MinimumArguments, definition.MaximumArguments, "None",
            definition.ReceiverType.ToString(), definition.ResultType.ToString(),
            definition.ArgumentTypes.Select(role => role.ToString()).ToArray(), definition.Documentation));
        var roots = global::Helix.MixinLanguage.Language.Compiler.MixinLanguageCatalog.Roots.Select(definition => new MixinLanguageDefinition(
            definition.Name, "Root", 0, 0, "None", "None", "Any", Array.Empty<string>(),
            definition.Documentation));
        var outputTargets = global::Helix.MixinLanguage.Language.Compiler.MixinLanguageCatalog.OutputTargets.Select(name => new MixinLanguageDefinition(
            name, "OutputTarget", 0, 0, "None", "None", "None", Array.Empty<string>(),
            "Generated output destination"));
        return directives.Concat(functions).Concat(roots).Concat(outputTargets).ToArray();
    }

    private static CoreRange Clamp(CoreRange range, int length) => new(
        Math.Max(0, Math.Min(length, range.Start)),
        Math.Max(0, Math.Min(length, Math.Max(range.Start, range.End))));

    private static WireRange Range(CoreRange range) => Range(range.Start, range.End);
    private static WireRange Range(int start, int end) => new(start, end);

    // FNV-1a over UTF-16 code units. Kotlin uses the same intentionally simple algorithm.
    private static long SourceHash(string source)
    {
        unchecked
        {
            var hash = 1469598103934665603L;
            foreach (var character in source)
            {
                hash ^= character;
                hash *= 1099511628211L;
            }
            return hash;
        }
    }

    private sealed record AnalyzedInput(MixinFileInput Input, MixinEditorAnalysisResult Analysis);
    private sealed record ResolvedDeclaration(AnalyzedInput Input, MixinEditorSymbol Declaration);
    private sealed record SemanticTarget(string FilePath, int StartOffset, int EndOffset);
}
#endif
