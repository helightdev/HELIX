using HELIX.Abstractions;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Navigation;
using HELIX.Widgets.Visual;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ExampleNavPage : NavPage {
    public int id;

    public ExampleNavPage() : this(1) { }

    public ExampleNavPage(int id) {
        this.id = id;
        this.BackgroundColor(Color.HSVToRGB(Random.value, 0.5f, 1f))
            .FlexContainer(mainAxisAlign: Justify.Center, crossAxisAlign: Align.Center)
            .Stretched();

        new Label("Page " + id).AddTo(this);

        var pushButton = new Button().AddTo(this);
        pushButton.text = "Push new";
        pushButton.clicked += () => { NavStackElement.Get(this).Push(new ExampleNavPage(id + 1)); };

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
            stack.PushReplacement(new ExampleNavPage(id));
        };

        var pop2Button = new Button().AddTo(this);
        pop2Button.text = "Pop 2";
        pop2Button.clicked += () => {
            var stack = NavStackElement.Get(this);
            var c = 0;
            stack.PopUntil(_ => c++ >= 2);
        };
    }

    public override string ToString() => $"ExampleNavPage {id}";
}

[UxmlElement]
public partial class ExampleScaffoldPopup : BaseElement {
    public ExampleScaffoldPopup() {
        RegisterCallback<PointerDownEvent>(evt => {
                var element = new Element("Offset").Positioned(top: 100.Percent())
                    .Sized(width: 100.Percent(), height: 100.Percent()).BackgroundColor(Color.red);

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
        foreach (var (swatchName, swatch) in Colors.Named) {
            Add(new Label(swatchName).Tight());
            Add(new SwatchVisualizer(swatch).Flexible());
        }
    }
}