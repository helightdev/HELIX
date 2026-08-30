using System;
using System.Collections.Generic;
using System.Linq;
using HelixSourceGenerator.Language.Compiler;
using HelixSourceGenerator.Language.Functions;

namespace HelixSourceGenerator.Language;

public enum MixinExpressionRoot {
  Target,
  This,
  Attribute,
  Argument,
  Variable,
  Local,
  True,
  False,
  Null,
  Table,
  Parameter,
  Carry
}

public enum MixinExpressionOutputTarget {
  Target,
  Class,
  File,
  Extends,
  Implements,
  Injection,
  Annotation,
  Using,
  Mixin
}

public readonly record struct MixinSourceLocation(int Program, int Line);

public sealed record MixinExpressionLog(string Text = "", int Line = -1, bool IsHint = false);

public sealed record MixinExpressionPreparedLog(string Text, int Line, int ProgramIndex);

public sealed record MixinExpressionValidationResult(bool Success, string Error, int ErrorLine);

public sealed class MixinExpressionOutput {
  internal MixinExpressionOutput(
    MixinExpressionOutputTarget target, IReadOnlyList<MixinString> segments,
    MixinStringPool strings, MixinString injectionTarget = default, int injectionPriority = 0
  ) {
    Target = target;
    Segments = segments ?? [];
    Strings = strings;
    InjectionTargetValue = injectionTarget;
    InjectionPriority = injectionPriority;
  }

  public MixinExpressionOutput(
    MixinExpressionOutputTarget target, string text, string injectionTarget = null,
    int injectionPriority = 0
  ) {
    var strings = new MixinStringPoolBuilder().Freeze();
    Target = target;
    Strings = strings;
    Segments = [MixinString.Dynamic(text ?? "")];
    InjectionTargetValue = MixinString.Dynamic(injectionTarget ?? "");
    InjectionPriority = injectionPriority;
  }

  public MixinExpressionOutputTarget Target { get; }
  public string Text => string.Concat(Segments.Select(item => item.Resolve(Strings)));
  public string InjectionTarget => InjectionTargetValue.Resolve(Strings);
  public int InjectionPriority { get; }
  internal IReadOnlyList<MixinString> Segments { get; }
  internal MixinStringPool Strings { get; }
  internal MixinString InjectionTargetValue { get; }
  internal bool IsEmpty => Segments.All(item => string.IsNullOrEmpty(item.Resolve(Strings)));

  internal string Resolve(MixinString value) {
    return value.Resolve(Strings);
  }

  internal MixinExpressionOutput Retarget(MixinExpressionOutputTarget target) {
    return new MixinExpressionOutput(target, Segments, Strings, InjectionTargetValue, InjectionPriority);
  }
}

public sealed class MixinExpressionResult {
  internal MixinExpressionResult(
    bool success, string error, int errorLine,
    IReadOnlyList<MixinExpressionOutput> outputs, IReadOnlyList<MixinExpressionLog> logs = null,
    IReadOnlyDictionary<string, object> variables = null, int executedOperations = 0,
    double executionMilliseconds = 0
  ) {
    Success = success;
    Error = error;
    ErrorLine = errorLine;
    Outputs = outputs ?? [];
    Logs = logs ?? [];
    Variables = variables ?? new Dictionary<string, object>();
    ExecutedOperations = executedOperations;
    ExecutionMilliseconds = executionMilliseconds;
  }

  public bool Success { get; }
  public string Error { get; }
  public int ErrorLine { get; }
  public IReadOnlyList<MixinExpressionOutput> Outputs { get; }
  public IReadOnlyList<MixinExpressionLog> Logs { get; }
  public IReadOnlyDictionary<string, object> Variables { get; }
  public int ExecutedOperations { get; }
  public double ExecutionMilliseconds { get; }
}

public abstract class ExecutionContext {
  protected ExecutionContext(MixinStringPool strings) {
    Strings = strings ?? throw new ArgumentNullException(nameof(strings));
  }

