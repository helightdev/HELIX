using HELIX.Abstractions;
using UnityEngine.UIElements;

namespace HELIX.NW {
  public interface IComposable : IElement {
    UssDirtyFlags DirtyFlags { get; set; }
    ulong TypeId { get; set; }
    void Reset();
  }

  public sealed class UserdataTracker : IComposable {
    public VisualElement Element { get; set; }
    public UssDirtyFlags DirtyFlags { get; set; }
    public ulong TypeId { get; set; }
    public void Reset() { }
  }

  public sealed class CompositionNode : VisualElement, IComposable {
    public VisualElement Element => this;

    public UssDirtyFlags DirtyFlags { get; set; }
    public ulong TypeId { get; set; }

    public void Reset() { }
  }
}