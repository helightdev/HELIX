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
using Hix;
using Hix.Compiler;
using Hix.Mixins;
using WireRange = HelixRider.Protocol.MixinSourceRange;
using CoreRange = Hix.Compiler.HixSourceRange;

namespace HelixRider.Protocol;

/// <summary>Semantic RD adapter over the shared ANTLR model. Java ANTLR owns synchronous frontend recognition.</summary>
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
public sealed class HelixMixinLanguageHost {
    private readonly ISymbolCache _symbolCache;
    public HelixMixinLanguageHost(ISolution solution, ISymbolCache symbolCache) {
        _symbolCache = symbolCache;
        var model = solution.GetProtocolSolution().GetHelixExpressionModel();
        model.ParseMixinFiles.SetAsync((_, request) => RdTask.Successful(Parse(request, ResolveType)));
        model.CompleteMixin.SetAsync((_, request) => RdTask.Successful(new MixinCompletionResponse(
            request.Kind == "CSharpType" ? CompleteTypes(request.Prefix) : Array.Empty<MixinCompletionItem>())));
        model.GetMixinLanguageCatalog.SetAsync((_, _) => RdTask.Successful(LanguageCatalog()));
    }

    internal static MixinParseResponse Parse(MixinParseRequest request) => Parse(request, null);
    internal static MixinLanguageCatalog LanguageCatalog() => new(Definitions());

