using HELIX.Abstractions;
using HELIX.Coloring.Material;
using HELIX.Extensions;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Navigation;
using HELIX.Widgets.Visual;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ExampleNavPageBase : NavPageBase {

    public int id;

    public ExampleNavPageBase() : this(1) { }

    public ExampleNavPageBase(int id) {
        this.id = id;
        this.BackgroundColor(Color.HSVToRGB(Random.value, 0.5f, 1f))
            .FlexContainer(mainAxisAlign: Justify.Center, crossAxisAlign: Align.Center)
            .Stretched();

        new Label("Page " + id).AddTo(this);

        var pushButton = new Button().AddTo(this);
        pushButton.text = "Push new";
        pushButton.clicked += () => { NavStackElement.Get(this).Push(new ExampleNavPageBase(id + 1)); };

        var popButton = new Button().AddTo(this);
        popButton.text = "Pop";
        popButton.clicked += () => {
            var stack = NavStackElement.Get(this);
            stack.Pop(this);
        };

        var replaceButton = new Button().AddTo(this);
        replaceButton.text = "Replace";
        replaceButton.clicked += () => {
            var stack = NavStackElement.Get(this);
            stack.PushReplacement(new ExampleNavPageBase(id));
        };

        var pop2Button = new Button().AddTo(this);
        pop2Button.text = "Pop 2";
        pop2Button.clicked += () => {
            var stack = NavStackElement.Get(this);
            var c = 0;
            stack.PopUntil(_ => c++ >= 2);
        };
    }

    public override string ToString() {
        return $"ExampleNavPage {id}";
    }

}

[UxmlElement]
public partial class ExampleScaffoldPopup : BaseElement {

    public ExampleScaffoldPopup() {
        RegisterCallback<PointerDownEvent>(evt => {
                var element = new Element("Offset").Positioned(100.Percent())
                    .Sized(100.Percent(), 100.Percent()).BackgroundColor(Color.red);

                var scaffold = ScaffoldElement.Get(this);
                var overlay = scaffold.AddAnchoredOverlay(this, element);
                element.RegisterCallback<PointerDownEvent>(_ => scaffold.RemoveOverlay(overlay));
            }
        );
    }

}

[UxmlElement]
public partial class HelixColorSwatchVisualizer : BaseElement {

    public HelixColorSwatchVisualizer() {
        this.FlexContainer(crossAxisAlign: Align.Stretch).Stretched();
        // foreach (var (swatchName, swatch) in Colors.Named) {
        //     var closestRadixTemplate = RadixSwatches.PickClosestTemplate(swatch);
        //     var text = $"{swatchName}, closest {closestRadixTemplate.name}";
        //     Add(new Label(text).Tight());
        //     // Add(new SwatchVisualizer(swatch.weights).Flexible());
        //     Add(new SwatchVisualizer(RadixSwatches.Generate(swatch, false, closestRadixTemplate)).Flexible());
        //     Add(new SwatchVisualizer(RadixSwatches.Generate(swatch, true, closestRadixTemplate)).Flexible());
        // }

        foreach (var template in MaterialColors.Primaries) {
            Add(new Label(template.name).Tight());
            // Add(new SwatchVisualizer(swatch.weights).Flexible());
            // Add(new SwatchVisualizer(template.weight).Flexible());
            // Add(new SwatchVisualizer(template.lightColors).Flexible());
            // Add(new SwatchVisualizer(template.darkColors).Flexible());

            Add(new SwatchVisualizer(template.ToSwatch()).Flexible());
            var scheme = template.Value.CreateScheme(Variant.Content);
            var schemeDark = template.Value.CreateScheme(Variant.Content, true);

            Add(new SwatchVisualizer(scheme.PrimaryPalette.ToMaterialColor().ToSwatch()).Flexible());
            Add(new SwatchVisualizer(scheme.NeutralPalette.ToMaterialColor().ToSwatch()).Flexible());
            Add(new SwatchVisualizer(scheme.SecondaryPalette.ToMaterialColor().ToSwatch()).Flexible());
            Add(new SwatchVisualizer(scheme.TertiaryPalette.ToMaterialColor().ToSwatch()).Flexible());

            Add(new SwatchVisualizer(RadixSwatch(scheme)).Flexible());
            Add(new SwatchVisualizer(RadixSwatch(schemeDark)).Flexible());
        }
    }

