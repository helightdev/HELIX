using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

/// <summary>A live, read-only table view of storage. Member reads never enumerate or copy storage.</summary>
internal sealed class MixinStorageValue : MixinTableValue {
  private readonly MixinValueDictionary storage;
  private readonly MixinValueDictionary fallback;

  internal MixinStorageValue(MixinValueDictionary storage,
    MixinValueDictionary fallback = null) : base(new StorageEntries(storage, fallback), true) {
    this.storage = storage;
    this.fallback = fallback;
  }

  internal static IMixinValue Capture(IMixinValue value) =>
    value is MixinStorageValue storage ? storage.CaptureTable() : value;

  private MixinTableValue CaptureTable() {
    var snapshot = storage.Snapshot();
    if (fallback != null)
      foreach (var entry in fallback)
        if (!snapshot.ContainsKey(entry.Key)) snapshot = snapshot.SetItem(entry.Key, entry.Value);
    return new MixinTableValue(snapshot);
  }

  public override bool TryGetValue(ExecutionContext context, MixinString member, out IMixinValue value) {
    var name = ExecutionContext.Dynamic(member.Resolve(context.Strings));
    if (storage.TryGetValue(name, out value)) return true;
    if (fallback != null) return fallback.TryGetValue(name, out value);
    value = null; return false;
  }

  // Table consumers enumerate this view directly. Operations that construct a new table
  // retain their existing materialization behavior; simply loading a root allocates no entries.
  private sealed class StorageEntries(MixinValueDictionary storage,
    MixinValueDictionary fallback) : IReadOnlyList<KeyValuePair<MixinString, IMixinValue>> {
    public int Count {
      get {
        var count = storage.Count;
        if (fallback != null)
          foreach (var entry in fallback) if (!storage.ContainsKey(entry.Key)) count++;
        return count;
      }
    }

    public KeyValuePair<MixinString, IMixinValue> this[int index] {
      get {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        foreach (var entry in this) if (index-- == 0) return entry;
        throw new ArgumentOutOfRangeException(nameof(index));
      }
    }

    public IEnumerator<KeyValuePair<MixinString, IMixinValue>> GetEnumerator() {
      // Locals override carried values; enumeration order is unspecified.
      if (fallback != null)
        foreach (var entry in fallback)
          yield return new(entry.Key, storage.TryGetValue(entry.Key, out var value) ? value : entry.Value);
      foreach (var entry in storage)
        if (fallback == null || !fallback.ContainsKey(entry.Key))
          yield return new(entry.Key, entry.Value);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}
