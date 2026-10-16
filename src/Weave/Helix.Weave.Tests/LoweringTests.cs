using System.Linq;
using Xunit;

namespace Helix.Weave.Tests;

/// <summary>
/// What lowering rejects. These are the errors that stop a mistake from becoming a silent
/// codegen bug or an incremental dependency on analyzed source.
/// </summary>
public class LoweringTests {
  private const string Preamble = """
    using System;
    using System.Collections.Immutable;
    using Helix.Weave;

    namespace Sample {
    """;

  private static Helix.Weave.Lowering.LoweringResult LowerMixin(string body) =>
    Harness.Lower(("Sample.cs", Preamble + body + "\n}\n"));

  private static void AssertError(Helix.Weave.Lowering.LoweringResult result, string fragment) {
    Assert.False(result.Success, "expected lowering to fail");
    Assert.Contains(result.Errors, error => error.Message.Contains(fragment));
  }

  [Fact]
  public void AcceptsASinglePassMixin() {
    var result = LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class MarkAttribute : Attribute {
        public string Note { get; init; }
        [Weave]
        static void Apply(TypeTarget t, MarkAttribute a) => t.Class($"// {Lit.Of(a.Note)}");
      }
      """);
    Assert.True(result.Success, string.Join("\n", result.Errors));
    Assert.Equal("Sample.MarkAttribute", result.Mixins.Single().AttributeMetadataName);
  }

  [Fact]
  public void RejectsAMixinWithNoWeaveMethod() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class EmptyAttribute : Attribute { }
      """), "has no [Weave] or [Prelude] method");

  [Fact]
  public void RejectsBothSingleAndTwoPassForms() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class ConfusedAttribute : Attribute {
        public sealed record Carry(string Text);
        [Weave] static void Apply(TypeTarget t, ConfusedAttribute a) { }
        [Prelude] static Carry Begin(TypeTarget t, ConfusedAttribute a) => new Carry("x");
        [Late] static void End(LateTarget t, Carry c) { }
      }
      """), "declares both [Weave] and [Prelude]");

  [Fact]
  public void RejectsALatePassWithNoPrelude() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class OrphanAttribute : Attribute {
        public sealed record Carry(string Text);
        [Late] static void End(LateTarget t, Carry c) { }
      }
      """), "no [Prelude] to feed it");

  [Fact]
  public void RejectsAPreludeCarryNobodyReceives() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class DanglingAttribute : Attribute {
        public sealed record Carry(string Text);
        [Prelude] static Carry Begin(TypeTarget t, DanglingAttribute a) => new Carry("x");
      }
      """), "no [Late] pass to receive it");

  [Fact]
  public void RejectsALatePassThatAsksForTheAttribute() =>
    // The whole point of the boundary: the late pass may not re-read analyzed source.
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class LeakyAttribute : Attribute {
        public sealed record Carry(string Text);
        [Prelude] static Carry Begin(TypeTarget t, LeakyAttribute a) => new Carry("x");
        [Late] static void End(LateTarget t, Carry c, LeakyAttribute a) { }
      }
      """), "may not see the attribute or any symbol");

  [Fact]
  public void RejectsALatePassThatTakesASymbolTarget() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class AttachedTargetAttribute : Attribute {
        public sealed record Carry(string Text);
        [Prelude] static Carry Begin(TypeTarget t, AttachedTargetAttribute a) => new Carry("x");
        [Late] static void End(TypeTarget t, Carry c) { }
      }
      """), "must take (LateTarget, <carry>)");

  [Fact]
  public void RejectsACarryHoldingASymbol() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class SymbolCarryAttribute : Attribute {
        public sealed record Carry(Microsoft.CodeAnalysis.ITypeSymbol Type);
        [Prelude] static Carry Begin(TypeTarget t, SymbolCarryAttribute a) => new Carry(t.Type);
        [Late] static void End(LateTarget t, Carry c) { }
      }
      """), "holds attached state");

  [Fact]
  public void RejectsACarryHoldingABackedTypeRef() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class BackedRefCarryAttribute : Attribute {
        public sealed record Carry(TypeRef Type);
        [Prelude] static Carry Begin(TypeTarget t, BackedRefCarryAttribute a) => new Carry(t.TypeRef);
        [Late] static void End(LateTarget t, Carry c) { }
      }
      """), "holds attached state");

  [Fact]
  public void RejectsACarryHidingASymbolInsideACollection() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class NestedLeakAttribute : Attribute {
        public sealed record Carry(ImmutableArray<Microsoft.CodeAnalysis.ITypeSymbol> Types);
        [Prelude] static Carry Begin(TypeTarget t, NestedLeakAttribute a) =>
          new Carry(ImmutableArray<Microsoft.CodeAnalysis.ITypeSymbol>.Empty);
        [Late] static void End(LateTarget t, Carry c) { }
      }
      """), "holds attached state");

  [Fact]
  public void RejectsACarryHidingASymbolInANestedRecord() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class DeepLeakAttribute : Attribute {
        public sealed record Inner(TypeRef Type);
        public sealed record Carry(Inner Nested);
        [Prelude] static Carry Begin(TypeTarget t, DeepLeakAttribute a) => new Carry(new Inner(t.TypeRef));
        [Late] static void End(LateTarget t, Carry c) { }
      }
      """), "holds attached state");

  [Fact]
  public void AcceptsACarryOfDetachedValues() {
    var result = LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class PureCarryAttribute : Attribute {
        public sealed record Carry(string Name, PureTypeRef Type, ImmutableArray<PureTypeRef> Args);
        [Prelude] static Carry Begin(TypeTarget t, PureCarryAttribute a) =>
          new Carry(t.Name, t.TypeRef.Pure(), ImmutableArray<PureTypeRef>.Empty);
        [Late] static void End(LateTarget t, Carry c) => t.Class($"// {c.Name}");
      }
      """);
    Assert.True(result.Success, string.Join("\n", result.Errors));
  }

  [Fact]
  public void AcceptsADerivationStep() {
    var result = LowerMixin("""
      [Mixin(Unit.Derivation, Order = 5)]
      public sealed class TweakAttribute : Attribute {
        public string Default { get; init; }
        [Weave]
        static DerivedProperty Derive(DerivationTarget t, TweakAttribute a, DerivedProperty p) =>
          p with { Default = a.Default, Required = false };
      }
      """);
    Assert.True(result.Success, string.Join("\n", result.Errors));
    Assert.Equal(Unit.Derivation, result.Mixins.Single().Unit);
    Assert.Equal(5, result.Mixins.Single().Order);
  }

  [Fact]
  public void RejectsADerivationStepWithTheWrongShape() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Derivation)]
      public sealed class WrongShapeAttribute : Attribute {
        [Weave] static void Derive(DerivationTarget t, WrongShapeAttribute a) { }
      }
      """), "must be DerivedProperty (DerivationTarget, <attribute>, DerivedProperty)");

  [Fact]
  public void RejectsADerivationStepThatAsksForTwoPasses() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Derivation)]
      public sealed class TwoPassDerivationAttribute : Attribute {
        public sealed record Carry(string Text);
        [Prelude] static Carry Begin(DerivationTarget t, TwoPassDerivationAttribute a) => new Carry("x");
        [Late] static void End(LateTarget t, Carry c) { }
      }
      """), "derivation step, so it declares a single [Weave] method");

  [Fact]
  public void ReportsAuthoringCompileErrorsRatherThanGuessing() =>
    AssertError(LowerMixin("""
      [Mixin(Unit.Type)]
      public sealed class BrokenAttribute : Attribute {
        [Weave] static void Apply(TypeTarget t, BrokenAttribute a) => t.Nonexistent("x");
      }
      """), "authoring project does not compile");
}
