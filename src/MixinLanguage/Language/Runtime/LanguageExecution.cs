using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mixins.Functions;

namespace Mixins.Runtime;

/// <summary>Execution state for a single compiled-image invocation; no syntax or source is retained.</summary>
internal sealed partial class LanguageExecution {
  private readonly ExecutionContext context;
  private readonly MixinVirtualMachine machine;
  private MixinExpressionExecutionProgram program;
  private LanguageFunctionScope scope;
  private IReadOnlyList<BytecodeDerivation> derivations => program.Derivations;
  private readonly HashSet<BytecodeDerivation> activeDerivations = [];
  private readonly List<MixinExpressionOutput> outputs = [];
  private readonly List<MixinExpressionLog> logs = [];
  private Dictionary<string, IMixinValue> locals = new(StringComparer.Ordinal);
  private readonly Dictionary<string, IMixinValue> carries = new(StringComparer.Ordinal);
  private readonly HashSet<string> carriedLocals = new(StringComparer.Ordinal);
  private readonly Dictionary<string, IMixinValue> variables = new(StringComparer.Ordinal);
  private readonly Dictionary<string, IMixinValue> targetVariables = new(StringComparer.Ordinal);
  private IMixinValue parameter = NullMixinValue.Instance;
  private IReadOnlyList<IMixinValue> positionalParameters = Array.Empty<IMixinValue>();
  private IMixinValue selector = NullMixinValue.Instance;
  private bool prelude;
  private int steps;
  private int depth;
  private bool pure;
  private const int StepLimit = 100000;
  internal ExecutionContext Context => context;
  internal bool IsPrelude => prelude;
  internal List<MixinExpressionOutput> Outputs => outputs;
  internal List<MixinExpressionLog> Logs => logs;
  internal NamedFunctionMixinValue BindFunction(string name) => scope.Bind(name);

  internal LanguageExecution(MixinVirtualMachine machine, ExecutionContext context, MixinExpressionExecutionProgram program) {
    this.machine = machine;
    this.context = context;
    this.program = program;
    scope = program.Scope;
  }

