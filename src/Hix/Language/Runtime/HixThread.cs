using Hix.Compiler;
using Hix.Collections;
using Hix.Env;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hix.Functions;
using System.Text.RegularExpressions;

namespace Hix.Runtime;

/// <summary>Reusable invocation state. A thread runs one call at a time; VMs share immutable code and pools.</summary>
public sealed class HixThread {
  private HixVM machine;
  private HixProgramImage program;
  private LanguageFunctionScope scope;
  private IReadOnlyList<BytecodeDerivation> derivations => program.Derivations;
  private readonly HashSet<BytecodeDerivation> activeDerivations = [];
  private readonly List<HixOutput> outputs = [];
  private readonly List<HixLog> logs = [];
  private HixValueDictionary locals;
  private HixValueDictionary carries => Context.Carries;
  private HixValueDictionary variables => Context.Variables;
  private HixValueDictionary targetVariables => Context.TargetVariables;
  private IHixValue parameter = NullHixValue.Instance;
  private IReadOnlyList<IHixValue> positionalParameters = Array.Empty<IHixValue>();
  private bool prelude;
  private int steps;
  private int depth;
  private bool directInvocation;
  private bool pure;
  private const int StepLimit = 100000;
  public IReadOnlyList<HixOutput> Outputs => outputs;
  public void Emit(IHixValue value, HixString target = default) => outputs.Add(new HixOutput(
    DetachValue(value), target.IsNull ? HixString.Empty : HixString.Dynamic(target.Resolve(Strings))));
  public List<HixLog> Logs => logs;
  public NamedFunctionHixValue BindFunction(HixString name) => scope.Bind(name.Resolve(Strings));

  private int running;
  public bool IsRunning => Volatile.Read(ref running) != 0;

  internal void Stop() {
    try { CommitContext(); }
    finally {
      prelude = pure = false;
      Context.Release();
      Volatile.Write(ref running, 0);
    }
  }

  internal void Start(HixVM machine, HixProgramImage program) {
    if (Interlocked.CompareExchange(ref running, 1, 0) != 0) throw new InvalidOperationException("A Hix thread is already running");
    try { Context.Acquire(); }
    catch { Volatile.Write(ref running, 0); throw; }
    try {
      // Context seeds use their own immutable pool; committed values are already detached.
      this.machine = null;
      CommitContext();
      this.machine = machine;
      this.program = program;
      scope = program.Scope;
      activeDerivations.Clear();
      outputs.Clear();
      logs.Clear();
      Context.BeginExecution();
      parameter = NullHixValue.Instance;
      positionalParameters = Array.Empty<IHixValue>();
      prelude = pure = directInvocation = false;
      steps = depth = 0;
      pendingControl = default;
    } catch {
      Stop();
      throw;
    }
  }

