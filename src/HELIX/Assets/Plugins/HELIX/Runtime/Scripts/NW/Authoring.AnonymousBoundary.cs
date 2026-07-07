namespace HELIX.NW {
  public ref partial struct CompositionAuthoring {

    private bool RequireAnonymouseBoundaryNode(
      ushort typeId,
      out CompositionBoundaryNode node,
      out AnonymousNodeState state,
      out bool retained
    ) {
      if (RequireBoundaryNode(typeId, out node, out retained)) {
        if (node.State is not AnonymousNodeState anonymousState) {
          anonymousState = new AnonymousNodeState();
          node.SetState(anonymousState);
          retained = false;
        }
        state = anonymousState;
        return true;
      }

      state = new AnonymousNodeState();
      node.SetState(state);
      return false;
    }

    public bool InitializeAnonymouseBoundaryNode(
      ushort typeId,
      out CompositionBoundaryNode node,
      out AnonymousNodeState state
    ) {
      RequireAnonymouseBoundaryNode(typeId, out node, out state, out var retained);
      return !retained;
    }
  }
}