using System.Collections.Generic;
using HELIX.Widgets.Theming;

namespace HELIX.Widgets.Universal {
    public class ThemeProvider : SingleChildWidget {
        public List<ThemeComponent> components;

        public Dictionary<ThemeProperty, object> properties;

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new ThemeProviderElement());
        }
    }
}