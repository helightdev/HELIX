using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hix.Runtime;

namespace Hix;

public sealed record NumberHixValue(double Value) : IHixValue {
  public HixValueKind Kind => HixValueKind.Number;
  public bool IsTruthy(HixExecutionContext context) => Value != 0;
  public HixString Render(HixExecutionContext context) => HixExecutionContext.Dynamic(Value.ToString("R", CultureInfo.InvariantCulture));
  public void Fingerprint(HixFingerprintBuilder builder, HixExecutionContext context) {
    builder.Append(nameof(NumberHixValue));
    builder.Append(Value.ToString("R", CultureInfo.InvariantCulture));
  }
  public IHixValue Select(HixExecutionContext context, HixString member) => NullHixValue.Instance;
  public object Unlink(HixExecutionContext context) => Value;
  public bool Equals(IHixValue other) => other is NumberHixValue number && Value.Equals(number.Value);
}

public sealed record TupleHixValue(IReadOnlyList<IHixValue> Values) : IHixValue {
  public static readonly TupleHixValue Empty = new(Array.Empty<IHixValue>());

  internal TupleHixValue Append(IHixValue value) {
    var values = new IHixValue[Values.Count + 1];
    for (var i = 0; i < Values.Count; i++) values[i] = Values[i];
    values[Values.Count] = value;
    return new TupleHixValue(values);
  }

  internal TupleHixValue RemoveLast() {
    if (Values.Count <= 1) return Empty;
    var values = new IHixValue[Values.Count - 1];
    for (var i = 0; i < values.Length; i++) values[i] = Values[i];
    return new TupleHixValue(values);
  }

  // Allocate only once an element changes; unchanged immutable tuples can be shared.
  public TupleHixValue Transform(Func<IHixValue, IHixValue> transform) {
    IHixValue[] values = null;
    for (var i = 0; i < Values.Count; i++) {
      var value = transform(Values[i]);
      if (values == null && !ReferenceEquals(value, Values[i])) {
        values = new IHixValue[Values.Count];
        for (var previous = 0; previous < i; previous++) values[previous] = Values[previous];
      }
      if (values != null) values[i] = value;
    }
    return values == null ? this : new TupleHixValue(values);
  }

  public HixValueKind Kind => HixValueKind.Tuple;
  public bool IsTruthy(HixExecutionContext context) => Values.Count != 0;
  public HixString Render(HixExecutionContext context) => HixExecutionContext.Dynamic(
    string.Join(", ", Values.Select(value => value.Render(context).Resolve(context.Strings))));
  public void Fingerprint(HixFingerprintBuilder builder, HixExecutionContext context) {
    builder.Append(nameof(TupleHixValue));
    builder.Append(Values.Count);
    foreach (var value in Values) value.Fingerprint(builder, context);
  }
  public IHixValue Select(HixExecutionContext context, HixString member) =>
    int.TryParse(member.Resolve(context.Strings), NumberStyles.None, CultureInfo.InvariantCulture, out var index) &&
    index >= 0 && index < Values.Count ? Values[index] : NullHixValue.Instance;
  public object Unlink(HixExecutionContext context) => Values.Select(value => value.Unlink(context)).ToArray();
  public bool Equals(IHixValue other) => other is TupleHixValue tuple && Values.SequenceEqual(tuple.Values);
}

public sealed record KindHixValue(string Name) : IHixValue {
  private static readonly IReadOnlyDictionary<string, KindHixValue> Kinds =
    Enum.GetValues(typeof(HixValueKind)).Cast<HixValueKind>().Where(kind => kind != HixValueKind.Any)
      .ToDictionary(kind => kind.ToString().ToLowerInvariant(), kind => new KindHixValue(kind.ToString().ToLowerInvariant()), StringComparer.Ordinal);
  public HixValueKind ValueKind => (HixValueKind)Enum.Parse(typeof(HixValueKind), Name, true);
  public HixValueKind Kind => HixValueKind.Kind;
  internal static KindHixValue Get(HixValueKind kind) => Kinds[kind.ToString().ToLowerInvariant()];
  public static bool TryGet(string name, out KindHixValue kind) => Kinds.TryGetValue(name, out kind);
  public bool IsTruthy(HixExecutionContext context) => true;
  public HixString Render(HixExecutionContext context) => HixExecutionContext.Dynamic(Name);
  public void Fingerprint(HixFingerprintBuilder builder, HixExecutionContext context) {
    builder.Append(nameof(KindHixValue));
    builder.Append(Name);
  }
  public IHixValue Select(HixExecutionContext context, HixString member) => NullHixValue.Instance;
  public object Unlink(HixExecutionContext context) => this;
  public bool Equals(IHixValue other) => other is KindHixValue kind && Name == kind.Name;
}

public sealed record NamedFunctionHixValue(string Name) : IHixValue {
  internal LanguageFunctionScope Scope { get; init; }
  public HixValueKind Kind => HixValueKind.Function;
  public bool IsTruthy(HixExecutionContext context) => true;
  public HixString Render(HixExecutionContext context) => HixExecutionContext.Dynamic("<function " + Name + ">");
  public void Fingerprint(HixFingerprintBuilder builder, HixExecutionContext context) {
    builder.Append(nameof(NamedFunctionHixValue));
    builder.Append(Name);
    Scope?.Fingerprint(builder);
  }
  public IHixValue Select(HixExecutionContext context, HixString member) => NullHixValue.Instance;
  public object Unlink(HixExecutionContext context) => this;
  public bool Equals(IHixValue other) => other is NamedFunctionHixValue function && Name == function.Name &&
    ReferenceEquals(Scope, function.Scope);
}
