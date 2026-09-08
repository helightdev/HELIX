using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

/// <summary>A live, read-only table view of storage. Member reads never enumerate or copy storage.</summary>
internal sealed record MixinStorageValue : MixinTableValue {
  private readonly MixinValueDictionary storage;
  private readonly MixinValueDictionary fallback;

  internal MixinStorageValue(MixinValueDictionary storage,
    MixinValueDictionary fallback = null) : base(new StorageEntries(storage, fallback)) {
    this.storage = storage;
    this.fallback = fallback;
  }

  internal static IMixinValue Capture(IMixinValue value) =>
    value is MixinStorageValue storage ? new MixinTableValue(storage.Entries.ToArray()) : value;

  public override IMixinValue Select(ExecutionContext context, MixinString member) {
    var name = ExecutionContext.Dynamic(member.Resolve(context.Strings));
    if (storage.TryGetValue(name, out var value)) return value;
    return fallback != null && fallback.TryGetValue(name, out value) ? value : NullMixinValue.Instance;
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
      // Preserve carried-key ordering, with locals taking precedence, just as the
      // materialized local table did. Non-carried locals follow in storage order.
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
