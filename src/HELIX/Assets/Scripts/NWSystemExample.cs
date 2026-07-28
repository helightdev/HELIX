using HELIX.Compose;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Universal;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace HELIX.Examples {

  [BoundaryComposable(Extension = true, UseLookupCache = true)]
  public partial class MyBetterWidget {

    public partial struct Props {
      [PropDefault("HELIX.Coloring.Colors.Red", PropInit.Deferred)]
      public Color color;
    }

    protected override void OnRecompose(ref Composition cx) {

    }
  }

  [UxmlElement]
  public partial class NwSystemExampleElement : BoundaryVisualElement {
    public override void Compose(ref Composition cx) {
      this.Fill();
      cx.NWSystemExample();
      cx.APPLY.Fill();
    }
  }

  public static partial class NwSystemsExample {

    [CompositionBoundary] public static partial void NWSystemExample(ref this Composition cx);

    public partial class NWSystemExampleComposable {
      protected override void OnRecompose(ref Composition cx) {
        var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
        cx.APPLY
          .BackgroundColor(theme.GetColor(ColorRoles.Surface))
          .TextColor(theme.GetColor(ColorRoles.OnSurface));

        Home(ref cx);
      }
    }


    [CompositionBoundary] public static partial void Home(ref Composition cx);

    public partial class HomeComposable {
      public enum ExampleMode : byte { Balanced, Performance, Quality }

      public static readonly SliderOptions VolumeOptions = new(0f, 1f, step: 0f, thumbRange: 0.1f);
      public static readonly NumericInputOptions DecimalOptions = new(format: "0.00");

      public string text = "Editable text";
      public int integer = 12;
      public float floatingPoint = 1.25f;
      public float volume = 0.65f;
      public bool enabled = true;
      public ExampleMode mode = ExampleMode.Balanced;

      private FontAsset _iconFont;

      public override void OnAttach(BoundaryData data, IBoundary boundary) {
        base.OnAttach(data, boundary);
        _iconFont = Resources.Load<FontAsset>("helix/fa/FontAwesome7FreeSolid");
      }

      protected override void OnRecompose(ref Composition cx) {
        var systemState = cx.Lookup<NWSystemExampleComposable>();
        using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
          cx.APPLY.Padding(16f);
          cx.Text("HELIX NW system example").TextRole(TextRole.TitleLarge);
          cx.Spacing(2);
          cx.Text("Controlled inputs");
          cx.Spacing(2);

          using (cx.Decorator(out var slots)) {
            using (slots.Prefix()) {
              HXDecorator.Label(
                ref cx,
                new LabelSpec("Prefix", new IconRef(FaSolidIcons.User.ToString(), _iconFont).Composable())
              );
            }
            using (slots.Element()) cx.Text("Element Data").TextRole(TextRole.BodyMedium);
            using (slots.Suffix()) HXDecorator.Label(ref cx, new LabelSpec("Suffix"));
            using (slots.Label()) HXDecorator.Label(ref cx, new LabelSpec("Label"));
            using (slots.Before()) HXDecorator.Label(ref cx, new LabelSpec("Before"));
            using (slots.After()) HXDecorator.Label(ref cx, new LabelSpec("After"));
            using (slots.Description()) HXDecorator.Label(ref cx, new LabelSpec("Description"));
          }

          using (cx.Flex(Axis.Horizontal, cross: Align.FlexStart)) {
            using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
              cx.APPLY.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
              cx.Text("Text");
              cx.Spacing(1);
              cx.TextInput(
                text,
                onChanged: (ctx, value) => {
                  using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value; }
                }
              );
              cx.Spacing(2);

              cx.Text("Integer");
              cx.Spacing(1);
              cx.IntInput(
                integer,
                onChanged: static (ctx, value) => {
                  using (ctx.Modify<HomeComposable>(out var composable)) { composable.integer = value; }
                }
              );
              cx.Spacing(2);

              cx.Text("Float");
              cx.Spacing(1);
              cx.FloatInput(
                floatingPoint,
                options: DecimalOptions,
                onChanged: static (ctx, value) => {
                  using (ctx.Modify<HomeComposable>(out var state)) { state.volume = value; }
                }
              );
            }
            cx.Spacing(2);

            using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
              cx.APPLY.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
              cx.Text("Volume");
              cx.Spacing(1);
              cx.Slider(
                volume,
                options: VolumeOptions,
                onChanged: static (ctx, value) => {
                  using (ctx.Modify<HomeComposable>(out var state)) { state.volume = value; }
                }
              );
              cx.Spacing(2);

              cx.Checkbox(
                enabled,
                onChanged: static (ctx, value) => {
                  using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = value; }
                }
              );
              cx.Spacing(2);
            }
          }
          cx.Spacing(2);
        }
      }
    }
  }
}