using System;
using System.Collections.Generic;
using System.Linq;
using Mixins.Runtime;

namespace Mixins.Compiler;

/// <summary>The only syntax-to-executable boundary. No emitted operand retains syntax or a delegate.</summary>
internal sealed class HixBytecodeCompiler {
  private readonly MixinStringPoolBuilder strings;
  private readonly List<IMixinValue> constants = [];
  private readonly List<HixInstruction> code = [];
  private readonly List<int> sourceLines = [];
  private int sourceLine;
  private readonly List<BytecodeTarget> targets = [];
  private readonly List<(int Instruction, Dictionary<string, int> Labels, string Name)> jumps = [];
  private readonly Stack<Dictionary<string, int>> labels = new();
  internal HixBytecodeCompiler(MixinStringPool seed) => strings = new(seed);
  private int S(string text) => strings.Intern(text).Id;
  private int Emit(HixOpcode op, int a = 0, int b = 0, int line = 0) {
    if (line != 0) sourceLine = line;
    code.Add(new(op, a, b)); sourceLines.Add(sourceLine); return code.Count - 1;
  }
  private void Patch(int pc, int target) => code[pc] = code[pc] with {A = target};
  private void Constant(IMixinValue value) {
    var index = constants.FindIndex(item => item.Equals(value));
    if (index < 0) { index = constants.Count; constants.Add(value); }
    Emit(HixOpcode.Constant, index);
  }
  internal MixinExpressionExecutionProgram Compile(IReadOnlyList<ExpressionDeclarationAst> expressions,
    IReadOnlyList<FunctionDeclarationAst> functions, MixinExpressionPreparedState globals) {
    foreach (var expression in expressions.Where(expression => !expression.IsPrelude)) CollectTargets(expression);
    var globalFunctions = globals.Functions.Select(Function).ToArray();
    var localFunctions = functions.Select(Function).ToArray();
    var derivationBodies = globals.Derivations.Select(declaration => (
      declaration.Name, declaration.Line,
      Expressions: declaration.Declarations.OfType<ExpressionDeclarationAst>().Select(Expression).ToArray(),
      Functions: declaration.Declarations.OfType<FunctionDeclarationAst>().Select(Function).ToArray())).ToArray();
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
    return new(bytes, lines, constants.ToArray(), pool, entries.Select(MapExpression).ToArray(), derivations, scope, targets.ToArray());
  }
  private BytecodeExpression Expression(ExpressionDeclarationAst expression) =>
    new(Block(expression.Body), expression.IsPrelude, expression.Line);
  private BytecodeFunction Function(FunctionDeclarationAst function) {
    var start = Block(function.Body);
    return new(function.Name, function.IsPure, function.Signatures.Select(signature => new BytecodeSignature(
      signature.InputKind, Fields(signature.Inputs), signature.OutputKind, Fields(signature.Outputs))).ToArray(), start);
  }
  private static IReadOnlyList<BytecodeField> Fields(IReadOnlyList<SignatureField> fields) =>
    fields?.Select(field => new BytecodeField(field.Name, field.Kind, field.Variadic)).ToArray();
  private int Block(BlockStatementAst block) {
    var localLabels = new Dictionary<string, int>(StringComparer.Ordinal);
    foreach (var statement in block.Statements) {
      var name = statement switch {ControlFlowStatementAst {Operation: ControlFlowKind.Label} marker => marker.Label,
        BlockStatementAst nested => nested.Label, _ => null};
      if (name != null) {
        if (localLabels.ContainsKey(name)) throw new ArgumentException("duplicate label '" + name + "'");
        localLabels.Add(name, -1);
      }
    }
    labels.Push(localLabels);
    var start = Emit(HixOpcode.Enter);
    foreach (var statement in block.Statements) {
      var name = statement switch {ControlFlowStatementAst {Operation: ControlFlowKind.Label} marker => marker.Label,
        BlockStatementAst nested => nested.Label, _ => null};
      if (name != null) localLabels[name] = code.Count;
      Statement(statement);
    }
    Emit(HixOpcode.End);
    Patch(start, code.Count);
    labels.Pop();
    return start;
  }
  private void NestedBlock(BlockStatementAst block) {
    var skip = Emit(HixOpcode.Jump);
    var start = Block(block);
    Patch(skip, code.Count);
    Emit(HixOpcode.Block, start);
  }
  private void Statement(StatementAst statement) {
    var line = statement.Line;
    sourceLine = line;
    switch (statement) {
      case BlockStatementAst block: NestedBlock(block); break;
      case AssignmentStatementAst assignment:
        var store = assignment.IsCarried ? HixOpcode.StoreCarry : (int)assignment.Storage == 0 ? HixOpcode.StoreLocal
          : (int)assignment.Storage == 1 ? HixOpcode.StoreVariable : HixOpcode.StoreTarget;
        if (store != HixOpcode.StoreLocal) Emit(store == HixOpcode.StoreCarry ? HixOpcode.CheckStoreCarry
          : store == HixOpcode.StoreVariable ? HixOpcode.CheckStoreVariable : HixOpcode.CheckStoreTarget, line: line);
        Value(assignment.Value);
        Emit(store, S(assignment.Name), line: line); break;
      case InvocationStatementAst invocation: Value(invocation.Call); Emit(HixOpcode.Pop); break;
      case SelectionStatementAst selection: Value(selection.Selection); Emit(HixOpcode.Pop); break;
      case ControlFlowStatementAst {Operation: ControlFlowKind.Label}: break;
      case ControlFlowStatementAst flow:
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
  private void Value(ExpressionAst expression, bool check = false) {
    var line = expression.Line;
    sourceLine = line;
    var handler = check ? Emit(HixOpcode.Check) : -1;
    switch (expression) {
      case StringExpressionAst text: Emit(HixOpcode.String, S(text.Value)); break;
      case NumberExpressionAst number: Constant(new NumberMixinValue(number.Value)); break;
      case BooleanExpressionAst boolean: Constant(boolean.Value ? BooleanMixinValue.True : BooleanMixinValue.False); break;
      case NullExpressionAst: Constant(NullMixinValue.Instance); break;
      case RootExpressionAst root: Emit(root.IsSmart ? HixOpcode.SmartRoot : HixOpcode.Root, S(root.Name)); break;
      case MemberExpressionAst {Receiver: RootExpressionAst {IsSmart: false, Name: "this" or "target" or "attr"} root} member:
        Emit(root.Name == "this" ? HixOpcode.HostThis : root.Name == "target" ? HixOpcode.HostTarget : HixOpcode.HostAttribute, S(member.Member)); break;
      case MemberExpressionAst {Receiver: RootExpressionAst {IsSmart: false, Name: "local" or "var" or "tar"} root} member:
        Emit(root.Name == "local" ? HixOpcode.LoadLocal : root.Name == "var" ? HixOpcode.LoadVariable : HixOpcode.LoadTarget, S(member.Member)); break;
      case MemberExpressionAst member: Value(member.Receiver); Emit(HixOpcode.Member, S(member.Member), line: line); break;
      case InterpolationExpressionAst interpolation:
        foreach (var part in interpolation.Parts) { Value(part); Emit(HixOpcode.Text, line: part.Line); }
        Emit(HixOpcode.Interpolate, interpolation.Parts.Count); break;
      case TupleExpressionAst tuple:
        foreach (var value in tuple.Values) Value(value);
        Emit(HixOpcode.Tuple, tuple.Values.Count); break;
      case TableExpressionAst table:
        var duplicate = table.Entries.GroupBy(entry => entry.Key, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicate != null) { Emit(HixOpcode.Error, S("duplicate table key '" + duplicate.Key + "'")); break; }
        foreach (var entry in table.Entries) { Emit(HixOpcode.String, S(entry.Key)); Value(entry.Value); }
        Emit(HixOpcode.Table, table.Entries.Count); break;
      case UnaryExpressionAst unary:
        Value(unary.Value, unary.Operation == UnaryOperation.Check);
        if (unary.Operation != UnaryOperation.Check) Emit(HixOpcode.Not);
        break;
      case FallbackExpressionAst fallback:
        Value(fallback.Value);
        var other = Emit(HixOpcode.JumpNotNull);
        Emit(HixOpcode.Pop); Value(fallback.Fallback); Patch(other, code.Count); break;
      case CallExpressionAst call:
        foreach (var argument in call.Arguments) Value(argument);
        Emit(HixOpcode.Call, S(call.Name), call.Arguments.Count, line: line);
        if (call.CoerceBoolean) Emit(HixOpcode.Boolean);
        break;
      case InlineExpressionAst inline: NestedBlock(inline.Body); Emit(HixOpcode.LoadLocal, S(inline.ResultLocal)); break;
      case SelectionExpressionAst selection:
        if (selection.Selector == null) Constant(BooleanMixinValue.True); else Value(selection.Selector);
        Emit(HixOpcode.PushSelector);
        var ends = new List<int>();
        foreach (var branch in selection.Branches) {
          var next = new List<int>();
          foreach (var condition in branch.Conditions) {
            Value(condition);
            if (!branch.IsTransformation && selection.Selector != null) Emit(HixOpcode.MatchSelector);
            next.Add(Emit(HixOpcode.JumpFalse));
          }
          Result(branch.Result); ends.Add(Emit(HixOpcode.Jump));
          foreach (var jump in next) Patch(jump, code.Count);
        }
        Result(selection.Fallback);
        foreach (var jump in ends) Patch(jump, code.Count);
        Emit(HixOpcode.PopSelector); break;
      default: throw new ArgumentException("unknown expression '" + expression.GetType().Name + "' at line " + line);
    }
    if (check) { Emit(HixOpcode.EndCheck); Patch(handler, code.Count); }
  }
  private void CollectTargets(HixAst node) {
    if (node is CallExpressionAst {Name: "inject"} call && call.Arguments.Count >= 2) {
      if (call.Arguments[0] is StringExpressionAst target) targets.Add(new(target.Value, false));
      else if (call.Arguments[0] is MemberExpressionAst {Receiver: RootExpressionAst {Name: "carry"}, Member: var carry})
        targets.Add(new(carry, true));
    }
    foreach (var child in node.SemanticChildren) CollectTargets(child);
  }

  private void Result(HixAst result) {
    if (result is ExpressionAst expression) Value(expression);
    else { if (result is StatementAst statement) Statement(statement); Constant(NullMixinValue.Instance); }
  }
}