    private Color[] RadixSwatch(DynamicScheme scheme) {
        return new[] {
            RadixDynamicColors.Background.GetArgb(scheme).ArgbToColor(),
            RadixDynamicColors.BackgroundLight.GetArgb(scheme).ArgbToColor(),
            RadixDynamicColors.InteractiveNormal.GetArgb(scheme).ArgbToColor(),
            RadixDynamicColors.BorderNormal.GetArgb(scheme).ArgbToColor(),
            RadixDynamicColors.Solid.GetArgb(scheme).ArgbToColor(),
            RadixDynamicColors.Text.GetArgb(scheme).ArgbToColor(),
            RadixDynamicColors.TextContrast.GetArgb(scheme).ArgbToColor()
        };
    }

    private Color[] ToRadixLight(TonalPalette palette) {
        return new[] {
            palette.GetHct(99).ToColor(), palette.GetHct(97).ToColor(), palette.GetHct(94).ToColor(),
            palette.GetHct(91).ToColor(), palette.GetHct(88).ToColor(), palette.GetHct(80).ToColor(),
            palette.GetHct(72).ToColor(), palette.GetHct(64).ToColor(), palette.GetHct(50).ToColor(),
            palette.GetHct(45).ToColor(), palette.GetHct(30).ToColor(), palette.GetHct(6).ToColor()
        };
    }

    private Color[] ToRadixDark(TonalPalette palette) {
        return new[] {
            palette.GetHct(2).ToColor(), palette.GetHct(6).ToColor(), palette.GetHct(12).ToColor(),
            palette.GetHct(18).ToColor(), palette.GetHct(24).ToColor(), palette.GetHct(30).ToColor(),
            palette.GetHct(40).ToColor(), palette.GetHct(45).ToColor(), palette.GetHct(50).ToColor(),
            palette.GetHct(58).ToColor(), palette.GetHct(72).ToColor(), palette.GetHct(91).ToColor()
        };
    }

    public static class RadixDynamicColors {

        public static readonly DynamicColor Background = DynamicColor.FromPalette(
            scheme => scheme.NeutralPalette,
            scheme => scheme.IsDark ? 3 : 97,
            "radix_background"
        );

        public static readonly DynamicColor BackgroundLight = DynamicColor.FromPalette(
            scheme => scheme.PrimaryPalette,
            scheme => scheme.IsDark ? 6 : 96,
            "radix_background_light"
        );

        public static readonly DynamicColor InteractiveNormal = DynamicColor.FromPalette(
            scheme => scheme.PrimaryPalette,
            scheme => scheme.IsDark ? 18 : 92,
            "radix_interactive_normal"
        );

        public static readonly DynamicColor BorderNormal = DynamicColor.FromPalette(
            scheme => scheme.PrimaryPalette,
            scheme => scheme.IsDark ? 28 : 80,
            "radix_border_normal"
        );

        public static readonly DynamicColor Solid = DynamicColor.FromPalette(
            scheme => scheme.PrimaryPalette,
            _ => 55,
            "radix_solid_normal"
        );

        public static readonly DynamicColor Text = DynamicColor.FromPalette(
            scheme => scheme.PrimaryPalette,
            scheme => scheme.IsDark ? 75 : 45,
            "radix_text"
        );

        public static readonly DynamicColor TextContrast = DynamicColor.FromPalette(
            scheme => scheme.NeutralPalette,
            scheme => scheme.IsDark ? 90 : 10,
            "radix_text_contrast"
        );

    }

}