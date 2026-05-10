using System;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Signals;
using UnityEngine;

namespace HELIX.Widgets.Universal.Controllers {
  public class SliderController : ValueSignal<float>, ISignalObserver {
    public readonly WidgetStateController widgetState;
    private float _thumbRange = 0.1f;
    public bool enabled = true;
    public Action<float> onChanged;

    public SliderController(WidgetStateController widgetState = null, float initialValue = 0f)
      :
      base(Mathf.Clamp01(initialValue)) {
      this.widgetState = widgetState;
    }

    public float ThumbRange {
      get => _thumbRange;
      set {
        if (Mathf.Approximately(_thumbRange, value)) return;
        _thumbRange = Mathf.Clamp01(value);
        NotifyObservers();
      }
    }

    public bool Enabled => enabled && (widgetState?.PeekValue().Enabled() ?? true);
    public ScrollController LinkedScrollController { get; private set; }

    public void OnSignalChanged(Signal signal) {
      if (signal == LinkedScrollController) RefreshFromLinkedScroll();
    }

    public void OnSignalRemoved(Signal signal) {
      if (signal == LinkedScrollController) LinkedScrollController = null;
    }

    public override void SetValue(float newValue) {
      var oldValue = PeekValue();
      base.SetValue(Mathf.Clamp01(newValue));
      var current = PeekValue();
      if (LinkedScrollController?.ScrollPosition != null) LinkedScrollController.NormalizedScrollOffset = current;
      if (!Mathf.Approximately(oldValue, current)) onChanged?.Invoke(current);
    }

    public override void SetWithoutNotify(float newValue) {
      base.SetWithoutNotify(Mathf.Clamp01(newValue));
    }

    public void LinkScrollController(ScrollController scrollController, bool syncValueFromScroll = true) {
      if (LinkedScrollController == scrollController) {
        RefreshFromLinkedScroll(syncValueFromScroll);
        return;
      }

      LinkedScrollController?.RemoveObserver(this);
      LinkedScrollController = scrollController;
      LinkedScrollController?.AddObserver(this);
      RefreshFromLinkedScroll(syncValueFromScroll);
    }

    public void UnlinkScrollController() {
      LinkedScrollController?.RemoveObserver(this);
      LinkedScrollController = null;
    }

    public void RefreshFromLinkedScroll(bool syncValue = true) {
      var position = LinkedScrollController?.ScrollPosition;
      if (position == null) return;

      var extentTotal = position.ExtentTotal;
      var extentInside = position.ExtentInside;
      ThumbRange = extentTotal <= 0f ? 1f : Mathf.Clamp01(extentInside / extentTotal);

      if (syncValue) SetValue(LinkedScrollController.NormalizedScrollOffset);
    }

    public override void Dispose() {
      UnlinkScrollController();
      base.Dispose();
    }
  }
}