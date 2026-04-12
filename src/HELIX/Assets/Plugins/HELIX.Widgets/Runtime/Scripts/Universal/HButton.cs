using System;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
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
            Debug.Log($"StyleButton: {effective}");
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
                            new SizeModifier(effective.constraints.ResolveOrDefault(state, BoxConstraints.Initial)).Fallback(),
                            new TextStyleModifier(effective.textStyle.ResolveOrDefault(state)).Fallback(),
                            new PaddingModifier(effective.padding.ResolveOrDefault(state, StyleLength4.Zero)).Fallback(),
                            new OpacityModifier(effective.opacity.ResolveOrDefault(state, 1f)).Fallback(),
                        }
                    }.Fill();
                    inner.AddModifiers(effective.modifiers.ResolveOrDefault(state, Array.Empty<Modifier>()));
                    
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