using System.Collections;
using System.Collections.Generic;
using MixinLanguage.Compiler;

namespace MixinLanguage;

/// <summary>
///   Runtime storage for named mixin values. Names are already-resolved
///   <see cref="MixinString" /> values; resolving or interning text belongs to the
///   execution context, not to this collection.
/// </summary>
internal sealed class MixinValueDictionary :
  IReadOnlyDictionary<MixinString, IMixinValue> {
  private readonly Dictionary<MixinString, IMixinValue> _values;

  internal MixinValueDictionary() {
    _values = new Dictionary<MixinString, IMixinValue>();
  }

  internal MixinValueDictionary(IEnumerable<KeyValuePair<MixinString, IMixinValue>> values) : this() {
    StoreIsolatedRange(values);
  }

  public IMixinValue this[MixinString key] => _values[key];

  public IEnumerable<MixinString> Keys => _values.Keys;
  public IEnumerable<IMixinValue> Values => _values.Values;
  public int Count => _values.Count;

  public bool ContainsKey(MixinString key) {
    return _values.ContainsKey(key);
  }

  public bool TryGetValue(MixinString key, out IMixinValue value) {
    return _values.TryGetValue(key, out value);
  }

  IEnumerator<KeyValuePair<MixinString, IMixinValue>> IEnumerable<KeyValuePair<MixinString, IMixinValue>>.
    GetEnumerator() {
    return _values.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return _values.GetEnumerator();
  }

  public Dictionary<MixinString, IMixinValue>.Enumerator GetEnumerator() {
    return _values.GetEnumerator();
  }

  internal void Store(ExecutionContext context, MixinString key, IMixinValue value) {
    _values[key] = context.DetachValue(value);
  }

  internal void StoreIsolated(MixinString key, IMixinValue value) {
    _values[key] = value;
  }

  internal void StoreIsolatedRange(IEnumerable<KeyValuePair<MixinString, IMixinValue>> values) {
    if (values is null) return;
    foreach (var item in values) _values[item.Key] = item.Value;
  }

  internal void Clear() {
    _values.Clear();
  }

  internal void ReplaceWith(IEnumerable<KeyValuePair<MixinString, IMixinValue>> values) {
    _values.Clear();
    StoreIsolatedRange(values);
  }
}