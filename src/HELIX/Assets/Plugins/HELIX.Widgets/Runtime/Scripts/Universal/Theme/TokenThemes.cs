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

        public static readonly ThemeProperty<Color> ContainerLow = ThemeProperty.ExtractMaybe(
            "primitive-c-container-low",
            PrimitiveThemeComponent.Default,
            component => component.backgroundSubtle
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.containerLow));

        public static readonly ThemeProperty<Color> Container = ThemeProperty.ExtractMaybe(
            "primitive-c-container",
            PrimitiveThemeComponent.Default,
            component => component.background
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.container));

        public static readonly ThemeProperty<Color> TextVariant = ThemeProperty.ExtractMaybe(
            "primitive-c-text-variant",
            PrimitiveThemeComponent.Default,
            component => component.text
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.onVariant));

        public static readonly ThemeProperty<Color> Text = ThemeProperty.ExtractMaybe(
            "primitive-c-text",
            PrimitiveThemeComponent.Default,
            component => component.textContrast
        ).StyleLoader().Compute(ColorSchema(elem => elem.surface.onMain));

        public static readonly ThemeProperty<Substance> ButtonFocusLayer = ThemeProperty.ExtractMaybe(
            "primitive-button-focus-layer",
            PrimitiveThemeComponent.Default,
            component => component.buttonFocusLayer
        ).Compute(
            ColorSchema(scheme => new BoxSubstance {
                    borderRadius = new AllWidgetStateProperty<BorderRadius>(BorderRadius.All(2)),
                    position = new AllWidgetStateProperty<StyleLength4>(EdgeInsets.Only(bottom: -6, left: 0, right: 0)),
                    constraints = BoxConstraints.Tight(StyleKeyword.Auto, 4),
                    background = new WidgetStatePropertyMap<BackgroundStyle> {
                        [WidgetState.Disabled] = Colors.Transparent,
                        [WidgetState.Focused | WidgetState.Navigated] = scheme.primary.main,
                        [WidgetState.None] = Colors.Transparent
                    }
                }
            )
        );

        public static readonly ThemeProperty<HButtonStyle> Button = ThemeProperty.ExtractMaybe(
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

        public static readonly ThemeProperty<HSliderStyle> Slider = ThemeProperty.ExtractMaybe(
            "primitive-slider",
            PrimitiveThemeComponent.Default,
            component => component.slider
        ).Compute(element => HSliderStyle.DefaultStyleOf(element));

        public static readonly ThemeProperty<HSliderStyle> Scrollbar = ThemeProperty.ExtractMaybe(
            "primitive-scrollbar",
            PrimitiveThemeComponent.Default,
            component => component.scrollbar
        ).Compute(HSliderStyle.DefaultScrollbarStyleOf);

        public static readonly ThemeProperty<HTextFieldStyle> TextField = ThemeProperty.ExtractMaybe(
            "primitive-text-field",
            PrimitiveThemeComponent.Default,
            component => component.textField
        ).Compute(HTextFieldStyle.DefaultStyleOf);

        public static readonly IReadOnlyList<ThemeProperty> Properties = new ThemeProperty[] {
            Surface, ContainerLow, Container, TextVariant, Text, Button, ButtonFocusLayer, Slider, Scrollbar
        };

        private static Func<ThemeProviderElement, T> ColorSchema<T>(Func<PrimitiveColorScheme, T> func) {
            return element => func(PrimitiveBaseTheme.Colors.Get(element));
        }

    }

    public class PrimitiveThemeComponent : ThemeComponent {

        public static readonly PrimitiveThemeComponent Default = new();
        public ThemeOptional<Color> background;
        public ThemeOptional<Color> backgroundSubtle;
        public ThemeOptional<HButtonStyle> button;
        public ThemeOptional<Substance> buttonFocusLayer;
        public ThemeOptional<HSliderStyle> scrollbar;
        public ThemeOptional<HSliderStyle> slider;

        public ThemeOptional<Color> surface;
        public ThemeOptional<Color> text;
        public ThemeOptional<Color> textContrast;
        public ThemeOptional<HTextFieldStyle> textField;

        public PrimitiveThemeComponent() {
            lookupScope = PrimitiveTheme.Properties;
        }

    }
}