using System;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;

namespace HELIX.Widgets.Universal {
    public class HButton : StatelessWidget<HButton> {
        public Widget child;
        public bool enabled = true;
        public Action onClick;
        public bool selected = false;

        public HButtonStyle style;

        public override Widget Build(BuildContext context) {
            var effective = style ?? context.GetThemed(PrimitiveTheme.ButtonTheme);
            return new ButtonBuilder {
                builder = (_, state) => {
                    Widget inner = child;
                    inner = new HBox {
                        backgroundStyle = effective.backgroundStyle.ResolveOrDefault(state),
                        borderRadius = effective.borderRadius.ResolveOrDefault(state, BorderRadius.None),
                        border = effective.border.ResolveOrDefault(state, Border.None),
                        alignment = effective.alignment.ResolveOrDefault(state, Alignment.Center),
                        child = inner,
                        Modifiers = new Modifier[] {
                            new SizeModifier(effective.constraints.ResolveOrDefault(state, BoxConstraints.Initial))
                                .Fallback(),
                            new TextStyleModifier(effective.textStyle.ResolveOrDefault(state)).Fallback(),
                            new PaddingModifier(effective.padding.ResolveOrDefault(state, StyleLength4.Zero))
                                .Fallback(),
                            new OpacityModifier(effective.opacity.ResolveOrDefault(state, 1f)).Fallback(),
                        }
                    }.Fill();
                    inner.AddModifiers(effective.modifiers.ResolveOrDefault(state, ModifierSet.Empty));

                    // var boxShadow = style.boxShadow.ResolveOrDefault(state);
                    // if (boxShadow != null) {
                    //     inner = new BoxShadow {
                    //         blurRadius = boxShadow.blurRadius,
                    //         borderRadius = boxShadow.borderRadius,
                    //         offset = boxShadow.offset,
                    //         shadowColor = boxShadow.shadowColor,
                    //         spreadRadius = boxShadow.spreadRadius,
                    //         child = inner
                    //     }.Fill();
                    // }

                    return inner;
                },
                onClick = onClick,
                enabled = enabled,
                selected = selected
            };
        }
    }

    public class HShapeButton : StatefulWidget<HShapeButton> {
        public Widget child;
        public ButtonController controller;

        public bool enabled = true;
        public bool selected = false;
        public Action onClick;

        public HButtonStyle style = null;
        public HInputRadius? radius = null;
        public HButtonVariant? variant = null;
        public HButtonSize? size = null;
        public ColorTokenPalette palette = null;

        public override State<HShapeButton> CreateState() {
            return new HShapeButtonState();
        }
    }

    public class HShapeButtonState : State<HShapeButton> {
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

        public override bool CanReconcile(HShapeButton oldWidget) {
            return base.CanReconcile(oldWidget) && widget.controller == oldWidget.controller;
        }

        public override void DidUpdateWidget(HShapeButton oldWidget) {
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
                effective = DefaultButtonStyles.DefaultThemeOf(
                    context,
                    widget.variant ?? HButtonVariant.Default,
                    widget.size ?? HButtonSize.Medium,
                    widget.radius ?? HInputRadius.Medium,
                    palette: widget.palette
                );
            } else { effective = context.GetThemed(PrimitiveTheme.ButtonTheme); }

            Widget inner = widget.child;
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

            inner = new HSubstanceBox {
                controller = _widgetStateController,
                substances = effective.layers,
                alignment = effective.alignment,
                builder = inner,
                boxModifiers = modifierProperty
            }.Fill();

            return inner;
        }
    }
}