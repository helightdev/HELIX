using System;
using System.Collections;
using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Styles;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
  /// <summary>
  /// <para>Modifiers provide a way to manipulate the underlying <see cref="VisualElement"/> of a widget.</para>
  /// </summary>
  /// <remarks>
  /// <para>
  /// Modifiers may alter appearance, layout, or other properties of the element.
  /// They can also be used to register even callbacks, but are expected to behave mostly immutably.
  /// </para>
  /// <para>
  /// When applying modifiers, deltas are computed and passed to the modifier using the
  /// <see cref="Modifier.Apply(Modifier,VisualElement)"/> method. Once a modifier is not used anymore,
  /// it is expected to call <see cref="Modifier.Reset(VisualElement)"/> to reset any alterations it may have made to
  /// the underlying <see cref="VisualElement"/>.
  /// </para>
  /// <para>
  /// All modifiers of the same type must be equal to each other, changes are tracked using
  /// <see cref="Modifier.DeepEquals(Modifier)"/> and the respective <see cref="Modifier.HasChanged(Modifier)"/>
  /// implementations.
  /// </para>
  /// <para>Mustn't expect any ordering guarantees as <see cref="ModifierSet"/> uses a hashset internally.</para>
  /// </remarks>
  public abstract class Modifier : DiagnosticableBase {
    public bool isFallback;
    public virtual void Apply(VisualElement element) { }
    public virtual void Reset(VisualElement element) { }

    public virtual void Apply(Modifier prev, VisualElement element) {
      Apply(element);
    }

    public virtual bool HasChanged([CanBeNull] Modifier previous) {
      return true;
    }

    public override bool Equals(object obj) {
      return obj is Modifier other && GetType() == other.GetType();
    }

    public bool DeepEquals(Modifier other) {
      if (ReferenceEquals(this, other)) return true;
      return !HasChanged(other);
    }

    public override int GetHashCode() {
      return GetType().GetHashCode();
    }

    public static void ApplyDelta(Widget previous, Widget next, VisualElement element) {
      ApplyDelta(previous?.GetModifiers(), next?.GetModifiers(), element);
    }

    public static void ApplyDelta(ModifierSet previous, ModifierSet current, VisualElement element) {
      if (current == null || element == null) return;
      if (ReferenceEquals(previous, current)) return;

      if (previous != null) {
        foreach (var modifier in previous) {
          if (!current.Contains(modifier))
            modifier.Reset(element);
        }

        foreach (var modifier in current) {
          if (previous.TryGetValue(modifier, out var prev) && modifier.DeepEquals(prev)) continue;

          modifier.Apply(prev, element);
        }
      } else
        foreach (var modifier in current)
          modifier.Apply(element);
    }

    public override string ToStringShort() {
      var name = GetType().Name;
      if (name.EndsWith("Modifier")) name = name[..^"Modifier".Length];
      var constant = FindConstantName();
      if (constant != null) name = $"{name}.{constant}";
      return name;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      if (FindConstantName() != null) return;

      properties.Add(new FlagProperty("isFallback", isFallback, "Fallback"));
      FillModifierProperties(properties);
    }

    public virtual void FillModifierProperties(DiagnosticPropertiesBuilder properties) { }

    protected virtual string FindConstantName() {
      return null;
    }

    public static void Append(HashSet<Modifier> modifiers, Modifier modifier) {
      if (modifiers.TryGetValue(modifier, out var existing)) {
        if (existing.isFallback) modifiers.Remove(existing);
        else if (modifier.isFallback) return;
        else {
          throw new InvalidOperationException(
            $"Modifier of type {modifier.GetType().Name} already exists on widget. Modifiers must be unique per widget."
          );
        }
      }

      modifiers.Add(modifier);
    }

    public static implicit operator Modifier(BoxConstraints constraints) {
      return new SizeModifier(constraints);
    }

    public static implicit operator Modifier(BackgroundStyle style) {
      return new BackgroundStyleModifier(style);
    }

    public static implicit operator Modifier(TextStyle style) {
      return new TextStyleModifier(style);
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

    public static T Shrink<T>(this T element) where T : Widget {
      return element.WithModifier(FlexibleModifier.Shrink);
    }

    public static T Tight<T>(this T element) where T : Widget {
      return element.WithModifier(FlexibleModifier.Tight);
    }

    public static T TightStretch<T>(this T element) where T : Widget {
      return element.WithModifier(FlexibleModifier.TightStretch);
    }

    public static T Expand<T>(this T element, float flex = 1f, Align selfCrossAxisAlign = Align.Auto)
      where T : Widget {
      if (Mathf.Approximately(flex, 1f) && selfCrossAxisAlign == Align.Auto)
        return element.WithModifier(FlexibleModifier.Expand);

      return element.Flexible(flex, flex, selfCrossAxisAlign);
    }

    public static T Positioned<T>(
      this T element,
      StyleLength4? offset = null,
      Position offsetType = Position.Absolute
    ) where T : Widget {
      switch (offset) {
        case null when offsetType == Position.Relative: return element.WithModifier(PositionModifier.None);
        case null when offsetType == Position.Absolute: return element.WithModifier(PositionModifier.Stretch);
        default:
          element.AddModifier(
            new PositionModifier(
              offset ?? StyleLength4.Initial,
              offsetType
            )
          );
          return element;
      }
    }

    public static T Stretch<T>(this T element) where T : Widget {
      return element.WithModifier(PositionModifier.Stretch);
    }

    public static T Size<T>(
      this T element,
      StyleLength2? size = null,
      StyleLength2? minSize = null,
      StyleLength2? maxSize = null
    ) where T : Widget {
      if (size == null && minSize == null && maxSize == null) return element.WithModifier(SizeModifier.None);

      element.AddModifier(
        new SizeModifier(
          size ?? StyleLength2.Initial,
          minSize ?? StyleLength2.Initial,
          maxSize ?? StyleLength2.Initial
        )
      );
      return element;
    }

    public static T Size<T>(this T element, StyleLength? width = null, StyleLength? height = null)
      where T : Widget {
      return element.WithModifier(
        SizeModifier.Of(
          width ?? StyleKeyword.Initial,
          height ?? StyleKeyword.Initial
        )
      );
    }

    public static T Const<T>(this T element, params object[] values) where T : Widget {
      element.constants = values;
      return element;
    }

    public static T Display<T>(this T element, bool display) where T : Widget {
      return element.WithModifier(display ? DisplayModifier.Visible : DisplayModifier.Hidden);
    }

    public static T Visibility<T>(this T element, bool visible) where T : Widget {
      return element.WithModifier(visible ? VisibilityModifier.Visible : VisibilityModifier.Hidden);
    }

    public static T Opacity<T>(this T element, float opacity) where T : Widget {
      return element.WithModifier(OpacityModifier.Of(opacity));
    }

    public static T Padding<T>(this T element, StyleLength4 padding) where T : Widget {
      return element.WithModifier(new PaddingModifier(padding));
    }

    public static T Margin<T>(this T element, StyleLength4 margin) where T : Widget {
      return element.WithModifier(new MarginModifier(margin));
    }

    public static T Clip<T>(this T element) where T : Widget {
      return element.WithModifier(ClipModifier.Clip);
    }

    public static T Fallback<T>(this T modifier) where T : Modifier {
      modifier.isFallback = true;
      return modifier;
    }
  }

  public class ModifierSet : DiagnosticableBase, IReadOnlyCollection<Modifier> {
    public static readonly ModifierSet Empty = new() { ReadOnly = true };

    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    public static readonly ModifierSet DefaultFlexFill = new ModifierSet {
      ModifierFallbacks.ImplicitFlexFill
    }.Sealed();

    /// <seealso cref="ModifierFallbacks.FlexTight"/>
    public static readonly ModifierSet DefaultFlexTight = new ModifierSet {
      ModifierFallbacks.FlexTight
    }.Sealed();

    /// <seealso cref="ModifierFallbacks.StackingStretch"/>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    public static readonly ModifierSet DefaultFlexFillAndStacking = new ModifierSet {
      ModifierFallbacks.ImplicitFlexFill,
      ModifierFallbacks.StackingStretch
    }.Sealed();

    private readonly HashSet<Modifier> _modifiers;

    public ModifierSet(int capacity = 1) {
      _modifiers = new HashSet<Modifier>(capacity);
    }

    public ModifierSet(IEnumerable<Modifier> modifiers) {
      _modifiers = new HashSet<Modifier>(modifiers);
    }

    public bool ReadOnly { get; private set; }

    public IEnumerator<Modifier> GetEnumerator() {
      return _modifiers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public int Count => _modifiers.Count;

    public bool Add(Modifier modifier) {
      if (modifier == null) return true;
      if (ReadOnly) throw new InvalidOperationException("Cannot add modifiers to a read-only ModifierSet.");

      if (_modifiers.TryGetValue(modifier, out var existing)) {
        if (existing.isFallback) _modifiers.Remove(existing);
        else if (modifier.isFallback) return true;
        else return false;
      }

      _modifiers.Add(modifier);
      return true;
    }

    public bool Add(IEnumerable<Modifier> modifiers) {
      if (modifiers == null) return true;
      if (ReadOnly) throw new InvalidOperationException("Cannot add modifiers to a read-only ModifierSet.");

      var result = true;
      foreach (var modifier in modifiers) {
        if (!Add(modifier))
          result = false;
      }

      return result;
    }

    public void AddCollection(IReadOnlyCollection<Modifier> modifiers) {
      if (modifiers == null || modifiers.Count == 0) return;
      if (ReadOnly) throw new InvalidOperationException("Cannot add modifiers to a read-only ModifierSet.");

      _modifiers.EnsureCapacity(_modifiers.Count + modifiers.Count);
      foreach (var modifier in modifiers) Add(modifier);
    }

    public bool Contains(Modifier modifier) {
      return _modifiers.Contains(modifier);
    }

    public bool TryGetValue(Modifier modifier, out Modifier existing) {
      return _modifiers.TryGetValue(modifier, out existing);
    }

    public void AddThrowing(Modifier modifier) {
      if (modifier == null) return;
      if (ReadOnly) throw new InvalidOperationException("Cannot add modifiers to a read-only ModifierSet.");

      if (_modifiers.TryGetValue(modifier, out var existing)) {
        if (existing.isFallback) _modifiers.Remove(existing);
        else if (modifier.isFallback) return;
        else {
          throw new InvalidOperationException(
            $"Modifier of type {modifier.GetType().Name} already exists on widget. Modifiers must be unique per widget."
          );
        }
      }

      _modifiers.Add(modifier);
    }

    public void AddThrowing(IEnumerable<Modifier> modifiers) {
      if (modifiers == null) return;
      if (ReadOnly) throw new InvalidOperationException("Cannot add modifiers to a read-only ModifierSet.");

      foreach (var modifier in modifiers) AddThrowing(modifier);
    }

    public void AddThrowingCollection(IReadOnlyCollection<Modifier> modifiers) {
      if (modifiers == null) return;
      if (ReadOnly) throw new InvalidOperationException("Cannot add modifiers to a read-only ModifierSet.");

      _modifiers.EnsureCapacity(_modifiers.Count + modifiers.Count);
      foreach (var modifier in modifiers) AddThrowing(modifier);
    }

    public ModifierSet Sealed() {
      ReadOnly = true;
      return this;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new IterableProperty<Modifier>("values", _modifiers, ifEmpty: null, showName: false));
    }
  }
}