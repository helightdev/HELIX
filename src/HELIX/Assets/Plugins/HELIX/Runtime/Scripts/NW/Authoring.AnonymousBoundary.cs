namespace HELIX.NW {
  public ref partial struct CompositionAuthoring {

    private bool RequireAnonymouseBoundaryNode(
      ushort typeId,
      out CompositionBoundaryNode node,
      out AnonymousBoundaryData data,
      out bool retained
    ) {
      if (RequireCompositionBoundaryNode(typeId, out node, out retained)) {
        if (node.Data is not AnonymousBoundaryData anonymousState) {
          anonymousState = new AnonymousBoundaryData();
          node.SetDataOnly(anonymousState);
          retained = false;
        }
        data = anonymousState;
        return true;
      }

      data = new AnonymousBoundaryData();
      node.SetDataOnly(data);
      return false;
    }

    public bool InitializeAnonymouseBoundaryNode(
      ushort typeId,
      out CompositionBoundaryNode node,
      out AnonymousBoundaryData data
    ) {
      RequireAnonymouseBoundaryNode(typeId, out node, out data, out var retained);
      return !retained;
    }
  }
}