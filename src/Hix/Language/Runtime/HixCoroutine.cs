using Hix.Compiler;
using Hix.Env;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Hix.Functions;
using System.Text.RegularExpressions;

namespace Hix.Runtime;

/// <summary>Heap-backed interpreter activation, cooperatively driven by its owning Hix worker.</summary>
public sealed class HixCoroutine : IDisposable {
    private readonly List<IHixValue> stack = [];
    private readonly List<(int Target, int Stack)> checks = [];
    private readonly List<ExecutionFrame> frames = [];
    private HixVM machine;
    private HixProgramImage program;
    private HixValueDictionary locals;
    private IHixValue parameter = NullHixValue.Instance;
    private IReadOnlyList<IHixValue> positionalParameters = Array.Empty<IHixValue>();
    private IHixValue argumentRoot = TupleHixValue.Empty;
    private int steps;
    private int depth;
    private bool pure;
    private const int StepLimit = 100000;
    private int running;
    private int driving;
    private int sliceRemaining = int.MaxValue;
    private bool sliceEnabled;
    private bool suspended;
    private HixTask<IHixValue> pendingOperation;
    private long coroutineStarted;
    private VmCompletion pendingControl;

    public HixCoroutine(HixContext context = null) {
        Context = context ?? HixCoreBackend.Instance.CreateContext();
    }

    public HixCoroutine(HixBackend backend, HixStringPool strings = null) : this(backend.CreateContext(strings)) {
    }

    private LanguageFunctionScope Scope => CurrentFunction?.Scope ?? machine.GlobalScope;

    private BytecodeFunctionEntry CurrentFunction {
        get {
            for (var index = frames.Count - 1; index >= 0; index--)
                if (frames[index].Function is { } function)
                    return function;
            return null;
        }
    }

    public IReadOnlyDictionary<string, HixPattern> PatternDefinitions => machine.Patterns;
    public HixTask<IHixValue> PendingOperation => pendingOperation;
    public bool IsCompleted { get; private set; }
    public HixInvocationResult Result { get; private set; }
    public bool IsRunning => Volatile.Read(ref running) != 0;
    public bool CanSuspend => sliceEnabled;
    private CallState CurrentCall => new(parameter, positionalParameters, argumentRoot, pure, depth);
    public HixContext Context { get; }
    public HixBackend Backend => machine?.Backend ?? Context.Backend;
    public bool IsPure => pure;
    public int CallDepth => depth;
    public int ExecutedOperations => steps;
    internal IHixValue Parameter => parameter;
    public HixStringPool Strings => machine?.StringPool ?? Context.Strings;
    public HixVM Machine => machine;
    public HixProgramImage Program => program;

    public NamedFunctionHixValue BindFunction(HixString name) => machine.Bind(Scope, name.Resolve(Strings));

    public bool Matches(HixPattern pattern, IHixValue value, out HixPatternFailure failure) => HixPatternMatcher.Matches(pattern, value, this, machine.Patterns, out failure);

    /// <summary>Abandon a parked coroutine and release its frames and context lease.</summary>
    public void Dispose() {
        if (Interlocked.CompareExchange(ref driving, 1, 0) != 0)
            throw new InvalidOperationException("Cannot dispose an executing coroutine");
        try {
            if (!IsRunning)
                return;
            try {
                while (frames.Count > 0)
                    CompleteFrame(frames[frames.Count - 1], Failed(Error("coroutine cancelled"), 0));
            } finally {
                IsCompleted = true;
                Stop();
            }
        } finally {
            Volatile.Write(ref driving, 0);
        }
    }

    /// <summary>Run a bounded slice on the calling thread. Returns true on completion.</summary>
    public bool Resume(int instructionBudget = 1000) {
        if (instructionBudget <= 0)
            throw new ArgumentOutOfRangeException(nameof(instructionBudget));
        if (Interlocked.CompareExchange(ref driving, 1, 0) != 0)
            throw new InvalidOperationException("A coroutine cannot be resumed concurrently");
        try {
            if (IsCompleted)
                return true;
            if (!IsRunning)
                throw new InvalidOperationException("The coroutine has not been prepared");
            sliceRemaining = instructionBudget;
            sliceEnabled = true;
            suspended = false;
            var completion = Drive(0);
            if (!suspended)
                FinishCoroutine(completion);
            return IsCompleted;
        } catch {
            if (IsRunning)
                Stop();
            IsCompleted = true;
            throw;
        } finally {
            sliceEnabled = false;
            Volatile.Write(ref driving, 0);
        }
    }

    public void Break(int line) => pendingControl = new(BytecodeFlow.Break, Line: line);

    public IHixValue Invoke(string name, IHixValue[] arguments, int line, SignatureHixPattern requested = null, LanguageFunctionScope binding = null) {
        using var profile = HixProfiler.Measure("vm.invoke");
        if (!machine.Contains(binding ?? Scope, name)) return Call(name, arguments, line, out _);
        var selectionError = SelectFunction(name, arguments, binding ?? Scope, out var match, requested);
        if (selectionError != null)
            return selectionError;
        return InvokeMatch(name, arguments, line, match);
    }

    public IHixValue Coerce(IHixValue value, HixPattern pattern) {
        if (TryConvert(value, pattern, out var converted, out _)) return converted;
        HixPatternMatcher.Matches(pattern, value, this, machine.Patterns, out var failure);
        return Error(failure?.ToString() ?? "value does not match pattern '" + pattern.Display + "'");
    }

