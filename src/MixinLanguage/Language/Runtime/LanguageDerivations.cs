using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

internal sealed partial class LanguageExecution {
  internal IMixinValue Derive(IMixinValue value, int line) => value is TupleMixinValue tuple
    ? new DerivationFrame(this, tuple, line).Run()
    : context.Error("derive requires a tuple of records");

  /// <summary>Owns one derivation invocation's rollback snapshots, caller state, and current record/provider.</summary>
  private struct DerivationFrame {
    private readonly LanguageExecution execution;
    private readonly TupleMixinValue tuple;
    private readonly int line;
    private readonly KeyValuePair<MixinString, IMixinValue>[] savedVariables;
    private readonly KeyValuePair<MixinString, IMixinValue>[] savedTargets;
    private readonly KeyValuePair<MixinString, IMixinValue>[] savedCarries;
    private readonly IMixinValue previousParameter;
    private readonly MixinValueDictionary previousLocals;
    private readonly LanguageFunctionScope previousScope;
    private readonly int outputCount;
    private readonly List<IMixinValue> result;
    private int index;
    private BytecodeDerivation provider;

    internal DerivationFrame(LanguageExecution execution, TupleMixinValue tuple, int line) {
      this.execution = execution;
      this.tuple = tuple;
      this.line = line;
      savedVariables = execution.variables.ToArray();
      savedTargets = execution.targetVariables.ToArray();
      savedCarries = execution.carries.ToArray();
      previousParameter = execution.parameter;
      previousLocals = execution.locals;
      previousScope = execution.scope;
      outputCount = execution.outputs.Count;
      result = new List<IMixinValue>(tuple.Values.Count);
      index = 0;
      provider = null;
    }

    private bool HasValue(MixinTableValue record) =>
      record.TryGetValue(execution.context, execution.context.ResolveString("value"), out _);

    internal IMixinValue Run() {
      try {
        for (; index < tuple.Values.Count; index++) {
          if (tuple.Values[index] is not MixinTableValue record)
            return Fail(execution.context.Error("record must be a table"), line);
          var symbol = record.Select(execution.context, execution.context.ResolveString("symbol"));
          if (symbol.Kind != MixinValueKind.Symbol)
            return Fail(execution.context.Error("record must contain a semantic symbol"), line);
          if (!HasValue(record))
            return Fail(execution.context.Error("record must contain value"), line);
          var current = record.Select(execution.context, execution.context.ResolveString("value"));
          foreach (var declaration in execution.derivations) {
            provider = declaration;
            if (!execution.activeDerivations.Add(provider))
              return Fail(execution.context.Error("recursive derivation"), provider.Line);
            execution.locals = new MixinValueDictionary();
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
                if (current is ErrorMixinValue) return Fail(current, expression.Line);
              }
            } finally { execution.activeDerivations.Remove(provider); }
          }
          result.Add(Functions.CollectionFunctions.Put(execution.context, record, "value", current));
          provider = null;
        }
        return new TupleMixinValue(result.ToArray());
      } finally {
        execution.parameter = previousParameter;
        execution.locals = previousLocals;
        execution.scope = previousScope;
      }
    }

    private IMixinValue Fail(IMixinValue error, int errorLine, bool control = false) {
      Restore(execution.variables, savedVariables);
      Restore(execution.targetVariables, savedTargets);
      Restore(execution.carries, savedCarries);
      execution.outputs.RemoveRange(outputCount, execution.outputs.Count - outputCount);
      return control ? error : execution.context.Error("derive entry " + index + (provider == null ? "" : " in '" + provider.Name + "'") +
        " at line " + errorLine + ": " + execution.Text(error));
    }
  }
}
