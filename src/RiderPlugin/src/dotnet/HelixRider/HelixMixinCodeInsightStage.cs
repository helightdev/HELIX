#if RIDER
using System;
using System.Linq;
using JetBrains.Application.Parts;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace HelixRider;

[DaemonStage(Instantiation.ContainerAsyncAnyThreadSafe,
    StagesBefore = new[] { typeof(GlobalFileStructureCollectorStage) },
    StagesAfter = new[] { typeof(LanguageSpecificDaemonStage) },
    HighlightingTypes = new[] { typeof(HelixMixinCodeInsightsHighlighting), typeof(HelixMixinGutterHighlighting) })]
public sealed class HelixMixinCodeInsightStage : CSharpDaemonStageBase
{
    private readonly HelixProjectSettings _settings;
    private readonly HelixMixinContributionCache _cache;
    private readonly HelixMixinCodeInsightProvider _provider;

    public HelixMixinCodeInsightStage(HelixProjectSettings settings, HelixMixinContributionCache cache,
        HelixMixinCodeInsightProvider provider)
    {
        _settings = settings;
        _cache = cache;
        _provider = provider;
    }

    protected override bool IsSupported(IPsiSourceFile sourceFile) =>
        _settings.IsEnabled && sourceFile.IsLanguageSupported<CSharpLanguage>();

    protected override IDaemonStageProcess CreateProcess(IDaemonProcess process,
        IContextBoundSettingsStore settings, DaemonProcessKind processKind, ICSharpFile file) =>
        new Process(process, file, _cache, _provider);

    private sealed class Process : IDaemonStageProcess
    {
        private readonly IDaemonProcess _process;
        private readonly ICSharpFile _file;
        private readonly HelixMixinContributionCache _cache;
        private readonly HelixMixinCodeInsightProvider _provider;

        public Process(IDaemonProcess process, ICSharpFile file,
            HelixMixinContributionCache cache, HelixMixinCodeInsightProvider provider)
        {
            _process = process;
            _file = file;
            _cache = cache;
            _provider = provider;
        }

        public IDaemonProcess DaemonProcess => _process;

        public void Execute(Action<DaemonStageResult> committer)
        {
            var sourceFile = _process.SourceFile;
            var document = sourceFile.Document;
            var contributions = _cache.Request(sourceFile, document.GetText());
            var consumer = new DefaultHighlightingConsumer(sourceFile);
            var gutterOffsets = new System.Collections.Generic.HashSet<int>();
            foreach (var group in contributions.GroupBy(item => new { item.Offset, item.Target }))
            {
                var offset = Math.Max(0, Math.Min(group.Key.Offset, document.GetTextLength()));
                ICSharpDeclaration declaration = null;
                var declarationLength = int.MaxValue;
                var documentOffset = new DocumentOffset(document, offset);
                foreach (var candidate in _file.Descendants<ICSharpDeclaration>())
                {
                    var candidateRange = candidate.GetDocumentRange();
                    if (!candidateRange.Contains(documentOffset))
                        continue;
                    var candidateLength = candidateRange.TextRange.Length;
                    if (candidateLength >= declarationLength)
                        continue;
                    declaration = candidate;
                    declarationLength = candidateLength;
                }
                var range = declaration?.GetNameDocumentRange() ??
                            new DocumentRange(document, new TextRange(offset, offset));
                _provider.AddHighlighting(consumer, range, declaration?.DeclaredElement,
                    group.Key.Target, group.ToList());
                consumer.AddHighlighting(new HelixMixinGutterHighlighting(range,
                    group.Count() == 1 ? "1 mixin hook" : $"{group.Count()} mixin hooks", group.ToList()));
                gutterOffsets.Add(range.StartOffset.Offset);
            }

            foreach (var declaration in _file.Descendants<ICSharpDeclaration>())
            {
                var matching = contributions.Where(item => IsSourceDeclaration(declaration, item)).Distinct().ToList();
                if (matching.Count == 0)
                    continue;
                var range = declaration.GetNameDocumentRange();
                if (!range.IsValid() || !gutterOffsets.Add(range.StartOffset.Offset))
                    continue;
                var tooltip = matching.Count == 1
                    ? "Contributes 1 mixin hook"
                    : $"Contributes {matching.Count} mixin hooks";
                consumer.AddHighlighting(new HelixMixinGutterHighlighting(
                    range, tooltip, matching, showApplicator: false));
            }
            committer(new DaemonStageResult(consumer.CollectHighlightings()));
        }

        private static bool IsSourceDeclaration(ICSharpDeclaration declaration,
            HelixMixinContribution contribution)
        {
            var sourceTypeName = GetSimpleTypeName(contribution.SourceType);
            if (string.IsNullOrEmpty(contribution.SourceMember))
                return contribution.SourceKind == "NamedType" &&
                       declaration is ITypeDeclaration && declaration.DeclaredName == sourceTypeName;

            var expectedName = contribution.SourceMember is ".ctor" or ".cctor"
                ? sourceTypeName
                : contribution.SourceMember;
            if (declaration.DeclaredName != expectedName ||
                declaration.GetContainingTypeDeclaration()?.DeclaredName != sourceTypeName ||
                !KindMatches(declaration, contribution.SourceKind))
                return false;

            return contribution.SourceKind != "Method" ||
                   declaration is IParametersOwnerDeclaration parametersOwner &&
                   parametersOwner.ParameterDeclarations.Count == contribution.SourceParameterCount;
        }

        private static string GetSimpleTypeName(string sourceType)
        {
            var name = sourceType.Substring(sourceType.LastIndexOf('.') + 1);
            name = name.Substring(name.LastIndexOf('+') + 1);
            var genericMarker = name.IndexOf('<');
            return genericMarker < 0 ? name : name.Substring(0, genericMarker);
        }

        private static bool KindMatches(ICSharpDeclaration declaration, string sourceKind) => sourceKind switch
        {
            "Method" => declaration is IMethodDeclaration || declaration is IConstructorDeclaration,
            "Field" => declaration is IFieldDeclaration || declaration is IEnumMemberDeclaration,
            "Property" => declaration is IPropertyDeclaration || declaration is IIndexerDeclaration,
            "Event" => declaration is IEventDeclaration,
            _ => true
        };
    }
}
#endif
