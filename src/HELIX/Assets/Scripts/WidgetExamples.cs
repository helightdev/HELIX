using System.Collections.Generic;
using System.Linq;
using Examples;
using HELIX.Coloring.Material;
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
                    new WidgetNavPage { Buildable = e.Item2.ToBuildable() }
                  );
                }
              ) { new HText(e.Item1) }
            ).Spread(new HGap()),
            new HGap(2)
          }
        }
      }
    ).Stretch().ToBuildable();
  }

  public List<(string, Widget)> Pages { get; } = new() {
    ("Buttons", new ButtonsExample()),
    ("Text Input", new TextInputExample()),
    ("Scroll Controller", new ScrollControllerExample()),
    ("Substances & States", new StateSubstanceExample()),
    ("Theme Tokens", new ThemeTokensExample()),
    ("ListView", new ListVirtualizationExample()),
    ("Prompts", new PromptsExample()),
    ("Red Box", new HBox(background: MaterialColors.Red))
  };

  public override bool Reconcile(Widget updated) {
    base.Reconcile(updated);
    return true;
  }
}