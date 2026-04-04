using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using HELIX.Extensions;
using HELIX.Types;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Descriptors {
    public abstract class Modifier {
        public bool isFallback = false;
        public abstract void Apply(VisualElement element);
        public abstract void Reset(VisualElement element);
        public virtual bool HasChanged([CanBeNull] Modifier previous) => true;

        public override bool Equals(object obj) {
            return obj is Modifier other && GetType() == other.GetType();
        }

        public bool DeepEquals(Modifier other) {
            if (ReferenceEquals(this, other)) return true;
            return !HasChanged(other);
        }

        public override int GetHashCode() => GetType().GetHashCode();

        public static void ApplyDelta(Widget previous, Widget next, VisualElement element) {
            ApplyDelta(previous?.GetModifiers(), next?.GetModifiers(), element);
        }

        public static void ApplyDelta(HashSet<Modifier> previous, HashSet<Modifier> current, VisualElement element) {
            if (current == null || element == null) { return; }

            if (previous != null) {
                foreach (var modifier in previous) {
                    if (!current.Contains(modifier)) { modifier.Reset(element); }
                }

                foreach (var modifier in current) {
                    if (previous.TryGetValue(modifier, out var prev) && modifier.DeepEquals(prev)) { continue; }

                    modifier.Apply(element);
                }
            } else {
                foreach (var modifier in current) { modifier.Apply(element); }
            }
        }
    }

    public static class ModifierExtensions {
        public static T WithModifier<T>(this T element, Modifier modifier) where T : Widget {
            element.AddModifier(modifier);
            return element;
        }

        public static T Flexible<T>(
            this T element,
            StyleFloat? grow = null,
            StyleFloat? shrink = null,
            Align selfCrossAxisAlign = Align.Auto
        ) where T : Widget {
            element.AddModifier(
                new FlexibleModifier(
                    selfCrossAxisAlign: selfCrossAxisAlign,
                    grow: grow ?? StyleKeyword.Initial,
                    shrink: shrink ?? StyleKeyword.Initial
                )
            );
            return element;
        }
        
        public static T Fill<T>(this T element) where T : Widget {
            return element.WithModifier(FlexibleModifier.Fill);
        }

        public static T Expand<T>(this T element, float flex = 1f, Align selfCrossAxisAlign = Align.Auto)
            where T : Widget {
            if (Mathf.Approximately(flex, 1f) && selfCrossAxisAlign == Align.Auto) {
                return element.WithModifier(FlexibleModifier.Expand);
            }

            return element.Flexible(flex, flex, selfCrossAxisAlign);
        }

        public static T Offset<T>(
            this T element,
            StyleLength4? offset = null,
            Position offsetType = Position.Relative
        ) where T : Widget {
            if (offset == null && offsetType == Position.Relative) { return element.WithModifier(OffsetModifier.None); }

            element.AddModifier(
                new OffsetModifier(
                    offset: offset ?? StyleLength4.Initial,
                    offsetType: offsetType
                )
            );
            return element;
        }

        public static T Stretch<T>(this T element) where T : Widget {
            return element.WithModifier(OffsetModifier.Stretch);
        }

        public static T Size<T>(
            this T element,
            StyleLength2? size = null,
            StyleLength2? minSize = null,
            StyleLength2? maxSize = null
        ) where T : Widget {
            if (size == null && minSize == null && maxSize == null) { return element.WithModifier(SizeModifier.None); }

            element.AddModifier(
                new SizeModifier(
                    size: size ?? StyleLength2.Initial,
                    minSize: minSize ?? StyleLength2.Initial,
                    maxSize: maxSize ?? StyleLength2.Initial
                )
            );
            return element;
        }
    }

    public class SpacingModifier : Modifier {
        public readonly StyleLength4 padding;
        public readonly StyleLength4 margin;

        public SpacingModifier(StyleLength4 padding, StyleLength4 margin) {
            this.padding = padding;
            this.margin = margin;
        }

        public SpacingModifier() {
            padding = StyleLength4.Initial;
            margin = StyleLength4.Initial;
        }

        public override void Apply(VisualElement element) {
            element.style.paddingLeft = padding.l;
            element.style.paddingTop = padding.t;
            element.style.paddingRight = padding.r;
            element.style.paddingBottom = padding.b;
            element.style.marginLeft = margin.l;
            element.style.marginTop = margin.t;
            element.style.marginRight = margin.r;
            element.style.marginBottom = margin.b;
        }

        public override void Reset(VisualElement element) {
            element.style.paddingLeft = StyleKeyword.Initial;
            element.style.paddingTop = StyleKeyword.Initial;
            element.style.paddingRight = StyleKeyword.Initial;
            element.style.paddingBottom = StyleKeyword.Initial;
            element.style.marginLeft = StyleKeyword.Initial;
            element.style.marginTop = StyleKeyword.Initial;
            element.style.marginRight = StyleKeyword.Initial;
            element.style.marginBottom = StyleKeyword.Initial;
        }

        public static SpacingModifier Padding(StyleLength4 padding) => new(padding, StyleLength4.Initial);
        public static SpacingModifier Margin(StyleLength4 margin) => new(StyleLength4.Initial, margin);

        public static SpacingModifier Of(StyleLength4? padding = null, StyleLength4? margin = null) =>
            new(padding ?? StyleLength4.Initial, margin ?? StyleLength4.Initial);

        public override bool HasChanged(Modifier previous) {
            if (previous is not SpacingModifier prev) return true;
            return !padding.Equals(prev.padding) || !margin.Equals(prev.margin);
        }

        public static readonly SpacingModifier Initial = new(StyleLength4.Initial, StyleLength4.Initial);
        public static readonly SpacingModifier None = new(StyleLength4.Initial, StyleLength4.Initial);
        public static readonly SpacingModifier NoPadding = new(StyleLength4.Zero, StyleLength4.Initial);
        public static readonly SpacingModifier NoMargin = new(StyleLength4.Zero, StyleLength4.Initial);
    }

    public class FlexibleModifier : Modifier {
        public readonly Align selfCrossAxisAlign;
        public readonly StyleFloat grow;
        public readonly StyleFloat shrink;

        public FlexibleModifier(StyleFloat grow, StyleFloat shrink, Align selfCrossAxisAlign) {
            this.selfCrossAxisAlign = selfCrossAxisAlign;
            this.grow = grow;
            this.shrink = shrink;
        }

        public FlexibleModifier() {
            selfCrossAxisAlign = Align.Auto;
            grow = 0;
            shrink = 1;
        }

        public override void Apply(VisualElement element) {
            element.style.alignSelf = selfCrossAxisAlign;
            element.style.flexGrow = grow;
            element.style.flexShrink = shrink;
        }

        public override void Reset(VisualElement element) {
            element.style.alignSelf = StyleKeyword.Initial;
            element.style.flexGrow = StyleKeyword.Initial;
            element.style.flexShrink = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not FlexibleModifier prev) return true;
            return grow != prev.grow || shrink != prev.shrink || selfCrossAxisAlign != prev.selfCrossAxisAlign;
        }

        public static FlexibleModifier Of(StyleFloat grow, StyleFloat shrink, Align selfCrossAxisAlign = Align.Auto) =>
            new(grow, shrink, selfCrossAxisAlign);

        public static readonly FlexibleModifier Expand = new(1f, 1f, Align.Auto);
        public static readonly FlexibleModifier Shrink = new(0f, 1f, Align.Auto);
        public static readonly FlexibleModifier Tight = new(0f, 0f, Align.Auto);
        public static readonly FlexibleModifier Fill = new(1f, 1f, Align.Stretch);
        public static readonly FlexibleModifier TightStretch = new(0f, 0f, Align.Stretch);
    }

    public class OffsetModifier : Modifier {
        public readonly StyleLength4 offset;
        public readonly Position offsetType;

        public OffsetModifier(StyleLength4 offset, Position offsetType) {
            this.offset = offset;
            this.offsetType = offsetType;
        }

        public OffsetModifier() {
            offset = StyleLength4.Initial;
            offsetType = Position.Relative;
        }

        public override void Apply(VisualElement element) {
            element.style.position = offsetType;
            element.style.left = offset.l;
            element.style.top = offset.t;
            element.style.right = offset.r;
            element.style.bottom = offset.b;
        }

        public override void Reset(VisualElement element) {
            element.style.position = StyleKeyword.Initial;
            element.style.left = StyleKeyword.Initial;
            element.style.top = StyleKeyword.Initial;
            element.style.right = StyleKeyword.Initial;
            element.style.bottom = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not OffsetModifier prev) return true;
            return offsetType != prev.offsetType || !offset.Equals(prev.offset);
        }

        public static OffsetModifier Absolute(StyleLength4 offset) => new(offset, Position.Absolute);
        public static OffsetModifier Relative(StyleLength4 offset) => new(offset, Position.Relative);

        public static readonly OffsetModifier Stretch = new(StyleLength4.Zero, Position.Absolute);
        public static readonly OffsetModifier None = new(StyleLength4.Initial, Position.Relative);
    }

    public class SizeModifier : Modifier {
        public readonly StyleLength2 size;
        public readonly StyleLength2 minSize;
        public readonly StyleLength2 maxSize;

        public SizeModifier(StyleLength2 size, StyleLength2 minSize, StyleLength2 maxSize) {
            this.size = size;
            this.minSize = minSize;
            this.maxSize = maxSize;
        }

        public SizeModifier() {
            size = StyleLength2.Initial;
            minSize = StyleLength2.Initial;
            maxSize = StyleLength2.Initial;
        }

        public override void Apply(VisualElement element) {
            element.style.width = size.w;
            element.style.height = size.h;
            element.style.minWidth = minSize.w;
            element.style.minHeight = minSize.h;
            element.style.maxWidth = maxSize.w;
            element.style.maxHeight = maxSize.h;
        }

        public override void Reset(VisualElement element) {
            element.style.width = StyleKeyword.Initial;
            element.style.height = StyleKeyword.Initial;
            element.style.minWidth = StyleKeyword.Initial;
            element.style.minHeight = StyleKeyword.Initial;
            element.style.maxWidth = StyleKeyword.Initial;
            element.style.maxHeight = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not SizeModifier prev) return true;
            return !size.Equals(prev.size) || !minSize.Equals(prev.minSize) || !maxSize.Equals(prev.maxSize);
        }

        public static SizeModifier Of(StyleLength width, StyleLength height) =>
            new(
                new StyleLength2(width, height),
                StyleLength2.Initial,
                StyleLength2.Initial
            );

        public static SizeModifier Of(BoxConstraints constraints) =>
            new(
                constraints.preferred,
                constraints.min,
                constraints.max
            );

        public static readonly SizeModifier None = new(
            StyleLength2.Initial,
            StyleLength2.Initial,
            StyleLength2.Initial
        );
    }

    public class TransformModifier : Modifier {
        public readonly StyleTranslate translate;
        public readonly StyleRotate rotate;
        public readonly StyleScale scale;

        public TransformModifier(StyleTranslate translate, StyleRotate rotate, StyleScale scale) {
            this.translate = translate;
            this.rotate = rotate;
            this.scale = scale;
        }

        public TransformModifier() {
            translate = StyleKeyword.Initial;
            rotate = StyleKeyword.Initial;
            scale = StyleKeyword.Initial;
        }

        public override void Apply(VisualElement element) {
            element.style.translate = translate;
            element.style.rotate = rotate;
            element.style.scale = scale;
        }

        public override void Reset(VisualElement element) {
            element.style.translate = StyleKeyword.Initial;
            element.style.rotate = StyleKeyword.Initial;
            element.style.scale = StyleKeyword.Initial;
        }

        public static readonly TransformModifier None = new(
            StyleKeyword.Initial,
            StyleKeyword.Initial,
            StyleKeyword.Initial
        );
    }

    public class TransitionModifier : Modifier {
        public readonly Transition[] transitions;

        public TransitionModifier(Transition[] transitions) {
            this.transitions = transitions;
        }

        public TransitionModifier() {
            transitions = Array.Empty<Transition>();
        }

        public override void Apply(VisualElement element) {
            element.Transitions(transitions);
        }

        public override void Reset(VisualElement element) {
            element.Transitions();
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not TransitionModifier prev) return true;
            if (transitions.Length != prev.transitions.Length) return true;
            for (var i = 0; i < transitions.Length; i++) {
                if (!transitions[i].Equals(prev.transitions[i])) return true;
            }

            return false;
        }

        public static TransitionModifier Of(params Transition[] transitions) => new(transitions);

        public static readonly TransitionModifier None = new(Array.Empty<Transition>());
    }

    public class FocusModifier : Modifier {
        public readonly bool focusable;
        public readonly PickingMode pickingMode;
        public readonly int tabIndex;

        public FocusModifier(bool focusable, PickingMode pickingMode, int tabIndex) {
            this.focusable = focusable;
            this.pickingMode = pickingMode;
            this.tabIndex = tabIndex;
        }

        public override void Apply(VisualElement element) {
            element.focusable = focusable;
            element.pickingMode = pickingMode;
            element.tabIndex = tabIndex;
        }

        public override void Reset(VisualElement element) {
            element.focusable = false;
            element.pickingMode = PickingMode.Position;
            element.tabIndex = -1;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not FocusModifier prev) return true;
            return focusable != prev.focusable || pickingMode != prev.pickingMode || tabIndex != prev.tabIndex;
        }

        public static FocusModifier Of(
            int tabIndex = 0,
            bool focusable = true,
            PickingMode mode = PickingMode.Position
        ) {
            return new FocusModifier(focusable, mode, tabIndex);
        }

        public static readonly FocusModifier Focusable = new(true, PickingMode.Position, 0);
        public static readonly FocusModifier FocusableNoTab = new(true, PickingMode.Position, -1);
        public static readonly FocusModifier Ignore = new(false, PickingMode.Ignore, -1);
        public static readonly FocusModifier None = new(false, PickingMode.Position, -1);
    }

    public class TextStyleModifier : Modifier {
        public readonly TextStyle style;

        public TextStyleModifier(TextStyle style) {
            this.style = style;
        }

        public override void Apply(VisualElement element) {
            (style ?? TextStyle.Default).Apply(element);
        }

        public override void Reset(VisualElement element) {
            TextStyle.Default.Apply(element);
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not TextStyleModifier prev) return true;
            return !Equals(style, prev.style);
        }

        public static TextStyleModifier Of(TextStyle style) => new(style);
    }

    public class BackgroundStyleModifier : Modifier {
        public readonly BackgroundStyle style;

        public BackgroundStyleModifier(BackgroundStyle style) {
            this.style = style;
        }

        public override void Apply(VisualElement element) {
            (style ?? BackgroundStyle.Default).Apply(element);
        }

        public override void Reset(VisualElement element) {
            BackgroundStyle.Default.Apply(element);
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not BackgroundStyleModifier prev) return true;
            return !Equals(style, prev.style);
        }

        public static BackgroundStyleModifier Of(BackgroundStyle style) => new(style);
    }

    public class ClipModifier : Modifier {
        public readonly bool enabled;

        public ClipModifier(bool enabled) {
            this.enabled = enabled;
        }

        public override void Apply(VisualElement element) {
            element.style.overflow = enabled ? Overflow.Hidden : Overflow.Visible;
        }

        public override void Reset(VisualElement element) {
            element.style.overflow = StyleKeyword.Initial;
        }
        
        public static readonly ClipModifier Clip = new(true);
        public static readonly ClipModifier None = new(false);
    }
}