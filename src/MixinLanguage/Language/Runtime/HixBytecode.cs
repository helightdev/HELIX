using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Mixins.Runtime;

/// <summary>Byte-aligned opcodes. Operands are little-endian; all s16 references are relative to the opcode address.</summary>
public enum HixOpcode : byte {
  /// <summary>s16 end; begin a block whose exclusive end is opcode address + displacement.</summary>
  Enter = 0,
  /// <summary>No operands; finish the current block.</summary>
  End = 1,
  /// <summary>u16 constant pool index; push the non-string constant.</summary>
  Constant = 2,
  /// <summary>u16 string pool index; push the string.</summary>
  String = 3,
  /// <summary>u16 string pool name; resolve and push a root.</summary>
  Root = 4,
  /// <summary>u16 string pool name; resolve and push a smart local root.</summary>
  SmartRoot = 5,
  /// <summary>u16 string pool member; push a member of the this host.</summary>
  HostThis = 6,
  /// <summary>u16 string pool member; push a member of the target host.</summary>
  HostTarget = 7,
  /// <summary>u16 string pool member; push an attribute host member.</summary>
  HostAttribute = 8,
  /// <summary>u16 string pool name; push a local or top-level carried value.</summary>
  LoadLocal = 9,
  /// <summary>u16 string pool name; push a shared variable.</summary>
  LoadVariable = 10,
  /// <summary>u16 string pool name; push a target variable.</summary>
  LoadTarget = 11,
  /// <summary>u16 string pool member; pop a receiver and push its member.</summary>
  Member = 12,
  /// <summary>u16 string pool name; pop into a local (or existing top-level carry).</summary>
  StoreLocal = 13,
  /// <summary>u16 string pool name; pop into a carried local; requires top-level prelude.</summary>
  StoreCarry = 14,
  /// <summary>u16 string pool name; pop into a shared variable.</summary>
  StoreVariable = 15,
  /// <summary>u16 string pool name; pop into a target variable.</summary>
  StoreTarget = 16,
  /// <summary>No operands; discard the top value.</summary>
  Pop = 17,
  /// <summary>u16 count; pop values in source order and push a tuple.</summary>
  Tuple = 18,
  /// <summary>u16 pair count; pop key/value pairs and push a table.</summary>
  Table = 19,
  /// <summary>u16 count; pop rendered text parts and push their concatenation.</summary>
  Interpolate = 20,
  /// <summary>u16 string pool function name, u16 argument count; pop arguments in source order and push the result.</summary>
  Call = 21,
  /// <summary>No operands; pop a value and push its truthiness, preserving checked errors.</summary>
  Boolean = 22,
  /// <summary>No operands; pop a value and push its negated truthiness.</summary>
  Not = 23,
  /// <summary>s16 handler; save stack/selector state; on error restore it, push a checked error and branch.</summary>
  Check = 24,
  /// <summary>No operands; remove the current checked-expression handler.</summary>
  EndCheck = 25,
  /// <summary>s16 target; branch unconditionally.</summary>
  Jump = 26,
  /// <summary>s16 target; branch if the top value is non-null, without popping it.</summary>
  JumpNotNull = 27,
  /// <summary>s16 target; pop a condition and branch if false.</summary>
  JumpFalse = 28,
  /// <summary>No operands; save the selector and replace it with the popped value.</summary>
  PushSelector = 29,
  /// <summary>No operands; restore the saved selector.</summary>
  PopSelector = 30,
  /// <summary>No operands; pop a value and push whether it equals the selector.</summary>
  MatchSelector = 31,
  /// <summary>s16 block; execute the referenced block and propagate its control result.</summary>
  Block = 32,
  /// <summary>No operands; pop the return value and leave the function/expression.</summary>
  Return = 33,
  /// <summary>s16 target; clear block state and transfer control, propagating through enclosing blocks as needed.</summary>
  Goto = 34,
  /// <summary>No operands; propagate an unresolved-goto error through the normal control-result path.</summary>
  InvalidGoto = 35,
  /// <summary>No operands; finish the current block.</summary>
  Break = 36,
  /// <summary>No operands; clear block state and restart after its Enter instruction.</summary>
  Continue = 37,
  /// <summary>u16 count; pop values and push null for zero, the value for one, or a tuple for multiple.</summary>
  Pack = 38,
  /// <summary>u16 string pool message; produce an unchecked runtime error.</summary>
  Error = 39,
  /// <summary>No operands; check shared-variable write permission before evaluating the assigned value.</summary>
  CheckStoreVariable = 40,
  /// <summary>No operands; check target-variable write permission before evaluating the assigned value.</summary>
  CheckStoreTarget = 41,
  /// <summary>No operands; check carry write permission and top-level prelude context before evaluation.</summary>
  CheckStoreCarry = 42,
  /// <summary>No operands; pop a value and push its rendered text, propagating rendering errors.</summary>
  Text = 43,
}