  public MixinStringPool Strings { get; internal set; }
  internal MixinValueDictionary Locals { get; } = new();
  internal MixinValueDictionary Variables { get; } = new();
  internal MixinValueDictionary Carries { get; } = new();
  internal IMixinValue Parameter { get; set; } = NullMixinValue.Instance;
  internal Func<ProgramFunctionMixinValue, IMixinValue, IMixinValue> ProgramInvoker { get; set; }

  public IMixinValue Resolve(MixinExpressionRoot root, MixinString member) {
    switch (root) {
      case MixinExpressionRoot.True: return BooleanMixinValue.True;
      case MixinExpressionRoot.False: return BooleanMixinValue.False;
      case MixinExpressionRoot.Null: return NullMixinValue.Instance;
      case MixinExpressionRoot.Local:
        return Locals.TryGetValue(member, out var local)
          ? local
          : NullMixinValue.Instance;
      case MixinExpressionRoot.Variable:
        return Variables.TryGetValue(member, out var variable)
          ? variable
          : NullMixinValue.Instance;
      case MixinExpressionRoot.Parameter:
        return string.IsNullOrEmpty(member.Resolve(Strings))
          ? Parameter
          : Parameter.Select(this, member);
      case MixinExpressionRoot.Table: return MixinTableValue.Empty;
      case MixinExpressionRoot.Carry:
        return Carries.TryGetValue(member, out var carry)
          ? carry
          : Error("unknown carried value");
      default: return ResolveHost(root, member);
    }
  }

  protected abstract IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member);

  public IMixinValue Invoke(
    FunctionDefinition function, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  ) {
    return function.Invoke(this, instance, arguments, negated);
  }

  public IMixinValue Evaluate(IMixinValue value) {
    return value switch {
      RootMixinValue root => Evaluate(root.Resolve(this)),
      InvokeMixinValue invocation => Evaluate(invocation.Resolve(this)),
      InterpolationMixinValue interpolation => EvaluateInterpolation(interpolation),
      AllMixinValue all => EvaluateAll(all),
      TableTransformMixinValue transform => EvaluateTransform(transform),
      _ => value ?? NullMixinValue.Instance
    };
  }

  private IMixinValue EvaluateInterpolation(InterpolationMixinValue interpolation) {
    var parts = interpolation.Parts.Select(Evaluate).ToArray();
    var error = parts.OfType<ErrorMixinValue>().FirstOrDefault();
    return error is not null
      ? error
      : new LiteralMixinValue(
        Dynamic(
          string.Concat(
            parts.Select(item => item.Render(this).Resolve(Strings))
          )
        )
      );
  }

  private IMixinValue EvaluateTransform(TableTransformMixinValue transform) {
    if (ProgramInvoker is null) return Error("program function invocation is not available");
    var entries = new List<KeyValuePair<MixinString, IMixinValue>>();
    foreach (var item in transform.Table.Entries) {
      var parameter = transform.Kind == TableTransformKind.MapValues
        ? item.Value
        : new MixinTableValue(
          [
            new KeyValuePair<MixinString, IMixinValue>(ResolveString("k"), new LiteralMixinValue(item.Key)),
            new KeyValuePair<MixinString, IMixinValue>(ResolveString("v"), item.Value)
          ]
        );
      var result = ProgramInvoker(transform.Function, parameter);
      if (result is ErrorMixinValue) return result;
      if (transform.Kind == TableTransformKind.Filter) {
        if (result.IsTruthy(this)) entries.Add(item);
      } else entries.Add(new KeyValuePair<MixinString, IMixinValue>(item.Key, result));
    }
    return new MixinTableValue(entries);
  }

  private IMixinValue EvaluateAll(AllMixinValue all) {
    foreach (var item in all.Values) {
      var value = Evaluate(item);
      if (value is ErrorMixinValue) return value;
      if (!value.IsTruthy(this)) return BooleanMixinValue.False;
    }
    return all.Values.Count == 0 ? BooleanMixinValue.False : BooleanMixinValue.True;
  }

  public virtual MixinString Render(IMixinValue value) {
    return value.Render(this);
  }

  public virtual MixinString NameOf(IMixinValue value) {
    return value is DetachedSemanticMixinValue detached
      ? detached.TypeName
      : value.Render(this);
  }

  public virtual IMixinValue Unwrap(IMixinValue value) {
    return value;
  }

  public virtual bool IsType(IMixinValue value, MixinString type) {
    if (value is not DetachedSemanticMixinValue detached) return false;
    var expected = type.Resolve(Strings).Replace("global::", "");
    var candidate = detached.Render(this).Resolve(Strings);
    return candidate == expected || detached.TypeName.Resolve(Strings) == expected;
  }

  public virtual IMixinValue Attributes(IMixinValue value, MixinString type, bool exact, bool first) {
    return first ? NullMixinValue.Instance : MixinTableValue.Empty;
  }

  public virtual IMixinValue ResolveMixin(MixinString local, IMixinValue operand) {
    return Error("mixin resolution is not supported by this execution context");
  }

  public virtual IMixinValue CreatePropStruct(IReadOnlyList<IMixinValue> arguments, IMixinValue operand) {
    return Error("prop structs are not supported by this execution context");
  }

  public virtual IMixinValue AugmentPropStruct(MixinString local, IMixinValue operand) {
    return Error("struct augmentation is not supported by this execution context");
  }

  public virtual bool HasTrait(IMixinValue value, MixinString trait) {
    return false;
  }

  internal virtual object UnlinkSnapshot(IMixinValue value) {
    return value.Unlink(this);
  }

  internal virtual IMixinValue DetachValue(IMixinValue value) {
    value = Evaluate(value);
    return value switch {
      MixinTableValue table => new MixinTableValue(
        [
          .. table.Entries.Select(item =>
            new KeyValuePair<MixinString, IMixinValue>(item.Key, DetachValue(item.Value))
          )
        ]
      ),
      DirectiveEffectMixinValue effect => effect with { Value = DetachValue(effect.Value) },
      _ => value
    };
  }

  public MixinString ResolveString(string value) {
    // Runtime pools are immutable. Reuse a compile-time string when possible,
    // but never grow the pool with data discovered while executing a mixin.
    return Strings.Get(value);
  }

  public static MixinString Dynamic(string value) {
    return MixinString.Dynamic(value ?? "");
  }

  public ErrorMixinValue Error(string value) {
    return new ErrorMixinValue(Dynamic(value));
  }
}

