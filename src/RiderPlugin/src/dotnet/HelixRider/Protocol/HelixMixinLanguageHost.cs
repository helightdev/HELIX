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
using Hix.Standalone.Analysis;
using WireRange = HelixRider.Protocol.MixinSourceRange;
using CoreRange = Hix.Compiler.HixSourceRange;

namespace HelixRider.Protocol;

/// <summary>Semantic RD adapter over the shared ANTLR model. Java ANTLR owns synchronous frontend recognition.</summary>
[SolutionComponent(Instantiation.ContainerAsyncAnyThreadSafe)]
public sealed class HelixMixinLanguageHost {
    private readonly ISymbolCache _symbolCache;
    private readonly IReadOnlyDictionary<string, HixAnalyzerService> _analyzers;
    public HelixMixinLanguageHost(ISolution solution, ISymbolCache symbolCache) {
        _symbolCache = symbolCache;
        _analyzers = new Dictionary<string, HixAnalyzerService>(StringComparer.OrdinalIgnoreCase) {
            ["Unity"] = NewAnalyzer(new RiderAnalyzerHost(this)),
            ["Standalone"] = new HixAnalyzerService()
        };
        var model = solution.GetProtocolSolution().GetHelixExpressionModel();
        model.ParseMixinFiles.SetAsync((_, request) => RdTask.Successful(Parse(Analyzer(request.Backend), request)));
        model.CompleteMixin.SetAsync((_, request) => RdTask.Successful(new MixinCompletionResponse(
            Analyzer(request.Backend).Complete(request.Kind, request.Prefix).Select(Completion).ToArray())));
        model.GetMixinLanguageCatalog.SetAsync((_, backend) => RdTask.Successful(LanguageCatalog(Analyzer(backend))));
    }

    private HixAnalyzerService Analyzer(string name) =>
        _analyzers.TryGetValue(name ?? string.Empty, out var analyzer) ? analyzer : _analyzers["Standalone"];

    internal static MixinParseResponse Parse(MixinParseRequest request) =>
        Parse(NewAnalyzer(), request);
    internal static MixinLanguageCatalog LanguageCatalog() =>
        LanguageCatalog(NewAnalyzer());

    private static HixAnalyzerService NewAnalyzer(IHixAnalyzerHost host = null) =>
        new(Hix.HixMixinBackend.Instance, host, AdditionalDefinitions());

    private static MixinParseResponse Parse(HixAnalyzerService analyzer, MixinParseRequest request) =>
        new(analyzer.Synchronize((request?.Files ?? Array.Empty<MixinFileInput>()).Select(input =>
            new HixDocument(input.FilePath, input.SourceText, input.Revision))).Select(Snapshot).ToArray());

    private static MixinLanguageCatalog LanguageCatalog(HixAnalyzerService analyzer) =>
        new(analyzer.Definitions.Select(Definition).ToArray());

    private static MixinFileSnapshot Snapshot(HixDocumentSnapshot value) => new(value.Path, value.Revision,
        value.SourceHash, Array.Empty<MixinSyntaxNode>(), Array.Empty<MixinToken>(),
        value.Declarations.Select(item => new MixinDeclaration(item.Name, item.Kind, Range(item.Range),
            Range(item.Scope))).ToArray(), value.References.Select(item => new MixinReference(item.Name, item.Kind,
            Range(item.Range), Range(item.Scope), item.Target?.Path ?? string.Empty,
            item.Target == null ? Range(0, 0) : Range(item.Target.Range))).ToArray(),
        value.Diagnostics.Select(item => new MixinDiagnostic(item.Message, item.Severity, Range(item.Range))).ToArray(),
        value.CompletionSites.Select(item => new MixinCompletionSite(item.Kind, Range(item.ActivationRange),
            Range(item.ReplacementRange), item.ExpectedType, Array.Empty<MixinCompletionItem>())).ToArray(),
        value.TypeFacts.Select(item => new MixinTypeFact(Range(item.Range), item.Type, item.Documentation,
            item.Inlay, item.Kind)).ToArray());

    private static MixinLanguageDefinition Definition(HixDefinition value) => new(value.Name, value.Kind,
        value.ArgumentCount, value.Variadic, value.OperandType, value.ReceiverType, value.ResultType,
        value.ArgumentTypes.ToArray(), value.Documentation);
    private static MixinCompletionItem Completion(HixCompletion value) => new(value.Name, value.InsertText,
        value.Kind, value.Documentation, value.Target?.Path ?? string.Empty,
        value.Target == null ? Range(0, 0) : Range(value.Target.Range));

    private static IEnumerable<HixDefinition> AdditionalDefinitions() {
        foreach (var definition in HixRootLibrary.Enumerate())
            yield return new HixDefinition(definition.Name, "Root", 0, false, "None", "None",
                definition.Kind.ToString(), Array.Empty<string>(), definition.Documentation);
        foreach (var name in Enum.GetNames(typeof(MixinEmissionTarget)))
            yield return new HixDefinition(name, "OutputTarget", 0, false, "None", "None", "None",
                Array.Empty<string>(), "Generated output destination");
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

    private HixLocation ResolveType(string name)
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
            return new HixLocation(sourceFile.GetLocation().FullPath,
                new CoreRange(textRange.StartOffset, textRange.EndOffset));
        });
    }

    private static WireRange Range(CoreRange range) => Range(range.Start, range.End);
    private static WireRange Range(int start, int end) => new(start, end);
    private sealed class RiderAnalyzerHost(HelixMixinLanguageHost owner) : IHixAnalyzerHost {
        public HixLocation Resolve(string kind, string name) =>
            kind == "CSharpType" ? owner.ResolveType(name) : null;
        public IReadOnlyList<HixCompletion> Complete(string kind, string prefix) =>
            kind == "CSharpType" ? owner.CompleteTypes(prefix).Select(item => new HixCompletion(item.Name,
                item.InsertText, item.Kind, item.Documentation,
                string.IsNullOrEmpty(item.TargetFilePath) ? null : new HixLocation(item.TargetFilePath,
                    new CoreRange(item.TargetRange.StartOffset, item.TargetRange.EndOffset)))).ToArray() : [];
    }
}
#endif
