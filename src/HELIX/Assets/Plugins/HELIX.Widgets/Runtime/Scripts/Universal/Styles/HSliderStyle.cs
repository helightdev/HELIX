using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal.Substances;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Styles {
    public class HSliderStyle : DiagnosticableBase {
        public SubstanceLayers track = default;
        public SubstanceLayers progress = default;
        public SubstanceLayers thumb = default;
        public WidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<object>("track", track));
            properties.Add(new DiagnosticsProperty<object>("progress", progress));
            properties.Add(new DiagnosticsProperty<object>("thumb", thumb));
        }

        public static HSliderStyle DefaultStyleOf(
            IThemeProvider context,
            ColorTokenPalette palette = null,
            SurfaceColorPalette surfacePalette = null
        ) {
            var typography = context.GetThemed(PrimitiveBaseTheme.Typography);
            var radius = context.GetThemed(PrimitiveBaseTheme.Radius);
            var spacing = context.GetThemed(PrimitiveBaseTheme.Spacing);
            var colors = context.GetThemed(PrimitiveBaseTheme.Colors);
            palette ??= colors.primary;
            surfacePalette ??= colors.surface;

            var trackPosition = new WidgetStatePropertyMap<StyleLength4> {
                [WidgetState.Special1] =
                    EdgeInsets.Only(bottom: 25.Percent(), top: 25.Percent(), left: 0, right: 0),
                [WidgetState.Special2] = EdgeInsets.Only(
                    bottom: 0,
                    top: 0,
                    left: 25.Percent(),
                    right: 25.Percent()
                ),
            };

            return new HSliderStyle {
                constraints = new WidgetStatePropertyMap<BoxConstraints>() {
                    [WidgetState.Special1] = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight2),
                    [WidgetState.Special2] = BoxConstraints.Tight(typography.LineHeight2, StyleKeyword.Auto),
                },
                track = new SubstanceBuilder(context as BuildContext)
                    .Append(_ => {
                            return new BoxSubstance {
                                borderRadius = BorderRadius.All(radius.Radius1),
                                position = trackPosition,
                                backgroundStyle = new BackgroundStyle { color = palette.container }
                            };
                        }
                    )
                    .Build(),
                progress = new SubstanceBuilder(context as BuildContext)
                    .Append(_ => new BoxSubstance {
                            position = trackPosition,
                            borderRadius = BorderRadius.All(radius.Radius1),
                            backgroundStyle = new BackgroundStyle { color = palette.main }
                        }
                    )
                    .Build(),
                thumb = new SubstanceBuilder(context as BuildContext)
                    .Append(_ => new BoxSubstance {
                            borderRadius = BorderRadius.All(radius.Radius2),
                            backgroundStyle = new WidgetStatePropertyMap<BackgroundStyle> {
                                [WidgetState.Focused | WidgetState.Navigated] = palette.main,
                                [WidgetState.None] = palette.main,
                            },
                            border = new WidgetStatePropertyMap<Border> {
                                [WidgetState.Focused | WidgetState.Navigated] =
                                    Border.All(spacing.Space1 * 0.75f, surfacePalette.inverse),
                                [WidgetState.None] = Border.All(spacing.Space1 * 0.5f, palette.onMain)
                            }
                        }
                    )
                    .Build()
            };
        }

        public static HSliderStyle DefaultScrollbarStyleOf(BuildContext context) {
            var typography = context.GetThemed(PrimitiveBaseTheme.Typography);
            var radius = context.GetThemed(PrimitiveBaseTheme.Radius);
            var spacing = context.GetThemed(PrimitiveBaseTheme.Spacing);
            var colors = context.GetThemed(PrimitiveBaseTheme.Colors);

            return new HSliderStyle {
                constraints = new WidgetStatePropertyMap<BoxConstraints>() {
                    [WidgetState.Special1] = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight2),
                    [WidgetState.Special2] = BoxConstraints.Tight(typography.LineHeight2, StyleKeyword.Auto),
                },
                track = new SubstanceBuilder(context)
                    .Append(_ => new BoxSubstance {
                            borderRadius = BorderRadius.All(radius.Radius2),
                            backgroundStyle = new BackgroundStyle {
                                color = colors.surface.containerHigh.WithOpacity(1 - colors.layerOpacityProgression.low)
                            }
                        }
                    )
                    .Build(),
                progress = new SubstanceLayers(),
                thumb = new SubstanceBuilder(context)
                    .Append(_ => new BoxSubstance {
                            borderRadius = BorderRadius.All(radius.Radius2),
                            position = EdgeInsets.All(spacing.Space1),
                            backgroundStyle = new WidgetStatePropertyMap<BackgroundStyle> {
                                [WidgetState.Focused | WidgetState.Navigated] = colors.primary.main,
                                [WidgetState.None] =
                                    colors.surface.onMain.WithOpacity(colors.layerOpacityProgression.high),
                            }
                        }
                    )
                    .Build()
            };
        }
    }
}