using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///     Flutter-compatible Material color constants and exact fixed swatches.
    ///     This uses the hard-coded Flutter Material 2 color tables, not generated tonal palettes.
    /// </summary>
    public static class MaterialColors {
        public static readonly Color Transparent = 0x00000000u.ArgbToColor();

        public static readonly Color Black = 0xFF000000u.ArgbToColor();
        public static readonly Color Black87 = 0xDD000000u.ArgbToColor();
        public static readonly Color Black54 = 0x8A000000u.ArgbToColor();
        public static readonly Color Black45 = 0x73000000u.ArgbToColor();
        public static readonly Color Black38 = 0x61000000u.ArgbToColor();
        public static readonly Color Black26 = 0x42000000u.ArgbToColor();
        public static readonly Color Black12 = 0x1F000000u.ArgbToColor();

        public static readonly Color White = 0xFFFFFFFFu.ArgbToColor();
        public static readonly Color White70 = 0xB3FFFFFFu.ArgbToColor();
        public static readonly Color White60 = 0x99FFFFFFu.ArgbToColor();
        public static readonly Color White54 = 0x8AFFFFFFu.ArgbToColor();
        public static readonly Color White38 = 0x62FFFFFFu.ArgbToColor();
        public static readonly Color White30 = 0x4DFFFFFFu.ArgbToColor();
        public static readonly Color White24 = 0x3DFFFFFFu.ArgbToColor();
        public static readonly Color White12 = 0x1FFFFFFFu.ArgbToColor();
        public static readonly Color White10 = 0x1AFFFFFFu.ArgbToColor();

        public static readonly MaterialColor Red = new FixedMaterialColor(
            0xFFF44336u,
            0xFFFFEBEEu,
            0xFFFFCDD2u,
            0xFFEF9A9Au,
            0xFFE57373u,
            0xFFEF5350u,
            0xFFF44336u,
            0xFFE53935u,
            0xFFD32F2Fu,
            0xFFC62828u,
            0xFFB71C1Cu,
            "red"
        );

        public static readonly MaterialAccentColor RedAccent = new FixedMaterialAccentColor(
            0xFFFF5252u,
            0xFFFF8A80u,
            0xFFFF5252u,
            0xFFFF1744u,
            0xFFD50000u,
            "redAccent"
        );

        public static readonly MaterialColor Pink = new FixedMaterialColor(
            0xFFE91E63u,
            0xFFFCE4ECu,
            0xFFF8BBD0u,
            0xFFF48FB1u,
            0xFFF06292u,
            0xFFEC407Au,
            0xFFE91E63u,
            0xFFD81B60u,
            0xFFC2185Bu,
            0xFFAD1457u,
            0xFF880E4Fu,
            "pink"
        );

        public static readonly MaterialAccentColor PinkAccent = new FixedMaterialAccentColor(
            0xFFFF4081u,
            0xFFFF80ABu,
            0xFFFF4081u,
            0xFFF50057u,
            0xFFC51162u,
            "pinkAccent"
        );

        public static readonly MaterialColor Purple = new FixedMaterialColor(
            0xFF9C27B0u,
            0xFFF3E5F5u,
            0xFFE1BEE7u,
            0xFFCE93D8u,
            0xFFBA68C8u,
            0xFFAB47BCu,
            0xFF9C27B0u,
            0xFF8E24AAu,
            0xFF7B1FA2u,
            0xFF6A1B9Au,
            0xFF4A148Cu,
            "purple"
        );

        public static readonly MaterialAccentColor PurpleAccent = new FixedMaterialAccentColor(
            0xFFE040FBu,
            0xFFEA80FCu,
            0xFFE040FBu,
            0xFFD500F9u,
            0xFFAA00FFu,
            "purpleAccent"
        );

        public static readonly MaterialColor DeepPurple = new FixedMaterialColor(
            0xFF673AB7u,
            0xFFEDE7F6u,
            0xFFD1C4E9u,
            0xFFB39DDBu,
            0xFF9575CDu,
            0xFF7E57C2u,
            0xFF673AB7u,
            0xFF5E35B1u,
            0xFF512DA8u,
            0xFF4527A0u,
            0xFF311B92u,
            "deepPurple"
        );

        public static readonly MaterialAccentColor DeepPurpleAccent = new FixedMaterialAccentColor(
            0xFF7C4DFFu,
            0xFFB388FFu,
            0xFF7C4DFFu,
            0xFF651FFFu,
            0xFF6200EAu,
            "deepPurpleAccent"
        );

        public static readonly MaterialColor Indigo = new FixedMaterialColor(
            0xFF3F51B5u,
            0xFFE8EAF6u,
            0xFFC5CAE9u,
            0xFF9FA8DAu,
            0xFF7986CBu,
            0xFF5C6BC0u,
            0xFF3F51B5u,
            0xFF3949ABu,
            0xFF303F9Fu,
            0xFF283593u,
            0xFF1A237Eu,
            "indigo"
        );

        public static readonly MaterialAccentColor IndigoAccent = new FixedMaterialAccentColor(
            0xFF536DFEu,
            0xFF8C9EFFu,
            0xFF536DFEu,
            0xFF3D5AFEu,
            0xFF304FFEu,
            "indigoAccent"
        );

        public static readonly MaterialColor Blue = new FixedMaterialColor(
            0xFF2196F3u,
            0xFFE3F2FDu,
            0xFFBBDEFBu,
            0xFF90CAF9u,
            0xFF64B5F6u,
            0xFF42A5F5u,
            0xFF2196F3u,
            0xFF1E88E5u,
            0xFF1976D2u,
            0xFF1565C0u,
            0xFF0D47A1u,
            "blue"
        );

        public static readonly MaterialAccentColor BlueAccent = new FixedMaterialAccentColor(
            0xFF448AFFu,
            0xFF82B1FFu,
            0xFF448AFFu,
            0xFF2979FFu,
            0xFF2962FFu,
            "blueAccent"
        );

        public static readonly MaterialColor LightBlue = new FixedMaterialColor(
            0xFF03A9F4u,
            0xFFE1F5FEu,
            0xFFB3E5FCu,
            0xFF81D4FAu,
            0xFF4FC3F7u,
            0xFF29B6F6u,
            0xFF03A9F4u,
            0xFF039BE5u,
            0xFF0288D1u,
            0xFF0277BDu,
            0xFF01579Bu,
            "lightBlue"
        );

        public static readonly MaterialAccentColor LightBlueAccent = new FixedMaterialAccentColor(
            0xFF40C4FFu,
            0xFF80D8FFu,
            0xFF40C4FFu,
            0xFF00B0FFu,
            0xFF0091EAu,
            "lightBlueAccent"
        );

        public static readonly MaterialColor Cyan = new FixedMaterialColor(
            0xFF00BCD4u,
            0xFFE0F7FAu,
            0xFFB2EBF2u,
            0xFF80DEEAu,
            0xFF4DD0E1u,
            0xFF26C6DAu,
            0xFF00BCD4u,
            0xFF00ACC1u,
            0xFF0097A7u,
            0xFF00838Fu,
            0xFF006064u,
            "cyan"
        );

        public static readonly MaterialAccentColor CyanAccent = new FixedMaterialAccentColor(
            0xFF18FFFFu,
            0xFF84FFFFu,
            0xFF18FFFFu,
            0xFF00E5FFu,
            0xFF00B8D4u,
            "cyanAccent"
        );

        public static readonly MaterialColor Teal = new FixedMaterialColor(
            0xFF009688u,
            0xFFE0F2F1u,
            0xFFB2DFDBu,
            0xFF80CBC4u,
            0xFF4DB6ACu,
            0xFF26A69Au,
            0xFF009688u,
            0xFF00897Bu,
            0xFF00796Bu,
            0xFF00695Cu,
            0xFF004D40u,
            "teal"
        );

        public static readonly MaterialAccentColor TealAccent = new FixedMaterialAccentColor(
            0xFF64FFDau,
            0xFFA7FFEBu,
            0xFF64FFDau,
            0xFF1DE9B6u,
            0xFF00BFA5u,
            "tealAccent"
        );

        public static readonly MaterialColor Green = new FixedMaterialColor(
            0xFF4CAF50u,
            0xFFE8F5E9u,
            0xFFC8E6C9u,
            0xFFA5D6A7u,
            0xFF81C784u,
            0xFF66BB6Au,
            0xFF4CAF50u,
            0xFF43A047u,
            0xFF388E3Cu,
            0xFF2E7D32u,
            0xFF1B5E20u,
            "green"
        );

        public static readonly MaterialAccentColor GreenAccent = new FixedMaterialAccentColor(
            0xFF69F0AEu,
            0xFFB9F6CAu,
            0xFF69F0AEu,
            0xFF00E676u,
            0xFF00C853u,
            "greenAccent"
        );

        public static readonly MaterialColor LightGreen = new FixedMaterialColor(
            0xFF8BC34Au,
            0xFFF1F8E9u,
            0xFFDCEDC8u,
            0xFFC5E1A5u,
            0xFFAED581u,
            0xFF9CCC65u,
            0xFF8BC34Au,
            0xFF7CB342u,
            0xFF689F38u,
            0xFF558B2Fu,
            0xFF33691Eu,
            "lightGreen"
        );

        public static readonly MaterialAccentColor LightGreenAccent = new FixedMaterialAccentColor(
            0xFFB2FF59u,
            0xFFCCFF90u,
            0xFFB2FF59u,
            0xFF76FF03u,
            0xFF64DD17u,
            "lightGreenAccent"
        );

        public static readonly MaterialColor Lime = new FixedMaterialColor(
            0xFFCDDC39u,
            0xFFF9FBE7u,
            0xFFF0F4C3u,
            0xFFE6EE9Cu,
            0xFFDCE775u,
            0xFFD4E157u,
            0xFFCDDC39u,
            0xFFC0CA33u,
            0xFFAFB42Bu,
            0xFF9E9D24u,
            0xFF827717u,
            "lime"
        );

        public static readonly MaterialAccentColor LimeAccent = new FixedMaterialAccentColor(
            0xFFEEFF41u,
            0xFFF4FF81u,
            0xFFEEFF41u,
            0xFFC6FF00u,
            0xFFAEEA00u,
            "limeAccent"
        );

        public static readonly MaterialColor Yellow = new FixedMaterialColor(
            0xFFFFEB3Bu,
            0xFFFFFDE7u,
            0xFFFFF9C4u,
            0xFFFFF59Du,
            0xFFFFF176u,
            0xFFFFEE58u,
            0xFFFFEB3Bu,
            0xFFFDD835u,
            0xFFFBC02Du,
            0xFFF9A825u,
            0xFFF57F17u,
            "yellow"
        );

        public static readonly MaterialAccentColor YellowAccent = new FixedMaterialAccentColor(
            0xFFFFFF00u,
            0xFFFFFF8Du,
            0xFFFFFF00u,
            0xFFFFEA00u,
            0xFFFFD600u,
            "yellowAccent"
        );

        public static readonly MaterialColor Amber = new FixedMaterialColor(
            0xFFFFC107u,
            0xFFFFF8E1u,
            0xFFFFECB3u,
            0xFFFFE082u,
            0xFFFFD54Fu,
            0xFFFFCA28u,
            0xFFFFC107u,
            0xFFFFB300u,
            0xFFFFA000u,
            0xFFFF8F00u,
            0xFFFF6F00u,
            "amber"
        );

        public static readonly MaterialAccentColor AmberAccent = new FixedMaterialAccentColor(
            0xFFFFD740u,
            0xFFFFE57Fu,
            0xFFFFD740u,
            0xFFFFC400u,
            0xFFFFAB00u,
            "amberAccent"
        );

        public static readonly MaterialColor Orange = new FixedMaterialColor(
            0xFFFF9800u,
            0xFFFFF3E0u,
            0xFFFFE0B2u,
            0xFFFFCC80u,
            0xFFFFB74Du,
            0xFFFFA726u,
            0xFFFF9800u,
            0xFFFB8C00u,
            0xFFF57C00u,
            0xFFEF6C00u,
            0xFFE65100u,
            "orange"
        );

        public static readonly MaterialAccentColor OrangeAccent = new FixedMaterialAccentColor(
            0xFFFFAB40u,
            0xFFFFD180u,
            0xFFFFAB40u,
            0xFFFF9100u,
            0xFFFF6D00u,
            "orangeAccent"
        );

        public static readonly MaterialColor DeepOrange = new FixedMaterialColor(
            0xFFFF5722u,
            0xFFFBE9E7u,
            0xFFFFCCBCu,
            0xFFFFAB91u,
            0xFFFF8A65u,
            0xFFFF7043u,
            0xFFFF5722u,
            0xFFF4511Eu,
            0xFFE64A19u,
            0xFFD84315u,
            0xFFBF360Cu,
            "deepOrange"
        );

        public static readonly MaterialAccentColor DeepOrangeAccent = new FixedMaterialAccentColor(
            0xFFFF6E40u,
            0xFFFF9E80u,
            0xFFFF6E40u,
            0xFFFF3D00u,
            0xFFDD2C00u,
            "deepOrangeAccent"
        );

        public static readonly MaterialColor Brown = new FixedMaterialColor(
            0xFF795548u,
            0xFFEFEBE9u,
            0xFFD7CCC8u,
            0xFFBCAAA4u,
            0xFFA1887Fu,
            0xFF8D6E63u,
            0xFF795548u,
            0xFF6D4C41u,
            0xFF5D4037u,
            0xFF4E342Eu,
            0xFF3E2723u,
            "brown"
        );

        public static readonly MaterialColor Grey = new FixedMaterialColor(
            0xFF9E9E9Eu,
            0xFFFAFAFAu,
            0xFFF5F5F5u,
            0xFFEEEEEEu,
            0xFFE0E0E0u,
            0xFFBDBDBDu,
            0xFF9E9E9Eu,
            0xFF757575u,
            0xFF616161u,
            0xFF424242u,
            0xFF212121u,
            "grey"
        );

        public static readonly MaterialColor BlueGrey = new FixedMaterialColor(
            0xFF607D8Bu,
            0xFFECEFF1u,
            0xFFCFD8DCu,
            0xFFB0BEC5u,
            0xFF90A4AEu,
            0xFF78909Cu,
            0xFF607D8Bu,
            0xFF546E7Au,
            0xFF455A64u,
            0xFF37474Fu,
            0xFF263238u,
            "blueGrey"
        );

        public static readonly MaterialColor[] Primaries = {
            Red, Pink, Purple, DeepPurple, Indigo, Blue, LightBlue, Cyan, Teal, Green, LightGreen, Lime, Yellow, Amber,
            Orange, DeepOrange, Brown, BlueGrey
        };

        public static readonly MaterialAccentColor[] Accents = {
            RedAccent, PinkAccent, PurpleAccent, DeepPurpleAccent, IndigoAccent, BlueAccent, LightBlueAccent,
            CyanAccent, TealAccent, GreenAccent, LightGreenAccent, LimeAccent, YellowAccent, AmberAccent, OrangeAccent,
            DeepOrangeAccent
        };
    }
}