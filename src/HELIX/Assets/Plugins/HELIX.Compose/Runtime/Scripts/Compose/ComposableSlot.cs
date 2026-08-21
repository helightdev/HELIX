using UnityEngine.UIElements;

namespace HELIX.Compose {
  public interface ISlotHost : IComposable {
    IBoundary Boundary { get; }
  }

  public class ComposableSlot : VisualElement, IComposable {
    public LocalId slotLocalId;
    public int SlotType { get; private set; }

    public ISlotHost Host { get; private set; }

    public ComposableSlot(ISlotHost host, LocalId id, int slotType = 0) {
      Host = host;
      slotLocalId = id;
      SlotType = slotType;
    }

    public ComposableSlot(ISlotHost host, UniqueStyleString slotType) {
      Host = host;
      slotLocalId = LocalId.FromData(slotType.id);
      SlotType = slotType.id;
      style.display = DisplayStyle.None;
    }

    public VisualElement Element => this;

    public UssFlag Flag { get; set; }

    public ulong PackedId { get; set; }

    // public void Recompose(Composable composable) {
    //   var cid = CompositionId.Generated(slotLocalId);
    //   var cx = new Composition(Host.Boundary, cid, this) { Slot = this };
    //   composable(ref cx);
    //
    //   style.display = DisplayStyle.Flex;
    // }

    public void Reset() {
      Clear();
      style.display = DisplayStyle.None;
    }

    public void MarkFlag(UssFlag flag) {
      Flag |= flag;
    }

    public ScopeHandle Scope(Composition cx, bool trimChildren = true) {
      PackedId = new CompositionId(slotLocalId, CompositionId.GeneratedCompositionId, CompositionId.SlotTypeId).packed;

      var handle = ScopeHandle.Push(
        cx.AUTHORING.cell,
        this,
        trimChildren ? static (BoundaryCell cell, in ScopeHandle _) => cell.TrimChildren() : null
      );
      cx.AUTHORING.cell.slot = this;
      style.display = DisplayStyle.Flex;

      return handle;
    }
  }
}