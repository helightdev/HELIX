using System;

namespace HELIX.Compose {
  [AttributeUsage(AttributeTargets.Class)]
  [MixinExpression(
    @"
@USING HELIX.Compose;
@LOCAL<Name> @target:name

@SCOPE
  @MATCH @attr#Name:!?eq<null>
  @LOCAL<Name> @attr#Name
@END

@SCOPE<ResolveDefaultName>
  @MATCH @local#Name:?eq<null>
  @LOCAL<Name> @this:name
@END

@LOCAL<ExtensionName> @(local#Name)Extensions

@AUGMENT_STRUCT<PropsModel> @this#Props

@SCOPE<DefaultBase>
  @MATCH @attr#Base:?eq<null>
  @CODE<IMPLEMENTS> PropsBoundaryComposable<@this#Props:type>
@END

@SCOPE<CustomBase>
  @MATCH @attr#Base:!?eq<null>
  @CODE<IMPLEMENTS> @attr#Base:makeGeneric<(@this#Props:type)>
@END

@CODE<CLASS> public static ref ElementRef ComposeBoundary(
  @\ ref Composition cx, Props props
  @\) { 
  @\  cx.AUTHORING.PropsBoundaryStateComposable<@this:type, Props>(@local#ExtensionName.typeId, out var node, out _, out var attachment);
  @\  attachment.ReceiveProps(props); 
  @\  node.composable = null; 
  @\  return ref cx.AUTHORING.YieldBoundary(ref cx, node); 
  @\}

@SCOPE<NoArgs>
  @MATCH @local#PropsModel:?structNoArgs
  @CODE<CLASS> public static ref ElementRef ComposeBoundary(ref Composition cx) => ref ComposeBoundary(ref cx, default);
    @\public static readonly Composable BakedComposable = static (ref Composition cx) => { ComposeBoundary(ref cx); };
@END

@CODE<CLASS> public override void OnRecompose(
  @\ ref Composition cx, BoundaryData state, IBoundary boundary
  @\) { 
  @\  var transfer = new CompositionInternals.TransferData();
  @\  CompositionInternals.EnterComposition(ref cx, @local#ExtensionName.compositionId, ref transfer);
  @\  try { @attr#UseLookupCache:switch<boundary.UseLookupCache();><> base.OnRecompose(ref cx, state, boundary);
  @\  } finally { 
  @\    CompositionInternals.ExitComposition(ref cx, ref transfer); 
  @\  } 
  @\}

@CODE<FILE> @this:visibility static class @local#ExtensionName { 
  @\  public static readonly ushort compositionId = CompositionId.GetCompositionId(""@local#Name"");
  @\  public static readonly ushort typeId = CompositionId.GetTypeId(""@local#Name""); 
  @\}

@SCOPE<Extension>
  @MATCH @attr#Extension
  @CODE<FILE> @this:visibility static class @(local#Name)CompositionExtensions { 
    @\  public static ref ElementRef @local#Name
    @+(ref this Composition cx, @this#Props:type props)
    @+ => ref @this:type.ComposeBoundary(ref cx, props); 
    @\}
@END
"
  )]
  public sealed class BoundaryComposableMixinAttribute : Attribute {
    public Type Base { get; set; }
    public bool Extension { get; set; }
    public string Name { get; set; }
    public bool UseLookupCache { get; set; }
  }
}