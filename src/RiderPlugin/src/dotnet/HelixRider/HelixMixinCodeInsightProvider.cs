#if RIDER
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Application.Components;
using JetBrains.Application.Parts;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.Application.UI.Controls.GotoByName;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.RdBackend.Common.Features.CodeInsights.Providers;
using JetBrains.RdBackend.Common.Features.Services;
using JetBrains.ReSharper.Daemon.CodeInsights;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.Rider.Model;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.RichText;
using Severity = JetBrains.ReSharper.Feature.Services.Daemon.Severity;

namespace HelixRider;

[SolutionComponent(Instantiation.DemandAnyThreadSafe)]
public sealed class HelixMixinCodeInsightProvider : ICodeInsightsProvider
{
    private readonly ILazy<BulbMenuComponent> _bulbMenu;
    private readonly ConcurrentDictionary<Tuple<IDocument, int>, IReadOnlyList<HelixMixinContribution>>
        _contributionsByLocation = new();

    public HelixMixinCodeInsightProvider(ILazy<BulbMenuComponent> bulbMenu)
    {
        _bulbMenu = bulbMenu;
    }

    public string ProviderId => "helix.mixin.hooks";
    public string DisplayName => "HELIX mixin hooks";
    public CodeVisionAnchorKind DefaultAnchor => CodeVisionAnchorKind.Top;
    public ICollection<CodeVisionRelativeOrdering> RelativeOrderings { get; } =
        new CodeVisionRelativeOrdering[] { new CodeVisionRelativeOrderingBefore(ReferencesCodeInsightsProvider.Id) };

    // Provider discovery happens before the frontend-backed setting is necessarily initialized.
    // Keep the provider registered for click routing and gate emitted highlightings in the stage.
    public bool IsAvailableIn(ISolution solution) => true;

    public void OnClick(CodeInsightHighlightInfo highlightInfo, ISolution solution, CodeInsightsClickInfo clickInfo)
    {
        var highlighting = highlightInfo.CodeInsightsHighlighting;
        var contributions = (highlighting as HelixMixinCodeInsightsHighlighting)?.Contributions;
        if (contributions == null)
            _contributionsByLocation.TryGetValue(
                Tuple.Create(highlighting.Range.Document, highlighting.Range.StartOffset.Offset), out contributions);
        if (contributions == null || contributions.Count == 0)
            return;

        var items = CreateMenuItems(contributions);
        var context = new PopupWindowContextSource(_ => new RiderEditorOffsetPopupWindowContext(
            highlighting.Range.StartOffset.Offset));
        _bulbMenu.Value.ShowBulbMenu(items, context);
    }

    public void OnExtraActionClick(CodeInsightHighlightInfo highlightInfo, string actionId, ISolution solution)
    {
    }

    public void AddHighlighting(IHighlightingConsumer consumer, DocumentRange range,
        IDeclaredElement declaredElement, string target, IReadOnlyList<HelixMixinContribution> contributions)
    {
        var count = contributions.Count;
        var text = count == 1 ? "1 mixin hook" : $"{count} mixin hooks";
        var shortTarget = target.Replace("global::", string.Empty).Split('.').Last();
        _contributionsByLocation[Tuple.Create(range.Document, range.StartOffset.Offset)] = contributions;
        consumer.AddHighlighting(new HelixMixinCodeInsightsHighlighting(
            range, text, $"Show mixed hooks for {shortTarget}", this, declaredElement, contributions));
    }

    internal static List<BulbMenuItem> CreateMenuItems(IReadOnlyList<HelixMixinContribution> contributions,
        bool showApplicator = true) =>
        contributions.OrderBy(item => item.Method, StringComparer.Ordinal)
            .ThenBy(item => item.Priority)
            .ThenBy(item => item.Mixin, StringComparer.Ordinal)
            .Select(item => new BulbMenuItem(
                new ExecutableItem(() => { }),
                new RichText(FormatContribution(item, showApplicator)),
                null,
                BulbMenuAnchors.PermanentBackgroundItems))
            .ToList();

    internal static string FormatContribution(HelixMixinContribution item, bool showApplicator = true)
    {
        var mixin = item.Mixin.Replace("global::", string.Empty).Split('.').Last();
        if (mixin.EndsWith("Attribute", StringComparison.Ordinal))
            mixin = mixin.Substring(0, mixin.Length - "Attribute".Length);
        var applicator = !showApplicator || string.IsNullOrEmpty(item.SourceMember)
            ? string.Empty
            : $" from {item.SourceMember}";
        return $"{item.Method} [{item.Priority}]{applicator} by {mixin}";
    }
}

[RegisterHighlighter(AttributeId,
    GroupId = HighlighterGroupIds.HIDDEN,
    EffectType = EffectType.NONE,
    Layer = HighlighterLayer.SYNTAX + 1,
    NotRecyclable = true,
    TransmitUpdates = true)]
[StaticSeverityHighlighting(Severity.INFO, typeof(HighlightingGroupIds.CodeInsights),
    AttributeId = AttributeId, OverlapResolve = OverlapResolveKind.NONE)]
public sealed class HelixMixinCodeInsightsHighlighting : CodeInsightsHighlighting, ICustomAttributeIdHighlighting
{
    public const string AttributeId = "HELIX Mixin Code Insights";

    public HelixMixinCodeInsightsHighlighting(DocumentRange range, string text, string tooltip,
        ICodeInsightsProvider provider, IDeclaredElement declaredElement,
        IReadOnlyList<HelixMixinContribution> contributions)
        : base(range, text, tooltip, text, provider, declaredElement, null)
    {
        Contributions = contributions;
    }

    public IReadOnlyList<HelixMixinContribution> Contributions { get; }
    string ICustomAttributeIdHighlighting.AttributeId => AttributeId;
}
#endif
