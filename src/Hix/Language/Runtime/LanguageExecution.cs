using Hix.Collections;
using Hix.Env;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hix.Functions;

namespace Hix.Runtime;

/// <summary>Execution state for a single compiled-image invocation; no syntax or source is retained.</summary>
internal sealed partial class LanguageExecution {
  private readonly HixExecutionContext context;
  private readonly HixVM machine;
  private HixExpressionExecutionProgram program;
  private LanguageFunctionScope scope;
  private IReadOnlyList<BytecodeDerivation> derivations => program.Derivations;
  private readonly HashSet<BytecodeDerivation> activeDerivations = [];
  private readonly List<HixExpressionOutput> outputs = [];
  private readonly List<HixExpressionLog> logs = [];
  private HixValueDictionary locals;
  private readonly HixValueDictionary carries = new();
  private readonly HashSet<string> carriedLocals = new(StringComparer.Ordinal);
  private readonly HixValueDictionary variables = new();
  private readonly HixValueDictionary targetVariables = new();
  private IHixValue parameter = NullHixValue.Instance;
  private IReadOnlyList<IHixValue> positionalParameters = Array.Empty<IHixValue>();
  private bool prelude;
  private int steps;
  private int depth;
  private bool directInvocation;
  private bool pure;
  private const int StepLimit = 100000;
  internal HixExecutionContext Context => context;
  internal bool IsPrelude => prelude;
  internal List<HixExpressionOutput> Outputs => outputs;
  internal List<HixExpressionLog> Logs => logs;
  internal NamedFunctionHixValue BindFunction(string name) => scope.Bind(name);

  internal LanguageExecution(HixVM machine, HixExecutionContext context, HixExpressionExecutionProgram program) {
    this.machine = machine;
    this.context = context;
    context.Execution = this;
    this.program = program;
    scope = program.Scope;
  }

  internal HixExpressionResult Execute(IEnumerable<BytecodeExpression> expressions,
    IDictionary<string, object> imported = null, IReadOnlyDictionary<string, object> importedCarries = null) {
    using var profile = HixProfiler.Measure("vm.execution");
    var started = Stopwatch.GetTimestamp();
    if (imported != null) foreach (var item in imported) variables.StoreIsolated(HixExecutionContext.Dynamic(item.Key), Import(item.Value));
    if (importedCarries != null) foreach (var item in importedCarries) {
      carries.StoreIsolated(HixExecutionContext.Dynamic(item.Key), Import(item.Value));
      carriedLocals.Add(item.Key);
    }
    foreach (var item in context.TargetVariables)
      targetVariables.StoreIsolated(HixExecutionContext.Dynamic(item.Key.Resolve(context.Strings)), item.Value);
    var previousOutput = context.OutputSink;
    var previousLog = context.LogSink;
    context.OutputSink = outputs.Add;
    context.LogSink = logs.Add;
    try {
      foreach (var expression in expressions.OrderBy(expression => expression.IsPrelude ? 0 : 1)) {
        if (prelude && !expression.IsPrelude) ClosePrelude();
        prelude = expression.IsPrelude;
        if (locals != null) machine.ReturnLocals(locals);
        locals = machine.RentLocals();
        var savedVariables = variables.Snapshot();
        var savedTargets = targetVariables.Snapshot();
        var savedCarries = carries.Snapshot();
        var outputCount = outputs.Count;
        var completion = Run(expression.Body);
        if (completion.Kind is not (BytecodeFlow.Normal or BytecodeFlow.Return)) {
          Restore(variables, savedVariables);
          Restore(targetVariables, savedTargets);
          Restore(carries, savedCarries);
          outputs.RemoveRange(outputCount, outputs.Count - outputCount);
          var error = completion.Kind == BytecodeFlow.Error ? Text(completion.Value)
            : "invalid " + completion.Kind + " outside its scope";
          return new HixExpressionResult(false, error, completion.Line, outputs.ToArray(), logs.ToArray(),
            Commit(imported), ExportCarries(), steps, Elapsed(started));
        }
        Commit(imported);
      }
      ClosePrelude();
      var exported = Commit(imported);
      return new HixExpressionResult(true, null, 0, outputs.ToArray(), logs.ToArray(), exported,
        ExportCarries(), steps, Elapsed(started));
    } finally {
      if (locals != null) machine.ReturnLocals(locals);
      locals = null;
      context.OutputSink = previousOutput;
      context.LogSink = previousLog;
      context.Execution = null;
    }
  }

