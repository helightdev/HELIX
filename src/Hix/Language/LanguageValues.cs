using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hix.Runtime;

namespace Hix;

public sealed record NumberHixValue(double Value) : IHixValue {
  public HixValueKind Kind => HixValueKind.Number;
  public bool IsTruthy(HixCoroutine context) => Value != 0;
  public HixString Render(HixCoroutine context) => HixString.Dynamic(Value.ToString("R", CultureInfo.InvariantCulture));
  public void Fingerprint(HixFingerprintBuilder builder, HixCoroutine context) {
    builder.Append(nameof(NumberHixValue));
    builder.Append(Value.ToString("R", CultureInfo.InvariantCulture));
  }
  public IHixValue Select(HixCoroutine context, HixString member) => NullHixValue.Instance;
  public object Unlink(HixCoroutine context) => Value;
  public bool Equals(IHixValue other) => other is NumberHixValue number && Value.Equals(number.Value);
}

public sealed record TupleHixValue(IReadOnlyList<IHixValue> Values) : IHixValue {
  public static readonly TupleHixValue Empty = new(Array.Empty<IHixValue>());

  public TupleHixValue Append(IHixValue value) {
    var values = new IHixValue[Values.Count + 1];
    for (var i = 0; i < Values.Count; i++) values[i] = Values[i];
    values[Values.Count] = value;
    return new TupleHixValue(values);
  }

  public TupleHixValue RemoveLast() {
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
  public bool IsTruthy(HixCoroutine context) => Values.Count != 0;
  public HixString Render(HixCoroutine context) => HixString.Dynamic(
    string.Join(", ", Values.Select(value => value.Render(context).Resolve(context.Strings))));
  public void Fingerprint(HixFingerprintBuilder builder, HixCoroutine context) {
    builder.Append(nameof(TupleHixValue));
    builder.Append(Values.Count);
    foreach (var value in Values) value.Fingerprint(builder, context);
  }
  public IHixValue Select(HixCoroutine context, HixString member) =>
    int.TryParse(member.Resolve(context.Strings), NumberStyles.None, CultureInfo.InvariantCulture, out var index) &&
    index >= 0 && index < Values.Count ? Values[index] : MissingHixValue.Instance;
  public object Unlink(HixCoroutine context) => Values.Select(value => value.Unlink(context)).ToArray();
  public bool Equals(IHixValue other) => other is TupleHixValue tuple && Values.SequenceEqual(tuple.Values);
}

public sealed record KindHixValue(HixValueKind ValueKind) : IHixValue {
  private static readonly IReadOnlyDictionary<string, KindHixValue> Kinds =
    Enum.GetValues(typeof(HixValueKind)).Cast<HixValueKind>().Where(kind => kind != HixValueKind.Any)
      .ToDictionary(kind => kind.ToString().ToLowerInvariant(), kind => new KindHixValue(kind), StringComparer.Ordinal);
  public HixString Name { get; } = HixString.Dynamic(ValueKind.ToString().ToLowerInvariant());
  public HixValueKind Kind => HixValueKind.Kind;
  public static KindHixValue Get(HixValueKind kind) => Kinds[kind.ToString().ToLowerInvariant()];
  public static bool TryGet(string name, out KindHixValue kind) => Kinds.TryGetValue(name, out kind);
  public bool IsTruthy(HixCoroutine context) => true;
  public HixString Render(HixCoroutine context) => Name;
  public void Fingerprint(HixFingerprintBuilder builder, HixCoroutine context) {
    builder.Append(nameof(KindHixValue));
    builder.Append(Name, context.Strings);
  }
  public IHixValue Select(HixCoroutine context, HixString member) => NullHixValue.Instance;
  public object Unlink(HixCoroutine context) => this;
  public bool Equals(IHixValue other) => other is KindHixValue kind && Name == kind.Name;
}

public sealed record NamedFunctionHixValue(HixString Name) : IHixValue {
  public LanguageFunctionScope Scope { get; init; }
  public HixValueKind Kind => HixValueKind.Function;
  public bool IsTruthy(HixCoroutine context) => true;
  public HixString Render(HixCoroutine context) => HixString.Dynamic("<function " + Name.Resolve(Scope?.Program.StringPool ?? context.Strings) + ">");
  public void Fingerprint(HixFingerprintBuilder builder, HixCoroutine context) {
    builder.Append(nameof(NamedFunctionHixValue));
    builder.Append(Name, Scope?.Program.StringPool ?? context.Strings);
    Scope?.Fingerprint(builder);
  }
  public IHixValue Select(HixCoroutine context, HixString member) => NullHixValue.Instance;
  public object Unlink(HixCoroutine context) => this;
  public bool Equals(IHixValue other) => other is NamedFunctionHixValue function && Name == function.Name &&
    ReferenceEquals(Scope, function.Scope);
}

/// <summary>A signature-only function reference in a compiled constant pool.</summary>
public record FunctionReferenceHixValue(SignatureHixPattern Signature) : IHixValue {
  public string DeclarationSignature { get; init; }
  public HixValueKind Kind => HixValueKind.Function;
  public bool IsTruthy(HixCoroutine context) => true;
  public HixString Render(HixCoroutine context) => HixString.Dynamic(Signature.Display);
  public virtual void Fingerprint(HixFingerprintBuilder builder, HixCoroutine context) {
    builder.Append(HixGenericTypes.SignatureIdentity(Signature));
    if (DeclarationSignature != null) builder.Append(DeclarationSignature);
  }
  public IHixValue Select(HixCoroutine context, HixString member) => NullHixValue.Instance;
  public object Unlink(HixCoroutine context) => this;
  public bool Equals(IHixValue other) => Equals(other as FunctionReferenceHixValue);
}

/// <summary>A function reference resolved against the loaded image and backend.</summary>
public sealed record ResolvedFunctionHixValue : FunctionReferenceHixValue {
  public LanguageFunctionCandidate Language { get; }
  public HixFunctionEntry Backend { get; }
  public FunctionSignature BackendSignature { get; }

  public ResolvedFunctionHixValue(SignatureHixPattern signature, LanguageFunctionCandidate language)
    : base(signature) { Language = language ?? throw new ArgumentNullException(nameof(language)); }

  public ResolvedFunctionHixValue(SignatureHixPattern signature, HixFunctionEntry backend,
    FunctionSignature backendSignature) : base(signature) {
    Backend = backend ?? throw new ArgumentNullException(nameof(backend));
    BackendSignature = backendSignature ?? throw new ArgumentNullException(nameof(backendSignature));
  }

  public override void Fingerprint(HixFingerprintBuilder builder, HixCoroutine context) {
    base.Fingerprint(builder, context);
    Language?.Owner.Fingerprint(builder);
  }
}
