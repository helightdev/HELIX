using System;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;

namespace HELIX.Widgets.Universal {
    public class HButton : StatefulWidget<HButton> {
        public Widget child;
        public ButtonController controller;

        public Key focusKey;
        public bool enabled = true;
        public bool selected = false;
        public Action onClick;

        public HButtonStyle style = null;
        public HInputRadius? radius = null;
        public HButtonVariant? variant = null;
        public HButtonSize? size = null;
        public ColorTokenPalette palette = null;

        public override State<HButton> CreateState() {
            return new HShapeButtonState();
        }
    }

    public class HShapeButtonState : State<HButton> {
        private ButtonController _controller;
        private WidgetStateController _widgetStateController;

        public override void InitState() {
            if (widget.controller != null) {
                _controller = widget.controller;
                _widgetStateController = _controller.widgetState;
            } else {
                _widgetStateController = AddDisposable(new WidgetStateController());
                _controller = AddDisposable(new ButtonController(_widgetStateController));
                _controller.onClick = widget.onClick;
                _controller.enabled = widget.enabled;
            }
        }

        public override bool CanReconcile(HButton oldWidget) {
            return base.CanReconcile(oldWidget) && widget.controller == oldWidget.controller;
        }

        public override void DidUpdateWidget(HButton oldWidget) {
            if (widget.controller != oldWidget.controller && oldWidget.controller == null) {
                _controller.onClick = widget.onClick;
                _controller.enabled = widget.enabled;
                _widgetStateController.Toggle(WidgetState.Selected, widget.selected);
            }
        }

        public override Widget Build(BuildContext context) {
            HButtonStyle effective;

            if (widget.style != null) {
                effective = widget.style; //
            } else if (widget.radius.HasValue || widget.variant.HasValue || widget.size.HasValue) {
                effective = DefaultButtonStyles.DefaultStyleOf(
                    context,
                    widget.variant ?? HButtonVariant.Flat,
                    widget.size ?? HButtonSize.Regular,
                    widget.radius ?? HInputRadius.Medium,
                    palette: widget.palette
                );
            } else { effective = context.GetThemed(PrimitiveTheme.Button); }

            // Possibly allocation heavy with default stale and property composition, but doesn't rebuild for
            // every state change since only the substance box listens to state changes, not the button itself.
            var modifierProperty = WidgetStateProperties.Modifiers(
                effective.modifiers,
                WidgetStateProperties.Func(state => {
                        var set = new ModifierSet {
                            new OpacityModifier(effective.opacity.ResolveOrDefault(state, 1f)),
                            new SizeModifier(effective.constraints.ResolveOrDefault(state, BoxConstraints.Initial)),
                            new TextStyleModifier(effective.textStyle.ResolveOrDefault(state)),
                            new PaddingModifier(effective.padding.ResolveOrDefault(state, StyleLength4.Zero)),
                            new ButtonControllerModifier(_controller),
                            FocusModifier.Focusable
                        };
                        if (_widgetStateController != null) set.Add(new WidgetStateModifier(_widgetStateController));
                        return set;
                    }
                )
            );
            
            return new HSubstanceBox {
                controller = _widgetStateController,
                substances = effective.layers,
                alignment = effective.alignment,
                builder = widget.child,
                boxKey = widget.focusKey,
                boxModifiers = modifierProperty
            }.Fill();
        }
    }
}