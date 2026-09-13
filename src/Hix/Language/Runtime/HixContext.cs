using System;
using System.Threading;
using System.Collections.Generic;
using Hix.Compiler;

namespace Hix.Runtime;

/// <summary>Subclassable host services. Active frames belong to HixCoroutine.</summary>
public class HixContext {
    private static readonly HixStringPool EmptyStrings = new HixStringPoolBuilder().Freeze();
    private List<HixLog> logs;
    private int running;

    public HixContext(HixBackend backend = null, HixStringPool strings = null) {
        Backend = backend ?? HixCoreBackend.Instance;
        Strings = strings ?? EmptyStrings;
    }

    public HixBackend Backend { get; }
    public virtual bool SupportsWorkerExecution => true;
    public HixStringPool Strings { get; }
    public List<HixLog> Logs => logs ??= new();
    protected int LogCount => logs?.Count ?? 0;

    public virtual IHixValue ResolveHost(HixCoroutine thread, HixExpressionRoot root, HixString member) => thread.Backend.ResolveRoot(thread, root.ToString().ToLowerInvariant()).Select(thread, member);

    public virtual void BeforeExecution(HixCoroutine thread) {
    }

    public virtual void BeginExecution(HixCoroutine thread) => logs?.Clear();

    public virtual void FinishExecution(HixCoroutine thread) {
    }

    public virtual IHixValue InitialParameter(HixCoroutine thread) => NullHixValue.Instance;

    public virtual IHixValue ResolveRoot(HixCoroutine thread, string name, HixValueDictionary locals) => name == "local" ? new HixStorageValue(locals) : thread.Error("unknown root '" + name + "'");

    public virtual IHixValue Resolve(HixCoroutine thread, HixExpressionRoot root, HixString member) => ResolveHost(thread, root, member);

    public virtual bool IsReservedCall(string name) => false;

    public virtual IHixValue Call(HixCoroutine thread, string name, IHixValue[] arguments) => thread.Error("unknown host operation '" + name + "'");

    public virtual IHixValue ValidateLanguageCall(HixCoroutine thread, BytecodeFunction function) => null;

    public virtual IHixValue ValidateAvailability(HixCoroutine thread, string name, bool requiresPreparation, bool root = false) => requiresPreparation ? thread.Error("host operation '" + name + "' requires preparation") : null;

    public virtual IHixValue Store(HixCoroutine thread, HixOpcode opcode, HixString key, IHixValue value, HixValueDictionary locals) {
        if (opcode != HixOpcode.StoreLocal)
            return thread.Error("host does not support this storage operation");
        locals.StoreIsolated(key, HixStorageValue.Capture(value));
        return null;
    }

    public virtual void Emit(HixCoroutine thread, IHixValue value, HixString target = default) => throw new InvalidOperationException("Host does not support emissions");

    public virtual HixContextCheckpoint CaptureCheckpoint(HixCoroutine thread) => new(Diagnostics: LogCount);

    public virtual void RollbackCheckpoint(HixCoroutine thread, HixContextCheckpoint checkpoint, bool preserveLogs = false) {
        if (!preserveLogs)
            logs?.RemoveRange(checkpoint.Diagnostics, LogCount - checkpoint.Diagnostics);
    }

    public virtual HixExecutionResult Result(HixCoroutine thread, bool success, HixString error, int line, double milliseconds) => new(success, error, line, Array.Empty<HixOutput>(), SnapshotLogs(), executedOperations: thread.ExecutedOperations, executionMilliseconds: milliseconds, strings: thread.Strings);

    internal void Acquire() {
        if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
            throw new InvalidOperationException("A Hix context is already in use by another execution");
    }

    internal void Release() => Volatile.Write(ref running, 0);

    protected HixLog[] SnapshotLogs() => logs?.ToArray() ?? Array.Empty<HixLog>();
}

/// <summary>Opaque, allocation-free host transaction token. The interpreter never inspects its payload.</summary>
public readonly record struct HixContextCheckpoint(object State1 = null, object State2 = null, object State3 = null, int Position = 0, int Diagnostics = 0, int Effects = 0);
