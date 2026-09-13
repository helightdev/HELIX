using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Hix.Collections;
using Hix.Compiler;
using Hix.Env;
using static Hix.Runtime.HixCoroutine;

namespace Hix.Runtime;

public sealed class HixExpressionState {
    private HixValueDictionary variables, carries, targetVariables;
    private List<HixOutput> outputs;

    public HixValueDictionary Variables => variables ??= new();
    public HixValueDictionary Carries => carries ??= new();
    public HixValueDictionary TargetVariables => targetVariables ??= new();
    internal HixValueDictionary ExistingVariables => variables;
    internal HixValueDictionary ExistingCarries => carries;
    internal HixValueDictionary ExistingTargetVariables => targetVariables;
    public IReadOnlyList<HixOutput> Outputs => outputs ?? (IReadOnlyList<HixOutput>)Array.Empty<HixOutput>();

    internal void Emit(HixOutput output) => (outputs ??= new()).Add(output);

    internal void ClearOutputs() => outputs?.Clear();

    internal void RollbackOutputs(int count) => outputs?.RemoveRange(count, outputs.Count - count);
}

/// <summary>Opt-in expression preparation, storage, emissions and transactions.</summary>
public class HixExpressionContext(HixBackend backend = null, HixStringPool strings = null) : HixContext(backend, strings) {
    private HixExpressionState state;
    private bool expressionExecution, prelude;
    private static readonly IReadOnlyDictionary<string, object> EmptyHostValues = new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

    public HixExpressionState ExpressionState => State;
    public HixValueDictionary Variables => State.Variables;
    public HixValueDictionary Carries => State.Carries;
    public HixValueDictionary TargetVariables => State.TargetVariables;
    public IReadOnlyList<HixOutput> Outputs => state?.Outputs ?? Array.Empty<HixOutput>();
    private HixExpressionState State => state ??= new();

    public virtual void BeginExecution() {
    }

    public virtual int CaptureEffects() => 0;

    public virtual void RollbackEffects(int checkpoint) {
    }

    public virtual IHixValue ResolveMixinParameter(HixCoroutine thread, BytecodeField parameter, int index) => parameter.DefaultValue ?? (parameter.Optional ? MissingHixValue.Instance : thread.Error("mixin parameter '" + parameter.Name + "' is required"));

    public static bool IsPrelude(HixCoroutine thread) => thread.Context is HixExpressionContext { prelude: true };

    public override void BeforeExecution(HixCoroutine thread) => Commit(thread);

    public override void BeginExecution(HixCoroutine thread) {
        base.BeginExecution(thread);
        prelude = expressionExecution = false;
        state?.ClearOutputs();
        BeginExecution();
    }

    public override void FinishExecution(HixCoroutine thread) {
        try {
            Commit(thread);
        } finally {
            prelude = expressionExecution = false;
        }
    }

    public override IHixValue InitialParameter(HixCoroutine thread) => thread.Machine.Parameters.Count == 0 ? NullHixValue.Instance : new LazyMixinParametersHixValue(thread.Machine.Parameters);

    public override IHixValue ResolveRoot(HixCoroutine thread, string name, HixValueDictionary locals) {
        if (name is not ("local" or "var" or "tar"))
            return base.ResolveRoot(thread, name, locals);
        if (thread.IsPure && name != "local")
            return thread.Error("pure functions cannot read shared storage");
        var storage = name switch {
            "local" => locals,
            "var" => State.Variables,
            _ => State.TargetVariables
        };
        return new HixStorageValue(storage, name == "local" && thread.CallDepth == 0 ? state?.ExistingCarries : null);
    }

    public override IHixValue Resolve(HixCoroutine thread, HixExpressionRoot root, HixString member) {
        if (root == HixExpressionRoot.Variable)
            return state?.ExistingVariables is { } variables && variables.TryGetValue(member, out var value) ? value : MissingHixValue.Instance;
        if (root == HixExpressionRoot.TargetVariable) {
            if (state?.ExistingTargetVariables is not { } targets)
                return MissingHixValue.Instance;
            var name = member.Resolve(thread.Strings);
            foreach (var item in targets)
                if (string.Equals(item.Key.Resolve(thread.Strings), name, StringComparison.Ordinal))
                    return item.Value;
            return MissingHixValue.Instance;
        }

        return base.Resolve(thread, root, member);
    }

    public override bool IsReservedCall(string name) => name is "var" or "tar" or "local";

