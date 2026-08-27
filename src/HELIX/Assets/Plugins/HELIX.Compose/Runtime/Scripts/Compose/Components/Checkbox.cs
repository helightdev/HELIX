using HELIX.Signals;
using HELIX.Theming;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public sealed class CheckboxController : Signal<bool> {
    private bool _value;

    public CompositionAction<bool> onChanged;
    public bool enabled = true;
    public bool error;

    public CheckboxController(bool initialValue = false) : base("CheckboxController", typeof(CheckboxController)) {
      _value = initialValue;
    }

    public State State {
      get {
        var state = State.None;
        state |= _value ? State.Selected : State.None;
        state |= enabled ? State.None : State.Disabled;
        state |= error ? State.Error : State.None;
        return state;
      }
    }

    public override bool PeekValue() => _value;

    public override void SetValue(bool newValue) {
      if (_value == newValue) return;
      _value = newValue;
      NotifyListeners();
    }

    public override void SetWithoutNotify(bool newValue) {
      _value = newValue;
      NotifyDirty();
    }

    internal void SynchronizeValue(bool newValue) {
      _value = newValue;
    }

    internal void SetUserValue(IBoundary boundary, bool newValue) {
      if (_value == newValue) return;
      _value = newValue;
      onChanged?.Call(boundary, newValue);
      NotifyListeners();
    }

    private void NotifyListeners() {
      NotifyDirty();
      NotifyObservers();
    }
  }

  [EnableMixins]
  [BoundaryComposableMixin(super: typeof(InputClickableComposable<>), extension: true)]
  public partial class Checkbox {
    public partial struct Props {
      [Prop(null)] public bool? value;
      [Prop(null)] public CheckboxController controller;
      [Prop(null)] public bool? initialValue;
      [Prop(null)] public CompositionAction<bool> onChanged;
      [Prop(true)] public bool enabled;
      [Prop(false)] public bool error;
      [Prop(null)] public HXControlBoxStyle? style;
    }

    public CheckboxController controller;
    public bool isAutomaticController = true;

    protected override void OnDetach() {
      DisposeAutomaticController();
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      EnsureController(props.controller);
      cx.SubscribeTo(controller);

      this.Toggle(State.Selected, controller.PeekValue());
      this.Toggle(State.Disabled, !controller.enabled);
      this.Toggle(State.Error, controller.error);
      var passedState = controller.State | InputState;
      var style = props.style ?? ThemeProperties.Checkbox[in cx];

      using (cx.WriteContext(out var context)) {
        style.RenderContext(in context, passedState);
      }

      cx.CURSOR.Focusable(controller.enabled);
      style.RenderContent(ref cx, passedState);
    }

    public void EnsureController(CheckboxController given) {
      if (ReferenceEquals(given, controller) && controller != null) return;
      if (given == null) {
        if (isAutomaticController && controller != null) {
          ConfigureAutomaticController();
          controller.SynchronizeValue(props.value ?? controller.PeekValue());
        } else {
          controller = new CheckboxController();
          isAutomaticController = true;
          ConfigureAutomaticController();
          controller.SynchronizeValue(props.value ?? props.initialValue ?? false);
        }
      } else {
        DisposeAutomaticController();
        controller = given;
      }
    }

    private void ConfigureAutomaticController() {
      controller.onChanged = props.onChanged;
      controller.enabled = props.enabled;
      controller.error = props.error;
    }

    private void DisposeAutomaticController() {
      if (!isAutomaticController) return;
      controller?.Dispose();
      isAutomaticController = false;
    }

    protected override void OnClick(EventBase evt) {
      if (controller == null || !controller.enabled) return;
      controller.SetUserValue(Node, !controller.PeekValue());
    }
  }

  [EnableMixins]
  [BoundaryComposableMixin(super: typeof(InputClickableComposable<>), extension: true)]
  public partial class RawCheckbox {
    public partial struct Props {
      [Prop] public bool value;
      [Prop(null)] public CompositionAction<bool> onChanged;
      [Prop(true)] public bool enabled;
      [Prop(false)] public bool error;
      [Prop(null)] public HXControlBoxStyle? style;
    }

    protected override void OnRecompose(ref Composition cx) {
      this.Toggle(State.Selected, props.value);
      this.Toggle(State.Disabled, !props.enabled);
      this.Toggle(State.Error, props.error);
      var style = props.style ?? ThemeProperties.Checkbox[in cx];

      using (cx.WriteContext(out var context)) {
        style.RenderContext(in context, InputState);
      }

      cx.CURSOR.Focusable(props.enabled);
      style.RenderContent(ref cx, InputState);
    }

    protected override void OnClick(EventBase evt) {
      if (!props.enabled) return;
      props.onChanged?.Call(Node, !props.value);
    }
  }
}