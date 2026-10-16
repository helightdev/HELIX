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

[Mixin(Unit.Method, Attribute = "HELIX.Compose.ComposableMethodAttribute")]
public sealed class ComposableMethod : Attribute {
  public ComposableMethod(bool requiresTracking = false, bool scope = false, bool extension = false,
                          string name = null, string scopeCallback = "cell.TrimChildren()") {
    RequiresTracking = requiresTracking;
    Scope = scope;
    Extension = extension;
    Name = name;
    ScopeCallback = scopeCallback;
  }

  public bool RequiresTracking { get; init; }
  public bool Scope { get; init; }
  public bool Extension { get; init; }
  public string Name { get; init; }
  public string ScopeCallback { get; init; } = "cell.TrimChildren()";

  [Weave]
  static void Apply(MethodTarget t, ComposableMethod a) {
    t.Using("HELIX.Compose");
    Shared.RequireCompositionTypeId(t);

    var props = t.DeriveParameters();
    var arguments = Code.Join(props, p => p.Argument);
    var parameters = Code.Join(props, p => p.Parameter);
    var parameterSuffix = parameters.Length == 0 ? "" : ", " + parameters;

    var self = t.SelfName;
    var companion = Shared.Companion(t);
    var require = a.RequiresTracking ? "RequireTracked" : "RequireComposable";
    var returnType = a.Scope ? "ScopeHandle" : "ref ElementRef";
    var returnStatement = a.Scope
      ? $"return cx.AUTHORING.YieldScope(ref cx, instance, " +
        $"static (BoundaryCell cell, in ScopeHandle handle) => {a.ScopeCallback});"
      : "return ref cx.AUTHORING.YieldElement(ref cx, instance);";

    var body = $$"""
      if (!cx.AUTHORING.{{require}}<{{self}}>({{companion}}.typeId, out var instance, out var retained)) {
          instance = new {{self}}();
        }
        instance.{{t.Name}}({{arguments}});
        {{returnStatement}}
      """;

    if (a.Extension) {
      Shared.DeclareCompanion(t, $$"""
        public static {{returnType}} {{a.Name ?? t.SelfName}}(
          ref this Composition cx{{parameterSuffix}}
        ) {
          {{body}}
        }
        """);
      return;
    }

    t.Class($$"""
      public static {{returnType}} Compose(
        ref Composition cx{{parameterSuffix}}
      ) {
        {{body}}
      }
      """);
  }
}

[Mixin(Unit.Method, Attribute = "HELIX.Compose.ComposableDelegateAttribute")]
public sealed class ComposableDelegate : Attribute {
  public ComposableDelegate(string name = null) => Name = name;
  public string Name { get; init; }

  [Weave]
  static void Apply(MethodTarget t, ComposableDelegate a) {
    t.Using("HELIX.Compose");
    var name = a.Name ?? (t.Name.StartsWith("_", StringComparison.Ordinal) ? t.Name.Substring(1) : t.Name);

    // Parameter 0 is always the composition; up to three more become the delegate's arguments.
    var args = "ref Composition cx";
    var callArgs = "ref cx";
    var types = "";

    for (var index = 1; index < t.Params.Length && index <= 3; index++) {
      var p = t.Param(index);
      args += $", {p.Type} {p.Name}";
      callArgs += $", {p.Name}";
      types += types.Length == 0 ? p.Type.ToString() : $", {p.Type}";
    }

    var typeArguments = types.Length == 0 ? "" : $"<{types}>";
    var idName = $"_{name}CompositionId";

    t.Class($$"""
      private static readonly ushort {{idName}} = CompositionId.GetCompositionId({{Lit.Of(name)}});

      public static readonly Composable{{typeArguments}} {{name}} = static ({{args}}) => {
        var transfer = new CompositionInternals.TransferData();
        CompositionInternals.EnterComposition(ref cx, {{idName}}, ref transfer);
        try {
          {{t.Name}}({{callArgs}});
        } finally {
          CompositionInternals.ExitComposition(ref cx, ref transfer);
        }
      };
      """);
  }
}

[Mixin(Unit.Type, Order = -1000, Attribute = "HELIX.Compose.CustomBoundaryElementAttribute")]
public sealed class CustomBoundaryElement : Attribute {
  public CustomBoundaryElement(bool constructor = true, bool composable = true, bool extension = false,
                               bool cacheLookups = false, bool trimChildren = true, string name = null) {
    Constructor = constructor;
    Composable = composable;
    Extension = extension;
    CacheLookups = cacheLookups;
    TrimChildren = trimChildren;
    Name = name;
  }

