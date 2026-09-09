using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Immutable;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Hix.Diagnostics;
using Xunit;
namespace HELIX.SourceGen.Tests;
public class HixDebugTests {
  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public void DebugTracePrintsEachPreparedProgramWithoutRelinking(bool internStrings) {
    var first = HixCompiler.Compile("mixin Example { expression { emit(<shared>); emit(42) } }", "Example");
    var second = HixCompiler.Compile("mixin Example { expression { emit(<different>); emit(<shared>); emit(7); emit(42) } }", "Example");
    var seed = new HixStringPoolBuilder().Freeze();
    HixDebugExpression Work(HixProgramImage program, string name) => new(
      program, program, ImmutableDictionary<string, object>.Empty, ImmutableDictionary<string, object>.Empty,
      name, "", "", 0, 0);
    var trace = HixDebugRenderer.BuildTrace(new HixDebugRenderData(seed,
      ImmutableArray.Create(Work(first, "first"), Work(second, "second")), internStrings, 0, default, default, default));
    Assert.DoesNotContain("//", trace);
    Assert.Contains("PRELUDE", trace);
    Assert.Contains("LATE", trace);
    var loads = Regex.Matches(trace, @"(?m)^[0-9A-F]+ +\| LOADSTRING s([0-9]+) +\| push\(""shared""\)");
    Assert.Equal(4, loads.Count);
    Assert.Contains(".constant", trace);
    Assert.Contains("Function emit(", trace);
    Assert.DoesNotContain("PatternHixValue {", trace);
  }

}
