using System.Collections;
using System.Collections.Generic;
using Hix.Runtime;
using Hix.Collections;

namespace Hix;

/// <summary>
///   Runtime storage for named Hix values. Names are already-resolved
///   <see cref="HixString" /> values; resolving or interning text belongs to the
///   executing thread, not to this collection.
///   Shared storage uses the flat/HAMT persistent map for constant-time snapshots.
///   VM locals opt into mutable dictionary backing and are cleared before pooling.
/// </summary>
public sealed class HixValueDictionary :
  IReadOnlyDictionary<HixString, IHixValue> {
  private readonly Dictionary<HixString, IHixValue> mutable;
  private static readonly PersistentMap<HixString, IHixValue> empty = new();
  private PersistentMap<HixString, IHixValue> persistent = empty;
  private IReadOnlyDictionary<HixString, IHixValue> ValuesMap => mutable ?? (IReadOnlyDictionary<HixString, IHixValue>)persistent;

  public HixValueDictionary() : this(false) { }

  public HixValueDictionary(bool mutable) {
    if (mutable) this.mutable = new Dictionary<HixString, IHixValue>();
  }

  public HixValueDictionary(IEnumerable<KeyValuePair<HixString, IHixValue>> values) : this() {
    StoreIsolatedRange(values);
  }

  public IHixValue this[HixString key] => ValuesMap[key];

  public IEnumerable<HixString> Keys => ValuesMap.Keys;
  public IEnumerable<IHixValue> Values => ValuesMap.Values;
  public int Count => ValuesMap.Count;

  public bool ContainsKey(HixString key) {
    return ValuesMap.ContainsKey(key);
  }

  public bool TryGetValue(HixString key, out IHixValue value) {
    return ValuesMap.TryGetValue(key, out value);
  }

  IEnumerator<KeyValuePair<HixString, IHixValue>> IEnumerable<KeyValuePair<HixString, IHixValue>>.
    GetEnumerator() {
    return ValuesMap.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return ValuesMap.GetEnumerator();
  }

  public IEnumerator<KeyValuePair<HixString, IHixValue>> GetEnumerator() {
    return ValuesMap.GetEnumerator();
  }

  public void Store(HixThread context, HixString key, IHixValue value) {
    StoreIsolated(key, context.DetachValue(value));
  }

  public void StoreIsolated(HixString key, IHixValue value) {
    if (mutable != null) mutable[key] = value;
    else persistent = persistent.SetItem(key, value);
  }

  public void StoreIsolatedRange(IEnumerable<KeyValuePair<HixString, IHixValue>> values) {
    if (values is null) return;
    foreach (var item in values) StoreIsolated(item.Key, item.Value);
  }

  public void Clear() {
    if (mutable != null) mutable.Clear();
    else persistent = empty;
  }

  public void ReplaceWith(IEnumerable<KeyValuePair<HixString, IHixValue>> values) {
    if (ReferenceEquals(this, values)) return;
    if (mutable == null && values is HixValueDictionary storage) {
      persistent = storage.Snapshot();
      return;
    }
    if (mutable != null) mutable.Clear();
    else persistent = empty;
    StoreIsolatedRange(values);
  }

  public PersistentMap<HixString, IHixValue> Snapshot() {
    if (mutable == null) return persistent;
    var builder = new PersistentMap<HixString, IHixValue>.Builder();
    foreach (var item in mutable) builder.SetItem(item.Key, item.Value);
    return builder.ToImmutable();
  }

  public void Restore(PersistentMap<HixString, IHixValue> snapshot) {
    if (mutable == null) persistent = snapshot;
    else ReplaceWith(snapshot);
  }
}
