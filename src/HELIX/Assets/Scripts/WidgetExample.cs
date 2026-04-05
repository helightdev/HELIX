using System;
using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Widgets;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;
using WidgetThemeComponent = HELIX.Widgets.Theming.WidgetThemeComponent;

[ThemePropertyCollection]
public static class MyThemes {
    public static readonly ThemeProperty<Color> PrimaryColor =
        ThemeProperty.Theme<Color>("c-primary", ExampleThemeComponent.Default);

    public static readonly ThemeProperty<Color> PrimaryWashedColor =
        ThemeProperty.Theme<Color>("c-primary-washed", ExampleThemeComponent.Default);

    public static readonly ThemeProperty<WidgetFactory<VisualElement>> WidgetFactory =
        ThemeProperty.WidgetFactory<VisualElement>("example-factory", ExampleThemeComponent.Default);
}

[UxmlObject, Serializable]
public partial class ExampleThemeComponent : WidgetThemeComponent {
    public static readonly ExampleThemeComponent Default = new() {
        factory = new TestFactory(),
        primaryColor = Color.white,
        primaryWashedColor = Color.white,
    };
    
    [Header("Example Theme Component")]
    [UxmlObjectReference("example-factory")]
    public VisualElementWidgetFactory factory;

    [UxmlAttribute("c-primary")]
    public ThemeOptional<Color> primaryColor;
    
    [UxmlAttribute("c-primary-washed")]
    public ThemeOptional<Color> primaryWashedColor;

    [UxmlAttribute("example-optional")]
    public ThemeOptional<Color> optionalColor;
}

[UxmlWidgetFactory, UxmlObject]
public partial class TestFactory : VisualElementWidgetFactory {
    public override VisualElement Create(BaseElement parentWidget) {
        return new Label("Hello, World!").Sized(width: 25);
    }
}

[UxmlWidgetFactory, UxmlObject]
public partial class AnotherTestFactory : VisualElementWidgetFactory {
    public override VisualElement Create(BaseElement parentWidget) {
        return new Label("This is just another test!").Positioned(right: 0, bottom: 0).Flexible();
    }
}

[UxmlElement]
public partial class Example : BaseElement {
    private readonly ThemeValue<Color> _primaryColor;
    private readonly WidgetFactorySlot<VisualElement> _factorySlotSlot;
    public IPublicWidgetFactorySlot<VisualElement> FactorySlot => _factorySlotSlot;

    [UxmlAttribute]
    public ThemeOverride<Color> PrimaryColor {
        get => _primaryColor.Override;
        set => _primaryColor.Override = value;
    }

    [UxmlAttribute]
    public ThemeOverride<Texture2D> SomeTexture { get; set; }

    [UxmlObjectReference("factory")]
    public VisualElementWidgetFactory FactoryMapped {
        get => _factorySlotSlot.GetMapped<VisualElementWidgetFactory>();
        set => _factorySlotSlot.SetMapped(value);
    }

    public Example() {
        _primaryColor = ThemeValue(MyThemes.PrimaryColor, OnPrimaryColorChanged);
        _factorySlotSlot = WidgetFactorySlot(MyThemes.WidgetFactory);
        _factorySlotSlot.StretchToParentSize();
        Add(_factorySlotSlot);
    }

    private void OnPrimaryColorChanged(Color newValue) {
        style.backgroundColor = newValue;
    }
}

[UxmlElement]
public partial class PerformUpdateWidget : BaseElement {
    public PerformUpdateWidget() {
        var button = new Button() { text = "Update me!" };
        button.clicked += () => {
            ThemeProvider.Components =
                new List<WidgetThemeComponent> { new ExampleThemeComponent { factory = new AnotherTestFactory() } };
        };
        Add(button);
    }
}