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
  public void DebugTracePrintsGlobalPoolsOnceAndUsesTheirIndices(bool internStrings) {
    var first = HixCompiler.Compile("mixin Example { expression { emit(<shared>); emit(42) } }", "Example");
    var second = HixCompiler.Compile("mixin Example { expression { emit(<different>); emit(<shared>); emit(7); emit(42) } }", "Example");
    var seed = new HixStringPoolBuilder().Freeze();
    var vm = new HixVM(new[] {first, second}, seed);
    HixDebugExpression Work(HixExpressionExecutionProgram program, string name) => new(
      program, program, ImmutableDictionary<string, object>.Empty, ImmutableDictionary<string, object>.Empty,
      name, "", "", 0, 0);
    var trace = HixDebugRenderer.BuildTrace(new HixDebugRenderData(seed,
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
    Assert.Equal(new[] {42d, 7d}, vm.ConstantPool.Cast<NumberHixValue>().Select(value => value.Value));
    var constantLoads = Regex.Matches(bodies, @"(?m)^//   [0-9A-F]+ +\| LOADCONST c([0-9]+) +\|");
    Assert.Equal(new[] {"0", "0", "1", "0", "1", "0"},
      constantLoads.Cast<Match>().Select(load => load.Groups[1].Value));
    var pool = vm.ConstantPool;
    Assert.Equal(new[] {"shared", "42"}, vm.Run(first, new HixExecutionContext()).Outputs.Select(output => output.Text));
    Assert.Equal(new[] {"different", "shared", "7", "42"}, vm.Run(second, new HixExecutionContext()).Outputs.Select(output => output.Text));
    Assert.Same(pool, vm.ConstantPool);

  }

}
