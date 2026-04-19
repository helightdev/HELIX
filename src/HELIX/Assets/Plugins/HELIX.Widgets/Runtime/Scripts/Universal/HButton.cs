using System;
using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;

namespace HELIX.Widgets.Universal {
    public class HButton : SingleChildStatefulWidget<HButton> {
        public readonly ButtonController controller;

        public readonly Key focusKey;
        public readonly bool enabled;
        public readonly bool selected;
        public readonly Action onClick;

        public readonly HButtonStyle style;
        public readonly HInputRadius? radius;
        public readonly HButtonVariant? variant;
        public readonly HButtonSize? size;
        public readonly ColorTokenPalette palette;

        public HButton(
            HButtonVariant? variant = null,
            ButtonController controller = null,
            Key focusKey = default,
            bool enabled = true,
            bool selected = false,
            Action onClick = null,
            HButtonStyle style = null,
            HInputRadius? radius = null,
            HButtonSize? size = null,
            ColorTokenPalette palette = null,
            Widget child = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(child, key, constants, modifiers) {
            this.child = child;
            this.controller = controller;
            this.focusKey = focusKey;
            this.enabled = enabled;
            this.selected = selected;
            this.onClick = onClick;
            this.style = style;
            this.radius = radius;
            this.variant = variant;
            this.size = size;
            this.palette = palette;
        }

        public override State<HButton> CreateState() {
            return new HShapeButtonState();
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<Widget>("child", child));
            properties.Add(new DiagnosticsProperty<ButtonController>("controller", controller, defaultValue: null));
            properties.Add(new DiagnosticsProperty<HButtonStyle>("style", style, defaultValue: null));
            properties.Add(new DiagnosticsProperty<HInputRadius?>("radius", radius, defaultValue: null));
            properties.Add(new DiagnosticsProperty<HButtonVariant?>("variant", variant, defaultValue: null));
            properties.Add(new DiagnosticsProperty<HButtonSize?>("size", size, defaultValue: null));
            properties.Add(new DiagnosticsProperty<ColorTokenPalette>("palette", palette, defaultValue: null));

            properties.Add(new FlagProperty("enabled", enabled, ifFalse: "Disabled"));
            properties.Add(new FlagProperty("selected", selected, ifTrue: "Selected"));
            properties.Add(new DiagnosticsProperty<Key>("focusKey", focusKey, defaultValue: Key.None));
            properties.Add(new DiagnosticsProperty<Action>("onClick", onClick, defaultValue: null));
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

                _widgetStateController.Toggle(WidgetState.Selected, widget.selected);
                _widgetStateController.Toggle(WidgetState.Disabled, !widget.enabled);
            }
        }

        public override bool CanReconcile(HButton oldWidget) {
            return base.CanReconcile(oldWidget) && widget.controller == oldWidget.controller;
        }

        public override void DidUpdateWidget(HButton oldWidget) {
            if (widget != oldWidget && oldWidget.controller == null) {
                _controller.onClick = widget.onClick;
                _controller.enabled = widget.enabled;
                _widgetStateController.Toggle(WidgetState.Selected, widget.selected);
                _widgetStateController.Toggle(WidgetState.Disabled, !widget.enabled);
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

            return new HSubstanceBox(
                controller: _widgetStateController,
                substances: effective.layers,
                alignment: effective.alignment,
                builder: widget.child,
                boxKey: widget.focusKey,
                boxModifiers: modifierProperty
            ).Fill();
        }
    }
}