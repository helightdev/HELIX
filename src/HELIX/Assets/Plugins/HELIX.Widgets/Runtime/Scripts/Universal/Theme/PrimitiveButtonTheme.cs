using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Types;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal.Styles;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Theme {
    public class DefaultButtonStyles {
        public static HButtonStyle DefaultThemeOf(
            BuildContext context,
            RadixPalette palette,
            HButtonVariant variant,
            HButtonSize size,
            HInputRadius rad,
            bool contrast
        ) {
            var typography = context.GetThemed(PrimitiveBaseTheme.Typography);
            var radius = context.GetThemed(PrimitiveBaseTheme.Radius);
            var spacing = context.GetThemed(PrimitiveBaseTheme.Spacing);
            palette ??= context.GetThemed(PrimitiveBaseTheme.AccentColors);
            var borderRadius = rad switch {
                HInputRadius.None   => BorderRadius.None,
                HInputRadius.Small  => BorderRadius.All(radius.Radius1),
                HInputRadius.Medium => BorderRadius.All(radius.Radius2),
                HInputRadius.Large  => BorderRadius.All(radius.Radius3),
                HInputRadius.Full   => BorderRadius.All(9999),
                _                   => throw new ArgumentOutOfRangeException(nameof(rad), rad, null)
            };
            BoxConstraints constraints;
            StyleLength4 padding;
            float fontSize;
            switch (size) {
                case HButtonSize.Small:
                    constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight1);
                    fontSize = typography.FontSize1;
                    padding = variant == HButtonVariant.Ghost
                        ? EdgeInsets.Symmetric(spacing.Space2, spacing.Space1)
                        : EdgeInsets.Symmetric(spacing.Space2, 0);
                    break;
                case HButtonSize.Regular:
                    constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight2);
                    fontSize = typography.FontSize2;
                    padding = variant == HButtonVariant.Ghost
                        ? EdgeInsets.Symmetric(spacing.Space2, spacing.Space1)
                        : EdgeInsets.Symmetric(spacing.Space3, 0);

                    break;
                case HButtonSize.Medium:
                    constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight3);
                    fontSize = typography.FontSize3;
                    padding = variant == HButtonVariant.Ghost
                        ? EdgeInsets.Symmetric(spacing.Space3, spacing.Space1 * 1.5f)
                        : EdgeInsets.Symmetric(spacing.Space4, 0);

                    break;
                case HButtonSize.Large:
                    constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight4);
                    fontSize = typography.FontSize4;
                    padding = variant == HButtonVariant.Ghost
                        ? EdgeInsets.Symmetric(spacing.Space4, spacing.Space2)
                        : EdgeInsets.Symmetric(spacing.Space5, 0);

                    break;
                default: throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }
            return variant switch {
                HButtonVariant.Default => CreateSolid(palette, contrast, constraints, borderRadius, fontSize, padding),
                HButtonVariant.Solid   => CreateSolid(palette, contrast, constraints, borderRadius, fontSize, padding),
                HButtonVariant.Soft    => CreateSoft(palette, contrast, constraints, borderRadius, fontSize, padding),
                HButtonVariant.Outline => CreateSolid(palette, contrast, constraints, borderRadius, fontSize, padding),
                HButtonVariant.Ghost   => CreateSolid(palette, contrast, constraints, borderRadius, fontSize, padding),
                _                    => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
            };
        }

        public static HButtonStyle CreateSolid(
            RadixPalette schema,
            bool contrast,
            BoxConstraints constraints,
            BorderRadius rad,
            float fontSize,
            StyleLength4 padding
        ) {
            return new HButtonStyle {
                opacity = new WidgetStatePropertyMap<float> {
                    [WidgetState.Disabled] = 0.5f,
                    [WidgetState.None] = 1f
                },
                padding = new AllWidgetStateProperty<StyleLength4>(padding),
                borderRadius = new AllWidgetStateProperty<BorderRadius>(rad),
                constraints = new AllWidgetStateProperty<BoxConstraints>(constraints),
                textStyle = new AllWidgetStateProperty<TextStyle>(
                    new TextStyle {
                        fontSize = fontSize,
                        color = schema.C1
                    }
                ),
                backgroundStyle = new WidgetStatePropertyMap<BackgroundStyle> {
                    [WidgetState.ModAny | WidgetState.Pressed | WidgetState.Selected] =
                        contrast ? Colors.AlphaBlend(schema.C12, schema.C1.WithOpacity(0.2f)) : schema.C11,
                    [WidgetState.Hovered] =
                        contrast ? Colors.AlphaBlend(schema.C12, schema.C1.WithOpacity(0.1f)) : schema.C10,
                    [WidgetState.None] = contrast ? schema.C12 : schema.C9
                }
            };
        }

        public static HButtonStyle CreateSoft(
            RadixPalette schema,
            bool contrast,
            BoxConstraints constraints,
            BorderRadius rad,
            float fontSize,
            StyleLength4 padding
        ) {
            return new HButtonStyle {
                opacity = new WidgetStatePropertyMap<float> {
                    [WidgetState.Disabled] = 0.5f,
                    [WidgetState.None] = 1f
                },
                padding = new AllWidgetStateProperty<StyleLength4>(padding),
                borderRadius = new AllWidgetStateProperty<BorderRadius>(rad),
                constraints = new AllWidgetStateProperty<BoxConstraints>(constraints),
                textStyle = new AllWidgetStateProperty<TextStyle>(
                    new TextStyle {
                        fontSize = fontSize,
                        color = contrast ? schema.C12 : schema.C11
                    }
                ),
                backgroundStyle = new WidgetStatePropertyMap<BackgroundStyle> {
                    [WidgetState.ModAny  | WidgetState.Selected  | WidgetState.Pressed] = schema.C5,
                    [WidgetState.Hovered] = schema.C4,
                    [WidgetState.None] = schema.C3
                },
            };
        }
    }
}