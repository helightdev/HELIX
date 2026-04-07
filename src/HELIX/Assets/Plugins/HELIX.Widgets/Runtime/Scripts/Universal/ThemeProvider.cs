using System.Collections.Generic;
using HELIX.Widgets.Theming;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal
{
    public class ThemeProvider : Widget {
        
        public Widget child;
        public Dictionary<ThemeProperty, object> properties;
        public List<WidgetThemeComponent> components;
        
        public override IWidgetElement CreateElement() {
            var element = new WidgetThemeProvider();
            element.RegisterCallbackOnce<AttachToPanelEvent>(_ => element.Reconcile(this));
            return element;
        }
    }
}