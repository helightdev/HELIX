namespace HELIX.NW {

  public ref partial struct CompositionAuthoring {

    private bool RequirePropsBoundaryNode<T>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out GenericPropsState<T> state,
      out bool retained
    ) where T : struct {
      if (RequireBoundaryNode(typeId, out node, out retained)) {
        if (node.State is not GenericPropsState<T> propsState) {
          propsState = new GenericPropsState<T>();
          node.SetState(propsState);
          retained = false;
        }
        state = propsState;
        return true;
      }

      state = new GenericPropsState<T>();
      node.SetState(state);
      return false;
    }

    public bool InitializePropsBoundaryNode<T>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out GenericPropsState<T> state
    ) where T : struct {
      RequirePropsBoundaryNode(typeId, out node, out state, out var retained);
      return !retained;
    }

    public bool PropsBoundaryStateNode<TAttachment, TProps>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out GenericPropsState<TProps> state,
      out TAttachment attachment
    ) where TProps : struct where TAttachment : PropsNodeStateAttachmentBase<TProps>, new() {
      RequirePropsBoundaryNode(typeId, out node, out state, out var retained);
      if (retained) {
        attachment = node.State.GetAttachment() as TAttachment;
      } else {
        attachment = new TAttachment();
        state.SetAttachment(attachment, node);
      }

      return retained;
    }
  }
}