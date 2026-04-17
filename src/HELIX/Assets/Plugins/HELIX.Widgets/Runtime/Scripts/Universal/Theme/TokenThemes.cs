using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Types;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Substances;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Theme {
    public class PrimitiveTheme {
        public static readonly ThemeProperty<Color> Surface = ThemeProperty.ExtractMaybe(
            "primitive-c-surface",
            PrimitiveThemeComponent.Default,
            component => component.surface
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.main));

        public static readonly ThemeProperty<Color> BackgroundSubtle = ThemeProperty.ExtractMaybe(
            "primitive-c-background-subtle",
            PrimitiveThemeComponent.Default,
            component => component.backgroundSubtle
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.containerLow));

        public static readonly ThemeProperty<Color> Background = ThemeProperty.ExtractMaybe(
            "primitive-c-background",
            PrimitiveThemeComponent.Default,
            component => component.background
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.container));

        public static readonly ThemeProperty<Color> Text = ThemeProperty.ExtractMaybe(
            "primitive-c-text",
            PrimitiveThemeComponent.Default,
            component => component.text
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.onVariant));

        public static readonly ThemeProperty<Color> TextContrast = ThemeProperty.ExtractMaybe(
            "primitive-c-text-contrast",
            PrimitiveThemeComponent.Default,
            component => component.textContrast
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.onMain));

        public static readonly ThemeProperty<Substance> ButtonFocusLayer = new ThemeProperty<Substance>()
            .Compute(
                ColorSchema(scheme => new BoxSubstance {
                        borderRadius = new AllWidgetStateProperty<BorderRadius>(BorderRadius.All(2)),
                        position = new AllWidgetStateProperty<StyleLength4>(
                            EdgeInsets.Only(bottom: -6, left: 0, right: 0)
                        ),
                        constraints = BoxConstraints.Tight(StyleKeyword.Auto, 4),
                        backgroundStyle = new WidgetStatePropertyMap<BackgroundStyle> {
                            [WidgetState.Disabled] = Colors.Transparent,
                            [WidgetState.Focused | WidgetState.Navigated] = scheme.primary.main,
                            [WidgetState.None] = Colors.Transparent
                        }
                    }
                )
            );

        public static readonly ThemeProperty<HButtonStyle> ButtonTheme = ThemeProperty.ExtractMaybe(
            "primitive-button",
            PrimitiveThemeComponent.Default,
            component => component.button
        ).Compute(element => DefaultButtonStyles.DefaultStyleOf(
                element,
                HButtonVariant.Flat,
                HButtonSize.Regular,
                HInputRadius.Medium
            )
        );

        public static readonly ThemeProperty<HSliderStyle> SliderTheme = new ThemeProperty<HSliderStyle>()
            .Compute(element => HSliderStyle.DefaultStyleOf(element));

        private static Func<ThemeProviderElement, T> ColorSchema<T>(Func<PrimitiveColorScheme, T> func) =>
            element => func(element.Resolve(PrimitiveBaseTheme.Colors));

        public static readonly IReadOnlyList<ThemeProperty> Properties = new ThemeProperty[] {
            Surface, BackgroundSubtle, Background, Text, TextContrast, ButtonTheme
        };
    }

    public class PrimitiveThemeComponent : ThemeComponent {
        public static readonly PrimitiveThemeComponent Default = new();

        public PrimitiveThemeComponent() {
            lookupScope = PrimitiveTheme.Properties;
        }

        public ThemeOptional<Color> surface;
        public ThemeOptional<Color> backgroundSubtle;
        public ThemeOptional<Color> background;
        public ThemeOptional<Color> text;
        public ThemeOptional<Color> textContrast;
        public ThemeOptional<HButtonStyle> button;
    }
}