using System;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public class ThemePropertyCollectionAttribute : Attribute { }
    public class ThemePropertyReferenceAttribute : PropertyAttribute { }
    
    [ThemePropertyCollection]
    public static class MyThemes {
        public static readonly ThemeProperty<Color> PrimaryColor = ThemeProperty.Theme("c-primary", Color.white);
        public static readonly ThemeProperty<Color> PrimaryWashedColor = ThemeProperty.Theme("c-primary-washed", Color.white);
    }

    [UxmlElement]
    public partial class Example : BaseWidget {
        private readonly ThemeValue<Color> _primaryColor;

        [UxmlAttribute]
        public ThemeOverride<Color> PrimaryColor {
            get => _primaryColor.Override;
            set => _primaryColor.Override = value;
        }

        public Example() {
            _primaryColor = ThemeValue(MyThemes.PrimaryColor, OnPrimaryColorChanged);
        }

        private void OnPrimaryColorChanged(Color newValue) {
            style.backgroundColor = newValue;
        }
    }
}