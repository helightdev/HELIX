using Hix.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hix.Runtime;

internal sealed partial class LanguageExecution {
  internal IHixValue Derive(IHixValue value, int line) => value is TupleHixValue tuple
    ? new DerivationFrame(this, tuple, line).Run()
    : context.Error("derive requires a tuple of records");

  /// <summary>Owns one derivation invocation's rollback snapshots, caller state, and current record/provider.</summary>
  private struct DerivationFrame {
    private readonly LanguageExecution execution;
    private readonly TupleHixValue tuple;
    private readonly int line;
    private readonly PersistentMap<HixString, IHixValue> savedVariables;
    private readonly PersistentMap<HixString, IHixValue> savedTargets;
    private readonly PersistentMap<HixString, IHixValue> savedCarries;
    private readonly IHixValue previousParameter;
    private readonly HixValueDictionary previousLocals;
    private readonly LanguageFunctionScope previousScope;
    private readonly int outputCount;
    private readonly List<IHixValue> result;
    private int index;
    private BytecodeDerivation provider;

    internal DerivationFrame(LanguageExecution execution, TupleHixValue tuple, int line) {
      this.execution = execution;
      this.tuple = tuple;
      this.line = line;
      savedVariables = execution.variables.Snapshot();
      savedTargets = execution.targetVariables.Snapshot();
      savedCarries = execution.carries.Snapshot();
      previousParameter = execution.parameter;
      previousLocals = execution.locals;
      previousScope = execution.scope;
      outputCount = execution.outputs.Count;
      result = new List<IHixValue>(tuple.Values.Count);
      index = 0;
      provider = null;
    }

    private bool HasValue(HixTableValue record) =>
      record.TryGetValue(execution.context, execution.context.ResolveString("value"), out _);

    internal IHixValue Run() {
      try {
        for (; index < tuple.Values.Count; index++) {
          if (tuple.Values[index] is not HixTableValue record)
            return Fail(execution.context.Error("record must be a table"), line);
          var symbol = record.Select(execution.context, execution.context.ResolveString("symbol"));
          if (symbol.Kind != HixValueKind.Symbol)
            return Fail(execution.context.Error("record must contain a semantic symbol"), line);
          if (!HasValue(record))
            return Fail(execution.context.Error("record must contain value"), line);
          var current = record.Select(execution.context, execution.context.ResolveString("value"));
          foreach (var declaration in execution.derivations) {
            provider = declaration;
            if (!execution.activeDerivations.Add(provider))
              return Fail(execution.context.Error("recursive derivation"), provider.Line);
            execution.locals = execution.machine.RentLocals();
            execution.scope = provider.Scope;
            try {
              foreach (var expression in provider.Expressions) {
                execution.parameter = Functions.CollectionFunctions.Put(execution.context, record, "value", current);
                var completion = execution.Run(expression.Body);
                if (completion.Kind == BytecodeFlow.Error) return Fail(completion.Value, completion.Line);
                if (completion.Kind == BytecodeFlow.Normal)
                  return Fail(execution.context.Error("derivation must explicitly return a value"), expression.Line);
                if (completion.Kind != BytecodeFlow.Return)
                  return Fail(execution.context.Error("control flow cannot cross derivation boundaries: " + completion.Kind), completion.Line, true);
                current = completion.Value;
                if (current is ErrorHixValue) return Fail(current, expression.Line);
              }
            } finally {
              execution.machine.ReturnLocals(execution.locals);
              execution.locals = previousLocals;
              execution.activeDerivations.Remove(provider);
            }
          }
          result.Add(Functions.CollectionFunctions.Put(execution.context, record, "value", current));
          provider = null;
        }
        return new TupleHixValue(result.ToArray());
      } finally {
        execution.parameter = previousParameter;
        execution.locals = previousLocals;
        execution.scope = previousScope;
      }
    }

    private IHixValue Fail(IHixValue error, int errorLine, bool control = false) {
      Restore(execution.variables, savedVariables);
      Restore(execution.targetVariables, savedTargets);
      Restore(execution.carries, savedCarries);
      execution.outputs.RemoveRange(outputCount, execution.outputs.Count - outputCount);
      return control ? error : execution.context.Error("derive entry " + index + (provider == null ? "" : " in '" + provider.Name + "'") +
        " at line " + errorLine + ": " + execution.Text(error));
    }
  }
}
