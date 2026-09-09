using Hix.Env;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Hix.Runtime;

/// <summary>A loaded bytecode VM. All loaded programs share immutable VM-wide string and constant pools.</summary>
public sealed class HixVM {
  private readonly Stack<HixValueDictionary> localPool = new();

  internal HixValueDictionary RentLocals() {
    lock (localPool) return localPool.Count == 0 ? new HixValueDictionary(mutable: true) : localPool.Pop();
  }

  internal void ReturnLocals(HixValueDictionary locals) {
    locals.Clear();
    lock (localPool) {
      // Bound retained dictionaries even when a shared VM serves concurrent callers.
      if (localPool.Count < 128) localPool.Push(locals);
    }
  }

  private static readonly ConditionalWeakTable<IReadOnlyList<byte>, LoadedImages> loadedImages = new();

  // Weak image/seed keys bound cache lifetime to the compilation. Cached VMs contain
  // immutable code/pools and a synchronized pool of cleared local dictionaries;
  // active execution state remains invocation-local.
  private sealed class LoadedImages {
    private readonly ConditionalWeakTable<HixStringPool, HixVM> bySeed = new();
    internal HixVM Get(HixExpressionExecutionProgram program, HixStringPool seed) {
      lock (bySeed) {
        if (bySeed.TryGetValue(seed, out var machine)) {
          HixProfiler.Increment("vm.load.cache_hit");
          return machine;
        }
        var loaded = new HixVM(new[] {program}, seed);
        bySeed.Add(seed, loaded);
        if (!ReferenceEquals(seed, loaded.StringPool)) bySeed.Add(loaded.StringPool, loaded);
        return loaded;
      }
    }
  }

  private readonly Dictionary<IReadOnlyList<byte>, IReadOnlyList<byte>> images = new();
  public HixStringPool StringPool { get; }
  public IReadOnlyList<IHixValue> ConstantPool { get; }

  public HixVM(IEnumerable<HixExpressionExecutionProgram> programs, HixStringPool strings = null) {
    using var profile = HixProfiler.Measure("vm.load");
    if (strings != null && strings.Count > ushort.MaxValue + 1)
      throw new ArgumentException("VM string pool cannot exceed 65536 entries (u16 indices)");
    var pool = new HixStringPoolBuilder(strings);
    var constants = new List<IHixValue>();
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

  internal IReadOnlyList<byte> Code(HixExpressionExecutionProgram program) => images[program.Bytecode];

  public string DisassemblePools() => HixExpressionExecutionProgram.DisassemblePools(StringPool, ConstantPool);
  public string Disassemble(HixExpressionExecutionProgram program) => program.Disassemble(Code(program), StringPool, ConstantPool);

  /// <summary>Invoke a global bytecode function using the normal Hix argument and signature rules.</summary>
  public HixInvocationResult Invoke(HixExpressionExecutionProgram program, HixExecutionContext context,
    string function = "main", params IHixValue[] arguments) {
    Code(program);
    context.Attach(this);
    return new LanguageExecution(this, context, program).InvokeGlobal(function, arguments ?? Array.Empty<IHixValue>());
  }

  public HixExpressionResult Run(HixExpressionExecutionProgram program, HixExecutionContext context,
    IDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null) {
    using var profile = HixProfiler.Measure("vm.run");
    if (program == null) throw new ArgumentNullException(nameof(program));
    Code(program);
    // Host keys may predate this VM. Resolve them before attaching the execution context to the fixed VM pool.
    var targets = context.TargetVariables.Select(entry => new KeyValuePair<HixString, IHixValue>(
      HixExecutionContext.Dynamic(entry.Key.Resolve(context.Strings)), entry.Value)).ToArray();
    context.TargetVariables.ReplaceWith(targets);
    context.Attach(this);
    var imported = variables == null ? null : new Dictionary<string, object>(variables, StringComparer.Ordinal);
    var result = new LanguageExecution(this, context, program).Execute(program.Expressions, imported, carries);
    if (variables != null)
      foreach (var item in result.Variables) variables[item.Key] = item.Value;
    return result;
  }

  public static HixExpressionResult Execute(HixExpressionExecutionProgram program, HixExecutionContext context,
    IDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null) {
    using var profile = HixProfiler.Measure("vm.execute.total");
    if (program == null) throw new ArgumentNullException(nameof(program));
    var programs = new HashSet<HixExpressionExecutionProgram> {program};
    var seen = new HashSet<object>();
    if (variables != null) foreach (var value in variables.Values) IncludeProgram(value, programs, seen);
    if (carries != null) foreach (var value in carries.Values) IncludeProgram(value, programs, seen);
    foreach (var entry in context.TargetVariables) IncludeProgram(entry.Value, programs, seen);
    var singleImage = programs.All(image => ReferenceEquals(image.Bytecode, program.Bytecode));
    var machine = singleImage
      ? loadedImages.GetOrCreateValue(program.Bytecode).Get(program, context.Strings)
      : new HixVM(programs, context.Strings);
    return machine.Run(program, context, variables, carries);
  }

  private static void IncludeProgram(object value, HashSet<HixExpressionExecutionProgram> programs, HashSet<object> seen) {
    if (value == null || value is string || !seen.Add(value)) return;
    if (value is NamedFunctionHixValue {Scope: { } scope}) programs.Add(scope.Program);
    else if (value is TupleHixValue tuple) foreach (var item in tuple.Values) IncludeProgram(item, programs, seen);
    else if (value is HixTableValue table) foreach (var entry in table.Entries) IncludeProgram(entry.Value, programs, seen);
    else if (value is IReadOnlyDictionary<string, object> dictionary) foreach (var entry in dictionary) IncludeProgram(entry.Value, programs, seen);
    else if (value is IEnumerable sequence) foreach (var item in sequence) IncludeProgram(item, programs, seen);
  }
}
