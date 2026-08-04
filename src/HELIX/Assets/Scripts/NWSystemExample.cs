using System;
using System.Globalization;
using HELIX.Compose;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using HELIX.Widgets.Universal;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace HELIX.Examples {
  [BoundaryComposable(Extension = true, UseLookupCache = true)]
  public partial class MyBetterWidget {
    public partial struct Props {
      [Prop("HELIX.Coloring.Colors.Red", PropInit.Deferred)]
      public Color color;
    }

    protected override void OnRecompose(ref Composition cx) { }
  }

  [UxmlElement]
  public partial class NwSystemExampleElement : BoundaryVisualElement {
    public override void Compose(ref Composition cx) {
      this.Fill();
      cx.NWSystemExample();
      cx.CURSOR.Fill();
    }
  }

  [BoundaryComposable(Extension = true)]
  public partial class NWSystemExample {
    protected override void OnRecompose(ref Composition cx) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.Surface))
        .TextColor(theme.GetColor(ColorRoles.OnSurface));

      HomeComposable.ComposeBoundary(ref cx);
    }
  }

  [BoundaryComposable()]
  public partial class HomeComposable {
    public enum ExampleMode : byte { Balanced, Performance, Quality }

    public static readonly SliderOptions VolumeOptions = new(0f, 1f, step: 0f, thumbRange: 0.1f);
    public static readonly SliderOptions VolumeOptionsScroll = new(
      0f, 1f, step: 0f, thumbRange: 0.1f, axis: Axis.Vertical
    );
    public static readonly TextInputValueAdapter<float> DecimalValueAdapter = new(
      value => value.ToString("0.00", CultureInfo.InvariantCulture),
      (string text, out float value) => float.TryParse(
        text, NumberStyles.Float, CultureInfo.InvariantCulture, out value
      )
    );

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
      // Debug.Log(string.Join("\n", States.CommonFocusableSelectable));
      // Debug.Log(string.Join("\n", States.CommonFocusable));
      // Debug.Log(string.Join("\n", States.Common));

      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(16f);
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

        using (cx.Flex(Axis.Horizontal, cross: Align.Stretch)) {
          using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
            if (cx.CursorDirty) cx.CURSOR.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
            cx.Text("Text");
            cx.Spacing(1);
            cx.HXTextField(
              initialValue: new TextEditingValue("Hello World!"),
              onChanged: static (ctx, value) => {
                Debug.Log($"Text changed: {value}");
                using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value.text; }
              },
              processor: (ref TextEditProcessorContext context) => {
                Debug.Log(
                  $"Processor: {context.trigger}\n{context.next.ToFormattedString()}\n/\\ Becomes /\\\n{context.previous.ToFormattedString()}\n;{context.physical}”"
                );
              }
            );
            cx.HXTextField(
              initialValue: new TextEditingValue("Hello World!"),
              value: text,
              onChanged: static (ctx, value) => {
                Debug.Log($"Text changed: {value}");
                using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value.text; }
              },
              processor: (ref TextEditProcessorContext context) => {
                Debug.Log(
                  $"Processor: {context.trigger}\n{context.next.ToFormattedString()}\n/\\ Becomes /\\\n{context.previous.ToFormattedString()}\n;{context.physical}”"
                );
              }
            );
            // cx.TextInput(
            //   text,
            //   onChanged: (ctx, value) => {
            //     Debug.Log($"Text changed: {value}");
            //     using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value; }
            //   },
            //   onSubmitted: (ctx, value) => {
            //     Debug.Log($"Text submitted: {value}");
            //   },
            //   onEditingStarted: (ctx) => {
            //     Debug.Log($"Text editing started");
            //   },
            //   onEditingEnded: (ctx, value, reason) => {
            //     Debug.Log($"Text editing ended");
            //   }
            // );
            cx.Spacing(2);
            // cx.TextInput(
            //   text,
            //   options: new TextInputOptions(multiline: true),
            //   processor: (ref TextEditProcessorContext context) => {
            //     if (context.AbsoluteLengthDelta > 3) {
            //       context.next = context.previous;
            //       context.result = TextEditResult.Break();
            //       return;
            //     }
            //
            //     Debug.Log($"Processor: {context.trigger}\n{context.next.ToFormattedString()}\n/\\ Becomes /\\\n{context.previous.ToFormattedString()}\n;{context.physical}”");
            //
            //     //context.result = TextEditResult.Break(true);
            //   },
            //   onChanged: (ctx, value) => {
            //     Debug.Log($"Text changed: {value}");
            //     using (ctx.Modify<HomeComposable>(out var composable)) { composable.text = value; }
            //   },
            //   onSubmitted: (ctx, value) => {
            //     Debug.Log($"Text submitted: {value}");
            //   },
            //   onEditingStarted: (ctx) => {
            //     Debug.Log($"Text editing started");
            //   },
            //   onEditingEnded: (ctx, value, reason) => {
            //     Debug.Log($"Text editing ended: {value.text} {reason}");
            //   }
            // );
            cx.Spacing(2);

            // cx.Text("Integer");
            // cx.Spacing(1);
            // cx.TextInput(
            //   integer,
            //   TextInputAdapters.Int32,
            //   onChanged: static (ctx, value) => {
            //     using (ctx.Modify<HomeComposable>(out var composable)) { composable.integer = value; }
            //   }
            // );
            // cx.Spacing(2);
            //
            // cx.Text("Float");
            // cx.Spacing(1);
            // cx.TextInput(
            //   floatingPoint,
            //   DecimalAdapter,
            //   onChanged: static (ctx, value) => {
            //     using (ctx.Modify<HomeComposable>(out var state)) {
            //       state.floatingPoint = value;
            //       Debug.Log($"Float changed: {value}");
            //     }
            //   }
            // );
          }
          cx.Spacing(2);

          using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
            cx.CURSOR.Size(BoxConstraints.Only(min: new StyleLength2(260f, 0f)));
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
            cx.HXButton(
              static (ref Composition cx) => cx.Text("Filled"),
              selected: enabled,
              style: ThemeProperties.ButtonFilled[in cx], action: static (ctx) => {
                using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
              }
            );
            cx.Spacing(2);
            cx.HXButton(
              static (ref Composition cx) => cx.Text("Outlined"),
              selected: enabled,
              style: ThemeProperties.ButtonOutlined[in cx], action: static (ctx) => {
                using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
              }
            );
            cx.Spacing(2);
            cx.HXButton(
              static (ref Composition cx) => cx.Text("Toggle"),
              selected: enabled,
              style: ThemeProperties.ButtonToggle[in cx], action: static (ctx) => {
                using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
              }
            );
            cx.Spacing(2);
            cx.HXButton(
              static (ref Composition cx) => cx.Text("Ghost"),
              selected: enabled,
              style: ThemeProperties.ButtonGhost[in cx], action: static (ctx) => {
                using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
              }
            );
          }

          cx.Slider(
            volume,
            options: VolumeOptionsScroll,
            onChanged: static (ctx, value) => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.volume = value; }
            },
            style: ThemeProperties.Scroller[in cx]
          );
        }
        cx.Spacing(2);

        // var colors = new Color[] {
        //   MaterialColors.White,
        //   MaterialColors.Red,
        //   MaterialColors.Pink,
        //   MaterialColors.Purple,
        //   MaterialColors.DeepPurple,
        //   MaterialColors.Indigo,
        //   MaterialColors.Blue,
        //   MaterialColors.LightBlue,
        //   MaterialColors.Cyan,
        //   MaterialColors.Teal,
        //   MaterialColors.Green,
        //   MaterialColors.LightGreen,
        //   MaterialColors.Lime,
        //   MaterialColors.Yellow,
        //   MaterialColors.Amber,
        //   MaterialColors.Orange,
        //   MaterialColors.DeepOrange
        // };

        // for (var i = 0; i < colors.Length; i++) {
        //   using (cx.Flex(Axis.Horizontal, cross: Align.Stretch)) {
        //     cx.APPLY.Size(BoxConstraints.Tight(400, 32));
        //
        //     cx.DrawSolidBox(color: MaterialColors.Black).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(MaterialColors.Black, colors[i], 0.08f)).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(MaterialColors.Black, colors[i], 0.12f)).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(MaterialColors.Black, colors[i], 0.38f)).Flexible();
        //     cx.DrawSolidBox(color: colors[i]).Flexible();
        //     cx.Spacing(2);
        //     cx.DrawSolidBox(color: colors[i]).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.White, 0.08f)).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.White, 0.12f)).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.White, 0.38f)).Flexible();
        //     cx.DrawSolidBox(color: MaterialColors.White).Flexible();
        //     cx.Spacing(2);
        //     cx.DrawSolidBox(color: colors[i]).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.Black, 0.08f)).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.Black, 0.12f)).Flexible();
        //     cx.DrawSolidBox(color: Colors.ContrastBlend(colors[i], MaterialColors.Black, 0.38f)).Flexible();
        //     cx.DrawSolidBox(color: MaterialColors.Black).Flexible();
        //
        //   }
        // }
      }
    }
  }
}