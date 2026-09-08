using System.Collections;
using System.Collections.Generic;
using Mixins.Runtime;
using Mixins.Collections;

namespace Mixins;

/// <summary>
///   Runtime storage for named mixin values. Names are already-resolved
///   <see cref="MixinString" /> values; resolving or interning text belongs to the
///   execution context, not to this collection.
///   Shared storage uses the flat/HAMT persistent map for constant-time snapshots.
///   VM locals opt into mutable dictionary backing and are cleared before pooling.
/// </summary>
internal sealed class MixinValueDictionary :
  IReadOnlyDictionary<MixinString, IMixinValue> {
  private readonly Dictionary<MixinString, IMixinValue> mutable;
  private static readonly PersistentMap<MixinString, IMixinValue> empty = new();
  private PersistentMap<MixinString, IMixinValue> persistent = empty;
  private IReadOnlyDictionary<MixinString, IMixinValue> ValuesMap => mutable ?? (IReadOnlyDictionary<MixinString, IMixinValue>)persistent;

  internal MixinValueDictionary() : this(false) { }

  internal MixinValueDictionary(bool mutable) {
    if (mutable) this.mutable = new Dictionary<MixinString, IMixinValue>();
  }

  internal MixinValueDictionary(IEnumerable<KeyValuePair<MixinString, IMixinValue>> values) : this() {
    StoreIsolatedRange(values);
  }

  public IMixinValue this[MixinString key] => ValuesMap[key];

  public IEnumerable<MixinString> Keys => ValuesMap.Keys;
  public IEnumerable<IMixinValue> Values => ValuesMap.Values;
  public int Count => ValuesMap.Count;

  public bool ContainsKey(MixinString key) {
    return ValuesMap.ContainsKey(key);
  }

  public bool TryGetValue(MixinString key, out IMixinValue value) {
    return ValuesMap.TryGetValue(key, out value);
  }

  IEnumerator<KeyValuePair<MixinString, IMixinValue>> IEnumerable<KeyValuePair<MixinString, IMixinValue>>.
    GetEnumerator() {
    return ValuesMap.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return ValuesMap.GetEnumerator();
  }

  public IEnumerator<KeyValuePair<MixinString, IMixinValue>> GetEnumerator() {
    return ValuesMap.GetEnumerator();
  }

  internal void Store(ExecutionContext context, MixinString key, IMixinValue value) {
    StoreIsolated(key, context.DetachValue(value));
  }

  internal void StoreIsolated(MixinString key, IMixinValue value) {
    if (mutable != null) mutable[key] = value;
    else persistent = persistent.SetItem(key, value);
  }

  internal void StoreIsolatedRange(IEnumerable<KeyValuePair<MixinString, IMixinValue>> values) {
    if (values is null) return;
    foreach (var item in values) StoreIsolated(item.Key, item.Value);
  }

  internal void Clear() {
    if (mutable != null) mutable.Clear();
    else persistent = empty;
  }

  internal void ReplaceWith(IEnumerable<KeyValuePair<MixinString, IMixinValue>> values) {
    if (ReferenceEquals(this, values)) return;
    if (mutable == null && values is MixinValueDictionary storage) {
      persistent = storage.Snapshot();
      return;
    }
    if (mutable != null) mutable.Clear();
    else persistent = empty;
    StoreIsolatedRange(values);
  }

  internal PersistentMap<MixinString, IMixinValue> Snapshot() {
    if (mutable == null) return persistent;
    var builder = new PersistentMap<MixinString, IMixinValue>.Builder();
    foreach (var item in mutable) builder.SetItem(item.Key, item.Value);
    return builder.ToImmutable();
  }

  internal void Restore(PersistentMap<MixinString, IMixinValue> snapshot) {
    if (mutable == null) persistent = snapshot;
    else ReplaceWith(snapshot);
  }
}
