using System;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Runtime;

public sealed record HixExpressionLog(string Text = "", int Line = -1, bool IsHint = false);

public sealed record HixExpressionPreparedLog(string Text, int Line, int ProgramIndex);

public sealed record ValidationResult(bool Success, string Error, int ErrorLine) {
  public static readonly ValidationResult Ok = new(true, null, -1);
  public static readonly ValidationResult UnknownError = new(false, "unknown error", -1);

  public static ValidationResult Fail(string error, int line) {
    return new ValidationResult(false, error, line);
  }
}

public sealed class HixExpressionOutput {
  internal HixExpressionOutput(
    HixEmissionTarget target, IReadOnlyList<HixString> segments,
    HixStringPool strings, HixString injectionTarget = default, int injectionPriority = 0
  ) {
    Target = target;
    Segments = segments ?? [];
    Strings = strings;
    InjectionTargetValue = injectionTarget;
    InjectionPriority = injectionPriority;
  }

  public HixExpressionOutput(
    HixEmissionTarget target, string text, string injectionTarget = null,
    int injectionPriority = 0
  ) {
    var strings = new HixStringPoolBuilder().Freeze();
    Target = target;
    Strings = strings;
    Segments = [HixString.Dynamic(text ?? "")];
    InjectionTargetValue = HixString.Dynamic(injectionTarget ?? "");
    InjectionPriority = injectionPriority;
  }

  public HixEmissionTarget Target { get; }
  public string Text => string.Concat(Segments.Select(item => item.Resolve(Strings)));
  public string InjectionTarget => InjectionTargetValue.Resolve(Strings);
  public int InjectionPriority { get; }
  internal IReadOnlyList<HixString> Segments { get; }
  internal HixStringPool Strings { get; }
  internal HixString InjectionTargetValue { get; }
  internal bool IsEmpty => Segments.All(item => string.IsNullOrEmpty(item.Resolve(Strings)));

  internal string Resolve(HixString value) {
    return value.Resolve(Strings);
  }

  internal HixExpressionOutput Retarget(HixEmissionTarget target) {
    return new HixExpressionOutput(target, Segments, Strings, InjectionTargetValue, InjectionPriority);
  }
}

public sealed class HixExpressionResult {
  internal HixExpressionResult(
    bool success, string error, int errorLine,
    IReadOnlyList<HixExpressionOutput> outputs, IReadOnlyList<HixExpressionLog> logs = null,
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
  public IReadOnlyList<HixExpressionOutput> Outputs { get; }
  public IReadOnlyList<HixExpressionLog> Logs { get; }
  public IReadOnlyDictionary<string, object> Variables { get; }
  public IReadOnlyDictionary<string, object> Carries { get; }
  public int ExecutedOperations { get; }
  public double ExecutionMilliseconds { get; }
}

public class HixExecutionContext {
  public HixExecutionContext(HixBackend backend = null, HixStringPool strings = null) {
    Backend = backend ?? HixCoreBackend.Instance;
    strings ??= new HixStringPoolBuilder().Freeze();
    initialStrings = strings ?? throw new ArgumentNullException(nameof(strings));
  }

  public HixBackend Backend { get; }
  internal LanguageExecution Execution { get; set; }
  public bool IsPrelude => Execution?.IsPrelude ?? false;
  public IHixValue Invoke(IHixValue function, IHixValue[] arguments, int line = 0) => Execution?.Callback(function, arguments, line) ?? Error("no active execution");
  private HixVM virtualMachine;
  private readonly HixStringPool initialStrings;
  public HixStringPool Strings => virtualMachine?.StringPool ?? initialStrings;
  internal void Attach(HixVM machine) => virtualMachine = machine;
  internal HixValueDictionary Locals { get; } = new();
  internal HixValueDictionary Variables { get; } = new();
  public HixValueDictionary TargetVariables { get; } = new();
  internal HixValueDictionary Carries { get; } = new();
  internal IHixValue Parameter { get; set; } = NullHixValue.Instance;
  internal Action<HixExpressionOutput> OutputSink { get; set; }
  internal Action<HixExpressionLog> LogSink { get; set; }