  internal MixinExpressionResult Execute(IEnumerable<BytecodeExpression> expressions,
    IDictionary<string, object> imported = null, IReadOnlyDictionary<string, object> importedCarries = null) {
    var started = Stopwatch.GetTimestamp();
    double Elapsed() => (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency;
    if (imported != null) foreach (var item in imported) variables[item.Key] = Import(item.Value);
    if (importedCarries != null) foreach (var item in importedCarries) {
      carries[item.Key] = Import(item.Value);
      carriedLocals.Add(item.Key);
    }
    foreach (var item in context.TargetVariables)
      targetVariables[item.Key.Resolve(context.Strings)] = item.Value;
    var previousOutput = context.OutputSink;
    var previousLog = context.LogSink;
    context.OutputSink = outputs.Add;
    context.LogSink = logs.Add;
    try {
      foreach (var expression in expressions.OrderBy(expression => expression.IsPrelude ? 0 : 1)) {
        if (prelude && !expression.IsPrelude) ClosePrelude();
        prelude = expression.IsPrelude;
        locals = new Dictionary<string, IMixinValue>(StringComparer.Ordinal);
        var savedVariables = variables.ToArray();
        var savedTargets = targetVariables.ToArray();
        var savedCarries = carries.ToArray();
        var outputCount = outputs.Count;
        var completion = Run(expression.Body);
        if (completion.Kind is not (BytecodeFlow.Normal or BytecodeFlow.Return)) {
          Restore(variables, savedVariables);
          Restore(targetVariables, savedTargets);
          Restore(carries, savedCarries);
          outputs.RemoveRange(outputCount, outputs.Count - outputCount);
          var error = completion.Kind == BytecodeFlow.Error ? Text(completion.Value)
            : "invalid " + completion.Kind + " outside its scope";
          return new MixinExpressionResult(false, error, completion.Line, outputs.ToArray(), logs.ToArray(),
            Commit(imported), ExportCarries(), steps, Elapsed());
        }
        Commit(imported);
      }
      ClosePrelude();
      var exported = Commit(imported);
      return new MixinExpressionResult(true, null, 0, outputs.ToArray(), logs.ToArray(), exported,
        ExportCarries(), steps, Elapsed());
    } finally {
      context.OutputSink = previousOutput;
      context.LogSink = previousLog;
    }
  }

  private IReadOnlyDictionary<string, object> Commit(IDictionary<string, object> imported) {
    var exported = variables.ToDictionary(item => item.Key,
      item => context.UnlinkSnapshot(item.Value), StringComparer.Ordinal);
    if (imported != null) foreach (var item in exported) imported[item.Key] = item.Value;
    context.TargetVariables.ReplaceWith(targetVariables.Select(item =>
      new KeyValuePair<MixinString, IMixinValue>(ExecutionContext.Dynamic(item.Key), item.Value)));
    return exported;
  }

  private IReadOnlyDictionary<string, object> ExportCarries() => carries.ToDictionary(
    item => item.Key, item => context.UnlinkSnapshot(item.Value), StringComparer.Ordinal);

  private void ClosePrelude() {
    foreach (var name in carries.Keys.ToArray()) carries[name] = context.DetachValue(carries[name]);
    foreach (var name in variables.Keys.ToArray()) variables[name] = context.DetachValue(variables[name]);
    foreach (var name in targetVariables.Keys.ToArray()) targetVariables[name] = context.DetachValue(targetVariables[name]);
  }

  private VmCompletion pendingControl;
  internal void Break(int line) => pendingControl = new(BytecodeFlow.Break, Line: line);
  private static VmCompletion Failed(IMixinValue error, int line) => new(BytecodeFlow.Error, error, Line: line);

  private IMixinValue Root(string name) {
    if (name == "args") return new TupleMixinValue(positionalParameters);
    if (name == "param") return parameter;
    if (name == "\0selector") return selector;
    if (KindMixinValue.TryGet(name, out var kind)) return kind;
    if (scope.Bind(name) is { } function) return function;
    if (name is "this" or "target" or "attr") {
      if (pure) return context.Error("pure functions cannot read host roots");
      if (!prelude) return context.Error("host root '" + name + "' requires the prelude pass");
      return context.Resolve(name switch {"this" => MixinExpressionRoot.This,
        "target" => MixinExpressionRoot.Target, _ => MixinExpressionRoot.Attribute}, context.ResolveString(""));
    }
    if (name is "local" or "var" or "tar") {
      if (pure && name != "local") return context.Error("pure functions cannot read shared storage");
      var storage = name switch {"local" => locals, "var" => variables, _ => targetVariables};
      if (name == "local" && depth == 0 && carries.Count != 0)
        return new MixinTableValue(carries.Concat(locals).GroupBy(item => item.Key, StringComparer.Ordinal)
          .Select(group => group.Last()).Select(item => new KeyValuePair<MixinString, IMixinValue>(
            context.ResolveString(item.Key), item.Value)).ToArray());
      return new MixinTableValue(storage.Select(item => new KeyValuePair<MixinString, IMixinValue>(
        context.ResolveString(item.Key), item.Value)).ToArray());
    }
    return context.Error("unknown root '" + name + "'");
  }

  private IMixinValue Invoke(string name, IMixinValue[] arguments, int line, LanguageFunctionScope binding = null) {
    var candidates = (binding ?? scope).Candidates(name);
    if (candidates.Count == 0) return context.Error("unknown function '" + name + "'");
    var matches = new List<(BytecodeFunction Function, BytecodeSignature Signature, IMixinValue Parameter, int Score, LanguageFunctionScope Owner)>();
    foreach (var candidate in candidates) {
      var function = candidate.Function;
      var signature = candidate.Signature;
      if (signature == null) matches.Add((function, null, Pack(arguments), -10000, candidate.Owner));
      else {
        if (signature.Inputs == null) {
          if (arguments.Length == 1 && TryConvert(arguments[0], signature.InputKind, out var converted, out var conversionCount))
            matches.Add((function, signature, converted,
              (signature.InputKind == "any" ? 1000 : 1001) - conversionCount, candidate.Owner));
          continue;
        }
        var fields = signature.Inputs;
        var variadic = fields.Count > 0 && fields[fields.Count - 1].Variadic;
        var fixedCount = fields.Count - (variadic ? 1 : 0);
        if (arguments.Length == 1 && arguments[0] is MixinTableValue supplied && !variadic) {
          var convertedEntries = supplied.Entries.ToArray();
          var namedConversions = 0;
          var valid = true;
          foreach (var field in fields) {
            var entryIndex = Array.FindIndex(convertedEntries,
              entry => entry.Key.Resolve(context.Strings) == field.Name);
            if (entryIndex < 0 || !TryConvert(convertedEntries[entryIndex].Value, field.Kind,
                  out var converted, out var count)) { valid = false; break; }
            convertedEntries[entryIndex] = new KeyValuePair<MixinString, IMixinValue>(
              convertedEntries[entryIndex].Key, converted);
            namedConversions += count;
          }
          if (valid) {
            matches.Add((function, signature, new MixinTableValue(convertedEntries),
              1000 + fields.Count(field => field.Kind != "any") - namedConversions, candidate.Owner));
            continue;
          }
        }
        if (arguments.Length < fixedCount || !variadic && arguments.Length != fixedCount) continue;
        var convertedArguments = new IMixinValue[arguments.Length];
        var positionalConversions = 0;
        var positionalValid = true;
        for (var index = 0; index < arguments.Length; index++) {
          var field = fields[Math.Min(index, fields.Count - 1)];
          if (!TryConvert(arguments[index], field.Kind, out convertedArguments[index], out var count)) {
            positionalValid = false;
            break;
          }
          positionalConversions += count;
        }
        if (!positionalValid) continue;
        var entries = fields.Select((field, index) => new KeyValuePair<MixinString, IMixinValue>(
          context.ResolveString(field.Name), field.Variadic
            ? new TupleMixinValue(convertedArguments.Skip(index).ToArray()) : convertedArguments[index])).ToArray();
        matches.Add((function, signature, new MixinTableValue(entries), (variadic ? 0 : 1000) +
          fields.Count(field => field.Kind != "any") - positionalConversions, candidate.Owner));
      }
    }
    if (matches.Count > 0) {
      var highest = matches.Max(candidate => candidate.Score);
      matches.RemoveAll(candidate => candidate.Score != highest);
    }
    if (matches.Count != 1) return context.Error(matches.Count == 0
      ? "no matching signature for '" + name + "'" : "ambiguous signature for '" + name + "'");
    var match = matches[0];
    if (pure && !match.Function.IsPure) return context.Error("pure functions cannot invoke impure functions");
    if (!prelude && !match.Function.IsPure) return context.Error("impure function '" + name + "' requires prelude preparation");
    if (++depth > 128) { depth--; return context.Error("call depth limit exceeded"); }
    var previousLocals = locals;
    var previousParameter = parameter;
    var previousPositionalParameters = positionalParameters;
    var previousPure = pure;
    var previousScope = scope;
    var previousProgram = program;
    var savedVariables = variables.ToArray();
    var savedTargets = targetVariables.ToArray();
    var savedCarries = carries.ToArray();
    var outputCount = outputs.Count;
    var logCount = logs.Count;
    void RollBack() {
      Restore(variables, savedVariables);
      Restore(targetVariables, savedTargets);
      Restore(carries, savedCarries);
      outputs.RemoveRange(outputCount, outputs.Count - outputCount);
      logs.RemoveRange(logCount, logs.Count - logCount);
    }
    locals = new Dictionary<string, IMixinValue>(StringComparer.Ordinal);
    parameter = match.Parameter;
    positionalParameters = arguments;
    pure = match.Function.IsPure;
    scope = match.Owner;
    program = scope.Program;
    try {
      var completion = Run(match.Function.Body);
      if (completion.Kind == BytecodeFlow.Error) { RollBack(); return completion.Value; }
      if (completion.Kind is not (BytecodeFlow.Normal or BytecodeFlow.Return)) {
        RollBack();
        return context.Error("control flow cannot cross function boundaries: " + completion.Kind);
      }
      var result = completion.Kind == BytecodeFlow.Return ? completion.Value : NullMixinValue.Instance;
      if (result is ErrorMixinValue) { RollBack(); return result; }
      if (match.Signature != null && !MatchesReturn(result, match.Signature)) {
        RollBack(); return context.Error("return kind does not match signature of '" + name + "'");
      }
      return result;
    } finally {
      locals = previousLocals;
      parameter = previousParameter;
      positionalParameters = previousPositionalParameters;
      pure = previousPure;
      scope = previousScope;
      program = previousProgram;
      depth--;
    }
  }

  private bool MatchesReturn(IMixinValue value, BytecodeSignature signature) => signature.Outputs == null
    ? MatchesKind(value, signature.OutputKind)
    : value is MixinTableValue table && signature.Outputs.All(field => table.Entries.Any(entry =>
      entry.Key.Resolve(context.Strings) == field.Name && MatchesKind(entry.Value, field.Kind)));
  internal static bool MatchesKind(IMixinValue value, string kind) => kind == "any" || value.Kind.ToString().Equals(kind, StringComparison.OrdinalIgnoreCase);
  private bool TryConvert(IMixinValue value, string kind, out IMixinValue converted, out int conversions) {
    if (kind == "any" || value.Kind.ToString().Equals(kind, StringComparison.OrdinalIgnoreCase)) {
      converted = value;
      conversions = 0;
      return true;
    }
    if (!KindMixinValue.TryGet(kind, out var target) ||
      !KindDefinitions.TryImplicitConvert(this, value, target.ValueKind, out converted)) {
      converted = null;
      conversions = 0;
      return false;
    }
    conversions = 1;
    return true;
  }
  private static void Restore(Dictionary<string, IMixinValue> storage, KeyValuePair<string, IMixinValue>[] saved) {
    storage.Clear();
    foreach (var item in saved) storage.Add(item.Key, item.Value);
  }
  private static IMixinValue Pack(IMixinValue[] values) => values.Length switch {
    0 => NullMixinValue.Instance, 1 => values[0], _ => new TupleMixinValue(values)
  };
  private static IMixinValue Import(object value) => value switch {
    null => NullMixinValue.Instance, IMixinValue typed => typed, string text => String(text),
    DetachedSemanticData semantic => DetachedSemanticMixinValue.Materialize(semantic),
    bool boolean => Bool(boolean), double number => new NumberMixinValue(number),
    int number => new NumberMixinValue(number), long number => new NumberMixinValue(number),
    IReadOnlyDictionary<string, object> table => new MixinTableValue(table.Select(entry =>
      new KeyValuePair<MixinString, IMixinValue>(ExecutionContext.Dynamic(entry.Key), Import(entry.Value))).ToArray()),
    IEnumerable<object> tuple => new TupleMixinValue(tuple.Select(Import).ToArray()),
    _ => new ObjectMixinValue(value)
  };
  internal string Text(IMixinValue value) => value switch {
    NullMixinValue => "",
    _ => value.Render(context).Resolve(context.Strings)
  };
  internal static LiteralMixinValue String(string text) => new(ExecutionContext.Dynamic(text));
  internal static BooleanMixinValue Bool(bool value) => value ? BooleanMixinValue.True : BooleanMixinValue.False;
  internal bool Equal(IMixinValue left, IMixinValue right) => (left, right) switch {
    (LiteralMixinValue a, LiteralMixinValue b) => Text(a) == Text(b),
    (TupleMixinValue a, TupleMixinValue b) => a.Values.Count == b.Values.Count && a.Values.Zip(b.Values, Equal).All(value => value),
    (MixinTableValue a, MixinTableValue b) => a.Count == b.Count && a.Entries.All(x => b.Entries.Any(y =>
      x.Key.Resolve(context.Strings) == y.Key.Resolve(context.Strings) && Equal(x.Value, y.Value))),
    _ => left.Equals(right)
  };

  internal IMixinValue RenderText(IMixinValue value) => value is MixinTableValue or TupleMixinValue
    ? context.Error("collections require an explicit join") : String(Text(value));
}
