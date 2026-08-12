using System;
using System.Collections.Generic;
using System.Linq;
using HELIX.Compose;
using HELIX.Diagnostics;
using HELIX.Diagnostics.Properties;

namespace HELIX.Theming {
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

    public static StateProperty<T> Const<T>(T constant) {
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
      return StateProperties.Const(constant);
    }
  }

  public class StatePropertyMap<T> : StateProperty<T> {
    internal readonly List<Pair> values = new();

    public new T this[State state] { get => base[state]; set => values.Add(new Pair(state, value)); }

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

    protected bool Equals(StatePropertyMap<T> other) => values != null && other.values != null &&
                                                        values.SequenceEqual(other.values);

    public override bool Equals(object obj) {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((StatePropertyMap<T>)obj);
    }

    public override int GetHashCode() => values != null ? values.GetHashCode() : 0;

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

      public override bool Equals(object obj) => obj is Pair other && Equals(other);

      public override int GetHashCode() => HashCode.Combine((int)mask, value);

      public override string ToString() => $"{mask.ToStateString()} => {value}";
    }
  }

  public class NeverStateProperty<T> : StateProperty<T> {
    public static readonly NeverStateProperty<T> Instance = new();
    private NeverStateProperty() { }

    public override bool TryResolve(State state, out T value) {
      value = default;
      return false;
    }

    public override bool HasValueFor(State state) => false;

    public override ref T GetValueRef(State state) =>
      throw new KeyNotFoundException($"No value found for state {state}");

    public bool Equals(NeverStateProperty<T> other) => true;

    public override bool Equals(object obj) {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((NeverStateProperty<T>)obj);
    }

    public override int GetHashCode() => 0;
    public override string ToString() => "Never";
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

    public override bool HasValueFor(State state) => true;

    public override ref T GetValueRef(State state) => ref _constant;

    public override bool Equals(object obj) => obj is AllStateProperty<T> other && Equals(other);

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(_constant);

    public override string ToString() => $"All({_constant})";
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

    public override bool HasValueFor(State state) => true;

    public override ref T GetValueRef(State state) {
      _buffer = _resolver(state);
      return ref _buffer;
    }

    public override bool Equals(object obj) => obj is FuncStateProperty<T> other && Equals(other);

    public override int GetHashCode() => _resolver != null ? _resolver.GetHashCode() : 0;
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

    public override bool HasValueFor(State state) => true;

    public override ref T GetValueRef(State state) {
      _buffer = _resolver(state).value;
      return ref _buffer;
    }

    public override bool Equals(object obj) => obj is SparseFuncStateProperty<T> other && Equals(other);
    public override int GetHashCode() => _resolver != null ? _resolver.GetHashCode() : 0;
  }
}