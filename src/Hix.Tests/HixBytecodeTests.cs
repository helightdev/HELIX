using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixBytecodeTests {
  [Fact]
  public void LoweredSelectionsSupportGotoExitAndReentryWithoutTemporaryNameCollisions() {
    var program = TestCompiler.Compile("""
      pure func eq { return(false) }
      mixin Example { expression {
        local __selector_0 = <user>
        local i = 0
        :again
        local i = plus(local#i, 1)
        when [local#i] {
          1 -> { goto again }
          2 -> { goto done }
          else -> { emit(<wrong>) }
        }
        :done
        emit(local#__selector_0)
        emit(local#i)
        local selected = when <value> {
          <value> -> <matched>
          else -> <wrong equality>
        }
        emit(local#selected)
      } }
      """, "Example");
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"user", "2", "matched"}, result.Outputs.Select(output => output.Text));
    Assert.Contains(HixInstruction.ReadAll(program.Bytecode), item => item.Instruction.Opcode == HixOpcode.Equal);
  }

  [Fact]
  public void SelectionsLowerToOrdinaryLocalsAndJumps() {
    var program = TestCompiler.Compile("""
      mixin Example { expression {
        when <outer> {
          [:eq<outer>] -> {
            local selected = when <inner> {
              [:eq<inner>] -> <matched>
              else -> <wrong inner>
            }
            emit(local#selected)
          }
          else -> { emit(<wrong outer>) }
        }
      } }
      """, "Example");
    var instructions = HixInstruction.ReadAll(program.Bytecode).Select(item => item.Instruction).ToArray();
    var temporaries = instructions.Where(instruction => instruction.Opcode == HixOpcode.StoreLocal)
      .Select(instruction => program.StringPool[instruction.A]).Where(name => name.StartsWith("__selector_")).ToArray();
    Assert.Equal(2, temporaries.Distinct().Count());
    Assert.DoesNotContain(Enum.GetNames(typeof(HixOpcode)), name => name.Contains("Selector"));
    Assert.Contains("local[\"__selector_", program.Disassemble());
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal("matched", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void TupleUpdatesPreserveInputsAndTransformsHandleEmptyAndPartialResults() {
    var program = TestCompiler.Compile("""
      mixin Example { expression {
        local original = @[1, 2, 3]
        emit(join(push(local#original, 4), <,>))
        emit(join(pop(local#original), <,>))
        emit(join(local#original, <,>))
        emit(length(pop(@[])))
        emit(length(pop(@[1])))
        emit(join(map(local#original, func => $0), <,>))
        emit(join(map(local#original, func => plus($0, 1)), <,>))
        emit(join(where(local#original, func => eq($0, 2)), <,>))
        emit(join(where(local#original, func => true), <,>))
        emit(length(where(local#original, func => false)))
        emit(length(map(@[], func => error<unexpected>)))
        emit(any(@[true, false], func => $0))
        emit(all(@[true, false], func => $0))
        emit(any(@[], func => error<unexpected>))
        emit(all(@[], func => error<unexpected>))
        emit(reduce(local#original, func => plus($0, $1), 0))
      } }
      """, "Example");
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"1,2,3,4", "1,2", "1,2,3", "0", "0", "1,2,3", "2,3,4", "2", "1,2,3", "0", "0",
      "true", "false", "false", "true", "6"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void TupleTransformReusesUnchangedTupleAndCopiesOnlyOnChange() {
    var first = new NumberHixValue(1);
    var second = new NumberHixValue(2);
    var tuple = new TupleHixValue(new IHixValue[] {first, second});
    var method = typeof(TupleHixValue).GetMethod("Transform", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
    Func<IHixValue, IHixValue> unchanged = value => value;
    Assert.Same(tuple, method.Invoke(tuple, new object[] {unchanged}));
    var replacement = new NumberHixValue(3);
    Func<IHixValue, IHixValue> changed = value => ReferenceEquals(value, second) ? replacement : value;
    var result = (TupleHixValue)method.Invoke(tuple, new object[] {changed})!;
    Assert.Same(first, result.Values[0]);
    Assert.Same(replacement, result.Values[1]);
    Assert.Same(second, tuple.Values[1]);
  }

  [Fact]
  public void PersistentStorageSnapshotsShareRootsAndRestoreWithoutMutatingCaptures() {
    var type = typeof(HixVM).Assembly.GetType("Hix.HixValueDictionary", true)!;
    var storage = Activator.CreateInstance(type, true)!;
    var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    var store = type.GetMethod("StoreIsolated", flags)!;
    var snapshot = type.GetMethod("Snapshot", flags)!;
    for (var i = 0; i < 100; i++)
      store.Invoke(storage, new object[] {HixString.Dynamic("key" + i), new NumberHixValue(i)});
    var saved = snapshot.Invoke(storage, null)!;
    Assert.Same(saved, snapshot.Invoke(storage, null));
    store.Invoke(storage, new object[] {HixString.Dynamic("key0"), new NumberHixValue(999)});
    Assert.Equal(new NumberHixValue(0), ((IReadOnlyDictionary<HixString, IHixValue>)saved)[HixString.Dynamic("key0")]);
    type.GetMethod("Restore", flags)!.Invoke(storage, new[] {saved});
    Assert.Same(saved, snapshot.Invoke(storage, null));
  }

  [Fact]
  public void VmReusesClearedLocalsAfterNestedCallsAndFailures() {
    var program = TestCompiler.Compile("""
      pure func captured { local name = <kept>; return(local) }
      pure func failing { local garbage = <discard>; return(error<failed>) }
      mixin Example { expression {
        local saved = captured()
        local failure = [failing()?]
        emit(local#saved#name)
        emit(length(captured()))
      } }
      """, "Example");
    var vm = new HixVM(new[] {program});
    for (var i = 0; i < 3; i++) {
      var result = vm.Run(program, new Context());
      Assert.True(result.Success, result.Error);
      Assert.Equal(new[] {"kept", "1"}, result.Outputs.Select(output => output.Text));
    }
    var pool = (IEnumerable)typeof(HixVM).GetField("localPool", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(vm)!;
    var dictionaries = pool.Cast<IReadOnlyDictionary<HixString, IHixValue>>().ToArray();
    Assert.Equal(2, dictionaries.Length);
    Assert.All(dictionaries, dictionary => Assert.Empty(dictionary));
  }

  private sealed class Context() : Hix.Runtime.HixExecutionContext(TestBackend.Instance, new HixStringPoolBuilder().Freeze()) {
    protected override IHixValue ResolveHost(HixExpressionRoot root, HixString member) => NullHixValue.Instance;
  }

  private sealed class SharedPoolContext(HixStringPool strings) : Hix.Runtime.HixExecutionContext(TestBackend.Instance, strings) {
    protected override IHixValue ResolveHost(HixExpressionRoot root, HixString member) => NullHixValue.Instance;
  }

  [Fact]
  public void StaticExecutionReusesLoadedImagesWithoutSharingMutableState() {
    var program = TestCompiler.Compile("mixin Example { expression { var name = <new>; emit(var#name) } }", "Example");
    var seed = new HixStringPoolBuilder().Freeze();
    var first = new SharedPoolContext(seed);
    Assert.True(HixVM.Execute(program, first).Success);
    var pool = first.Strings;
    System.Threading.Tasks.Parallel.For(0, 8, _ => {
      var context = new SharedPoolContext(seed);
      var variables = new Dictionary<string,object> { ["private"] = new object() };
      var result = HixVM.Execute(program, context, variables);
      Assert.True(result.Success, result.Error);
      Assert.Same(pool, context.Strings);
      Assert.Equal("new", Assert.Single(result.Outputs).Text);
    });
    Assert.True(HixVM.Execute(program, first).Success);
    Assert.Same(pool, first.Strings);
  }

  [Fact]
  public void ProgramIdentityIsCachedStructuralDataRatherThanDisassembly() {
    var first = TestCompiler.Compile("mixin Example { expression { emit(1) } }", "Example");
    var same = TestCompiler.Compile("mixin Example { expression { emit(1) } }", "Example");
    var changed = TestCompiler.Compile("mixin Example { expression { emit(2) } }", "Example");
    var property = typeof(HixExpressionExecutionProgram).GetProperty("Identity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
    var identity = (string)property.GetValue(first)!;
    Assert.Matches("^[0-9A-F]{16}:[0-9]+$", identity);
    Assert.Same(identity, property.GetValue(first));
    Assert.Equal(identity, property.GetValue(same));
    Assert.NotEqual(identity, property.GetValue(changed));
    var forPass = typeof(HixExpressionExecutionProgram).GetMethod("ForPass", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
    Assert.NotEqual(property.GetValue(forPass.Invoke(first, new object[] {true})),
      property.GetValue(forPass.Invoke(first, new object[] {false})));

  }

  [Fact]
  public void TableEqualityAndFingerprintsIgnoreEntryOrderButTuplesRemainOrdered() {
    var context = new Context();
    var first = new KeyValuePair<HixString, IHixValue>(HixString.Dynamic("first"), new NumberHixValue(1));
    var second = new KeyValuePair<HixString, IHixValue>(HixString.Dynamic("second"), new NumberHixValue(2));
    var left = new HixTableValue(new[] {first, second});
    var right = new HixTableValue(new[] {second, first});
    Assert.True(left.Equals((IHixValue)right));
    Assert.True(left.Equals((object)right));
    Assert.Equal(left.GetHashCode(), right.GetHashCode());
    var leftHash = new HixFingerprintBuilder();
    var rightHash = new HixFingerprintBuilder();
    left.Fingerprint(leftHash, context);
    right.Fingerprint(rightHash, context);
    Assert.Equal(leftHash.Hash, rightHash.Hash);
    Assert.Equal(leftHash.Length, rightHash.Length);
    var changed = new HixTableValue(new[] {first, new KeyValuePair<HixString, IHixValue>(second.Key, new NumberHixValue(3))});
    Assert.False(left.Equals((IHixValue)changed));
    var changedHash = new HixFingerprintBuilder();
    changed.Fingerprint(changedHash, context);
    Assert.NotEqual(leftHash.Hash, changedHash.Hash);
    Assert.False(new TupleHixValue(new[] {first.Value, second.Value})
      .Equals((IHixValue)new TupleHixValue(new[] {second.Value, first.Value})));
  }

  [Fact]
  public void FunctionSelectionRechecksConversionsAndKeepsNamedExtras() {
    var program = TestCompiler.Compile("""
      pure func choose { return(<fallback>) }
      pure func choose sig number -> string { return(<number>) }
      pure func named sig @{value=number} -> string { return(<[param#value]:[param#extra]>) }
      mixin Example { expression {
        emit(choose(<12>))
        emit(choose(<not numeric>))
        emit(choose(<34>))
        emit(named(@{value=<7>, extra=<kept>}))
      } }
      """, "Example");
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"number", "fallback", "number", "7:kept"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void FunctionSelectionRetainsTiesUntilAHigherScoringCandidateWins() {
    var program = TestCompiler.Compile("""
      pure func choose sig number -> string { return(<number>) }
      pure func choose sig bool -> string { return(<bool>) }
      pure func choose sig string -> string { return(<string>) }
      pure func tied sig number -> string { return(<number>) }
      pure func tied sig bool -> string { return(<bool>) }
      mixin Example { expression {
        emit(choose(<12>))
        local failure = [tied(<12>)?]
        emit(kind(local#failure))
      } }
      """, "Example");
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"string", "error"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void StorageMemberLookupDoesNotCopyAndLocalsOverrideCarries() {
    var dictionaryType = typeof(HixVM).Assembly.GetType("Hix.HixValueDictionary", true)!;
    object Dictionary() => Activator.CreateInstance(dictionaryType, true)!;
    var locals = Dictionary();
    var carries = Dictionary();
    var store = dictionaryType.GetMethod("StoreIsolated", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
    void Put(object dictionary, string name, int value) => store.Invoke(dictionary,
      new object[] {HixString.Dynamic(name), new NumberHixValue(value)});
    Put(locals, "name", 1); Put(carries, "name", 2); Put(carries, "carried", 3);
    for (var i = 0; i < 10000; i++) Put(locals, "entry" + i, i);
    var type = typeof(HixVM).Assembly.GetType("Hix.Runtime.HixStorageValue", true)!;
    var value = (IHixValue)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
      null, new[] {locals, carries}, null)!;
    var context = new Context();
    var key = HixString.Dynamic("name");
    Assert.Equal(new NumberHixValue(1), value.Select(context, key));
    Assert.Equal(new NumberHixValue(3), value.Select(context, HixString.Dynamic("carried")));
    Assert.Same(NullHixValue.Instance, value.Select(context, HixString.Dynamic("missing")));
    var before = GC.GetAllocatedBytesForCurrentThread();
    for (var i = 0; i < 1000; i++) value.Select(context, key);
    Assert.True(GC.GetAllocatedBytesForCurrentThread() - before < 65536, "Member reads must not copy storage entries");
  }

  [Fact]
  public void StorageRootsSupportTablesAndCaptureValuesBeforeMutation() {
    var program = TestCompiler.Compile("""
      pure func captured { local name = <function>; return(local) }
      mixin Example { expression {
        var name = <before>
        local saved = [var]
        var name = <after>
        emit(var#name)
        emit(local#saved#name)
        emit(join(keys(local#saved), <,>))
        emit(get(captured(), <name>))
        local self = [local]
        emit(length(local#self))
      } }
      """, "Example");
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"after", "before", "name", "function", "1"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void LiteralLoadsAndPackingUseDedicatedOpcodesWithoutPoolEntries() {
    var program = TestCompiler.Compile("""
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
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"true:0:0", "true,false"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void SmartReferencesCompileToExplicitRootsAndMemberSelection() {
    var program = TestCompiler.Compile("""
      pure func read { local value = [$0]; return(<[$value]:[$it]>) }
      mixin Example { expression { emit(read(<argument>)) } }
      """, "Example");
    var roots = HixInstruction.ReadAll(program.Bytecode).Where(item => item.Instruction.Opcode == HixOpcode.LoadRoot)
      .Select(item => program.StringPool[item.Instruction.A]).ToArray();
    Assert.Contains("args", roots);
    Assert.Contains("param", roots);
    Assert.Contains("local", roots);
    Assert.DoesNotContain(Enum.GetNames(typeof(HixOpcode)), name => name.Contains("Smart") || name.StartsWith("Host"));
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal("argument:argument", Assert.Single(result.Outputs).Text);
  }

  [Fact]
  public void HostMembersUseLoadRootAndMemberInstructions() {
    var program = TestCompiler.Compile("""
      mixin Example { prelude expression { emit(this#name); emit(target#name); emit(attr#name) } }
      """, "Example");
    var instructions = HixInstruction.ReadAll(program.Bytecode).Select(item => item.Instruction).ToArray();
    Assert.Equal(new[] {"this", "target", "attr"}, instructions.Where(item => item.Opcode == HixOpcode.LoadRoot)
      .Select(item => program.StringPool[item.A]));
    Assert.Equal(3, instructions.Count(item => item.Opcode == HixOpcode.Member));
    var reads = 0;
    var result = HixVM.Execute(program, new PoolContext(_ => reads++));
    Assert.True(result.Success, result.Error);
    Assert.Equal(3, reads);
  }

  [Fact]
  public void DisassemblyHasAlignedColumnsAndHeadersAtFunctionAndLabelAddresses() {
    var program = TestCompiler.Compile("""
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
    var program = TestCompiler.Compile("mixin Example { expression {\n:again\ngoto again\n} }", "Example");
    var result = HixVM.Execute(program, new Context());
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
    var error = new ErrorHixValue(HixString.Dynamic("bad"));
    var variables = new Dictionary<string, object> {
      ["failure"] = error, ["items"] = new TupleHixValue(new IHixValue[] {error})
    };
    var program = TestCompiler.Compile("mixin Example { expression {\n" + statements + "\n} }", "Example");
    var result = HixVM.Execute(program, new Context(), variables);
    Assert.Equal(handled, result.Success);
    if (handled) Assert.Equal("caught", Assert.Single(result.Outputs).Text);
    else { Assert.Equal("bad", result.Error); Assert.Empty(result.Outputs); }
  }

  [Fact]
  public void ControlFlowAndCheckedFailuresDoNotThrowManagedExceptions() {
    var program = TestCompiler.Compile("""
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
    HixExpressionResult result;
    AppDomain.CurrentDomain.FirstChanceException += observe;
    try { result = HixVM.Execute(program, new Context()); }
    finally { AppDomain.CurrentDomain.FirstChanceException -= observe; }
    Assert.True(result.Success, result.Error);
    Assert.Empty(exceptions);
    Assert.Equal(new[] {"returned", "caught", "collection", "2"}, result.Outputs.Select(output => output.Text));
  }

  private sealed class PoolContext(Action<HixStringPool> observe) : Hix.Runtime.HixExecutionContext(TestBackend.Instance, new HixStringPoolBuilder().Freeze()) {
    protected override IHixValue ResolveHost(HixExpressionRoot root, HixString member) {
      Assert.Equal("", member.Resolve(Strings));
      observe(Strings); return HixTableValue.Empty;
    }
  }

  [Fact]
  public void AllProgramsAndCallbacksKeepOneVmGlobalStringPool() {
    var first = TestCompiler.Compile("""
      func origin { return(this#name) }
      mixin Example { prelude expression { var callback = [origin] } }
      """, "Example");
    var second = TestCompiler.Compile("""
      mixin Example { prelude expression { emit(call(var#callback)); emit(this#name) } }
      """, "Example");
    var vm = new HixVM(new[] {first, second});
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
    Assert.Contains("s16", Assert.Throws<ArgumentException>(() => TestCompiler.Compile(source, "Example")).Message);
  }

  [Fact]
  public void GlobalStringPoolAllowsU16CapacityAndRejectsOverflowDuringLoading() {
    var builder = new HixStringPoolBuilder();
    for (var i = 0; i < 65536; i++) builder.Intern("seed" + i);
    var pool = builder.Freeze();
    Assert.Equal(65536, new HixVM(Array.Empty<HixExpressionExecutionProgram>(), pool).StringPool.Count);
    var program = TestCompiler.Compile("mixin Example { expression { emit(<new>) } }", "Example");
    Assert.Contains("u16", Assert.Throws<ArgumentException>(() => new HixVM(new[] {program}, pool)).Message);
  }

  [Fact]
  public void ConstantsAreDeduplicatedAndStringsUseTheirOwnPool() {
    var program = TestCompiler.Compile("""
      mixin Example { expression { emit(42); emit(42); emit(<repeated>); emit(<repeated>); emit(true); emit(null) } }
      """, "Example");
    Assert.Equal(HixInstruction.ReadAll(program.Bytecode).Select(item => item.Offset), program.SourceLines.Keys);
    Assert.Single(program.ConstantPool.OfType<NumberHixValue>());
    Assert.DoesNotContain(program.ConstantPool, value => value.Kind == HixValueKind.String);
    Assert.True(program.StringPool.TryGetId("repeated", out var stringIndex));
    var instructions = HixInstruction.ReadAll(program.Bytecode).Select(item => item.Instruction).ToArray();
    Assert.Equal(2, instructions.Count(instruction => instruction.Opcode == HixOpcode.LoadString && instruction.A == stringIndex));
    var stringCount = program.StringPool.Count;
    var result = HixVM.Execute(program, new Context());
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
    var program = TestCompiler.Compile("""
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
    Assert.All(typeof(HixVM).GetMethods(BindingFlags.Public | BindingFlags.Static), method =>
      Assert.DoesNotContain(method.GetParameters(), parameter => parameter.ParameterType == typeof(string) || typeof(HixAst).IsAssignableFrom(parameter.ParameterType)));
  }

  [Fact]
  public void SharedFunctionValuesExecuteTheirOriginalBytecodeAndPools() {
    var variables = new Dictionary<string, object>();
    var first = TestCompiler.Compile("""
      pure func decorate { return(<origin:[param]:[length(param)]>) }
      mixin Example { expression { var callback = [decorate] } }
      """, "Example");
    var second = TestCompiler.Compile("""
      pure func distract { return(<different layout>) }
      mixin Example { expression { emit(call(var#callback, <value>)); emit(<caller>) } }
      """, "Example");
    Assert.True(HixVM.Execute(first, new Context(), variables).Success);
    var result = HixVM.Execute(second, new Context(), variables);
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"origin:value:5", "caller"}, result.Outputs.Select(output => output.Text));
  }

  [Fact]
  public void BranchesUseSignedRelativeByteOffsetsAndRestoreStacks() {
    var program = TestCompiler.Compile("""
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
    var result = HixVM.Execute(program, new Context());
    Assert.True(result.Success, result.Error);
    Assert.Equal(new[] {"value:2", "caught", "yes"}, result.Outputs.Select(output => output.Text));
  }
}
