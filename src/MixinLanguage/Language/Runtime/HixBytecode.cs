using Mixins.Env;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Mixins.Runtime;

/// <summary>Byte-aligned opcodes. Operands are little-endian; all s16 references are relative to the opcode address.</summary>
public enum HixOpcode : byte {
  // Loads and member access
  /// <summary>u16 constant pool index; push the non-string constant.</summary>
  LoadConst = 0,
  /// <summary>u16 string pool index; push the string.</summary>
  LoadString = 1,
  /// <summary>No operands; push null.</summary>
  LoadNull = 2,
  /// <summary>No operands; push true.</summary>
  LoadTrue = 3,
  /// <summary>No operands; push false.</summary>
  LoadFalse = 4,
  /// <summary>No operands; push the empty tuple.</summary>
  LoadTuple = 5,
  /// <summary>No operands; push the empty table.</summary>
  LoadTable = 6,
  /// <summary>u16 string pool name; resolve and push a root.</summary>
  LoadRoot = 7,
  /// <summary>u16 string pool member; pop a receiver and push its member.</summary>
  Member = 8,


  // Storage
  /// <summary>u16 string pool name; pop into a local (or existing top-level carry).</summary>
  StoreLocal = 9,
  /// <summary>u16 string pool name; pop into a carried local; requires top-level prelude.</summary>
  StoreCarry = 10,
  /// <summary>u16 string pool name; pop into a shared variable.</summary>
  StoreVariable = 11,
  /// <summary>u16 string pool name; pop into a target variable.</summary>
  StoreTarget = 12,
  /// <summary>No operands; check carry write permission and top-level prelude context before evaluation.</summary>
  CheckStoreCarry = 13,
  /// <summary>No operands; check shared-variable write permission before evaluating the assigned value.</summary>
  CheckStoreVariable = 14,
  /// <summary>No operands; check target-variable write permission before evaluating the assigned value.</summary>
  CheckStoreTarget = 15,


  // Stack and packing
  /// <summary>No operands; discard the top value.</summary>
  Pop = 16,
  /// <summary>u16 count; pop values and push null for zero, the value for one, or a tuple for multiple.</summary>
  Pack = 17,
  /// <summary>u16 count; pop values in source order and push a tuple.</summary>
  PackTuple = 18,
  /// <summary>u16 pair count; pop key/value pairs and push a table.</summary>
  PackTable = 19,

  // Conversions and value operations
  /// <summary>No operands; pop a value and push its truthiness, preserving checked errors.</summary>
  CastBoolean = 20,
  /// <summary>No operands; pop a value and push its rendered text, propagating rendering errors.</summary>
  CastString = 21,
  /// <summary>No operands; pop a value and push its negated truthiness.</summary>
  Not = 22,
  /// <summary>u16 count; pop rendered text parts and push their concatenation.</summary>
  Interpolate = 23,

  /// <summary>No operands; pop right and left values and push their equality.</summary>
  Equal = 24,

  // Calls and blocks
  /// <summary>u16 string pool function name, u16 argument count; pop arguments in source order and push the result.</summary>
  Call = 25,
  /// <summary>No operands; pop the return value and leave the function/expression.</summary>
  Return = 26,
  /// <summary>s16 end; begin a block whose exclusive end is opcode address + displacement.</summary>
  Enter = 27,
  /// <summary>No operands; finish the current block.</summary>
  End = 28,
  /// <summary>s16 block; execute the referenced block and propagate its control result.</summary>
  Block = 29,

  // Branches and loop control
  /// <summary>s16 target; branch unconditionally.</summary>
  Jump = 30,
  /// <summary>s16 target; pop a condition and branch if false.</summary>
  JumpFalse = 31,
  /// <summary>s16 target; branch if the top value is non-null, without popping it.</summary>
  JumpNotNull = 32,
  /// <summary>s16 target; clear block state and transfer control, propagating through enclosing blocks as needed.</summary>
  Goto = 33,
  /// <summary>No operands; propagate an unresolved-goto error through the normal control-result path.</summary>
  InvalidGoto = 34,
  /// <summary>No operands; finish the current block.</summary>
  Break = 35,
  /// <summary>No operands; clear block state and restart after its Enter instruction.</summary>
  Continue = 36,

