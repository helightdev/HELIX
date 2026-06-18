using System;
using System.Collections.Generic;
using Examples;
using HELIX.Coloring;
using HELIX.Coloring.Material;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;
using UnityEngine.UIElements;

public partial class WidgetExamples {
  public static List<(string Id, string Title, Func<Widget> Create)> Pages { get; } = new() {
    ("layout", "Layout", BuildLayoutPreview),
    ("text-icons", "Text & Icons", BuildTextIconPreview),
    ("buttons", "Buttons", () => new ButtonsExample()),
    ("text-input", "Text Input", () => new TextInputExample()),
    ("sliders", "Sliders", BuildSliderPreview),
    ("scroll-controller", "Scroll Controller", () => new ScrollControllerExample()),
    ("state-substance", "Substances & States", () => new StateSubstanceExample()),
    ("theme-tokens", "Theme Tokens", () => new ThemeTokensExample()),
    ("list-view", "ListView", () => new ListVirtualizationExample()),
    ("prompts", "Prompts", () => new PromptsExample()),
    ("icons", "Icons", () => new IconExample()),
    ("red-box", "Red Box", () => new HBox(background: MaterialColors.Red)),
    ("hosted-widget", "Hosted Widget", () => new HostedWidgetExamples()),
    ("redirectable-signal-listener", "Redirectable Signal Listener", () => new RedirectableSignalListenerExample())
  };

  private static Widget PreviewFrame(Widget preview) {
    var colors = PrimitiveColorScheme.From(Colors.Hex("#e56772"), Brightness.Dark);
    colors.primary = ColorTokenPalette.Simple(
      Colors.Hex("#e66670"),
      Colors.Hex("#ebebeb"),
      Colors.Hex("#2d2021"),
      Colors.Hex("#e66670")
    );
    colors.secondary = ColorTokenPalette.Simple(
      Colors.Hex("#715f61"),
      Colors.Hex("#fefefe"),
      Colors.Hex("#302b2b"),
      Colors.Hex("#fffefe")
    );
    colors.tertiary = colors.secondary;

    colors.surface = SurfaceColorPalette.Simple(
      Colors.Hex("#121212"),
      Colors.Hex("#191919"),
      Colors.Hex("#212121"),
      Colors.Hex("#232323"),
      Colors.Hex("#ebebeb"),
      Colors.Hex("#b8b8b8")
    );
    colors.outline = Colors.White.WithOpacity(0.39f);

    return new HThemeProvider(
      new List<ThemeComponent> {
        new PrimitiveBaseThemeComponent {
          // colors = PrimitiveColorScheme.From(MaterialColors.Indigo, Brightness.Dark)
          colors = colors
        }
      }
    ) {
      new HStatefulBuilder((context, _) =>
        new HBox(background: context.GetThemed(PrimitiveTheme.Surface)) { preview }.Fill()
      ).Stretch()
    }.Fill();
  }

