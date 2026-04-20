using System.Collections.Generic;
using HELIX.Widgets.Theming;

namespace HELIX.Widgets.Universal.Theme {
    public class PrimitiveBaseTheme {

        public static readonly ThemeProperty<PrimitiveColorScheme> Colors = ThemeProperty.ExtractMaybe(
            "primitive-colors",
            PrimitiveBaseThemeComponent.Default,
            component => component.colors
        );

        public static readonly ThemeProperty<PrimitiveTypographyScheme> Typography = ThemeProperty.ExtractMaybe(
            "primitive-typography",
            PrimitiveBaseThemeComponent.Default,
            component => component.typography
        );

        public static readonly ThemeProperty<PrimitiveRadiusScheme> Radius = ThemeProperty.ExtractMaybe(
            "primitive-radius",
            PrimitiveBaseThemeComponent.Default,
            component => component.radius
        );

        public static readonly ThemeProperty<PrimitiveSpacingScheme> Spacing = ThemeProperty.ExtractMaybe(
            "primitive-spacing",
            PrimitiveBaseThemeComponent.Default,
            component => component.spacing
        );

        public static IReadOnlyList<ThemeProperty> Properties =
            new ThemeProperty[] { Colors, Typography, Radius, Spacing };

    }

    public class PrimitiveBaseThemeComponent : ThemeComponent {

        public static readonly PrimitiveBaseThemeComponent Default = new() {
            colors = PrimitiveColorScheme.Default,
            typography = PrimitiveTypographyScheme.Default,
            radius = PrimitiveRadiusScheme.Default,
            spacing = PrimitiveSpacingScheme.Default
        };

        public ThemeOptional<PrimitiveColorScheme> colors;
        public ThemeOptional<PrimitiveRadiusScheme> radius;
        public ThemeOptional<PrimitiveSpacingScheme> spacing;
        public ThemeOptional<PrimitiveTypographyScheme> typography;

        public PrimitiveBaseThemeComponent() {
            lookupScope = PrimitiveBaseTheme.Properties;
        }

    }
}