    public override IHixValue Call(HixCoroutine thread, string name, IHixValue[] arguments) {
        if (name == "local")
            return thread.Error("locals cannot be modified through the storage function");
        if (arguments.Length != 2)
            return thread.Error("storage function '" + name + "' expects a key and value");
        if (thread.IsPure)
            return thread.Error("pure functions cannot mutate shared storage");
        var value = HixStorageValue.Capture(arguments[1]);
        (name == "var" ? State.Variables : State.TargetVariables).StoreIsolated(HixString.Dynamic(thread.ResolveText(arguments[0])), value);
        return value;
    }

    public override IHixValue ValidateLanguageCall(HixCoroutine thread, BytecodeFunction function) => expressionExecution && !prelude && !function.IsPure ? thread.Error("impure function '" + function.Name + "' requires prelude preparation") : null;

    public override IHixValue ValidateAvailability(HixCoroutine thread, string name, bool requiresPreparation, bool root = false) => !prelude && requiresPreparation ? thread.Error((root ? "host root '" : "function '") + name + "' requires the prelude pass") : null;

    public override IHixValue Store(HixCoroutine thread, HixOpcode opcode, HixString key, IHixValue value, HixValueDictionary locals) {
        var carry = opcode != HixOpcode.StoreLocal;
        if (carry && thread.IsPure)
            return thread.Error("pure functions cannot mutate shared storage");
        if (carry && (!prelude || thread.CallDepth != 0))
            return thread.Error("carry local assignments require a top-level prelude expression");
        if (opcode == HixOpcode.CheckStoreCarry)
            return null;
        var destination = carry || thread.CallDepth == 0 && state?.ExistingCarries?.ContainsKey(key) == true ? State.Carries : locals;
        destination.StoreIsolated(key, HixStorageValue.Capture(value));
        return null;
    }

    public override void Emit(HixCoroutine thread, IHixValue value, HixString target = default) => State.Emit(new HixOutput(thread.DetachValue(value), target.IsNull ? HixString.Empty : HixString.Dynamic(target.Resolve(thread.Strings))));

    public override HixContextCheckpoint CaptureCheckpoint(HixCoroutine thread) => new(state?.ExistingVariables?.Snapshot(), state?.ExistingTargetVariables?.Snapshot(), state?.ExistingCarries?.Snapshot(), state?.Outputs.Count ?? 0, LogCount, CaptureEffects());

    public override void RollbackCheckpoint(HixCoroutine thread, HixContextCheckpoint checkpoint, bool preserveLogs = false) {
        Restore(state?.ExistingVariables, checkpoint.State1);
        Restore(state?.ExistingTargetVariables, checkpoint.State2);
        Restore(state?.ExistingCarries, checkpoint.State3);
        state?.RollbackOutputs(checkpoint.Position);
        RollbackEffects(checkpoint.Effects);
        base.RollbackCheckpoint(thread, checkpoint, preserveLogs);
    }

    public override HixExecutionResult Result(HixCoroutine thread, bool success, HixString error, int line, double milliseconds) => new(success, error, line, state?.Outputs.ToArray() ?? Array.Empty<HixOutput>(), SnapshotLogs(), Snapshot(state?.ExistingVariables), Snapshot(state?.ExistingCarries), thread.ExecutedOperations, milliseconds, thread.Strings);

    public HixExecutionResult Execute(HixCoroutine thread, IDictionary<string, object> imported, IReadOnlyDictionary<string, object> importedCarries) {
        using var profile = HixProfiler.Measure("vm.execution");
        var started = Stopwatch.GetTimestamp();
        expressionExecution = true;
        if (imported != null)
            foreach (var item in imported)
                State.Variables.StoreIsolated(HixString.Dynamic(item.Key), thread.Import(item.Value));
        if (importedCarries != null)
            foreach (var item in importedCarries)
                State.Carries.StoreIsolated(HixString.Dynamic(item.Key), thread.Import(item.Value));
        foreach (var expression in thread.Program.Expressions.OrderBy(entry => entry.IsPrelude ? 0 : 1)) {
            if (prelude && !expression.IsPrelude)
                ClosePrelude(thread);
            prelude = expression.IsPrelude;
            var checkpoint = CaptureCheckpoint(thread);
            var completion = thread.ExecuteFunction(expression.Function, thread.Program.Scope, thread.Parameter);
            if (completion.Kind is not (BytecodeFlow.Normal or BytecodeFlow.Return)) {
                RollbackCheckpoint(thread, checkpoint, preserveLogs: true);
                var error = completion.Kind == BytecodeFlow.Error ? thread.Text(completion.Value) : HixString.Dynamic("invalid " + completion.Kind + " outside its scope");
                return Result(thread, false, error, completion.Line, Elapsed(started));
            }
        }

        ClosePrelude(thread);
        return Result(thread, true, default, 0, Elapsed(started));
    }