/// <summary>A decoded instruction. Its encoded size is one opcode byte plus only the operands that opcode uses.</summary>
public readonly record struct HixInstruction(HixOpcode Opcode, int A = 0, int B = 0) {
  public bool IsRelative => Opcode is HixOpcode.Enter or HixOpcode.Check or HixOpcode.Jump or
    HixOpcode.JumpNotNull or HixOpcode.JumpFalse or HixOpcode.Block or HixOpcode.Goto;
  public bool UsesStringPool => Opcode is HixOpcode.String or HixOpcode.Root or HixOpcode.SmartRoot or
    HixOpcode.HostThis or HixOpcode.HostTarget or HixOpcode.HostAttribute or HixOpcode.LoadLocal or
    HixOpcode.LoadVariable or HixOpcode.LoadTarget or HixOpcode.Member or HixOpcode.StoreLocal or
    HixOpcode.StoreCarry or HixOpcode.StoreVariable or HixOpcode.StoreTarget or HixOpcode.Call or HixOpcode.Error;
  public int Size => Opcode switch {
    HixOpcode.Call => 5,
    _ when IsRelative || UsesStringPool || Opcode is HixOpcode.Constant or HixOpcode.Tuple or HixOpcode.Table or
      HixOpcode.Interpolate or HixOpcode.Pack => 3,
    HixOpcode.End or HixOpcode.Pop or HixOpcode.Boolean or HixOpcode.Not or HixOpcode.EndCheck or
      HixOpcode.PushSelector or HixOpcode.PopSelector or HixOpcode.MatchSelector or HixOpcode.Return or
      HixOpcode.InvalidGoto or HixOpcode.Break or HixOpcode.Continue or HixOpcode.CheckStoreVariable or
      HixOpcode.CheckStoreTarget or HixOpcode.CheckStoreCarry or HixOpcode.Text => 1,
    _ => throw new ArgumentException("Unknown opcode " + Opcode)
  };
  public void Encode(byte[] bytes, int offset) {
    var size = Size;
    if (offset < 0 || offset > bytes.Length - size) throw new ArgumentException("Truncated instruction buffer");
    if (size == 1 && A != 0 || size != 5 && B != 0) throw new ArgumentException("Unexpected operand for " + Opcode);
    if (size > 1 && (IsRelative ? A < short.MinValue || A > short.MaxValue : A < 0 || A > ushort.MaxValue))
      throw new ArgumentException(Opcode + " operand exceeds " + (IsRelative ? "s16" : "u16") + " range");
    if (size == 5 && (B < 0 || B > ushort.MaxValue)) throw new ArgumentException("Call argument count exceeds u16 range");
    bytes[offset] = (byte)Opcode;
    if (size > 1) Write(bytes, offset + 1, A);
    if (size == 5) Write(bytes, offset + 3, B);
  }
  public static HixInstruction Decode(IReadOnlyList<byte> bytes, int offset) {
    if (offset < 0 || offset >= bytes.Count) throw new ArgumentException("Missing opcode");
    var instruction = new HixInstruction((HixOpcode)bytes[offset]);
    var size = instruction.Size;
    if (offset > bytes.Count - size) throw new ArgumentException("Truncated " + instruction.Opcode + " instruction");
    var a = size > 1 ? Read(bytes, offset + 1) : 0;
    return instruction with { A = instruction.IsRelative ? unchecked((short)a) : a, B = size == 5 ? Read(bytes, offset + 3) : 0 };
  }
  public static IEnumerable<(int Offset, HixInstruction Instruction)> ReadAll(IReadOnlyList<byte> bytes) {
    for (var offset = 0; offset < bytes.Count;) {
      var instruction = Decode(bytes, offset);
      yield return (offset, instruction);
      offset += instruction.Size;
    }
  }
  private static int Read(IReadOnlyList<byte> bytes, int offset) => bytes[offset] | bytes[offset + 1] << 8;
  private static void Write(byte[] bytes, int offset, int value) {
    bytes[offset] = (byte)value; bytes[offset + 1] = (byte)(value >> 8);
  }
}

