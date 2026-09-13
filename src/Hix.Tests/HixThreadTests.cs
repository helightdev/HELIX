using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Hix;
using Hix.Compiler;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class HixThreadTests {
  [Fact]
  public void FunctionOnlyContextOwnsTransactionsWithoutExpressionState() {
    var context = new FunctionOnlyContext();
    var program = HixCompiler.CompileFunctions(["""
      func child() -> any { emit<discard>; fail<stop> }
      func main { local failed = [child()?]; emit<kept>; return(42) }
      """], TestBackend.Instance);
    var thread = new HixCoroutine(context);
    var result = new HixVM([program]).Invoke(program, thread);
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(42, Assert.IsType<NumberHixValue>(result.Value).Value);
    var host = Assert.IsType<FunctionOnlyContext>(thread.Context);
    Assert.Equal(1, host.EmissionCount);
    Assert.Equal(1, host.RollbackCount);
    Assert.Empty(result.Execution.Outputs);
    foreach (var name in new[] { "Emit", "ExportVariables", "ExportCarries", "CaptureCheckpoint", "RollbackCheckpoint" })
      Assert.Null(typeof(HixCoroutine).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
    Assert.DoesNotContain(typeof(HixCoroutine).GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
      field => field.FieldType == typeof(HixExpressionState));
    Assert.Null(typeof(HixContext).GetMethod("ResolveMixinParameter"));
    Assert.Null(typeof(HixContext).GetMethod("ExportVariables"));
    Assert.Null(typeof(HixContext).GetMethod("CaptureEffects"));
    Assert.Null(typeof(HixContext).GetProperty("Host"));
  }

  private sealed class FunctionOnlyContext() : HixContext(TestBackend.Instance) {
    public int EmissionCount { get; private set; }
    public int RollbackCount { get; private set; }
    public override void Emit(HixCoroutine thread, IHixValue value, HixString target = default) => EmissionCount++;
    public override HixContextCheckpoint CaptureCheckpoint(HixCoroutine thread) =>
      base.CaptureCheckpoint(thread) with { Position = EmissionCount };
    public override void RollbackCheckpoint(HixCoroutine thread, HixContextCheckpoint checkpoint, bool preserveLogs = false) {
      EmissionCount = checkpoint.Position;
      RollbackCount++;
      base.RollbackCheckpoint(thread, checkpoint, preserveLogs);
    }
  }

  [Fact]
  public void OutputCollectionDoesNotAllocateUnusedSharedStores() {
    var context = new HixExpressionContext(TestBackend.Instance);
    var program = TestCompiler.Compile("mixin Example { expression { emit(<hello>) } }", "Example");
    var result = HixVM.Execute(program, new HixCoroutine(context));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("hello", Assert.Single(context.Outputs).ReadText());
    foreach (var name in new[] {"variables", "carries", "targetVariables"}) {
      var field = typeof(HixExpressionState).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
      Assert.Null(field.GetValue(context.ExpressionState));
    }
  }

  [Fact]
  public void FailedInvocationRollsBackLazilyCreatedSharedState() {
    var program = HixCompiler.CompileFunctions(["""
      func main { var<saved> @= <discard>; emit(<discard>); fail<stop> }
      """], TestBackend.Instance);
    var thread = TestBackend.Instance.CreateCoroutine();
    var result = new HixVM([program]).Invoke(program, thread);
    Assert.False(result.Success);
    Assert.Empty(result.Execution.Outputs);
    Assert.Empty(((HixExpressionContext)thread.Context).ExportVariables(thread));
    Assert.Same(MissingHixValue.Instance, thread.Resolve(HixExpressionRoot.Variable, HixString.Dynamic("saved")));
  }

  [Theory]
  [InlineData(false)]
  [InlineData(true)]
  public void NestedBytecodeCallsUseHeapFramesInsteadOfRecursiveInterpreterCalls(bool callbackCalls) {
    var backend = new ProbeBackend();
    var functions = Enumerable.Range(0, 80).Select(index => "pure func step" + index + "() -> any { return(" +
      (callbackCalls ? "call(step" + (index + 1) + ")" : "step" + (index + 1) + "()") + ") }");
    var source = string.Join("\n", functions) + "\npure func identity { return(param) }\n" +
      "pure func step80() -> any { return(probe(identity)) }";
    var program = HixCompiler.CompileFunctions([source], backend);
    var thread = backend.CreateCoroutine();
    var callerThread = Environment.CurrentManagedThreadId;
    backend.Probe = active => {
      Assert.Same(thread, active);
      Assert.Same(backend, active.Machine.Backend);
      Assert.Same(active.Machine.Backend, active.Backend);
      Assert.Equal(callerThread, Environment.CurrentManagedThreadId);
      var frames = (System.Collections.ICollection)typeof(HixCoroutine).GetField("frames", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(thread);
      Assert.Equal(81, frames.Count);
      Assert.Same(active.Machine.Patterns, active.PatternDefinitions);
      Assert.DoesNotContain(typeof(HixCoroutine).GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
        field => field.FieldType == typeof(LanguageFunctionScope));
      foreach (var frame in frames) {
        var function = Assert.IsType<BytecodeFunctionEntry>(frame.GetType().GetField(
          "Function", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(frame));
        Assert.Contains(active.Machine.Functions, entry => ReferenceEquals(entry, function));
      }
      Assert.Single(new StackTrace().GetFrames().Where(frame => frame.GetMethod().Name == "RunInstructions" &&
        frame.GetMethod().DeclaringType.DeclaringType == typeof(HixCoroutine)));
    };
    var result = new HixVM([program]).Invoke(program, thread, "step0");
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.IsType<HixTableValue>(result.Value);
  }

  [Fact]
  public void NestedBlocksUseTheSameHeapStackAndHostExceptionsReleaseIt() {
    var backend = new ProbeBackend();
    var body = new string('{', 80) + " return(probe(identity)) " + new string('}', 80);
    var program = HixCompiler.CompileFunctions([
      "pure func identity { return(param) } pure func main { " + body + " }"
    ], backend);
    var machine = new HixVM([program]);
    var thread = backend.CreateCoroutine();
    object Buffer(string name) => typeof(HixCoroutine).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(thread);
    var frames = (System.Collections.ICollection)Buffer("frames");
    backend.Probe = _ => {
      Assert.Equal(81, frames.Count);
      Assert.Single(new StackTrace().GetFrames().Where(frame => frame.GetMethod().Name == "RunInstructions" &&
        frame.GetMethod().DeclaringType.DeclaringType == typeof(HixCoroutine)));
      throw new InvalidOperationException("host exception");
    };
    Assert.Equal("host exception", Assert.Throws<InvalidOperationException>(() => machine.Invoke(program, thread)).Message);
    Assert.Equal(0, frames.Count);
    Assert.Empty((List<IHixValue>)Buffer("stack"));
    Assert.Empty((List<(int Target, int Stack)>)Buffer("checks"));
    Assert.False(thread.IsRunning);
    backend.Probe = _ => { };
    var result = machine.Invoke(program, thread, "main", HixCoroutine.String("reused"));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal("reused", result.Value.Unlink(thread));
    Assert.Equal(0, frames.Count);
  }

  [Fact]
  public void NestedCallsAndChecksReuseThreadBuffersAndReleaseEveryFrameSlice() {
    var program = HixCompiler.CompileFunctions(["""
      pure func nested() -> any {
        local failure = [cast(<inner>, error)?]
        { local value = 11 }
        return(plus(1, 2))
      }
      pure func main {
        local failure = [plus(10, plus(nested(), cast(<outer>, error)))?]
        assert(is(local#failure, error))
        return(plus(20, nested()))
      }
      pure func broken { return(plus(10, plus(nested(), cast(<uncaught>, error)))) }
      """], TestBackend.Instance);
    var machine = new HixVM([program]);
    var thread = TestBackend.Instance.CreateCoroutine();
    object Buffer(string name) => typeof(HixCoroutine).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(thread);
    var operands = Assert.IsType<List<IHixValue>>(Buffer("stack"));
    var checks = Assert.IsType<List<(int Target, int Stack)>>(Buffer("checks"));
    var frames = (System.Collections.ICollection)Buffer("frames");
    for (var index = 0; index < 3; index++) {
      var success = machine.Invoke(program, thread);
      Assert.True(success.Success, success.Error.Resolve(success.Strings));
      Assert.Equal(23, Assert.IsType<NumberHixValue>(success.Value).Value);
      Assert.Empty(operands);
      Assert.Empty(checks);
      Assert.Equal(0, frames.Count);
      var failure = machine.Invoke(program, thread, "broken");
      Assert.False(failure.Success);
      Assert.Empty(operands);
      Assert.Empty(checks);
      Assert.Equal(0, frames.Count);
      Assert.Same(operands, Buffer("stack"));
      Assert.Same(checks, Buffer("checks"));
    }
    var frame = typeof(HixCoroutine).GetNestedType("ExecutionFrame", BindingFlags.NonPublic);
    Assert.False(frame.IsValueType);
    Assert.NotNull(frame.GetField("pc", BindingFlags.Instance | BindingFlags.NonPublic));
    Assert.DoesNotContain(frame.GetFields(BindingFlags.Instance | BindingFlags.NonPublic), field =>
      typeof(System.Collections.ICollection).IsAssignableFrom(field.FieldType));
  }

  [Fact]
  public void ContextOwnsVariablesAndCarriesAcrossThreadsAndProgramPools() {
    var context = new HixExpressionContext(TestBackend.Instance);
    var first = TestCompiler.Compile("""
      mixin Example { prelude expression {
        var<saved> @= <value>;
        carry local carried = null
        local transient = <private>
      } }
      """, "Example");
    Assert.True(HixVM.Execute(first, new HixCoroutine(context)).Success);
    Assert.True(context.Carries.ContainsKey(HixString.Dynamic("carried")));
    Assert.Equal("value", Assert.IsType<LiteralHixValue>(context.Variables[HixString.Dynamic("saved")]).Value.Resolve(null));
    var second = TestCompiler.Compile("""
      mixin Example { expression {
        local carried = <updated>
        emit(var#saved)
        emit(local#carried)
        emit(local#transient)
      } }
      """, "Example");
    var result = HixVM.Execute(second, new HixCoroutine(context));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(new[] {"value", "updated", "missing"}, result.Outputs.Select(output => output.ReadText()));
    Assert.Equal("updated", Assert.IsType<LiteralHixValue>(context.Carries[HixString.Dynamic("carried")]).Value.Resolve(null));
    var failing = TestCompiler.Compile("""
      mixin Example { expression { var<saved> @= <discard>; local carried = <discard>; fail<stop> } }
      """, "Example");
    Assert.False(HixVM.Execute(failing, new HixCoroutine(context)).Success);
    Assert.Equal("value", Assert.IsType<LiteralHixValue>(context.Variables[HixString.Dynamic("saved")]).Value.Resolve(null));
    Assert.Equal("updated", Assert.IsType<LiteralHixValue>(context.Carries[HixString.Dynamic("carried")]).Value.Resolve(null));
  }

  [Fact]
  public void ReuseResetsInvocationStateAndPreservesCommittedTargets() {
    var program = TestCompiler.Compile("""
      mixin Example { expression {
        emit(var#input)
        tar<saved> @= [var#input];
      } }
      """, "Example");
    var thread = TestBackend.Instance.CreateCoroutine();
    var first = HixVM.Execute(program, thread, new Dictionary<string, object> { ["input"] = "first" });
    var second = HixVM.Execute(program, thread, new Dictionary<string, object> { ["input"] = "second" });
    Assert.True(first.Success, first.Error.Resolve(first.Strings));
    Assert.True(second.Success, second.Error.Resolve(second.Strings));
    Assert.Equal("first", Assert.Single(first.Outputs).ReadText());
    Assert.Equal("second", Assert.Single(second.Outputs).ReadText());
    Assert.Equal(first.ExecutedOperations, second.ExecutedOperations);
    Assert.Equal("second", thread.Resolve(HixExpressionRoot.TargetVariable, HixString.Dynamic("saved")).Unlink(thread));
    Assert.False(thread.IsRunning);
    Assert.False(HixExpressionContext.IsPrelude(thread));
    Assert.IsType<ErrorHixValue>(thread.Invoke(new NamedFunctionHixValue(HixString.Dynamic("missing")), []));
  }

  [Fact]
  public void IndependentThreadsShareTheCachedVmAcrossDifferentSeedPools() {
    var program = TestCompiler.Compile("mixin Example { expression { emit(var#input) } }", "Example");
    var threads = Enumerable.Range(0, 8).Select(_ => new HixCoroutine(TestBackend.Instance, new HixStringPoolBuilder().Freeze())).ToArray();
    System.Threading.Tasks.Parallel.For(0, threads.Length, index => {
      var result = HixVM.Execute(program, threads[index], new Dictionary<string, object> { ["input"] = index });
      Assert.True(result.Success, result.Error.Resolve(result.Strings));
      Assert.Equal(index.ToString(), Assert.Single(result.Outputs).ReadText());
    });
    Assert.All(threads, thread => Assert.Same(threads[0].Machine, thread.Machine));
  }

  [Fact]
  public void HostCallsSeeCurrentFrameAndCannotRestartTheActiveThread() {
    var backend = new ProbeBackend();
    var program = HixCompiler.CompileFunctions(["""
      pure func identity { return(param) }
      pure func resolved (number value) -> number { return($value) }
      pure func main { local current = [param]; return(probe(identity)) }
      """], backend);
    var machine = new HixVM([program]);
    var thread = backend.CreateCoroutine();
    backend.Probe = active => {
      Assert.Same(thread, active);
      var candidate = Assert.Single(program.Scope.Candidates("resolved"));
      var reference = new ResolvedFunctionHixValue(candidate.Signature.Constant("resolved"), candidate);
      Assert.Equal(17, Assert.IsType<NumberHixValue>(active.Invoke(reference, [new NumberHixValue(17)])).Value);
      Assert.True(active.IsRunning);
      Assert.Equal(42d, active.Resolve(HixExpressionRoot.Parameter, HixString.Dynamic("")).Unlink(active));
      Assert.Equal(42d, active.Resolve(HixExpressionRoot.Local, HixString.Dynamic("current")).Unlink(active));
      Assert.Throws<InvalidOperationException>(() => machine.Invoke(program, active));
      var competing = new HixCoroutine(active.Context);
      Assert.Throws<InvalidOperationException>(() => machine.Invoke(program, competing));
      Assert.False(competing.IsRunning);
      Assert.True(active.IsRunning);
    };
    var result = machine.Invoke(program, thread, "main", new NumberHixValue(42));
    Assert.True(result.Success, result.Error.Resolve(result.Strings));
    Assert.Equal(42, Assert.IsType<NumberHixValue>(result.Value).Value);
    Assert.False(thread.IsRunning);
    Assert.IsType<MissingHixValue>(thread.Resolve(HixExpressionRoot.Local, HixString.Dynamic("current")));
  }

  [Fact]
  public void ContextRetainsTargetDataAcrossFreshThreadsAndFailedRuns() {
    var program = TestCompiler.Compile("""
      mixin Example { prelude expression {
        emit(tar#saved)
        tar<saved> @= [this];
        emit(this)
      } }
      """, "Example");
    var context = new TargetContext("first");
    var first = HixVM.Execute(program, new HixCoroutine(context));
    var second = HixVM.Execute(program, new HixCoroutine(context));
    var other = HixVM.Execute(program, new HixCoroutine(new TargetContext("other")));
    Assert.True(first.Success, first.Error.Resolve(first.Strings));
    Assert.True(second.Success, second.Error.Resolve(second.Strings));
    Assert.True(other.Success, other.Error.Resolve(other.Strings));
    Assert.Equal(new[] {"missing", "first"}, first.Outputs.Select(output => output.ReadText()));
    Assert.Equal(new[] {"first", "first"}, second.Outputs.Select(output => output.ReadText()));
    Assert.Equal(new[] {"missing", "other"}, other.Outputs.Select(output => output.ReadText()));

    var failing = TestCompiler.Compile("mixin Example { expression { tar<saved> @= <discarded>; fail<stop> } }", "Example");
    Assert.False(HixVM.Execute(failing, new HixCoroutine(context)).Success);
    Assert.Equal("first", Assert.IsType<LiteralHixValue>(context.TargetVariables[HixString.Dynamic("saved")]).Value.DynamicValue);
  }

  [Fact]
  public void ContextSeedStringsAreDetachedBeforeLoadingAnotherProgramPool() {
    var strings = new HixStringPoolBuilder();
    var key = strings.Intern("seed");
    var value = strings.Intern("host value");
    var context = new HixExpressionContext(TestBackend.Instance, strings.Freeze());
    context.TargetVariables.StoreIsolated(key, new LiteralHixValue(value));
    var program = TestCompiler.Compile("mixin Example { expression { emit(tar#seed) } }", "Example");
    for (var i = 0; i < 2; i++) {
      var result = HixVM.Execute(program, new HixCoroutine(context));
      Assert.True(result.Success, result.Error.Resolve(result.Strings));
      Assert.Equal("host value", Assert.Single(result.Outputs).ReadText());
    }
  }

  private sealed class TargetContext(string name) : HixExpressionContext(TestBackend.Instance) {
    public override IHixValue ResolveHost(HixCoroutine thread, HixExpressionRoot root, HixString member) =>
      new LiteralHixValue(HixString.Dynamic(name));
  }

  private sealed class ProbeBackend : HixBackend {
    internal Action<HixCoroutine> Probe;
    protected override void RegisterFunctions(FunctionSignatureRegistryBuilder functions) {
      base.RegisterFunctions(functions);
      functions.AddLibrary("pure func probe(any callback) -> any external;");
      functions.Add(new ProbeFunction(this));
    }
  }

  private sealed class ProbeFunction(ProbeBackend backend) : FunctionDefinition("probe") {
    public override IHixValue Execute(HixCoroutine thread, IHixValue[] arguments, int line) {
      backend.Probe(thread);
      return thread.Invoke(arguments[0], [thread.Resolve(HixExpressionRoot.Parameter, HixString.Dynamic(""))], line);
    }
  }
}
