using System;
using System.Collections.Generic;
using System.Linq;
using MixinLanguage.Compiler;
using MixinLanguage.Functions;

namespace MixinLanguage;

public enum MixinExpressionRoot {
  Target,
  This,
  Attribute,
  Argument,
  Variable,
  TargetVariable,
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
  internal MixinValueDictionary TargetVariables { get; } = new();
  internal MixinValueDictionary Carries { get; } = new();
  internal IMixinValue Parameter { get; set; } = NullMixinValue.Instance;
  internal Func<ProgramFunctionMixinValue, IMixinValue, ProgramFunctionResult> ProgramInvoker { get; set; }
  internal Action<MixinExpressionOutput> OutputSink { get; set; }
  internal Action<MixinExpressionLog> LogSink { get; set; }

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
      case MixinExpressionRoot.TargetVariable:
        var targetName = member.Resolve(Strings);
        foreach (var item in TargetVariables) {
          if (string.Equals(item.Key.Resolve(Strings), targetName, StringComparison.Ordinal))
            return item.Value;
        }
        return NullMixinValue.Instance;
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
      var invocation = ProgramInvoker(transform.Function, parameter);
      var result = invocation.Value;
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

  public virtual IMixinValue Derive(IMixinValue value) {
    return Error(":derive is not available in this context");
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
      LiteralMixinValue literal => new LiteralMixinValue(
        MixinString.Dynamic(literal.Value.Resolve(Strings))
      ),
      MixinTableValue table => new MixinTableValue(
        [
          .. table.Entries.Select(item =>
            new KeyValuePair<MixinString, IMixinValue>(
              MixinString.Dynamic(item.Key.Resolve(Strings)), DetachValue(item.Value)
            )
          )
        ]
      ),
      DirectiveEffectMixinValue effect => effect with { Value = DetachValue(effect.Value) },
      _ => value
    };
  }

  internal void StoreTargetVariable(MixinString key, IMixinValue value) {
    var name = key.Resolve(Strings);
    var existing = TargetVariables.Keys.FirstOrDefault(item =>
      string.Equals(item.Resolve(Strings), name, StringComparison.Ordinal)
    );
    value = Evaluate(value);
    if (existing.IsInterned || existing.DynamicValue is not null)
      TargetVariables.StoreIsolated(existing, value);
    else TargetVariables.StoreIsolated(ResolveString(name), value);
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
  public MixinLanguageValueKind ReceiverType { get; private set; } = MixinLanguageValueKind.Any;
  public MixinLanguageValueKind ResultType { get; private set; } = MixinLanguageValueKind.Any;
  public IReadOnlyList<MixinLanguageValueKind> ArgumentTypes { get; private set; } = [];
  public string Documentation { get; private set; } = "Transforms the current value.";

  internal FunctionDefinition WithLanguageSignature(
    MixinLanguageValueKind receiver,
    MixinLanguageValueKind result,
    IReadOnlyList<MixinLanguageValueKind> arguments = null,
    string documentation = null
  ) {
    ReceiverType = receiver;
    ResultType = result;
    ArgumentTypes = arguments ?? [];
    if (!string.IsNullOrWhiteSpace(documentation)) Documentation = documentation;
    return this;
  }

  public abstract IMixinValue Invoke(
    ExecutionContext context, IMixinValue instance,
    IReadOnlyList<IMixinValue> arguments, bool negated
  );
}

public static class FunctionLibrary {
  private static readonly IReadOnlyDictionary<string, FunctionDefinition> Definitions =
    CreateDefinitions();