  public IHixValue Resolve(HixExpressionRoot root, HixString member) {
    switch (root) {
      case HixExpressionRoot.True: return BooleanHixValue.True;
      case HixExpressionRoot.False: return BooleanHixValue.False;
      case HixExpressionRoot.Null: return NullHixValue.Instance;
      case HixExpressionRoot.Local:
        return Locals.TryGetValue(member, out var local)
          ? local
          : NullHixValue.Instance;
      case HixExpressionRoot.Variable:
        return Variables.TryGetValue(member, out var variable)
          ? variable
          : NullHixValue.Instance;
      case HixExpressionRoot.TargetVariable:
        var targetName = member.Resolve(Strings);
        foreach (var item in TargetVariables) {
          if (string.Equals(item.Key.Resolve(Strings), targetName, StringComparison.Ordinal))
            return item.Value;
        }
        return NullHixValue.Instance;
      case HixExpressionRoot.Parameter:
        return string.IsNullOrEmpty(member.Resolve(Strings))
          ? Parameter
          : Parameter.Select(this, member);
      case HixExpressionRoot.Table: return HixTableValue.Empty;
      default: return ResolveHost(root, member);
    }
  }

  protected virtual IHixValue ResolveHost(HixExpressionRoot root, HixString member) => Backend.ResolveRoot(this, root.ToString().ToLowerInvariant()).Select(this, member);

  public IHixValue Evaluate(IHixValue value) => value ?? NullHixValue.Instance;

  public virtual HixString Render(IHixValue value) {
    return Backend.Render(this, value);
  }

  public virtual HixString NameOf(IHixValue value) => Backend.NameOf(this, value);

  public virtual IHixValue Unwrap(IHixValue value) => Backend.Unwrap(this, value);

  public virtual bool IsType(IHixValue value, HixString type) => Backend.IsType(this, value, type);

  public virtual IHixValue Attributes(IHixValue value, HixString type, bool exact, bool first) => Backend.Attributes(this, value, type, exact, first);

  public virtual IHixValue ResolveMixin(HixString local, IHixValue operand) => Backend.ResolveMixin(this, local, operand);

  public virtual IHixValue DefineTarget(string name, string descriptor) => Backend.DefineTarget(this, name, descriptor);
  public virtual string ResolveInjectionTarget(string target) => Backend.ResolveInjectionTarget(this, target);
  public virtual IHixValue Configure(string name, IHixValue value) => Backend.Configure(this, name, value);

  public virtual bool HasTrait(IHixValue value, HixString trait) => Backend.HasTrait(this, value, trait);

  public object UnlinkSnapshot(IHixValue value) => Backend.UnlinkSnapshot(this, value);

  public IHixValue DetachValue(IHixValue value) => Backend.DetachValue(this, value);

  protected internal IHixValue DetachCore(IHixValue value) {
    value = Evaluate(value);
    return value switch {
      ErrorHixValue error => error with { Message = HixString.Dynamic(error.Message.Resolve(Strings)) },
      LiteralHixValue literal when !literal.Value.IsInterned => literal,
      LiteralHixValue literal => new LiteralHixValue(
        HixString.Dynamic(literal.Value.Resolve(Strings))
      ),
      TupleHixValue tuple => tuple.Transform(DetachValue),
      HixTableValue table => new HixTableValue(
        [
          .. table.Entries.Select(item =>
            new KeyValuePair<HixString, IHixValue>(
              HixString.Dynamic(item.Key.Resolve(Strings)), DetachValue(item.Value)
            )
          )
        ]
      ),
      _ => value
    };
  }

  public HixString ResolveString(string value) => Dynamic(value);
  public static HixString Dynamic(string value) => HixString.Dynamic(value ?? "");
  public ErrorHixValue Error(string value) => new(Dynamic(value));
}
