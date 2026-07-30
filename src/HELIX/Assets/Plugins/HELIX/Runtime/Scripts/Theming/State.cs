using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Compose;
using HELIX.Diagnostics;
using HELIX.Diagnostics.Properties;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable Unity.BurstLoadingManagedType
// ReSharper disable Unity.BurstAccessingManagedMethod

namespace HELIX.Theming {
  [Flags]
  public enum State : ushort {
    None = 0,
    Hovered = 1 << 0,
    Focused = 1 << 1,
    Selected = 1 << 2,
    Active = 1 << 3,
    Disabled = 1 << 4,
    Error = 1 << 5,
    Navigated = 1 << 6,
    Special1 = 1 << 7,
    Special2 = 1 << 8,
    InputKeyboardMouse = 1 << 10,
    InputGamepad = 1 << 11,
    InputTouch = 1 << 12,
    MetaState = Hovered | Focused | Active | Selected | Disabled | Error | Navigated | Special1 |
                Special2,
    MetaInput = InputKeyboardMouse | InputGamepad | InputTouch,
    MetaAll = MetaState | MetaInput,
    ModNot = 1 << 14,
    ModAny = 1 << 15
  }

  public static class WidgetStateExtensions {
    public const State OperatorMask = State.ModNot | State.ModAny;

    public static State StateFlagFromPseudoFlags(this VisualElement element) {
      var flag = State.None;
      if (element.hasDisabledPseudoState) flag |= State.Disabled;
      if (element.hasHoverPseudoState) flag |= State.Hovered;
      if (element.hasCheckedPseudoState) flag |= State.Selected;
      if (element.hasActivePseudoState) flag |= State.Active;
      if (element.hasFocusPseudoState) flag |= State.Focused;
      return flag;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Hovered(this State state) => (state & State.Hovered) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Focused(this State state) => (state & State.Focused) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Active(this State state) => (state & State.Active) != 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Selected(this State state) => (state & State.Selected) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Disabled(this State state) => (state & State.Disabled) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Enabled(this State state) => (state & State.Disabled) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Error(this State state) => (state & State.Error) != 0;

    public static bool Matches(this State actual, State query) {
      var subject = query & ~OperatorMask;
      var any = (query & State.ModAny) != 0;
      var not = (query & State.ModNot) != 0;
      var result = any ? (actual & subject) != 0 : (actual & subject) == subject;
      return not ? !result : result;
    }

    public static string ToStateString(this State state) {
      if (state == State.None) return "None";
      var flags = Enum.GetValues(typeof(State)).Cast<State>()
        .Where(flag => flag != State.None && state.HasFlag(flag))
        .Select(flag => flag.ToString());
      return string.Join(",", flags);
    }
  }

  public interface IWidgetStateHolder {
    ref State InputState { get; }
  }

  public static class WidgetStateHolderExtensions {
    public static void Enable<T>(this T state, State widgetState)
      where T : IWidgetStateHolder {
      state.InputState |= widgetState;
    }

    public static void Disable<T>(this T state, State widgetState)
      where T : IWidgetStateHolder {
      state.InputState &= ~widgetState;
    }

    public static void Toggle<T>(this T state, State widgetState)
      where T : IWidgetStateHolder {
      state.InputState ^= widgetState;
    }

    public static void Toggle<T>(this T state, State widgetState, bool toggle)
      where T : IWidgetStateHolder {
      if (toggle) state.Enable(widgetState);
      else state.Disable(widgetState);
    }

    public static void DisableEnable<T>(this T state, State disable, State enable)
      where T : IWidgetStateHolder {
      state.InputState = (state.InputState & ~disable) | enable;
    }
  }

  public class InputBoundaryComposable<T> : PropsBoundaryComposable<T>, IWidgetStateHolder where T : struct {
    public bool handleFocus;
    private State _inputState;

    public ref State InputState => ref _inputState;

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
      if (!handleFocus || (InputState & State.Focused) == 0) return;
      this.Disable(State.Focused);
      Node.MarkDirty();
    }

    protected virtual void OnFocusIn(FocusInEvent evt) {
      if (!handleFocus || (InputState & State.Focused) != 0) return;
      this.Enable(State.Focused);
      Node.MarkDirty();
      //if (WidgetStateController.LastNavigated) state.Enable(WidgetState.Navigated);
    }

    protected virtual void OnPointerLeave(PointerLeaveEvent evt) {
      if ((InputState & State.Hovered) == 0) return;
      this.Disable(State.Hovered);
      Node.MarkDirty();
    }