  private static IReadOnlyDictionary<string, FunctionDefinition> CreateDefinitions() {
    using var profile = MixinProfiler.Measure("static.function_library");
    var definitions = new Dictionary<string, FunctionDefinition>(StringComparer.Ordinal) {
      ["name"] = new NameFunction(), ["type"] = new TypeFunction(), ["fullName"] = new FullNameFunction(),
      ["members"] = new MembersFunction(), ["parameters"] = new ParametersFunction(),
      ["nullableType"] = new NullableTypeFunction(), ["csharpLiteral"] = new CSharpLiteralFunction(),
      ["makeGeneric"] = new MakeGenericFunction(), ["visibility"] = new VisibilityFunction(),
      ["path"] = new PathFunction(), ["unwrap"] = new UnwrapFunction(), ["switch"] = new SwitchFunction(),
      ["size"] = new SizeFunction(), ["replace"] = new ReplaceFunction(), ["replaceFirst"] = new ReplaceFirstFunction(),
      ["format"] = new FormatFunction(), ["identifier"] = new IdentifierFunction(),
      ["floatTime"] = new FloatTimeFunction(), ["table"] = new AsTableFunction(), ["put"] = new PutFunction(),
      ["remove"] = new RemoveFunction(), ["push"] = new PushFunction(), ["pop"] = new PopFunction(),
      ["joinKeys"] = new JoinKeysFunction(), ["joinValues"] = new JoinValuesFunction(),
      ["join"] = new JoinEntriesFunction(), ["mapValues"] = new MapValuesFunction(), ["map"] = new MapFunction(),
      ["filter"] = new FilterFunction(), ["reduce"] = new ReduceFunction(), ["attributes"] = new AttributesFunction(),
      ["derive"] = new DeriveFunction(), ["attributesOf"] = new AttributesOfFunction(),
      ["attributesOfExact"] = new AttributesOfExactFunction(), ["attributeOf"] = new AttributeOfFunction(),
      ["wire"] = new WireFunction(), ["signature"] = new SignaturePredicate(), ["wireable"] = new WireablePredicate(),
      ["exists"] = new ExistsPredicate(), ["and"] = new AndPredicate(), ["or"] = new OrPredicate(),
      ["is"] = new IsPredicate(), ["has"] = new HasPredicate(), ["eq"] = new EqualPredicate(),
      ["matches"] = new MatchesPredicate(), ["isSelf"] = new TraitPredicate("isSelf"),
      ["ref"] = new TraitPredicate("ref"), ["in"] = new TraitPredicate("in"), ["out"] = new TraitPredicate("out"),
      ["inout"] = new TraitPredicate("inout"), ["argument"] = new TraitPredicate("argument"),
      ["static"] = new TraitPredicate("static"), ["async"] = new TraitPredicate("async"),
      ["public"] = new TraitPredicate("public"), ["exposed"] = new TraitPredicate("exposed"),
      ["top"] = new TraitPredicate("top"), ["concrete"] = new TraitPredicate("concrete"),
      ["partial"] = new TraitPredicate("partial"), ["generic"] = new TraitPredicate("generic"),
      ["genericMethod"] = new TraitPredicate("genericMethod"), ["struct"] = new TraitPredicate("struct"),
      ["class"] = new TraitPredicate("class"), ["field"] = new TraitPredicate("field"),
      ["property"] = new TraitPredicate("property"), ["method"] = new TraitPredicate("method"),
      ["event"] = new TraitPredicate("event"), ["parameter"] = new TraitPredicate("parameter"),
      ["typeSymbol"] = new TraitPredicate("typeSymbol"), ["referenceType"] = new TraitPredicate("referenceType"),
      ["valueType"] = new TraitPredicate("valueType"), ["nullable"] = new TraitPredicate("nullable"),
      ["pointer"] = new TraitPredicate("pointer"), ["containsPointer"] = new TraitPredicate("containsPointer"),
      ["enum"] = new TraitPredicate("enum"), ["primitive"] = new TraitPredicate("primitive"),
      ["parameterDefault"] = new TraitPredicate("parameterDefault"),
      ["nonEmptyStringConstant"] = new TraitPredicate("nonEmptyStringConstant"),
      ["equatableSelf"] = new TraitPredicate("equatableSelf"),
      ["typedEqualsSelf"] = new TraitPredicate("typedEqualsSelf"),
      ["ordinaryTypedEqualsSelf"] = new TraitPredicate("ordinaryTypedEqualsSelf"),
      ["objectEquals"] = new TraitPredicate("objectEquals"), ["hashCode"] = new TraitPredicate("hashCode")
    };
    ConfigureLanguageSignatures(definitions);
    return definitions;
  }

  public static bool TryGet(string name, out FunctionDefinition definition) {
    return Definitions.TryGetValue(name ?? "", out definition);
  }

  public static bool IsPredicate(string name) {
    return TryGet(name, out var definition) && definition.IsPredicate;
  }

  public static IEnumerable<FunctionDefinition> Enumerate() => Definitions.Values;

  private static void ConfigureLanguageSignatures(IDictionary<string, FunctionDefinition> definitions) {
    foreach (var definition in definitions.Values)
      definition.WithLanguageSignature(
        MixinLanguageValueKind.Any,
        definition.IsPredicate ? MixinLanguageValueKind.Boolean : MixinLanguageValueKind.Any,
        Enumerable.Repeat(MixinLanguageValueKind.Any, Math.Min(definition.MaximumArguments, 16)).ToArray(),
        definition.IsPredicate ? "Tests the current value." : "Transforms the current value."
      );

    Signature(definitions, "name", MixinLanguageValueKind.Any, MixinLanguageValueKind.Text, []);
    Signature(definitions, "path", MixinLanguageValueKind.Any, MixinLanguageValueKind.Any,
      [MixinLanguageValueKind.Text]);
    Signature(definitions, "switch", MixinLanguageValueKind.Boolean, MixinLanguageValueKind.Any,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Any]);
    Signature(definitions, "size", MixinLanguageValueKind.Any, MixinLanguageValueKind.Text, []);
    Signature(definitions, "table", MixinLanguageValueKind.Any, MixinLanguageValueKind.Table, []);

