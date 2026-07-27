namespace HELIX.NW {
  public ref partial struct CompositionAuthoring {
    private bool RequirePropsBoundary<T>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out GenericPropsData<T> data,
      out bool retained
    ) where T : struct {
      if (RequireCompositionBoundaryNode(typeId, out node, out retained)) {
        if (node.Data is not GenericPropsData<T> propsState) {
          propsState = new GenericPropsData<T>();
          node.SetDataOnly(propsState);
          retained = false;
        }
        data = propsState;
        return true;
      }

      data = new GenericPropsData<T>();
      node.SetDataOnly(data);
      return false;
    }

    public bool InitializePropsBoundaryNode<T>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out GenericPropsData<T> data
    ) where T : struct {
      RequirePropsBoundary(typeId, out node, out data, out var retained);
      return !retained;
    }

    public bool PropsBoundaryStateComposable<TAttachment, TProps>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out GenericPropsData<TProps> data,
      out TAttachment attachment
    ) where TProps : struct where TAttachment : PropsBoundaryComposable<TProps>, new() {
      return RequireBoundaryStateComposable(typeId, out node, out data, out attachment);
    }
  }
}