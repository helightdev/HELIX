using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable Unity.BurstLoadingManagedType
// ReSharper disable Unity.BurstAccessingManagedMethod

namespace HELIX.Widgets {
    [Flags]
    public enum WidgetState : byte {
        None = 0,
        Hovered = 1 << 0,
        Focused = 1 << 1,
        Pressed = 1 << 2,
        Dragged = 1 << 3,
        Selected = 1 << 4,
        Disabled = 1 << 5,
        Error = 1 << 6,
        Negative = 1 << 7
    }

    public delegate Widget WidgetStateBuilder(BuildContext context, WidgetState state);

    public static class WidgetStateExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Hovered(this WidgetState state) {
            return state.HasFlag(WidgetState.Hovered);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Focused(this WidgetState state) {
            return state.HasFlag(WidgetState.Focused);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Pressed(this WidgetState state) {
            return state.HasFlag(WidgetState.Pressed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Dragged(this WidgetState state) {
            return state.HasFlag(WidgetState.Dragged);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Selected(this WidgetState state) {
            return state.HasFlag(WidgetState.Selected);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Disabled(this WidgetState state) {
            return state.HasFlag(WidgetState.Disabled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Enabled(this WidgetState state) {
            return !state.HasFlag(WidgetState.Disabled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Error(this WidgetState state) {
            return state.HasFlag(WidgetState.Error);
        }

        public static bool Matches(this WidgetState actual, WidgetState mask) {
            if (!mask.HasFlag(WidgetState.Negative)) return (actual & mask) == mask;
            var withoutMod = mask & ~WidgetState.Negative;
            return (actual & withoutMod) == 0;
        }

        public static string ToStateString(this WidgetState state) {
            if (state == WidgetState.None) return "None";
            var flags = Enum.GetValues(typeof(WidgetState)).Cast<WidgetState>()
                .Where(flag => flag != WidgetState.None && state.HasFlag(flag))
                .Select(flag => flag.ToString());
            return string.Join(",", flags);
        }
    }

    public static class WidgetStateProperties {
        public static IWidgetStateProperty<T> Never<T>() => NeverWidgetStateProperty<T>.Instance;
        public static IWidgetStateProperty<T> All<T>(T constant) => new AllWidgetStateProperty<T>(constant);
        public static IWidgetStateProperty<T> Func<T>(Func<WidgetState, T> resolver) => new FuncWidgetStateProperty<T>(resolver);

    }

    public interface IWidgetStateProperty<T> {
        bool TryResolve(WidgetState state, out T value);

        T ResolveOrDefault(WidgetState state, T defaultValue = default) {
            return TryResolve(state, out var value) ? value : defaultValue;
        }
    }

    public class WidgetStatePropertyMap<T> : IWidgetStateProperty<T> {
        private readonly List<Pair> _values = new();

        public T this[WidgetState state] {
            get => _values.Find(pair => pair.mask == state).value;
            set => _values.Add(new Pair(state, value));
        }

        public bool TryResolve(WidgetState state, out T value) {
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

        private readonly struct Pair : IEquatable<Pair> {
            public readonly WidgetState mask;
            public readonly T value;

            public Pair(WidgetState mask, T value) {
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
        }
    }

    public class NeverWidgetStateProperty<T> : IWidgetStateProperty<T> {
        private NeverWidgetStateProperty() { }

        public bool TryResolve(WidgetState state, out T value) {
            value = default;
            return false;
        }

        public static readonly NeverWidgetStateProperty<T> Instance = new();
    }

    public readonly struct AllWidgetStateProperty<T> : IWidgetStateProperty<T>, IEquatable<AllWidgetStateProperty<T>> {
        private readonly T _constant;

        public AllWidgetStateProperty(T constant) {
            _constant = constant;
        }

        public bool TryResolve(WidgetState state, out T value) {
            value = _constant;
            return true;
        }

        public bool Equals(AllWidgetStateProperty<T> other) {
            return EqualityComparer<T>.Default.Equals(_constant, other._constant);
        }

        public override bool Equals(object obj) {
            return obj is AllWidgetStateProperty<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return EqualityComparer<T>.Default.GetHashCode(_constant);
        }
    }

    public readonly struct FuncWidgetStateProperty<T> : IWidgetStateProperty<T>,
        IEquatable<FuncWidgetStateProperty<T>> {
        private readonly Func<WidgetState, T> _resolver;

        public FuncWidgetStateProperty(Func<WidgetState, T> resolver) {
            _resolver = resolver;
        }

        public bool TryResolve(WidgetState state, out T value) {
            value = _resolver(state);
            return true;
        }

        public bool Equals(FuncWidgetStateProperty<T> other) {
            return Equals(_resolver, other._resolver);
        }

        public override bool Equals(object obj) {
            return obj is FuncWidgetStateProperty<T> other && Equals(other);
        }

        public override int GetHashCode() {
            return (_resolver != null ? _resolver.GetHashCode() : 0);
        }
    }
}