    public IReadOnlyDictionary<string, object> ExportVariables(HixCoroutine thread) {
        using var profile = HixProfiler.Measure("vm.commit");
        return Export(thread, state?.ExistingVariables);
    }

    public IReadOnlyDictionary<string, object> ExportCarries(HixCoroutine thread) => Export(thread, state?.ExistingCarries);

    private void Commit(HixCoroutine thread) {
        DetachStorage(thread, state?.ExistingVariables);
        DetachStorage(thread, state?.ExistingCarries);
        DetachStorage(thread, state?.ExistingTargetVariables);
    }

    private static void DetachStorage(HixCoroutine thread, HixValueDictionary storage) {
        if (storage == null || storage.Count == 0)
            return;
        storage.ReplaceWith(storage.Select(item => new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(item.Key.Resolve(thread.Strings)), thread.DetachValue(item.Value))).ToArray());
    }

    private static void Restore(HixValueDictionary storage, object saved) {
        if (storage == null)
            return;
        if (saved == null)
            storage.Clear();
        else
            storage.Restore((PersistentMap<HixString, IHixValue>)saved);
    }

    private static IReadOnlyDictionary<HixString, IHixValue> Snapshot(HixValueDictionary storage) => storage == null || storage.Count == 0 ? HixExecutionResult.EmptyValues : storage.Snapshot();

    private void ClosePrelude(HixCoroutine thread) {
        using var profile = HixProfiler.Measure("vm.detach");
        DetachPreludeStorage(thread, state?.ExistingCarries);
        DetachPreludeStorage(thread, state?.ExistingVariables);
        DetachPreludeStorage(thread, state?.ExistingTargetVariables);
    }

    private static void DetachPreludeStorage(HixCoroutine thread, HixValueDictionary storage) {
        if (storage == null)
            return;
        foreach (var item in storage)
            storage.Store(thread, item.Key, item.Value);
    }

    private static double Elapsed(long started) => (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency;

    private static IReadOnlyDictionary<string, object> Export(HixCoroutine thread, HixValueDictionary storage) {
        if (storage == null || storage.Count == 0)
            return EmptyHostValues;
        var exported = new Dictionary<string, object>(storage.Count, StringComparer.Ordinal);
        foreach (var item in storage)
            exported.Add(item.Key.Resolve(thread.Strings), thread.UnlinkSnapshot(item.Value));
        return exported;
    }
}

internal sealed class LazyMixinParametersHixValue(IReadOnlyList<BytecodeField> fields) : IHixValue {
    private readonly IHixValue[] values = new IHixValue[fields.Count];

    public HixValueKind Kind => HixValueKind.Table;

    public IHixValue Select(HixCoroutine context, HixString member) {
        var name = member.Resolve(context.Strings);
        var index = int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var position) ? position : FindIndex(name);
        return index >= 0 && index < fields.Count ? Get(context, index) : MissingHixValue.Instance;
    }

    public bool IsTruthy(HixCoroutine context) => fields.Count != 0;

    public HixString Render(HixCoroutine context) => Materialize(context).Render(context);

    public void Fingerprint(HixFingerprintBuilder builder, HixCoroutine context) => Materialize(context).Fingerprint(builder, context);

    public object Unlink(HixCoroutine context) => Materialize(context).Unlink(context);

    public bool Equals(IHixValue other) => ReferenceEquals(this, other);

    private IHixValue Get(HixCoroutine thread, int index) {
        if (values[index] != null)
            return values[index];
        var field = fields[index];
        var value = thread.Context is HixExpressionContext context ? context.ResolveMixinParameter(thread, field, index) : field.DefaultValue ?? (field.Optional ? MissingHixValue.Instance : thread.Error("mixin parameter '" + field.Name + "' is required"));
        if (value is MissingHixValue)
            return values[index] = value;
        return values[index] = thread.Coerce(value, fields[index].Pattern);
    }

    private int FindIndex(string name) {
        for (var index = 0; index < fields.Count; index++)
            if (fields[index].Name == name)
                return index;
        return -1;
    }

    private HixTableValue Materialize(HixCoroutine context) => new(fields.Select((field, index) => new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(field.Name), Get(context, index))));
}
