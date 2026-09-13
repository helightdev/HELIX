using System;
using System.Collections.Generic;
using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public class HixDecompilerTests {
  [Fact]
  public void MissingHasItsOwnOperandFreeLoad() {
    var instruction = new HixInstruction(HixOpcode.LoadMissing);
    Assert.Equal(11, (byte)instruction.Opcode); Assert.Equal(1, instruction.Size);
    var bytes = new byte[1]; instruction.Encode(bytes, 0); Assert.Equal(instruction, HixInstruction.Decode(bytes, 0));
    Assert.Throws<ArgumentException>(() => new HixInstruction(HixOpcode.LoadMissing, 1).Encode(bytes, 0));
    Assert.Throws<ArgumentException>(() => new HixInstruction(HixOpcode.LoadMissing, B: 1).Encode(bytes, 0));
    var image = HixCompiler.CompileFunctions(["func main { return(missing, null) }"]);
    Assert.Contains(HixInstruction.ReadAll(image.Bytecode), item => item.Instruction.Opcode == HixOpcode.LoadMissing);
    Assert.DoesNotContain(image.ConstantPool, value => value is MissingHixValue);
    Assert.Contains("push(missing)", image.Disassemble());
    var result = new HixVM([image]).Invoke(image, new HixCoroutine(), "main", []);
    Assert.True(result.Success); var tuple = Assert.IsType<TupleHixValue>(result.Value);
    Assert.IsType<MissingHixValue>(tuple.Values[0]); Assert.IsType<NullHixValue>(tuple.Values[1]);
    RoundTrip("func main { return(missing, null) }");
  }

  [Theory]
  [InlineData("pure func main { return(1, true, false, null, missing, @[], @{}, @[2, 3], @{key=4}) }")]
  [InlineData("""pure func main { local value = "line\n\"quoted\""; return(<[$value]:[uppercase($value)]>) }""")]
  [InlineData("pure func main { return(!false, [error<oops>?], [null ?: <fallback>]) }")]
  [InlineData("pure func main(number value) -> string { local result = when [$value] {\n 1 -> <one>\n 2 -> <two>\n else -> <other>\n }; return($result) }")]
  [InlineData("pure func main { when(true) { return(<yes>) }; return(<no>) }")]
  [InlineData("pure func main { local count = 0; :again\n $count = plus($count, 1); when(compare($count, <lt>, 3)) { goto again }; return($count) }")]
  [InlineData("pure func main { local values = @[1,2]; return(map($values, func => plus($0, 1))) }")]
  [InlineData("pure func helper(%optional string value, string fallback = [<default>]) -> any { return($value, $fallback) } pure func main { return(helper(fallback = <other>)) }")]
  [InlineData("type Settings = @{%optional string name, number count = [2]} pure func main { return(Settings(count = 3)) }")]
  [InlineData("pure func main { local result = when [<ab>] {\n [:startsWith<a>] -> <yes>\n else -> <no>\n }; return($result) }")]
  [InlineData("pure func choose(string value) -> string => <string>; pure func choose(number value) -> string => <number>; pure func main { local any value = 1; return(choose($value)) }")]
  [InlineData("pure func main { return(@root(\"null\"), @root(\"missing\"), null, missing) }")]
  [InlineData("pure func main { return() }")]
  [InlineData("pure func main { local count = 0; { $count = plus($count, 1); when {\n (compare($count, <lt>, 3)) -> continue\n else -> break\n } }; return($count) }")]
  [InlineData("pure func main(number value) -> number { %marker(<raw-args>)\n local valueCopy = [args#0]; return(plus($valueCopy, 1)) }")]
  [InlineData("pure func main [%many number] -> number { return(length(param)) }")]
  [InlineData("pure func main(number value, %variadic any remaining) -> any { return(param) }")]
  [InlineData("pure func main -> (number count, %optional string label) { return(@{count=1}) }")]
  [InlineData("pure func main { return($unknown) }")]
  [InlineData("pure func main { goto missingLabel }")]
  [InlineData("pure func main { local __selector_0 = 4; local __selector_value = 5; return($__selector_0, $__selector_value) }")]
  [InlineData("pure func truth { return(true) } pure func main { return([@{value=2}#value], [truth() @dynamic @bool]) }")]
  [InlineData("type Shape = @{string value} | null; pure func main { return(Shape(@{value=<text>})) }")]
  [InlineData("pure func choose(string value) -> string => <string>; pure func main { return(choose(1)) } pure func choose(number value) -> string => <number>;")]
  public void FunctionLibrariesRoundTrip(string source) => RoundTrip(source);

  private static void RoundTrip(string source) {
    var unit = AntlrSyntax.Parse(source); Assert.Empty(unit.Diagnostics);
    var baseline = HixCompiler.CompileFunctions([source]);
    var printed = HixDecompiler.ToSource(unit);
    Assert.Equal(printed, HixDecompiler.ToSource(unit));
    var sourceImage = HixCompiler.CompileFunctions([printed]); HixRoundTripAssertions.Equal(baseline, sourceImage);
    var recovered = HixDecompiler.Decompile(baseline);
    var recoveredGlobals = HixCompiler.PrepareGlobals([recovered]);
    HixRoundTripAssertions.Equal(baseline, new HixBytecodeCompiler(recoveredGlobals.StringPool).Compile([], [], recoveredGlobals));
    var decompiled = HixDecompiler.ToSource(recovered);
    Assert.Equal(decompiled, HixDecompiler.ToSource(HixDecompiler.Decompile(baseline)));
    var roundtrip = HixCompiler.CompileFunctions([decompiled]); HixRoundTripAssertions.Equal(baseline, roundtrip);
    Assert.Equal(decompiled, HixDecompiler.ToSource(HixDecompiler.Decompile(roundtrip)));
    HixRoundTripAssertions.Equal(roundtrip, HixCompiler.CompileFunctions([HixDecompiler.ToSource(HixDecompiler.Decompile(roundtrip))]));
  }

  [Fact]
  public void PreparedInlineExpressionsKeepChecksAndFallbackLazy() {
    const string source = "inline pure func decorate(string value) -> string { return(uppercase($value)) } mixin Example { expression { local value = [null ?: decorate(<hello>)]; emit($value) } }";
    var unit = AntlrSyntax.Parse(source, TestBackend.Instance);
    var globals = HixCompiler.PrepareGlobals([unit], TestBackend.Instance);
    var declaration = unit.Declarations.OfType<MixinDeclarationIr>().Single();
    var module = HixCompiler.PrepareIr(declaration, globals);
    var prepared = new CompilationUnitIr("", globals.Functions.Cast<HixIrNode>().Concat([new MixinDeclarationIr("Example", false, module.Prelude.Cast<HixIrNode>().Concat(module.Late).Concat(module.Functions).ToArray())]).ToArray(), [], []);
    var baseline = HixCompiler.Compile(unit, "Example", TestBackend.Instance);
    var printed = HixDecompiler.ToSource(prepared);
    var roundtrip = HixCompiler.Compile(printed, "Example", TestBackend.Instance);
    HixRoundTripAssertions.Equal(baseline, roundtrip);
    var recovered = HixDecompiler.Decompile(baseline, "Example");
    HixRoundTripAssertions.Equal(baseline, HixCompiler.Compile(HixDecompiler.ToSource(recovered), "Example", TestBackend.Instance));
  }

  [Fact]
  public void ExpressionsDerivationsCarriesAndFilteredPassesRoundTrip() {
    const string source = """
      derivation mixin Named { expression { emit(<derived>) } }
      pure func helper(string value) -> string { return(uppercase($value)) }
      mixin Example(string value = [<default>]) {
        prelude expression { carry local saved = <prelude>; derive(@[]) }
        expression { emit(local#saved); emit(helper($value)) }
      }
      """;
    var baseline = HixCompiler.Compile(source, "Example", TestBackend.Instance);
    foreach (var image in new[] {baseline, baseline.ForPass(true), baseline.ForPass(false)}) {
      var unit = HixDecompiler.Decompile(image, "Example");
      var roundtrip = HixCompiler.Compile(HixDecompiler.ToSource(unit), "Example", TestBackend.Instance);
      if (ReferenceEquals(image, baseline)) HixRoundTripAssertions.Equal(image, roundtrip);
      var firstThread = new HixCoroutine(); var secondThread = new HixCoroutine();
      if (!image.Expressions.Any(entry => entry.IsPrelude)) {
        Assert.True(HixVM.Execute(baseline.ForPass(true), firstThread).Success);
        Assert.True(HixVM.Execute(baseline.ForPass(true), secondThread).Success);
      }
      var first = HixVM.Execute(image, firstThread); var second = HixVM.Execute(roundtrip, secondThread);
      Assert.True(first.Success, first.Error.Resolve(first.Strings)); Assert.True(second.Success, second.Error.Resolve(second.Strings));
      Assert.Equal(first.Outputs.Select(output => output.ReadText()), second.Outputs.Select(output => output.ReadText()));
    }
  }

  [Theory]
  [InlineData("[error<failed>?]", "[error<failed>?]")]
  [InlineData("[[<kept>] ?: error<unreachable>]", "[[<kept>] ?: error<unreachable>]")]
  public void InlineCallsInsideCheckedAndFallbackExpressionsPreserveExecution(string first, string second) {
    var source = "inline func identity(any value) -> any { return($value) } mixin Example { expression { local result = [identity(" + first + ")?]; emit(kind($result)); local other = " + second + "; emit(kind($other)) } }";
    var baseline = HixCompiler.Compile(source, "Example", TestBackend.Instance);
    var roundtrip = HixCompiler.Compile(HixDecompiler.ToSource(HixDecompiler.Decompile(baseline, "Example")), "Example", TestBackend.Instance);
    HixRoundTripAssertions.Equal(baseline, roundtrip);
    var expected = HixVM.Execute(baseline, new HixCoroutine()); var actual = HixVM.Execute(roundtrip, new HixCoroutine());
    Assert.True(expected.Success); Assert.True(actual.Success);
    Assert.Equal(expected.Outputs.Select(output => output.ReadText()), actual.Outputs.Select(output => output.ReadText()));
  }

  [Fact]
  public void CanonicalComparisonPreservesStringLiteralsAndUserLocalNames() {
    var first = HixCompiler.CompileFunctions(["pure func main { return(<__selector_0>) }"]);
    var second = HixCompiler.CompileFunctions(["pure func main { return(<__selector_1>) }"]);
    Assert.NotEqual(HixRoundTripAssertions.Canonical(first), HixRoundTripAssertions.Canonical(second));
    first = HixCompiler.CompileFunctions(["pure func main { local __selector_0 = 1; return(local) }"]);
    second = HixCompiler.CompileFunctions(["pure func main { local __selector_1 = 1; return(local) }"]);
    Assert.NotEqual(HixRoundTripAssertions.Canonical(first), HixRoundTripAssertions.Canonical(second));
  }

  [Theory]
  [InlineData(new byte[] {(byte)HixOpcode.Enter, 5, 0, (byte)HixOpcode.Pop, (byte)HixOpcode.End}, 3)]
  [InlineData(new byte[] {(byte)HixOpcode.Enter, 6, 0, (byte)HixOpcode.LoadTrue, (byte)HixOpcode.CastString, (byte)HixOpcode.End}, 5)]
  [InlineData(new byte[] {(byte)HixOpcode.Enter, 7, 0, (byte)HixOpcode.Interpolate, 1, 0, (byte)HixOpcode.End}, 3)]
  public void MalformedInstructionSequencesFailAtTheOffendingOffset(byte[] bytes, int offset) {
    var image = new HixProgramImage(bytes, new(), [], new HixStringPoolBuilder().Freeze(), [new BytecodeExpression(new BytecodeFunction("", false, [], 0), false, 0)], [], new LanguageFunctionScope([]), new Dictionary<string,HixPattern>(), HixCoreBackend.Instance);
    Assert.Contains("offset " + offset, Assert.Throws<ArgumentException>(() => HixDecompiler.Decompile(image)).Message);
  }

  [Theory]
  [InlineData(new byte[] {255})]
  [InlineData(new byte[] {(byte)HixOpcode.LoadConst})]
  [InlineData(new byte[] {(byte)HixOpcode.Jump, 1, 0})]
  [InlineData(new byte[] {(byte)HixOpcode.LoadString, 0, 0})]
  public void MalformedImagesReportOffsets(byte[] bytes) {
    var image = new HixProgramImage(bytes, new(), [], new HixStringPoolBuilder().Freeze(), [], [], new LanguageFunctionScope([]), new Dictionary<string,HixPattern>(), HixCoreBackend.Instance);
    Assert.Contains("offset 0", Assert.Throws<ArgumentException>(() => HixDecompiler.Decompile(image)).Message);
  }
}
