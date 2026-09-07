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
using Mixins;
using Mixins.Compiler;
using WireRange = HelixRider.Protocol.MixinSourceRange;
using CoreRange = Mixins.Compiler.MixinSourceRange;

namespace HelixRider.Protocol;

/// <summary>Semantic RD adapter over the shared ANTLR model. Java ANTLR owns synchronous frontend recognition.</summary>
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
public sealed class HelixMixinLanguageHost {
    private readonly ISymbolCache _symbolCache;
    public HelixMixinLanguageHost(ISolution solution, ISymbolCache symbolCache) {
        _symbolCache = symbolCache;
        var model = solution.GetProtocolSolution().GetHelixExpressionModel();
        model.ParseMixinFiles.SetAsync((_, request) => RdTask.Successful(Parse(request, CompleteTypes, ResolveType)));
        model.GetMixinLanguageCatalog.SetAsync((_, _) => RdTask.Successful(LanguageCatalog()));
    }

    internal static MixinParseResponse Parse(MixinParseRequest request) => Parse(request, null, null);
    internal static MixinLanguageCatalog LanguageCatalog() => new(Definitions());

    private static MixinParseResponse Parse(MixinParseRequest request,
        Func<string, MixinCompletionItem[]> completeTypes, Func<string, SemanticTarget> resolveType) {
        var batch = (request?.Files ?? Array.Empty<MixinFileInput>())
            .Select(input => (Input: input, Analysis: new LanguageAnalysis(input.SourceText ?? string.Empty))).ToArray();
        return new MixinParseResponse(batch.Select(file => {
            var source = file.Input.SourceText ?? string.Empty;
            var siblings = batch.Where(candidate => SameDirectory(file.Input.FilePath, candidate.Input.FilePath)).ToArray();
            var references = file.Analysis.References.Select(reference => {
                if (reference.Kind == "CSharpType") {
                    var type = resolveType?.Invoke(reference.Name);
                    return new MixinReference(reference.Name, reference.Kind, Range(reference.Range),
                        Range(LanguageAnalysis.Scope(reference.Node)), type?.FilePath ?? string.Empty,
                        type == null ? Range(0, 0) : Range(type.StartOffset, type.EndOffset));
                }
                var target = file.Analysis.Resolve(reference, siblings.Select(item => item.Analysis), out var owner);
                var path = target == null ? string.Empty : siblings.First(item => ReferenceEquals(item.Analysis, owner)).Input.FilePath;
                return new MixinReference(reference.Name, reference.Kind, Range(reference.Range),
                    Range(LanguageAnalysis.Scope(reference.Node)), path, target == null ? Range(0, 0) : Range(target.Range));
            }).ToArray();
            var declarations = file.Analysis.Declarations.Select(symbol => new MixinDeclaration(
                symbol.Name, symbol.Kind, Range(symbol.Range), Range(symbol.Scope))).ToArray();
            var diagnostics = file.Analysis.Program.Diagnostics.Select(diagnostic => {
                var token = file.Analysis.Program.Tokens.FirstOrDefault(item => item.Line >= diagnostic.Line);
                return new MixinDiagnostic(diagnostic.Message, "Error",
                    token == null ? Range(source.Length, source.Length) : Range(token.SourceRange));
            }).ToArray();
            var sites = file.Analysis.References.Where(reference => reference.Kind == "CSharpType")
                .Select(reference => new MixinCompletionSite("CSharpType", Range(reference.Range), Range(reference.Range),
                    "Type", completeTypes?.Invoke(reference.Name) ?? Array.Empty<MixinCompletionItem>())).ToArray();
            return new MixinFileSnapshot(file.Input.FilePath ?? string.Empty, file.Input.Revision, SourceHash(source),
                Array.Empty<MixinSyntaxNode>(), Array.Empty<MixinToken>(), declarations, references, diagnostics, sites);
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

    private static MixinLanguageDefinition[] Definitions() {
        var functions = FunctionLibrary.Enumerate().Select(definition => new MixinLanguageDefinition(
            definition.Name, "Function", definition.ArgumentCount,
            definition.IsVariadic, "None", definition.ReceiverType.ToString(), definition.ResultType.ToString(),
            definition.ArgumentTypes.Select(type => type.ToString()).ToArray(), definition.Documentation));
        var roots = MixinRootLibrary.Enumerate().Select(definition => new MixinLanguageDefinition(
            definition.Name, "Root", 0, false, "None", "None", definition.Kind.ToString(), Array.Empty<string>(), definition.Documentation));
        var targets = Enum.GetNames(typeof(MixinExpressionOutputTarget)).Select(name => new MixinLanguageDefinition(
            name, "OutputTarget", 0, false, "None", "None", "None", Array.Empty<string>(), "Generated output destination"));
        return functions.Concat(roots).Concat(targets).ToArray();
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
