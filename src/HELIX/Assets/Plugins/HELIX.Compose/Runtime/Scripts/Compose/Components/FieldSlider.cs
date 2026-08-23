using HELIX.Theming;
using HELIX.Types;
using HELIX.Prose;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  /// <summary>A slider with a compact exact-value editor beside it.</summary>
  [BoundaryComposable(Extension = true, Name = "FieldSlider", UseLookupCache = false)]
  public partial class FieldSlider {
    public partial struct Props {
      public float value;
      [Prop(null)] public CompositionAction<float> onChanged;
      [Prop(null)] public CompositionAction onCommitted;
      [Prop(0f)] public float min;
      [Prop(1f)] public float max;
      [Prop(0f)] public float step;
      [Prop(true)] public bool enabled;
      [Prop(false)] public bool error;
      [Prop("default", PropInit.Constant, Equatable = false)] public NumericFormatSettings formatting;
      [Prop(null)] public Composable prefix;
      [Prop(null)] public Composable suffix;
      [Prop(null, Equatable = false)] public IDatatype<float> datatype;
    }

    protected override void OnRecompose(ref Composition cx) {
      using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
        cx.Slider(
            props.value,
            onChanged: ChangeFromSlider,
            onCommitted: CommitFromSlider,
            options: new SliderOptions(props.min, props.max, props.step),
            enabled: props.enabled,
            error: props.error
          )
          .Flexible();
        cx.Gap(ThemeProperties.TextGap[in cx]);
        cx.Spec(new ControlSpec<float>(
          props.value,
          new TextControlDatatype<float>(
            props.datatype ?? new FloatDatatype(
              format: props.formatting.format ?? "R",
              min: props.min,
              max: props.max,
              scale: props.formatting.scale == 0f ? 1f : props.formatting.scale,
              step: props.step
            ),
            props.prefix,
            props.suffix
          ),
          props.onChanged,
          props.onCommitted,
          props.enabled,
          props.error
        ));
        cx.CURSOR.Width(ThemeProperties.CompanionFieldWidth[in cx]);
      }
    }

    private static void ChangeFromSlider(CompositionContext context, float value) {
      var component = context.Lookup<FieldSlider>();
      component?.props.onChanged?.Call(component.Node, value);
    }

    private static void CommitFromSlider(CompositionContext context, float _) {
      var component = context.Lookup<FieldSlider>();
      component?.props.onCommitted?.Call(component.Node);
    }
  }
}
