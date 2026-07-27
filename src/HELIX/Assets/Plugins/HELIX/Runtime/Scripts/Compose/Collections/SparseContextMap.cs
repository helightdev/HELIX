using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.Pool;

namespace HELIX.Compose.Collections {
  public class SparseContextMap {
    private const int _defaultCapacity = 5;

    private static readonly ObjectPool<SparseContextMap> _pool = new(
      () => new SparseContextMap(),
      null,
      static obj => obj.Clear(),
      null,
      false,
      10,
      128
    );

    private static readonly ProfilerCounterValue<int> _poolSize = new(
      HelixProfiling.HelixCategory,
      "SCM Pool Size",
      ProfilerMarkerDataUnit.Count,
      ProfilerCounterOptions.FlushOnEndOfFrame
    );

    private static readonly ProfilerCounterValue<float> _poolActiveRatio = new(
      HelixProfiling.HelixCategory,
      "SCM Pool Active Ratio",
      ProfilerMarkerDataUnit.Percent,
      ProfilerCounterOptions.FlushOnEndOfFrame
    );

    internal static void TrackProfiling() {
      _poolSize.Value = _pool.CountAll;
      _poolActiveRatio.Value = (_pool.CountAll > 0 ? (float)_pool.CountActive / _pool.CountAll : 0f) * 100f;
    }

    public RefKeyedSet<Publication, int> publications;
    public RefKeyedSet<Subscription, int> subscriptions;

    public static SparseContextMap Get() {
      return _pool.Get();
    }

    public static void Release(SparseContextMap contextMap) {
      _pool.Release(contextMap);
    }


    public SparseContextMap(int capacity) {
      publications = new RefKeyedSet<Publication, int>(capacity);
      subscriptions = new RefKeyedSet<Subscription, int>(capacity);
    }

    public SparseContextMap() : this(_defaultCapacity) { }

    public int PublicationCount => publications.Count;
    public bool IsUnused => publications.IsEmpty && subscriptions.IsEmpty;

    public void Clear() {
      publications.Clear();
      subscriptions.Clear();
    }

    public void Put(int key, ContextData value) {
      var index = publications.GetOrAddIndex(key, out _);
      ref var entry = ref publications[index];
      if (!ReferenceEquals(entry.value, value)) entry.value?.Dispose();
      entry.key = key;
      entry.value = value;
      entry.marked = true;
    }

    public bool Remove(int key) {
      return publications.RemoveSwapBack(key);
    }

    public bool TryGet(int key, out ContextData value) {
      var index = publications.FindIndex(key);
      value = null;
      if (index == -1) return false;
      value = publications[index].value;
      return true;
    }

    public bool TryGet<T>(ContextKey<T> key, out ContextData<T> value) {
      var index = publications.FindIndex(key);
      value = null;
      if (index == -1) return false;
      value = publications[index].value as ContextData<T>;
      return value != null;
    }

    public void LoadInto(Dictionary<int, ContextData> context) {
      for (var i = 0; i < publications.Count; i++) {
        ref var entry = ref publications[i];
        context.TryAdd(entry.key, entry.value);
      }
    }

    public void Subscribe(int key, ContextData data) {
      var index = subscriptions.GetOrAddIndex(key, out var existed);
      ref var entry = ref subscriptions[index];
      if (entry.marked) return; // Prevent dirty erasure by multiple subscriptions in the same recomposition
      var isDirty = !existed || !ReferenceEquals(entry.source, data) || entry.version.counter != data.version.counter;
      if (isDirty) {
        entry.id = key;
        entry.source = data;
        entry.version = data.version;
        entry.version.flags |= ContextFlags.Dirty;
      } else {
        entry.version.flags &= ~ContextFlags.Dirty;
      }
      entry.marked = true;
    }

    public bool CheckSubscriptionsModified() {
      for (var i = 0; i < subscriptions.Count; i++) {
        ref var entry = ref subscriptions[i];
        if (entry.IsModified) return true;
      }
      return false;
    }

    public void ResetMarkers() {
      for (var i = publications.Count - 1; i >= 0; i--) {
        ref var entry = ref publications[i];
        entry.marked = false;
      }

      for (var i = subscriptions.Count - 1; i >= 0; i--) {
        ref var entry = ref subscriptions[i];
        entry.marked = false;
      }
    }

    public void Prune() {
      PrunePublications();
      PruneSubscriptions();
    }

    public void PrunePublications() {
      for (var i = publications.Count - 1; i >= 0; i--) {
        ref var entry = ref publications[i];
        if (entry.marked) continue;
        entry.value.Dispose();
        publications.RemoveAtSwapBack(i);
      }
    }

    public void PruneSubscriptions() {
      for (var i = subscriptions.Count - 1; i >= 0; i--) {
        ref var entry = ref subscriptions[i];
        if (entry.marked) continue;
        subscriptions.RemoveAtSwapBack(i);
      }
    }

    public struct Publication : IRefKeyed<int> {
      public int key;
      public ContextData value;
      public bool marked;

      public Publication(int key, ContextData value, bool marked) {
        this.key = key;
        this.value = value;
        this.marked = marked;
      }

      public int Key => key;
    }

    public struct Subscription : IRefKeyed<int> {
      public int id;
      public ContextData source;
      public ContextVersion version;
      public bool marked;
      public int Key => id;
      public bool IsModified => version.counter != source.version.counter;
    }
  }
}