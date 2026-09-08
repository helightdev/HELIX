using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mixins.Runtime;

namespace Mixins;

public sealed record NumberMixinValue(double Value) : IMixinValue {
  public MixinValueKind Kind => MixinValueKind.Number;
  public bool IsTruthy(ExecutionContext context) => Value != 0;
  public MixinString Render(ExecutionContext context) => ExecutionContext.Dynamic(Value.ToString("R", CultureInfo.InvariantCulture));
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(NumberMixinValue));
    builder.Append(Value.ToString("R", CultureInfo.InvariantCulture));
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) => NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => Value;
  public bool Equals(IMixinValue other) => other is NumberMixinValue number && Value.Equals(number.Value);
}

public sealed record TupleMixinValue(IReadOnlyList<IMixinValue> Values) : IMixinValue {
  public static readonly TupleMixinValue Empty = new(Array.Empty<IMixinValue>());

  internal TupleMixinValue Append(IMixinValue value) {
    var values = new IMixinValue[Values.Count + 1];
    for (var i = 0; i < Values.Count; i++) values[i] = Values[i];
    values[Values.Count] = value;
    return new TupleMixinValue(values);
  }

  internal TupleMixinValue RemoveLast() {
    if (Values.Count <= 1) return Empty;
    var values = new IMixinValue[Values.Count - 1];
    for (var i = 0; i < values.Length; i++) values[i] = Values[i];
    return new TupleMixinValue(values);
  }

  // Allocate only once an element changes; unchanged immutable tuples can be shared.
  internal TupleMixinValue Transform(Func<IMixinValue, IMixinValue> transform) {
    IMixinValue[] values = null;
    for (var i = 0; i < Values.Count; i++) {
      var value = transform(Values[i]);
      if (values == null && !ReferenceEquals(value, Values[i])) {
        values = new IMixinValue[Values.Count];
        for (var previous = 0; previous < i; previous++) values[previous] = Values[previous];
      }
      if (values != null) values[i] = value;
    }
    return values == null ? this : new TupleMixinValue(values);
  }

  public MixinValueKind Kind => MixinValueKind.Tuple;
  public bool IsTruthy(ExecutionContext context) => Values.Count != 0;
  public MixinString Render(ExecutionContext context) => ExecutionContext.Dynamic(
    string.Join(", ", Values.Select(value => value.Render(context).Resolve(context.Strings))));
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(TupleMixinValue));
    builder.Append(Values.Count);
    foreach (var value in Values) value.Fingerprint(builder, context);
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) =>
    int.TryParse(member.Resolve(context.Strings), NumberStyles.None, CultureInfo.InvariantCulture, out var index) &&
    index >= 0 && index < Values.Count ? Values[index] : NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => Values.Select(value => value.Unlink(context)).ToArray();
  public bool Equals(IMixinValue other) => other is TupleMixinValue tuple && Values.SequenceEqual(tuple.Values);
}

public sealed record KindMixinValue(string Name) : IMixinValue {
  private static readonly IReadOnlyDictionary<string, KindMixinValue> Kinds =
    Enum.GetValues(typeof(MixinValueKind)).Cast<MixinValueKind>().Where(kind => kind != MixinValueKind.Any)
      .ToDictionary(kind => kind.ToString().ToLowerInvariant(), kind => new KindMixinValue(kind.ToString().ToLowerInvariant()), StringComparer.Ordinal);
  public MixinValueKind ValueKind => (MixinValueKind)Enum.Parse(typeof(MixinValueKind), Name, true);
  public MixinValueKind Kind => MixinValueKind.Kind;
  internal static KindMixinValue Get(MixinValueKind kind) => Kinds[kind.ToString().ToLowerInvariant()];
  public static bool TryGet(string name, out KindMixinValue kind) => Kinds.TryGetValue(name, out kind);
  public bool IsTruthy(ExecutionContext context) => true;
  public MixinString Render(ExecutionContext context) => ExecutionContext.Dynamic(Name);
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(KindMixinValue));
    builder.Append(Name);
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) => NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => this;
  public bool Equals(IMixinValue other) => other is KindMixinValue kind && Name == kind.Name;
}

public sealed record NamedFunctionMixinValue(string Name) : IMixinValue {
  internal LanguageFunctionScope Scope { get; init; }
  public MixinValueKind Kind => MixinValueKind.Function;
  public bool IsTruthy(ExecutionContext context) => true;
  public MixinString Render(ExecutionContext context) => ExecutionContext.Dynamic("<function " + Name + ">");
  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(NamedFunctionMixinValue));
    builder.Append(Name);
    Scope?.Fingerprint(builder);
  }
  public IMixinValue Select(ExecutionContext context, MixinString member) => NullMixinValue.Instance;
  public object Unlink(ExecutionContext context) => this;
  public bool Equals(IMixinValue other) => other is NamedFunctionMixinValue function && Name == function.Name &&
    ReferenceEquals(Scope, function.Scope);
}
