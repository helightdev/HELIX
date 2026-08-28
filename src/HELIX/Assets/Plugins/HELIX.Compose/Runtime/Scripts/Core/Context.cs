using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct ContextKey<T> {
    public readonly int id;
    public readonly T defaultValue;

    private ContextKey(int id) {
      this.id = id;
      defaultValue = default;
    }

    public ContextKey(string name) {
      // if (ContextKeyData.ByName.TryGetValue(name, out var target)) {
      //   var data = ContextKeyData.Registry.GetValueOrDefault(target);
      //   throw new ArgumentException(
      //     $"ContextKey with name '{name}' already exists for type {data.type} with id {target}."
      //   );
      // }

      if (ContextKeyData.NextId == int.MaxValue)
        throw new InvalidOperationException("Maximum number of context keys reached.");
      id = ContextKeyData.NextId++;
      defaultValue = default;
      ContextKeyData.Registry[id] = new ContextKeyData(typeof(T), name);
    }

    public ContextKey(string name, T defaultValue) : this(name) {
      this.defaultValue = defaultValue;
    }

    public static implicit operator ContextKey<T>(int id) {
      return new ContextKey<T>(id);
    }

    public static implicit operator int(ContextKey<T> key) {
      return key.id;
    }

    public bool TryReadDataAt(VisualElement element, out ContextData<T> data, bool includeSelf = true) {
      data = null;
      if (!ContextData.TryLookup(element, id, out var read, includeSelf)) return false;
      if (read is not ContextData<T> typedData) return false;
      data = typedData;
      return true;
    }


    public bool TryReadAt(VisualElement element, out T value, bool includeSelf = true) {
      value = default;
      if (!ContextData.TryLookup(element, id, out var data, includeSelf)) return false;
      if (data is not ContextData<T> typedData) return false;
      value = typedData.value;
      return true;
    }

    public T ReadAt(VisualElement element, bool includeSelf = true) {
      if (!ContextData.TryLookup(element, id, out var data, includeSelf)) return defaultValue;
      if (data is not ContextData<T> typedData) return defaultValue;
      return typedData.value;
    }

    public T ReadAtOrDefault(VisualElement element, T onDefault = default, bool includeSelf = true) {
      if (!ContextData.TryLookup(element, id, out var data, includeSelf)) return onDefault;
      if (data is not ContextData<T> typedData) return onDefault;
      return typedData.value;
    }

    public T ReadOrThemeProperty(in Composition cx, ThemeProperty<T> property) {
      return cx.TryReadContext(this, out var value) ? value : property[ThemeData.Key[in cx]];
    }

    public T this[VisualElement element] => ReadAt(element);

    public T this[in Composition cx] => cx.ReadContext(this);

    public T this[in ContextAccessor accessor] {
      get => ReadAt(accessor.contributor.Element);
      set => accessor.Put(this, value);
    }

    public ContextReference<T> CreateReference() {
      return new ContextReference<T>(this, null);
    }

    public static ContextKeyData GetData(int id) {
      if (ContextKeyData.Registry.TryGetValue(id, out var data)) return data;
      throw new KeyNotFoundException($"ContextKey with id {id} not found.");
    }

    public static bool TryGetData(int id, out ContextKeyData data) {
      return ContextKeyData.Registry.TryGetValue(id, out data);
    }
  }


  public readonly struct ContextKeyData {
    internal const int MaxAnonymousPoolSize = 128;

    internal static readonly Dictionary<int, ContextKeyData> Registry = new();
    internal static readonly Queue<int> AnonymousIdPool = new();
    internal static int NextId = 1;
    internal static int AnonymousId = -1;

    public readonly Type type;
    public readonly string name;
    public readonly WeakReference<object> reference;

    public ContextKeyData(Type type, string name) {
      this.type = type;
      this.name = name;
      reference = null;
    }

    public ContextKeyData(Type type, string name, WeakReference<object> reference) {
      this.type = type;
      this.name = name;
      this.reference = reference;
    }

    public bool HasInstance => reference != null && reference.TryGetTarget(out _);

    public bool TryGetInstance(out object instance) {
      instance = null;
      return reference != null && reference.TryGetTarget(out instance);
    }

    public static void CollectRegistry(List<KeyValuePair<int, ContextKeyData>> results) {
      if (results == null) throw new ArgumentNullException(nameof(results));
      foreach (var entry in Registry) results.Add(entry);
    }

    public static int ClaimAnonymous(Type type, string debugName, WeakReference<object> instance = null) {
      int id;
      if (AnonymousIdPool.Count > 0) id = AnonymousIdPool.Dequeue();
      else {
        if (AnonymousId == int.MinValue) AnonymousId = -(1 << 8);
        id = AnonymousId--;
      }

      var data = new ContextKeyData(type, debugName, instance);
      Registry[id] = data;
      return id;
    }

    public static void ReleaseAnonymous(int id) {
      if (id == 0) return;
      Registry.Remove(id);
      if (AnonymousIdPool.Count >= MaxAnonymousPoolSize) return;
      AnonymousIdPool.Enqueue(id);
    }
  }

  public abstract class ContextData : IDisposable {
    protected bool detached = false; // Non-hierarchal context data is detached (Like static signals)
    protected internal ContextVersion version = ContextVersion.Initial;
    public ContextVersion Version => version;

    public bool ContextDetached {
      get => HasFlag(ContextFlags.Detached);
      set {
        if (value) version.flags |= ContextFlags.Detached;
        else version.flags &= ~ContextFlags.Detached;
      }
    }

    public bool HasValue => (version.flags & ContextFlags.NoValueMask) == 0;

    protected ContextFlags CleanFlags => detached ? ContextFlags.Detached : ContextFlags.None;

    public abstract void Dispose();

    public void IncrementContextVersion() {
      unchecked { version.counter++; }
    }

    protected void DisposeContext() {
      unchecked { version.counter++; }
      version.UpdateValueFlagsCleanly(ContextFlags.Disposed);
    }

    public bool HasFlag(ContextFlags flags) {
      return (version.flags & flags) != 0;
    }

    public static bool TryLookup(VisualElement element, int key, out ContextData data, bool includeSelf = false) {
      using (HXComposeProfiling.LookupContextMarker.Auto()) {
        data = null;
        if (element == null) return false;
        if (includeSelf && element is IContextComposable self) return self.TryLookupContext(key, out data);
        var contributor = element.GetFirstAncestorOfType<IContextComposable>();
        return contributor != null && contributor.TryLookupContext(key, out data);
      }
    }
  }

  public sealed class ContextData<T> : ContextData {
    public T value;

    public void UpdateContextValue(T updated) {
      value = updated;
      unchecked { version.counter++; }
      version.UpdateValueFlagsCleanly(ContextFlags.None);
    }

    public void DeleteContextValue() {
      value = default;
      unchecked { version.counter++; }
      version.UpdateValueFlagsCleanly(ContextFlags.Empty);
    }

    public override void Dispose() {
      DisposeContext();
      value = default;
    }

    public ref T GetValueRef() {
      return ref value;
    }
  }

  [StructLayout(LayoutKind.Sequential, Size = 8)]
  public struct ContextVersion {
    public static readonly ContextVersion Initial = new(0, ContextFlags.None);
    public static readonly ContextVersion Disposed = new(0, ContextFlags.Disposed);

    public uint counter;
    public ContextFlags flags;

    public ContextVersion(uint counter, ContextFlags flags) {
      this.counter = counter;
      this.flags = flags;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateValueFlags(ContextFlags updated) {
      UpdateValueFlagsCleanly(updated & ContextFlags.ValueMask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UpdateValueFlagsCleanly(ContextFlags updated) {
      flags = updated | (flags & ContextFlags.PersistentMask);
    }
  }

  [Flags]
  public enum ContextFlags : byte {
    None = 0,
    Detached = 1 << 1, // Used to mark that a context data is not associated to the element tree (static signals)

    Dirty = 1 << 3, // Used by trackers to mark a value/version change

    Empty = 1 << 6, // The context data exists but no value is set (null support for structs)
    Disposed = 1 << 7, // The context data has been disposed

    ValueMask = Dirty | Empty | Disposed,
    PersistentMask = Detached,
    NoValueMask = Empty | Disposed
  }

  public struct ContextReference<T> {
    public readonly ContextKey<T> key;
    public ContextData<T> value;
    public ContextVersion version;

    public ContextReference(ContextKey<T> key, ContextData<T> value) : this() {
      this.value = value;
      this.key = key;
      version = value?.Version ?? ContextVersion.Disposed;
    }

    public static ContextReference<T> Create(ContextKey<T> key) {
      return new ContextReference<T>(key, null);
    }

    public static ContextReference<T> Create(ContextKey<T> key, ContextData<T> value) {
      return new ContextReference<T>(key, value);
    }

    public static implicit operator ContextReference<T>(ContextKey<T> key) {
      return new ContextReference<T>(key, null);
    }

    public static implicit operator ContextKey<T>(ContextReference<T> reference) {
      return reference.key;
    }

    public bool IsDirty => (version.flags & ContextFlags.Dirty) != 0;
    public bool IsEmpty => (version.flags & ContextFlags.Empty) != 0;
    public bool IsDisposed => (version.flags & ContextFlags.Disposed) != 0;
    public bool HasValue => (version.flags & ContextFlags.NoValueMask) == 0;

    public bool Refresh(ContextData<T> read) {
      if (read != value) {
        value = read;
        version = read.Version;
        version.flags |= ContextFlags.Dirty;
        return true;
      }

      if (value.version.counter == version.counter) {
        version.flags &= ~ContextFlags.Dirty;
        return false;
      }

      version = value.version;
      version.flags |= ContextFlags.Dirty;
      return true;
    }

    public void Pull() {
      // if (RecompositionScope.TryGetContext(key, out var current)) {
      //   Refresh(current);
      // } else {
      //   if (version.flags < ContextFlags.Empty) {
      //     // We previously had data, this is technically dirty
      //     version.flags = ContextFlags.Dirty | ContextFlags.Disposed;
      //     value = null;
      //   } else {
      //     // No data, so this doesn't affect consumers
      //     Reset();
      //   }
      // }
      throw new NotImplementedException();
    }

    public void Reset() {
      value = null;
      version = ContextVersion.Disposed;
    }

    public bool TryRead(out T output) {
      if (version.flags >= ContextFlags.Empty) {
        output = default;
        return false;
      }

      output = value.value;
      return true;
    }

    public T GetOrDefault(T defaultValue) {
      return TryRead(out var result) ? result : defaultValue;
    }
  }

  public interface IContextComposable : IComposable {
    IContextComposable ContextParent { get; }
    bool TryLookupContext(int key, out ContextData data);
  }

  public interface IContextWriteable : IComposable {
    SparseContextMap WrittenContext { get; set; }

    SparseContextMap AcquireWriteableContext() {
      return WrittenContext ??= SparseContextMap.Get();
    }

    void BeginContextModification() {
      WrittenContext?.ResetPublicationMarkers();
    }

    void EndContextModification() {
      WrittenContext?.PrunePublications();
    }
  }

  public struct LookupCache {
    internal static readonly ObjectPool<Dictionary<int, LookupCacheEntry>> Pool = new(
      () => new Dictionary<int, LookupCacheEntry>(8),
      null,
      static obj => obj.Clear(),
      null,
      false,
      10,
      128
    );

    private Dictionary<int, LookupCacheEntry> _cache;

    public void Claim() {
      if (_cache != null) return;
      _cache = Pool.Get();
    }

    public void Release() {
      if (_cache == null) return;
      _cache.Clear();
      Pool.Release(_cache);
      _cache = null;
    }

    public void Clear() {
      _cache?.Clear();
    }

    public bool TryLookup(IContextComposable parent, int key, out ContextData data) {
      data = null;
      if (_cache == null) return parent != null && parent.TryLookupContext(key, out data);

      if (_cache.TryGetValue(key, out var entry)) {
        data = entry.data;
        return entry.hasData;
      }

      if (parent == null) return false;
      var hasData = parent.TryLookupContext(key, out data);
      _cache[key] = new LookupCacheEntry(hasData, data);
      return hasData;
    }

    internal readonly struct LookupCacheEntry {
      public readonly bool hasData;
      public readonly ContextData data;

      public LookupCacheEntry(bool hasData, ContextData data) {
        this.hasData = hasData;
        this.data = data;
      }
    }
  }

  public class ContextComposableElement : ComposableElement, IContextComposable, IContextWriteable {
    public ContextComposableElement() {
      RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    }

    public IContextComposable ContextParent { get; private set; }

    public override void Reset() {
      base.Reset();
      if (WrittenContext != null) SparseContextMap.Release(WrittenContext);
    }

    public bool TryLookupContext(int key, out ContextData data) {
      data = null;
      if (WrittenContext != null && WrittenContext.TryGet(key, out data)) return true;
      return ContextParent != null && ContextParent.TryLookupContext(key, out data);
    }

    public SparseContextMap WrittenContext { get; set; }

    public SparseContextMap AcquireWriteableContext() {
      return WrittenContext ??= SparseContextMap.Get();
    }

    public void EndContextModification() {
      if (WrittenContext == null) return;
      WrittenContext.PrunePublications();
      if (!WrittenContext.IsUnused) return;
      SparseContextMap.Release(WrittenContext);
      WrittenContext = null;
    }

    private void OnAttachToPanel(AttachToPanelEvent evt) {
      RefreshHierarchy();
    }

    public void RefreshHierarchy() {
      ContextParent = GetFirstAncestorOfType<IContextComposable>();
    }

    private void OnDetachFromPanel(DetachFromPanelEvent evt) {
      Reset();
    }
  }

  public readonly struct ContextModificationScope : IDisposable {
    private readonly IContextWriteable _contributor;

    public ContextModificationScope(IContextWriteable contributor) {
      _contributor = contributor;
    }

    public void Dispose() {
      _contributor.EndContextModification();
    }
  }

  public readonly ref struct ContextAccessor {
    internal readonly IContextWriteable contributor;

    public ContextAccessor(IContextWriteable contributor) {
      this.contributor = contributor;
    }

    public T ReadInherited<T>(ContextKey<T> key) {
      return key.ReadAt(contributor.Element, false);
    }

    public bool TryReadInherited<T>(ContextKey<T> key, out T value) {
      return key.TryReadAt(contributor.Element, out value, false);
    }

    public ContextData<T> AcquireWritableData<T>(ContextKey<T> key) {
      var context = contributor.AcquireWriteableContext();
      return context.GetWriteable(key);
    }

    public void Put<T>(ContextKey<T> key, T value) {
      AcquireWritableData(key).UpdateContextValue(value);
    }

    public void PutEmpty<T>(ContextKey<T> key) {
      AcquireWritableData(key).DeleteContextValue();
    }

    public void Put<T>(ContextKey<T> key, T value, bool empty) {
      if (empty) PutEmpty(key);
      else Put(key, value);
    }

    public void Remove<T>(ContextKey<T> key) {
      if (contributor.WrittenContext == null) return;
      contributor.AcquireWriteableContext().Remove(key);
    }
  }
}