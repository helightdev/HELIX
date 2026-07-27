using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Diagnostics;
using HELIX.Diagnostics.Properties;
using UnityEngine.UIElements;

// ReSharper disable Unity.BurstLoadingManagedType
// ReSharper disable Unity.BurstAccessingManagedMethod

namespace HELIX.NW {
  [Flags]
  public enum StateFlag : ushort {
    None = 0,
    Hovered = 1 << 0,
    Focused = 1 << 1,
    Pressed = 1 << 2,
    Dragged = 1 << 3,
    Selected = 1 << 4,
    Disabled = 1 << 5,
    Error = 1 << 6,
    Navigated = 1 << 7,
    Special1 = 1 << 8,
    Special2 = 1 << 9,
    InputKeyboardMouse = 1 << 11,
    InputGamepad = 1 << 12,
    InputTouch = 1 << 13,
    MetaState = Hovered | Focused | Pressed | Dragged | Selected | Disabled | Error | Navigated | Special1 |
                Special2,
    MetaInput = InputKeyboardMouse | InputGamepad | InputTouch,
    MetaAll = MetaState | MetaInput,
    ModNot = 1 << 14,
    ModAny = 1 << 15
  }

  public static class WidgetStateExtensions {
    public const StateFlag OperatorMask = StateFlag.ModNot | StateFlag.ModAny;

    public static StateFlag StateFlagFromPseudoFlags(this VisualElement element) {
      var flag = StateFlag.None;
      if (element.hasDisabledPseudoState) flag |= StateFlag.Disabled;
      if (element.hasHoverPseudoState) flag |= StateFlag.Hovered;
      if (element.hasCheckedPseudoState) flag |= StateFlag.Selected;
      if (element.hasActivePseudoState) flag |= StateFlag.Pressed;
      if (element.hasFocusPseudoState) flag |= StateFlag.Focused;
      return flag;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Hovered(this StateFlag state) => (state & StateFlag.Hovered) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Focused(this StateFlag state) => (state & StateFlag.Focused) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Pressed(this StateFlag state) => (state & StateFlag.Pressed) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Dragged(this StateFlag state) => (state & StateFlag.Dragged) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Selected(this StateFlag state) => (state & StateFlag.Selected) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Disabled(this StateFlag state) => (state & StateFlag.Disabled) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Enabled(this StateFlag state) => (state & StateFlag.Disabled) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Error(this StateFlag state) => (state & StateFlag.Error) != 0;

    public static bool Matches(this StateFlag actual, StateFlag query) {
      var subject = query & ~OperatorMask;
      var any = (query & StateFlag.ModAny) != 0;
      var not = (query & StateFlag.ModNot) != 0;
      var result = any ? (actual & subject) != 0 : (actual & subject) == subject;
      return not ? !result : result;
    }

    public static string ToStateString(this StateFlag state) {
      if (state == StateFlag.None) return "None";
      var flags = Enum.GetValues(typeof(StateFlag)).Cast<StateFlag>()
        .Where(flag => flag != StateFlag.None && state.HasFlag(flag))
        .Select(flag => flag.ToString());
      return string.Join(",", flags);
    }
  }

  public delegate void StateComposable(ref Composition cx, StateFlag state);

  public interface IWidgetStateHolder {
    StateFlag InputState { get; set; }
  }

  public static class WidgetStateHolderExtensions {
    public static void Enable<T>(this T state, StateFlag widgetState)
      where T : IWidgetStateHolder {
      state.InputState |= widgetState;
    }

    public static void Disable<T>(this T state, StateFlag widgetState)
      where T : IWidgetStateHolder {
      state.InputState &= ~widgetState;
    }

    public static void Toggle<T>(this T state, StateFlag widgetState)
      where T : IWidgetStateHolder {
      state.InputState ^= widgetState;
    }

