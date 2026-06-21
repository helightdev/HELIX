using System;
using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Forms;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;

namespace HELIX.Widgets.Universal.Forms {
  public sealed class HFormSlider : FormFieldWidget<HFormSlider, float> {
    public readonly Key focusKey;
    public readonly Axis axis;
    public readonly bool reverse;
    public readonly float thumbSize;
    public readonly Action<float> onChanged;
    public readonly WidgetStateProperty<ModifierSet> boxModifiers;
    public readonly HSliderStyle style;

    public HFormSlider(
      string path,

      Key focusKey = default, // Slider settings //
      Axis axis = Axis.Horizontal,
      bool reverse = false,
      float thumbSize = -1f,
      Action<float> onChanged = null,
      WidgetStateProperty<ModifierSet> boxModifiers = null,
      HSliderStyle style = null,
      IEnumerable<IFormValidator> validators = null, // Form settings //
      ValidationMode validationMode = ValidationMode.OnChange,
      float initialValue = 0f,
      IEqualityComparer<object> comparer = null,
      bool enabled = true,
      FormController controller = null,
      Key key = default, // Widget settings //
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(path, validators, validationMode, initialValue, comparer, enabled, controller, key, modifiers) {
      this.focusKey = focusKey;
      this.axis = axis;
      this.reverse = reverse;
      this.thumbSize = thumbSize;
      this.onChanged = onChanged;
      this.boxModifiers = boxModifiers;
      this.style = style;
    }

    public override State<HFormSlider> CreateState() {
      return new HFormSliderState();
    }
  }

  public sealed class HFormSliderState : FormFieldState<HFormSlider, float> {
    private SliderController _controller;

    public override void InitState() {
      base.InitState();
      widgetState = AddDisposable(new WidgetStateController());
      _controller = AddDisposable(new SliderController(widgetState, widget.initialValue));
      _controller.onChanged = value => {
        widget.onChanged?.Invoke(value);
        NotifyUserValueChanged(value);
        form.MarkFinishedEditing(path);
      };
    }

    protected override void OnFormFieldChanged(float value, bool hasValue) {
      if (hasValue && !Mathf.Approximately(_controller.PeekValue(), value)) {
        _controller.SetValue(value);
      }
    }

    public override Widget Build(BuildContext context) {
      return new HSlider(
        controller: _controller,
        focusKey: widget.focusKey,
        axis: widget.axis,
        reverse: widget.reverse,
        thumbSize: widget.thumbSize,
        boxModifiers: widget.boxModifiers,
        style: widget.style
      );
    }
  }
}