    public IHixValue Invoke(IHixValue function, IHixValue[] arguments, int line = 0) => IsRunning ? Callback(function, arguments, line) : Error("no active execution");

    public IHixValue Resolve(HixExpressionRoot root, HixString member) {
        switch (root) {
            case HixExpressionRoot.True:
                return BooleanHixValue.True;
            case HixExpressionRoot.False:
                return BooleanHixValue.False;
            case HixExpressionRoot.Null:
                return NullHixValue.Instance;
            case HixExpressionRoot.Local:
                return locals != null && locals.TryGetValue(member, out var local) ? local : MissingHixValue.Instance;
            case HixExpressionRoot.Parameter:
                return string.IsNullOrEmpty(member.Resolve(Strings)) ? parameter : parameter.Select(this, member);
            case HixExpressionRoot.Table:
                return HixTableValue.Empty;
            default:
                return Context.Resolve(this, root, member);
        }
    }

    public IHixValue Evaluate(IHixValue value) => value ?? NullHixValue.Instance;

    public string ResolveText(IHixValue value) => value switch {
        NullHixValue => "",
        _ => value.Render(this).Resolve(Strings)};

    public HixString Text(IHixValue value) => value is NullHixValue ? HixString.Empty : value.Render(this);

    public bool Equal(IHixValue left, IHixValue right) => (left, right) switch {
        (LiteralHixValue a, LiteralHixValue b) => a.Value == b.Value || ResolveText(a) == ResolveText(b),
        (TupleHixValue a, TupleHixValue b) => a.Values.Count == b.Values.Count && a.Values.Zip(b.Values, Equal).All(value => value),
        (HixTableValue a, HixTableValue b) => a.Count == b.Count && a.Entries.All(entry => b.Entries.Any(candidate => (entry.Key == candidate.Key || entry.Key.Resolve(Strings) == candidate.Key.Resolve(Strings)) && Equal(entry.Value, candidate.Value))),
        _ => left.Equals(right)};

    public IHixValue RenderText(IHixValue value) => value is HixTableValue or TupleHixValue ? Error("collections require an explicit join") : value is LiteralHixValue ? value : String(Text(value));

