using System.Linq;
using Hix;
using Hix.Compiler;
using Hix.Mixins;
using Hix.Runtime;
using Xunit;

namespace HELIX.SourceGen.Tests;

public sealed class MixinOutputTests {
  [Fact]
  public void ConcreteScalarOverloadsBindAndExecuteStatically() {
    var backend = HixMixinBackend.Instance;
    var image = HixCompiler.Compile("""
      mixin Test {
        prelude expression {
          emit(<Class>, csharpLiteral(null))
          emit(<Class>, csharpLiteral(<hello>))
          emit(<Class>, csharpLiteral(12))
          emit(<Class>, csharpLiteral(true))
          emit(<Class>, floatTime(2))
          emit(<Class>, floatTime<2ms>)
          emit(<Class>, floatTime(null))
        }
      }
      """, "Test", backend);
    var thread = backend.CreateCoroutine();
    Assert.True(HixVM.Execute(image, thread).Success);
    var context = Assert.IsType<MixinOutputContext>(thread.Context);
    Assert.Equal(new[] {"null", "\"hello\"", "12", "true", "2f", "0.002f", "-0f"},
      context.Emissions.Outputs.Select(output => output.ResolveText()));
  }

  [Fact]
  public void GeneratorEmissionsRollbackWithCallsAndExpressions() {
    var backend = HixMixinBackend.Instance;
    var program = HixCompiler.Compile("""
      func broken() -> any { inject(<discarded>, <body>); fail<bad> }
      mixin Test {
        prelude expression {
          inject(<kept>, <before>)
          local failure = [broken()?]
          assert(is(local#failure, error))
          emit(<CLASS>, <class-body>)
          inject(<last>, 2, <after>)
        }
        expression { inject(<doomed>, <body>); fail<stop> }
      }
      """, "Test", backend);
    var thread = backend.CreateCoroutine();
    var context = Assert.IsType<MixinOutputContext>(thread.Context);
    for (var run = 0; run < 2; run++) {
      var result = HixVM.Execute(program, thread);
      Assert.False(result.Success);
      Assert.Empty(result.Outputs);
      Assert.Equal(new[] {"before", "class-body", "after"}, context.Emissions.Outputs.Select(output => output.ResolveText()));
      Assert.Equal(MixinEmissionTarget.Class, context.Emissions.Outputs[1].Target);
      Assert.Equal("last", context.Emissions.Outputs[2].ResolveInjectionTarget());
      Assert.Equal(2, context.Emissions.Outputs[2].InjectionPriority);
    }
  }
}