    protected virtual void OnPointerEnter(PointerEnterEvent evt) {
      if ((InputState & State.Hovered) != 0) return;
      this.Enable(State.Hovered);
      Node.MarkDirty();
    }

    protected virtual void OnNavigationMove(NavigationMoveEvent evt) {
      if ((InputState & State.Navigated) != 0) return;
      this.Enable(State.Navigated);
      Node.MarkDirty();
    }
  }

  public static class States {
    public static readonly State[] CommonFocusableSelectable = Matrix(State.Selected, State.Focused);
    public static readonly State[] CommonFocusable = Matrix(State.Focused);
    public static readonly State[] CommonSelectable = Matrix(State.Selected);
    public static readonly State[] Common = Matrix();

    public static State[] Matrix(
      IReadOnlyList<State> prefix,
      IReadOnlyList<State> states,
      IReadOnlyList<State> flags
    ) {
      states ??= new[] { State.Active, State.Hovered, State.None };
      flags ??= Array.Empty<State>();

      var variants = Variants(flags);
      var values = new List<State>();
      if (prefix == null) {
        values.Add(State.Error);
        if (flags.Contains(State.Selected)) values.Add(State.Disabled | State.Selected);
        values.Add(State.Disabled);
      } else {
        values.AddRange(prefix);
      }
      values.AddRange(Explode(states, variants));
      return values.ToArray();
    }

    public static State[] Matrix(params State[] flags) => Matrix(null, null, flags);

    public static State[] Variants(IReadOnlyList<State> ordered, bool none = true) {
      var results = new List<State>();
      for (var n = ordered.Count; n >= 0; n--) {
        var flag = State.None;
        for (var i = 0; i < n; i++) {
          flag |= ordered[i];
        }
        if (flag == State.None && !none) continue;
        results.Add(flag);
      }
      return results.ToArray();
    }

    public static State[] Explode(IReadOnlyList<State> states, params State[] variants) {
      var results = new List<State>();
      foreach (var branch in variants) {
        results.AddRange(states.Select(s => s | branch));
      }
      return results.ToArray();
    }

    public static int FirstMatch(IReadOnlyList<State> flags, State query) {
      for (var i = 0; i < flags.Count; i++) {
        if (flags[i].Matches(query)) return i;
      }
      return -1;
    }
  }

  public static class StateProperties {
    public static StateProperty<T> PrecomputedBranch<T>(
      StateProperty<T> property,
      IReadOnlyList<State> states,
      IReadOnlyList<State> branches
    ) {
      var values = new List<HXOptional<T>[]>(branches.Count);
      for (var i = 0; i < branches.Count; i++) {
        var branch = branches[i];
        var elements = new HXOptional<T>[states.Count];
        for (var j = 0; j < states.Count; j++) {
          var state = states[j];
          var hasValue = property.TryResolve(state | branch, out var value);
          elements[j] = new HXOptional<T>(value, hasValue);
        }
        values.Add(elements);
      }
      return Func(state => {
          var branch = States.FirstMatch(branches, state);
          var element = States.FirstMatch(states, state);
          if (branch == -1 || element == -1) return HXOptional<T>.None;
          return values[branch][element];
        }
      );
    }

    public static StateProperty<T> DynamicBranch<T>(
      IReadOnlyList<StateProperty<T>> properties,
      IReadOnlyList<State> branches,
      bool unsetBranchCondition = true
    ) {
      return Func(state => {
          var branch = States.FirstMatch(branches, state);
          if (branch == -1) return HXOptional<T>.None;
          var lookup = state;
          if (unsetBranchCondition) lookup &= ~branches[branch];
          var hasValue = properties[branch].TryResolve(lookup, out var value);
          return new HXOptional<T>(value, hasValue);
        }
      );
    }

    public static StateProperty<T> Never<T>() {
      return NeverStateProperty<T>.Instance;
    }

    public static StateProperty<T> All<T>(T constant) {
      return new AllStateProperty<T>(constant);
    }

    public static StateProperty<T> Func<T>(Func<State, T> resolver) {
      return new FuncStateProperty<T>(resolver);
    }

    public static StateProperty<T> Func<T>(Func<State, HXOptional<T>> resolver) {
      return new SparseFuncStateProperty<T>(resolver);
    }

