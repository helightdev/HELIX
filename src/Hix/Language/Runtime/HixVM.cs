using Hix.Compiler;
using Hix.Env;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Hix.Runtime;

/// <summary>A loaded bytecode VM. All loaded programs share immutable VM-wide string and constant pools.</summary>
public sealed class HixVM : IDisposable {
    private readonly HixThread[] workers;
    private int nextWorker;
    private readonly ConcurrentQueue<HixThread.Work> submissions = new();
    private readonly CancellationTokenSource workerStop = new();
    private readonly object workerGate = new();
    private readonly HashSet<HixThread.Work> pendingWork = new();
    private bool disposed;
    // Loaded code and lookup maps are immutable; shared buffers and worker queues are synchronized.
    private static readonly ConditionalWeakTable<HixProgramImage, HixVM> loadedImages = new();
    private readonly HixProgramImage image;
    private readonly Dictionary<LanguageFunctionScope, LanguageFunctionScope> loadedScopes = new();
    private readonly Dictionary<(BytecodeFunction Function, LanguageFunctionScope Scope), BytecodeFunctionEntry> functionMap = new();
    private readonly Dictionary<(LanguageFunctionScope Scope, string Name), IReadOnlyList<LanguageFunctionCandidate>> candidates = new();

    public HixVM(IEnumerable<HixProgramImage> programs, int workerCount = 0) {
        if (workerCount < 0)
            throw new ArgumentOutOfRangeException(nameof(workerCount));
        using var profile = HixProfiler.Measure("vm.load");
        var distinct = programs.Distinct().ToArray();
        if (distinct.Length != 1)
            throw new ArgumentException("A VM executes exactly one prepared program image", nameof(programs));
        image = distinct[0];
        Backend = image.Backend;
        StringPool = image.StringPool;
        Patterns = image.Patterns.ToDictionary(item => item.Key, item => HixMetadataCompiler.Runtime(item.Value));
        Metadata = HixMetadataCompiler.Runtime(image.Metadata);
        MixinMetadata = HixMetadataCompiler.Runtime(image.MixinMetadata);
        MetadataDefinitions = image.MetadataDefinitions.Where(definition => definition.Retention == HixMetadataRetention.Runtime)
            .Select(definition => definition with {Arguments = definition.Arguments.Select(HixMetadataCompiler.Runtime).ToArray()}).ToArray();
        TypeMetadata = image.TypeMetadata.ToDictionary(item => item.Key, item => HixMetadataCompiler.Runtime(item.Value));
        Parameters = image.Parameters.Select(HixMetadataCompiler.Runtime).ToArray();
        var loadedFunctions = new Dictionary<BytecodeFunction, BytecodeFunction>();
        BytecodeFunction LoadFunction(BytecodeFunction function) {
            if (!loadedFunctions.TryGetValue(function, out var loaded))
                loadedFunctions.Add(function, loaded = HixMetadataCompiler.Runtime(function));
            return loaded;
        }
        LanguageFunctionScope LoadScope(LanguageFunctionScope scope) {
            if (scope == null) return null;
            if (!loadedScopes.TryGetValue(scope, out var loaded)) {
                loaded = new LanguageFunctionScope(scope.Functions(false).Select(entry => LoadFunction(entry.Function)), LoadScope(scope.Parent));
                loaded.Attach(image);
                loadedScopes.Add(scope, loaded);
            }
            return loaded;
        }
        LoadScope(image.Scope);
        Functions = Array.AsReadOnly(image.Functions.Select(entry => {
            var loaded = new BytecodeFunctionEntry(LoadFunction(entry.Function), LoadScope(entry.Scope));
            functionMap[(entry.Function, entry.Scope)] = loaded;
            functionMap[(loaded.Function, loaded.Scope)] = loaded;
            return loaded;
        }).ToArray());
        Derivations = image.Derivations.Select(derivation => derivation with {
            Scope = LoadScope(derivation.Scope), Metadata = HixMetadataCompiler.Runtime(derivation.Metadata),
            Expressions = derivation.Expressions.Select(expression => expression with {Function = LoadFunction(expression.Function)}).ToArray()
        }).ToArray();
        foreach (var pair in loadedScopes)
            foreach (var group in pair.Value.AllCandidates().GroupBy(candidate => candidate.Function.Name, StringComparer.Ordinal)) {
                var entries = Array.AsReadOnly(group.ToArray());
                candidates[(pair.Key, group.Key)] = entries;
                candidates[(pair.Value, group.Key)] = entries;
            }
        ConstantPool = LoadConstants(image);
        workers = new HixThread[workerCount];
        for (var index = 0; index < workers.Length; index++)
            workers[index] = new HixThread(submissions, workers, workerStop.Token);
        foreach (var worker in workers)
            worker.Start();
    }

