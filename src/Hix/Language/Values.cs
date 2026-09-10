using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Runtime;
using Hix.Compiler;
using Hix.Collections;
using System.Collections;

namespace Hix;

public interface IHixValue : IEquatable<IHixValue> {
  HixValueKind Kind { get; }
  bool IsTruthy(HixThread context);
  HixString Render(HixThread context);
  void Fingerprint(HixFingerprintBuilder builder, HixThread context);
  IHixValue Select(HixThread context, HixString member);
  object Unlink(HixThread context);
}

public sealed record ErrorHixValue(HixString Message, bool IsChecked = false) : IHixValue {
  public HixValueKind Kind => HixValueKind.Error;
  public bool IsTruthy(HixThread context) {
    return false;
  }

  public HixString Render(HixThread context) {
    return Message;
  }

  public void Fingerprint(HixFingerprintBuilder builder, HixThread context) {
    builder.Append(nameof(ErrorHixValue));
    builder.Append(Message, context.Strings);
    builder.Append(IsChecked);
  }

  public IHixValue Select(HixThread context, HixString member) {
    return this;
  }

  public object Unlink(HixThread context) {
    return new ErrorHixValue(HixString.Dynamic(Message.Resolve(context.Strings)), IsChecked);
  }

  public bool Equals(IHixValue other) {
    return other is ErrorHixValue value && Equals(value);
  }
}

public sealed class NullHixValue : IHixValue {
  public static readonly NullHixValue Instance = new();
  private NullHixValue() { }
  public HixValueKind Kind => HixValueKind.Null;

  public bool IsTruthy(HixThread context) {
    return false;
  }

  public HixString Render(HixThread context) {
    return HixString.Dynamic("null");
  }

  public void Fingerprint(HixFingerprintBuilder builder, HixThread context) {
    builder.Append(nameof(NullHixValue));
  }

  public IHixValue Select(HixThread context, HixString member) {
    return this;
  }

  public object Unlink(HixThread context) {
    return null;
  }

  public bool Equals(IHixValue other) {
    return other is NullHixValue;
  }

  public override bool Equals(object obj) {
    return obj is NullHixValue;
  }

  public override int GetHashCode() {
    return 0;
  }
}

public sealed record BooleanHixValue(bool Value) : IHixValue {
  public static readonly BooleanHixValue True = new(true), False = new(false);
  public static BooleanHixValue From(bool value) => value ? True : False;
  public HixValueKind Kind => HixValueKind.Bool;

  public bool IsTruthy(HixThread context) {
    return Value;
  }

  public HixString Render(HixThread context) {
    return HixString.Dynamic(Value ? "true" : "false");
  }

  public void Fingerprint(HixFingerprintBuilder builder, HixThread context) {
    builder.Append(nameof(BooleanHixValue));
    builder.Append(Value);
  }

  public IHixValue Select(HixThread context, HixString member) {
    return NullHixValue.Instance;
  }

  public object Unlink(HixThread context) {
    return Value;
  }

  public bool Equals(IHixValue other) {
    return other is BooleanHixValue value && Equals(value);
  }
}

public sealed record ObjectHixValue(object Value) : IHixValue {
  public HixValueKind Kind => HixValueKind.Symbol;
  public bool IsTruthy(HixThread context) {
    return Value is not null && Value is not false;
  }

  public HixString Render(HixThread context) {
    return HixString.Dynamic(Convert.ToString(Value));
  }

  public void Fingerprint(HixFingerprintBuilder builder, HixThread context) {
    builder.Append(nameof(ObjectHixValue));
    builder.Append(Convert.ToString(Value));
  }

  public IHixValue Select(HixThread context, HixString member) {
    return NullHixValue.Instance;
  }

  public object Unlink(HixThread context) {
    return Value;
  }

  public bool Equals(IHixValue other) {
    return other is ObjectHixValue value && Equals(Value, value.Value);
  }
}

/// <summary>A keyed collection. Enumeration order is unspecified and is not part of table identity.</summary>
public class HixTableValue : IHixValue {
  private readonly PersistentMap<HixString, IHixValue> map;
  public IReadOnlyList<KeyValuePair<HixString, IHixValue>> Entries { get; }

  public HixTableValue(IEnumerable<KeyValuePair<HixString, IHixValue>> entries, HixStringPool strings = null) {
    var builder = new PersistentMap<HixString, IHixValue>.Builder();
    foreach (var entry in entries) {
      if (entry.Key.IsInterned && strings == null)
        throw new ArgumentException("A string pool is required to normalize interned table keys", nameof(strings));
      var key = entry.Key.IsInterned ? HixString.Dynamic(entry.Key.Resolve(strings)) : entry.Key;
      builder.SetItem(key, entry.Value);
    }
    map = builder.ToImmutable();
    Entries = new MapEntries(map);
  }