  public bool Constructor { get; init; } = true;
  public bool Composable { get; init; } = true;
  public bool Extension { get; init; }
  public bool CacheLookups { get; init; }
  public bool TrimChildren { get; init; } = true;
  public string Name { get; init; }

  [Weave]
  static void Apply(TypeTarget t, CustomBoundaryElement a) {
    Boundary.DefineOwnTargets(t);
    if (a.Composable) Boundary.GenerateElement(t, a.Name, a.Extension);

    t.Using("HELIX.Compose", "HELIX.Coloring");
    if (!t.IsA(Types.VisualElement)) t.Extends($"global::{Types.VisualElement}");
    t.Implements("IBoundary", "IRecomposeMixinTargets");

    t.Class($$"""
      public global::{{Types.VisualElement}} Element => this;
      public BoundaryCell Cell { get; } = BoundaryCell.Shared;
      public int TreeDepth { get; set; }
      public IBoundary Parent { get; set; }
      public IContextComposable ContextParent { get; set; }
      public bool IsDisposed { get; set; }
      public UssFlag Flag { get; set; }
      public ulong PackedId { get; set; }
      public SparseContextMap WrittenContext { get; set; }

      private LookupCache _lookupCache;
      private bool _initialAttachment = true;
      private bool _initialized = false;

      public bool TryLookupContext(int key, out ContextData data) {
        data = null;
        if (WrittenContext != null && WrittenContext.TryGet(key, out data)) return true;
        return _lookupCache.TryLookup(ContextParent, key, out data);
      }

      public void RefreshHierarchy() {
        Parent = GetFirstAncestorOfType<IBoundary>();
        ContextParent = GetFirstAncestorOfType<IContextComposable>();
        TreeDepth = BoundaryHelper.GetDepth(this);
      }

      public void MarkDirty() => HXComposer.MarkDirty(this, false);
      """);

    // The recompose ladder. The priorities are the composition mechanism; keep them exactly.
    t.Inject(Targets.Recompose, -1000, "try {");
    t.Inject(Targets.Recompose, -100, "BoundaryHelper.ContextBefore(this);");
    t.Inject(Targets.Recompose, -75, "_lookupCache.Clear();");
    t.Inject(Targets.Recompose, -60, "if (!_initialized) Init();");
    if (a.CacheLookups) t.Inject(Targets.Recompose, -55, "_lookupCache.Claim();");
    t.Inject(Targets.Recompose, -50, "try {");
    t.Inject(Targets.Recompose, -25, "var cx = new Composition(this);");
    t.Inject(Targets.Recompose, 0, "PerformCompose(ref cx);");
    t.Inject(Targets.Recompose, 50,
      "} catch (global::System.Exception e) { global::UnityEngine.Debug.LogException(e); }");
    t.Inject(Targets.Recompose, 100, "BoundaryHelper.ContextAfter(this);");
    if (a.TrimChildren) t.Inject(Targets.Recompose, 200, "Cell.TrimChildren();");
    t.Inject(Targets.Recompose, 1000,
      "} catch (global::System.Exception e) { global::UnityEngine.Debug.LogException(e); }");

    t.Inject(Targets.Reset, -1, "HXComposer.RemoveDirty(this);");
    t.Inject(Targets.Reset, 1, "_lookupCache.Release();");
    t.Inject(Targets.Reset, 100, "_initialized = false;");
    t.Inject(Targets.Compose, 0, "// Default handle");
    t.Inject(Targets.BoundaryInit, 100, "_initialized = true;");
    t.Inject(Targets.BoundaryDispose, -100,
      "if (IsDisposed) return; IsDisposed = true; _initialized = false;");
    t.Inject(Targets.BoundaryDispose, -1, "Reset();");
    t.Inject(Targets.PostConstruct, 1000, """
      if (HXComposer.IsProcessing) {
        _initialAttachment = false;
        HXComposer.MarkDirty(this);
      }
      """);

    t.Inject(Targets.DetachBoundary, 0, """
      _initialAttachment = true;
      _lookupCache.Release();
      HXComposer.NotifyDetach(this);
      """);
    t.Inject(Targets.AttachBoundary, 0, """
      IsDisposed = false;
      RefreshHierarchy();
      """);
    t.Inject(Targets.AttachBoundary, 100, "HXComposer.RegisterActiveBoundary(this);");
    t.Inject(Targets.AttachBoundary, 200, """
      if (_initialAttachment) {
        _initialAttachment = false;
        HXComposer.MarkDirty(this);
      }
      """);

    if (a.Constructor)
      t.Class($$"""
        public {{t.SelfName}}() {
          RegisterCallback<global::UnityEngine.UIElements.AttachToPanelEvent>(_ => AttachBoundary());
          RegisterCallback<global::UnityEngine.UIElements.DetachFromPanelEvent>(_ => DetachBoundary());
          PostConstruct();
        }
        """);
  }
}

