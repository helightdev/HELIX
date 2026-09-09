using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hix;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixResultTests {
  [Fact]
  public void RenderedOutputsDoNotAllocateOrCopyText() {
    var text = new string('x', 64);
    ReadOutputs(HixString.Dynamic(text), 1);
    var before = GC.GetAllocatedBytesForCurrentThread();
    var length = ReadOutputs(HixString.Dynamic(text), 1000);
    var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
    Assert.Equal(64000, length);
    Assert.Equal(0, allocated);
    Assert.Same(text, new HixOutput(HixString.Dynamic(text)).Text.Resolve(null));
  }

  [Fact]
  public void OutputsAndErrorsPreservePooledTextAfterThreadReuse() {
    var thread = TestBackend.Instance.CreateThread();
    var first = HixVM.Execute(TestCompiler.Compile("mixin Example { expression { log(<logged>); emit(<pooled>) } }", "Example"), thread);
    var output = Assert.Single(first.Outputs);
    Assert.True(output.Text.IsInterned);
    Assert.True(Assert.Single(first.Logs).Text.IsInterned);
    var second = HixVM.Execute(TestCompiler.Compile("mixin Example { expression { fail<failure> } }", "Example"), thread);
    Assert.False(second.Success);
    Assert.True(second.Error.IsInterned);
    Assert.Equal("failure", second.Error.Resolve(second.Strings));
    Assert.Equal("pooled", output.Text.Resolve(first.Strings));
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
  private static int ReadOutputs(HixString text, int count) {
    var length = 0;
    for (var i = 0; i < count; i++) {
      var output = new HixOutput(text);
      length += output.Text.Length(null);
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
    Assert.Equal("", default(HixOutput).Text.Resolve(null));
  }
}
