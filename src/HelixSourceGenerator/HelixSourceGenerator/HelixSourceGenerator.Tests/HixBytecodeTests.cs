using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Mixins.Diagnostics;
using System.Linq;
using System.Reflection;
using Mixins;
using Mixins.Compiler;
using Mixins.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixBytecodeTests {
  private sealed class Context() : Mixins.Runtime.ExecutionContext(new MixinStringPoolBuilder().Freeze()) {
    protected override IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member) => NullMixinValue.Instance;
  }

  [Fact]
  public void LiteralLoadsAndPackingUseDedicatedOpcodesWithoutPoolEntries() {
    var program = HixCompiler.Compile("""
      pure func pair { return(true, false) }
      mixin Example { expression {
        local a = null
        local b = true
        local c = false
        local d = @[]
        local e = @{}
        local f = @[true, false]
        local g = @{a=null}
        emit(<[local#b]:[length(local#d)]:[length(local#e)]>)
        emit(join(pair(), <,>))
      } }
      """, "Example");
    Assert.Empty(program.ConstantPool);
    var instructions = HixInstruction.ReadAll(program.Bytecode).Select(item => item.Instruction).ToArray();
    foreach (var opcode in new[] {HixOpcode.LoadNull, HixOpcode.LoadTrue, HixOpcode.LoadFalse, HixOpcode.LoadTuple, HixOpcode.LoadTable}) {
      Assert.Contains(instructions, instruction => instruction.Opcode == opcode);
      Assert.Equal(1, new HixInstruction(opcode).Size);
    }
    foreach (var opcode in new[] {HixOpcode.PackTuple, HixOpcode.PackTable, HixOpcode.Pack, HixOpcode.CastString})
      Assert.Contains(instructions, instruction => instruction.Opcode == opcode);
    var result = MixinVirtualMachine.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"true:0:0", "true,false"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void SmartReferencesCompileToExplicitRootsAndMemberSelection() {
    var program = HixCompiler.Compile("""
      pure func read { local value = [$0]; return(<[$value]:[$it]>) }
      mixin Example { expression { emit(read(<argument>)) } }
      """, "Example");
    var roots = HixInstruction.ReadAll(program.Bytecode).Where(item => item.Instruction.Opcode == HixOpcode.LoadRoot)
      .Select(item => program.StringPool[item.Instruction.A]).ToArray();
    Assert.Contains("args", roots);
    Assert.Contains("param", roots);
    Assert.Contains("local", roots);
    Assert.DoesNotContain(Enum.GetNames(typeof(HixOpcode)), name => name.Contains("Smart") || name.StartsWith("Host"));
    var result = MixinVirtualMachine.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal("argument:argument", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void HostMembersUseLoadRootAndMemberInstructions() {
    var program = HixCompiler.Compile("""
      mixin Example { prelude expression { emit(this#name); emit(target#name); emit(attr#name) } }
      """, "Example");
    var instructions = HixInstruction.ReadAll(program.Bytecode).Select(item => item.Instruction).ToArray();
    Assert.Equal(new[] {"this", "target", "attr"}, instructions.Where(item => item.Opcode == HixOpcode.LoadRoot)
      .Select(item => program.StringPool[item.A]));
    Assert.Equal(3, instructions.Count(item => item.Opcode == HixOpcode.Member));
    var reads = 0;
    var result = MixinVirtualMachine.Execute(program, new PoolContext(_ => reads++));
    Assert.True(result.Success, result.Error);
    Assert.Equal(3, reads);
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public void DebugTracePrintsGlobalPoolsOnceAndUsesTheirIndices(bool internStrings) {
    var first = HixCompiler.Compile("mixin Example { expression { emit(<shared>); emit(42) } }", "Example");
    var second = HixCompiler.Compile("mixin Example { expression { emit(<different>); emit(<shared>); emit(7); emit(42) } }", "Example");
    var seed = new MixinStringPoolBuilder().Freeze();
    var vm = new MixinVirtualMachine(new[] {first, second}, seed);
    MixinDebugExpression Work(MixinExpressionExecutionProgram program, string name) => new(
      program, program, ImmutableDictionary<string, object>.Empty, ImmutableDictionary<string, object>.Empty,
      name, "", "", 0, 0);
    var trace = MixinDebugRenderer.BuildTrace(new MixinDebugRenderData(seed,
      ImmutableArray.Create(Work(first, "first"), Work(second, "second")), internStrings, 0, default, default, default));
    Assert.Equal(1, Regex.Matches(trace, "// GLOBAL POOLS").Count);
    Assert.Equal(vm.StringPool.Count, Regex.Matches(trace, @"(?m)^//   \.string ").Count);
    Assert.Equal(vm.ConstantPool.Count, Regex.Matches(trace, @"(?m)^//   \.constant ").Count);
    var bodies = trace.Substring(trace.IndexOf("// EXPRESSION", StringComparison.Ordinal));
    Assert.DoesNotContain(".string ", bodies);
    Assert.DoesNotContain(".constant ", bodies);
    Assert.True(vm.StringPool.TryGetId("shared", out var shared));
    var loads = Regex.Matches(bodies, @"(?m)^//   [0-9A-F]+ +\| LOADSTRING s([0-9]+) +\| push\(""shared""\)");
    Assert.Equal(4, loads.Count);
    foreach (Match load in loads) Assert.Equal(shared.ToString(), load.Groups[1].Value);
    Assert.Equal(new[] {42d, 7d}, vm.ConstantPool.Cast<NumberMixinValue>().Select(value => value.Value));
    var constantLoads = Regex.Matches(bodies, @"(?m)^//   [0-9A-F]+ +\| LOADCONST c([0-9]+) +\|");
    Assert.Equal(new[] {"0", "0", "1", "0", "1", "0"},
      constantLoads.Cast<Match>().Select(load => load.Groups[1].Value));
    var pool = vm.ConstantPool;
    Assert.Equal(new[] {"shared", "42"}, vm.Run(first, new Context()).Outputs.Select(output => output.Text));
    Assert.Equal(new[] {"different", "shared", "7", "42"}, vm.Run(second, new Context()).Outputs.Select(output => output.Text));
    Assert.Same(pool, vm.ConstantPool);

  }

  [Fact]
  public void DisassemblyHasAlignedColumnsAndHeadersAtFunctionAndLabelAddresses() {
    var program = HixCompiler.Compile("""
      pure func named sig string -> string { return(param) }
      mixin Example { expression {
        local value = <hello>
        local selected = when <right> {
          <left> -> <no>
          <right> -> named(local#value)
          else -> <fallback>
        }
        emit(local#selected)
      } }
      """, "Example");
    var bytes = program.Bytecode.ToArray();
    var dump = program.Disassemble();
    Assert.Equal(bytes, program.Bytecode);
    var rows = dump.Split('\n').Where(line => Regex.IsMatch(line, @"^[0-9A-F]+ +\|")).ToArray();
    Assert.Equal(HixInstruction.ReadAll(program.Bytecode).Count(), rows.Length);
    var separators = rows[0].Select((value, index) => (value, index)).Where(item => item.value == '|').Select(item => item.index).ToArray();
    Assert.Equal(2, separators.Length);
    foreach (var row in rows) foreach (var index in separators) Assert.Equal('|', row[index]);
    Assert.Contains("PSEUDOCODE", dump);
    Assert.Matches(@"(?m)^\.function global::named string -> string \[pure\] @ 0x0000\n0000 +\| ENTER", dump);
    Assert.Matches(@"(?m)^\.entry late expression 1 @ 0x[0-9A-F]+", dump);
    Assert.Contains("push(named(pop()))", dump);
    Assert.Contains("local[\"value\"] = pop()", dump);
    Assert.Contains("push(\"hello\")", dump);
    Assert.Contains("if (!truthy(pop())) goto loc_", dump);
    var labels = Regex.Matches(dump, @"(?m)^(loc_[0-9A-F]+):$").Cast<Match>().Select(match => match.Groups[1].Value).ToArray();
    Assert.NotEmpty(labels);
    Assert.Equal(labels.Length, labels.Distinct().Count());
    var references = Regex.Matches(string.Join("\n", rows), @"\bloc_[0-9A-F]+\b");
    Assert.NotEmpty(references.Cast<Match>());
    foreach (Match reference in references) Assert.Contains(reference.Value, labels);
    foreach (var label in labels) {
      var address = label.Substring(4);
      var value = int.Parse(address, System.Globalization.NumberStyles.HexNumber);
      if (value < program.Bytecode.Count) Assert.Matches("(?m)^" + label + ":\n" + address + @" +\|", dump);
    }
  }

  [Fact]
  public void InstructionBudgetStopsBackwardsJumpsWithoutBookkeepingOpcodes() {
    var program = HixCompiler.Compile("mixin Example { expression {\n:again\ngoto again\n} }", "Example");
    var result = MixinVirtualMachine.Execute(program, new Context());
    Assert.False(result.Success);
    Assert.Contains("execution limit", result.Error);
    Assert.Equal(100000, result.ExecutedOperations);
  }

  [Theory]
  [InlineData("emit(var#failure)", false)]
  [InlineData("emit(var#items#0)", false)]
  [InlineData("local checked = [var#failure?]\nemit(catch(local#checked) ?: <caught>)", true)]
  [InlineData("local checked = [var#items#0?]\nemit(catch(local#checked) ?: <caught>)", true)]
  public void LoadsAndMemberReadsPropagateErrorsAtTheirProducer(string statements, bool handled) {
    var error = new ErrorMixinValue(MixinString.Dynamic("bad"));
    var variables = new Dictionary<string, object> {
      ["failure"] = error, ["items"] = new TupleMixinValue(new IMixinValue[] {error})
    };
    var program = HixCompiler.Compile("mixin Example { expression {\n" + statements + "\n} }", "Example");
    var result = MixinVirtualMachine.Execute(program, new Context(), variables);
    Assert.Equal(handled, result.Success);
    if (handled) Assert.Equal("caught", Assert.Single(result.Outputs).Text);
    else { Assert.Equal("bad", result.Error); Assert.Empty(result.Outputs); }
  }

  [Fact]
  public void ControlFlowAndCheckedFailuresDoNotThrowManagedExceptions() {
    var program = HixCompiler.Compile("""
      pure func returned { return(<returned>) }
      pure func broken { goto missing }
      mixin Example { expression {
        local i = 0
        {
          local i = plus(local#i, 1)
          match(eq(local#i, 1))
          continue
        }
        { break }
        emit(returned())
        local handled = [broken()?]
        emit(catch(local#handled) ?: <caught>)
        local invalid = [emit(@[<bad>])?]
        emit(catch(local#invalid) ?: <collection>)
        emit(local#i)
      } }
      """, "Example");
    var exceptions = new List<Exception>();
    var thread = System.Environment.CurrentManagedThreadId;
    EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs> observe = (_, args) => {
      if (System.Environment.CurrentManagedThreadId == thread) exceptions.Add(args.Exception);
    };
    MixinExpressionResult result;
    AppDomain.CurrentDomain.FirstChanceException += observe;
    try { result = MixinVirtualMachine.Execute(program, new Context()); }
    finally { AppDomain.CurrentDomain.FirstChanceException -= observe; }
    Assert.True(result.Success, result.Error);
    Assert.Empty(exceptions);
    Assert.Equal(new[] {"returned", "caught", "collection", "2"}, result.Outputs.Select(output => output.Text));
  }

  private sealed class PoolContext(Action<MixinStringPool> observe) : Mixins.Runtime.ExecutionContext(new MixinStringPoolBuilder().Freeze()) {
    protected override IMixinValue ResolveHost(MixinExpressionRoot root, MixinString member) {
      Assert.Equal("", member.Resolve(Strings));
      observe(Strings); return MixinTableValue.Empty;
    }
  }

  [Fact]
  public void AllProgramsAndCallbacksKeepOneVmGlobalStringPool() {
    var first = HixCompiler.Compile("""
      func origin { return(this#name) }
      mixin Example { prelude expression { var callback = [origin] } }
      """, "Example");
    var second = HixCompiler.Compile("""
      mixin Example { prelude expression { emit(call(var#callback)); emit(this#name) } }
      """, "Example");
    var vm = new MixinVirtualMachine(new[] {first, second});
    var pool = vm.StringPool;
    var count = pool.Count;
    var observations = 0;
    var context = new PoolContext(actual => { Assert.Same(pool, actual); observations++; });
    var variables = new Dictionary<string, object>();
    Assert.True(vm.Run(first, context, variables).Success);
    Assert.Same(pool, context.Strings);
    var result = vm.Run(second, context, variables);
    Assert.True(result.Success, result.Error);
    Assert.Equal(2, observations);
    Assert.Same(pool, context.Strings);
    Assert.Equal(count, pool.Count);
  }

  [Fact]
  public void InstructionsUseOnlyTheirOpcodeSpecificLittleEndianOperands() {
    Assert.Equal(typeof(byte), Enum.GetUnderlyingType(typeof(HixOpcode)));
    var instructions = new[] {new HixInstruction(HixOpcode.Pop), new(HixOpcode.LoadString, 0xABCD),
      new(HixOpcode.Call, 0x1234, 0x5678), new(HixOpcode.Jump, -9)};
    var bytes = new byte[instructions.Sum(instruction => instruction.Size)];
    var offset = 0;
    foreach (var instruction in instructions) { instruction.Encode(bytes, offset); offset += instruction.Size; }
    Assert.Equal(new byte[] {(byte)HixOpcode.Pop, (byte)HixOpcode.LoadString, 0xCD, 0xAB,
      (byte)HixOpcode.Call, 0x34, 0x12, 0x78, 0x56, (byte)HixOpcode.Jump, 0xF7, 0xFF}, bytes);
    Assert.Equal(new[] {0, 1, 4, 9}, HixInstruction.ReadAll(bytes).Select(item => item.Offset));
    Assert.Equal(instructions, HixInstruction.ReadAll(bytes).Select(item => item.Instruction));
  }

  [Theory]
  [InlineData(HixOpcode.Jump, -32768)]
  [InlineData(HixOpcode.Jump, 32767)]
  [InlineData(HixOpcode.LoadConst, 0)]
  [InlineData(HixOpcode.LoadConst, 65535)]
  public void OperandBoundariesRoundTrip(HixOpcode opcode, int operand) {
    var instruction = new HixInstruction(opcode, operand);
    var bytes = new byte[instruction.Size];
    instruction.Encode(bytes, 0);
    Assert.Equal(instruction, HixInstruction.Decode(bytes, 0));
  }

  [Fact]
  public void InvalidOperandsAndTruncatedInstructionsAreRejected() {
    foreach (var instruction in new[] {new HixInstruction(HixOpcode.Jump, -32769), new(HixOpcode.Jump, 32768),
      new(HixOpcode.LoadString, -1), new(HixOpcode.LoadString, 65536), new(HixOpcode.Call, 0, 65536),
      new(HixOpcode.Pop, 1), new(HixOpcode.LoadConst, 0, 1)})
      Assert.Throws<ArgumentException>(() => instruction.Encode(new byte[5], 0));
    Assert.Throws<ArgumentException>(() => HixInstruction.Decode(new byte[] {(byte)HixOpcode.Call, 0, 0}, 0));
    Assert.Throws<ArgumentException>(() => HixInstruction.Decode(new byte[] {255}, 0));
  }

  [Fact]
  public void OversizedRelativeBlockIsRejectedWithoutTruncation() {
    var source = "mixin Example { expression {" + string.Concat(Enumerable.Repeat("emit(0);", 4000)) + "} }";
    Assert.Contains("s16", Assert.Throws<ArgumentException>(() => HixCompiler.Compile(source, "Example")).Message);
  }

  [Fact]
  public void GlobalStringPoolAllowsU16CapacityAndRejectsOverflowDuringLoading() {
    var builder = new MixinStringPoolBuilder();
    for (var i = 0; i < 65536; i++) builder.Intern("seed" + i);
    var pool = builder.Freeze();
    Assert.Equal(65536, new MixinVirtualMachine(Array.Empty<MixinExpressionExecutionProgram>(), pool).StringPool.Count);
    var program = HixCompiler.Compile("mixin Example { expression { emit(<new>) } }", "Example");
    Assert.Contains("u16", Assert.Throws<ArgumentException>(() => new MixinVirtualMachine(new[] {program}, pool)).Message);
  }

  [Fact]
  public void ConstantsAreDeduplicatedAndStringsUseTheirOwnPool() {
    var program = HixCompiler.Compile("""
      mixin Example { expression { emit(42); emit(42); emit(<repeated>); emit(<repeated>); emit(true); emit(null) } }
      """, "Example");
    Assert.Equal(HixInstruction.ReadAll(program.Bytecode).Select(item => item.Offset), program.SourceLines.Keys);
    Assert.Single(program.ConstantPool.OfType<NumberMixinValue>());
    Assert.DoesNotContain(program.ConstantPool, value => value.Kind == MixinValueKind.String);
    Assert.True(program.StringPool.TryGetId("repeated", out var stringIndex));
    var instructions = HixInstruction.ReadAll(program.Bytecode).Select(item => item.Instruction).ToArray();
    Assert.Equal(2, instructions.Count(instruction => instruction.Opcode == HixOpcode.LoadString && instruction.A == stringIndex));
    var stringCount = program.StringPool.Count;
    var result = MixinVirtualMachine.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"42", "42", "repeated", "repeated", "true", ""}, result.Outputs.Select(output => output.Text));
    Assert.Equal(stringCount, program.StringPool.Count);
    Assert.Matches(@"(?m)^0000 +\| ENTER loc_[0-9A-F]+ +\| begin block", program.Disassemble());
    Assert.DoesNotContain("TICK", program.Disassemble());
    Assert.DoesNotContain("VALIDATE", program.Disassemble());
    Assert.Equal(HixInstruction.ReadAll(program.Bytecode).Count(), result.ExecutedOperations);
  }

  [Fact]
  public void ExecutableObjectGraphContainsNeitherSyntaxNorExecutableDelegates() {
    var program = HixCompiler.Compile("""
      pure func identity sig @{value=string} -> string { return($value) }
      derivation mixin Decorator { expression { return(param#value) } }
      mixin Example { expression { emit(identity(<compiled>)) } }
      """, "Example");
    var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
    void Visit(object value) {
      if (value == null || value is string || !seen.Add(value)) return;
      Assert.False(value is HixAst, "Executable retains " + value.GetType().Name);
      Assert.False(value is Delegate, "Executable retains a delegate");
      if (value is IEnumerable sequence) foreach (var item in sequence) Visit(item);
      var type = value.GetType();
      if (type.Assembly != typeof(HixCompiler).Assembly && !type.IsGenericType) return;
      foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) Visit(field.GetValue(value));
    }
    Visit(program);
    Assert.All(typeof(MixinVirtualMachine).GetMethods(BindingFlags.Public | BindingFlags.Static), method =>
      Assert.DoesNotContain(method.GetParameters(), parameter => parameter.ParameterType == typeof(string) || typeof(HixAst).IsAssignableFrom(parameter.ParameterType)));
  }

  [Fact]
  public void SharedFunctionValuesExecuteTheirOriginalBytecodeAndPools() {
    var variables = new Dictionary<string, object>();
    var first = HixCompiler.Compile("""
      pure func decorate { return(<origin:[param]:[length(param)]>) }
      mixin Example { expression { var callback = [decorate] } }
      """, "Example");
    var second = HixCompiler.Compile("""
      pure func distract { return(<different layout>) }
      mixin Example { expression { emit(call(var#callback, <value>)); emit(<caller>) } }
      """, "Example");
    Assert.True(MixinVirtualMachine.Execute(first, new Context(), variables).Success);
    var result = MixinVirtualMachine.Execute(second, new Context(), variables);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"origin:value:5", "caller"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void BranchesUseSignedRelativeByteOffsetsAndRestoreStacksAndSelectors() {
    var program = HixCompiler.Compile("""
      mixin Example { expression {
        local i = 0
        :again
        local i = plus($i, 1)
        when(eq($i, 1)) { goto again }
        emit(<value:[$i]>)
        local checked = [error<bad>?]
        emit(catch(local#checked) ?: <caught>)
        local chosen = when <right> {
          <wrong> -> <no>
          <right> -> <yes>
        }
        emit(local#chosen)
      } }
      """, "Example");
    var instructions = HixInstruction.ReadAll(program.Bytecode).ToArray();
    var boundaries = instructions.Select(item => item.Offset).Append(program.Bytecode.Count).ToHashSet();
    foreach (var (pc, instruction) in instructions.Where(item => item.Instruction.IsRelative)) {
      Assert.Contains(pc + instruction.A, boundaries);
      Assert.InRange(instruction.A, short.MinValue, short.MaxValue);
    }
    Assert.Contains(instructions, item => item.Instruction.IsRelative && item.Instruction.A < 0);
    Assert.Contains(instructions, item => item.Instruction.IsRelative && item.Instruction.A > 0);
    var result = MixinVirtualMachine.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"value:2", "caught", "yes"}, result.Outputs.Select(output => output.Text));
  }
}