    public static StateProperty<TValue> Derive<TOther, TValue>(
      this StateProperty<TOther> property,
      Func<TOther, bool, TValue> converter,
      params State[] states
    ) {
      var map = new StatePropertyMap<TValue>();
      for (var i = 0; i < states.Length; i++) {
        var state = states[i];
        var hasValue = property.TryResolve(state, out var value);
        map[state] = converter(value, hasValue);
      }
      return map;
    }

    public static StateProperty<TValue> Derive<TOther, TValue>(
      this StateProperty<TOther> property,
      StatePropertyMap<Func<TOther, bool, TValue>> functions
    ) {
      var map = new StatePropertyMap<TValue>();
      for (var i = 0; i < functions.values.Count; i++) {
        var pair = functions.values[i];
        var hasValue = property.TryResolve(pair.mask, out var value);
        map[pair.mask] = pair.value(value, hasValue);
      }
      return map;
    }

    public static StateProperty<TValue> Derive<TValue>(this StateProperty<TValue> property, params State[] states) {
      var map = new StatePropertyMap<TValue>();
      for (var i = 0; i < states.Length; i++) {
        var state = states[i];
        map[state] = property.ResolveOrDefault(state);
      }
      return map;
    }

    public static StateProperty<TValue> Zip<TLeft, TRight, TValue>(
      StateProperty<TLeft> left,
      StateProperty<TRight> right,
      Func<State, TLeft, TRight, TValue> zip,
      params State[] states
    ) {
      var map = new StatePropertyMap<TValue>();
      for (var i = 0; i < states.Length; i++) {
        var state = states[i];
        var leftValue = left.ResolveOrDefault(state);
        var rightValue = right.ResolveOrDefault(state);
        map[state] = zip(state, leftValue, rightValue);
      }
      return map;
    }

    public static StateProperty<TValue> Zip<TValue>(State[] states, TValue[] values) {
      var map = new StatePropertyMap<TValue>();
      for (var i = 0; i < states.Length; i++) {
        map[states[i]] = values[i];
      }
      return map;
    }

    public static Composable<State> ToComposable(this StateProperty<Composable> property) {
      return (ref Composition cx, State state) => {
        if (property.TryResolve(state, out var composable)) {
          composable(ref cx);
        }
      };
    }
  }

  public abstract class StateProperty<T> : DiagnosticableBase {
    public abstract bool TryResolve(State state, out T value);

    public T ResolveOrDefault(State state, T defaultValue = default) {
      return TryResolve(state, out var value) ? value : defaultValue;
    }

    public abstract bool HasValueFor(State state);
    public abstract ref T GetValueRef(State state);

    public T this[State state] => ResolveOrDefault(state);

    public static implicit operator StateProperty<T>(T constant) {
      return StateProperties.All(constant);
    }
  }

  public class StatePropertyMap<T> : StateProperty<T> {
    internal readonly List<Pair> values = new();

    public new T this[State state] {
      get => base[state];
      set => values.Add(new Pair(state, value));
    }

    public override bool TryResolve(State state, out T value) {
      value = default;
      foreach (var pair in values) {
        if (!state.Matches(pair.mask)) continue;
        value = pair.value;
        return true;
      }

      return false;
    }

    public override bool HasValueFor(State state) {
      for (var index = 0; index < values.Count; index++) {
        var pair = values[index];
        if (state.Matches(pair.mask)) return true;
      }
      return false;
    }

    public override ref T GetValueRef(State state) {
      for (var index = 0; index < values.Count; index++) {
        var pair = values[index];
        if (state.Matches(pair.mask)) return ref pair.value;
      }
      throw new KeyNotFoundException($"No value found for state {state}");
    }

    protected bool Equals(StatePropertyMap<T> other) {
      return values != null && other.values != null &&
             values.SequenceEqual(other.values);
    }

    public override bool Equals(object obj) {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((StatePropertyMap<T>)obj);
    }

    public override int GetHashCode() {
      return values != null ? values.GetHashCode() : 0;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new IterableProperty<Pair>("values", values, showName: false));
    }

    internal class Pair : IEquatable<Pair> {
      public readonly State mask;
      public T value;