    public HixString Render(IHixValue value) => Backend.Render(this, value);

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
            ErrorHixValue error => error with {
                Message = HixString.Dynamic(error.Message.Resolve(Strings))
            },
            LiteralHixValue literal when !literal.Value.IsInterned => literal,
            LiteralHixValue literal => new LiteralHixValue(HixString.Dynamic(literal.Value.Resolve(Strings))),
            TupleHixValue tuple => tuple.Transform(DetachValue),
            HixTableValue table => new HixTableValue([..table.Entries.Select(item => new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(item.Key.Resolve(Strings)), DetachValue(item.Value)))]),
            _ => value
        };
    }

    public HixString ResolveString(string value) => HixString.Dynamic(value ?? "");

    public ErrorHixValue Error(HixString value) => new(value.IsNull ? HixString.Empty : value);

    public ErrorHixValue Error(string value) => new(HixString.Dynamic(value ?? ""));

    public static LiteralHixValue String(HixString text) => new(text);

    public static LiteralHixValue String(string text) => new(HixString.Dynamic(text));

    public static BooleanHixValue Bool(bool value) => value ? BooleanHixValue.True : BooleanHixValue.False;

    public IHixValue Callback(IHixValue function, IHixValue[] arguments, int line) => function switch {
        ResolvedFunctionHixValue resolved => InvokePrepared(resolved, arguments, line),
        NamedFunctionHixValue named when named.Name.Length(named.Scope?.Program.StringPool ?? Strings) == 0 => NullHixValue.Instance,
        NamedFunctionHixValue named => Invoke(named.Name.Resolve(named.Scope?.Program.StringPool ?? Strings), arguments, line, binding: named.Scope),
        _ => Error("expected a function value")};

    internal void Prepare(HixVM vm, HixProgramImage image, string function, IHixValue[] arguments) {
        Prepare(vm, image, new NamedFunctionHixValue(HixString.Dynamic(function)), arguments);
    }

    internal void Prepare(HixVM vm, HixProgramImage image, IHixValue function, IHixValue[] arguments) {
        Start(vm, image);
        coroutineStarted = Stopwatch.GetTimestamp();
        try {
            LanguageFunctionCandidate candidate = (function as ResolvedFunctionHixValue)?.Language;
            var named = function as NamedFunctionHixValue;
            if (candidate == null && named == null) {
                FinishCoroutine(new(BytecodeFlow.Error, Error("coroutine entry requires a Hix function or lambda")));
                return;
            }
            var name = candidate?.Function.Name ?? named.Name.Resolve(named.Scope?.Program.StringPool ?? Strings);
            var error = SelectFunction(name, arguments, candidate?.Owner ?? named?.Scope ?? Scope, out var match, prepared: candidate);
            error ??= BeginInvocation(name, arguments, match);
            if (error != null)
                FinishCoroutine(new(BytecodeFlow.Error, error));
        } catch {
            Stop();
            throw;
        }
    }

    internal void Stop() {
        try {
            Context.FinishExecution(this);
        } finally {
            pure = false;
            Context.Release();
            Volatile.Write(ref running, 0);
        }
    }

    internal void Start(HixVM machine, HixProgramImage program) {
        if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
            throw new InvalidOperationException("A Hix thread is already running");
        try {
            Context.Acquire();
        } catch {
            Volatile.Write(ref running, 0);
            throw;
        }

        try {
            // Context seeds use their own immutable pool; committed values are already detached.
            this.machine = null;
            Context.BeforeExecution(this);
            this.machine = machine;
            this.program = program;
            Context.BeginExecution(this);
            parameter = Context.InitialParameter(this);
            positionalParameters = Array.Empty<IHixValue>();
            argumentRoot = parameter is NullHixValue ? TupleHixValue.Empty : parameter;
            pure = false;
            steps = depth = 0;
            IsCompleted = false;
            Result = default;
            pendingOperation = null;
            pendingControl = default;
        } catch {
            Stop();
            throw;
        }
    }

    internal HixInvocationResult InvokeGlobal(string name, IHixValue[] arguments) {
        var started = Stopwatch.GetTimestamp();
        var value = Invoke(name, arguments, 0);
        var error = value as ErrorHixValue;
        return new HixInvocationResult(value, Context.Result(this, error == null, error == null ? default : error.Message, 0, Elapsed(started)));
    }

    internal IHixValue Import(object value) => value switch {
        null => NullHixValue.Instance,
        IHixValue typed => typed,
        string text => String(HixString.Dynamic(text)),
        bool boolean => Bool(boolean),
        double number => new NumberHixValue(number),
        int number => new NumberHixValue(number),
        long number => new NumberHixValue(number),
        IReadOnlyDictionary<string, object> table => new HixTableValue(table.Select(entry => new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(entry.Key), Import(entry.Value))).ToArray()),
        IEnumerable<object> tuple => new TupleHixValue(tuple.Select(Import).ToArray()),
        _ => Backend.Import(this, value)};

    internal bool IsExecuting(BytecodeFunction function) {
        foreach (var frame in frames)
            if (ReferenceEquals(frame.Function?.Function, function))
                return true;
        return false;
    }

    internal VmCompletion ExecuteFunction(BytecodeFunction function, LanguageFunctionScope owner, IHixValue parameter, HixValueDictionary sharedLocals = null) => RunFunction(function, owner, CurrentCall with { Parameter = parameter }, sharedLocals);

    private void FinishCoroutine(VmCompletion completion) {
        var value = completion.Value ?? NullHixValue.Instance;
        var error = value as ErrorHixValue;
        try {
            Result = new HixInvocationResult(value, Context.Result(this, error == null, error == null ? default : error.Message, completion.Line, Elapsed(coroutineStarted)));
            IsCompleted = true;
        } finally {
            Stop();
        }
    }

    private static VmCompletion Failed(IHixValue error, int line) => new(BytecodeFlow.Error, error, Line: line);

    private IHixValue Root(string name) {
        if (name == "args")
            return argumentRoot;
        if (name == "param")
            return parameter;
        if (name == "null") return NullHixValue.Instance;
        if (name == "missing") return MissingHixValue.Instance;
        if (HixPatterns.TryPrimitive(name, out var primitive))
            return primitive;
        if (machine.Patterns.TryGetValue(name, out var pattern))
            return new PatternHixValue(new NamedHixPattern(name));
        if (name is not ("local" or "var" or "tar") && machine.Bind(Scope, name)is { } function)
            return function;
        if (Backend.Roots.TryGetValue(name, out var root)) {
            if (pure && root.HasEffects)
                return Error("pure functions cannot read host roots");
            if (Context.ValidateAvailability(this, name, root.RequiresPrelude, root: true)is { } unavailable)
                return unavailable;
            return Backend.ResolveRoot(this, name);
        }

        return Context.ResolveRoot(this, name, locals);
    }

    private IHixValue InvokeMatch(string name, IHixValue[] arguments, int line, FunctionMatch match) {
        var boundary = frames.Count;
        if (BeginInvocation(name, arguments, match)is { } error)
            return error;
        var completion = Drive(boundary);
        return completion.Kind == BytecodeFlow.Error ? completion.Value : completion.Value ?? NullHixValue.Instance;
    }

    private IHixValue BeginInvocation(string name, IHixValue[] arguments, FunctionMatch match) {
        if (!ReferenceEquals(program.Bytecode, match.Owner.Program.Bytecode))
            return Error("Execution state contains a function from another program image");
        if (pure && !match.Function.IsPure)
            return Error("pure functions cannot invoke impure functions");
        if (Context.ValidateLanguageCall(this, match.Function)is { } unavailable)
            return unavailable;
        if (depth >= 128)
            return Error("call depth limit exceeded");
        var supplied = match.Arguments ?? arguments;
        var call = new CallState(match.Parameter, supplied, new TupleHixValue(supplied), match.Function.IsPure, depth + 1);
        BeginFunction(match.Function, match.Owner, call, signature: match.Signature, invocation: true);
        return null;
    }

    /// <summary>Enter an ordinary function and drain its heap frames immediately, without scheduling.</summary>
    private VmCompletion RunFunction(BytecodeFunction function, LanguageFunctionScope owner, CallState call, HixValueDictionary sharedLocals = null, BytecodeSignature signature = null) {
        var boundary = frames.Count;
        BeginFunction(function, owner, call, sharedLocals, signature);
        return Drive(boundary);
    }

    private IHixValue InvokePrepared(ResolvedFunctionHixValue prepared, IHixValue[] arguments, int line) {
        using var profile = HixProfiler.Measure("vm.invoke_prepared");
        if (prepared.Backend != null) {
            if (!HixFunctionEntry.TryConvertValues(this, prepared.BackendSignature, arguments, out var converted))
                return Error("no matching function '" + prepared.Backend.Name + "' for " + arguments.Length + " arguments");
            return ExecuteBackend(prepared.Backend, converted, line);
        }

        var candidate = prepared.Language;
        var selectionError = SelectFunction(candidate.Function.Name, arguments, candidate.Owner, out var match, prepared: candidate);
        return selectionError ?? InvokeMatch(candidate.Function.Name, arguments, line, match);
    }

    // Scheduling here means pushing a child frame, not dispatching onto another OS thread.
    private bool SchedulePrepared(ResolvedFunctionHixValue prepared, IHixValue[] arguments, int line, out IHixValue value) {
        if (prepared.Backend != null) {
            if (!HixFunctionEntry.TryConvertValues(this, prepared.BackendSignature, arguments, out var converted)) {
                value = Error("no matching function '" + prepared.Backend.Name + "' for " + arguments.Length + " arguments");
                return false;
            }

            return ScheduleBackend(prepared.Backend, converted, line, out value);
        }

        var candidate = prepared.Language;
        value = SelectFunction(candidate.Function.Name, arguments, candidate.Owner, out var match, prepared: candidate);
        if (value != null)
            return false;
        value = BeginInvocation(candidate.Function.Name, arguments, match);
        return value == null;
    }

    private bool ScheduleCallback(IHixValue function, IHixValue[] arguments, int line, out IHixValue value) {
        if (function is ResolvedFunctionHixValue resolved)
            return SchedulePrepared(resolved, arguments, line, out value);
        if (function is NamedFunctionHixValue named) {
            var name = named.Name.Resolve(named.Scope?.Program.StringPool ?? Strings);
            if (name.Length == 0) {
                value = NullHixValue.Instance;
                return false;
            }

            if (!machine.Contains(named.Scope ?? Scope, name)) {
                value = Call(name, arguments, line, out var scheduled);
                return scheduled;
            }
            value = SelectFunction(name, arguments, named.Scope ?? Scope, out var match);
            if (value != null)
                return false;
            value = BeginInvocation(name, arguments, match);
            return value == null;
        }

        value = Error("expected a function value");
        return false;
    }

    private bool ScheduleBackend(HixFunctionEntry definition, IHixValue[] arguments, int line, out IHixValue value) {
        value = CheckBackend(definition, arguments);
        if (value != null)
            return false;
        if (definition.Handler is CallbackFunction)
            return ScheduleCallback(arguments[0], arguments.Skip(1).ToArray(), line, out value);
        if (definition.Handler is CatchFunction && arguments.Length == 2 && arguments[0] is ErrorHixValue)
            return ScheduleCallback(arguments[1], [arguments[0]], line, out value);
        value = ExecuteBackendUnchecked(definition, arguments, line);
        return false;
    }

    private bool MatchesReturn(IHixValue value, BytecodeSignature signature, out HixPatternFailure failure) {
        if (signature.Outputs == null)
            return HixPatternMatcher.Matches(signature.OutputPattern, value, this, machine.Patterns, out failure);
        var pattern = new TableHixPattern(signature.Outputs.Select(field => field.AsPatternField()).ToArray());
        return HixPatternMatcher.Matches(pattern, value, this, machine.Patterns, out failure);
    }

    private static bool MatchesKind(IHixValue value, string kind) => kind == "any" || value.Kind.ToString().Equals(kind, StringComparison.OrdinalIgnoreCase);

    private bool MatchesPattern(IHixValue value, HixPattern pattern) => HixPatternMatcher.Matches(pattern, value, this, machine.Patterns, out _);

    private bool TryConvert(IHixValue value, HixPattern pattern, out IHixValue converted, out int conversions) {
        if (value is MissingHixValue && AcceptsKind(pattern, HixValueKind.Null)) {
            converted = NullHixValue.Instance;
            conversions = 1;
            return true;
        }

        if (MatchesPattern(value, pattern)) {
            converted = value;
            conversions = 0;
            return true;
        }

        if (pattern is UnionHixPattern union) {
            foreach (var member in union.Patterns)
                if (TryConvert(value, member, out converted, out conversions))
                    return true;
            converted = null;
            conversions = 0;
            return false;
        }

        if (value is NullHixValue || pattern is not KindHixPattern kind || !KindHixValue.TryGet(kind.Display, out var target) || !KindDefinitions.TryImplicitConvert(this, value, target.ValueKind, out converted)) {
            converted = null;
            conversions = 0;
            return false;
        }

        conversions = 1;
        return true;
    }

    private static bool AcceptsKind(HixPattern pattern, HixValueKind kind) => pattern switch {
        KindHixPattern value => value.ValueKind == kind,
        UnionHixPattern union => union.Patterns.Any(member => AcceptsKind(member, kind)),
        DocumentedHixPattern documented => AcceptsKind(documented.Underlying, kind),
        ConstrainedHixPattern constrained => AcceptsKind(constrained.Underlying, kind),
        _ => false
    };

    private static double Elapsed(long started) => (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency;

    private static IHixValue Pack(IHixValue[] values) => values.Length switch {
        0 => NullHixValue.Instance,
        1 => values[0],
        _ => new TupleHixValue(values)};

    private IHixValue Call(string name, IHixValue[] arguments, int line, out bool scheduled) {
        scheduled = false;
        if (Context.IsReservedCall(name))
            return Context.Call(this, name, arguments);
        if (machine.Contains(Scope, name))
            return Invoke(name, arguments, line);
        var definitions = Backend.Functions.Resolve(name, arguments.Length);
        HixFunctionEntry definition = null;
        IHixValue[] convertedArguments = null;
        var conversions = int.MaxValue;
        foreach (var candidate in definitions) {
            if (!candidate.TryConvertValues(this, arguments, out var converted, out var count) || count >= conversions)
                continue;
            definition = candidate;
            convertedArguments = converted;
            conversions = count;
            if (count == 0)
                break;
        }

        if (definition == null)
            return Error("no matching function '" + name + "' for " + arguments.Length + " arguments");
        scheduled = ScheduleBackend(definition, convertedArguments, line, out var value);
        return value;
    }

    private IHixValue ExecuteBackend(HixFunctionEntry definition, IHixValue[] arguments, int line) {
        return CheckBackend(definition, arguments) ?? ExecuteBackendUnchecked(definition, arguments, line);
    }

    private IHixValue CheckBackend(HixFunctionEntry definition, IHixValue[] arguments) {
        var name = definition.Name;
        if (pure && definition.HasEffects)
            return Error("pure functions cannot perform '" + name + "'");
        if (Context.ValidateAvailability(this, name, false)is { } unavailable)
            return unavailable;
        return null;
    }

    private IHixValue ExecuteBackendUnchecked(HixFunctionEntry definition, IHixValue[] arguments, int line) {
        try {
            if (definition.Handler.ExecuteAsync(this, arguments, line)is { } operation) {
                if (!sliceEnabled || operation.IsCompleted)
                    return ReadAsyncResult(operation);
                pendingOperation = operation;
                return null;
            }

            return definition.Handler.Execute(this, arguments, line) ?? Error("invalid arguments for '" + definition.Name + "'");
        } catch (ArgumentException exception) {
            return Error(exception.Message);
        } catch (OverflowException exception) {
            return Error(exception.Message);
        } catch (RegexMatchTimeoutException) {
            return Error("regular expression timed out");
        }
    }

    private IHixValue ReadAsyncResult(HixTask<IHixValue> operation) {
        try {
            return operation.GetResult() ?? Error("async function returned no value");
        } catch (ArgumentException exception) {
            return Error(exception.Message);
        } catch (OverflowException exception) {
            return Error(exception.Message);
        } catch (RegexMatchTimeoutException) {
            return Error("regular expression timed out");
        }
    }

    // Only declaration metadata is reused. Conversion success can depend on argument values,
    // so a previous winner is never cached by argument kind alone.
    private IHixValue SelectFunction(string name, IHixValue[] arguments, LanguageFunctionScope binding, out FunctionMatch match, SignatureHixPattern requested = null, LanguageFunctionCandidate prepared = null) {
        using var profile = HixProfiler.Measure("vm.select_function");
        match = default;
        var candidates = prepared == null ? machine.Candidates(binding, name) : null;
        var candidateCount = prepared == null ? candidates.Count : 1;
        var requestedIdentity = requested == null ? null : HixGenericTypes.SignatureIdentity(requested);
        if (candidateCount == 0)
            return Error("unknown function '" + name + "'");
        LanguageFunctionCandidate best = null;
        IHixValue[] bestArguments = null;
        IHixValue bestParameter = null;
        var bestScore = int.MinValue;
        var ambiguous = false;
        var matchedSignature = false;
        for (var candidateIndex = 0; candidateIndex < candidateCount; candidateIndex++) {
            var candidate = prepared ?? candidates[candidateIndex];
            if (requestedIdentity != null && candidate.SignatureIdentity != requestedIdentity)
                continue;
            matchedSignature = true;
            // Conversion penalties can only reduce this score; equal scores must still
            // be examined to preserve ambiguity detection.
            if (candidate.BaseScore < bestScore)
                continue;
            var signature = candidate.Signature;
            var score = candidate.BaseScore;
            var convertedArguments = arguments;
            IHixValue suppliedParameter = null;
            if (signature is { Inputs: null, InputPattern: not null }) {
                if (arguments.Length != 1 || !TryConvert(arguments[0], signature.InputPattern, out suppliedParameter, out var count))
                    continue;
                score -= count;
            } else if (signature?.Inputs != null) {
                var fields = signature.Inputs;
                if (arguments.Length < candidate.FixedCount || !candidate.Variadic && arguments.Length > fields.Count)
                    continue;
                if (!candidate.Variadic && arguments.Length < fields.Count) {
                    convertedArguments = new IHixValue[fields.Count];
                    Array.Copy(arguments, convertedArguments, arguments.Length);
                    for (var i = arguments.Length; i < fields.Count; i++)
                        convertedArguments[i] = fields[i].DefaultValue ?? (fields[i].Optional ? MissingHixValue.Instance : null);
                    if (convertedArguments.Any(value => value == null))
                        continue;
                }

                score = candidate.BaseScore;
                var valid = true;
                for (var i = 0; i < convertedArguments.Length; i++) {
                    var field = fields[Math.Min(i, fields.Count - 1)];
                    if (convertedArguments[i] is MissingHixValue && field.AllowsMissing && !field.Variadic) {
                        if (ReferenceEquals(convertedArguments, arguments))
                            convertedArguments = (IHixValue[])arguments.Clone();
                        convertedArguments[i] = field.DefaultValue ?? MissingHixValue.Instance;
                        if (field.DefaultValue == null)
                            continue;
                    }

                    if (!TryConvert(convertedArguments[i], field.Pattern, out var converted, out var count)) {
                        valid = false;
                        break;
                    }

                    if (count != 0) {
                        if (ReferenceEquals(convertedArguments, arguments))
                            convertedArguments = (IHixValue[])arguments.Clone();
                        convertedArguments[i] = converted;
                        score -= count;
                    }
                }

                if (!valid)
                    continue;
            }

            if (score < bestScore)
                continue;
            if (score == bestScore) {
                ambiguous = true;
                continue;
            }

            best = candidate;
            bestScore = score;
            bestArguments = convertedArguments;
            bestParameter = suppliedParameter;
            ambiguous = false;
        }

        if (!matchedSignature)
            return Error("unknown function '" + name + "'");
        if (best == null || ambiguous)
            return Error(best == null ? "no matching signature for '" + name + "'" : "ambiguous signature for '" + name + "'");
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
                    } else
                        value = i < bestArguments.Length ? bestArguments[i] : NullHixValue.Instance;
                    entries[i] = new(ResolveString(fields[i].Name), value);
                }

                bestParameter = new HixTableValue(entries);
            }
        }

        match = new(best.Function, best.Signature, bestParameter, bestArguments, best.Owner);
        return null;
    }

    private void BeginFunction(BytecodeFunction function, LanguageFunctionScope owner, CallState call, HixValueDictionary sharedLocals = null, BytecodeSignature signature = null, bool invocation = false) {
        var frame = new ExecutionFrame(this, function.Body) {
            Function = machine.Function(function, owner),
            Signature = signature,
            Invocation = invocation,
            PreviousCall = CurrentCall,
            PreviousLocals = locals,
            PreviousProgram = program,
            OwnsLocals = sharedLocals == null,
            Checkpoint = invocation ? Context.CaptureCheckpoint(this) : default
        };
        frame.LocalStorage = sharedLocals ?? HixThread.Current.RentLocals();
        locals = frame.LocalStorage;
        parameter = call.Parameter;
        positionalParameters = call.Arguments;
        argumentRoot = call.Root;
        pure = call.Pure;
        depth = call.Depth;
        program = owner.Program;
        frames.Add(frame);
    }

    // Reentrant host callbacks drain only their own frames, leaving their caller parked on the heap.
    private VmCompletion Drive(int boundary) {
        var completion = default(VmCompletion);
        var returning = false;
        var previousSliceEnabled = sliceEnabled;
        // Synchronous C# callbacks cannot yield their native call stack.
        if (boundary != 0)
            sliceEnabled = false;
        var yielded = false;
        try {
            while (frames.Count > boundary) {
                var frame = frames[frames.Count - 1];
                FrameStep step;
                if (returning) {
                    returning = false;
                    if (!frame.ResumeChild(completion, out completion))
                        continue;
                    step = completion;
                } else
                    step = frame.RunInstructions();
                if (step.Suspended) {
                    yielded = suspended = true;
                    return default;
                }

                if (step.ChildScheduled)
                    continue;
                completion = CompleteFrame(frame, step.Completion);
                returning = true;
            }

            return completion;
        } finally {
            sliceEnabled = previousSliceEnabled;
            if (!yielded)
                while (frames.Count > boundary)
                    ReleaseFrame(frames[frames.Count - 1]);
        }
    }

    private VmCompletion CompleteFrame(ExecutionFrame frame, VmCompletion completion) {
        try {
            if (frame.Invocation) {
                if (completion.Kind is not (BytecodeFlow.Normal or BytecodeFlow.Return or BytecodeFlow.Error))
                    completion = Failed(Error("control flow cannot cross function boundaries: " + completion.Kind), completion.Line);
                var result = completion.Kind == BytecodeFlow.Normal ? NullHixValue.Instance : completion.Value;
                if (result is ErrorHixValue)
                    completion = Failed(result, completion.Line);
                else if (frame.Signature != null && completion.Kind != BytecodeFlow.Error && !MatchesReturn(result, frame.Signature, out var failure))
                    completion = Failed(Error("return pattern does not match signature of '" + frame.Function.Function.Name + "': " + failure), 0);
                if (completion.Kind == BytecodeFlow.Error) {
                    Context.RollbackCheckpoint(this, frame.Checkpoint);
                } else
                    completion = new(BytecodeFlow.Return, result);
            }

            return completion;
        } finally {
            ReleaseFrame(frame);
        }
    }

    private void ReleaseFrame(ExecutionFrame frame) {
        frame.Reset();
        if (frame.Function != null) {
            if (frame.OwnsLocals)
                HixThread.Current.ReturnLocals(frame.LocalStorage);
            locals = frame.PreviousLocals;
            parameter = frame.PreviousCall.Parameter;
            positionalParameters = frame.PreviousCall.Arguments;
            argumentRoot = frame.PreviousCall.Root;
            pure = frame.PreviousCall.Pure;
            depth = frame.PreviousCall.Depth;
            program = frame.PreviousProgram;
        }

        frames.RemoveAt(frames.Count - 1);
    }

    private readonly record struct CallState(IHixValue Parameter, IReadOnlyList<IHixValue> Arguments, IHixValue Root, bool Pure, int Depth);
    /// <summary>A heap frame owns only slices of the thread's reusable operand and handler buffers.</summary>
    private sealed class ExecutionFrame {
        internal BytecodeFunctionEntry Function;
        internal BytecodeSignature Signature;
        internal bool Invocation;
        internal bool OwnsLocals;
        internal HixValueDictionary LocalStorage;
        internal CallState PreviousCall;
        internal HixValueDictionary PreviousLocals;
        internal HixProgramImage PreviousProgram;
        internal HixContextCheckpoint Checkpoint;
        private readonly HixCoroutine execution;
        private readonly int address;
        private readonly int end;
        private readonly HixInstruction header;
        private readonly IReadOnlyList<byte> code;
        private readonly int stackBase;
        private readonly int checkBase;
        private int stackCount;
        private int checkCount;
        private int pc;
        private bool awaitingValue;
        private bool awaitingAsync;
        private int callLine;

        public ExecutionFrame(HixCoroutine execution, int address) {
            this.execution = execution;
            this.address = address;
            code = execution.machine.Code(execution.program);
            header = HixInstruction.Decode(code, address);
            end = address + header.A;
            stackBase = execution.stack.Count;
            checkBase = execution.checks.Count;
            stackCount = checkCount = 0;
            pc = address;
        }

        private IHixValue Top { get => execution.stack[stackBase + stackCount - 1]; set => execution.stack[stackBase + stackCount - 1] = value; }

        internal void Reset() {
            Truncate(0);
            execution.checks.RemoveRange(checkBase, checkCount);
            checkCount = 0;
        }

        internal bool ResumeChild(VmCompletion completion, out VmCompletion terminal) {
            if (awaitingValue) {
                completion = Push(completion.Value ?? NullHixValue.Instance, callLine);
                awaitingValue = false;
            }

            terminal = completion;
            return HandleCompletion(ref terminal);
        }

        internal FrameStep RunInstructions() {
            if (awaitingAsync) {
                if (!execution.pendingOperation.IsCompleted)
                    return new(false, default, true);
                var operation = execution.pendingOperation;
                execution.pendingOperation = null;
                awaitingAsync = false;
                var completed = Push(execution.ReadAsyncResult(operation), callLine);
                if (HandleCompletion(ref completed))
                    return completed;
            }

            while (pc < end) {
                if (execution.sliceEnabled && execution.sliceRemaining-- <= 0)
                    return new(false, default, true);
                var line = execution.program.SourceLines[pc];
                if (execution.steps >= StepLimit)
                    return Failed(execution.Error("execution limit exceeded"), line);
                execution.steps++;
                var instruction = HixInstruction.Decode(code, pc);
                var instructionAddress = pc;
                pc += instruction.Size;
                var a = instruction.A;
                var b = instruction.B;
                var completion = default(VmCompletion);
                switch (instruction.Opcode) {
                    case HixOpcode.End:
                        return default;
                    case HixOpcode.Enter:
                        break;
                    case HixOpcode.LoadConst:
                        completion = Push(execution.machine.ConstantPool[a], line);
                        break;
                    case HixOpcode.LoadString:
                        Add(String(HixString.Interned(a)));
                        break;
                    case HixOpcode.LoadTrue:
                        Add(BooleanHixValue.True);
                        break;
                    case HixOpcode.LoadFalse:
                        Add(BooleanHixValue.False);
                        break;
                    case HixOpcode.LoadMissing:
                        Add(MissingHixValue.Instance);
                        break;
                    case HixOpcode.LoadNull:
                        Add(NullHixValue.Instance);
                        break;
                    case HixOpcode.LoadTuple:
                        Add(TupleHixValue.Empty);
                        break;
                    case HixOpcode.LoadTable:
                        Add(HixTableValue.Empty);
                        break;
                    case HixOpcode.LoadRoot:
                        completion = Push(execution.Root(Name(a)), line);
                        break;
                    case HixOpcode.Equal:
                        var right = Pop();
                        var left = Pop();
                        Add(Bool(execution.Equal(left, right)));
                        break;
                    case HixOpcode.Member:
                        var receiver = Pop();
                        completion = Push(receiver is LiteralHixValue or NumberHixValue or BooleanHixValue or NullHixValue or KindHixValue ? execution.Error("member selection requires a container or host selector") : receiver.Select(execution, HixString.Dynamic(Name(a))), line);
                        break;
                    case HixOpcode.CheckStoreCarry:
                    case HixOpcode.StoreCarry:
                    case HixOpcode.StoreLocal:
                        var stored = execution.Context.Store(execution, instruction.Opcode, HixString.Dynamic(Name(a)), instruction.Opcode == HixOpcode.CheckStoreCarry ? null : Pop(), execution.locals);
                        if (stored is ErrorHixValue)
                            completion = Failed(stored, line);
                        break;
                    case HixOpcode.Pop:
                        Pop();
                        break;
                    case HixOpcode.PackTuple:
                        Add(new TupleHixValue(PopMany(a)));
                        break;
                    case HixOpcode.Pack:
                        Add(Pack(PopMany(a)));
                        break;
                    case HixOpcode.PackTable:
                        var items = PopMany(a * 2);
                        var entries = new KeyValuePair<HixString, IHixValue>[a];
                        for (var i = 0; i < a; i++)
                            entries[i] = new(((LiteralHixValue)items[i * 2]).Value, items[i * 2 + 1]);
                        Add(new HixTableValue(entries, execution.Strings));
                        break;
                    case HixOpcode.Throw:
                        completion = Failed(execution.Error(Name(a)), line);
                        break;
                    case HixOpcode.CastString:
                        var rendered = execution.RenderText(Pop());
                        if (rendered is ErrorHixValue)
                            completion = Failed(rendered, line);
                        else
                            Add(rendered);
                        break;
                    case HixOpcode.Interpolate:
                        Add(String(HixString.Dynamic(string.Concat(PopMany(a).Select(execution.ResolveText)))));
                        break;
                    case HixOpcode.Call:
                        var prepared = (ResolvedFunctionHixValue)execution.machine.ConstantPool[a];
                        if (execution.SchedulePrepared(prepared, PopMany(b), line, out var called)) {
                            awaitingValue = true;
                            callLine = line;
                            return new(true, default);
                        }

                        completion = execution.pendingControl;
                        execution.pendingControl = default;
                        if (execution.pendingOperation != null) {
                            awaitingAsync = true;
                            callLine = line;
                            return new(false, default, true);
                        }

                        if (completion.Kind == BytecodeFlow.Normal)
                            completion = Push(called, line);
                        break;
                    case HixOpcode.CastBoolean:
                        var boolean = Pop();
                        Add(boolean is ErrorHixValue ? boolean : Bool(boolean.IsTruthy(execution)));
                        break;
                    case HixOpcode.Not:
                        Add(Bool(!Pop().IsTruthy(execution)));
                        break;
                    case HixOpcode.Check:
                        PushCheck(instructionAddress + a);
                        break;
                    case HixOpcode.EndCheck:
                        PopCheck();
                        if (Top is ErrorHixValue checkedError)
                            Top = checkedError with {
                                IsChecked = true
                            };
                        break;
                    case HixOpcode.Jump:
                        pc = instructionAddress + a;
                        break;
                    case HixOpcode.JumpNotNull:
                        if (Top is not (MissingHixValue or NullHixValue))
                            pc = instructionAddress + a;
                        break;
                    case HixOpcode.JumpFalse:
                        if (!Pop().IsTruthy(execution))
                            pc = instructionAddress + a;
                        break;
                    case HixOpcode.Block:
                        execution.frames.Add(new ExecutionFrame(execution, instructionAddress + a));
                        return new(true, default);
                    case HixOpcode.Return:
                        completion = new(BytecodeFlow.Return, HixStorageValue.Capture(Pop()), Line: line);
                        break;
                    case HixOpcode.Goto:
                        completion = new(BytecodeFlow.Goto, Target: instructionAddress + a, Line: line);
                        break;
                    case HixOpcode.InvalidGoto:
                        completion = new(BytecodeFlow.Goto, Target: -1, Line: line);
                        break;
                    case HixOpcode.Break:
                        completion = new(BytecodeFlow.Break, Line: line);
                        break;
                    case HixOpcode.Continue:
                        completion = new(BytecodeFlow.Continue, Line: line);
                        break;
                    default:
                        throw new InvalidOperationException("Invalid opcode at " + instructionAddress);
                }

                if (HandleCompletion(ref completion))
                    return completion;
            }

            return default;
        }

        private string Name(int index) => execution.machine.StringPool[index];

        private void Add(IHixValue value) {
            execution.stack.Add(value);
            stackCount++;
        }

        private void Truncate(int count) {
            execution.stack.RemoveRange(stackBase + count, stackCount - count);
            stackCount = count;
        }

        private void PushCheck(int target) {
            execution.checks.Add((target, stackCount));
            checkCount++;
        }

        private (int Target, int Stack) PopCheck() {
            var index = checkBase + --checkCount;
            var handler = execution.checks[index];
            execution.checks.RemoveAt(index);
            return handler;
        }

        private IHixValue Pop() {
            var index = stackBase + --stackCount;
            var value = execution.stack[index];
            execution.stack.RemoveAt(index);
            return value;
        }

        private IHixValue[] PopMany(int count) {
            if (count == 0)
                return Array.Empty<IHixValue>();
            var start = stackCount - count;
            var values = new IHixValue[count];
            for (var i = 0; i < count; i++)
                values[i] = HixStorageValue.Capture(execution.stack[stackBase + start + i]);
            Truncate(start);
            return values;
        }

        private VmCompletion Push(IHixValue value, int line) {
            if (value is ErrorHixValue { IsChecked: false })
                return Failed(value, line);
            Add(value);
            return default;
        }

        private bool HandleCompletion(ref VmCompletion completion) {
            if (completion.Kind == BytecodeFlow.Error && checkCount != 0) {
                var handler = PopCheck();
                Truncate(handler.Stack);
                Add(completion.Value is ErrorHixValue error ? error with { IsChecked = true } : completion.Value);
                pc = handler.Target;
                return false;
            }

            if (completion.Kind == BytecodeFlow.Break) {
                completion = default;
                return true;
            }

            if (completion.Kind == BytecodeFlow.Continue) {
                Reset();
                pc = address + header.Size;
                return false;
            }

            if (completion.Kind == BytecodeFlow.Goto && completion.Target > address && completion.Target < end) {
                Reset();
                pc = completion.Target;
                return false;
            }

            return completion.Kind != BytecodeFlow.Normal;
        }
    }

    private readonly record struct FunctionMatch(BytecodeFunction Function, BytecodeSignature Signature, IHixValue Parameter, IHixValue[] Arguments, LanguageFunctionScope Owner);
    private readonly record struct FrameStep(bool ChildScheduled, VmCompletion Completion, bool Suspended = false) {
        public static implicit operator FrameStep(VmCompletion completion) => new(false, completion);
    }

    internal enum BytecodeFlow {
        Normal,
        Return,
        Goto,
        Break,
        Continue,
        Error
    }

    internal readonly record struct VmCompletion(BytecodeFlow Kind = BytecodeFlow.Normal, IHixValue Value = null, int Target = -1, int Line = 0);
}
