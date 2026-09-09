using Hix.Env;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Hix.Runtime;

/// <summary>Formats executable instructions only; pseudocode describes stack effects without interpreting syntax.</summary>
internal static class HixDisassembler {
  internal static string Render(HixExpressionExecutionProgram program, IReadOnlyList<byte> code,
    HixStringPool strings, IReadOnlyList<IHixValue> constants, bool includePools) {
    using var profile = HixProfiler.Measure("bytecode.disassemble");
    var headers = new Dictionary<int, List<string>>();
    var labels = new HashSet<int>();
    void Header(int address, string title) {
      if (!headers.TryGetValue(address, out var titles)) headers.Add(address, titles = []);
      titles.Add(title);
    }
    foreach (var item in program.Scope.DisassemblyFunctions("mixin"))
      Header(item.Function.Body, FunctionHeader(item.Scope, item.Function));
    foreach (var derivation in program.Derivations) {
      foreach (var item in derivation.Scope.DisassemblyFunctions("derivation::" + derivation.Name, false))
        Header(item.Function.Body, FunctionHeader(item.Scope, item.Function));
      for (var i = 0; i < derivation.Expressions.Count; i++)
        Header(derivation.Expressions[i].Body, ".derivation " + derivation.Name + " expression " + (i + 1));
    }
    for (var i = 0; i < program.Expressions.Count; i++) {
      var entry = program.Expressions[i];
      Header(entry.Body, ".entry " + (entry.IsPrelude ? "prelude" : "late") + " expression " + (i + 1));
    }

    var rows = new List<(int Address, string Instruction, string Pseudocode)>();
    foreach (var (pc, instruction) in HixInstruction.ReadAll(code)) {
      if (instruction.IsRelative) labels.Add(pc + instruction.A);
      if (instruction.Opcode == HixOpcode.Enter && !headers.ContainsKey(pc))
        Header(pc, ".block block_" + Address(pc));
      var (operands, pseudocode) = Describe(instruction, pc, strings, constants);
      var operation = instruction.Opcode.ToString().ToUpperInvariant();
      var assembly = operation + (operands.Length == 0 ? "" : " " + operands);
      var line = program.SourceLines[pc];
      rows.Add((pc, assembly, pseudocode + (line > 0 ? "  // line " + Number(line) : "")));
    }
    var addressWidth = Math.Max("ADDRESS".Length, Address(code.Count).Length);
    var instructionWidth = Math.Max(32, rows.Count == 0 ? 0 : rows.Max(row => row.Instruction.Length));
    var text = new StringBuilder();
    if (includePools) text.Append(HixExpressionExecutionProgram.DisassemblePools(strings, constants)).AppendLine();
    text.Append("ADDRESS".PadRight(addressWidth)).Append(" | ").Append("INSTRUCTION".PadRight(instructionWidth))
      .AppendLine(" | PSEUDOCODE");
    void Boundary(int address) {
      var hasHeader = headers.TryGetValue(address, out var titles);
      if (!hasHeader && !labels.Contains(address)) return;
      text.AppendLine();
      if (hasHeader) foreach (var title in titles) text.Append(title).Append(" @ 0x").AppendLine(Address(address));
      if (labels.Contains(address)) text.Append(Label(address)).AppendLine(":");
    }
    foreach (var row in rows) {
      Boundary(row.Address);
      text.Append(Address(row.Address).PadRight(addressWidth)).Append(" | ")
        .Append(row.Instruction.PadRight(instructionWidth)).Append(" | ").AppendLine(row.Pseudocode);
    }
    Boundary(code.Count);
    return text.ToString();
  }

  private static string FunctionHeader(string scope, BytecodeFunction function) {
    string Fields(IReadOnlyList<BytecodeField> fields, string kind) => fields == null ? kind ?? "any"
      : "(" + string.Join(", ", fields.Select(field => (field.Variadic ? "..." : "") + field.Name + ": " + field.Kind)) + ")";
    var signatures = function.Signatures.Count == 0 ? "(*) -> any" : string.Join("; ", function.Signatures.Select(signature =>
      Fields(signature.Inputs, signature.InputKind) + " -> " + Fields(signature.Outputs, signature.OutputKind)));
    return ".function " + scope + "::" + function.Name + " " + signatures + (function.IsPure ? " [pure]" : " [impure]");
  }

