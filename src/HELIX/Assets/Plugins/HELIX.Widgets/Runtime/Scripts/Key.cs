using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    [SuppressMessage("ReSharper", "Unity.BurstAccessingManagedMethod")]
    [SuppressMessage("ReSharper", "Unity.BurstLoadingManagedType")]
    [SuppressMessage("ReSharper", "Unity.BurstFunctionSignatureContainsManagedTypes")]
    public readonly struct Key : IEquatable<Key> {
        public readonly ulong value;
        public readonly BaseKey details;

        public static readonly Key None = new Key(0);

        public Key(ulong value) {
            this.value = value;
            details = null;
        }

        public Key(ulong value, BaseKey detailsKey) {
            this.value = value;
            details = detailsKey;
        }

        public bool IsNone => value == 0 && details == null;

        public bool Equals(Key other) =>
            value == other.value && EqualityComparer<BaseKey>.Default.Equals(details, other.details);

        public override bool Equals(object obj) => obj is Key other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(value, details);

        public static bool operator ==(Key a, Key b) =>
            a.value == b.value && EqualityComparer<BaseKey>.Default.Equals(a.details, b.details);

        public static bool operator !=(Key a, Key b) =>
            a.value != b.value || !EqualityComparer<BaseKey>.Default.Equals(a.details, b.details);

        public override string ToString() => IsNone ? "None" : details != null ? $"Key({details})" : $"Key({value})";

        public static implicit operator Key(ulong value) => new(value);
        public static implicit operator Key(uint value) => new(value);
        public static implicit operator Key(int value) => new(unchecked((uint)value));
        public static implicit operator Key(string value) => new(HashString(value));

        public static implicit operator Key(BaseKey detailsKey) =>
            new((ulong)detailsKey.GetType().GetHashCode(), detailsKey);

        internal static ulong HashString(string value) {
            var source = Encoding.UTF8.GetBytes(value);
            return XxHash3.HashToUInt64(source);
        }
    }

    public abstract class BaseKey {
        public virtual void OnMounted(IWidgetElement element) { }
        public virtual void OnUnmounted(IWidgetElement element) { }
    }

    public class GlobalKey : BaseKey {
        public IWidgetElement Target { get; private set; }

        public override void OnMounted(IWidgetElement element) {
            if (Target != null) {
                Debug.LogError(
                    $"GlobalKey collision detected. A widget with the same GlobalKey is already mounted. This can lead to unpredictable behavior."
                );
            }

            Target = element;
        }

        public override void OnUnmounted(IWidgetElement element) {
            if (Target != element) {
                Debug.LogError(
                    $"GlobalKey unmounting error. The widget being unmounted does not match the currently mounted widget for this key. This can lead to unpredictable behavior."
                );
            }

            Target = null;
        }
    }

    public class GlobalKey<T> : BaseKey where T : VisualElement {
        public IWidgetElement Target { get; private set; }
        public T Element { get; private set; }

        public override void OnMounted(IWidgetElement element) {
            if (Target != null) {
                Debug.LogError(
                    $"GlobalKey collision detected. A widget with the same GlobalKey is already mounted. This can lead to unpredictable behavior."
                );
            }

            Target = element;
            if (element.Element is T typedElement) { Element = typedElement; } else {
                Debug.LogError(
                    $"GlobalKey type mismatch. Expected element of type {typeof(T).Name} but got {element.Element.GetType().Name}. This can lead to unpredictable behavior."
                );
            }
        }

        public override void OnUnmounted(IWidgetElement element) {
            if (Target != element) {
                Debug.LogError(
                    $"GlobalKey unmounting error. The widget being unmounted does not match the currently mounted widget for this key. This can lead to unpredictable behavior."
                );
            }

            Target = null;
            Element = null;
        }
    }
}