  private IReadOnlyDictionary<string, object> Commit(IDictionary<string, object> imported) {
    using var profile = HixProfiler.Measure("vm.commit");
    var exported = variables.ToDictionary(item => item.Key.Resolve(context.Strings),
      item => context.UnlinkSnapshot(item.Value), StringComparer.Ordinal);
    if (imported != null) foreach (var item in exported) imported[item.Key] = item.Value;
    context.TargetVariables.ReplaceWith(targetVariables);
    return exported;
  }

  private IReadOnlyDictionary<string, object> ExportCarries() => carries.ToDictionary(
    item => item.Key.Resolve(context.Strings), item => context.UnlinkSnapshot(item.Value), StringComparer.Ordinal);

  private void ClosePrelude() {
    using var profile = HixProfiler.Measure("vm.detach");
    foreach (var item in carries) carries.Store(context, item.Key, item.Value);
    foreach (var item in variables) variables.Store(context, item.Key, item.Value);
    foreach (var item in targetVariables) targetVariables.Store(context, item.Key, item.Value);
  }

  private VmCompletion pendingControl;
  internal void Break(int line) => pendingControl = new(BytecodeFlow.Break, Line: line);
  private static VmCompletion Failed(IHixValue error, int line) => new(BytecodeFlow.Error, error, Line: line);

  private IHixValue Root(string name) {
    if (name == "args") return new TupleHixValue(positionalParameters);
    if (name == "param") return parameter;
    if (KindHixValue.TryGet(name, out var kind)) return kind;
    if (scope.Bind(name) is { } function) return function;
    if (context.Backend.Roots.TryGetValue(name, out var root)) {
      if (pure && root.HasEffects) return context.Error("pure functions cannot read host roots");
      if (!prelude && root.RequiresPrelude) return context.Error("host root '" + name + "' requires the prelude pass");
      return context.Backend.ResolveRoot(context, name);
    }
    if (name is "local" or "var" or "tar") {
      if (pure && name != "local") return context.Error("pure functions cannot read shared storage");
      var storage = name switch {"local" => locals, "var" => variables, _ => targetVariables};
      return new HixStorageValue(storage, name == "local" && depth == 0 ? carries : null);
    }
    return context.Error("unknown root '" + name + "'");
  }

  internal HixInvocationResult InvokeGlobal(string name, IHixValue[] arguments) {
    var started = Stopwatch.GetTimestamp();
    directInvocation = true;
    try {
      var value = Invoke(name, arguments, 0);
      var error = value as ErrorHixValue;
      return new HixInvocationResult(value, new HixExpressionResult(error == null,
        error == null ? null : Text(error), 0, outputs.ToArray(), logs.ToArray(),
        executedOperations: steps, executionMilliseconds: Elapsed(started)));
    } finally { context.Execution = null; }
  }

  private IHixValue Invoke(string name, IHixValue[] arguments, int line, LanguageFunctionScope binding = null) {
    using var profile = HixProfiler.Measure("vm.invoke");
    var selectionError = SelectFunction(name, arguments, binding ?? scope, out var match);
    if (selectionError != null) return selectionError;
    if (pure && !match.Function.IsPure) return context.Error("pure functions cannot invoke impure functions");
    if (!directInvocation && !prelude && !match.Function.IsPure) return context.Error("impure function '" + name + "' requires prelude preparation");
    if (++depth > 128) { depth--; return context.Error("call depth limit exceeded"); }
    var previousLocals = locals;
    var previousParameter = parameter;
    var previousPositionalParameters = positionalParameters;
    var previousPure = pure;
    var previousScope = scope;
    var previousProgram = program;
    var savedVariables = variables.Snapshot();
    var savedTargets = targetVariables.Snapshot();
    var savedCarries = carries.Snapshot();
    var outputCount = outputs.Count;
    var logCount = logs.Count;
    locals = machine.RentLocals();
    parameter = match.Parameter;
    positionalParameters = arguments;
    pure = match.Function.IsPure;
    scope = match.Owner;
    program = scope.Program;
    try {
      var completion = Run(match.Function.Body);
      if (completion.Kind == BytecodeFlow.Error) { Rollback(savedVariables, savedTargets, savedCarries, outputCount, logCount); return completion.Value; }
      if (completion.Kind is not (BytecodeFlow.Normal or BytecodeFlow.Return)) {
        Rollback(savedVariables, savedTargets, savedCarries, outputCount, logCount);
        return context.Error("control flow cannot cross function boundaries: " + completion.Kind);
      }
      var result = completion.Kind == BytecodeFlow.Return ? completion.Value : NullHixValue.Instance;
      if (result is ErrorHixValue) { Rollback(savedVariables, savedTargets, savedCarries, outputCount, logCount); return result; }
      if (match.Signature != null && !MatchesReturn(result, match.Signature)) {
        Rollback(savedVariables, savedTargets, savedCarries, outputCount, logCount); return context.Error("return kind does not match signature of '" + name + "'");
      }
      return result;
    } finally {
      machine.ReturnLocals(locals);
      locals = previousLocals;
      parameter = previousParameter;
      positionalParameters = previousPositionalParameters;
      pure = previousPure;
      scope = previousScope;
      program = previousProgram;
      depth--;
    }
  }