  private static (string Operands, string Pseudocode) Describe(HixInstruction instruction, int address,
    HixStringPool strings, IReadOnlyList<IHixValue> constants) {
    var a = instruction.IsRelative ? address + instruction.A : instruction.A;
    var b = instruction.B;
    string Name() => strings[a];
    string StringId() => "s" + Number(a);
    string Storage() => instruction.Opcode switch {
      HixOpcode.StoreCarry or HixOpcode.CheckStoreCarry => "carry.local",
      HixOpcode.StoreVariable or HixOpcode.CheckStoreVariable => "var",
      HixOpcode.StoreTarget or HixOpcode.CheckStoreTarget => "target.var", _ => "local"
    };
    string Slot() => Storage() + "[" + Quote(Name()) + "]";
    string Args(int count) => count == 0 ? "" : count == 1 ? "pop()" : "pop_args(" + Number(count) + ")...";
    switch (instruction.Opcode) {
      case HixOpcode.Enter: return (Label(a), "begin block; end = " + Label(a));
      case HixOpcode.End: return ("", "end block");
      case HixOpcode.LoadConst: return ("c" + Number(a), "push(" + Constant(constants[a]) + ")");
      case HixOpcode.LoadString: return (StringId(), "push(" + Quote(Name()) + ")");
      case HixOpcode.Equal: return ("", "right = pop(); left = pop(); push(left == right)");
      case HixOpcode.LoadRoot: return (StringId(), "push(" + Name() + ")");
      case HixOpcode.LoadTrue: return ("", "push(true)");
      case HixOpcode.LoadFalse: return ("", "push(false)");
      case HixOpcode.LoadNull: return ("", "push(null)");
      case HixOpcode.LoadTuple: return ("", "push(tuple())");
      case HixOpcode.LoadTable: return ("", "push(table())");
      case HixOpcode.Member: return (StringId(), "push(pop()[" + Quote(Name()) + "])");
      case HixOpcode.CheckStoreCarry:
      case HixOpcode.CheckStoreVariable:
      case HixOpcode.CheckStoreTarget: return ("", "require_writable(" + Storage() + ")");
      case HixOpcode.StoreLocal:
      case HixOpcode.StoreCarry:
      case HixOpcode.StoreVariable:
      case HixOpcode.StoreTarget: return (StringId(), Slot() + " = pop()");
      case HixOpcode.Pop: return ("", "discard(pop())");
      case HixOpcode.PackTuple: return (Number(a), "push(tuple(" + Args(a) + "))");
      case HixOpcode.PackTable: return (Number(a), "push(table(pop_pairs(" + Number(a) + ")))");
      case HixOpcode.Pack: return (Number(a), a == 0 ? "push(null)" : a == 1 ? "keep top value" : "push(tuple(" + Args(a) + "))");
      case HixOpcode.Interpolate: return (Number(a), "push(concat(" + Args(a) + "))");
      case HixOpcode.Call: return (StringId() + ", argc=" + Number(b), "push(" + Name() + "(" + Args(b) + "))");
      case HixOpcode.CastBoolean: return ("", "push(bool_or_error(pop()))");
      case HixOpcode.Not: return ("", "push(!truthy(pop()))");
      case HixOpcode.CastString: return ("", "push(text(pop()))");
      case HixOpcode.Throw: return (StringId(), "fail(" + Quote(Name()) + ")");
      case HixOpcode.Check: return (Label(a), "begin checked; on error push(checked(error)), goto " + Label(a));
      case HixOpcode.EndCheck: return ("", "end checked");
      case HixOpcode.Jump: return (Label(a), "goto " + Label(a));
      case HixOpcode.JumpNotNull: return (Label(a), "if (peek() != null) goto " + Label(a));
      case HixOpcode.JumpFalse: return (Label(a), "if (!truthy(pop())) goto " + Label(a));
      case HixOpcode.Block: return (Label(a), "execute_block(" + Label(a) + ")");
      case HixOpcode.Return: return ("", "return pop()");
      case HixOpcode.Break: return ("", "break");
      case HixOpcode.Continue: return ("", "continue");
      case HixOpcode.Goto: return (Label(a), "goto " + Label(a));
      case HixOpcode.InvalidGoto: return ("", "fail(\"unresolved goto\")");
      default: throw new ArgumentException("Unknown opcode " + instruction.Opcode);
    }
  }

  private static string Constant(IHixValue value) => value switch {
    NumberHixValue number => number.Value.ToString("R", CultureInfo.InvariantCulture),
    BooleanHixValue boolean => boolean.Value ? "true" : "false",
    NullHixValue => "null",
    KindHixValue kind => kind.Name,
    _ => value.ToString()
  };
  private static string Number(int number) => number.ToString(CultureInfo.InvariantCulture);
  private static string Address(int address) => address.ToString("X4", CultureInfo.InvariantCulture);
  private static string Label(int address) => "loc_" + Address(address);
  private static string Quote(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"")
    .Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0") + "\"";
}
