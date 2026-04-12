using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;

// ReSharper disable Unity.BurstLoadingManagedType
// ReSharper disable Unity.BurstAccessingManagedMethod

namespace HELIX.Widgets {
    [Flags]
    public enum WidgetState : ushort {
        None = 0,
        
        Hovered = 1 << 0,
        Focused = 1 << 1,
        Pressed = 1 << 2,
        Dragged = 1 << 3,
        Selected = 1 << 4,
        Disabled = 1 << 5,
        Error = 1 << 6,
        
        InputMouse = 1 << 9,
        InputKeyboard = 1 << 10,
        InputGamepad = 1 << 11,
        InputTouch = 1 << 12,
        
        MetaInputPointer = InputMouse | InputTouch,
        MetaInputButton = InputKeyboard | InputGamepad,
        
        ModNot = 1 << 14,
        ModAny = 1 << 15
    }

    public static class WidgetStateExtensions {
        public const WidgetState OperatorMask = WidgetState.ModNot | WidgetState.ModAny;
        
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

        public static bool Matches(this WidgetState actual, WidgetState query) {
            var subject = query & ~OperatorMask;
            var any = (query & WidgetState.ModAny) != 0;
            var not = (query & WidgetState.ModNot) != 0;

            var result = any
                ? (actual & subject) != 0
                : (actual & subject) == subject;

            return not ? !result : result;
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
        public static IWidgetStateProperty<T> Never<T>() {
            return NeverWidgetStateProperty<T>.Instance;
        }

        public static IWidgetStateProperty<T> All<T>(T constant) {
            return new AllWidgetStateProperty<T>(constant);
        }

        public static IWidgetStateProperty<T> Func<T>(Func<WidgetState, T> resolver) {
            return new FuncWidgetStateProperty<T>(resolver);
        }
    }

    public interface IWidgetStateProperty<T> {
        bool TryResolve(WidgetState state, out T value);

        T ResolveOrDefault(WidgetState state, T defaultValue = default) {
            return TryResolve(state, out var value) ? value : defaultValue;
        }
    }

    public class WidgetStatePropertyMap<T> : DiagnosticableBase, IWidgetStateProperty<T> {
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

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new IterableProperty<Pair>("values", _values, showName: false));
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

            public override string ToString() {
                return $"{mask.ToStateString()} => {value}";
            }
        }
    }

    public class NeverWidgetStateProperty<T> : IWidgetStateProperty<T> {
        public static readonly NeverWidgetStateProperty<T> Instance = new();
        private NeverWidgetStateProperty() { }

        public bool TryResolve(WidgetState state, out T value) {
            value = default;
            return false;
        }

        protected bool Equals(NeverWidgetStateProperty<T> other) {
            return true;
        }

        public override bool Equals(object obj)
        {
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

        public override string ToString() {
            return $"All({_constant})";
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
            return _resolver != null ? _resolver.GetHashCode() : 0;
        }
    }
}