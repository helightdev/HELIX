using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

// ReSharper disable Unity.BurstLoadingManagedType
// ReSharper disable Unity.BurstAccessingManagedMethod

namespace HELIX.Compose {
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

  public static class States {
    public static readonly State[] CommonFocusableSelectable = Matrix(State.Selected, State.Focused);
    public static readonly State[] CommonFocusable = Matrix(State.Focused);
    public static readonly State[] CommonSelectable = Matrix(State.Selected);
    public static readonly State[] Common = Matrix();
    public static readonly State[] EnabledDisabled = { State.Disabled, State.None };
    public static readonly State[] EnabledDisabledError = { State.Error, State.Disabled, State.None };

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
      } else values.AddRange(prefix);
      values.AddRange(Explode(states, variants));
      return values.ToArray();
    }

    public static State[] Matrix(params State[] flags) {
      return Matrix(null, null, flags);
    }

    public static State[] Variants(IReadOnlyList<State> ordered, bool none = true) {
      var results = new List<State>();
      for (var n = ordered.Count; n >= 0; n--) {
        var flag = State.None;
        for (var i = 0; i < n; i++) flag |= ordered[i];
        if (flag == State.None && !none) continue;
        results.Add(flag);
      }
      return results.ToArray();
    }

    public static State[] Explode(IReadOnlyList<State> states, params State[] variants) {
      var results = new List<State>();
      foreach (var branch in variants) results.AddRange(states.Select(s => s | branch));
      return results.ToArray();
    }

    public static int FirstMatch(IReadOnlyList<State> flags, State query) {
      for (var i = 0; i < flags.Count; i++)
        if (flags[i].Matches(query))
          return i;
      return -1;
    }
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
    public static bool Hovered(this State state) {
      return (state & State.Hovered) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Focused(this State state) {
      return (state & State.Focused) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Active(this State state) {
      return (state & State.Active) != 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Selected(this State state) {
      return (state & State.Selected) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Disabled(this State state) {
      return (state & State.Disabled) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Enabled(this State state) {
      return (state & State.Disabled) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Error(this State state) {
      return (state & State.Error) != 0;
    }

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

  public interface IStateHolder : IDirty {
    ref State InputState { get; }
    void OnStateChanged(State oldState, State newState) { }
  }

  public delegate void StateChangedHandler(State oldState, State newState);

  public static class WidgetStateHolderExtensions {
    private static bool DoChange<T>(this T state, State old, State next)
    where T : IStateHolder {
      if (old == next) return false;
      state.InputState = next;
      state.OnStateChanged(old, next);
      return true;
    }

    public static bool Enable<T>(this T state, State widgetState)
    where T : IStateHolder {
      var old = state.InputState;
      var next = old | widgetState;
      return state.DoChange(old, next);
    }

    public static bool Disable<T>(this T state, State widgetState)
    where T : IStateHolder {
      var old = state.InputState;
      var next = old & ~widgetState;
      return state.DoChange(old, next);
    }

    public static bool Toggle<T>(this T state, State widgetState)
    where T : IStateHolder {
      var old = state.InputState;
      var next = old ^ widgetState;
      state.InputState = next;
      state.OnStateChanged(old, next);
      return true;
    }

    public static bool Toggle<T>(this T state, State widgetState, bool toggle)
    where T : IStateHolder {
      return toggle ? state.Enable(widgetState) : state.Disable(widgetState);
    }

    public static bool DisableEnable<T>(this T state, State disable, State enable)
    where T : IStateHolder {
      var old = state.InputState;
      var next = (old & ~disable) | enable;
      return state.DoChange(old, next);
    }
  }
}