    private static MixinParseResponse Parse(MixinParseRequest request, Func<string, SemanticTarget> resolveType) {
        var batch = (request?.Files ?? Array.Empty<MixinFileInput>())
            .Select(input => (Input: input, Analysis: new LanguageAnalysis(input.SourceText ?? string.Empty,
                Hix.HixMixinBackend.Instance, recoverValidDeclarations: true))).ToArray();
        return new MixinParseResponse(batch.Select(file => {
            var source = file.Input.SourceText ?? string.Empty;
            var siblings = batch.Where(candidate => SameDirectory(file.Input.FilePath, candidate.Input.FilePath)).ToArray();
            var siblingPatterns = siblings.SelectMany(sibling => sibling.Analysis.Declarations)
                .Where(declaration => declaration.Kind == "Pattern").Select(declaration => declaration.Name)
                .ToHashSet(StringComparer.Ordinal);
            var references = file.Analysis.References.Select(reference => {
                if (reference.Kind == "CSharpType") {
                    var type = resolveType?.Invoke(reference.Name);
                    return new MixinReference(reference.Name, reference.Kind, Range(reference.Range),
                        Range(LanguageAnalysis.Scope(reference.Node)), type?.FilePath ?? string.Empty,
                        type == null ? Range(0, 0) : Range(type.StartOffset, type.EndOffset));
                }
                var effective = reference.Kind == "Function" && siblingPatterns.Contains(reference.Name)
                    ? new LanguageAnalysis.Reference(reference.Name, "Pattern", reference.Range, reference.Node)
                    : reference;
                var target = file.Analysis.Resolve(effective, siblings.Select(item => item.Analysis), out var owner);
                var path = target == null ? string.Empty : siblings.First(item => ReferenceEquals(item.Analysis, owner)).Input.FilePath;
                return new MixinReference(effective.Name, effective.Kind, Range(effective.Range),
                    Range(LanguageAnalysis.Scope(reference.Node)), path, target == null ? Range(0, 0) : Range(target.Range));
            }).ToArray();
            var declarations = file.Analysis.Declarations.Select(symbol => new MixinDeclaration(
                symbol.Name, symbol.Kind, Range(symbol.Range), Range(symbol.Scope))).ToArray();
            var diagnostics = file.Analysis.Program.Diagnostics.Where(diagnostic =>
                !ResolvedPatternDiagnostic(diagnostic.Message, siblingPatterns)).Select(diagnostic => {
                var token = file.Analysis.Program.Tokens.FirstOrDefault(item => item.Line >= diagnostic.Line);
                return new MixinDiagnostic(diagnostic.Message, "Error",
                    token == null ? Range(source.Length, source.Length) : Range(HixSourceRange.FromToken(token)));
            }).ToArray();
            var typeSites = file.Analysis.References.Where(reference => reference.Kind == "CSharpType")
                .Select(reference => new MixinCompletionSite("CSharpType", Range(reference.Range), Range(reference.Range),
                    "Type", Array.Empty<MixinCompletionItem>()));
            var patternSites = references.Where(reference => reference.Kind == "Pattern")
                .Select(reference => new MixinCompletionSite("Pattern", reference.Range, reference.Range,
                    "Pattern", Array.Empty<MixinCompletionItem>()));
            var sites = typeSites.Concat(patternSites).ToArray();
            var typeFacts = file.Analysis.TypeFacts.Select(fact => new MixinTypeFact(
                Range(fact.Range), fact.Type, fact.Documentation ?? string.Empty, fact.Inlay, fact.Kind)).ToArray();
            return new MixinFileSnapshot(file.Input.FilePath ?? string.Empty, file.Input.Revision, SourceHash(source),
                Array.Empty<MixinSyntaxNode>(), Array.Empty<MixinToken>(), declarations, references, diagnostics, sites, typeFacts);
        }).ToArray());
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


    private static bool SameDirectory(string left, string right) => string.Equals(
        System.IO.Path.GetDirectoryName(left ?? string.Empty),
        System.IO.Path.GetDirectoryName(right ?? string.Empty), StringComparison.OrdinalIgnoreCase);

    private static bool ResolvedPatternDiagnostic(string message, ISet<string> patterns) {
        const string prefix = "unknown pattern '";
        return message != null && message.StartsWith(prefix, StringComparison.Ordinal) && message.EndsWith("'", StringComparison.Ordinal) &&
               patterns.Contains(message.Substring(prefix.Length, message.Length - prefix.Length - 1));
    }

    private static MixinLanguageDefinition[] Definitions() {
        var functions = Hix.HixMixinBackend.Instance.Functions.Enumerate().SelectMany(definition =>
            definition.Signatures.Select(signature => new MixinLanguageDefinition(
                definition.Name, "Function", signature.ArgumentCount, signature.IsVariadic, "None",
                (signature.ArgumentTypes.Count == 0 ? HixValueKind.Any : signature.ArgumentTypes[0]).ToString(),
                signature.ResultType.ToString(), signature.ArgumentTypes.Select(type => type.ToString()).ToArray(),
                definition.Documentation)));
        var roots = HixRootLibrary.Enumerate().Select(definition => new MixinLanguageDefinition(
            definition.Name, "Root", 0, false, "None", "None", definition.Kind.ToString(), Array.Empty<string>(), definition.Documentation));
        var targets = Enum.GetNames(typeof(MixinEmissionTarget)).Select(name => new MixinLanguageDefinition(
            name, "OutputTarget", 0, false, "None", "None", "None", Array.Empty<string>(), "Generated output destination"));
        var hostRoots = Hix.HixMixinBackend.Instance.Roots.Values.Select(root => new MixinLanguageDefinition(
            root.Name, "Root", 0, false, "None", "None", root.Kind.ToString(), Array.Empty<string>(), "Backend root"));
        var kinds = Enum.GetValues(typeof(HixValueKind)).Cast<HixValueKind>().Select(kind =>
            new MixinLanguageDefinition(kind.ToString().ToLowerInvariant(), "Kind", 0, false, "None", "None",
                "Kind", Array.Empty<string>(), "Matches values of kind `" + kind.ToString().ToLowerInvariant() + "`."));
        var metadata = HixPatternMetadata.Definitions.Select(definition => new MixinLanguageDefinition(
            definition.Name, "PatternMetadata", definition.ArgumentKinds.Count, definition.Variadic,
            definition.Name == "optional" ? "Field" : "Pattern", "None", "Pattern",
            definition.ArgumentKinds.Select(kind => kind.ToString()).ToArray(),
            definition.Documentation));
        var fileMetadata = HixFileMetadata.Definitions.Select(definition => new MixinLanguageDefinition(
            definition.Name, "FileMetadata", definition.ArgumentTypes.Count, false, "File", "None", "None",
            definition.ArgumentTypes.ToArray(), definition.Documentation));
        return functions.Concat(roots).Concat(hostRoots).Concat(targets).Concat(kinds).Concat(metadata)
            .Concat(fileMetadata).ToArray();
    }

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


    private sealed record SemanticTarget(string FilePath, int StartOffset, int EndOffset);
}
#endif
