using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

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

  public MixinString ResolveString(string value) => Strings.Get(value);
  public static MixinString Dynamic(string value) => MixinString.Dynamic(value ?? "");
  public ErrorMixinValue Error(string value) => new(Dynamic(value));
}