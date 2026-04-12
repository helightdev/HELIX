using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Types;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;

namespace HELIX.Widgets.Universal.Theme {
    public class PrimitiveTheme {
        public static readonly ThemeProperty<Color> Surface = ThemeProperty.ExtractMaybe(
            "primitive-c-surface",
            PrimitiveThemeComponent.Default,
            component => component.surface
        ).StyleLoader().Compute(Neutral(elem => elem.C1));

        public static readonly ThemeProperty<Color> BackgroundSubtle = ThemeProperty.ExtractMaybe(
            "primitive-c-background-subtle",
            PrimitiveThemeComponent.Default,
            component => component.backgroundSubtle
        ).StyleLoader().Compute(Neutral(elem => elem.C2));

        public static readonly ThemeProperty<Color> Background = ThemeProperty.ExtractMaybe(
            "primitive-c-background",
            PrimitiveThemeComponent.Default,
            component => component.background
        ).StyleLoader().Compute(Neutral(elem => elem.C3));

        public static readonly ThemeProperty<Color> Text = ThemeProperty.ExtractMaybe(
            "primitive-c-text",
            PrimitiveThemeComponent.Default,
            component => component.text
        ).StyleLoader().Compute(Neutral(elem => elem.C11));

        public static readonly ThemeProperty<Color> TextContrast = ThemeProperty.ExtractMaybe(
            "primitive-c-text-contrast",
            PrimitiveThemeComponent.Default,
            component => component.textContrast
        ).StyleLoader().Compute(Neutral(elem => elem.C12));
        
        public static readonly ThemeProperty<HButtonStyle> ButtonTheme = ThemeProperty.ExtractMaybe(
            "primitive-button",
            PrimitiveThemeComponent.Default,
            component => component.button
        ).Compute(element =>  new HButtonStyle());
        
        private static Func<ThemeProviderElement, Color> Neutral(Func<RadixPalette, Color> func) =>
            element => func(element.Resolve(PrimitiveBaseTheme.NeutralColors));

        private static Func<ThemeProviderElement, Color> Accent(Func<RadixPalette, Color> func) =>
            element => func(element.Resolve(PrimitiveBaseTheme.AccentColors));

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