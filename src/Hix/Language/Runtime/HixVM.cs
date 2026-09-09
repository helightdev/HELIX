using Hix.Env;
using System;
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

  private readonly HixExpressionExecutionProgram image;
  internal sealed record PreparedInvocation(LanguageFunctionCandidate Language, FunctionDefinition Backend,
    FunctionSignature BackendSignature = null);
  public HixStringPool StringPool { get; }
  public IReadOnlyList<IHixValue> ConstantPool { get; }

  public HixVM(IEnumerable<HixExpressionExecutionProgram> programs, HixStringPool strings = null) {
    using var profile = HixProfiler.Measure("vm.load");
    var distinct = programs.Distinct().ToArray();
    if (distinct.Length != 1) throw new ArgumentException("A VM executes exactly one prepared program image", nameof(programs));
    image = distinct[0];
    StringPool = image.StringPool;
    ConstantPool = image.ConstantPool;
  }

  internal IReadOnlyList<byte> Code(HixExpressionExecutionProgram program) =>
    ReferenceEquals(image.Bytecode, program.Bytecode) ? program.Bytecode
      : throw new ArgumentException("Execution state contains a function from another program image", nameof(program));
  internal PreparedInvocation PreparedCall(HixExpressionExecutionProgram program, int address) =>
    program.PreparedCalls[address];

  internal static IReadOnlyDictionary<int, PreparedInvocation> BindCalls(HixExpressionExecutionProgram program) {
    var calls = new Dictionary<int, PreparedInvocation>();
    foreach (var (pc, instruction) in HixInstruction.ReadAll(program.Bytecode)) {
      if (instruction.Opcode != HixOpcode.Call) continue;
      var signature = (SignatureHixPattern)((PatternHixValue)program.ConstantPool[instruction.A]).Pattern;
      var candidate = ScopeAt(program, pc, program.Bytecode).Resolve(signature);
      if (candidate != null) { calls.Add(pc, new(candidate, null)); continue; }
      var definitions = program.Backend.Functions.Resolve(signature.Name, instruction.B)
        .SelectMany(definition => definition.Signatures.Select(item => (definition, signature: item)))
        .Where(item => BuiltinSignature(item.definition.Name, item.signature).Display == signature.Display).ToArray();
      if (definitions.Length != 1) throw new ArgumentException("cannot bind signature '" + signature.Display + "' while preparing program");
      calls.Add(pc, new(null, definitions[0].definition, definitions[0].signature));
    }
    return new System.Collections.ObjectModel.ReadOnlyDictionary<int, PreparedInvocation>(calls);
  }

  private static SignatureHixPattern BuiltinSignature(string name, FunctionSignature signature) => new(name,
    signature.ArgumentTypes.Select((kind, index) => new HixPatternField(index.ToString(),
      kind == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(kind))).ToArray(),
    signature.ResultType == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(signature.ResultType));

  private static LanguageFunctionScope ScopeAt(HixExpressionExecutionProgram program, int address, IReadOnlyList<byte> code) {
    var scopes = new[] {program.Scope}.Concat(program.Derivations.Select(value => value.Scope)).Distinct().ToArray();
    foreach (var scope in scopes)
      foreach (var candidate in scope.AllCandidates()) {
        var body = candidate.Function.Body;
        var header = HixInstruction.Decode(code, body);
        if (address >= body && address < body + header.A) return candidate.Owner;
      }
    foreach (var derivation in program.Derivations)
      foreach (var expression in derivation.Expressions) {
        var header = HixInstruction.Decode(code, expression.Body);
        if (address >= expression.Body && address < expression.Body + header.A) return derivation.Scope;
      }
    return program.Scope;
  }

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
    var machine = loadedImages.GetOrCreateValue(program.Bytecode).Get(program, context.Strings);
    return machine.Run(program, context, variables, carries);
  }
}
