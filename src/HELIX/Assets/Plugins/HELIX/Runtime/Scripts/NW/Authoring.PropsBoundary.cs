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
          node.SetData(propsState);
          retained = false;
        }
        data = propsState;
        return true;
      }

      data = new GenericPropsData<T>();
      node.SetData(data);
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

    public bool BoundaryStateComposable<TData, TStateComposable>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out TData data,
      out TStateComposable attachment
    ) where TData : BoundaryData, new() where TStateComposable : IBoundaryComposable, new() {
      if (RequireCompositionBoundaryNode(typeId, out node, out var retained)) {
        if (node.Data is not TData currentProps) {
          data = new TData();
          attachment = new TStateComposable();
          node.SetData(data, attachment);
          return false;
        }
        if (node.BoundaryComposable is not TStateComposable currentAttachment || !retained) {
          attachment = new TStateComposable();
          data = currentProps;
          node.SwapState(attachment);
          return false;
        }

        attachment = currentAttachment;
        data = currentProps;
        return true;
      }

      data = new TData();
      attachment = new TStateComposable();
      node.SetData(data, attachment);
      return false;
    }

    public bool PropsBoundaryStateComposable<TAttachment, TProps>(
      ushort typeId,
      out CompositionBoundaryNode node,
      out GenericPropsData<TProps> data,
      out TAttachment attachment
    ) where TProps : struct where TAttachment : PropsBoundaryComposable<TProps>, new() {
      return BoundaryStateComposable(typeId, out node, out data, out attachment);
    }
  }
}