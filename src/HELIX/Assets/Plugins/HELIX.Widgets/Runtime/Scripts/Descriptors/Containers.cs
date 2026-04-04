using System;
using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Input;
using HELIX.Widgets.Layout;
using HELIX.Widgets.Visual;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Descriptors {
    public abstract class DirectionalContainerWidget : Widget {
        public IReadOnlyList<Widget> children;
        public Align crossAxisAlign = Align.Center;
        public float gap = 0f;
        public Justify mainAxisAlign = Justify.FlexStart;
        public bool reverse = false;
    }

    public class FlexRow : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            var row = new RowElement();
            row.Reconcile(this);
            return row;
        }
    }

    public class FlexColumn : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            var row = new ColumnElement();
            row.Reconcile(this);
            return row;
        }
    }

    public class FlexCenter : Widget {
        public Widget child;

        public override IWidgetElement CreateElement() {
            var center = new CenterElement();
            center.Reconcile(this);
            return center;
        }
    }

    public class FlexAlign : Widget {
        public Widget child;
        public Alignment alignment;

        public override IWidgetElement CreateElement() {
            var align = new FlexAlignElement();
            align.Reconcile(this);
            return align;
        }
    }

    public class ButtonBuilder : Widget {
        private static readonly FocusModifier _defaultFocus = new(true, PickingMode.Position, 0) { isFallback = true };

        public ButtonBuilder() {
            modifiers.Add(_defaultFocus);
        }

        public WidgetStateBuilder builder;
        public Action onClick;
        public Alignment alignment = Alignment.Center;
        public bool enabled = true;
        public bool selected = false;

        public override IWidgetElement CreateElement() {
            var button = new GenericButton();
            button.Reconcile(this);
            return button;
        }
    }

    public class Container : Widget {
        public Widget child;

        public Alignment alignment = Alignment.Center;
        public BackgroundStyle backgroundStyle;
        public Border border = Border.None;
        public BorderRadius borderRadius = BorderRadius.None;
        public BoxConstraints constraints = BoxConstraints.Initial;

        // ReSharper disable once InconsistentNaming
        public StyleLength2 size {
            set => constraints = BoxConstraints.Preferred(value);
        }

        public override IWidgetElement CreateElement() {
            var element = new ContainerElement();
            element.Reconcile(this);
            return element;
        }

        public class ContainerElement : SingleChildWidgetHostImage<Container> {
            public override Widget GetChild(Container widget) {
                return widget.child;
            }

            public override void Apply(Container previous, Container widget) {
                if (previous == null || !Equals(previous.backgroundStyle, widget.backgroundStyle)) {
                    (widget.backgroundStyle ?? BackgroundStyle.Default).Apply(this);
                }

                widget.border.Apply(this);
                widget.borderRadius.Apply(this);
                widget.constraints.Apply(this);
                widget.alignment.AlignAsColumn(this);
            }
        }
    }

    public class BoxShadowStyle : IEquatable<BoxShadowStyle> {
        public float blurRadius = 4f;
        public Vector4 borderRadius = new(0, 0, 0, 0);
        public Vector2 offset = Vector2.zero;
        public Color shadowColor = new(0, 0, 0, 0.25f);
        public float spreadRadius;

        public bool Equals(BoxShadowStyle other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return blurRadius.Equals(other.blurRadius) && borderRadius.Equals(other.borderRadius) &&
                   offset.Equals(other.offset) && shadowColor.Equals(other.shadowColor) &&
                   spreadRadius.Equals(other.spreadRadius);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BoxShadowStyle)obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(blurRadius, borderRadius, offset, shadowColor, spreadRadius);
        }
    }

    public class BoxShadow : Widget {
        public Widget child;
        public float blurRadius = 4f;
        public Vector4 borderRadius = new(0, 0, 0, 0);
        public Vector2 offset = Vector2.zero;
        public Color shadowColor = new(0, 0, 0, 0.25f);
        public float spreadRadius;

        public override IWidgetElement CreateElement() {
            var element = new BoxShadowElement();
            element.Reconcile(this);
            return element;
        }
    }

    public class SimpleButtonStyle {
        public IWidgetStateProperty<BackgroundStyle> backgroundStyle = WidgetStateProperties.Never<BackgroundStyle>();
        public IWidgetStateProperty<TextStyle> textStyle = WidgetStateProperties.Never<TextStyle>();
        public IWidgetStateProperty<Border> border = WidgetStateProperties.Never<Border>();
        public IWidgetStateProperty<BorderRadius> borderRadius = WidgetStateProperties.Never<BorderRadius>();
        public IWidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();
        public IWidgetStateProperty<StyleLength4> padding = WidgetStateProperties.Never<StyleLength4>();
        public IWidgetStateProperty<Transition[]> transitions = WidgetStateProperties.Never<Transition[]>();
        public IWidgetStateProperty<Alignment> alignment = WidgetStateProperties.Never<Alignment>();
        public IWidgetStateProperty<BoxShadowStyle> boxShadow = WidgetStateProperties.Never<BoxShadowStyle>();
    }

    public class SimpleButton : StatelessWidget<SimpleButton> {
        public Widget child;
        public Action onClick;
        public bool enabled = true;
        public bool selected = false;

        public SimpleButtonStyle style = new();

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
                            new TransitionModifier(style.transitions.ResolveOrDefault(state, Array.Empty<Transition>()))
                        }
                    }.Fill();
                    var boxShadow = style.boxShadow.ResolveOrDefault(state);
                    if (boxShadow != null) {
                        inner = new BoxShadow {
                            blurRadius = boxShadow.blurRadius,
                            borderRadius = boxShadow.borderRadius,
                            offset = boxShadow.offset,
                            shadowColor = boxShadow.shadowColor,
                            spreadRadius = boxShadow.spreadRadius,
                            child = inner
                        }.Fill();
                    }

                    return inner;
                },
                onClick = onClick,
                enabled = enabled,
                selected = selected
            };
        }
    }
}