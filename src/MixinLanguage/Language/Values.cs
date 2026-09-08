using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;
using Mixins.Compiler;
using Mixins.Collections;
using System.Collections;

namespace Mixins;

public interface IMixinValue : IEquatable<IMixinValue> {
  MixinValueKind Kind { get; }
  bool IsTruthy(ExecutionContext context);
  MixinString Render(ExecutionContext context);
  void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context);
  IMixinValue Select(ExecutionContext context, MixinString member);
  object Unlink(ExecutionContext context);
}

public sealed record ErrorMixinValue(MixinString Message, bool IsChecked = false) : IMixinValue {
  public MixinValueKind Kind => MixinValueKind.Error;
  public bool IsTruthy(ExecutionContext context) {
    return false;
  }

  public MixinString Render(ExecutionContext context) {
    return Message;
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(ErrorMixinValue));
    builder.Append(Message.Resolve(context.Strings));
    builder.Append(IsChecked);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return this;
  }

  public object Unlink(ExecutionContext context) {
    return new ErrorMixinValue(MixinString.Dynamic(Message.Resolve(context.Strings)), IsChecked);
  }

  public bool Equals(IMixinValue other) {
    return other is ErrorMixinValue value && Equals(value);
  }
}

public sealed class NullMixinValue : IMixinValue {
  public static readonly NullMixinValue Instance = new();
  private NullMixinValue() { }
  public MixinValueKind Kind => MixinValueKind.Null;

  public bool IsTruthy(ExecutionContext context) {
    return false;
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic("null");
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(NullMixinValue));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return this;
  }

  public object Unlink(ExecutionContext context) {
    return null;
  }

  public bool Equals(IMixinValue other) {
    return other is NullMixinValue;
  }

  public override bool Equals(object obj) {
    return obj is NullMixinValue;
  }

  public override int GetHashCode() {
    return 0;
  }
}

public sealed record BooleanMixinValue(bool Value) : IMixinValue {
  public static readonly BooleanMixinValue True = new(true), False = new(false);
  public static BooleanMixinValue From(bool value) => value ? True : False;
  public MixinValueKind Kind => MixinValueKind.Bool;

  public bool IsTruthy(ExecutionContext context) {
    return Value;
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic(Value ? "true" : "false");
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(BooleanMixinValue));
    builder.Append(Value);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Value;
  }

  public bool Equals(IMixinValue other) {
    return other is BooleanMixinValue value && Equals(value);
  }
}

public sealed record ObjectMixinValue(object Value) : IMixinValue {
  public MixinValueKind Kind => MixinValueKind.Symbol;
  public bool IsTruthy(ExecutionContext context) {
    return Value is not null && Value is not false;
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic(Convert.ToString(Value));
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(ObjectMixinValue));
    builder.Append(Convert.ToString(Value));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Value;
  }

  public bool Equals(IMixinValue other) {
    return other is ObjectMixinValue value && Equals(Value, value.Value);
  }
}

/// <summary>A keyed collection. Enumeration order is unspecified and is not part of table identity.</summary>
public class MixinTableValue : IMixinValue {
  private readonly PersistentMap<MixinString, IMixinValue> map;
  public IReadOnlyList<KeyValuePair<MixinString, IMixinValue>> Entries { get; }

  public MixinTableValue(IEnumerable<KeyValuePair<MixinString, IMixinValue>> entries, MixinStringPool strings = null) {
    var builder = new PersistentMap<MixinString, IMixinValue>.Builder();
    foreach (var entry in entries) {
      if (entry.Key.IsInterned && strings == null)
        throw new ArgumentException("A string pool is required to normalize interned table keys", nameof(strings));
      var key = entry.Key.IsInterned ? MixinString.Dynamic(entry.Key.Resolve(strings)) : entry.Key;
      builder.SetItem(key, entry.Value);
    }
    map = builder.ToImmutable();
    Entries = new MapEntries(map);
  }