public interface IMixinValue : IEquatable<IMixinValue> {
  bool IsTruthy(ExecutionContext context);
  MixinString Render(ExecutionContext context);
  void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context);
  IMixinValue Select(ExecutionContext context, MixinString member);
  object Unlink(ExecutionContext context);
}

public sealed record ErrorMixinValue(MixinString Message) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return false;
  }

  public MixinString Render(ExecutionContext context) {
    return Message;
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(ErrorMixinValue));
    builder.Append(Message.Resolve(context.Strings));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return this;
  }

  public object Unlink(ExecutionContext context) {
    return Message.Resolve(context.Strings);
  }

  public bool Equals(IMixinValue other) {
    return other is ErrorMixinValue value && Equals(value);
  }
}

public sealed class NullMixinValue : IMixinValue {
  public static readonly NullMixinValue Instance = new();
  private NullMixinValue() { }

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
    return Entries.FirstOrDefault(item => item.Key == member).Value ?? NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Entries.ToDictionary(
      item => item.Key.Resolve(context.Strings), item => item.Value.Unlink(context), StringComparer.Ordinal
    );
  }

  public bool Equals(IMixinValue other) {
    return other is MixinTableValue table && Entries.SequenceEqual(table.Entries);
  }

  public MixinTableValue Put(MixinString key, IMixinValue value) {
    return new MixinTableValue(
      [.. Entries.Where(item => item.Key != key), new KeyValuePair<MixinString, IMixinValue>(key, value)]
    );
  }

  public MixinTableValue Remove(MixinString key) {
    return new MixinTableValue([.. Entries.Where(item => item.Key != key)]);
  }

  public MixinTableValue Push(ExecutionContext context, IMixinValue value) {
    return Put(context.ResolveString(Count.ToString()), value);
  }

  public MixinTableValue Pop() {
    return Count == 0 ? this : new MixinTableValue([.. Entries.Take(Count - 1)]);
  }
}