internal enum BytecodeFlow { Normal, Return, Goto, Break, Continue, Error }
internal readonly record struct VmCompletion(BytecodeFlow Kind = BytecodeFlow.Normal, IMixinValue Value = null, int Target = -1, int Line = 0);
internal sealed record BytecodeField(string Name, string Kind, bool Variadic);
internal sealed record BytecodeSignature(string InputKind, IReadOnlyList<BytecodeField> Inputs,
  string OutputKind, IReadOnlyList<BytecodeField> Outputs);
internal sealed record BytecodeFunction(string Name, bool IsPure, IReadOnlyList<BytecodeSignature> Signatures,
  int Body);
internal sealed record BytecodeExpression(int Body, bool IsPrelude, int Line);
internal sealed record BytecodeDerivation(string Name, int Line, IReadOnlyList<BytecodeExpression> Expressions,
  LanguageFunctionScope Scope);
internal sealed record BytecodeTarget(string Value, bool IsCarry);

/// <summary>Immutable compiled image: instructions, constant pools and syntax-free entry-point metadata.</summary>
public sealed class MixinExpressionExecutionProgram {
  public IReadOnlyList<byte> Bytecode { get; }
  public IReadOnlyDictionary<int, int> SourceLines { get; }
  public IReadOnlyList<IMixinValue> ConstantPool { get; }
  public MixinStringPool StringPool { get; }
  internal IReadOnlyList<BytecodeExpression> Expressions { get; }
  internal IReadOnlyList<BytecodeDerivation> Derivations { get; }
  internal LanguageFunctionScope Scope { get; }
  internal IReadOnlyList<BytecodeTarget> LateTargets { get; }
  internal MixinExpressionExecutionProgram(byte[] code, Dictionary<int, int> sourceLines, IMixinValue[] constants, MixinStringPool strings,
    IReadOnlyList<BytecodeExpression> expressions, IReadOnlyList<BytecodeDerivation> derivations,
    LanguageFunctionScope scope, IReadOnlyList<BytecodeTarget> targets) {
    Bytecode = Array.AsReadOnly((byte[])code.Clone());
    SourceLines = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>(sourceLines));
    ConstantPool = Array.AsReadOnly((IMixinValue[])constants.Clone());
    StringPool = strings;
    Expressions = expressions;
    Derivations = derivations;
    Scope = scope;
    LateTargets = targets;
    scope.Attach(this);
    foreach (var derivation in derivations) derivation.Scope.Attach(this);
  }
  private MixinExpressionExecutionProgram(MixinExpressionExecutionProgram image, bool prelude) {
    Bytecode = image.Bytecode; SourceLines = image.SourceLines; ConstantPool = image.ConstantPool; StringPool = image.StringPool;
    Expressions = image.Expressions.Where(entry => entry.IsPrelude == prelude).ToArray();
    Derivations = image.Derivations; Scope = image.Scope; LateTargets = image.LateTargets;
  }
  internal MixinExpressionExecutionProgram ForPass(bool prelude) => new(this, prelude);
  internal string ExecutableIr => Disassemble();
  public string Disassemble() => Disassemble(Bytecode, StringPool, ConstantPool, true);
  internal string Disassemble(IReadOnlyList<byte> bytecode, MixinStringPool strings, IReadOnlyList<IMixinValue> constants, bool includePools = false) =>
    HixDisassembler.Render(this, bytecode, strings, constants, includePools);
  internal static string DisassemblePools(MixinStringPool strings, IReadOnlyList<IMixinValue> constants) {
    var text = new StringBuilder();
    for (var i = 0; i < constants.Count; i++)
      text.Append(".constant ").Append(i).Append(' ').Append(constants[i].Kind).Append(' ')
        .AppendLine(constants[i] switch {
          NumberMixinValue number => number.Value.ToString("R", CultureInfo.InvariantCulture),
          BooleanMixinValue boolean => boolean == BooleanMixinValue.True ? "true" : "false",
          NullMixinValue => "null", _ => constants[i].ToString()
        });
    for (var i = 0; i < strings.Count; i++)
      text.Append(".string ").Append(i).Append(" \"").Append(Escape(strings[i])).AppendLine("\"");
    return text.ToString();
  }
  internal void Fingerprint(MixinFingerprintBuilder builder) {
    foreach (var value in Bytecode) builder.Append((int)value);
    for (var i = 0; i < StringPool.Count; i++) builder.Append(StringPool[i]);
    foreach (var value in ConstantPool) {
      builder.Append((int)value.Kind);
      if (value is NumberMixinValue number) builder.Append(number.Value);
      else builder.Append(value.ToString());
    }
  }
  private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"")
    .Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0");
}
