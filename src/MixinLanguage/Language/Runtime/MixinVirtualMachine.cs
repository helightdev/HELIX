using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mixins.Runtime;

/// <summary>A loaded bytecode VM. All loaded programs share immutable VM-wide string and constant pools.</summary>
public sealed class MixinVirtualMachine {
  private readonly Dictionary<IReadOnlyList<byte>, IReadOnlyList<byte>> images = new();
  public MixinStringPool StringPool { get; }
  public IReadOnlyList<IMixinValue> ConstantPool { get; }

  public MixinVirtualMachine(IEnumerable<MixinExpressionExecutionProgram> programs, MixinStringPool strings = null) {
    if (strings != null && strings.Count > ushort.MaxValue + 1)
      throw new ArgumentException("VM string pool cannot exceed 65536 entries (u16 indices)");
    var pool = new MixinStringPoolBuilder(strings);
    var constants = new List<IMixinValue>();
    foreach (var program in programs.GroupBy(program => program.Bytecode).Select(group => group.First())) {
      // Loading deduplicates constants and relocates operands into the VM-wide pools. Execution never interns strings or changes pools.
      var indices = new int[program.StringPool.Count];
      for (var i = 0; i < indices.Length; i++) {
        indices[i] = pool.Intern(program.StringPool[i]).Id;
        if (indices[i] > ushort.MaxValue) throw new ArgumentException("VM string pool cannot exceed 65536 entries (u16 indices)");
      }
      var constantIndices = new int[program.ConstantPool.Count];
      for (var i = 0; i < constantIndices.Length; i++) {
        var value = program.ConstantPool[i];
        var index = constants.FindIndex(existing => existing.Equals(value));
        if (index < 0) { index = constants.Count; constants.Add(value); }
        if (index > ushort.MaxValue) throw new ArgumentException("VM constant pool cannot exceed 65536 entries (u16 indices)");
        constantIndices[i] = index;
      }
      var bytes = program.Bytecode.ToArray();
      foreach (var (pc, instruction) in HixInstruction.ReadAll(bytes)) {
        if (instruction.UsesStringPool)
          (instruction with {A = indices[instruction.A]}).Encode(bytes, pc);
        else if (instruction.Opcode == HixOpcode.LoadConst)
          (instruction with {A = constantIndices[instruction.A]}).Encode(bytes, pc);
      }
      images.Add(program.Bytecode, Array.AsReadOnly(bytes));
    }
    StringPool = pool.Freeze();
    ConstantPool = constants.AsReadOnly();
  }

  internal IReadOnlyList<byte> Code(MixinExpressionExecutionProgram program) => images[program.Bytecode];

  public string DisassemblePools() => MixinExpressionExecutionProgram.DisassemblePools(StringPool, ConstantPool);
  public string Disassemble(MixinExpressionExecutionProgram program) => program.Disassemble(Code(program), StringPool, ConstantPool);

  public MixinExpressionResult Run(MixinExpressionExecutionProgram program, ExecutionContext context,
    IDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null) {
    if (program == null) throw new ArgumentNullException(nameof(program));
    Code(program);
    // Host keys may predate this VM. Resolve them before attaching the execution context to the fixed VM pool.
    var targets = context.TargetVariables.Select(entry => new KeyValuePair<MixinString, IMixinValue>(
      ExecutionContext.Dynamic(entry.Key.Resolve(context.Strings)), entry.Value)).ToArray();
    context.TargetVariables.ReplaceWith(targets);
    context.Attach(this);
    var imported = variables == null ? null : new Dictionary<string, object>(variables, StringComparer.Ordinal);
    var result = new LanguageExecution(this, context, program).Execute(program.Expressions, imported, carries);
    if (variables != null)
      foreach (var item in result.Variables) variables[item.Key] = item.Value;
    return result;
  }

  public static MixinExpressionResult Execute(MixinExpressionExecutionProgram program, ExecutionContext context,
    IDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null) {
    if (program == null) throw new ArgumentNullException(nameof(program));
    var programs = new HashSet<MixinExpressionExecutionProgram> {program};
    var seen = new HashSet<object>();
    if (variables != null) foreach (var value in variables.Values) IncludeProgram(value, programs, seen);
    if (carries != null) foreach (var value in carries.Values) IncludeProgram(value, programs, seen);
    foreach (var entry in context.TargetVariables) IncludeProgram(entry.Value, programs, seen);
    return new MixinVirtualMachine(programs, context.Strings).Run(program, context, variables, carries);
  }

  private static void IncludeProgram(object value, HashSet<MixinExpressionExecutionProgram> programs, HashSet<object> seen) {
    if (value == null || value is string || !seen.Add(value)) return;
    if (value is NamedFunctionMixinValue {Scope: { } scope}) programs.Add(scope.Program);
    else if (value is TupleMixinValue tuple) foreach (var item in tuple.Values) IncludeProgram(item, programs, seen);
    else if (value is MixinTableValue table) foreach (var entry in table.Entries) IncludeProgram(entry.Value, programs, seen);
    else if (value is IReadOnlyDictionary<string, object> dictionary) foreach (var entry in dictionary) IncludeProgram(entry.Value, programs, seen);
    else if (value is IEnumerable sequence) foreach (var item in sequence) IncludeProgram(item, programs, seen);
  }
}
