using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Diagnostics;
using HELIX.Diagnostics.Properties;
using HELIX.NW;
using UnityEngine;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Hovered(this StateFlag state) {
      return state.HasFlag(StateFlag.Hovered);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Focused(this StateFlag state) {
      return state.HasFlag(StateFlag.Focused);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Pressed(this StateFlag state) {
      return state.HasFlag(StateFlag.Pressed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Dragged(this StateFlag state) {
      return state.HasFlag(StateFlag.Dragged);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Selected(this StateFlag state) {
      return state.HasFlag(StateFlag.Selected);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Disabled(this StateFlag state) {
      return state.HasFlag(StateFlag.Disabled);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Enabled(this StateFlag state) {
      return !state.HasFlag(StateFlag.Disabled);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Error(this StateFlag state) {
      return state.HasFlag(StateFlag.Error);
    }

    public static bool Matches(this StateFlag actual, StateFlag query) {
      var subject = query & ~OperatorMask;
      var any = (query & StateFlag.ModAny) != 0;
      var not = (query & StateFlag.ModNot) != 0;

      var result = any
        ? (actual & subject) != 0
        : (actual & subject) == subject;

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

  public delegate void WidgetStateComposable(ref Composition cx, StateFlag state);

  public interface IWidgetStateHolder {
    StateFlag InputState { get; set; }
  }

  public interface IInlineComposableState<T> {
    InlineComposable<T> Composable { get; set; }
    T CompositionArgument { get; }
  }

  public interface IInlineComposableState {
    InlineComposable Composable { get; set; }
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

  public class InputStateBase<T> : PropsNodeStateAttachmentBase<T>, IWidgetStateHolder where T : struct {
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
      if (!handleFocus) return;
      this.Disable(StateFlag.Focused);
      Node.MarkDirty();
    }

    protected virtual void OnFocusIn(FocusInEvent evt) {
      if (!handleFocus) return;
      this.Enable(StateFlag.Focused);
      Node.MarkDirty();
      //if (WidgetStateController.LastNavigated) state.Enable(WidgetState.Navigated);
    }

    protected virtual void OnPointerLeave(PointerLeaveEvent evt) {
      this.Disable(StateFlag.Hovered);
      Node.MarkDirty();
    }

    protected virtual void OnPointerEnter(PointerEnterEvent evt) {
      this.Enable(StateFlag.Hovered);
      Node.MarkDirty();
    }

    protected virtual void OnNavigationMove(NavigationMoveEvent evt) {
      this.Enable(StateFlag.Navigated);
      Node.MarkDirty();
    }
  }

  public static class WidgetStateProperties {
    public static WidgetStateProperty<T> Never<T>() {
      return NeverWidgetStateProperty<T>.Instance;
    }

    public static WidgetStateProperty<T> All<T>(T constant) {
      return new AllWidgetStateProperty<T>(constant);
    }

    public static WidgetStateProperty<T> Func<T>(Func<StateFlag, T> resolver) {
      return new FuncWidgetStateProperty<T>(resolver);
    }
  }

  public abstract class WidgetStateProperty<T> : DiagnosticableBase {
    public abstract bool TryResolve(StateFlag state, out T value);

    public T ResolveOrDefault(StateFlag state, T defaultValue = default) {
      return TryResolve(state, out var value) ? value : defaultValue;
    }

    public static implicit operator WidgetStateProperty<T>(T constant) {
      return WidgetStateProperties.All(constant);
    }
  }

  public class WidgetStatePropertyMap<T> : WidgetStateProperty<T> {
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

    protected bool Equals(WidgetStatePropertyMap<T> other) {
      return _values != null && other._values != null &&
             _values.SequenceEqual(other._values);
    }

    public override bool Equals(object obj) {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((WidgetStatePropertyMap<T>)obj);
    }

    public override int GetHashCode() {
      return _values != null ? _values.GetHashCode() : 0;
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new IterableProperty<Pair>("values", _values, showName: false));
    }

    private readonly struct Pair : IEquatable<Pair> {
      public readonly StateFlag mask;
      public readonly T value;

      public Pair(StateFlag mask, T value) {
        this.mask = mask;
        this.value = value;
      }

      public bool Equals(Pair other) {
        return mask == other.mask && EqualityComparer<T>.Default.Equals(value, other.value);
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

  public class NeverWidgetStateProperty<T> : WidgetStateProperty<T> {
    public static readonly NeverWidgetStateProperty<T> Instance = new();
    private NeverWidgetStateProperty() { }

    public override bool TryResolve(StateFlag state, out T value) {
      value = default;
      return false;
    }

    protected bool Equals(NeverWidgetStateProperty<T> other) {
      return true;
    }

    public override bool Equals(object obj) {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((NeverWidgetStateProperty<T>)obj);
    }

    public override int GetHashCode() {
      return 0;
    }

    public override string ToString() {
      return "Never";
    }
  }

  public class AllWidgetStateProperty<T> : WidgetStateProperty<T>, IEquatable<AllWidgetStateProperty<T>> {
    private readonly T _constant;

    public AllWidgetStateProperty(T constant) {
      _constant = constant;
    }

    public bool Equals(AllWidgetStateProperty<T> other) {
      if (ReferenceEquals(null, other)) return false;
      return EqualityComparer<T>.Default.Equals(_constant, other._constant);
    }

    public override bool TryResolve(StateFlag state, out T value) {
      value = _constant;
      return true;
    }

    public override bool Equals(object obj) {
      return obj is AllWidgetStateProperty<T> other && Equals(other);
    }

    public override int GetHashCode() {
      return EqualityComparer<T>.Default.GetHashCode(_constant);
    }

    public override string ToString() {
      return $"All({_constant})";
    }
  }

  public class FuncWidgetStateProperty<T> : WidgetStateProperty<T>,
    IEquatable<FuncWidgetStateProperty<T>> {
    private readonly Func<StateFlag, T> _resolver;

    public FuncWidgetStateProperty(Func<StateFlag, T> resolver) {
      _resolver = resolver;
    }

    public bool Equals(FuncWidgetStateProperty<T> other) {
      if (ReferenceEquals(null, other)) return false;
      return Equals(_resolver, other._resolver);
    }

    public override bool TryResolve(StateFlag state, out T value) {
      value = _resolver(state);
      return true;
    }

    public override bool Equals(object obj) {
      return obj is FuncWidgetStateProperty<T> other && Equals(other);
    }

    public override int GetHashCode() {
      return _resolver != null ? _resolver.GetHashCode() : 0;
    }
  }
}