      public Pair(State mask, T value) {
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

    public override bool TryResolve(State state, out T value) {
      value = default;
      return false;
    }

    public override bool HasValueFor(State state) {
      return false;
    }

    public override ref T GetValueRef(State state) {
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

    public override bool TryResolve(State state, out T value) {
      value = _constant;
      return true;
    }

    public override bool HasValueFor(State state) {
      return true;
    }

    public override ref T GetValueRef(State state) {
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
    private readonly Func<State, T> _resolver;
    private T _buffer;

    public FuncStateProperty(Func<State, T> resolver) {
      _resolver = resolver;
    }

    public bool Equals(FuncStateProperty<T> other) {
      if (ReferenceEquals(null, other)) return false;
      return Equals(_resolver, other._resolver);
    }

    public override bool TryResolve(State state, out T value) {
      value = _resolver(state);
      return true;
    }

    public override bool HasValueFor(State state) {
      return true;
    }

    public override ref T GetValueRef(State state) {
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

  public class SparseFuncStateProperty<T> : StateProperty<T> {
    private readonly Func<State, HXOptional<T>> _resolver;
    private T _buffer;

    public SparseFuncStateProperty(Func<State, HXOptional<T>> resolver) {
      _resolver = resolver;
    }

    public bool Equals(SparseFuncStateProperty<T> other) {
      if (ReferenceEquals(null, other)) return false;
      return Equals(_resolver, other._resolver);
    }

    public override bool TryResolve(State state, out T value) {
      var optional = _resolver(state);
      value = optional.value;
      return optional.hasValue;
    }

    public override bool HasValueFor(State state) {
      return true;
    }

    public override ref T GetValueRef(State state) {
      _buffer = _resolver(state).value;
      return ref _buffer;
    }

    public override bool Equals(object obj) {
      return obj is SparseFuncStateProperty<T> other && Equals(other);
    }

    public override int GetHashCode() {
      return _resolver != null ? _resolver.GetHashCode() : 0;
    }
  }

  public abstract class InputClickableComposable<T> : InputBoundaryComposable<T> where T : struct {
    private int _activePointerId = -1;

    protected bool Active { get; private set; }
    protected Vector2 LastMousePosition { get; private set; }

    protected InputClickableComposable() {
      handleFocus = true;
    }

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<PointerDownEvent>(OnPointerDown);
      Node.RegisterCallback<PointerMoveEvent>(OnPointerMove);
      Node.RegisterCallback<PointerUpEvent>(OnPointerUp);
      Node.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
      Node.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      Node.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
    }

    protected override void OnDetach() {
      base.OnDetach();
      Node.UnregisterCallback<PointerDownEvent>(OnPointerDown);
      Node.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
      Node.UnregisterCallback<PointerUpEvent>(OnPointerUp);
      Node.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
      Node.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      Node.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
    }

    protected virtual void OnClick(EventBase evt) { }

    protected virtual void OnNavigationSubmit(NavigationSubmitEvent evt) {
      if ((InputState & State.Disabled) != 0) return;
      OnClick(evt);
      evt.StopPropagation();
    }

    protected virtual void OnPointerDown(PointerDownEvent evt) {
      if (Active || evt.button != (int)MouseButton.LeftMouse) return;

      Active = true;
      _activePointerId = evt.pointerId;
      LastMousePosition = evt.localPosition;

      Node.CapturePointer(evt.pointerId);
      this.Enable(State.Active);
      Node.MarkDirty();

      evt.StopImmediatePropagation();
    }

    protected virtual void OnPointerMove(PointerMoveEvent evt) {
      if (!Active) return;

      LastMousePosition = evt.localPosition;
      var pressed = Node.worldBound.Contains(evt.position);
      if (((InputState & State.Active) != 0) != pressed) {
        this.Toggle(State.Active, pressed);
        Node.MarkDirty();
      }

      evt.StopPropagation();
    }

    protected virtual void OnPointerUp(PointerUpEvent evt) {
      if (!Active || evt.pointerId != _activePointerId) return;

      var clicked = Node.worldBound.Contains(evt.position);

      Active = false;
      _activePointerId = -1;
      LastMousePosition = evt.localPosition;

      Node.ReleasePointer(evt.pointerId);
      this.Disable(State.Active);
      Node.MarkDirty();

      if (clicked) {
        OnClick(evt);
      }

      evt.StopPropagation();
    }

    protected virtual void OnPointerCancel(PointerCancelEvent evt) {
      if (!Active || evt.pointerId != _activePointerId) return;

      Cancel(evt, evt.pointerId);
    }

    protected virtual void OnPointerCaptureOut(PointerCaptureOutEvent evt) {
      if (!Active) return;

      Cancel(evt, evt.pointerId);
    }

    protected virtual void Cancel(EventBase evt, int pointerId) {
      Active = false;
      _activePointerId = -1;

      Node.ReleasePointer(pointerId);
      this.Disable(State.Active);
      Node.MarkDirty();

      evt.StopPropagation();
    }
  }
}