  private MixinTableValue(PersistentMap<MixinString, IMixinValue> map) {
    this.map = map; Entries = new MapEntries(map);
  }

  // Storage views deliberately borrow live entries; ordinary tables always own immutable data.
  protected MixinTableValue(IReadOnlyList<KeyValuePair<MixinString, IMixinValue>> entries, bool storageView) {
    Entries = entries;
  }

  private sealed class MapEntries(PersistentMap<MixinString, IMixinValue> map) : IReadOnlyList<KeyValuePair<MixinString, IMixinValue>> {
    private KeyValuePair<MixinString, IMixinValue>[] indexed;
    public int Count => map.Count;
    public KeyValuePair<MixinString, IMixinValue> this[int index] => (indexed ??= map.ToArray())[index];
    public IEnumerator<KeyValuePair<MixinString, IMixinValue>> GetEnumerator() => map.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }

  private PersistentMap<MixinString, IMixinValue> ImmutableMap() {
    if (map != null) return map;
    var builder = new PersistentMap<MixinString, IMixinValue>.Builder();
    foreach (var entry in Entries) builder.SetItem(entry.Key, entry.Value);
    return builder.ToImmutable();
  }
  public static readonly MixinTableValue Empty = new([]);
  public int Count => Entries.Count;
  public MixinValueKind Kind => MixinValueKind.Table;

  public bool IsTruthy(ExecutionContext context) {
    return Count != 0;
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic(
      string.Join(
        ", ", Entries.Select(item =>
          item.Key.Resolve(context.Strings) + "=" + item.Value.Render(context).Resolve(context.Strings)
        )
      )
    );
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(MixinTableValue));
    builder.Append(Count);
    foreach (var item in Entries.OrderBy(entry => entry.Key.Resolve(context.Strings), StringComparer.Ordinal)) {
      builder.Append(item.Key.Resolve(context.Strings));
      item.Value.Fingerprint(builder, context);
    }
  }

  public virtual bool TryGetValue(ExecutionContext context, MixinString key, out IMixinValue value) =>
    ImmutableMap().TryGetValue(ExecutionContext.Dynamic(key.Resolve(context.Strings)), out value);

  public virtual IMixinValue Select(ExecutionContext context, MixinString member) =>
    TryGetValue(context, member, out var value) ? value : NullMixinValue.Instance;

  public object Unlink(ExecutionContext context) {
    return Entries.ToDictionary(
      item => item.Key.Resolve(context.Strings), item => item.Value.Unlink(context), StringComparer.Ordinal
    );
  }

  public bool Equals(IMixinValue other) {
    return other is MixinTableValue table && Count == table.Count && Entries.All(entry =>
      table.ImmutableMap().TryGetValue(entry.Key, out var value) && entry.Value.Equals(value));
  }

  public override bool Equals(object other) => other is IMixinValue value && Equals(value);

  public override int GetHashCode() {
    // Commutative key hashing keeps table identity independent of enumeration order.
    // Values are compared by IMixinValue.Equals, whose implementations may use
    // structural equality without matching CLR object hashes (for example tuples).
    var hash = Count;
    foreach (var entry in Entries) hash = unchecked(hash + entry.Key.GetHashCode());
    return hash;
  }

  public MixinTableValue Put(ExecutionContext context, MixinString key, IMixinValue value) {
    var current = ImmutableMap();
    var updated = current.SetItem(ExecutionContext.Dynamic(key.Resolve(context.Strings)), value);
    return ReferenceEquals(current, updated) && map != null ? this : new MixinTableValue(updated);
  }

  public MixinTableValue Remove(ExecutionContext context, MixinString key) {
    var current = ImmutableMap();
    var updated = current.Remove(ExecutionContext.Dynamic(key.Resolve(context.Strings)));
    return ReferenceEquals(current, updated) && map != null ? this : new MixinTableValue(updated);
  }

  public MixinTableValue Push(ExecutionContext context, IMixinValue value) {
    return Put(context, context.ResolveString(Count.ToString()), value);
  }

}
