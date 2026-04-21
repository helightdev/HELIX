using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Coloring.Material;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace Examples {
  public class ThemeTokensExample : StatefulWidget<ThemeTokensExample> {
    public override State<ThemeTokensExample> CreateState() {
      return new ThemeTokensExampleState();
    }
  }

  public class ThemeTokensExampleState : State<ThemeTokensExample> {
    private Brightness _brightness = Brightness.Light;
    private bool _warmPalette = true;

    public override Widget Build(BuildContext context) {
      return new HThemeProvider(
        new List<ThemeComponent> {
          new PrimitiveBaseThemeComponent {
            colors = _warmPalette
              ? PrimitiveColorScheme.From(MaterialColors.Red, _brightness)
              : PrimitiveColorScheme.From(MaterialColors.Blue, _brightness)
          }
        }
      ) {
        new HStatefulBuilder(
          (innerContext, _) => BuildContent(innerContext),
          modifiers: new Modifier[] {
            ClipModifier.Clip,
            MarginModifier.Of(16),
            BorderModifier.Of(
              Border.All(1, context.GetThemed(PrimitiveTheme.TextVariant)),
              BorderRadius.All(16)
            )
          }
        ).Tight().Const()
      }.Fill();
    }

    private Widget BuildContent(BuildContext context) {
      var surface = context.GetThemed(PrimitiveTheme.Surface);
      var background = context.GetThemed(PrimitiveTheme.Container);
      var backgroundSubtle = context.GetThemed(PrimitiveTheme.ContainerLow);
      var text = context.GetThemed(PrimitiveTheme.TextVariant);
      var textContrast = context.GetThemed(PrimitiveTheme.Text);
      var typography = context.GetThemed(PrimitiveBaseTheme.Typography);

      return new HColumn(
        gap: 16,
        crossAxisAlign: Align.Stretch,
        modifiers: new Modifier[] {
          PaddingModifier.Of(16), new BackgroundStyle { color = surface },
          new TextStyle { fontSize = typography.FontSize3, color = textContrast }
        }
      ) {
        new HText("Theme provider and token overrides").Heading(context),
        new HText($"Active palette: {(_warmPalette ? "warm" : "cool")}").Body(context),
        new HRow(gap: 8f) {
          new HButton(
            HButtonVariant.TwoState,
            selected: _warmPalette,
            onClick: () => {
              _warmPalette = true;
              SetState();
            }
          ) { new HText("Warm palette") },
          new HButton(
            HButtonVariant.TwoState,
            selected: !_warmPalette,
            onClick: () => {
              _warmPalette = false;
              SetState();
            }
          ) { new HText("Cool palette") },
          new HButton(
            HButtonVariant.TwoState,
            selected: _brightness == Brightness.Dark,
            onClick: () => {
              _brightness = _brightness == Brightness.Light ? Brightness.Dark : Brightness.Light;
              SetState();
            }
          ) { new HText("Dark Mode") }
        },
        new HBox(background: surface, borderRadius: BorderRadius.All(16)) {
          new HColumn(gap: 12, crossAxisAlign: Align.Stretch) {
            SwatchRow(context, "Surface", surface),
            SwatchRow(context, "Background", background),
            SwatchRow(context, "Background subtle", backgroundSubtle),
            SwatchRow(context, "Text", text),
            SwatchRow(context, "Text contrast", textContrast)
          }
        }.Fill().Padding(16)
      };
    }

    private Widget SwatchRow(BuildContext context, string label, Color color) {
      return new HRow(gap: 12, crossAxisAlign: Align.Center) {
        new HBox(
          background: new BackgroundStyle { color = color },
          borderRadius: BorderRadius.All(10),
          border: Border.All(1, context.GetThemed(PrimitiveTheme.TextVariant).WithOpacity(0.5f))
        ).Size(32, 32),
        new HColumn(gap: 2f, crossAxisAlign: Align.FlexStart) {
          new HText(label).Body(context),
          new HText($"#{ColorUtility.ToHtmlStringRGBA(color)}").Caption(context)
        }
      };
    }
  }
}