  // Error handling
  /// <summary>s16 handler; save stack state; on error restore it, push a checked error and branch.</summary>
  Check = 37,
  /// <summary>No operands; remove the current checked-expression handler.</summary>
  EndCheck = 38,
  /// <summary>u16 string pool message; produce an unchecked runtime error.</summary>
  Throw = 39,
}

/// <summary>A decoded instruction. Its encoded size is one opcode byte plus only the operands that opcode uses.</summary>
public readonly record struct HixInstruction(HixOpcode Opcode, int A = 0, int B = 0) {
  public bool IsRelative => Opcode is HixOpcode.Enter or HixOpcode.Check or HixOpcode.Jump or
    HixOpcode.JumpNotNull or HixOpcode.JumpFalse or HixOpcode.Block or HixOpcode.Goto;
  public bool UsesStringPool => Opcode is HixOpcode.LoadString or HixOpcode.LoadRoot or HixOpcode.Member
    or HixOpcode.StoreLocal or
    HixOpcode.StoreCarry or HixOpcode.StoreVariable or HixOpcode.StoreTarget or HixOpcode.Call or HixOpcode.Throw;
  public int Size => Opcode switch {
    HixOpcode.Call => 5,
    _ when IsRelative || UsesStringPool || Opcode is HixOpcode.LoadConst or HixOpcode.PackTuple or HixOpcode.PackTable
      or
      HixOpcode.Interpolate or HixOpcode.Pack => 3,
    HixOpcode.End or HixOpcode.Pop or HixOpcode.CastBoolean or HixOpcode.Not or HixOpcode.EndCheck or
      HixOpcode.Equal or HixOpcode.Return or
      HixOpcode.InvalidGoto or HixOpcode.Break or HixOpcode.Continue or HixOpcode.CheckStoreVariable or
      HixOpcode.CheckStoreTarget or HixOpcode.CheckStoreCarry or HixOpcode.CastString or HixOpcode.LoadTrue
      or HixOpcode.LoadFalse or
      HixOpcode.LoadNull or HixOpcode.LoadTuple or HixOpcode.LoadTable => 1,
    _ => throw new ArgumentException("Unknown opcode " + Opcode)
  };

  public void Encode(byte[] bytes, int offset) {
    var size = Size;
    if (offset < 0 || offset > bytes.Length - size) throw new ArgumentException("Truncated instruction buffer");
    if (size == 1 && A != 0 || size != 5 && B != 0) throw new ArgumentException("Unexpected operand for " + Opcode);
    if (size > 1 && (IsRelative ? A < short.MinValue || A > short.MaxValue : A < 0 || A > ushort.MaxValue))
      throw new ArgumentException(Opcode + " operand exceeds " + (IsRelative ? "s16" : "u16") + " range");
    if (size == 5 && (B < 0 || B > ushort.MaxValue))
      throw new ArgumentException("Call argument count exceeds u16 range");
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
    return instruction with {
      A = instruction.IsRelative ? unchecked((short)a) : a, B = size == 5 ? Read(bytes, offset + 3) : 0
    };
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
    bytes[offset] = (byte)value;
    bytes[offset + 1] = (byte)(value >> 8);
  }
}

internal enum BytecodeFlow { Normal, Return, Goto, Break, Continue, Error }

internal readonly record struct VmCompletion(BytecodeFlow Kind = BytecodeFlow.Normal, IMixinValue Value = null,
  int Target = -1, int Line = 0
);

internal sealed record BytecodeField(string Name, string Kind, bool Variadic);

internal sealed record BytecodeSignature(string InputKind, IReadOnlyList<BytecodeField> Inputs,
  string OutputKind, IReadOnlyList<BytecodeField> Outputs
);

internal sealed record BytecodeFunction(string Name, bool IsPure, IReadOnlyList<BytecodeSignature> Signatures,
  int Body
);

internal sealed record BytecodeExpression(int Body, bool IsPrelude, int Line);

