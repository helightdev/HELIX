using System;
using System.Collections.Generic;
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
    public static readonly IReadOnlyList<DropdownOption<ExampleMode>> ModeOptions =
      new DropdownOption<ExampleMode>[] {
        new(ExampleMode.Balanced, "Balanced"),
        new(ExampleMode.Performance, "Performance"),
        new(ExampleMode.Quality, "Quality")
      };
    public static readonly IReadOnlyList<MenuItemSpec> ExampleMenuItems = new MenuItemSpec[] {
      MenuItemSpec.Heading("Actions"),
      new(
        "Toggle controls",
        static ctx => {
          using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
        }
      ),
      new("Selected action", static ctx => Debug.Log("Selected menu action"), selected: true),
      new("Unavailable action", enabled: false),
      MenuItemSpec.Separator(),
      MenuItemSpec.Submenu(
        "Mode",
        new MenuItemSpec[] {
          new(
            "Balanced",
            static ctx => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.mode = ExampleMode.Balanced; }
            }
          ),
          new(
            "Performance",
            static ctx => {
              using (ctx.Modify<HomeComposable>(out var state)) { state.mode = ExampleMode.Performance; }
            }
          ),
          MenuItemSpec.Submenu(
            "Quality",
            new MenuItemSpec[] {
              new(
                "High",
                static ctx => {
                  using (ctx.Modify<HomeComposable>(out var state)) { state.mode = ExampleMode.Quality; }
                }
              ),
              new("Ultra", static ctx => Debug.Log("Ultra quality selected"))
            }
          )
        }
      )
    };

    public string text = "Editable text";
    public int integer = 12;
    public float floatingPoint = 1.25f;
    public float volume = 0.65f;
    public bool enabled = true;
    public ExampleMode mode = ExampleMode.Balanced;

    private FontAsset _iconFont;
    private NavigationGraph _navigationGraph;
    private NavigationController _navigationController;
    private OverlayController _overlayController;

    public override void OnAttach(BoundaryData data, IBoundary boundary) {
      base.OnAttach(data, boundary);
      _iconFont = Resources.Load<FontAsset>("helix/fa/FontAwesome7FreeSolid");
      _navigationGraph = NavigationGraph.Builder("navigation-home")
        .Route("navigation-home", ComposeNavigationHome)
        .Route("navigation-details", ComposeNavigationDetails)
        .Build();
      _navigationController = new NavigationController(_navigationGraph);
      _overlayController = new OverlayController();
    }

    public override void OnDetach(BoundaryData data, IBoundary boundary) {
      _navigationController?.Dispose();
      _overlayController?.Dispose();
      _navigationController = null;
      _overlayController = null;
      _navigationGraph = null;
      base.OnDetach(data, boundary);
    }

    protected override void OnRecompose(ref Composition cx) {
      // Debug.Log(string.Join("\n", States.CommonFocusableSelectable));
      // Debug.Log(string.Join("\n", States.CommonFocusable));
      // Debug.Log(string.Join("\n", States.Common));

      using (cx.OverlayHost(_overlayController))
      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(16f);
        cx.Text("HELIX NW system example").TextRole(TextRole.TitleLarge);
        cx.Spacing(2);

        cx.Text("Navigation and overlays").TextRole(TextRole.TitleMedium);
        cx.Spacing(1);
        using (cx.Flex(Axis.Horizontal, cross: Align.Stretch)) {
          using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
            cx.Text("Declarative navigation").TextRole(TextRole.LabelLarge);
            cx.Spacing(1);
            cx.NavigationHost(_navigationGraph, _navigationController)
              .Height(220f).Width(500)
              .BorderRadius(12f)
              .Overflow(Overflow.Hidden);
            cx.Spacing(1);
            cx.Button(
              static (ref Composition child) => child.Text("Reset navigation"),
              style: ThemeProperties.ButtonOutlined[in cx],
              action: static ctx => ctx.Lookup<HomeComposable>()?._navigationController?.Reset()
            );
          }

          cx.Spacing(2);
          using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
            cx.Text("Overlay builders").TextRole(TextRole.LabelLarge);
            cx.Spacing(1);
            cx.Button(
              static (ref Composition child) => child.Text("Open modal"),
              action: static ctx => Overlay.Build(ComposeExampleModal)
                .Modal(dismissOnOutsidePointer: true)
                .Constraints(BoxConstraints.Only(
                  min: new StyleLength2(360f, 0f),
                  max: new StyleLength2(520f, StyleKeyword.None)
                ))
                .DismissOnCancel()
                .Show(ctx)
            );
            cx.Spacing(1);
            cx.Button(
              static (ref Composition child) => child.Text("Open anchored menu"),
              style: ThemeProperties.ButtonOutlined[in cx],
              action: static ctx => Overlay.Build(ComposeExampleMenu)
                .AnchorTo(ctx.element, OverlayPlacement.BelowStart, new Vector2(0f, 6f))
                .MatchAnchorWidth()
                .DismissOnOutsidePointer()
                .CaptureFocus()
                .Show(ctx)
            );
            cx.Spacing(1);
            cx.Button(
              static (ref Composition child) => child.Text("Show notification"),
              style: ThemeProperties.ButtonGhost[in cx],
              action: static ctx => Overlay.Build(ComposeExampleNotification)
                .At(OverlayPlacement.TopEnd)
                .Stacked(spacing: 8f)
                .Timeout(3500)
                .Show(ctx)
            );
          }

          cx.Spacing(2);
          using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
            cx.Text("Themed popup controls").TextRole(TextRole.LabelLarge);
            cx.Spacing(1);
            cx.DropdownButton(
              mode,
              ModeOptions,
              onChanged: static (ctx, value) => {
                using (ctx.Modify<HomeComposable>(out var state)) { state.mode = value; }
              },
              placeholder: "Choose a mode"
            ).Width(220f);
            cx.Spacing(1);
            cx.MenuButton(
              static (ref Composition child) => child.Text("Open menu"),
              ExampleMenuItems
            ).Width(220f);
          }
        }
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
            cx.TextField(
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
            cx.TextField(
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
            cx.Button(
              static (ref Composition cx) => cx.Text("Filled"),
              selected: enabled,
              style: ThemeProperties.ButtonFilled[in cx], action: static (ctx) => {
                using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
              }
            );
            cx.Spacing(2);
            cx.Button(
              static (ref Composition cx) => cx.Text("Outlined"),
              selected: enabled,
              style: ThemeProperties.ButtonOutlined[in cx], action: static (ctx) => {
                using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
              }
            );
            cx.Spacing(2);
            cx.Button(
              static (ref Composition cx) => cx.Text("Toggle"),
              selected: enabled,
              style: ThemeProperties.ButtonToggle[in cx], action: static (ctx) => {
                using (ctx.Modify<HomeComposable>(out var state)) { state.enabled = !state.enabled; }
              }
            );
            cx.Spacing(2);
            cx.Button(
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

    private static void ComposeNavigationHome(ref Composition cx, NavigationContextData navigation) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainerLow))
        .TextColor(theme.GetColor(ColorRoles.OnSurface));
      using (cx.Flex(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(16f);
        cx.Text("Navigation home").TextRole(TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text("Each destination is its own retained HELIX boundary.");
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Push details"),
          action: static ctx => ctx.NavigationController()?.Navigate(
            "navigation-details",
            NavigationArguments.Empty.With("message", "Arguments are retained with the back-stack entry.")
          )
        );
      }
    }

    private static void ComposeNavigationDetails(ref Composition cx, NavigationContextData navigation) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SecondaryContainer))
        .TextColor(theme.GetColor(ColorRoles.OnSecondaryContainer));
      using (cx.Flex(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(16f);
        cx.Text("Details route").TextRole(TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(navigation.arguments.Get("message", "No route argument was supplied."));
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Pop route"),
          style: ThemeProperties.ButtonOutlined[in cx],
          action: static ctx => ctx.NavigationController()?.Pop()
        );
      }
    }

    private static void ComposeExampleModal(ref Composition cx, OverlayContextData overlay) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainer))
        .TextColor(theme.GetColor(ColorRoles.OnSurfaceContainer))
        .BorderRadius(16f);
      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(20f);
        cx.Text("Composable modal").TextRole(TextRole.TitleLarge);
        cx.Spacing(1);
        cx.Text("The modal, its barrier, focus policy, and dismissal rules are all an overlay entry.").TextRole(TextRole.BodySmall);
        cx.Spacing(2);
        cx.Button(
          static (ref Composition child) => child.Text("Close"),
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
      }
    }

    private static void ComposeExampleMenu(ref Composition cx, OverlayContextData overlay) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainerHighest))
        .TextColor(theme.GetColor(ColorRoles.OnSurface))
        .BorderRadius(10f);
      using (cx.Flex(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(8f);
        cx.Button(
          static (ref Composition child) => child.Text("First menu action"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
        cx.Button(
          static (ref Composition child) => child.Text("Second menu action"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
      }
    }

    private static void ComposeExampleNotification(ref Composition cx, OverlayContextData overlay) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.PrimaryContainer))
        .TextColor(theme.GetColor(ColorRoles.OnPrimaryContainer))
        .BorderRadius(12f);
      using (cx.Flex(Axis.Horizontal, cross: Align.Center)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(12f);
        cx.Text("A stacked notification that dismisses after 3.5 seconds.").Flexible();
        cx.Spacing(1);
        cx.Button(
          static (ref Composition child) => child.Text("Dismiss"),
          style: ThemeProperties.ButtonGhost[in cx],
          action: static ctx => ctx.OverlayEntry()?.Dismiss()
        );
      }
    }
  }
}
