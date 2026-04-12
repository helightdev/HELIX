using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Widgets.Theming;

namespace HELIX.Widgets.Universal.Theme {
    public class PrimitiveBaseTheme {
        public static readonly ThemeProperty<RadixPalette> NeutralColors = ThemeProperty.ExtractMaybe(
            "primitive-neutral-colors",
            PrimitiveBaseThemeComponent.Default,
            component => component.neutralColors
        );

        public static readonly ThemeProperty<RadixPalette> AccentColors = ThemeProperty.ExtractMaybe(
            "primitive-accent-colors",
            PrimitiveBaseThemeComponent.Default,
            component => component.accentColors
        );

        public static readonly ThemeProperty<PrimitiveTypographySchema> Typography = ThemeProperty.ExtractMaybe(
            "primitive-typography",
            PrimitiveBaseThemeComponent.Default,
            component => component.typography
        );

        public static readonly ThemeProperty<PrimitiveRadiusSchema> Radius = ThemeProperty.ExtractMaybe(
            "primitive-radius",
            PrimitiveBaseThemeComponent.Default,
            component => component.radius
        );

        public static readonly ThemeProperty<PrimitiveSpacingSchema> Spacing = ThemeProperty.ExtractMaybe(
            "primitive-spacing",
            PrimitiveBaseThemeComponent.Default,
            component => component.spacing
        );

        public static IReadOnlyList<ThemeProperty> Properties =
            new ThemeProperty[] { NeutralColors, AccentColors, Typography, Radius, Spacing };
    }

    public class PrimitiveBaseThemeComponent : ThemeComponent {
        public static readonly PrimitiveBaseThemeComponent Default = new() {
            neutralColors = Colors.Gray.Light,
            accentColors = Colors.Indigo.Light,
            typography = PrimitiveTypographySchema.Default,
            radius = PrimitiveRadiusSchema.Default,
            spacing = PrimitiveSpacingSchema.Default
        };

        public PrimitiveBaseThemeComponent() {
            lookupScope = PrimitiveBaseTheme.Properties;
        }

        public ThemeOptional<RadixPalette> neutralColors;
        public ThemeOptional<RadixPalette> accentColors;
        public ThemeOptional<PrimitiveTypographySchema> typography;
        public ThemeOptional<PrimitiveRadiusSchema> radius;
        public ThemeOptional<PrimitiveSpacingSchema> spacing;
    }
}