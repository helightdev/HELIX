using HELIX.Abstractions;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

[ThemePropertyCollection]
public static class MyThemes {
    public static readonly ThemeProperty<Color> PrimaryColor = ThemeProperty.Theme("c-primary", Color.white);

    public static readonly ThemeProperty<Color> PrimaryWashedColor =
        ThemeProperty.Theme("c-primary-washed", Color.white);

    public static readonly ThemeProperty<WidgetFactory<VisualElement>> WidgetFactory =
        ThemeProperty.WidgetFactory<VisualElement>("example-factory", typeof(TestFactory));
}

[UxmlWidgetFactory]
public class TestFactory : WidgetFactory<VisualElement> {
    public override VisualElement Create(BaseWidget parentWidget) {
        return new Label("Hello, World!").Sized(width: 25).Positioned(right: 0);
    }
}

[UxmlWidgetFactory]
public class AnotherTestFactory : WidgetFactory<VisualElement> {
    public override VisualElement Create(BaseWidget parentWidget) {
        return new Label("This is just another test!").Positioned(right: 0, bottom: 0);
    }
}

[UxmlElement]
public partial class Example : BaseWidget {
    private readonly ThemeValue<Color> _primaryColor;
    private readonly WidgetFactorySlot<VisualElement> _factorySlot;

    [UxmlAttribute]
    public ThemeOverride<Color> PrimaryColor {
        get => _primaryColor.Override;
        set => _primaryColor.Override = value;
    }

    [UxmlAttribute]
    public WidgetFactoryReference<VisualElement> FactoryReference {
        get => _factorySlot.Reference;
        set => _factorySlot.Reference = value;
    }

    public Example() {
        _primaryColor = ThemeValue(MyThemes.PrimaryColor, OnPrimaryColorChanged);
        _factorySlot = WidgetFactorySlot(MyThemes.WidgetFactory);
        _factorySlot.StretchToParentSize();
        Add(_factorySlot);
        var a = new Element {
            Child = new Label("Inside Factory Slot").Transitions(
                new Transition(StyleProperties.Color) { duration = 0.5f },
                new Transition(StyleProperties.FontSize) {
                    duration = 0.25f,
                    easing = EasingMode.EaseInOut
                })
        };
    }


    private void OnPrimaryColorChanged(Color newValue) {
        style.backgroundColor = newValue;
    }
}