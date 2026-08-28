using System;

namespace HELIX.Compose {
  [MixinLibrary(
    @"
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

@FUNC<BoundaryElementMixinFastImpl>
  @USING HELIX.Compose;
  @USING HELIX.Coloring;

  @CODE<IMPLEMENTS> HELIX.Compose.GeneratedBoundaryBase
  @CODE<IMPLEMENTS> HELIX.Compose.IBoundaryHooks

  @CODE<CLASS> public @this:name() : base(@attr#cacheLookups, @attr#trimChildren) {}

  @MIXIN<$Recompose><0> ((IBoundaryHooks)this).MixinCompose(ref cx);

  @SCOPE
    @MATCH @attr#composable:?eq<true>
    @CALL<BoundaryElementCompositionImpl>
  @END
@END

@FUNC<BoundaryElementMixinImpl>
  @USING HELIX.Compose;
  @USING HELIX.Coloring;

  @SCOPE
    @MATCH @this:!?is<global::UnityEngine.UIElements.VisualElement>
    @CODE<IMPLEMENTS> global::UnityEngine.UIElements.VisualElement
  @END

  @CODE<IMPLEMENTS> IBoundary
  @CODE<IMPLEMENTS> IRecomposeMixinTargets

  @CODE<CLASS>
    @\public global::UnityEngine.UIElements.VisualElement Element => this;
    @\public BoundaryCell Cell { get; } = BoundaryCell.Shared;
    @\public int TreeDepth { get; set; }
    @\public IBoundary Parent { get; set; }
    @\public IContextComposable ContextParent { get; set; }
    @\public bool IsDisposed { get; set; }
    @\public UssFlag Flag { get; set; }
    @\public ulong PackedId { get; set; }
    @\public SparseContextMap WrittenContext { get;set; }
    @\
    @\private LookupCache _lookupCache;
    @\private bool _initialAttachment = true;
    @\private bool _initialized = false;
    @\public bool TryLookupContext(int key, out ContextData data) {
    @\  data = null;
    @\  if (WrittenContext != null && WrittenContext.TryGet(key, out data)) return true;
    @\  return _lookupCache.TryLookup(ContextParent, key, out data);
    @\}
    @\
    @\public void RefreshHierarchy() {
    @\  Parent = GetFirstAncestorOfType<IBoundary>();
    @\  ContextParent = GetFirstAncestorOfType<IContextComposable>();
    @\  TreeDepth = BoundaryHelper.GetDepth(this);
    @\}
    @\public void MarkDirty() => HXComposer.MarkDirty(this, false);

  @MIXIN<$Recompose><-1000> try {
  @MIXIN<$Recompose><-100> BoundaryHelper.ContextBefore(this);
  @MIXIN<$Recompose><-75> _lookupCache.Clear();

  @Mixin<$Recompose><-60> if (!_initialized) Init(); 
  @MIXIN<$Recompose><-55> @attr#cacheLookups:switch<_lookupCache.Claim();><>
  @MIXIN<$Recompose><-50> try {
  @MIXIN<$Recompose><-25> var cx = new Composition(this);
  @MIXIN<$Recompose><0> PerformCompose(ref cx)
  @MIXIN<$Recompose><50> } catch (global::System.Exception e) { global::UnityEngine.Debug.LogException(e); }

  @MIXIN<$Recompose><100> BoundaryHelper.ContextAfter(this);
  @MIXIN<$Recompose><200> @attr#trimChildren:switch<Cell.TrimChildren();><>
  @MIXIN<$Recompose><1000> } catch (global::System.Exception e) { global::UnityEngine.Debug.LogException(e); }

  @MIXIN<$Reset><-1> HXComposer.RemoveDirty(this);
  @MIXIN<$Reset><1> _lookupCache.Release();
  @MIXIN<$Reset><100> _initialized = false;

  @MIXIN<$Compose> // Default handle

  @MIXIN<$Init><100> _initialized = true;
  @MIXIN<$Dispose><-100> if (IsDisposed) return; IsDisposed = true; _initialized = false;
  @MIXIN<$Dispose><-1> Reset();

  @MIXIN<$PostConstruct><1000> if (HXComposer.IsProcessing) { 
    @\  _initialAttachment = false;
    @\  HXComposer.MarkDirty(this);
    @\}

  @MIXIN<DetachBoundary> 
    @\_initialAttachment = true;
    @\_lookupCache.Release();
    @\HXComposer.NotifyDetach(this);

  @MIXIN<AttachBoundary>
    @\IsDisposed = false;
    @\RefreshHierarchy();

  @MIXIN<AttachBoundary><100>HXComposer.RegisterActiveBoundary(this);

  @MIXIN<AttachBoundary><200>if (_initialAttachment) {
    @\  _initialAttachment = false;
    @\  HXComposer.MarkDirty(this);
    @\}

  @SCOPE
    @MATCH @attr#constructor:?eq<true>
    @CODE<CLASS> public @this:name() {
      @\  RegisterCallback<global::UnityEngine.UIElements.AttachToPanelEvent>(_ => AttachBoundary());
      @\  RegisterCallback<global::UnityEngine.UIElements.DetachFromPanelEvent>(_ => DetachBoundary());
      @\  PostConstruct();
      @\}
  @END


  @SCOPE
    @MATCH @attr#composable:?eq<true>
    @CALL<BoundaryElementCompositionImpl>
  @END
@END

@FUNC<BoundaryElementCompositionImpl>
  @LOCAL<ComposeCalls>
  @LOCAL<ComposeArgs>

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
    @MATCH @local#PropsModel:!?structNoArgs
    @CODE<IMPLEMENTS> IProps<@this:type.Props>
    @CODE<CLASS> private Props _props;
      @\public ref Props props => ref _props;
      @\public void ReceiveProps(in Props props) => _props = props;
    @MIXIN<$Reset><1> _props = default;
    @LOCAL<ComposeArgs> @local#ComposeArgs, @local#PropsModel:structParams
    @LOCAL<ComposeCalls> @local#ComposeCalls
      @\var props = new @(this:type).Props(@local#PropsModel:structArgs);
      @\instance.ReceiveProps(in props);
    @GOTO<GenerateCompose>
  @END

  @CODE<CLASS> public static readonly Composable BakedComposable = static (ref Composition cx) => { ComposeBoundary(ref cx); };

  @SCOPE<GenerateCompose>
  @END

  @LOCAL<Body> {
    @\  if (!cx.AUTHORING.RequireComposable<@this:type>(@(var#CompanionName).typeId, out var instance, out var retained)) {
    @\    instance = new @this:type();
    @\  }
    @\  @local#ComposeCalls
    @\  return ref cx.AUTHORING.YieldBoundary(ref cx, instance);
    @\}

  @CODE<CLASS> public static ref ElementRef ComposeBoundary(
    @\  ref Composition cx@local#ComposeArgs
    @\) @local#Body

  @LOCAL<Name> @this:name
  @SCOPE
    @MATCH @attr#name:!?eq<null>
    @LOCAL<Name> @attr#name:unwrap
  @END

  @SCOPE
    @MATCH @attr#extension:?eq<true>
    @CALL<DeclareCompanion> public static ref ElementRef @local#Name(
      @\ ref this Composition cx@local#ComposeArgs
      @\) @local#Body
  @END

  @MIXIN<$Recompose><-24> cx.AUTHORING.SetId(@(var#CompanionName).fullId);
@END

@FUNC<RequireInputState>
  @SCOPE
    @MATCH @var#HasInputState:!?eq<true>
    @VAR<HasInputState> true
    @USING HELIX.Compose;
    @CODE<IMPLEMENTS> IStateHolder
    @CODE<CLASS> private State inputState;
      @\ public ref State InputState => ref inputState;
  @END
@END

@FUNC<InputStateListenerImpl>
  @CALL<RequireInputState>

  @MIXIN<$PostConstruct> global::UnityEngine.UIElements.VisualElementExtensions.AddManipulator(this,
    @\  new InputListenerManipulator(this, this, @attr#focus)
    @\);

  @SCOPE
    @MATCH @attr#dirty:?eq<true>
    @MIXIN<^OnStateChanged:HELIX.Compose.StateChangedHandler> this.MarkDirty();
  @END
@END

@FUNC<ClickHandlerImpl>
  @CALL<RequireInputState>

  @MIXIN<$PostConstruct> global::UnityEngine.UIElements.VisualElementExtensions.AddManipulator(this,
    @\  new InputClickableManipulator(this, this, @target:name)
    @\);
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

  public interface IBoundaryHooks {
    void MixinPostConstruct() { }
    void MixinRecompose(ref Composition cx) { }
    void MixinCompose(ref Composition cx) { }
    void MixinInit() { }
    void MixinReset() { }
    void MixinDispose() { }
  }

  public static class RecomposeMixinTargets {
    public const string Init = "^MixinInit";
    public const string Dispose = "^MixinDispose";
    public const string Reset = "^MixinReset";
    public const string Recompose = "^MixinRecompose:HELIX.Compose.Composable";
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
  [MixinExpression("@CALL<BoundaryElementMixinImpl>")]
  [MixinDefineTarget(MixinOn.Compose, "PerformCompose:HELIX.Compose.Composable")]
  [MixinDefineTarget(MixinOn.Recompose, "^Recompose")]
  [MixinDefineTarget(MixinOn.Reset, "^Reset")]
  [MixinDefineTarget(MixinOn.Init, "Init")]
  [MixinDefineTarget(MixinOn.Dispose, "^Dispose")]
  [MixinDefineTarget(MixinOn.BoundaryPostConstruct, "^PostConstruct")]
  [MixinImport(typeof(CoreMixinLibrary))]
  [MixinImport(typeof(ComposeMixinLibrary))]
  public class CustomBoundaryElementAttribute : Attribute {
    public CustomBoundaryElementAttribute(
      bool constructor = true,
      bool composable = true,
      bool extension = false,
      bool cacheLookups = false,
      bool trimChildren = true,
      string name = null
    ) { }
  }


  [AttributeUsage(AttributeTargets.Class)]
  [MixinExpression("@CALL<BoundaryElementMixinFastImpl>")]
  [MixinDefineTarget(MixinOn.Init, "^MixinInit")]
  [MixinDefineTarget(MixinOn.Compose, "^MixinCompose:HELIX.Compose.Composable")]
  [MixinDefineTarget(MixinOn.Recompose, "^MixinRecompose:HELIX.Compose.Composable")]
  [MixinDefineTarget(MixinOn.Reset, "^MixinReset")]
  [MixinDefineTarget(MixinOn.Dispose, "^MixinDispose")]
  [MixinDefineTarget(MixinOn.BoundaryPostConstruct, "^MixinPostConstruct")]
  [MixinImport(typeof(CoreMixinLibrary))]
  [MixinImport(typeof(ComposeMixinLibrary))]
  public class BoundaryElementMixinAttribute : Attribute {
    public BoundaryElementMixinAttribute(
      bool composable = true,
      bool extension = false,
      bool cacheLookups = false,
      bool trimChildren = true,
      string name = null
    ) { }
  }

  [MixinExpression(
    @"
@ASSERT @var#HasWriteContext:!?eq<true>

@MIXIN<$Compose><-1> using (cx.WriteContext(out var context)) {
  @\  @target:name(ref cx, context);
  @\}
@VAR<HasWriteContext> true
"
  )]
  public class WriteContextHandlerAttribute : Attribute { }
}