  internal HixExecutionResult Execute(IEnumerable<BytecodeExpression> expressions,
    IDictionary<string, object> imported = null, IReadOnlyDictionary<string, object> importedCarries = null) {
    using var profile = HixProfiler.Measure("vm.execution");
    var started = Stopwatch.GetTimestamp();
    if (imported != null) foreach (var item in imported) variables.StoreIsolated(HixString.Dynamic(item.Key), Import(item.Value));
    if (importedCarries != null) foreach (var item in importedCarries) {
      carries.StoreIsolated(HixString.Dynamic(item.Key), Import(item.Value));
    }
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
        var hostEffects = Context.CaptureEffects();
        var completion = Run(expression.Body);
        if (completion.Kind is not (BytecodeFlow.Normal or BytecodeFlow.Return)) {
          Restore(variables, savedVariables);
          Restore(targetVariables, savedTargets);
          Restore(carries, savedCarries);
          outputs.RemoveRange(outputCount, outputs.Count - outputCount);
          Context.RollbackEffects(hostEffects);
          var error = completion.Kind == BytecodeFlow.Error ? Text(completion.Value)
            : HixString.Dynamic("invalid " + completion.Kind + " outside its scope");
          return new HixExecutionResult(false, error, completion.Line, outputs.ToArray(), logs.ToArray(),
            Snapshot(variables), Snapshot(carries), steps, Elapsed(started), Strings);
        }
      }
      ClosePrelude();
      var exported = Snapshot(variables);
      return new HixExecutionResult(true, default, 0, outputs.ToArray(), logs.ToArray(), exported,
        Snapshot(carries), steps, Elapsed(started), Strings);
    } finally {
      if (locals != null) machine.ReturnLocals(locals);
      locals = null;
    }
  }

  private void CommitContext() {
    DetachStorage(variables);
    DetachStorage(carries);
    DetachStorage(targetVariables);
  }

  private void DetachStorage(HixValueDictionary storage) {
    if (storage.Count == 0) return;
    storage.ReplaceWith(storage.Select(item =>
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(item.Key.Resolve(Strings)), DetachValue(item.Value))).ToArray());
  }

  public IReadOnlyDictionary<string, object> ExportVariables() {
    using var profile = HixProfiler.Measure("vm.commit");
    return Export(variables);
  }

  public IReadOnlyDictionary<string, object> ExportCarries() => Export(carries);

  private static readonly IReadOnlyDictionary<string, object> EmptyHostValues =
    new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
  private static IReadOnlyDictionary<HixString, IHixValue> Snapshot(HixValueDictionary storage) =>
    storage.Count == 0 ? HixExecutionResult.EmptyValues : storage.Snapshot();

  private IReadOnlyDictionary<string, object> Export(HixValueDictionary storage) {
    if (storage.Count == 0) return EmptyHostValues;
    var exported = new Dictionary<string, object>(storage.Count, StringComparer.Ordinal);
    foreach (var item in storage) exported.Add(item.Key.Resolve(Strings), UnlinkSnapshot(item.Value));
    return exported;
  }

  private void ClosePrelude() {
    using var profile = HixProfiler.Measure("vm.detach");
    foreach (var item in carries) carries.Store(this, item.Key, item.Value);
    foreach (var item in variables) variables.Store(this, item.Key, item.Value);
    foreach (var item in targetVariables) targetVariables.Store(this, item.Key, item.Value);
  }

  private VmCompletion pendingControl;
  public void Break(int line) => pendingControl = new(BytecodeFlow.Break, Line: line);
  private static VmCompletion Failed(IHixValue error, int line) => new(BytecodeFlow.Error, error, Line: line);

  private IHixValue Root(string name) {
    if (name == "args") return new TupleHixValue(positionalParameters);
    if (name == "param") return parameter;
    if (KindHixValue.TryGet(name, out var kind)) return kind;
    if (program.Patterns.TryGetValue(name, out var pattern)) return new PatternHixValue(new NamedHixPattern(name));
    if (scope.Bind(name) is { } function) return function;
    if (Backend.Roots.TryGetValue(name, out var root)) {
      if (pure && root.HasEffects) return Error("pure functions cannot read host roots");
      if (!prelude && root.RequiresPrelude) return Error("host root '" + name + "' requires the prelude pass");
      return Backend.ResolveRoot(this, name);
    }
    if (name is "local" or "var" or "tar") {
      if (pure && name != "local") return Error("pure functions cannot read shared storage");
      var storage = name switch {"local" => locals, "var" => variables, _ => targetVariables};
      return new HixStorageValue(storage, name == "local" && depth == 0 ? carries : null);
    }
    return Error("unknown root '" + name + "'");
  }

  internal HixInvocationResult InvokeGlobal(string name, IHixValue[] arguments) {
    var started = Stopwatch.GetTimestamp();
    directInvocation = true;
    try {
      var value = Invoke(name, arguments, 0);
      var error = value as ErrorHixValue;
      return new HixInvocationResult(value, new HixExecutionResult(error == null,
        error == null ? default : error.Message, 0, outputs.ToArray(), logs.ToArray(),
        executedOperations: steps, executionMilliseconds: Elapsed(started), strings: Strings));
    } finally { directInvocation = false; }
  }

  public IHixValue Invoke(string name, IHixValue[] arguments, int line, SignatureHixPattern requested = null,
    LanguageFunctionScope binding = null) {
    using var profile = HixProfiler.Measure("vm.invoke");
    var selectionError = SelectFunction(name, arguments, binding ?? scope, out var match, requested);
    if (selectionError != null) return selectionError;
    return InvokeMatch(name, arguments, line, match);
  }

  private IHixValue InvokeMatch(string name, IHixValue[] arguments, int line, FunctionMatch match) {
    if (pure && !match.Function.IsPure) return Error("pure functions cannot invoke impure functions");
    if (!directInvocation && !prelude && !match.Function.IsPure) return Error("impure function '" + name + "' requires prelude preparation");
    if (++depth > 128) { depth--; return Error("call depth limit exceeded"); }
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
    var hostEffects = Context.CaptureEffects();
    var logCount = logs.Count;
    locals = machine.RentLocals();
    parameter = match.Parameter;
    positionalParameters = match.Arguments ?? arguments;
    pure = match.Function.IsPure;
    scope = match.Owner;
    program = scope.Program;
    try {
      var completion = Run(match.Function.Body);
      if (completion.Kind == BytecodeFlow.Error) { Rollback(savedVariables, savedTargets, savedCarries, outputCount, logCount, hostEffects); return completion.Value; }
      if (completion.Kind is not (BytecodeFlow.Normal or BytecodeFlow.Return)) {
        Rollback(savedVariables, savedTargets, savedCarries, outputCount, logCount, hostEffects);
        return Error("control flow cannot cross function boundaries: " + completion.Kind);
      }
      var result = completion.Kind == BytecodeFlow.Return ? completion.Value : NullHixValue.Instance;
      if (result is ErrorHixValue) { Rollback(savedVariables, savedTargets, savedCarries, outputCount, logCount, hostEffects); return result; }
      if (match.Signature != null && !MatchesReturn(result, match.Signature, out var returnFailure)) {
        Rollback(savedVariables, savedTargets, savedCarries, outputCount, logCount, hostEffects);
        return Error("return pattern does not match signature of '" + name + "': " + returnFailure);
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

  private IHixValue InvokePrepared(ResolvedFunctionHixValue prepared, IHixValue[] arguments, int line) {
    using var profile = HixProfiler.Measure("vm.invoke_prepared");
    if (prepared.Backend != null) {
      if (!FunctionDefinition.TryConvertValues(this, prepared.BackendSignature, arguments, out var converted))
        return Error("no matching function '" + prepared.Backend.Name + "' for " + arguments.Length + " arguments");
      return ExecuteBackend(prepared.Backend, converted, line);
    }
    var candidate = prepared.Language;
    var selectionError = SelectFunction(candidate.Function.Name, arguments, candidate.Owner, out var match,
      prepared: candidate);
    return selectionError ?? InvokeMatch(candidate.Function.Name, arguments, line, match);
  }

  private bool MatchesReturn(IHixValue value, BytecodeSignature signature, out HixPatternFailure failure) {
    if (signature.Outputs == null)
      return HixPatternMatcher.Matches(signature.OutputPattern, value, this, program.Patterns, out failure);
    var pattern = new TableHixPattern(signature.Outputs.Select(field => field.AsPatternField()).ToArray());
    return HixPatternMatcher.Matches(pattern, value, this, program.Patterns, out failure);
  }
  private static bool MatchesKind(IHixValue value, string kind) => kind == "any" || value.Kind.ToString().Equals(kind, StringComparison.OrdinalIgnoreCase);
  private bool MatchesPattern(IHixValue value, HixPattern pattern) =>
    HixPatternMatcher.Matches(pattern, value, this, program.Patterns, out _);
  private bool TryConvert(IHixValue value, HixPattern pattern, out IHixValue converted, out int conversions) {
    if (MatchesPattern(value, pattern)) {
      converted = value;
      conversions = 0;
      return true;
    }
    if (pattern is not KindHixPattern kind ||
      !KindHixValue.TryGet(kind.Display, out var target) ||
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
    int outputCount, int logCount, int hostEffects) {
    Restore(variables, savedVariables);
    Restore(targetVariables, savedTargets);
    Restore(carries, savedCarries);
    outputs.RemoveRange(outputCount, outputs.Count - outputCount);
    Context.RollbackEffects(hostEffects);
    logs.RemoveRange(logCount, logs.Count - logCount);
  }

  private static void Restore(HixValueDictionary storage, PersistentMap<HixString, IHixValue> saved) =>
    storage.Restore(saved);
  private static IHixValue Pack(IHixValue[] values) => values.Length switch {
    0 => NullHixValue.Instance, 1 => values[0], _ => new TupleHixValue(values)
  };
  private IHixValue Import(object value) => value switch {
    null => NullHixValue.Instance, IHixValue typed => typed, string text => String(HixString.Dynamic(text)),
    bool boolean => Bool(boolean), double number => new NumberHixValue(number),
    int number => new NumberHixValue(number), long number => new NumberHixValue(number),
    IReadOnlyDictionary<string, object> table => new HixTableValue(table.Select(entry =>
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(entry.Key), Import(entry.Value))).ToArray()),
    IEnumerable<object> tuple => new TupleHixValue(tuple.Select(Import).ToArray()),
    _ => Backend.Import(this, value)
  };
  public string ResolveText(IHixValue value) => value switch {
    NullHixValue => "",
    _ => value.Render(this).Resolve(Strings)
  };
  public HixString Text(IHixValue value) => value is NullHixValue ? HixString.Empty : value.Render(this);
  public static LiteralHixValue String(HixString text) => new(text);
  public static LiteralHixValue String(string text) => new(HixString.Dynamic(text));
  public static BooleanHixValue Bool(bool value) => value ? BooleanHixValue.True : BooleanHixValue.False;
  public bool Equal(IHixValue left, IHixValue right) => (left, right) switch {
    (LiteralHixValue a, LiteralHixValue b) => a.Value == b.Value || ResolveText(a) == ResolveText(b),
    (TupleHixValue a, TupleHixValue b) => a.Values.Count == b.Values.Count && a.Values.Zip(b.Values, Equal).All(value => value),
    (HixTableValue a, HixTableValue b) => a.Count == b.Count && a.Entries.All(entry =>
      b.TryGetValue(this, entry.Key, out var value) && Equal(entry.Value, value)),
    _ => left.Equals(right)
  };

  public IHixValue RenderText(IHixValue value) => value is HixTableValue or TupleHixValue
    ? Error("collections require an explicit join") : value is LiteralHixValue ? value : String(Text(value));

  public HixThread(HixContext context = null) {
    Context = context ?? new HixContext();
  }

  public HixThread(HixBackend backend, HixStringPool strings = null) : this(new HixContext(backend, strings)) { }

  public HixContext Context { get; }
  public HixBackend Backend => Context.Backend;
  public bool IsPrelude => prelude;
  public IHixValue Invoke(IHixValue function, IHixValue[] arguments, int line = 0) =>
    IsRunning ? Callback(function, arguments, line) : Error("no active execution");
  public HixStringPool Strings => machine?.StringPool ?? Context.Strings;
  public HixVM Machine => machine;

  public IHixValue Resolve(HixExpressionRoot root, HixString member) {
    switch (root) {
      case HixExpressionRoot.True: return BooleanHixValue.True;
      case HixExpressionRoot.False: return BooleanHixValue.False;
      case HixExpressionRoot.Null: return NullHixValue.Instance;
      case HixExpressionRoot.Local:
        return locals != null && locals.TryGetValue(member, out var local)
          ? local
          : NullHixValue.Instance;
      case HixExpressionRoot.Variable:
        return variables.TryGetValue(member, out var variable)
          ? variable
          : NullHixValue.Instance;
      case HixExpressionRoot.TargetVariable:
        var targetName = member.Resolve(Strings);
        foreach (var item in targetVariables) {
          if (string.Equals(item.Key.Resolve(Strings), targetName, StringComparison.Ordinal))
            return item.Value;
        }
        return NullHixValue.Instance;
      case HixExpressionRoot.Parameter:
        return string.IsNullOrEmpty(member.Resolve(Strings))
          ? parameter
          : parameter.Select(this, member);
      case HixExpressionRoot.Table: return HixTableValue.Empty;
      default: return Context.ResolveHost(this, root, member);
    }
  }


  public IHixValue Evaluate(IHixValue value) => value ?? NullHixValue.Instance;

  public HixString Render(IHixValue value) {
    return Backend.Render(this, value);
  }

  public HixString NameOf(IHixValue value) => Backend.NameOf(this, value);

  public IHixValue Unwrap(IHixValue value) => Backend.Unwrap(this, value);

  public bool IsType(IHixValue value, HixString type) => Backend.IsType(this, value, type);

  public IHixValue Attributes(IHixValue value, HixString type, bool exact, bool first) => Backend.Attributes(this, value, type, exact, first);



  public bool HasTrait(IHixValue value, HixString trait) => Backend.HasTrait(this, value, trait);

  public object UnlinkSnapshot(IHixValue value) => Backend.UnlinkSnapshot(this, value);

  public IHixValue DetachValue(IHixValue value) => Backend.DetachValue(this, value);

  public IHixValue DetachCore(IHixValue value) {
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

  public HixString ResolveString(string value) => HixString.Dynamic(value ?? "");
  public ErrorHixValue Error(HixString value) => new(value.IsNull ? HixString.Empty : value);
  public ErrorHixValue Error(string value) => new(HixString.Dynamic(value ?? ""));

  private VmCompletion Run(int address) => new ExecutionFrame(this, address).Run();

  /// <summary>State owned by one block invocation. Nested blocks have independent stacks and handlers.</summary>
  private readonly struct ExecutionFrame {
    private readonly HixThread execution;
    private readonly int address;
    private readonly int end;
    private readonly HixInstruction header;
    private readonly IReadOnlyList<byte> code;
    private readonly List<IHixValue> stack;
    private readonly Stack<(int Target, int Stack)> checks;

    public ExecutionFrame(HixThread execution, int address) {
      this.execution = execution;
      this.address = address;
      code = execution.machine.Code(execution.program);
      header = HixInstruction.Decode(code, address);
      end = address + header.A;
      stack = new List<IHixValue>();
      checks = new Stack<(int, int)>();
    }

    private string Name(int index) => execution.machine.StringPool[index];
    private IHixValue Pop() {
      var index = stack.Count - 1;
      var value = stack[index];
      stack.RemoveAt(index);
      return value;
    }

    private IHixValue[] PopMany(int count) {
      if (count == 0) return Array.Empty<IHixValue>();
      var start = stack.Count - count;
      var values = new IHixValue[count];
      for (var i = 0; i < count; i++) values[i] = HixStorageValue.Capture(stack[start + i]);
      stack.RemoveRange(start, count);
      return values;
    }

    private VmCompletion Push(IHixValue value, int line) {
      if (value is ErrorHixValue {IsChecked: false}) return Failed(value, line);
      stack.Add(value);
      return default;
    }

    private void Reset() {
      stack.Clear(); checks.Clear();
    }

    public VmCompletion Run() {
      for (var pc = address; pc < end;) {
        var line = execution.program.SourceLines[pc];
        if (execution.steps >= StepLimit) return Failed(execution.Error("execution limit exceeded"), line);
        execution.steps++;
        var instruction = HixInstruction.Decode(code, pc);
        var instructionAddress = pc;
        pc += instruction.Size;
        var a = instruction.A; var b = instruction.B;
        var completion = default(VmCompletion);
        switch (instruction.Opcode) {
          case HixOpcode.End: return default;
          case HixOpcode.Enter: break;
          case HixOpcode.LoadConst: completion = Push(execution.machine.ConstantPool[a], line); break;
          case HixOpcode.LoadString: stack.Add(String(HixString.Interned(a))); break;
          case HixOpcode.LoadTrue: stack.Add(BooleanHixValue.True); break;
          case HixOpcode.LoadFalse: stack.Add(BooleanHixValue.False); break;
          case HixOpcode.LoadNull: stack.Add(NullHixValue.Instance); break;
          case HixOpcode.LoadTuple: stack.Add(TupleHixValue.Empty); break;
          case HixOpcode.LoadTable: stack.Add(HixTableValue.Empty); break;
          case HixOpcode.LoadRoot: completion = Push(execution.Root(Name(a)), line); break;
          case HixOpcode.Equal:
            var right = Pop(); var left = Pop();
            stack.Add(Bool(execution.Equal(left, right))); break;
          case HixOpcode.Member:
            var receiver = Pop();
            completion = Push(receiver is LiteralHixValue or NumberHixValue or BooleanHixValue or NullHixValue or KindHixValue
              ? execution.Error("member selection requires a container or host selector")
              : receiver.Select(execution, HixString.Dynamic(Name(a))), line); break;
          case HixOpcode.CheckStoreCarry:
          case HixOpcode.CheckStoreVariable:
          case HixOpcode.CheckStoreTarget:
          case HixOpcode.StoreCarry:
          case HixOpcode.StoreLocal:
          case HixOpcode.StoreVariable:
          case HixOpcode.StoreTarget:
            b = instruction.Opcode is HixOpcode.StoreVariable or HixOpcode.CheckStoreVariable ? 1
              : instruction.Opcode is HixOpcode.StoreTarget or HixOpcode.CheckStoreTarget ? 2 : 0;
            var isCarry = instruction.Opcode is HixOpcode.StoreCarry or HixOpcode.CheckStoreCarry;
            if (execution.pure && (b != 0 || isCarry)) { completion = Failed(execution.Error("pure functions cannot mutate shared storage"), line); break; }
            if (isCarry && (!execution.prelude || execution.depth != 0)) { completion = Failed(execution.Error("carry local assignments require a top-level prelude expression"), line); break; }
            if (instruction.Opcode is HixOpcode.CheckStoreCarry or HixOpcode.CheckStoreVariable or HixOpcode.CheckStoreTarget) break;
            var destination = b == 0 ? isCarry || execution.depth == 0 && execution.carries.ContainsKey(HixString.Dynamic(Name(a))) ? execution.carries : execution.locals
              : b == 1 ? execution.variables : execution.targetVariables;
            destination.StoreIsolated(HixString.Dynamic(Name(a)), HixStorageValue.Capture(Pop())); break;
          case HixOpcode.Pop: Pop(); break;
          case HixOpcode.PackTuple: stack.Add(new TupleHixValue(PopMany(a))); break;
          case HixOpcode.Pack: stack.Add(Pack(PopMany(a))); break;
          case HixOpcode.PackTable:
            var items = PopMany(a * 2);
            var entries = new KeyValuePair<HixString, IHixValue>[a];
            for (var i = 0; i < a; i++) entries[i] = new(((LiteralHixValue)items[i * 2]).Value, items[i * 2 + 1]);
            stack.Add(new HixTableValue(entries, execution.Strings)); break;
          case HixOpcode.Throw: completion = Failed(execution.Error(Name(a)), line); break;
          case HixOpcode.CastString:
            var rendered = execution.RenderText(Pop());
            if (rendered is ErrorHixValue) completion = Failed(rendered, line); else stack.Add(rendered);
            break;
          case HixOpcode.Interpolate: stack.Add(String(HixString.Dynamic(string.Concat(PopMany(a).Select(execution.ResolveText))))); break;
          case HixOpcode.Call:
            var prepared = (ResolvedFunctionHixValue)execution.machine.ConstantPool[a];
            var called = execution.InvokePrepared(prepared, PopMany(b), line);
            completion = execution.pendingControl; execution.pendingControl = default;
            if (completion.Kind == BytecodeFlow.Normal) completion = Push(called, line);
            break;
          case HixOpcode.CallDynamic:
            called = execution.Call(Name(a), PopMany(b), line);
            completion = execution.pendingControl; execution.pendingControl = default;
            if (completion.Kind == BytecodeFlow.Normal) completion = Push(called, line);
            break;
          case HixOpcode.CastBoolean:
            var boolean = Pop(); stack.Add(boolean is ErrorHixValue ? boolean : Bool(boolean.IsTruthy(execution))); break;
          case HixOpcode.Not: stack.Add(Bool(!Pop().IsTruthy(execution))); break;
          case HixOpcode.Check: checks.Push((instructionAddress + a, stack.Count)); break;
          case HixOpcode.EndCheck:
            checks.Pop();
            if (stack[stack.Count - 1] is ErrorHixValue checkedError)
              stack[stack.Count - 1] = checkedError with {IsChecked = true};
            break;
          case HixOpcode.Jump: pc = instructionAddress + a; break;
          case HixOpcode.JumpNotNull: if (stack[stack.Count - 1] is not NullHixValue) pc = instructionAddress + a; break;
          case HixOpcode.JumpFalse: if (!Pop().IsTruthy(execution)) pc = instructionAddress + a; break;
          case HixOpcode.Block: completion = execution.Run(instructionAddress + a); break;
          case HixOpcode.Return: completion = new(BytecodeFlow.Return, HixStorageValue.Capture(Pop()), Line: line); break;
          case HixOpcode.Goto: completion = new(BytecodeFlow.Goto, Target: instructionAddress + a, Line: line); break;
          case HixOpcode.InvalidGoto: completion = new(BytecodeFlow.Goto, Target: -1, Line: line); break;
          case HixOpcode.Break: completion = new(BytecodeFlow.Break, Line: line); break;
          case HixOpcode.Continue: completion = new(BytecodeFlow.Continue, Line: line); break;
          default: throw new InvalidOperationException("Invalid opcode at " + instructionAddress);
        }
        if (completion.Kind == BytecodeFlow.Error && checks.Count != 0) {
          var handler = checks.Pop();
          stack.RemoveRange(handler.Stack, stack.Count - handler.Stack);
          stack.Add(completion.Value is ErrorHixValue error ? error with {IsChecked = true} : completion.Value);
          pc = handler.Target;
          continue;
        }
        if (completion.Kind == BytecodeFlow.Break) return default;
        if (completion.Kind == BytecodeFlow.Continue) { Reset(); pc = address + header.Size; continue; }
        if (completion.Kind == BytecodeFlow.Goto && completion.Target > address && completion.Target < end) {
          Reset(); pc = completion.Target; continue;
        }
        if (completion.Kind != BytecodeFlow.Normal) return completion;
      }
      return default;
    }
  }

  private IHixValue Call(string name, IHixValue[] arguments, int line) {
    if (name is "var" or "tar" or "local") return ModifyStorage(name, arguments);
    if (program.Patterns.TryGetValue(name, out var pattern)) {
      if (arguments.Length != 1) return Error("pattern '" + name + "' expects one value");
      return HixPatternMatcher.Matches(pattern, arguments[0], this, program.Patterns, out var patternFailure)
        ? arguments[0] : Error("value does not match pattern '" + name + "': " + patternFailure);
    }

    if (scope.Contains(name)) return Invoke(name, arguments, line);
    var definitions = Backend.Functions.Resolve(name, arguments.Length);
    FunctionDefinition definition = null;
    IHixValue[] convertedArguments = null;
    var conversions = int.MaxValue;
    foreach (var candidate in definitions) {
      if (!candidate.TryConvertValues(this, arguments, out var converted, out var count) || count >= conversions) continue;
      definition = candidate;
      convertedArguments = converted;
      conversions = count;
      if (count == 0) break;
    }
    if (definition == null) return Error("no matching function '" + name + "' for " + arguments.Length + " arguments");
    return ExecuteBackend(definition, convertedArguments, line);
  }

  private IHixValue ModifyStorage(string name, IHixValue[] arguments) {
    if (name == "local") return Error("locals cannot be modified through the storage function");
    if (arguments.Length != 2) return Error("storage function '" + name + "' expects a key and value");
    if (pure) return Error("pure functions cannot mutate shared storage");
    var key = HixString.Dynamic(ResolveText(arguments[0]));
    var value = HixStorageValue.Capture(arguments[1]);
    (name == "var" ? variables : targetVariables).StoreIsolated(key, value);
    return value;
  }

  private IHixValue ExecuteBackend(FunctionDefinition definition, IHixValue[] arguments, int line) {
    var name = definition.Name;
    if (pure && definition.HasEffects) return Error("pure functions cannot perform '" + name + "'");
    if (!prelude && definition.RequiresPrelude) return Error("function '" + name + "' requires the prelude pass");
    if (!definition.AcceptsErrors)
      for (var i = 0; i < arguments.Length; i++)
        if (arguments[i] is ErrorHixValue failure) return failure with {IsChecked = false};
    try { return definition.Execute(this, arguments, line) ?? Error("invalid arguments for '" + name + "'"); }
    catch (ArgumentException exception) { return Error(exception.Message); }
    catch (OverflowException exception) { return Error(exception.Message); }
    catch (RegexMatchTimeoutException) { return Error("regular expression timed out"); }
  }

  public IHixValue Callback(IHixValue function, IHixValue[] arguments, int line) => function switch {
    ResolvedFunctionHixValue resolved => InvokePrepared(resolved, arguments, line),
    NamedFunctionHixValue named when named.Name.Length(named.Scope?.Program.StringPool ?? Strings) == 0 => NullHixValue.Instance,
    NamedFunctionHixValue named => Invoke(named.Name.Resolve(named.Scope?.Program.StringPool ?? Strings), arguments, line, binding: named.Scope),
    _ => Error("expected a function value")
  };

  private readonly record struct FunctionMatch(BytecodeFunction Function, BytecodeSignature Signature,
    IHixValue Parameter, IHixValue[] Arguments, LanguageFunctionScope Owner);

  // Only declaration metadata is reused. Conversion success can depend on argument values,
  // so a previous winner is never cached by argument kind alone.
  private IHixValue SelectFunction(string name, IHixValue[] arguments, LanguageFunctionScope binding,
    out FunctionMatch match, SignatureHixPattern requested = null, LanguageFunctionCandidate prepared = null) {
    using var profile = HixProfiler.Measure("vm.select_function");
    match = default;
    var candidates = prepared == null ? binding.Candidates(name) : null;
    var candidateCount = prepared == null ? candidates.Count : 1;
    var requestedDisplay = requested?.Display;
    if (candidateCount == 0) return Error("unknown function '" + name + "'");
    LanguageFunctionCandidate best = null;
    IHixValue[] bestArguments = null;
    IHixValue bestParameter = null;
    var bestScore = int.MinValue;
    var ambiguous = false;
    var matchedSignature = false;
    for (var candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++) {
      var candidate = prepared ?? candidates[candidateIndex];
      if (requestedDisplay != null && candidate.SignatureDisplay != requestedDisplay) continue;
      matchedSignature = true;
      // Conversion penalties can only reduce this score; equal scores must still
      // be examined to preserve ambiguity detection.
      if (candidate.BaseScore < bestScore) continue;
      var signature = candidate.Signature;
      var score = candidate.BaseScore;
      var convertedArguments = arguments;
      IHixValue suppliedParameter = null;
      if (signature is {Inputs: null, InputPattern: not null}) {
        if (arguments.Length != 1 || !TryConvert(arguments[0], signature.InputPattern, out suppliedParameter, out var count)) continue;
        score -= count;
      } else if (signature?.Inputs != null) {
        var fields = signature.Inputs;
        if (arguments.Length < candidate.FixedCount || !candidate.Variadic && arguments.Length > fields.Count) continue;
        score = candidate.BaseScore;
        var valid = true;
        for (var i = 0; i < arguments.Length; i++) {
          if (!TryConvert(arguments[i], fields[Math.Min(i, fields.Count - 1)].Pattern, out var converted, out var count)) {
            valid = false; break;
          }
          if (count != 0) {
            if (ReferenceEquals(convertedArguments, arguments)) convertedArguments = (IHixValue[])arguments.Clone();
            convertedArguments[i] = converted;
            score -= count;
          }
        }
        if (!valid) continue;
      }
      if (score < bestScore) continue;
      if (score == bestScore) { ambiguous = true; continue; }
      best = candidate;
      bestScore = score;
      bestArguments = convertedArguments;
      bestParameter = suppliedParameter;
      ambiguous = false;
    }
    if (!matchedSignature) return Error("unknown function '" + name + "'");
    if (best == null || ambiguous) return Error(best == null
      ? "no matching signature for '" + name + "'" : "ambiguous signature for '" + name + "'");

    // Allocate the parameter container only for the selected signature.
    if (bestParameter == null) {
      if (best.Signature == null || best.Signature.Inputs == null && best.Signature.InputPattern == null)
        bestParameter = Pack(arguments);
      else {
        var fields = best.Signature.Inputs;
        var entries = new KeyValuePair<HixString, IHixValue>[fields.Count];
        for (var i = 0; i < fields.Count; i++) {
          IHixValue value;
          if (fields[i].Variadic) {
            var tail = new IHixValue[bestArguments.Length - i];
            Array.Copy(bestArguments, i, tail, 0, tail.Length);
            value = new TupleHixValue(tail);
          } else value = i < bestArguments.Length ? bestArguments[i] : NullHixValue.Instance;
          entries[i] = new(ResolveString(fields[i].Name), value);
        }
        bestParameter = new HixTableValue(entries);
      }
    }
    match = new(best.Function, best.Signature, bestParameter, bestArguments, best.Owner);
    return null;
  }

  public IHixValue Derive(IHixValue value, int line) {
    if (value is not TupleHixValue tuple) return Error("derive requires a tuple of records");
    if (tuple.Values.Count == 0) return TupleHixValue.Empty;

    // Providers use normal bytecode frames, but the entire batch is one transaction.
    var savedVariables = variables.Snapshot();
    var savedTargets = targetVariables.Snapshot();
    var savedCarries = carries.Snapshot();
    var previousParameter = parameter;
    var previousLocals = locals;
    var previousScope = scope;
    var outputCount = outputs.Count;
    var hostEffects = Context.CaptureEffects();
    var result = new IHixValue[tuple.Values.Count];
    var index = 0;
    BytecodeDerivation provider = null;

    IHixValue Fail(IHixValue error, int errorLine, bool control = false) {
      Restore(variables, savedVariables);
      Restore(targetVariables, savedTargets);
      Restore(carries, savedCarries);
      outputs.RemoveRange(outputCount, outputs.Count - outputCount);
      Context.RollbackEffects(hostEffects);
      return control ? error : Error("derive entry " + index + (provider == null ? "" : " in '" + provider.Name + "'") +
        " at line " + errorLine + ": " + ResolveText(error));
    }

    try {
      for (; index < tuple.Values.Count; index++) {
        if (tuple.Values[index] is not HixTableValue record)
          return Fail(Error("record must be a table"), line);
        if (Backend.ValidateDerivationRecord(this, record) is { } invalidRecord)
          return Fail(invalidRecord, line);
        if (!record.TryGetValue(this, ResolveString("value"), out var current))
          return Fail(Error("record must contain value"), line);
        foreach (var declaration in derivations) {
          provider = declaration;
          if (!activeDerivations.Add(provider))
            return Fail(Error("recursive derivation"), provider.Line);
          locals = machine.RentLocals();
          scope = provider.Scope;
          try {
            foreach (var expression in provider.Expressions) {
              parameter = Functions.CollectionFunctions.Put(this, record, "value", current);
              var completion = Run(expression.Body);
              if (completion.Kind == BytecodeFlow.Error) return Fail(completion.Value, completion.Line);
              if (completion.Kind == BytecodeFlow.Normal)
                return Fail(Error("derivation must explicitly return a value"), expression.Line);
              if (completion.Kind != BytecodeFlow.Return)
                return Fail(Error("control flow cannot cross derivation boundaries: " + completion.Kind), completion.Line, true);
              current = completion.Value;
              if (current is ErrorHixValue) return Fail(current, expression.Line);
            }
          } finally {
            machine.ReturnLocals(locals);
            locals = previousLocals;
            activeDerivations.Remove(provider);
          }
        }
        result[index] = Functions.CollectionFunctions.Put(this, record, "value", current);
        provider = null;
      }
      return new TupleHixValue(result);
    } finally {
      parameter = previousParameter;
      locals = previousLocals;
      scope = previousScope;
    }
  }


  private enum BytecodeFlow { Normal, Return, Goto, Break, Continue, Error }

  private readonly record struct VmCompletion(BytecodeFlow Kind = BytecodeFlow.Normal, IHixValue Value = null,
  int Target = -1, int Line = 0
);
}