    Signature(definitions, "put", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Any]);
    Signature(definitions, "remove", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table,
      [MixinLanguageValueKind.Any]);
    Signature(definitions, "push", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table,
      [MixinLanguageValueKind.Any]);
    Signature(definitions, "pop", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table, []);
    foreach (var name in new[] { "joinKeys", "joinValues" })
      Signature(definitions, name, MixinLanguageValueKind.Table, MixinLanguageValueKind.Text,
        [MixinLanguageValueKind.Text]);
    Signature(definitions, "join", MixinLanguageValueKind.Table, MixinLanguageValueKind.Text,
      [MixinLanguageValueKind.Text, MixinLanguageValueKind.Text]);
    foreach (var name in new[] { "mapValues", "map", "filter" })
      Signature(definitions, name, MixinLanguageValueKind.Table, MixinLanguageValueKind.Table,
        [MixinLanguageValueKind.Function]);
    Signature(definitions, "reduce", MixinLanguageValueKind.Table, MixinLanguageValueKind.Any,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Function]);
    Signature(definitions, "derive", MixinLanguageValueKind.Table, MixinLanguageValueKind.Table, []);

    Signature(definitions, "type", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Type, []);
    Signature(definitions, "fullName", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Text, []);
    Signature(definitions, "visibility", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Text, []);
    Signature(definitions, "members", MixinLanguageValueKind.Type, MixinLanguageValueKind.Table, []);
    Signature(definitions, "parameters", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Table, []);
    Signature(definitions, "nullableType", MixinLanguageValueKind.Type, MixinLanguageValueKind.Type, []);
    Signature(definitions, "makeGeneric", MixinLanguageValueKind.Type, MixinLanguageValueKind.Type,
      [MixinLanguageValueKind.CSharpType]);
    Signature(definitions, "attributes", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Table, []);
    foreach (var name in new[] { "attributesOf", "attributesOfExact" })
      Signature(definitions, name, MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Table,
        [MixinLanguageValueKind.CSharpType]);
    Signature(definitions, "attributeOf", MixinLanguageValueKind.Symbol, MixinLanguageValueKind.Any,
      [MixinLanguageValueKind.CSharpType]);

    foreach (var name in new[] { "replace", "replaceFirst" })
      Signature(definitions, name, MixinLanguageValueKind.Text, MixinLanguageValueKind.Text,
        [MixinLanguageValueKind.Text, MixinLanguageValueKind.Text]);
    Signature(definitions, "format", MixinLanguageValueKind.Text, MixinLanguageValueKind.Text,
      [MixinLanguageValueKind.Any, MixinLanguageValueKind.Any]);
    foreach (var name in new[] { "identifier", "floatTime" })
      Signature(definitions, name, MixinLanguageValueKind.Text, MixinLanguageValueKind.Text, []);
    Signature(definitions, "matches", MixinLanguageValueKind.Text, MixinLanguageValueKind.Boolean,
      [MixinLanguageValueKind.Text]);

    Configure(definitions, ["containsPointer"], MixinLanguageValueKind.Type,
      MixinLanguageValueKind.Boolean);
  }

  private static void Signature(IDictionary<string, FunctionDefinition> definitions, string name,
    MixinLanguageValueKind receiver, MixinLanguageValueKind result,
    IReadOnlyList<MixinLanguageValueKind> arguments) {
    if (definitions.TryGetValue(name, out var definition)) definition.WithLanguageSignature(
      receiver, definition.IsPredicate ? MixinLanguageValueKind.Boolean : result,
      arguments, definition.Documentation);
  }

  private static void Configure(IDictionary<string, FunctionDefinition> definitions,
    IEnumerable<string> names, MixinLanguageValueKind receiver, MixinLanguageValueKind result) {
    foreach (var name in names)
      if (definitions.TryGetValue(name, out var definition)) definition.WithLanguageSignature(
        receiver, definition.IsPredicate ? MixinLanguageValueKind.Boolean : result,
        definition.ArgumentTypes, definition.Documentation);
  }

  internal static void CollectConstants(MixinStringPoolBuilder pool) {
    foreach (var name in Definitions.Keys) pool.Intern(name);
  }
}
