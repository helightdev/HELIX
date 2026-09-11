using System;
using System.Collections.Generic;
using System.Linq;
using Hix.Runtime;

namespace Hix.Compiler;

/// <summary>The only syntax-to-executable boundary. No emitted operand retains syntax or a delegate.</summary>
public sealed class HixBytecodeCompiler {
  private readonly HixStringPoolBuilder strings;
  private readonly List<IHixValue> constants = [];
  private readonly List<HixInstruction> code = [];
  private readonly List<int> sourceLines = [];
  private int sourceLine;
  private int nextTemporary;
  private string selectionLocal;
  private HashSet<string> localNames = new(StringComparer.Ordinal);
  private readonly List<(int Instruction, Dictionary<string, int> Labels, string Name)> jumps = [];
  private readonly Stack<Dictionary<string, int>> labels = new();
  private readonly Dictionary<string, (int Instruction, int Line, string Type)> markers = new(StringComparer.Ordinal);
  public HixBytecodeCompiler(HixStringPool seed) => strings = new(seed);
  private int S(string text) => strings.Intern(text).Id;
  private int Emit(HixOpcode op, int a = 0, int b = 0, int line = 0) {
    if (line != 0) sourceLine = line;
    code.Add(new(op, a, b)); sourceLines.Add(sourceLine); return code.Count - 1;
  }
  private void Patch(int pc, int target) => code[pc] = code[pc] with {A = target};
  private void Constant(IHixValue value) {
    if (value is NullHixValue) { Emit(HixOpcode.LoadNull); return; }
    if (value is BooleanHixValue boolean) { Emit(boolean.Value ? HixOpcode.LoadTrue : HixOpcode.LoadFalse); return; }
    Emit(HixOpcode.LoadConst, C(value));
  }
  private int C(IHixValue value) {
    var index = constants.FindIndex(item => item.Equals(value));
    if (index < 0) { index = constants.Count; constants.Add(value); }
    return index;
  }
  private int FunctionReference(SignatureHixPattern signature) {
    // Equal signatures can bind to different lexical declarations. Keep call-site slots distinct.
    var index = constants.Count;
    constants.Add(new FunctionReferenceHixValue(signature));
    return index;
  }
  public HixProgramImage Compile(IReadOnlyList<ExpressionDeclarationIr> expressions,
    IReadOnlyList<FunctionDeclarationIr> functions, HixCompilerCatalog globals,
    IReadOnlyList<SignatureField> parameters = null) {
    var globalFunctions = globals.Functions.Select(Function).ToArray();
    var localFunctions = functions.Select(Function).ToArray();
    var derivationBodies = globals.Derivations.Select(declaration => (
      declaration.Name, declaration.Line,
      Expressions: declaration.Declarations.OfType<ExpressionDeclarationIr>().Select(Expression).ToArray(),
      Functions: declaration.Declarations.OfType<FunctionDeclarationIr>().Select(Function).ToArray())).ToArray();
    var entries = expressions.Select(Expression).ToArray();
    foreach (var jump in jumps) {
      if (!jump.Labels.TryGetValue(jump.Name, out var target)) throw new ArgumentException("unknown label '" + jump.Name + "'");
      code[jump.Instruction] = code[jump.Instruction] with {A = target};
    }
    var offsets = new int[code.Count + 1];
    for (var i = 0; i < code.Count; i++) offsets[i + 1] = checked(offsets[i] + code[i].Size);
    var bytes = new byte[offsets[code.Count]];
    var lines = new Dictionary<int, int>();
    for (var i = 0; i < code.Count; i++) {
      var instruction = code[i];
      if (instruction.IsRelative) instruction = instruction with {A = offsets[instruction.A] - offsets[i]};
      try { instruction.Encode(bytes, offsets[i]); }
      catch (ArgumentException error) { throw new ArgumentException("Bytecode at line " + sourceLines[i] + ": " + error.Message, error); }
      lines.Add(offsets[i], sourceLines[i]);
    }
    var pool = strings.Freeze();
    if (pool.Count > ushort.MaxValue + 1 || constants.Count > ushort.MaxValue + 1)
      throw new ArgumentException("Bytecode pools cannot exceed 65536 entries (u16 indices)");
    BytecodeFunction MapFunction(BytecodeFunction function) => function with {Body = offsets[function.Body]};
    BytecodeExpression MapExpression(BytecodeExpression expression) => expression with {Body = offsets[expression.Body]};
    var global = new LanguageFunctionScope(globalFunctions.Select(MapFunction).ToArray());
    var scope = new LanguageFunctionScope(localFunctions.Select(MapFunction).ToArray(), global);
    var derivations = derivationBodies.Select(item => new BytecodeDerivation(item.Name, item.Line,
      item.Expressions.Select(MapExpression).ToArray(), new LanguageFunctionScope(item.Functions.Select(MapFunction).ToArray(), global))).ToArray();
    var mappedMarkers = markers.ToDictionary(item => item.Key,
      item => new BytecodeMarker(item.Value.Instruction < offsets.Length
        ? offsets[item.Value.Instruction]
        : bytes.Length, item.Value.Line, item.Value.Type), StringComparer.Ordinal);
    return new(bytes, lines, constants.ToArray(), pool, entries.Select(MapExpression).ToArray(), derivations, scope,
      globals.Patterns, globals.Backend, Fields(parameters), mappedMarkers);
  }
  private BytecodeExpression Expression(ExpressionDeclarationIr expression) =>
    new(EntryBlock(expression.Body), expression.IsPrelude, expression.Line);
  private BytecodeFunction Function(FunctionDeclarationIr function) {
    var start = EntryBlock(function.Body);
    return new(function.Name, function.IsPure, function.Signatures.Select(signature => new BytecodeSignature(
      signature.InputPattern, Fields(signature.Inputs), signature.OutputPattern, Fields(signature.Outputs))).ToArray(), start);
  }
  private IReadOnlyList<BytecodeField> Fields(IReadOnlyList<SignatureField> fields) =>
    fields?.Select(field => new BytecodeField(field.Name, field.Pattern, field.Variadic, field.Optional,
      field.DefaultValue == null ? null : DefaultValue(field.DefaultValue))).ToArray();
  private static IHixValue DefaultValue(ExpressionIr value) => value switch {
    StringExpressionIr text => new LiteralHixValue(HixString.Dynamic(text.Value)),
    NumberExpressionIr number => new NumberHixValue(number.Value),
    BooleanExpressionIr boolean => BooleanHixValue.From(boolean.Value),
    NullExpressionIr => NullHixValue.Instance,
    MissingExpressionIr => MissingHixValue.Instance,
    TupleExpressionIr tuple => new TupleHixValue(tuple.Values.Select(DefaultValue).ToArray()),
    TableExpressionIr table => new HixTableValue(table.Entries.Select(entry =>
      new KeyValuePair<HixString, IHixValue>(HixString.Dynamic(entry.Key), DefaultValue(entry.Value)))),
    _ => throw new ArgumentException("parameter defaults must be constant values")
  };
  private int EntryBlock(BlockStatementIr block) {
    var previous = localNames;
    localNames = new HashSet<string>(StringComparer.Ordinal);
    selectionLocal = null;
    CollectLocals(block);
    var start = Block(block);
    localNames = previous;
    return start;
  }
  private void CollectLocals(HixIrNode node) {
    if (node is AssignmentStatementIr {Storage: StorageSpace.Local} assignment) localNames.Add(assignment.Name);
    foreach (var child in node.SemanticChildren) CollectLocals(child);
  }
  private string TemporaryLocal() {
    string name;
    do { name = "__selector_" + nextTemporary++; } while (!localNames.Add(name));
    return name;
  }
  private void LoadMember(string root, string member) {
    Emit(HixOpcode.LoadRoot, S(root)); Emit(HixOpcode.Member, S(member));
  }
  private int Block(BlockStatementIr block) {
    var localLabels = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (var statement in block.Statements) {
      var name = statement switch {ControlFlowStatementIr {Operation: ControlFlowKind.Label} marker => marker.Label,
        BlockStatementIr nested => nested.Label, _ => null};
      if (name != null) {
        if (localLabels.ContainsKey(name)) throw new ArgumentException("duplicate label '" + name + "'");
        localLabels.Add(name, -1);
      }
    }
    labels.Push(localLabels);
    var start = Emit(HixOpcode.Enter);
    foreach (var statement in block.Statements) {
      var name = statement switch {ControlFlowStatementIr {Operation: ControlFlowKind.Label} marker => marker.Label,
        BlockStatementIr nested => nested.Label, _ => null};
      if (name != null) localLabels[name] = code.Count;
      Statement(statement);
    }
    Emit(HixOpcode.End);
    Patch(start, code.Count);
    labels.Pop();
    return start;
  }
  private void NestedBlock(BlockStatementIr block) {
    var skip = Emit(HixOpcode.Jump);
    var start = Block(block);
    Patch(skip, code.Count);
    Emit(HixOpcode.Block, start);
  }
  private void Statement(StatementIr statement) {
    var line = statement.Line;
    sourceLine = line;
    foreach (var metadata in statement.Metadata.Where(item => item.Name == "marker")) {
      if (metadata.Values.Count != 1 || metadata.Values[0] is not StringExpressionIr name)
        throw new ArgumentException("%marker requires one literal string");
      if (markers.ContainsKey(name.Value)) throw new ArgumentException("duplicate bytecode marker '" + name.Value + "'");
      var type = statement switch {
        AssignmentStatementIr assignment => assignment.Value?.InferredPattern.Display ?? "missing",
        InvocationStatementIr invocation => invocation.Call.InferredPattern.Display,
        ExpressionStatementIr expression => expression.Expression.InferredPattern.Display,
        _ => "null"
      };
      markers.Add(name.Value, (code.Count, line, type));
    }
    switch (statement) {
      case BlockStatementIr block: NestedBlock(block); break;
      case AssignmentStatementIr assignment:
        var store = assignment.IsCarried ? HixOpcode.StoreCarry : (int)assignment.Storage == 0 ? HixOpcode.StoreLocal
          : (int)assignment.Storage == 1 ? HixOpcode.StoreVariable : HixOpcode.StoreTarget;
        if (store != HixOpcode.StoreLocal) Emit(store == HixOpcode.StoreCarry ? HixOpcode.CheckStoreCarry
          : store == HixOpcode.StoreVariable ? HixOpcode.CheckStoreVariable : HixOpcode.CheckStoreTarget, line: line);
        if (assignment.Value == null) Constant(NullHixValue.Instance); else Value(assignment.Value);
        Emit(store, S(assignment.Name), line: line); break;
      case InvocationStatementIr invocation: Value(invocation.Call); Emit(HixOpcode.Pop); break;
      case ExpressionStatementIr expression: Value(expression.Expression); Emit(HixOpcode.Pop); break;
      case SelectionStatementIr selection: Value(selection.Selection); Emit(HixOpcode.Pop); break;
      case ControlFlowStatementIr {Operation: ControlFlowKind.Label}: break;
      case ControlFlowStatementIr flow:
        foreach (var value in flow.Values) { Value(value); if (flow.Operation != ControlFlowKind.Return) Emit(HixOpcode.Pop); }
        if (flow.Operation == ControlFlowKind.Return && flow.Values.Count != 1) Emit(HixOpcode.Pack, flow.Values.Count);
        var owner = flow.Operation == ControlFlowKind.Goto ? labels.FirstOrDefault(item => item.ContainsKey(flow.Label)) : null;
        var instruction = Emit(flow.Operation switch {
          ControlFlowKind.Return => HixOpcode.Return, ControlFlowKind.Goto => owner == null ? HixOpcode.InvalidGoto : HixOpcode.Goto,
          ControlFlowKind.Break => HixOpcode.Break, ControlFlowKind.Continue => HixOpcode.Continue,
          _ => throw new ArgumentException("invalid control flow")
        }, line: line);
        if (owner != null) jumps.Add((instruction, owner, flow.Label));
        break;
      default: throw new ArgumentException("unknown statement at line " + line);
    }
  }
  private void Value(ExpressionIr expression, bool check = false) {
    var line = expression.Line;
    sourceLine = line;
    var handler = check ? Emit(HixOpcode.Check) : -1;
    switch (expression) {
      case StringExpressionIr text: Emit(HixOpcode.LoadString, S(text.Value)); break;
      case NumberExpressionIr number: Constant(new NumberHixValue(number.Value)); break;
      case BooleanExpressionIr boolean: Constant(boolean.Value ? BooleanHixValue.True : BooleanHixValue.False); break;
      case NullExpressionIr: Constant(NullHixValue.Instance); break;
      case MissingExpressionIr: Constant(MissingHixValue.Instance); break;
      case RootExpressionIr root:
        switch (root.Binding.Kind) {
          case HixReferenceKind.Local: LoadMember("local", root.Binding.Variable.Name); break;
          case HixReferenceKind.Parameter:
            if (root.Binding.ParameterIndex < 0) Emit(HixOpcode.LoadRoot, S("param"));
            else LoadMember("args", root.Binding.ParameterIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));
            break;
          case HixReferenceKind.Invalid: Emit(HixOpcode.Throw, S("unknown local or parameter '" + root.Name + "'")); break;
          case HixReferenceKind.Unbound: throw new InvalidOperationException("Root '" + root.Name + "' must be bound before bytecode generation");
          default: Emit(HixOpcode.LoadRoot, S(root.Binding.Name)); break;
        }
        break;
      case SelectorExpressionIr:
        if (selectionLocal == null) throw new ArgumentException("Selection value outside a selection condition");
        LoadMember("local", selectionLocal); break;
      case MemberExpressionIr member: Value(member.Receiver); Emit(HixOpcode.Member, S(member.Member), line: line); break;
      case InterpolationExpressionIr interpolation:
        foreach (var part in interpolation.Parts) { Value(part); Emit(HixOpcode.CastString, line: part.Line); }
        Emit(HixOpcode.Interpolate, interpolation.Parts.Count); break;
      case TupleExpressionIr tuple:
        foreach (var value in tuple.Values) Value(value);
        if (tuple.Values.Count == 0) Emit(HixOpcode.LoadTuple); else Emit(HixOpcode.PackTuple, tuple.Values.Count); break;
      case TableExpressionIr table:
        var duplicate = table.Entries.GroupBy(entry => entry.Key, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicate != null) { Emit(HixOpcode.Throw, S("duplicate table key '" + duplicate.Key + "'")); break; }
        foreach (var entry in table.Entries) { Emit(HixOpcode.LoadString, S(entry.Key)); Value(entry.Value); }
        if (table.Entries.Count == 0) Emit(HixOpcode.LoadTable); else Emit(HixOpcode.PackTable, table.Entries.Count); break;
      case UnaryExpressionIr unary:
        Value(unary.Value, unary.Operation == UnaryOperation.Check);
        if (unary.Operation != UnaryOperation.Check) Emit(HixOpcode.Not);
        break;
      case FallbackExpressionIr fallback:
        Value(fallback.Value);
        var other = Emit(HixOpcode.JumpNotNull);
        Emit(HixOpcode.Pop); Value(fallback.Fallback); Patch(other, code.Count); break;
      case CallExpressionIr call:
        var arguments = call.EffectiveArguments;
        foreach (var argument in arguments) Value(argument);
        if (call.Binding.Kind == HixCallKind.Unbound)
          throw new InvalidOperationException("Call '" + call.Name + "' must be bound before bytecode generation");
        if (call.Binding.Kind == HixCallKind.Static)
          Emit(HixOpcode.Call, FunctionReference(call.Binding.Signature), arguments.Count, line: line);
        else Emit(HixOpcode.CallDynamic, S(call.Name), arguments.Count, line: line);
        if (call.CoerceBoolean) Emit(HixOpcode.CastBoolean);
        break;
      case InlineExpressionIr inline: NestedBlock(inline.Body); LoadMember("local", inline.ResultLocal); break;
      case SelectionExpressionIr selection:
        var previousSelectionLocal = selectionLocal;
        var temporary = TemporaryLocal();
        if (selection.Selector == null) Constant(BooleanHixValue.True); else Value(selection.Selector);
        Emit(HixOpcode.StoreLocal, S(temporary));
        selectionLocal = temporary;
        var ends = new List<int>();
        foreach (var branch in selection.Branches) {
          var next = new List<int>();
          foreach (var condition in branch.Conditions) {
            if (!branch.IsTransformation && selection.Selector != null) LoadMember("local", temporary);
            Value(condition);
            if (!branch.IsTransformation && selection.Selector != null) Emit(HixOpcode.Equal);
            next.Add(Emit(HixOpcode.JumpFalse));
          }
          Result(branch.Result); ends.Add(Emit(HixOpcode.Jump));
          foreach (var jump in next) Patch(jump, code.Count);
        }
        Result(selection.Fallback);
        foreach (var jump in ends) Patch(jump, code.Count);
        selectionLocal = previousSelectionLocal; break;
      default: throw new ArgumentException("unknown expression '" + expression.GetType().Name + "' at line " + line);
    }
    if (check) { Emit(HixOpcode.EndCheck); Patch(handler, code.Count); }
  }
  private void Result(HixIrNode result) {
    if (result is ExpressionIr expression) Value(expression);
    else { if (result is StatementIr statement) Statement(statement); Constant(NullHixValue.Instance); }
  }
}
