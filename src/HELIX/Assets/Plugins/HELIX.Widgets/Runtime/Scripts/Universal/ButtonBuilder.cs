using System;
using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class ButtonBuilder : Widget {
        private static readonly FocusModifier _defaultFocus = new(true, PickingMode.Position, 0) { isFallback = true };
        public Alignment alignment = Alignment.Center;

        public WidgetStateBuilder builder;
        public bool enabled = true;
        public Action onClick;
        public bool selected = false;

        public ButtonBuilder() {
            modifiers.Add(_defaultFocus);
        }

        public override IWidgetElement CreateElement() {
            var element = new GenericButton();
            element.RegisterCallbackOnce<AttachToPanelEvent>(_ => element.Reconcile(this));
            return element;
        }
    }
}