    public HixThread CurrentThread => HixThread.Current;
    public HixStringPool StringPool { get; }
    public HixBackend Backend { get; }
    public IReadOnlyList<IHixValue> ConstantPool { get; }
    public IReadOnlyDictionary<string, HixPattern> Patterns { get; }
    public IReadOnlyList<BytecodeFunctionEntry> Functions { get; }
    public IReadOnlyList<BytecodeMetadata> Metadata { get; }
    public IReadOnlyList<BytecodeMetadata> MixinMetadata { get; }
    public IReadOnlyList<BytecodeMetadataDefinition> MetadataDefinitions { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<BytecodeMetadata>> TypeMetadata { get; }
    public IReadOnlyList<BytecodeField> Parameters { get; }
    public IReadOnlyList<BytecodeDerivation> Derivations { get; }
    internal LanguageFunctionScope GlobalScope => loadedScopes[image.Scope];

    public HixCoroutine CreateCoroutine(HixProgramImage program, HixContext context = null, string function = "main", params IHixValue[] arguments) {
        return CreateCoroutine(program, new NamedFunctionHixValue(HixString.Dynamic(function)), context, arguments);
    }

    public HixCoroutine CreateCoroutine(HixProgramImage program, IHixValue function, HixContext context = null, params IHixValue[] arguments) {
        Code(program);
        var coroutine = new HixCoroutine(context ?? Backend.CreateContext());
        coroutine.Prepare(this, program, function, arguments ?? Array.Empty<IHixValue>());
        return coroutine;
    }

    /// <summary>Submit to the current OS thread's event queue; VM-pool submission is opt-in.</summary>
    public HixTask<HixInvocationResult> InvokeAsync(HixProgramImage program, HixContext context = null, bool useThreadPool = false, string function = "main", params IHixValue[] arguments) {
        return InvokeAsync(program, new NamedFunctionHixValue(HixString.Dynamic(function)), context, useThreadPool, arguments);
    }

    public HixTask<HixInvocationResult> InvokeAsync(HixProgramImage program, IHixValue function, HixContext context = null, bool useThreadPool = false, params IHixValue[] arguments) {
        Code(program);
        if (useThreadPool) {
            if (workers.Length == 0)
                throw new InvalidOperationException("This VM has no background worker pool");
            context ??= Backend.CreateContext();
            if (!context.SupportsWorkerExecution)
                throw new InvalidOperationException("This context does not support VM thread-pool submission");
            var work = new HixThread.Work(() => CreateCoroutine(program, function, context, arguments), canSteal: true);
            HixThread[] pool;
            lock (workerGate) {
                if (disposed)
                    throw new ObjectDisposedException(nameof(HixVM));
                pool = workers;
                pendingWork.Add(work);
                submissions.Enqueue(work);
                work.Completion.OnCompleted(() => {
                    lock (workerGate)
                        pendingWork.Remove(work);
                });
            }

            var start = (uint)Interlocked.Increment(ref nextWorker) % (uint)pool.Length;
            var selected = pool[start];
            for (var offset = 0; offset < pool.Length; offset++) {
                var candidate = pool[(start + (uint)offset) % (uint)pool.Length];
                if (candidate.IsIdle) {
                    selected = candidate;
                    break;
                }
            }

            selected.Notify();
            return work.Completion;
        }

        return CurrentThread.Enqueue(() => CreateCoroutine(program, function, context, arguments));
    }

    public HixTask<HixInvocationResult> InvokeAsync(HixThread worker, HixProgramImage program, HixContext context = null, string function = "main", params IHixValue[] arguments) {
        if (worker == null)
            throw new ArgumentNullException(nameof(worker));
        Code(program);
        return worker.Enqueue(() => CreateCoroutine(program, context, function, arguments));
    }

    public void Dispose() {
        HixThread[] pool;
        lock (workerGate) {
            if (disposed)
                return;
            disposed = true;
            workerStop.Cancel();
            pool = workers;
        }

        foreach (var worker in pool)
            worker.Join();
        HixThread.Work[] abandoned;
        lock (workerGate) {
            abandoned = pendingWork.ToArray();
            pendingWork.Clear();
        }

        foreach (var work in abandoned) {
            try {
                work.Coroutine?.Dispose();
            } catch (Exception error) {
                work.Completion.TrySetException(error);
            } finally {
                work.Completion.TrySetCanceled();
            }
        }

        while (submissions.TryDequeue(out _)) {
        }
    }

    public string DisassemblePools() => HixProgramImage.DisassemblePools(StringPool, ConstantPool);

    public string Disassemble(HixProgramImage program) => program.Disassemble(Code(program), StringPool, ConstantPool);

    /// <summary>Invoke a global bytecode function using the normal Hix argument and signature rules.</summary>
    public HixInvocationResult Invoke(HixProgramImage program, HixCoroutine thread, string function = "main", params IHixValue[] arguments) {
        Code(program);
        thread.Prepare(this, program, function, arguments ?? Array.Empty<IHixValue>());
        while (!thread.Resume(int.MaxValue)) {
            if (thread.PendingOperation is { } pending) {
                try {
                    CurrentThread.RunUntil(pending);
                } catch {
                }
            }
        }

        return thread.Result;
    }

    public HixExecutionResult Run(HixProgramImage program, HixCoroutine thread, IDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null) {
        using var profile = HixProfiler.Measure("vm.run");
        if (program == null)
            throw new ArgumentNullException(nameof(program));
        Code(program);
        if (thread.Context is not HixExpressionContext context)
            throw new InvalidOperationException("Expression execution requires an expression context");
        thread.Start(this, program);
        try {
            var result = context.Execute(thread, variables, carries);
            if (variables != null)
                foreach (var item in result.Variables)
                    variables[item.Key.Resolve(thread.Strings)] = thread.UnlinkSnapshot(item.Value);
            return result;
        } finally {
            thread.Stop();
        }
    }

    public static HixExecutionResult Execute(HixProgramImage program, HixCoroutine thread, IDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null) {
        using var profile = HixProfiler.Measure("vm.execute.total");
        if (program == null)
            throw new ArgumentNullException(nameof(program));
        var machine = loadedImages.GetValue(program, image => new HixVM(new[] { image }));
        return machine.Run(program, thread, variables, carries);
    }

    internal BytecodeFunctionEntry Function(BytecodeFunction function, LanguageFunctionScope scope) => functionMap.TryGetValue((function, scope), out var entry) ? entry : throw new ArgumentException("Execution state contains a function from another program image");

    internal IReadOnlyList<LanguageFunctionCandidate> Candidates(LanguageFunctionScope scope, string name) => candidates.TryGetValue((scope, name), out var result) ? result : ReferenceEquals(scope.Program.Bytecode, image.Bytecode) ? Array.Empty<LanguageFunctionCandidate>() : scope.Candidates(name);

    internal bool Contains(LanguageFunctionScope scope, string name) => Candidates(scope, name).Count != 0;

    internal NamedFunctionHixValue Bind(LanguageFunctionScope scope, string name) => Contains(scope, name) ? new NamedFunctionHixValue(StringPool.Get(name)) {
        Scope = scope
    }

    : null;

    internal IReadOnlyList<byte> Code(HixProgramImage program) {
        if (Volatile.Read(ref disposed))
            throw new ObjectDisposedException(nameof(HixVM));
        return ReferenceEquals(image.Bytecode, program.Bytecode) ? program.Bytecode : throw new ArgumentException("Execution state contains a function from another program image", nameof(program));
    }

    private IReadOnlyList<IHixValue> LoadConstants(HixProgramImage program) {
        var constants = program.ConstantPool.ToArray();
        var scopes = new LanguageFunctionScope[constants.Length];
        var ranges = ScopeRanges(program);
        var rangeIndex = 0;
        foreach (var(pc, instruction)in HixInstruction.ReadAll(program.Bytecode)) {
            if (instruction.Opcode is not (HixOpcode.Call or HixOpcode.LoadConst) || constants[instruction.A] is not FunctionReferenceHixValue)
                continue;
            while (rangeIndex < ranges.Length && pc >= ranges[rangeIndex].End)
                rangeIndex++;
            var scope = rangeIndex < ranges.Length && pc >= ranges[rangeIndex].Start ? ranges[rangeIndex].Scope : program.Scope;
            if (scopes[instruction.A] is { } previous && !ReferenceEquals(previous, scope))
                throw new ArgumentException("A function-reference constant cannot be shared across lexical scopes");
            scopes[instruction.A] = scope;
        }

        var resolved = new Dictionary<(LanguageFunctionScope Scope, string Signature), ResolvedFunctionHixValue>();
        for (var index = 0; index < constants.Length; index++) {
            if (constants[index] is not FunctionReferenceHixValue reference)
                continue;
            var signature = reference.Signature;
            var owner = loadedScopes[scopes[index] ?? program.Scope];
            var key = (owner, signature.Display + (reference.DeclarationSignature == null ? "" : "|" + reference.DeclarationSignature));
            if (resolved.TryGetValue(key, out var cached)) {
                constants[index] = cached;
                continue;
            }

            var candidate = reference.DeclarationSignature == null ? owner.Resolve(signature) : owner.Candidates(signature.Name)
                .SingleOrDefault(item => item.Signature?.DeclarationSignature == reference.DeclarationSignature);
            if (candidate != null) {
                constants[index] = resolved[key] = new ResolvedFunctionHixValue(signature, candidate);
                continue;
            }

            var definitions = Backend.Functions.Resolve(signature.Name, signature.Parameters.Count).SelectMany(definition => definition.Signatures.Select(item => (definition, signature: item))).Where(item =>
                BuiltinSignature(item.definition.Name, item.signature).Display == signature.Display &&
                (reference.DeclarationSignature == null || HixGenericTypes.SignatureIdentity(new SignatureHixPattern(item.definition.Name,
                  item.signature.ArgumentPatterns.Select((pattern, position) => new HixPatternField(
                    item.signature.GetArgumentName(position) ?? position.ToString(), pattern)).ToArray(), item.signature.ResultPattern)) == reference.DeclarationSignature)).ToArray();
            if (definitions.Length != 1)
                throw new ArgumentException("cannot bind signature '" + signature.Display + "' while loading program");
            constants[index] = resolved[key] = new ResolvedFunctionHixValue(signature, definitions[0].definition, definitions[0].signature);
        }

        return Array.AsReadOnly(constants);
    }

    private static SignatureHixPattern BuiltinSignature(string name, FunctionSignature signature) => new(name, signature.ArgumentTypes.Select((kind, index) => new HixPatternField(signature.GetArgumentName(index) ?? index.ToString(), kind == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(kind), DefaultValue: signature.GetArgumentDefault(index), HasDefault: signature.GetArgumentDefault(index) != null)).ToArray(), signature.ResultType == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(signature.ResultType));

    private static ScopeRange[] ScopeRanges(HixProgramImage program) {
        var ranges = new Dictionary<int, ScopeRange>();
        void Add(int body, LanguageFunctionScope scope) {
            if (!ranges.ContainsKey(body))
                ranges.Add(body, new(body, body + HixInstruction.Decode(program.Bytecode, body).A, scope));
        }

        foreach (var entry in program.Functions)
            Add(entry.Function.Body, entry.Scope);
        return ranges.Values.OrderBy(range => range.Start).ToArray();
    }

    private readonly record struct ScopeRange(int Start, int End, LanguageFunctionScope Scope);
}
