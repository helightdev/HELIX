#if RIDER
using System.Collections.Generic;
using JetBrains.Application.UI.Controls.BulbMenu.Anchors;
using JetBrains.Application.UI.Controls.BulbMenu.Items;
using JetBrains.DocumentModel;
using JetBrains.RdBackend.Common.Features.Icons;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.TextControl.DocumentMarkup;
using JetBrains.UI.Icons;
using JetBrains.UI.RichText;

namespace HelixRider;

[RegisterHighlighter(AttributeId,
    EffectType = EffectType.GUTTER_MARK,
    GutterMarkType = typeof(HelixMixinGutterMark),
    Layer = HighlighterLayer.SYNTAX + 1)]
[StaticSeverityHighlighting(Severity.INFO, typeof(HighlightingGroupIds.GutterMarks),
    AttributeId = AttributeId, OverlapResolve = OverlapResolveKind.NONE)]
public sealed class HelixMixinGutterHighlighting : ICustomAttributeIdHighlighting
{
    public const string AttributeId = "HELIX Mixin Hook Gutter Mark";
    private readonly DocumentRange _range;

    public HelixMixinGutterHighlighting(DocumentRange range, string tooltip,
        IReadOnlyList<HelixMixinContribution> contributions, bool showApplicator = true)
    {
        _range = range;
        ToolTip = tooltip;
        Contributions = contributions;
        Actions = HelixMixinCodeInsightProvider.CreateMenuItems(contributions, showApplicator);
    }

    string ICustomAttributeIdHighlighting.AttributeId => AttributeId;
    public string ToolTip { get; }
    public string ErrorStripeToolTip => ToolTip;
    public IReadOnlyList<HelixMixinContribution> Contributions { get; }
    public IReadOnlyList<BulbMenuItem> Actions { get; }
    public bool IsValid() => _range.IsValid();
    public DocumentRange CalculateRange() => _range;
}

public sealed class HelixMixinGutterMark : IconGutterMarkType
{
    public HelixMixinGutterMark() : base(new FrontendIconId("icons/mixin_contribution.svg"))
    {
    }

    public override IAnchor Priority => BulbMenuAnchors.PermanentBackgroundItems;

    public override IEnumerable<BulbMenuItem> GetBulbMenuItems(IHighlighter highlighter)
    {
        if (highlighter.GetHighlighting() is not HelixMixinGutterHighlighting highlighting)
            return System.Array.Empty<BulbMenuItem>();
        return highlighting.Actions;
    }
}
#endif
