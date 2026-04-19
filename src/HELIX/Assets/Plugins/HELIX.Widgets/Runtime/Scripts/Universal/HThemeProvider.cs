using System.Collections.Generic;
using HELIX.Widgets.Theming;

namespace HELIX.Widgets.Universal {
    public class HThemeProvider : SingleChildWidget {
        public readonly List<ThemeComponent> components;

        public readonly Dictionary<ThemeProperty, object> properties;

        public HThemeProvider(
            List<ThemeComponent> components = null,
            Dictionary<ThemeProperty, object> properties = null,
            Widget child = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(child, key, constants) {
            this.components = components;
            this.properties = properties;

            DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
        }

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new ThemeProviderElement());
        }
    }
}