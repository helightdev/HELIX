using System;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

public sealed record MixinExpressionLog(string Text = "", int Line = -1, bool IsHint = false);

public sealed record MixinExpressionPreparedLog(string Text, int Line, int ProgramIndex);

public sealed record ValidationResult(bool Success, string Error, int ErrorLine) {
  public static readonly ValidationResult Ok = new(true, null, -1);
  public static readonly ValidationResult UnknownError = new(false, "unknown error", -1);

  public static ValidationResult Fail(string error, int line) {
    return new ValidationResult(false, error, line);
  }
}

public sealed class MixinExpressionOutput {
  internal MixinExpressionOutput(
    MixinEmissionTarget target, IReadOnlyList<MixinString> segments,
    MixinStringPool strings, MixinString injectionTarget = default, int injectionPriority = 0
  ) {
    Target = target;
    Segments = segments ?? [];
    Strings = strings;
    InjectionTargetValue = injectionTarget;
    InjectionPriority = injectionPriority;
  }

  public MixinExpressionOutput(
    MixinEmissionTarget target, string text, string injectionTarget = null,
    int injectionPriority = 0
  ) {
    var strings = new MixinStringPoolBuilder().Freeze();
    Target = target;
    Strings = strings;
    Segments = [MixinString.Dynamic(text ?? "")];
    InjectionTargetValue = MixinString.Dynamic(injectionTarget ?? "");
    InjectionPriority = injectionPriority;
  }

  public MixinEmissionTarget Target { get; }
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

  internal MixinExpressionOutput Retarget(MixinEmissionTarget target) {
    return new MixinExpressionOutput(target, Segments, Strings, InjectionTargetValue, InjectionPriority);
  }
}

public sealed class MixinExpressionResult {
  internal MixinExpressionResult(
    bool success, string error, int errorLine,
    IReadOnlyList<MixinExpressionOutput> outputs, IReadOnlyList<MixinExpressionLog> logs = null,
    IReadOnlyDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null,
    int executedOperations = 0,
    double executionMilliseconds = 0
  ) {
    Success = success;
    Error = error;
    ErrorLine = errorLine;
    Outputs = outputs ?? [];
    Logs = logs ?? [];
    Variables = variables ?? new Dictionary<string, object>();
    Carries = carries ?? new Dictionary<string, object>();
    ExecutedOperations = executedOperations;
    ExecutionMilliseconds = executionMilliseconds;
  }

  public bool Success { get; }
  public string Error { get; }
  public int ErrorLine { get; }
  public IReadOnlyList<MixinExpressionOutput> Outputs { get; }
  public IReadOnlyList<MixinExpressionLog> Logs { get; }
  public IReadOnlyDictionary<string, object> Variables { get; }
  public IReadOnlyDictionary<string, object> Carries { get; }
  public int ExecutedOperations { get; }
  public double ExecutionMilliseconds { get; }
}

public abstract class ExecutionContext {
  protected ExecutionContext(MixinStringPool strings) {
    initialStrings = strings ?? throw new ArgumentNullException(nameof(strings));
  }

  private MixinVirtualMachine virtualMachine;
  private readonly MixinStringPool initialStrings;
  public MixinStringPool Strings => virtualMachine?.StringPool ?? initialStrings;
  internal void Attach(MixinVirtualMachine machine) => virtualMachine = machine;
  internal MixinValueDictionary Locals { get; } = new();
  internal MixinValueDictionary Variables { get; } = new();
  internal MixinValueDictionary TargetVariables { get; } = new();
  internal MixinValueDictionary Carries { get; } = new();
  internal IMixinValue Parameter { get; set; } = NullMixinValue.Instance;
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
      default: return ResolveHost(root, member);
    }
  }

  protected abstract IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member);

  public IMixinValue Evaluate(IMixinValue value) => value ?? NullMixinValue.Instance;

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

  public virtual IMixinValue DefineTarget(string name, string descriptor) => Error("target aliases are not supported by this host");
  public virtual string ResolveInjectionTarget(string target) => target;
  public virtual IMixinValue Configure(string name, IMixinValue value) => Error("unknown host configuration '" + name + "'");

  public virtual bool HasTrait(IMixinValue value, MixinString trait) {
    return false;
  }

  internal virtual object UnlinkSnapshot(IMixinValue value) {
    return value.Unlink(this);
  }

  internal virtual IMixinValue DetachValue(IMixinValue value) {
    value = Evaluate(value);
    return value switch {
      ErrorMixinValue error => error with { Message = MixinString.Dynamic(error.Message.Resolve(Strings)) },
      LiteralMixinValue literal when !literal.Value.IsInterned => literal,
      LiteralMixinValue literal => new LiteralMixinValue(
        MixinString.Dynamic(literal.Value.Resolve(Strings))
      ),
      TupleMixinValue tuple => tuple.Transform(DetachValue),
      MixinTableValue table => new MixinTableValue(
        [
          .. table.Entries.Select(item =>
            new KeyValuePair<MixinString, IMixinValue>(
              MixinString.Dynamic(item.Key.Resolve(Strings)), DetachValue(item.Value)
            )
          )
        ]
      ),
      _ => value
    };
  }

  public MixinString ResolveString(string value) => Dynamic(value);
  public static MixinString Dynamic(string value) => MixinString.Dynamic(value ?? "");
  public ErrorMixinValue Error(string value) => new(Dynamic(value));
}
