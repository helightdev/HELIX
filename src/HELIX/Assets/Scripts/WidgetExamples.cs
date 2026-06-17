using System.Linq;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Navigation;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.UIElements;

[UxmlElement]
public partial class WidgetExamples : WidgetHostElement {
  private readonly GlobalKey<NavStackElement> _navStackKey = new();

  public WidgetExamples() {
    ShowGallery();
  }

  public bool ShowPreview(string previewId) {
    if (string.IsNullOrWhiteSpace(previewId)) {
      ShowGallery();
      return true;
    }

    var normalized = NormalizePreviewId(previewId);
    var page = Pages.FirstOrDefault(e => NormalizePreviewId(e.Id) == normalized || NormalizePreviewId(e.Title) == normalized);
    if (page.Create == null) return false;

    Buildable = PreviewFrame(page.Create()).Stretch().ToBuildable();
    return true;
  }

  public void ShowGallery() {
    Buildable = new HStatefulBuilder((context, state) =>
      new HColumn(
        modifiers: new Modifier[] {
          new BackgroundStyleModifier(context.GetThemed(PrimitiveTheme.Surface))
        }
      ) {
        new HNavStack(key: _navStackKey).Fill(),
        new HScrollView(
          Axis.Horizontal,
          modifiers: new Modifier[] {
            FlexibleModifier.TightStretch,
            MarginModifier.Only(bottom: 8, top: 8)
          }
        ) {
          new HRow {
            new HGap(2),
            Pages.Select(e =>
              new HButton(
                onClick: () => {
                  _navStackKey.Element.PushReplacement(
                    new WidgetNavPage { Buildable = PreviewFrame(e.Create()).ToBuildable() }
                  );
                }
              ) { new HText(e.Title) }
            ).Spread(new HGap()),
            new HGap(2)
          }
        }
      }
    ).Stretch().ToBuildable();
  }

  public override bool Reconcile(Widget updated) {
    base.Reconcile(updated);
    return true;
  }
}
