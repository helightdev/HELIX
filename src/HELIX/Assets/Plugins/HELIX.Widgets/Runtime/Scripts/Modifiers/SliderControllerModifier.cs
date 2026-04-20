using HELIX.Types;
using HELIX.Widgets.Universal.Controllers;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
  public class SliderControllerModifier : SingletonModifier {
    public readonly Axis axis;
    public readonly SliderController controller;
    public readonly float thumbSize;
    public readonly WidgetStateController widgetState;
    private SliderManipulator _manipulator;
    public bool reverse;

    public SliderControllerModifier(
      SliderController controller,
      Axis axis,
      float thumbSize = -1,
      bool reverse = false,
      WidgetStateController widgetState = null
    ) {
      this.controller = controller;
      this.axis = axis;
      this.thumbSize = thumbSize < 0f ? -1f : thumbSize;
      this.reverse = reverse;
      this.widgetState = widgetState ?? controller?.widgetState;
    }

    public override void Hook(VisualElement element) {
      _manipulator = new SliderManipulator(controller, axis, thumbSize, reverse, widgetState);
      element.AddManipulator(_manipulator);
    }

    public override void Unhook(VisualElement element) {
      if (_manipulator != null && _manipulator.target == element) element.RemoveManipulator(_manipulator);
      _manipulator = null;
    }

    public override bool HasChanged(Modifier previous) {
      return previous is not SliderControllerModifier prev ||
             !ReferenceEquals(controller, prev.controller) ||
             axis != prev.axis || reverse != prev.reverse ||
             !Mathf.Approximately(thumbSize, prev.thumbSize) ||
             !ReferenceEquals(widgetState, prev.widgetState);
    }

    public class SliderManipulator : Manipulator {
      private readonly Axis _axis;
      private readonly SliderController _controller;
      private readonly bool _reverse;
      private readonly float _thumbSize;
      private readonly WidgetStateController _widgetState;
      private float _dragStartAxisPosition;
      private float _dragStartValue;
      private int _pointerId = -1;

      public SliderManipulator(
        SliderController controller,
        Axis axis,
        float thumbSize,
        bool reverse,
        WidgetStateController widgetState
      ) {
        _controller = controller;
        _axis = axis;
        _thumbSize = thumbSize < 0f ? -1f : thumbSize;
        _reverse = reverse;
        _widgetState = widgetState;
      }

      protected override void RegisterCallbacksOnTarget() {
        target.focusable = true;
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        target.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
      }

      protected override void UnregisterCallbacksFromTarget() {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
        target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        target.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
      }

      private void OnPointerDown(PointerDownEvent evt) {
        if (_controller == null || !_controller.Enabled) return;
        if (_pointerId != -1) return;

        _pointerId = evt.pointerId;
        target.CapturePointer(_pointerId);

        var rect = target.contentRect;
        var axisPosition = GetAxisPosition(evt.localPosition, rect);
        var thumbPixels = GetThumbPixels(rect);
        var range = GetRange(rect, thumbPixels);

        if (!IsPointerOnThumb(axisPosition, thumbPixels, range)) UpdateValue(evt.localPosition);

        _dragStartAxisPosition = axisPosition;
        _dragStartValue = _controller.Value;

        _widgetState?.DisableEnable(WidgetState.Navigated, WidgetState.Pressed | WidgetState.Dragged);
        evt.StopPropagation();
      }

      private void OnPointerMove(PointerMoveEvent evt) {
        if (_controller == null || evt.pointerId != _pointerId) return;
        if (!target.HasPointerCapture(_pointerId)) return;
        if (!_controller.Enabled) return;

        UpdateDragValue(evt.localPosition);
        evt.StopPropagation();
      }

      private void OnPointerUp(PointerUpEvent evt) {
        if (evt.pointerId != _pointerId) return;
        EndDrag(evt.pointerId);
        evt.StopPropagation();
      }

      private void OnPointerCancel(PointerCancelEvent evt) {
        if (evt.pointerId != _pointerId) return;
        EndDrag(evt.pointerId);
        evt.StopPropagation();
      }

      private void OnPointerCaptureOut(PointerCaptureOutEvent evt) {
        if (_pointerId == -1) return;
        _pointerId = -1;
        _widgetState?.Disable(WidgetState.Pressed | WidgetState.Dragged);
      }

      private void OnNavigationMove(NavigationMoveEvent evt) {
        if (_controller == null || !_controller.Enabled) return;

        if (!TryGetMoveDirection(evt, out var direction)) return;
        ApplyStep(direction, GetBaseStepNormalized());
        evt.StopPropagation();
      }

      private void EndDrag(int pointerId) {
        if (target.HasPointerCapture(pointerId)) target.ReleasePointer(pointerId);
        _pointerId = -1;
        _widgetState?.Disable(WidgetState.Pressed | WidgetState.Dragged);
      }

      private void UpdateValue(Vector2 localPosition) {
        var rect = target.contentRect;
        var thumbPixels = GetThumbPixels(rect);
        var range = GetRange(rect, thumbPixels);

        if (Mathf.Approximately(range, 0f)) {
          _controller.Value = 0f;
          return;
        }

        var axisPosition = GetAxisPosition(localPosition, rect);
        var normalized = (axisPosition - thumbPixels * 0.5f) / range;
        if (_reverse) normalized = 1f - normalized;

        _controller.Value = Mathf.Clamp01(normalized);
      }

      private void UpdateDragValue(Vector2 localPosition) {
        var rect = target.contentRect;
        var thumbPixels = GetThumbPixels(rect);
        var range = GetRange(rect, thumbPixels);

        if (Mathf.Approximately(range, 0f)) {
          _controller.Value = 0f;
          return;
        }

        var axisPosition = GetAxisPosition(localPosition, rect);
        var deltaNormalized = (axisPosition - _dragStartAxisPosition) / range;
        if (_reverse) deltaNormalized = -deltaNormalized;

        _controller.Value = Mathf.Clamp01(_dragStartValue + deltaNormalized);
      }

      private float GetThumbPixels(Rect rect) {
        var axisSize = _axis.GetRectSize(rect);
        return _thumbSize < 0f ? _controller.ThumbRange * axisSize : _thumbSize;
      }

      private float GetBaseStepNormalized() {
        var rect = target.contentRect;
        var range = GetRange(rect, GetThumbPixels(rect));
        if (Mathf.Approximately(range, 0f)) return 0f;
        return Mathf.Max(0.01f, 8f / range);
      }

      private void ApplyStep(int direction, float step) {
        if (Mathf.Approximately(step, 0f)) return;

        var signedStep = direction * step;
        if (_reverse) signedStep = -signedStep;

        SetAbsoluteValue(_controller.Value + signedStep);
      }

      private void SetAbsoluteValue(float value) {
        _controller.Value = Mathf.Clamp01(value);
        _widgetState?.Enable(WidgetState.Navigated | WidgetState.Pressed);
        target.schedule.Execute(DisablePressed).ExecuteLater(100);
      }

      private void DisablePressed() {
        _widgetState?.Disable(WidgetState.Pressed);
      }

      private bool TryGetMoveDirection(NavigationMoveEvent evt, out int direction) {
        direction = 0;
        switch (evt.direction) {
          case NavigationMoveEvent.Direction.Left:
            if (_axis != Axis.Horizontal) return false;
            direction = -1;
            return true;
          case NavigationMoveEvent.Direction.Right:
            if (_axis != Axis.Horizontal) return false;
            direction = 1;
            return true;
          case NavigationMoveEvent.Direction.Up:
            if (_axis != Axis.Vertical) return false;
            direction = -1;
            return true;
          case NavigationMoveEvent.Direction.Down:
            if (_axis != Axis.Vertical) return false;
            direction = 1;
            return true;
          default: return false;
        }
      }

      private float GetRange(Rect rect, float thumbPixels) {
        var axisSize = _axis.GetRectSize(rect);
        return Mathf.Max(0f, axisSize - thumbPixels);
      }

      private float GetAxisPosition(Vector2 localPosition, Rect rect) {
        return _axis.GetVectorComponent(localPosition - rect.min);
      }

      private bool IsPointerOnThumb(float axisPosition, float thumbPixels, float range) {
        if (Mathf.Approximately(range, 0f)) return true;

        var normalized = _reverse ? 1f - _controller.Value : _controller.Value;
        var thumbStart = normalized * range;
        var thumbEnd = thumbStart + thumbPixels;
        return axisPosition >= thumbStart && axisPosition <= thumbEnd;
      }
    }
  }
}