  public HixTableValue(PersistentMap<HixString, IHixValue> map) {
    this.map = map; Entries = new MapEntries(map);
  }

  // Storage views deliberately borrow live entries; ordinary tables always own immutable data.
  protected HixTableValue(IReadOnlyList<KeyValuePair<HixString, IHixValue>> entries, bool storageView) {
    Entries = entries;
  }

  private sealed class MapEntries(PersistentMap<HixString, IHixValue> map) : IReadOnlyList<KeyValuePair<HixString, IHixValue>> {
    private KeyValuePair<HixString, IHixValue>[] indexed;
    public int Count => map.Count;
    public KeyValuePair<HixString, IHixValue> this[int index] => (indexed ??= map.ToArray())[index];
    public IEnumerator<KeyValuePair<HixString, IHixValue>> GetEnumerator() => map.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  private PersistentMap<HixString, IHixValue> ImmutableMap() {
    if (map != null) return map;
    var builder = new PersistentMap<HixString, IHixValue>.Builder();
    foreach (var entry in Entries) builder.SetItem(entry.Key, entry.Value);
    return builder.ToImmutable();
  }
  public static readonly HixTableValue Empty = new([]);
  public int Count => Entries.Count;
  public HixValueKind Kind => HixValueKind.Table;

  public bool IsTruthy(HixThread context) {
    return Count != 0;
  }

  public HixString Render(HixThread context) {
    return HixString.Dynamic(
      string.Join(
        ", ", Entries.Select(item =>
          item.Key.Resolve(context.Strings) + "=" + item.Value.Render(context).Resolve(context.Strings)
        )
      )
    );
  }

  public void Fingerprint(HixFingerprintBuilder builder, HixThread context) {
    builder.Append(nameof(HixTableValue));
    builder.Append(Count);
    foreach (var item in Entries.OrderBy(entry => entry.Key.Resolve(context.Strings), StringComparer.Ordinal)) {
      builder.Append(item.Key, context.Strings);
      item.Value.Fingerprint(builder, context);
    }
  }

  public virtual bool TryGetValue(HixThread context, HixString key, out IHixValue value) =>
    ImmutableMap().TryGetValue(HixString.Dynamic(key.Resolve(context.Strings)), out value);

  public virtual IHixValue Select(HixThread context, HixString member) =>
    TryGetValue(context, member, out var value) ? value : NullHixValue.Instance;

  public object Unlink(HixThread context) {
    return Entries.ToDictionary(
      item => item.Key.Resolve(context.Strings), item => item.Value.Unlink(context), StringComparer.Ordinal
    );
  }

  public bool Equals(IHixValue other) {
    return other is HixTableValue table && Count == table.Count && Entries.All(entry =>
      table.ImmutableMap().TryGetValue(entry.Key, out var value) && entry.Value.Equals(value));
  }

  public override bool Equals(object other) => other is IHixValue value && Equals(value);

  public override int GetHashCode() {
    // Commutative key hashing keeps table identity independent of enumeration order.
    // Values are compared by IHixValue.Equals, whose implementations may use
    // structural equality without matching CLR object hashes (for example tuples).
    var hash = Count;
    foreach (var entry in Entries) hash = unchecked(hash + entry.Key.GetHashCode());
    return hash;
  }

  public HixTableValue Put(HixThread context, HixString key, IHixValue value) {
    var current = ImmutableMap();
    var updated = current.SetItem(HixString.Dynamic(key.Resolve(context.Strings)), value);
    return ReferenceEquals(current, updated) && map != null ? this : new HixTableValue(updated);
  }

  public HixTableValue Remove(HixThread context, HixString key) {
    var current = ImmutableMap();
    var updated = current.Remove(HixString.Dynamic(key.Resolve(context.Strings)));
    return ReferenceEquals(current, updated) && map != null ? this : new HixTableValue(updated);
  }

  public HixTableValue Push(HixThread context, IHixValue value) {
    return Put(context, context.ResolveString(Count.ToString()), value);
  }

}

public sealed record LiteralHixValue(HixString Value) : IHixValue {
   public HixValueKind Kind => HixValueKind.String;
  public bool IsTruthy(HixThread context) {
    return !string.IsNullOrEmpty(Value.Resolve(context.Strings));
  }

  public HixString Render(HixThread context) {
    return Value;
  }

  public void Fingerprint(HixFingerprintBuilder builder, HixThread context) {
    builder.Append(nameof(LiteralHixValue));
    builder.Append(Value, context.Strings);
  }

  public IHixValue Select(HixThread context, HixString member) {
    return NullHixValue.Instance;
  }

  public object Unlink(HixThread context) {
    return Value.Resolve(context.Strings);
  }

  public bool Equals(IHixValue other) {
    return other is LiteralHixValue value && Equals(value);
  }
}
