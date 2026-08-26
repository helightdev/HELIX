using System;
using System.Collections;
using System.Collections.Generic;

namespace HELIX.Context {
  /// <summary>Observes bindings published in a scope and all of its descendants.</summary>
  public abstract class BindingObserver {
    protected internal abstract void BindingAdded(ManagedScope scope, TypeKey key, ManagedBinding binding);
    protected internal abstract void BindingRemoved(ManagedScope scope, TypeKey key, ManagedBinding binding);
  }

  public class ManagedRegistry<T, TData> : BindingObserver,
    IEnumerable<KeyValuePair<ManagedId, ManagedRegistry<T, TData>.Entry>>, IReadOnlyList<T> where TData : struct {
    public readonly Dictionary<ManagedId, Entry> items = new();

    public int Count => items.Count;
    public Entry this[ManagedId id] => items[id];
    public T this[int index] => ValueAt(index);

    protected internal override void BindingAdded(ManagedScope scope, TypeKey key, ManagedBinding binding) {
      if (binding.id.owner == 0 || binding.value is not T value) return;
      items.Add(binding.id, new Entry(value));
    }

    protected internal override void BindingRemoved(ManagedScope scope, TypeKey key, ManagedBinding binding) {
      if (binding.id.owner != 0 && binding.value is T) items.Remove(binding.id);
    }

    public bool Contains(ManagedId id) => items.ContainsKey(id);

    public bool TryGet(ManagedId id, out Entry entry) => items.TryGetValue(id, out entry);

    public bool SetData(ManagedId id, TData data) {
      if (!items.TryGetValue(id, out var entry)) return false;
      entry.data = data;
      items[id] = entry;
      return true;
    }

    // Direct foreach uses this struct enumerator and does not box it.
    public Dictionary<ManagedId, Entry>.Enumerator GetEnumerator() => items.GetEnumerator();

    IEnumerator<KeyValuePair<ManagedId, Entry>>
      IEnumerable<KeyValuePair<ManagedId, Entry>>.GetEnumerator() =>
      GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new ValueEnumerator(items.GetEnumerator());

    private T ValueAt(int index) {
      if ((uint)index >= (uint)items.Count) throw new ArgumentOutOfRangeException(nameof(index));
      var enumerator = items.GetEnumerator();
      while (enumerator.MoveNext()) {
        if (index-- == 0) return enumerator.Current.Value.value;
      }
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    private struct ValueEnumerator : IEnumerator<T> {
      private Dictionary<ManagedId, Entry>.Enumerator _enumerator;

      public ValueEnumerator(Dictionary<ManagedId, Entry>.Enumerator enumerator) {
        _enumerator = enumerator;
      }

      public T Current => _enumerator.Current.Value.value;
      object IEnumerator.Current => Current;
      public bool MoveNext() => _enumerator.MoveNext();
      public void Dispose() => _enumerator.Dispose();
      void IEnumerator.Reset() => throw new NotSupportedException();
    }

    public struct Entry {
      public T value;
      public TData data;

      internal Entry(T value) {
        this.value = value;
        data = default;
      }
    }
  }
}
