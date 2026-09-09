using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Runtime;

/// <summary>A live, read-only table view of storage. Member reads never enumerate or copy storage.</summary>
public sealed class HixStorageValue : HixTableValue {
  private readonly HixValueDictionary storage;
  private readonly HixValueDictionary fallback;

  internal HixStorageValue(HixValueDictionary storage,
    HixValueDictionary fallback = null) : base(new StorageEntries(storage, fallback), true) {
    this.storage = storage;
    this.fallback = fallback;
  }

  public static IHixValue Capture(IHixValue value) =>
    value is HixStorageValue storage ? storage.CaptureTable() : value;

  private HixTableValue CaptureTable() {
    var snapshot = storage.Snapshot();
    if (fallback != null)
      foreach (var entry in fallback)
        if (!snapshot.ContainsKey(entry.Key)) snapshot = snapshot.SetItem(entry.Key, entry.Value);
    return new HixTableValue(snapshot);
  }

  public override bool TryGetValue(HixExecutionContext context, HixString member, out IHixValue value) {
    var name = HixExecutionContext.Dynamic(member.Resolve(context.Strings));
    if (storage.TryGetValue(name, out value)) return true;
    if (fallback != null) return fallback.TryGetValue(name, out value);
    value = null; return false;
  }

  // Table consumers enumerate this view directly. Operations that construct a new table
  // retain their existing materialization behavior; simply loading a root allocates no entries.
  private sealed class StorageEntries(HixValueDictionary storage,
    HixValueDictionary fallback) : IReadOnlyList<KeyValuePair<HixString, IHixValue>> {
    public int Count {
      get {
        var count = storage.Count;
        if (fallback != null)
          foreach (var entry in fallback) if (!storage.ContainsKey(entry.Key)) count++;
        return count;
      }
    }

    public KeyValuePair<HixString, IHixValue> this[int index] {
      get {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        foreach (var entry in this) if (index-- == 0) return entry;
        throw new ArgumentOutOfRangeException(nameof(index));
      }
    }

    public IEnumerator<KeyValuePair<HixString, IHixValue>> GetEnumerator() {
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
