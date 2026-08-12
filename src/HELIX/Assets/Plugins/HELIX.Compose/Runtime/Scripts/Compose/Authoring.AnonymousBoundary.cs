namespace HELIX.Compose {
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
          retention = CompositionRetention.Reset;
        }
        data = anonymousState;
        retention = CompositionRetention.Retained;
        return true;
      }

      data = new AnonymousBoundaryData();
      node.SetDataOnly(data);
      retention = CompositionRetention.New;
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