// Port of Compose.hix: boundary elements, composable methods and input handlers.
//
// The $Compose / $Recompose / $Reset targets are *per-type* aliases: a custom boundary element
// and a boundary mixin define the same alias as different methods on the type they apply to.
// That is what DefineTarget exists for.
using System;
using Helix.Weave;

namespace HELIX.Weave;

public static class Boundary {
  /// <summary>Targets for a boundary element that owns its lifecycle.</summary>
  public static void DefineOwnTargets(TypeTarget t) {
    t.DefineTarget(Targets.Compose, new InjectionTarget("PerformCompose", Types.Composable));
    t.DefineTarget(Targets.Recompose, new InjectionTarget("Recompose", isPublic: true));
    t.DefineTarget(Targets.Reset, new InjectionTarget("Reset", isPublic: true));
    t.DefineTarget(Targets.BoundaryInit, new InjectionTarget("Init"));
    t.DefineTarget(Targets.BoundaryDispose, new InjectionTarget("Dispose", isPublic: true));
    t.DefineTarget(Targets.PostConstruct, new InjectionTarget("PostConstruct", isPublic: true));
    t.DefineTarget(Targets.AttachBoundary, new InjectionTarget("AttachBoundary"));
    t.DefineTarget(Targets.DetachBoundary, new InjectionTarget("DetachBoundary"));
  }

  /// <summary>Targets for an element that inherits its lifecycle from GeneratedBoundaryBase.</summary>
  public static void DefineMixinTargets(TypeTarget t) {
    t.DefineTarget(Targets.BoundaryInit, new InjectionTarget("MixinInit", isPublic: true));
    t.DefineTarget(Targets.Compose, new InjectionTarget("MixinCompose", Types.Composable, isPublic: true));
    t.DefineTarget(Targets.Recompose, new InjectionTarget("MixinRecompose", Types.Composable, isPublic: true));
    t.DefineTarget(Targets.Reset, new InjectionTarget("MixinReset", isPublic: true));
    t.DefineTarget(Targets.BoundaryDispose, new InjectionTarget("MixinDispose", isPublic: true));
    t.DefineTarget(Targets.PostConstruct, new InjectionTarget("MixinPostConstruct", isPublic: true));
  }

  /// <summary>Port of <c>GenerateBoundaryElement</c>.</summary>
  public static void GenerateElement(TypeTarget t, string name, bool extension) {
    Shared.RequireCompositionTypeId(t);
    Shared.RequireCompositionId(t);
    var companion = Shared.Companion(t);
    var self = t.TypeRef.Qualified;

    Shared.DeclareCompanion(t, """
      public static readonly CompositionId fullId = new() {
        composition = compositionId,
        type = typeId,
        local = LocalId.Initial
      };
      """);

    var composeArgs = "";
    var composeCalls = "";
    var props = t.Nested("Props");

    if (props is not null) {
      var model = props.DeriveFields();
      if (model.Length > 0) {
        var parameters = Code.Join(model, p => p.Parameter);
        var arguments = Code.Join(model, p => p.Argument);

        t.Class($$"""
          public partial struct Props {
            public Props({{parameters}}) {
              {{Code.Join(model, p => p.Assignment, "\n    ")}}
            }
          }
          """);

        t.Implements($"IProps<{self}.Props>");
        t.Class($$"""
          private Props _props;
          public ref Props props => ref _props;
          public void ReceiveProps(in Props props) => _props = props;
          """);

        t.Inject(Targets.Reset, 1, "_props = default;");
        composeArgs = ", " + parameters;
        composeCalls = $$"""
          var props = new {{self}}.Props({{arguments}});
            instance.ReceiveProps(in props);
          """;
      }
    }

    if (composeArgs.Length == 0)
      t.Class("public static readonly Composable BakedComposable = " +
              "static (ref Composition cx) => { ComposeBoundary(ref cx); };");

    var body = $$"""
      {
        if (!cx.AUTHORING.RequireComposable<{{self}}>({{companion}}.typeId, out var instance, out var retained)) {
          instance = new {{self}}();
        }
        {{composeCalls}}
        return ref cx.AUTHORING.YieldBoundary(ref cx, instance);
      }
      """;

    t.Class($$"""
      public static ref ElementRef ComposeBoundary(
        ref Composition cx{{composeArgs}}
      ) {{body}}
      """);

    if (extension)
      Shared.DeclareCompanion(t, $$"""
        public static ref ElementRef {{name ?? t.SelfName}}(
          ref this Composition cx{{composeArgs}}
        ) {{body}}
        """);

    t.Inject(Targets.Recompose, -24, $"cx.AUTHORING.SetId({companion}.fullId);");
  }
}
