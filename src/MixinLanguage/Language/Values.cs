using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;
using Mixins.Compiler;

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

public sealed record MixinTableValue(IReadOnlyList<KeyValuePair<MixinString, IMixinValue>> Entries) : IMixinValue {
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
    foreach (var item in Entries) {
      builder.Append(item.Key.Resolve(context.Strings));
      item.Value.Fingerprint(builder, context);
    }
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    var name = member.Resolve(context.Strings);
    return Entries.FirstOrDefault(item => string.Equals(
        item.Key.Resolve(context.Strings), name, StringComparison.Ordinal
      )
    ).Value ?? NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Entries.ToDictionary(
      item => item.Key.Resolve(context.Strings), item => item.Value.Unlink(context), StringComparer.Ordinal
    );
  }

  public bool Equals(IMixinValue other) {
    return other is MixinTableValue table && Entries.SequenceEqual(table.Entries);
  }

  public MixinTableValue Put(ExecutionContext context, MixinString key, IMixinValue value) {
    var name = key.Resolve(context.Strings);
    return new MixinTableValue(
      [
        .. Entries.Where(item => !string.Equals(
            item.Key.Resolve(context.Strings), name, StringComparison.Ordinal
          )
        ),
        new KeyValuePair<MixinString, IMixinValue>(key, value)
      ]
    );
  }

  public MixinTableValue Remove(ExecutionContext context, MixinString key) {
    var name = key.Resolve(context.Strings);
    return new MixinTableValue(
      [
        .. Entries.Where(item => !string.Equals(
            item.Key.Resolve(context.Strings), name, StringComparison.Ordinal
          )
        )
      ]
    );
  }

  public MixinTableValue Push(ExecutionContext context, IMixinValue value) {
    return Put(context, context.ResolveString(Count.ToString()), value);
  }

  public MixinTableValue Pop() {
    return Count == 0 ? this : new MixinTableValue([.. Entries.Take(Count - 1)]);
  }
}