    public static void Toggle<T>(this T state, StateFlag widgetState, bool toggle)
      where T : IWidgetStateHolder {
      if (toggle) state.Enable(widgetState);
      else state.Disable(widgetState);
    }

    public static void DisableEnable<T>(this T state, StateFlag disable, StateFlag enable)
      where T : IWidgetStateHolder {
      state.InputState = (state.InputState & ~disable) | enable;
    }
  }

  public class InputBoundaryComposable<T> : PropsBoundaryComposable<T>, IWidgetStateHolder where T : struct {
    public bool handleFocus;

    public StateFlag InputState { get; set; }

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
      Node.RegisterCallback<FocusInEvent>(OnFocusIn);
      Node.RegisterCallback<FocusOutEvent>(OnFocusOut);
      Node.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
    }

    protected override void OnDetach() {
      Node.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
      Node.UnregisterCallback<FocusInEvent>(OnFocusIn);
      Node.UnregisterCallback<FocusOutEvent>(OnFocusOut);
      Node.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
      base.OnDetach();
    }

    protected virtual void OnFocusOut(FocusOutEvent evt) {
      if (!handleFocus || (InputState & StateFlag.Focused) == 0) return;
      this.Disable(StateFlag.Focused);
      Node.MarkDirty();
    }

    protected virtual void OnFocusIn(FocusInEvent evt) {
      if (!handleFocus || (InputState & StateFlag.Focused) != 0) return;
      this.Enable(StateFlag.Focused);
      Node.MarkDirty();
      //if (WidgetStateController.LastNavigated) state.Enable(WidgetState.Navigated);
    }

    protected virtual void OnPointerLeave(PointerLeaveEvent evt) {
      if ((InputState & StateFlag.Hovered) == 0) return;
      this.Disable(StateFlag.Hovered);
      Node.MarkDirty();
    }

    protected virtual void OnPointerEnter(PointerEnterEvent evt) {
      if ((InputState & StateFlag.Hovered) != 0) return;
      this.Enable(StateFlag.Hovered);
      Node.MarkDirty();
    }

    protected virtual void OnNavigationMove(NavigationMoveEvent evt) {
      if ((InputState & StateFlag.Navigated) != 0) return;
      this.Enable(StateFlag.Navigated);
      Node.MarkDirty();
    }
  }

  public static class StateProperties {
    public static StateProperty<T> Never<T>() {
      return NeverStateProperty<T>.Instance;
    }

    public static StateProperty<T> All<T>(T constant) {
      return new AllStateProperty<T>(constant);
    }

    public static StateProperty<T> Func<T>(Func<StateFlag, T> resolver) {
      return new FuncStateProperty<T>(resolver);
    }
  }

  public abstract class StateProperty<T> : DiagnosticableBase {
    public abstract bool TryResolve(StateFlag state, out T value);

    public T ResolveOrDefault(StateFlag state, T defaultValue = default) {
      return TryResolve(state, out var value) ? value : defaultValue;
    }

    public abstract bool HasValueFor(StateFlag state);
    public abstract ref T GetValueRef(StateFlag state);

    public static implicit operator StateProperty<T>(T constant) {
      return StateProperties.All(constant);
    }
  }

  public class StatePropertyMap<T> : StateProperty<T> {
    private readonly List<Pair> _values = new();

    public T this[StateFlag state] {
      get => _values.Find(pair => pair.mask == state).value;
      set => _values.Add(new Pair(state, value));
    }

    public override bool TryResolve(StateFlag state, out T value) {
      value = default;
      foreach (var pair in _values) {
        if (!state.Matches(pair.mask)) continue;
        value = pair.value;
        return true;
      }

      return false;
    }

    public override bool HasValueFor(StateFlag state) {
      for (var index = 0; index < _values.Count; index++) {
        var pair = _values[index];
        if (state.Matches(pair.mask)) return true;
      }
      return false;
    }