  private bool MatchesReturn(IHixValue value, BytecodeSignature signature) => signature.Outputs == null
    ? MatchesKind(value, signature.OutputKind)
    : value is HixTableValue table && signature.Outputs.All(field =>
      table.TryGetValue(context, context.ResolveString(field.Name), out var member) && MatchesKind(member, field.Kind));
  internal static bool MatchesKind(IHixValue value, string kind) => kind == "any" || value.Kind.ToString().Equals(kind, StringComparison.OrdinalIgnoreCase);
  private bool TryConvert(IHixValue value, string kind, out IHixValue converted, out int conversions) {
    if (kind == "any" || value.Kind.ToString().Equals(kind, StringComparison.OrdinalIgnoreCase)) {
      converted = value;
      conversions = 0;
      return true;
    }
    if (!KindHixValue.TryGet(kind, out var target) ||
      !KindDefinitions.TryImplicitConvert(this, value, target.ValueKind, out converted)) {
      converted = null;
      conversions = 0;
      return false;
    }
    conversions = 1;
    return true;
  }
  private static double Elapsed(long started) => (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency;

  private void Rollback(PersistentMap<HixString, IHixValue> savedVariables,
    PersistentMap<HixString, IHixValue> savedTargets, PersistentMap<HixString, IHixValue> savedCarries,
    int outputCount, int logCount) {
    Restore(variables, savedVariables);
    Restore(targetVariables, savedTargets);
    Restore(carries, savedCarries);
    outputs.RemoveRange(outputCount, outputs.Count - outputCount);
    logs.RemoveRange(logCount, logs.Count - logCount);
  }

  private static void Restore(HixValueDictionary storage, PersistentMap<HixString, IHixValue> saved) =>
    storage.Restore(saved);
  private static IHixValue Pack(IHixValue[] values) => values.Length switch {
    0 => NullHixValue.Instance, 1 => values[0], _ => new TupleHixValue(values)
  };
  private IHixValue Import(object value) => value switch {
    null => NullHixValue.Instance, IHixValue typed => typed, string text => String(text),
    bool boolean => Bool(boolean), double number => new NumberHixValue(number),
    int number => new NumberHixValue(number), long number => new NumberHixValue(number),
    IReadOnlyDictionary<string, object> table => new HixTableValue(table.Select(entry =>
      new KeyValuePair<HixString, IHixValue>(HixExecutionContext.Dynamic(entry.Key), Import(entry.Value))).ToArray()),
    IEnumerable<object> tuple => new TupleHixValue(tuple.Select(Import).ToArray()),
    _ => context.Backend.Import(context, value)
  };
  internal string Text(IHixValue value) => value switch {
    NullHixValue => "",
    _ => value.Render(context).Resolve(context.Strings)
  };
  internal static LiteralHixValue String(string text) => new(HixExecutionContext.Dynamic(text));
  internal static BooleanHixValue Bool(bool value) => value ? BooleanHixValue.True : BooleanHixValue.False;
  internal bool Equal(IHixValue left, IHixValue right) => (left, right) switch {
    (LiteralHixValue a, LiteralHixValue b) => Text(a) == Text(b),
    (TupleHixValue a, TupleHixValue b) => a.Values.Count == b.Values.Count && a.Values.Zip(b.Values, Equal).All(value => value),
    (HixTableValue a, HixTableValue b) => a.Count == b.Count && a.Entries.All(entry =>
      b.TryGetValue(context, entry.Key, out var value) && Equal(entry.Value, value)),
    _ => left.Equals(right)
  };

  internal IHixValue RenderText(IHixValue value) => value is HixTableValue or TupleHixValue
    ? context.Error("collections require an explicit join") : String(Text(value));
}
