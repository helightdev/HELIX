using System;
using System.Collections.Generic;
using HELIX.Extensions;
using HELIX.Widgets;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

[ThemePropertyCollection]
public static class MyThemes {
    // public static readonly ThemeProperty<Color> PrimaryColor1 = new ThemePropertyBuilder<Color>()
    //     .Key("c-primary").StyleLoader().ComponentExtractor<ExampleThemeComponent>(component => component.primaryColor)
    //     .Build();

    public static readonly ThemeProperty<Color> PrimaryColor = ThemeProperty.ExtractMaybe(
        "c-primary",
        ExampleThemeComponent.Default,
        component => component.primaryColor
    ).StyleLoader();
    //ThemeProperty.Theme<Color>("c-primary", ExampleThemeComponent.Default);

    public static readonly ThemeProperty<Color> PrimaryWashedColor = ThemeProperty.ExtractMaybe(
        "c-primary-washed",
        ExampleThemeComponent.Default,
        component => component.primaryWashedColor
    ).StyleLoader();

    public static readonly ThemeProperty<ElementFactory<VisualElement>> ExampleFactory = ThemeProperty.Extract(
        "example-factory",
        ExampleThemeComponent.Default,
        component => component.factory as ElementFactory<VisualElement>
    );

    public static readonly IReadOnlyList<ThemeProperty> Properties =
        new ThemeProperty[] { PrimaryColor, PrimaryWashedColor, ExampleFactory };
}

[UxmlObject, Serializable]
public partial class ExampleThemeComponent : ThemeComponent {
    public static readonly ExampleThemeComponent Default = new() {
        factory = new TestFactory(),
        primaryColor = Color.white,
        primaryWashedColor = Color.white
    };

    [UxmlAttribute("c-primary")]
    public ThemeOptional<Color> primaryColor;

    [UxmlAttribute("c-primary-washed")]
    public ThemeOptional<Color> primaryWashedColor;

    [UxmlAttribute("example-optional")]
    public ThemeOptional<Color> optionalColor;

    [Header("Example Theme Component"), UxmlObjectReference("example-factory")]
    public VisualElementFactory factory;

    public ExampleThemeComponent() {
        lookupScope = MyThemes.Properties;
    }
}

[UxmlWidgetFactory, UxmlObject]
public partial class TestFactory : VisualElementFactory {
    public override VisualElement Create(BaseElement parentElement) {
        return new Label("Hello, World!").Sized(25);
    }
}

[UxmlWidgetFactory, UxmlObject]
public partial class AnotherTestFactory : VisualElementFactory {
    public override VisualElement Create(BaseElement parentElement) {
        return new Label("This is just another test!").Positioned(right: 0, bottom: 0).Flexible();
    }
}

[UxmlElement]
public partial class Example : BaseElement {
    private readonly ElementFactorySlot<VisualElement> _factorySlotSlot;
    private readonly ThemeValue<Color> _primaryColor;

    public Example() {
        _primaryColor = ThemeValue(MyThemes.PrimaryColor, OnPrimaryColorChanged);
        _factorySlotSlot = WidgetFactorySlot(MyThemes.ExampleFactory);
        _factorySlotSlot.StretchToParentSize();
        Add(_factorySlotSlot);
    }

    public IPublicElementFactorySlot<VisualElement> FactorySlot => _factorySlotSlot;

    [UxmlAttribute]
    public ThemeOverride<Color> PrimaryColor {
        get => _primaryColor.Override;
        set => _primaryColor.Override = value;
    }

    [UxmlAttribute]
    public ThemeOverride<Texture2D> SomeTexture { get; set; }

    [UxmlObjectReference("factory")]
    public VisualElementFactory FactoryMapped {
        get => _factorySlotSlot.GetMapped<VisualElementFactory>();
        set => _factorySlotSlot.SetMapped(value);
    }

    private void OnPrimaryColorChanged(Color newValue) {
        style.backgroundColor = newValue;
    }
}

[UxmlElement]
public partial class PerformUpdateWidget : BaseElement {
    public PerformUpdateWidget() {
        var button = new Button { text = "Update me!" };
        button.clicked += () => {
            ThemeProviderElement.Components =
                new List<ThemeComponent> { new ExampleThemeComponent { factory = new AnotherTestFactory() } };
        };
        Add(button);
    }
}