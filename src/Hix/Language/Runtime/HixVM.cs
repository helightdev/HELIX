using Hix.Compiler;
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

  // Loaded code and pools are immutable; only the synchronized local pool is shared.
  private static readonly ConditionalWeakTable<HixProgramImage, HixVM> loadedImages = new();

  private readonly HixProgramImage image;
  public HixStringPool StringPool { get; }
  public IReadOnlyList<IHixValue> ConstantPool { get; }

  public HixVM(IEnumerable<HixProgramImage> programs) {
    using var profile = HixProfiler.Measure("vm.load");
    var distinct = programs.Distinct().ToArray();
    if (distinct.Length != 1) throw new ArgumentException("A VM executes exactly one prepared program image", nameof(programs));
    image = distinct[0];
    StringPool = image.StringPool;
    ConstantPool = LoadConstants(image);
  }

  internal IReadOnlyList<byte> Code(HixProgramImage program) =>
    ReferenceEquals(image.Bytecode, program.Bytecode) ? program.Bytecode
      : throw new ArgumentException("Execution state contains a function from another program image", nameof(program));
  private static IReadOnlyList<IHixValue> LoadConstants(HixProgramImage program) {
    var constants = program.ConstantPool.ToArray();
    var scopes = new LanguageFunctionScope[constants.Length];
    var ranges = ScopeRanges(program);
    var rangeIndex = 0;
    foreach (var (pc, instruction) in HixInstruction.ReadAll(program.Bytecode)) {
      if (instruction.Opcode is not (HixOpcode.Call or HixOpcode.LoadConst) ||
          constants[instruction.A] is not FunctionReferenceHixValue) continue;
      while (rangeIndex < ranges.Length && pc >= ranges[rangeIndex].End) rangeIndex++;
      var scope = rangeIndex < ranges.Length && pc >= ranges[rangeIndex].Start
        ? ranges[rangeIndex].Scope : program.Scope;
      if (scopes[instruction.A] is { } previous && !ReferenceEquals(previous, scope))
        throw new ArgumentException("A function-reference constant cannot be shared across lexical scopes");
      scopes[instruction.A] = scope;
    }
    var resolved = new Dictionary<(LanguageFunctionScope Scope, string Signature), ResolvedFunctionHixValue>();
    for (var index = 0; index < constants.Length; index++) {
      if (constants[index] is not FunctionReferenceHixValue reference) continue;
      var signature = reference.Signature;
      var owner = scopes[index] ?? program.Scope;
      var key = (owner, signature.Display);
      if (resolved.TryGetValue(key, out var cached)) { constants[index] = cached; continue; }
      var candidate = owner.Resolve(signature);
      if (candidate != null) {
        constants[index] = resolved[key] = new ResolvedFunctionHixValue(signature, candidate);
        continue;
      }
      var definitions = program.Backend.Functions.Resolve(signature.Name, signature.Parameters.Count)
        .SelectMany(definition => definition.Signatures.Select(item => (definition, signature: item)))
        .Where(item => BuiltinSignature(item.definition.Name, item.signature).Display == signature.Display).ToArray();
      if (definitions.Length != 1) throw new ArgumentException("cannot bind signature '" + signature.Display + "' while loading program");
      constants[index] = resolved[key] = new ResolvedFunctionHixValue(signature, definitions[0].definition, definitions[0].signature);
    }
    return Array.AsReadOnly(constants);
  }

  private static SignatureHixPattern BuiltinSignature(string name, FunctionSignature signature) => new(name,
    signature.ArgumentTypes.Select((kind, index) => new HixPatternField(signature.GetArgumentName(index) ?? index.ToString(),
      kind == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(kind), signature.GetArgumentDefault(index) != null)).ToArray(),
    signature.ResultType == HixValueKind.Any ? HixPattern.Any : new KindHixPattern(signature.ResultType));

  private readonly record struct ScopeRange(int Start, int End, LanguageFunctionScope Scope);

  private static ScopeRange[] ScopeRanges(HixProgramImage program) {
    var ranges = new Dictionary<int, ScopeRange>();
    void Add(int body, LanguageFunctionScope scope) {
      if (!ranges.ContainsKey(body))
        ranges.Add(body, new(body, body + HixInstruction.Decode(program.Bytecode, body).A, scope));
    }
    foreach (var scope in new[] {program.Scope}.Concat(program.Derivations.Select(value => value.Scope)).Distinct())
      foreach (var candidate in scope.AllCandidates()) Add(candidate.Function.Body, candidate.Owner);
    foreach (var derivation in program.Derivations)
      foreach (var expression in derivation.Expressions) Add(expression.Body, derivation.Scope);
    return ranges.Values.OrderBy(range => range.Start).ToArray();
  }

  public string DisassemblePools() => HixProgramImage.DisassemblePools(StringPool, ConstantPool);
  public string Disassemble(HixProgramImage program) => program.Disassemble(Code(program), StringPool, ConstantPool);

  /// <summary>Invoke a global bytecode function using the normal Hix argument and signature rules.</summary>
  public HixInvocationResult Invoke(HixProgramImage program, HixThread thread,
    string function = "main", params IHixValue[] arguments) {
    Code(program);
    thread.Start(this, program);
    try {
      var result = thread.InvokeGlobal(function, arguments ?? Array.Empty<IHixValue>());
      return result;
    }
    finally { thread.Stop(); }
  }

  public HixExecutionResult Run(HixProgramImage program, HixThread thread,
    IDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null) {
    using var profile = HixProfiler.Measure("vm.run");
    if (program == null) throw new ArgumentNullException(nameof(program));
    Code(program);
    thread.Start(this, program);
    try {
      var result = thread.Execute(program.Expressions, variables, carries);
      if (variables != null)
        foreach (var item in thread.ExportVariables()) variables[item.Key] = item.Value;
      return result;
    } finally { thread.Stop(); }
  }

  public static HixExecutionResult Execute(HixProgramImage program, HixThread thread,
    IDictionary<string, object> variables = null, IReadOnlyDictionary<string, object> carries = null) {
    using var profile = HixProfiler.Measure("vm.execute.total");
    if (program == null) throw new ArgumentNullException(nameof(program));
    var machine = loadedImages.GetValue(program, image => new HixVM(new[] {image}));
    return machine.Run(program, thread, variables, carries);
  }
}