public abstract class FunctionDefinition {
  protected FunctionDefinition(string name, int minimumArguments, int maximumArguments) {
    Name = name;
    MinimumArguments = minimumArguments;
    MaximumArguments = maximumArguments;
  }

  public string Name { get; }
  public int MinimumArguments { get; }
  public int MaximumArguments { get; }
  public virtual bool IsPredicate => false;

  public abstract IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  );
}

internal static class FunctionLibrary {
  private static readonly IReadOnlyDictionary<string, FunctionDefinition> Definitions =
    CreateDefinitions();

  private static IReadOnlyDictionary<string, FunctionDefinition> CreateDefinitions() {
    using var profile = Shared.MixinProfiler.Measure("static.function_library");
    return new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal) {
      ["name"] = new NameFunction(), ["type"] = new TypeFunction(), ["fullName"] = new FullNameFunction(),
      ["makeGeneric"] = new MakeGenericFunction(), ["visibility"] = new VisibilityFunction(),
      ["path"] = new PathFunction(), ["unwrap"] = new UnwrapFunction(), ["switch"] = new SwitchFunction(),
      ["size"] = new SizeFunction(), ["replace"] = new ReplaceFunction(), ["replaceFirst"] = new ReplaceFirstFunction(),
      ["floatTime"] = new FloatTimeFunction(), ["table"] = new AsTableFunction(), ["put"] = new PutFunction(),
      ["remove"] = new RemoveFunction(), ["push"] = new PushFunction(), ["pop"] = new PopFunction(),
      ["joinKeys"] = new JoinKeysFunction(), ["joinValues"] = new JoinValuesFunction(),
      ["join"] = new JoinEntriesFunction(), ["mapValues"] = new MapValuesFunction(), ["map"] = new MapFunction(),
      ["filter"] = new FilterFunction(), ["attributes"] = new AttributesFunction(),
      ["attributesOf"] = new AttributesOfFunction(), ["attributesOfExact"] = new AttributesOfExactFunction(),
      ["attributeOf"] = new AttributeOfFunction(), ["wire"] = new WireFunction(),
      ["structParams"] = new StructParamsFunction(), ["structArgs"] = new StructArgsFunction(),
      ["propStructCall"] = new PropStructCallFunction(), ["signature"] = new SignaturePredicate(),
      ["wireable"] = new WireablePredicate(), ["structHasEquality"] = new StructHasEqualityPredicate(),
      ["structNoArgs"] = new StructNoArgsPredicate(), ["structAugment"] = new StructAugmentPredicate(),
      ["exists"] = new ExistsPredicate(), ["and"] = new AndPredicate(), ["or"] = new OrPredicate(),
      ["is"] = new IsPredicate(), ["has"] = new HasPredicate(), ["eq"] = new EqualPredicate(),
      ["matches"] = new MatchesPredicate(), ["isSelf"] = new TraitPredicate("isSelf"),
      ["ref"] = new TraitPredicate("ref"), ["in"] = new TraitPredicate("in"), ["out"] = new TraitPredicate("out"),
      ["inout"] = new TraitPredicate("inout"), ["argument"] = new TraitPredicate("argument"),
      ["static"] = new TraitPredicate("static"), ["async"] = new TraitPredicate("async"),
      ["public"] = new TraitPredicate("public"), ["exposed"] = new TraitPredicate("exposed"),
      ["top"] = new TraitPredicate("top"), ["concrete"] = new TraitPredicate("concrete"),
      ["partial"] = new TraitPredicate("partial"), ["generic"] = new TraitPredicate("generic"),
      ["struct"] = new TraitPredicate("struct"), ["class"] = new TraitPredicate("class")
    };
  }

  internal static bool TryGet(string name, out FunctionDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }

  internal static bool IsPredicate(string name) {
    return TryGet(name, out var definition) && definition.IsPredicate;
  }

  internal static void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var name in Definitions.Keys) pool.Intern(name);
  }
}