internal sealed record BytecodeDerivation(string Name, int Line, IReadOnlyList<BytecodeExpression> Expressions,
  LanguageFunctionScope Scope
);

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

  internal MixinExpressionExecutionProgram(
    byte[] code, Dictionary<int, int> sourceLines, IMixinValue[] constants, MixinStringPool strings,
    IReadOnlyList<BytecodeExpression> expressions, IReadOnlyList<BytecodeDerivation> derivations,
    LanguageFunctionScope scope, IReadOnlyList<BytecodeTarget> targets
  ) {
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
    Bytecode = image.Bytecode;
    SourceLines = image.SourceLines;
    ConstantPool = image.ConstantPool;
    StringPool = image.StringPool;
    Expressions = image.Expressions.Where(entry => entry.IsPrelude == prelude).ToArray();
    Derivations = image.Derivations;
    Scope = image.Scope;
    LateTargets = image.LateTargets;
  }

  internal MixinExpressionExecutionProgram ForPass(bool prelude) => new(this, prelude);
  private string identity;
  internal string Identity {
    get {
      var cached = Volatile.Read(ref identity);
      if (cached != null) return cached;
      var computed = ComputeIdentity();
      return Interlocked.CompareExchange(ref identity, computed, null) ?? computed;
    }
  }

  private string ComputeIdentity() {
    using var profile = MixinProfiler.Measure("bytecode.identity");
    var builder = new MixinFingerprintBuilder();
    Fingerprint(builder);
    foreach (var line in SourceLines.OrderBy(item => item.Key)) {
      builder.Append(line.Key);
      builder.Append(line.Value);
    }
    AppendScopeIdentity(builder, Scope);
    builder.Append(Expressions.Count);
    foreach (var entry in Expressions) {
      builder.Append(entry.Body);
      builder.Append(entry.IsPrelude);
      builder.Append(entry.Line);
    }
    builder.Append(Derivations.Count);
    foreach (var derivation in Derivations) {
      builder.Append(derivation.Name);
      builder.Append(derivation.Line);
      AppendScopeIdentity(builder, derivation.Scope);
      builder.Append(derivation.Expressions.Count);
      foreach (var entry in derivation.Expressions) {
        builder.Append(entry.Body);
        builder.Append(entry.IsPrelude);
        builder.Append(entry.Line);
      }
    }
    builder.Append(LateTargets.Count);
    foreach (var target in LateTargets) {
      builder.Append(target.Value);
      builder.Append(target.IsCarry);
    }
    return builder.Hash.ToString("X16", CultureInfo.InvariantCulture) + ":" +
      builder.Length.ToString(CultureInfo.InvariantCulture);
  }

  private static void AppendScopeIdentity(MixinFingerprintBuilder builder, LanguageFunctionScope scope) {
    foreach (var item in scope.DisassemblyFunctions("mixin")) {
      builder.Append(item.Scope);
      builder.Append(item.Function.Name);
      builder.Append(item.Function.Body);
      builder.Append(item.Function.IsPure);
      builder.Append(item.Function.Signatures.Count);
      foreach (var signature in item.Function.Signatures) {
        builder.Append(signature.InputKind);
        builder.Append(signature.OutputKind);
        AppendFieldsIdentity(builder, signature.Inputs);
        AppendFieldsIdentity(builder, signature.Outputs);
      }
    }
  }

  private static void AppendFieldsIdentity(MixinFingerprintBuilder builder, IReadOnlyList<BytecodeField> fields) {
    builder.Append(fields?.Count ?? -1);
    if (fields == null) return;
    foreach (var field in fields) {
      builder.Append(field.Name);
      builder.Append(field.Kind);
      builder.Append(field.Variadic);
    }
  }

  public string Disassemble() => Disassemble(Bytecode, StringPool, ConstantPool, true);

  internal string Disassemble(
    IReadOnlyList<byte> bytecode, MixinStringPool strings, IReadOnlyList<IMixinValue> constants,
    bool includePools = false
  ) => HixDisassembler.Render(this, bytecode, strings, constants, includePools);

  internal static string DisassemblePools(MixinStringPool strings, IReadOnlyList<IMixinValue> constants) {
    using var profile = MixinProfiler.Measure("bytecode.disassemble_pools");
    var text = new StringBuilder();
    for (var i = 0; i < constants.Count; i++)
      text.Append(".constant ").Append(i).Append(' ').Append(constants[i].Kind).Append(' ')
        .AppendLine(
          constants[i] switch {
            NumberMixinValue number => number.Value.ToString("R", CultureInfo.InvariantCulture),
            BooleanMixinValue boolean => boolean == BooleanMixinValue.True ? "true" : "false",
            NullMixinValue => "null", _ => constants[i].ToString()
          }
        );
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