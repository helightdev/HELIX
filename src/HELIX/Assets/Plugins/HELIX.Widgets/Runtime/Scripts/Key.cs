using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Text;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
  [SuppressMessage("ReSharper", "Unity.BurstAccessingManagedMethod")]
  [SuppressMessage("ReSharper", "Unity.BurstLoadingManagedType")]
  [SuppressMessage("ReSharper", "Unity.BurstFunctionSignatureContainsManagedTypes")]
  public readonly struct Key : IEquatable<Key> {
    public readonly ulong value;
    public readonly BaseKey details;

    public static readonly Key None = new(0);

    public Key(ulong value) {
      this.value = value;
      details = null;
    }

    public Key(ulong value, BaseKey detailsKey) {
      this.value = value;
      details = detailsKey;
    }

    public bool IsNone => value == 0 && details == null;

    public bool Equals(Key other) {
      return value == other.value && EqualityComparer<BaseKey>.Default.Equals(details, other.details);
    }

    public override bool Equals(object obj) {
      return obj is Key other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(value, details);
    }

    public static bool operator ==(Key a, Key b) {
      return a.value == b.value && EqualityComparer<BaseKey>.Default.Equals(a.details, b.details);
    }

    public static bool operator !=(Key a, Key b) {
      return a.value != b.value || !EqualityComparer<BaseKey>.Default.Equals(a.details, b.details);
    }

    public override string ToString() {
      return IsNone ? "None" : details != null ? details.ToString() : value.ToString();
    }

    public static implicit operator Key(ulong value) {
      return new Key(value);
    }

    public static implicit operator Key(uint value) {
      return new Key(value);
    }

    public static implicit operator Key(int value) {
      return new Key(unchecked((uint)value));
    }

    public static implicit operator Key(string value) {
      return new Key(HashString(value));
    }

    public static implicit operator Key(BaseKey detailsKey) {
      return new Key((ulong)detailsKey.GetType().GetHashCode(), detailsKey);
    }

    internal static ulong HashString(string value) {
      var source = Encoding.UTF8.GetBytes(value);
      return XxHash3.HashToUInt64(source);
    }
  }

  public abstract class BaseKey : DiagnosticableBase {
    public virtual void OnMounted(IWidgetElement element, Widget descriptor) { }
    public virtual void OnUnmounted(IWidgetElement element) { }
  }

  public class GlobalKey : BaseKey {
    public readonly string debugName;

    public GlobalKey(string debugName = null) {
      this.debugName = debugName;
    }

    public IWidgetElement Target { get; protected set; }

    public override void OnMounted(IWidgetElement element, Widget descriptor) {
      if (element == Target) return;
      if (Target != null) {
        HelixDiagnostics.Build(
          "An already mounted global key has been used by a new element",
          "Global keys may only be used by one element at a time. " +
          "This warning indicates that a global key is being reused without being unmounted first, " +
          "which can lead to unpredictable behavior.",
          new DiagnosticsNode[] {
            new ErrorProperty(
              "The issue occured near",
              BuildContext.GetUserTarget(BuildContext.ReconcilerCurrent, element)
            ),
            new ErrorSpacer(), new ErrorProperty("The previous owner of the key was", Target),
            new ErrorSpacer(), new ErrorProperty("The new owner of this key is", element)
          },
          hints: new DiagnosticsNode[] {
            new ErrorHint(
              "Ensure that the global key is unmounted from its previous element before being used again."
            )
          }
        ).Report(DiagnosticLevel.Warning);
      }

      Target = element;
    }

    public override void OnUnmounted(IWidgetElement element) {
      if (Target != element) {
        HelixDiagnostics.Build(
          "A global key is being unmounted by an element that does not own it",
          "This warning indicates that a global key is being unmounted by an element that is " +
          "not the current owner of the key. This can lead to unpredictable behavior, " +
          "as the key may still be considered mounted to its previous owner.",
          new DiagnosticsNode[] {
            new ErrorProperty("The current owner of the key is", Target), new ErrorSpacer(),
            new ErrorProperty("The element attempting to unmount the key is", element)
          },
          hints: new DiagnosticsNode[] {
            new ErrorHint("Ensure that the element unmounting the key is the same element that mounted it.")
          }
        ).Report(DiagnosticLevel.Warning);
      }

      Target = null;
    }

    public override string ToStringShort() {
      return $"GlobalKey#{debugName ?? this.ShortHash()}";
    }
  }

  public class GlobalKey<T> : GlobalKey where T : VisualElement {
    public GlobalKey(string debugName = null) : base(debugName) { }
    public T Element { get; private set; }

    public override void OnMounted(IWidgetElement element, Widget descriptor) {
      if (element == Target) return;
      base.OnMounted(element, descriptor);
      if (element.Element is T typedElement) Element = typedElement;
      else {
        HelixDiagnostics.Build(
          "A global key was mounted to an element of an incompatible type",
          "The generic type parameter of a GlobalKey<T> must match the type of the VisualElement it is mounted to. " +
          "This warning indicates that a GlobalKey<T> was mounted to an element whose type does not match T, " +
          "which will lead to null reference exceptions when trying to access the Element property.",
          new DiagnosticsNode[] {
            new ErrorProperty("The expected type of the element is", typeof(T).Name), new ErrorSpacer(),
            new ErrorProperty("The actual type of the element is", element.Element.GetType().Name)
          },
          hints: new DiagnosticsNode[] {
            new ErrorHint("Ensure that the element being mounted to the key is of the correct type.")
          }
        ).Report(DiagnosticLevel.Warning);
      }
    }

    public override void OnUnmounted(IWidgetElement element) {
      base.OnUnmounted(element);
      Element = null;
    }

    public override string ToStringShort() {
      return $"GlobalKey<{typeof(T).Name}>#{debugName ?? this.ShortHash()}";
    }
  }
}