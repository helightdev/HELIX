using HELIX.Theming;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [EnableMixins]
  [BoundaryComposableMixin(super: typeof(InputClickableComposable<>), name: "Button", extension: true)]
  public partial class HXButton {
    public partial struct Props {
      public Composable content;
      [Prop(null)] public CompositionAction action;
      [Prop(true)] public bool enabled;
      [Prop(false)] public bool selected;
      [Prop(null)] public HXControlBoxStyle? style;
    }

    public static readonly ContextKey<HXControlBoxStyle> Style = new("ButtonStyle", HXControlBoxStyle.Default);

    protected override void OnRecompose(ref Composition cx) {
      this.Toggle(State.Selected, props.selected);
      this.Toggle(State.Disabled, !props.enabled);
      cx.CURSOR.Focusable(props.enabled);

      var boxStyle = props.style ?? Style.ReadOrThemeProperty(in cx, ThemeProperties.ButtonFilled);
      boxStyle.RenderBoundary(ref cx, InputState);
      if (props.content != null) props.content.Invoke(ref cx);
    }

    protected override void OnClick(EventBase evt) {
      if (!props.enabled) return;
      props.action?.Call(Node);
    }
  }
}