    public override ref T GetValueRef(StateFlag state) {
      for (var index = 0; index < _values.Count; index++) {
        var pair = _values[index];
        if (state.Matches(pair.mask)) return ref pair.value;
      }
      throw new KeyNotFoundException($"No value found for state {state}");
    }

    protected bool Equals(StatePropertyMap<T> other) {
      return _values != null && other._values != null &&
             _values.SequenceEqual(other._values);
    }

    public override bool Equals(object obj) {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((StatePropertyMap<T>)obj);
    }

    public override int GetHashCode() {
      return _values != null ? _values.GetHashCode() : 0;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new IterableProperty<Pair>("values", _values, showName: false));
    }

    private class Pair : IEquatable<Pair> {
      public readonly StateFlag mask;
      public T value;

      public Pair(StateFlag mask, T value) {
        this.mask = mask;
        this.value = value;
      }

      public bool Equals(Pair other) {
        return other != null && mask == other.mask && EqualityComparer<T>.Default.Equals(value, other.value);
      }

      public override bool Equals(object obj) {
        return obj is Pair other && Equals(other);
      }

      public override int GetHashCode() {
        return HashCode.Combine((int)mask, value);
      }

      public override string ToString() {
        return $"{mask.ToStateString()} => {value}";
      }
    }
  }

  public class NeverStateProperty<T> : StateProperty<T> {
    public static readonly NeverStateProperty<T> Instance = new();
    private NeverStateProperty() { }

    public override bool TryResolve(StateFlag state, out T value) {
      value = default;
      return false;
    }

    public override bool HasValueFor(StateFlag state) {
      return false;
    }

    public override ref T GetValueRef(StateFlag state) {
      throw new KeyNotFoundException($"No value found for state {state}");
    }

    protected bool Equals(NeverStateProperty<T> other) {
      return true;
    }

    public override bool Equals(object obj) {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((NeverStateProperty<T>)obj);
    }

    public override int GetHashCode() {
      return 0;
    }

    public override string ToString() {
      return "Never";
    }
  }

  public class AllStateProperty<T> : StateProperty<T>, IEquatable<AllStateProperty<T>> {
    private T _constant;

    public AllStateProperty(T constant) {
      _constant = constant;
    }

    public bool Equals(AllStateProperty<T> other) {
      if (ReferenceEquals(null, other)) return false;
      return EqualityComparer<T>.Default.Equals(_constant, other._constant);
    }

    public override bool TryResolve(StateFlag state, out T value) {
      value = _constant;
      return true;
    }

    public override bool HasValueFor(StateFlag state) {
      return true;
    }

    public override ref T GetValueRef(StateFlag state) {
      return ref _constant;
    }

    public override bool Equals(object obj) {
      return obj is AllStateProperty<T> other && Equals(other);
    }

    public override int GetHashCode() {
      return EqualityComparer<T>.Default.GetHashCode(_constant);
    }

    public override string ToString() {
      return $"All({_constant})";
    }
  }

  public class FuncStateProperty<T> : StateProperty<T>,
    IEquatable<FuncStateProperty<T>> {
    private readonly Func<StateFlag, T> _resolver;
    private T _buffer;

    public FuncStateProperty(Func<StateFlag, T> resolver) {
      _resolver = resolver;
    }

    public bool Equals(FuncStateProperty<T> other) {
      if (ReferenceEquals(null, other)) return false;
      return Equals(_resolver, other._resolver);
    }

    public override bool TryResolve(StateFlag state, out T value) {
      value = _resolver(state);
      return true;
    }

    public override bool HasValueFor(StateFlag state) {
      return true;
    }

    public override ref T GetValueRef(StateFlag state) {
      _buffer = _resolver(state);
      return ref _buffer;
    }

    public override bool Equals(object obj) {
      return obj is FuncStateProperty<T> other && Equals(other);
    }

    public override int GetHashCode() {
      return _resolver != null ? _resolver.GetHashCode() : 0;
    }
  }
}