[Mixin(Unit.Type, Order = -1000, Attribute = "HELIX.Compose.BoundaryElementMixinAttribute")]
public sealed class BoundaryElementMixin : Attribute {
  public BoundaryElementMixin(bool composable = true, bool extension = false, bool cacheLookups = false,
                              bool trimChildren = true, string name = null) {
    Composable = composable;
    Extension = extension;
    CacheLookups = cacheLookups;
    TrimChildren = trimChildren;
    Name = name;
  }

  public bool Composable { get; init; } = true;
  public bool Extension { get; init; }
  public bool CacheLookups { get; init; }
  public bool TrimChildren { get; init; } = true;
  public string Name { get; init; }

  public sealed record Carry(bool Placeholder);

  [Prelude]
  static Carry Prelude(TypeTarget t, BoundaryElementMixin a) {
    Boundary.DefineMixinTargets(t);
    t.Extends("HELIX.Compose.GeneratedBoundaryBase");
    t.Class($"public {t.SelfName}() : base({Lit.Of(a.CacheLookups)}, {Lit.Of(a.TrimChildren)}) {{}}");
    if (a.Composable) Boundary.GenerateElement(t, a.Name, a.Extension);
    return new Carry(true);
  }

  [Late]
  static void Late(LateTarget t, Carry c) {
    t.Using("HELIX.Compose", "HELIX.Coloring");
    t.Implements("HELIX.Compose.IBoundaryHooks");
    t.Inject(Targets.Recompose, 0, "((IBoundaryHooks)this).MixinCompose(ref cx);");
  }
}

[Mixin(Unit.Method, Attribute = "HELIX.Compose.WriteContextHandlerAttribute")]
public sealed class WriteContextHandler : Attribute {
  [Weave]
  static void Apply(MethodTarget t, WriteContextHandler a) {
    if (!Shared.ClaimWriteContext(t)) {
      t.Report("HX0400", "a type can declare only one write-context handler");
      return;
    }
    t.Inject(Targets.Compose, -1, $$"""
      using (cx.WriteContext(out var context)) {
        {{t.Name}}(ref cx, context);
      }
      """);
  }
}

[Mixin(Unit.Method, Attribute = "HELIX.Compose.ClickHandlerAttribute")]
public sealed class ClickHandler : Attribute {
  [Weave]
  static void Apply(MethodTarget t, ClickHandler a) {
    Shared.RequireInputState(t);
    t.Inject(Targets.PostConstruct, 0, $$"""
      global::UnityEngine.UIElements.VisualElementExtensions.AddManipulator(this,
        new InputClickableManipulator(this, this, {{t.Name}})
      );
      """);
  }
}

[Mixin(Unit.Type, Attribute = "HELIX.Compose.InputStateListenerAttribute")]
public sealed class InputStateListener : Attribute {
  public InputStateListener(bool focus = true, bool hover = true, bool dirty = true) {
    Focus = focus;
    Hover = hover;
    Dirty = dirty;
  }

  public bool Focus { get; init; } = true;
  public bool Hover { get; init; } = true;
  public bool Dirty { get; init; } = true;

  [Weave]
  static void Apply(TypeTarget t, InputStateListener a) {
    Shared.RequireInputState(t);
    t.Inject(Targets.PostConstruct, 0, $$"""
      global::UnityEngine.UIElements.VisualElementExtensions.AddManipulator(this,
        new InputListenerManipulator(this, this, {{Lit.Of(a.Focus)}})
      );
      """);

    if (a.Dirty)
      t.Inject(new InjectionTarget("OnStateChanged", Types.StateChangedHandler, isPublic: true), 0,
        "this.MarkDirty();");
  }
}
