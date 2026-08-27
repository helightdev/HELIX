using System;

namespace HELIX.Compose {
  [MixinLibrary(
    @"
@FUNC<BoundaryComposableImpl>
  @USING HELIX.Compose;
  @USING HELIX.Coloring;
  @LOCAL<Name> @target:name

  @SCOPE
    @MATCH @attr#name:!?eq<null>
    @LOCAL<Name> @attr#name:unwrap
  @END

  @SCOPE<ResolveDefaultName>
    @MATCH @local#Name:?eq<null>
    @LOCAL<Name> @this:name
  @END

  @LOCAL<ExtensionName> @(local#Name)Extensions

  @AUGMENT_STRUCT<PropsModel> @this#Props

  @SCOPE<DefaultBase>
    @MATCH @attr#super:?eq<null>
    @CODE<IMPLEMENTS> PropsBoundaryComposable<@this#Props:type>
  @END

  @SCOPE<CustomBase>
    @MATCH @attr#super:!?eq<null>
    @CODE<IMPLEMENTS> @attr#super:makeGeneric<(@this#Props:type)>
  @END

  @CODE<IMPLEMENTS> IRecomposeMixinTargets

  @CODE<CLASS> public static ref ElementRef ComposeBoundary(
    @\ @local#PropsModel:structParams<ref Composition cx>
    @\) {
    @\  cx.AUTHORING.PropsBoundaryStateComposable<@this:type, Props>(
    @\    @local#ExtensionName.typeId, out var node, out _, out var attachment
    @\  );
    @\  var props = new Props(@local#PropsModel:structArgs);
    @\  attachment.ReceiveProps(in props);
    @\  node.composable = null;
    @\  return ref cx.AUTHORING.YieldBoundary(ref cx, node);
    @\}

  @SCOPE<NoArgs>
    @MATCH @local#PropsModel:?structNoArgs
    @CODE<CLASS> public static readonly Composable BakedComposable = static (ref Composition cx) => { ComposeBoundary(ref cx); };
  @END

  @CODE<CLASS> public override void OnRecompose(
    @\ ref Composition cx, BoundaryData state, IBoundary boundary
    @\) { 
    @\  var transfer = new CompositionInternals.TransferData();
    @\  CompositionInternals.EnterComposition(ref cx, @local#ExtensionName.compositionId, ref transfer);
    @\  try { 
    @\    @attr#cacheLookups:switch<boundary.UseLookupCache();><>
    @\    ((IRecomposeMixinTargets)this).MixinRecompose(ref cx);
    @\    base.OnRecompose(ref cx, state, boundary);
    @\  } finally { 
    @\    CompositionInternals.ExitComposition(ref cx, ref transfer); 
    @\  } 
    @\}

  @CODE<CLASS> public override void OnAttach(BoundaryData data, IBoundary boundary) {
    @\  base.OnAttach(data, boundary);
    @\  ((IRecomposeMixinTargets)this).MixinInit();
    @\}

  @CODE<CLASS> public override void OnDetach(BoundaryData data, IBoundary boundary) {
    @\  base.OnDetach(data, boundary);
    @\  ((IRecomposeMixinTargets)this).MixinReset();
    @\  ((IRecomposeMixinTargets)this).MixinDispose();
    @\}

  @CODE<FILE> @this:visibility static class @local#ExtensionName { 
    @\  public static readonly ushort compositionId = CompositionId.GetCompositionId(""@local#Name"");
    @\  public static readonly ushort typeId = CompositionId.GetTypeId(""@local#Name""); 
    @\}

  @SCOPE<Extension>
    @MATCH @attr#extension
    @CODE<FILE> @this:visibility static class @(local#Name)CompositionExtensions {
      @\  public static ref ElementRef @local#Name(
      @\    @local#PropsModel:structParams<ref this Composition cx>
      @\  ) => ref @this:type.ComposeBoundary(
      @\    @local#PropsModel:structArgs<ref cx>
      @\  );
      @\}
  @END
@END

@FUNC<RequireCompositionTypeId>
  @SCOPE
    @MATCH @var#HasTypeId:!?eq<true>
    @VAR<HasTypeId> true
    @CALL<DeclareCompanion> public static readonly ushort typeId = CompositionId.GetTypeId(""@this:name"");
  @END
@END

@FUNC<RequireCompositionId>
  @SCOPE
    @MATCH @var#HasCompositionId:!?eq<true>
    @VAR<HasCompositionId> true
    @CALL<DeclareCompanion> public static readonly ushort compositionId = CompositionId.GetCompositionId(""@this:name"");
  @END
@END


@FUNC<ComposableMethodImpl>
  @USING HELIX.Compose;

  @LOCAL<StructName> @(target:name)Props
  @PROP_STRUCT<(@local#StructName)><StructHandle><noGenerate> @target

  @LOCAL<RequireMethod> RequireComposable
  @LOCAL<ReturnStatement> return ref cx.AUTHORING.YieldElement(ref cx, instance);
  @LOCAL<ReturnType> ref ElementRef
  @LOCAL<Name> @this:name

  @SCOPE
    @MATCH @attr#requiresTracking:?eq<true>
    @LOCAL<RequireMethod> RequireTracked
  @END

  @SCOPE
    @MATCH @attr#name:!?eq<null>
    @LOCAL<Name> @attr#name:unwrap
  @END

  @SCOPE
    @MATCH @attr#scope:?eq<true>
    @LOCAL<ReturnType> ScopeHandle
    @LOCAL<ReturnStatement> return cx.AUTHORING.YieldScope(
      @+ref cx, instance, static (BoundaryCell cell, in ScopeHandle handle) => @attr#scopeCallback:unwrap
      @+);
  @END

  @CALL<RequireCompositionTypeId>
 
  @LOCAL<Body> if (!cx.AUTHORING.@(local#RequireMethod)<@this:name>(@(var#CompanionName).typeId, out var instance, out var retained)) {
    @\    instance = new @this:name();
    @\  }
    @\  instance.@target:name(@local#StructHandle:structArgs);
    @\  @local#ReturnStatement

  @SCOPE
    @MATCH @attr#extension:?eq<true>
    @CALL<DeclareCompanion> 
      @\  public static @local#ReturnType @(local#Name)(
      @\    @local#StructHandle:structParams<ref this Composition cx>
      @\  ) { 
      @\    @local#Body
      @\  }
    @RETURN
  @SCOPE
    @CODE<CLASS> public static @local#ReturnType Compose(
      @\ @local#StructHandle:structParams<ref Composition cx>
      @\) {
      @\  @local#Body
      @\}
  @END
@END

@FUNC<ComposableDelegateImpl>
  @USING HELIX.Compose;
  @LOCAL<Name> @target:name:replaceFirst<^_><>
  @SCOPE
    @MATCH @attr#name:!?eq<null>
    @LOCAL<Name> @attr#name:unwrap
  @END

  @LOCAL<Args> ref Composition cx
  @LOCAL<CallArgs> ref cx
  @LOCAL<Types>

  @SCOPE
    @MATCH<NoArgs> @arg#1:?exists
    @LOCAL<Args> @local#Args, @arg#1:type:unwrap @arg#1:name
    @LOCAL<CallArgs> @local#CallArgs, @arg#1:name
    @LOCAL<Types> @arg#1:type:unwrap
  @SCOPE
    @MATCH @arg#2:?exists
    @LOCAL<Args> @local#Args, @arg#2:type:unwrap @arg#2:name
    @LOCAL<CallArgs> @local#CallArgs, @arg#2:name
    @LOCAL<Types> @local#Types, @arg#2:type:unwrap
  @SCOPE
    @MATCH @arg#3:?exists
    @LOCAL<Args> @local#Args, @arg#3:type:unwrap @arg#3:name
    @LOCAL<CallArgs> @local#CallArgs, @arg#3:name
    @LOCAL<Types> @local#Types, @arg#3:type:unwrap
  @END
  @LOCAL<Types> <@local#Types>
  @SCOPE<NoArgs>
  @END
  @LOCAL<IdName> _@(local#Name)CompositionId

  @CODE<CLASS> private static readonly ushort @local#IdName = CompositionId.GetCompositionId(""@local#Name"");
    @\public static readonly Composable@local#Types @local#Name = static (@local#Args) => {
    @\  var transfer = new CompositionInternals.TransferData();
    @\  CompositionInternals.EnterComposition(ref cx, @local#IdName, ref transfer);
    @\  try { 
    @\    @(target:name)(@local#CallArgs);
    @\  } finally { 
    @\    CompositionInternals.ExitComposition(ref cx, ref transfer); 
    @\  }
    @\};
@END

@# The props and compose calls are currently experimental
@FUNC<BoundaryElementImpl>
  @USING HELIX.Compose;
  @CODE<IMPLEMENTS> IRecomposeMixinTargets

  @LOCAL<ComposeCalls>
  @LOCAL<ComposeArgs> ref Composition cx

  @CALL<RequireCompositionTypeId>
  @CALL<RequireCompositionId>
  @CALL<DeclareCompanion> public static readonly CompositionId fullId = new() {
    @\  composition = compositionId,
    @\  type = typeId,
    @\  local = LocalId.Initial
    @\};

  @SCOPE
    @MATCH @this#Props:?exists
    @AUGMENT_STRUCT<PropsModel> @this#Props
    @CODE<IMPLEMENTS> IProps<@this:name.Props>
    @CODE<CLASS> private Props _props;
      @\public ref Props props => ref _props;
      @\public void ReceiveProps(in Props props) => _props = props;
    @MIXIN<$Reset><1> _props = default;
    @LOCAL<ComposeArgs> @local#ComposeArgs, @local#PropsModel:structParams
    @LOCAL<ComposeCalls> @local#ComposeCalls
      @\var props = new Props(@local#PropsModel:structArgs);
      @\instance.ReceiveProps(in props);
  @END


  @CODE<CLASS> public static ref ElementRef Compose(
    @\  @local#ComposeArgs
    @\) {
    @\  if (!cx.AUTHORING.RequireComposable<@this:name>(@(var#CompanionName).typeId, out var instance, out var retained)) {
    @\    instance = new @this:name();
    @\  }
    @\  @local#ComposeCalls
    @\  return ref cx.AUTHORING.YieldBoundary(ref cx, instance);
    @\}

  @CODE<CLASS> public override void PerformCompose(ref Composition cx) {
    @\  if (!IsInitialized) { 
    @\    ((IRecomposeMixinTargets)this).MixinInit();
    @\    IsInitialized = true;
    @\  }
    @\  cx.AUTHORING.SetId(@(var#CompanionName).fullId);
    @\  ((IRecomposeMixinTargets)this).MixinRecompose(ref cx);
    @\}

  @CODE<CLASS> public override void Dispose() {
    @\  ((IRecomposeMixinTargets)this).MixinDispose();
    @\  base.Dispose();
    @\  IsInitialized = false;
    @\}

  @CODE<CLASS> public override void Reset() {
    @\  ((IRecomposeMixinTargets)this).MixinReset();
    @\  base.Reset(); 
    @\  IsInitialized = false;
    @\}
@END

"
  )]
  public static class ComposeMixinLibrary { }

  public interface IRecomposeMixinTargets {
    void MixinInit() { }
    void MixinDispose() { }
    void MixinReset() { }
    void MixinRecompose(ref Composition cx) { }
  }

  public static class RecomposeMixinTargets {
    public const string Init = "^MixinInit";
    public const string Dispose = "^MixinDispose";
    public const string Reset = "^MixinReset";
    public const string Recompose = "^MixinRecompose:HELIX.Compose.Composable";
  }

  [AttributeUsage(AttributeTargets.Class)]
  [MixinImport(typeof(ComposeMixinLibrary))]
  [MixinExpression("@CALL<BoundaryComposableImpl>")]
  [MixinDefineTarget(MixinOn.Init, RecomposeMixinTargets.Init)]
  [MixinDefineTarget(MixinOn.Reset, RecomposeMixinTargets.Reset)]
  [MixinDefineTarget(MixinOn.Dispose, RecomposeMixinTargets.Dispose)]
  [MixinDefineTarget(MixinOn.Recompose, RecomposeMixinTargets.Recompose)]
  public sealed class BoundaryComposableMixinAttribute : Attribute {
    public BoundaryComposableMixinAttribute(
      Type super = null,
      bool extension = false,
      string name = null,
      bool cacheLookups = false
    ) { }
  }

  [AttributeUsage(AttributeTargets.Method)]
  [MixinImport(typeof(ComposeMixinLibrary))]
  [MixinImport(typeof(CoreMixinLibrary))]
  [MixinExpression("@CALL<ComposableMethodImpl>")]
  public sealed class ComposableMethodAttribute : Attribute {
    public ComposableMethodAttribute(
      bool requiresTracking = false,
      bool scope = false,
      bool extension = false,
      string name = null,
      string scopeCallback = "cell.TrimChildren()"
    ) { }
  }

  [AttributeUsage(AttributeTargets.Method)]
  [MixinImport(typeof(ComposeMixinLibrary))]
  [MixinExpression("@CALL<ComposableDelegateImpl>")]
  public sealed class ComposableDelegateAttribute : Attribute {
    public ComposableDelegateAttribute(
      string name = null
    ) { }
  }

  [AttributeUsage(AttributeTargets.Class)]
  [MixinImport(typeof(ComposeMixinLibrary))]
  [MixinImport(typeof(CoreMixinLibrary))]
  [MixinExpression("@CALL<BoundaryElementImpl>")]
  [MixinDefineTarget(MixinOn.Init, RecomposeMixinTargets.Init)]
  [MixinDefineTarget(MixinOn.Reset, RecomposeMixinTargets.Reset)]
  [MixinDefineTarget(MixinOn.Dispose, RecomposeMixinTargets.Dispose)]
  [MixinDefineTarget(MixinOn.Recompose, RecomposeMixinTargets.Recompose)]
  public sealed class BoundaryElementAttribute : Attribute {
    public BoundaryElementAttribute() { }
  }
}