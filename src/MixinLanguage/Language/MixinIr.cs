using System.Collections.Generic;
using System.Linq;
using Mixins.Compiler;
using Mixins.Runtime;

namespace Mixins;

internal enum MixinOpcode {
  Empty,
  Scope,
  Function,
  End,
  Match,
  Assert,
  Emit,
  Mixin,
  Using,
  Log,
  StoreLocal,
  StoreVariable,
  StoreTargetVariable,
  Carry,
  Return,
  Call,
  Goto,
  Skip,
  Fail,
  Directive
}

internal sealed record MixinInstruction(
  MixinOpcode Opcode, MixinSourceLocation Location, IMixinValue Operand = null,
  IReadOnlyList<IMixinValue> Arguments = null, MixinString Name = default,
  MixinExpressionOutputTarget OutputTarget = default, int Destination = -1,
  int SecondaryDestination = -1, DirectiveDefinition Directive = null,
  MixinString Message = default
);

internal sealed record LiteralMixinValue(MixinString Value) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return !string.IsNullOrEmpty(Value.Resolve(context.Strings));
  }

  public MixinString Render(ExecutionContext context) {
    return Value;
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(LiteralMixinValue));
    builder.Append(Value.Resolve(context.Strings));
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Value.Resolve(context.Strings);
  }

  public bool Equals(IMixinValue other) {
    return other is LiteralMixinValue value && Equals(value);
  }
}

internal sealed record RootMixinValue(MixinExpressionRoot Root, MixinString Member) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return Resolve(context).IsTruthy(context);
  }

  public MixinString Render(ExecutionContext context) {
    return Resolve(context).Render(context);
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    Resolve(context).Fingerprint(builder, context);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return Resolve(context).Select(context, member);
  }

  public object Unlink(ExecutionContext context) {
    return Resolve(context).Unlink(context);
  }

  public bool Equals(IMixinValue other) {
    return other is RootMixinValue value && Equals(value);
  }

  public IMixinValue Resolve(ExecutionContext context) {
    return context.Resolve(Root, Member);
  }
}

internal sealed record InvokeMixinValue(FunctionDefinition Function, IMixinValue Instance,
  IReadOnlyList<IMixinValue> Arguments, bool Negated
) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return Resolve(context).IsTruthy(context);
  }

  public MixinString Render(ExecutionContext context) {
    return Resolve(context).Render(context);
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    Resolve(context).Fingerprint(builder, context);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return Resolve(context).Select(context, member);
  }

  public object Unlink(ExecutionContext context) {
    return Resolve(context).Unlink(context);
  }

  public bool Equals(IMixinValue other) {
    return other is InvokeMixinValue value &&
      ReferenceEquals(Function, value.Function) && Equals(Instance, value.Instance) && Negated == value.Negated &&
      Arguments.SequenceEqual(value.Arguments);
  }

  internal IMixinValue Resolve(ExecutionContext context) {
    return context.Invoke(Function, Instance, Arguments, Negated);
  }
}

internal sealed record InterpolationMixinValue(IReadOnlyList<IMixinValue> Parts) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return Parts.Count != 0;
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic(
      string.Concat(
        Parts.Select(item => item.Render(context).Resolve(context.Strings))
      )
    );
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(InterpolationMixinValue));
    foreach (var part in Parts) part.Fingerprint(builder, context);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return Render(context).Resolve(context.Strings);
  }

  public bool Equals(IMixinValue other) {
    return other is InterpolationMixinValue value && Parts.SequenceEqual(value.Parts);
  }
}

internal sealed record AllMixinValue(IReadOnlyList<IMixinValue> Values) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    foreach (var value in Values) {
      if (!value.IsTruthy(context))
        return false;
    }
    return Values.Count != 0;
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic(IsTruthy(context) ? "true" : "false");
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(AllMixinValue));
    foreach (var value in Values) value.Fingerprint(builder, context);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return IsTruthy(context);
  }

  public bool Equals(IMixinValue other) {
    return other is AllMixinValue value && Values.SequenceEqual(value.Values);
  }
}

internal sealed record ProgramFunctionMixinValue(int Entry) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return Entry >= 0;
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic("<function>");
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(ProgramFunctionMixinValue));
    builder.Append(Entry);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return this;
  }

  public bool Equals(IMixinValue other) {
    return other is ProgramFunctionMixinValue function && function.Entry == Entry;
  }
}

internal readonly record struct ProgramFunctionResult(bool HasValue, IMixinValue Value);

internal sealed record DirectiveEffectMixinValue(IMixinValue Value, MixinString ClassCode) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return Value.IsTruthy(context);
  }

  public MixinString Render(ExecutionContext context) {
    return Value.Render(context);
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    Value.Fingerprint(builder, context);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return Value.Select(context, member);
  }

  public object Unlink(ExecutionContext context) {
    return Value.Unlink(context);
  }

  public bool Equals(IMixinValue other) {
    return other is DirectiveEffectMixinValue effect && Equals(effect);
  }
}

internal enum TableTransformKind { MapValues, Map, Filter }

internal sealed record TableTransformMixinValue(MixinTableValue Table, TableTransformKind Kind,
  ProgramFunctionMixinValue Function
) : IMixinValue {
  public bool IsTruthy(ExecutionContext context) {
    return Table.IsTruthy(context);
  }

  public MixinString Render(ExecutionContext context) {
    return ExecutionContext.Dynamic("<table transform>");
  }

  public void Fingerprint(MixinFingerprintBuilder builder, ExecutionContext context) {
    builder.Append(nameof(TableTransformMixinValue));
    Table.Fingerprint(builder, context);
    builder.Append((int)Kind);
    Function.Fingerprint(builder, context);
  }

  public IMixinValue Select(ExecutionContext context, MixinString member) {
    return NullMixinValue.Instance;
  }

  public object Unlink(ExecutionContext context) {
    return this;
  }

  public bool Equals(IMixinValue other) {
    return other is TableTransformMixinValue transform && Equals(transform);
  }
}

public sealed class MixinExpressionPreparedState {
  internal MixinExpressionPreparedState(
    MixinStringPool stringPool,
    IReadOnlyList<MixinInstruction> instructions, MixinValueDictionary variables,
    IReadOnlyDictionary<MixinString, int> functionEntries,
    IReadOnlyList<MixinExpressionPreparedLog> logs, int executedOperations
  ) {
    StringPool = stringPool;
    Instructions = instructions;
    Variables = variables;
    Logs = logs;
    FunctionEntries = functionEntries;
    ExecutedOperations = executedOperations;
  }

  public MixinStringPool StringPool { get; }
  internal IReadOnlyList<MixinInstruction> Instructions { get; }
  internal MixinValueDictionary Variables { get; }
  internal IReadOnlyDictionary<MixinString, int> FunctionEntries { get; }
  public IReadOnlyList<MixinExpressionPreparedLog> Logs { get; }
  public int ExecutedOperations { get; }
}

internal sealed record MixinExpressionExecutionProgram(
  MixinStringPool StringPool, IReadOnlyList<MixinInstruction> Instructions,
  MixinValueDictionary Variables
);