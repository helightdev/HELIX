using System;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Styles;

namespace HELIX.Widgets.Universal {
    public class StyleButton : StatelessWidget<StyleButton> {
        public Widget child;
        public bool enabled = true;
        public Action onClick;
        public bool selected = false;

        public SimpleButtonStyle style = new();

        public StyleButton() {
            AddModifier(ModifierFallbacks.TightStretch);
        }

        public override Widget Build(BuildContext context) {
            return new ButtonBuilder {
                builder = (_, state) => {
                    Widget inner = new Container {
                        backgroundStyle = style.backgroundStyle.ResolveOrDefault(state),
                        borderRadius = style.borderRadius.ResolveOrDefault(state, BorderRadius.None),
                        constraints = style.constraints.ResolveOrDefault(state, BoxConstraints.Initial),
                        border = style.border.ResolveOrDefault(state, Border.None),
                        alignment = style.alignment.ResolveOrDefault(state, Alignment.Center),
                        child = child,
                        Modifiers = new Modifier[] {
                            new TextStyleModifier(style.textStyle.ResolveOrDefault(state)), new SpacingModifier(
                                style.padding.ResolveOrDefault(state, StyleLength4.Zero),
                                StyleLength4.Zero
                            ),
                            new TransitionsModifier(
                                style.transitions.ResolveOrDefault(state, Array.Empty<Transition>())
                            )
                        }
                    }.Fill();
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
}