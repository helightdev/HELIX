using Hix.Compiler;
using Hix.Runtime;
using K = Hix.HixValueKind;

namespace Hix.Standalone;

internal static class CompilationFunctions {
  public static void Register(FunctionSignatureRegistryBuilder functions) {
    functions.Add(new FunctionDefinition("marker", [
        new FunctionSignature(K.Pattern, [K.String], ArgumentNames: ["name"])
      ], "Names a statement or variable declaration for standalone compilation inspection.",
      HixMetadataKind.Statement | HixMetadataKind.VariableDeclaration));
    functions.Add(
      Function("compile", [new(K.Table, []), new(K.Table, [K.String])], Compile),
      Function("success", [new(K.Bool, [K.Table])], (thread, args) => Field(thread, args[0], "success")),
      Function("diagnostics", [new(K.Tuple, [K.Table])], (thread, args) => Field(thread, args[0], "diagnostics")),
      Function("functions", [new(K.Tuple, [K.Table])], (thread, args) => Field(thread, args[0], "functions")),
      Function("function", [new(K.Table, [K.Table, K.String])], FindFunction),
      Function("disassemble", [new(K.String, [K.Table])], (thread, args) => Field(thread, args[0], "disassembly")),
      Function("instructions", [new(K.Tuple, [K.Table])], (thread, args) => Field(thread, args[0], "instructions")),
      Function("marker", [new(K.Table, [K.Table, K.String])], FindMarker),
      Function("opcode", [new(K.String, [K.Table])], (thread, args) => Field(thread, args[0], "opcode")),
      Function("offset", [new(K.Number, [K.Table])], (thread, args) => Field(thread, args[0], "offset")),
      Function("operands", [new(K.Tuple, [K.Table])], (thread, args) => Field(thread, args[0], "operands")),
      Function("target", [new(K.Any, [K.Table])], (thread, args) => Field(thread, args[0], "target")),
      Function("line", [new(K.Number, [K.Table])], (thread, args) => Field(thread, args[0], "line")),
      Function("inferredType", [new(K.String, [K.Table])], (thread, args) => Field(thread, args[0], "type"))
    );
  }

  private static SimpleFunction Function(string name, IReadOnlyList<FunctionSignature> signatures,
    InlineFunction implementation) => new(name, signatures, implementation,
      documentation: "Inspects standalone Hix compilation artifacts.");

  private static IHixValue Compile(HixThread thread, IHixValue[] args) {
    var backend = (HixStandaloneBackend)thread.Backend;
    var sources = args.Length == 0 ? backend.CompilationSources : [thread.ResolveText(args[0])];
    if (sources.Count == 0) return Failure("compile() requires an enclosing standalone compilation");
    try { return Artifact(HixCompiler.CompileFunctions(sources, backend)); }
    catch (ArgumentException exception) { return Failure(exception.Message); }
  }

  private static HixTableValue Artifact(HixProgramImage program) {
    var decoded = HixInstruction.ReadAll(program.Bytecode).ToArray();
    var allFunctions = program.Scope.DisassemblyFunctions("standalone").Select(item => item.Function)
      .Distinct().OrderBy(item => item.Body).ToArray();
    var instructionValues = decoded.Select(item => (IHixValue)Instruction(program, item.Offset, item.Instruction)).ToArray();
    var functionValues = allFunctions.Select((function, index) => (IHixValue)Table(
      ("artifact", Text("function")), ("name", Text(function.Name)), ("offset", Number(function.Body)),
      ("signature", Text(string.Join("; ", function.Signatures.Select(signature => signature.Constant(function.Name).Display)))),
      ("instructions", new TupleHixValue(decoded.Where(item => item.Offset >= function.Body &&
          item.Offset < (index + 1 < allFunctions.Length ? allFunctions[index + 1].Body : program.Bytecode.Count))
        .Select(item => (IHixValue)Instruction(program, item.Offset, item.Instruction)).ToArray())),
      ("disassembly", Text(program.Disassemble())))).ToArray();
    var markers = program.Markers.Select(item => new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(item.Key),
      Table(("artifact", Text("marker")), ("offset", Number(item.Value.Offset)), ("line", Number(item.Value.Line)),
        ("type", Text(item.Value.Type)), ("instruction", item.Value.Offset < program.Bytecode.Count
          ? Instruction(program, item.Value.Offset, HixInstruction.Decode(program.Bytecode, item.Value.Offset))
          : MissingHixValue.Instance)))).ToArray();
    return Table(("artifact", Text("compilation")), ("success", BooleanHixValue.True),
      ("diagnostics", TupleHixValue.Empty), ("functions", new TupleHixValue(functionValues)),
      ("instructions", new TupleHixValue(instructionValues)), ("markers", new HixTableValue(markers)),
      ("disassembly", Text(program.Disassemble())));
  }

  private static HixTableValue Instruction(HixProgramImage program, int offset, HixInstruction instruction) {
    IHixValue target = instruction.IsRelative ? Number(offset + instruction.A) : MissingHixValue.Instance;
    return Table(("artifact", Text("instruction")), ("opcode", Text(instruction.Opcode.ToString())),
      ("offset", Number(offset)), ("line", Number(program.SourceLines.TryGetValue(offset, out var line) ? line : 0)),
      ("operands", new TupleHixValue(instruction.Size == 1 ? [] : instruction.Size == 5
        ? [Number(instruction.A), Number(instruction.B)] : [Number(instruction.A)])), ("target", target));
  }

  private static HixTableValue Failure(string diagnostic) => Table(("artifact", Text("compilation")),
    ("success", BooleanHixValue.False), ("diagnostics", new TupleHixValue([Text(diagnostic)])),
    ("functions", TupleHixValue.Empty), ("instructions", TupleHixValue.Empty),
    ("markers", HixTableValue.Empty), ("disassembly", Text("")));

  private static IHixValue FindFunction(HixThread thread, IHixValue[] args) {
    var name = thread.ResolveText(args[1]);
    return ((TupleHixValue)Field(thread, args[0], "functions")).Values.FirstOrDefault(value =>
      thread.Equal(Field(thread, value, "name"), Text(name))) ?? thread.Error("unknown compiled function '" + name + "'");
  }

  private static IHixValue FindMarker(HixThread thread, IHixValue[] args) {
    var markers = Field(thread, args[0], "markers");
    var name = thread.ResolveText(args[1]);
    var marker = ((HixTableValue)markers).Select(thread, HixString.Dynamic(name));
    return marker is MissingHixValue ? thread.Error("unknown bytecode marker '" + name + "'") : marker;
  }

  private static IHixValue Field(HixThread thread, IHixValue value, string name) =>
    value is HixTableValue table ? table.Select(thread, HixString.Dynamic(name))
      : thread.Error("expected compilation artifact");
  private static HixTableValue Table(params (string Name, IHixValue Value)[] entries) => new(entries.Select(entry =>
    new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(entry.Name), entry.Value)));
  private static LiteralHixValue Text(string value) => HixThread.String(value);
  private static NumberHixValue Number(double value) => new(value);
}
