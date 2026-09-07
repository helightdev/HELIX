using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mixins.Runtime;

namespace Mixins;

public sealed record NumberMixinValue(double Value) : IMixinValue {
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
  public static bool TryGet(string name, out KindMixinValue kind) => Kinds.TryGetValue(name, out kind);
  public static KindMixinValue Of(IMixinValue value) => Kinds[value switch {
    LiteralMixinValue => "string", NumberMixinValue => "number", BooleanMixinValue => "bool",
    ErrorMixinValue => "error", NullMixinValue => "null", MixinTableValue => "table", TupleMixinValue => "tuple",
    KindMixinValue => "kind", NamedFunctionMixinValue => "function", _ => "symbol"
  }];
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
  internal Compiler.LanguageFunctionScope Scope { get; init; }
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