  private static Widget BuildLayoutPreview() {
    return new HStatefulBuilder((context, _) => {
        var container = context.GetThemed(PrimitiveTheme.Container);
        var containerLow = context.GetThemed(PrimitiveTheme.ContainerLow);
        var border = Border.All(1, context.GetThemed(PrimitiveTheme.TextVariant));

        return new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
          new HText("Layout primitives").Heading(context),
          new HRow(gap: 12) {
            PreviewTile(
              context,
              "HColumn",
              new HColumn(gap: 6, crossAxisAlign: Align.Stretch) {
                new HBox(background: containerLow, borderRadius: 8).Size(height: 18),
                new HBox(background: containerLow, borderRadius: 8).Size(height: 18),
                new HBox(background: containerLow, borderRadius: 8).Size(height: 18)
              }
            ).Expand(),
            PreviewTile(
              context,
              "HRow",
              new HRow(gap: 6, crossAxisAlign: Align.Stretch) {
                new HBox(background: containerLow, borderRadius: 8).Size(width: 36),
                new HBox(background: containerLow, borderRadius: 8).Size(width: 36),
                new HBox(background: containerLow, borderRadius: 8).Size(width: 36)
              }
            ).Expand(),
            PreviewTile(
              context,
              "HCenter",
              new HCenter {
                new HBox(background: containerLow, borderRadius: 8).Size(88, 36)
              }
            ).Expand()
          },
          new HRow(gap: 12, crossAxisAlign: Align.Stretch) {
            new HBox(background: container, border: border, borderRadius: 16) {
              new HAlign(Alignment.BottomRight) {
                new HBox(background: containerLow, borderRadius: 8) {
                  new HText("HAlign").Body(context)
                }.Size(112, 40)
              }
            }.Padding(16).Expand(),
            new HBoxShadow {
              new HBox(background: container, borderRadius: 16) {
                new HCenter { new HText("HBoxShadow").Body(context) }
              }
            }.Expand()
          }.Size(height: 104)
        }.Margin(16);
      }
    ).Stretch();
  }

  private static Widget BuildTextIconPreview() {
    return new HStatefulBuilder((context, _) => {
        var typography = context.GetThemed(PrimitiveBaseTheme.Typography);
        var colors = context.GetThemed(PrimitiveBaseTheme.Colors);

        return new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
          new HText("Text and icon styles").Heading(context),
          new HText("Display text").Display(context),
          new HText("Body copy follows the active theme typography and color tokens.").Body(context),
          new HText("Caption text is intended for secondary metadata.").Caption(context),
          new HRow(gap: 18, crossAxisAlign: Align.Center) {
            new HIcon(
              FaSolidIcons.AddressBook,
              FaSolidIcons.FontDefinition,
              size: typography.FontSize6,
              color: colors.primary.main
            ),
            new HIcon(
              FaSolidIcons.MagnifyingGlass,
              FaSolidIcons.FontDefinition,
              size: typography.FontSize6,
              color: colors.secondary.main
            ),
            new HIcon(
              FaSolidIcons.FloppyDisk,
              FaSolidIcons.FontDefinition,
              size: typography.FontSize6,
              color: colors.tertiary.main
            )
          }
        }.Margin(16);
      }
    ).Stretch();
  }

  private static Widget BuildSliderPreview() {
    var horizontal = 0.65f;
    var vertical = 0.35f;

    return new HStatefulBuilder((context, state) =>
      new HColumn(gap: 16, crossAxisAlign: Align.Stretch) {
        new HText("Sliders").Heading(context),
        new HText($"Horizontal value: {Mathf.RoundToInt(horizontal * 100)}%").Body(context),
        new HSlider(
          initialValue: horizontal,
          onChanged: value => {
            horizontal = value;
            state.SetState();
          }
        ).TightStretch(),
        new HRow(gap: 16) {
          new HSlider(
            axis: Axis.Vertical,
            initialValue: vertical,
            onChanged: value => {
              vertical = value;
              state.SetState();
            }
          ).TightStretch(),
          new HText($"Vertical value: {Mathf.RoundToInt(vertical * 100)}%").Body(context)
        }.Fill()
      }.Margin(16)
    ).Stretch();
  }

  private static Widget PreviewTile(BuildContext context, string title, Widget child) {
    return new HBox(
      background: context.GetThemed(PrimitiveTheme.Container),
      border: Border.All(1, context.GetThemed(PrimitiveTheme.TextVariant)),
      borderRadius: BorderRadius.All(14)
    ) {
      new HColumn(gap: 10, crossAxisAlign: Align.Stretch) {
        new HText(title).Body(context),
        child.Fill()
      }.Padding(12)
    }.Size(height: 200, width: Length.Auto());
  }

  private static string NormalizePreviewId(string value) {
    return value.Trim().ToLowerInvariant().Replace("_", "-").Replace(" ", "-");
  }
}