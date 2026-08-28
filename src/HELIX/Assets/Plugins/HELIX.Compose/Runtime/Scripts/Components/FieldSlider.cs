namespace HELIX.Compose {
  /// <summary>A slider with a compact exact-value editor beside it.</summary>
  [EnableMixins]
  [BoundaryElementMixin(name: "FieldSlider", extension: true)]
  public partial class FieldSlider {
    private FloatDatatype _automaticDatatype;
    private float _automaticMin, _automaticMax, _automaticStep;

    [Hook]
    private void OnCompose(ref Composition cx) {
      var datatype = props.datatype;
      if (datatype == null) {
        if (_automaticDatatype == null || !_automaticMin.Equals(props.min) || !_automaticMax.Equals(props.max) ||
          !_automaticStep.Equals(props.step)) {
          _automaticMin = props.min;
          _automaticMax = props.max;
          _automaticStep = props.step;
          _automaticDatatype = new FloatDatatype(
            min: props.min,
            max: props.max,
            step: props.step
          );
        }
        datatype = _automaticDatatype;
      }
      cx.DatatypeFieldSlider(
        props.value,
        datatype,
        props.onChanged,
        props.onCommitted,
        props.min,
        props.max,
        props.step,
        props.enabled,
        props.error,
        props.prefix,
        props.suffix
      );
    }

    public partial struct Props {
      public float value;
      [Prop(null)] public CompositionAction<float> onChanged;
      [Prop(null)] public CompositionAction onCommitted;
      [Prop(0f)] public float min;
      [Prop(1f)] public float max;
      [Prop(0f)] public float step;
      [Prop(true)] public bool enabled;
      [Prop(false)] public bool error;
      [Prop(null)] public Composable prefix;
      [Prop(null)] public Composable suffix;
      [Prop(null, Equatable = false)] public IDatatype<float> datatype;
    }
  }
}