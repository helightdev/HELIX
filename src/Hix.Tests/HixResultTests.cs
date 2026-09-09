using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hix;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixResultTests {
  [Fact]
  public void EmittedValuesKeepTheirTypesAndDetachNestedStorageAtEmission() {
    var thread = TestBackend.Instance.CreateThread();
    var program = TestCompiler.Compile("""
      mixin Example { expression {
        local name = <before>
        emit(42)
        emit(true)
        emit(null)
        emit(<data>, @[@{name=<nested>}, local])
        emit(local)
        emit([error<kept>?])
        local name = <after>
      } }
      """, "Example");
    var result = HixVM.Execute(program, thread);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(42, Assert.IsType<NumberHixValue>(result.Outputs[0].Value).Value);
    Assert.IsType<BooleanHixValue>(result.Outputs[1].Value);
    Assert.IsType<NullHixValue>(result.Outputs[2].Value);
    var tuple = Assert.IsType<TupleHixValue>(result.Outputs[3].Value);
    Assert.Equal("data", result.Outputs[3].Target.Resolve(null));
    Assert.False(result.Outputs[3].Target.IsInterned);
    var second = TestCompiler.Compile("mixin Example { expression { local name = <replacement>; emit(local) } }", "Example");
    Assert.True(HixVM.Execute(second, thread).Success);
    Assert.Equal("nested", ReadName(tuple.Values[0]));
    Assert.Equal("before", ReadName(tuple.Values[1]));
    Assert.Equal("before", ReadName(result.Outputs[4].Value));
    Assert.Equal("kept", Assert.IsType<ErrorHixValue>(result.Outputs[5].Value).Message.Resolve(null));

    static string ReadName(IHixValue value) {
      var table = Assert.IsType<HixTableValue>(value);
      Assert.True(table.TryGetValue(new HixThread(), HixString.Dynamic("name"), out var name));
      var literal = Assert.IsType<LiteralHixValue>(name);
      Assert.False(literal.Value.IsInterned);
      return literal.Value.Resolve(null);
    }
  }

  [Fact]
  public void OutputWrappersDoNotAllocateOrCopyValues() {
    var text = new string('x', 64);
    var value = new LiteralHixValue(HixString.Dynamic(text));
    ReadOutputs(value, 1);
    var before = GC.GetAllocatedBytesForCurrentThread();
    var length = ReadOutputs(value, 1000);
    var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    Assert.Equal(64000, length);
    Assert.Equal(0, allocated);
    Assert.Same(text, Assert.IsType<LiteralHixValue>(new HixOutput(value).Value).Value.Resolve(null));
  }

  [Fact]
  public void OutputsDetachWhileLogsAndErrorsRetainTheirPool() {
    var thread = TestBackend.Instance.CreateThread();
    var first = HixVM.Execute(TestCompiler.Compile("mixin Example { expression { log(<logged>); emit(<pooled>) } }", "Example"), thread);
    var output = Assert.Single(first.Outputs);
    Assert.False(Assert.IsType<LiteralHixValue>(output.Value).Value.IsInterned);
    Assert.True(Assert.Single(first.Logs).Text.IsInterned);
    var second = HixVM.Execute(TestCompiler.Compile("mixin Example { expression { fail<failure> } }", "Example"), thread);
    Assert.False(second.Success);
    Assert.True(second.Error.IsInterned);
    Assert.Equal("failure", second.Error.Resolve(second.Strings));
    Assert.Equal("pooled", output.ReadText());
  }

  [Fact]
  public void FingerprintsCompareContentAcrossPoolsAndDynamicStrings() {
    var first = new HixStringPoolBuilder();
    var a = first.Intern("same");
    var second = new HixStringPoolBuilder();
    var different = second.Intern("different");
    var b = second.Intern("same");
    var firstPool = first.Freeze();
    var secondPool = second.Freeze();
    Assert.Equal(a.Fingerprint(firstPool), b.Fingerprint(secondPool));
    Assert.Equal(a.Fingerprint(firstPool), HixString.Dynamic("same").Fingerprint(null));
    Assert.NotEqual(a.Fingerprint(firstPool), different.Fingerprint(secondPool));
    var before = GC.GetAllocatedBytesForCurrentThread();
    for (var i = 0; i < 1000; i++) a.Fingerprint(firstPool);
    Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static int ReadOutputs(LiteralHixValue text, int count) {
    var length = 0;
    for (var i = 0; i < count; i++) {
      var output = new HixOutput(text);
      length += ((LiteralHixValue)output.Value).Value.Length(null);
    }
    return length;
  }

  [Fact]
  public void DefaultAndEmptyResultsExposeImmutableSharedCollections() {
    var empty = default(HixExecutionResult);
    Assert.Empty(empty.Outputs);
    Assert.Empty(empty.Logs);
    Assert.Empty(empty.Variables);
    Assert.Empty(empty.Carries);
    var program = TestCompiler.Compile("mixin Example { expression { emit(<ready>) } }", "Example");
    var result = HixVM.Execute(program, TestBackend.Instance.CreateThread());
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Same(empty.Variables, result.Variables);
    Assert.Same(empty.Carries, result.Carries);
    Assert.Throws<NotSupportedException>(() => ((IDictionary<HixString, IHixValue>)result.Variables).Add(HixString.Dynamic("leak"), new NumberHixValue(1)));
    Assert.IsType<NullHixValue>(default(HixOutput).Value);
  }
}
