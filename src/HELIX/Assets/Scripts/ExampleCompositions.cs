using System;
using HELIX;
using HELIX.Coloring;
using HELIX.NW;
using HELIX.Types;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Universal;
using UnityEngine;
using UnityEngine.UIElements;

namespace TestNamespace {
  public static partial class ExampleCompositions {
    public static ulong counter = 0;
    public static ulong clickCounter = 0;


    public static readonly TextStyle LocalDefault = new(style: FontStyle.Bold);

    public static readonly Signal<int> counterSignal = Signal.Value(0);

    [Composition]
    private static void _MyComposition(ref Composition cx) {
      var theme = ThemeData.Context.ReadScopeOrDefault();

      cx.WriteContext(SpecConfiguration.Key, DefaultFactory);
      ref var defaultTextStyle = ref theme.GetTextStyleRef(TextRole.BodyMedium);
      TextStyle.WriteMerged(ref cx, in defaultTextStyle);
      defaultTextStyle.Apply(cx.boundary);

      // using var exampleContext = cx.WriteContext<ExampleContext>();
      // exampleContext.value.counter = counter;
      using (cx.Flex(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        cx.APPLY.Padding(10).Padding(20);

        //cx.APPLY.BackgroundColor(theme.GetColor(ColorRoles.Surface));

        var id = counter++;

        cx.Text($"Title");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello");
          cx.Text($"World");
        }
        cx.Text($"AfterGroup1");

        // cx.Button(ButtonContent, static x => {
        //   clickCounter++;
        // }).Padding(20);

        cx.DrawSolidBox(
          color: theme.GetColor(ColorRoles.SurfaceContainerLow),
          constraints: BoxConstraints.Preferred(64, 64)
        );
        cx.DrawSolidBox(
          color: theme.GetColor(ColorRoles.SurfaceContainer),
          constraints: BoxConstraints.Preferred(64, 64)
        );
        cx.DrawSolidBox(
          color: theme.GetColor(ColorRoles.SurfaceContainerHigh),
          constraints: BoxConstraints.Preferred(64, 64)
        );
        cx.DrawSolidBox(
          color: theme.GetColor(ColorRoles.SurfaceContainerHighest),
          constraints: BoxConstraints.Preferred(64, 64)
        );

        //if (cx.Conditional(counter / 100 % 2 == 0))
        // cx.Button(
        //   $"Click me Switch",
        //   static x => {
        //     clickCounter++;
        //     Debug.Log($"Click {clickCounter} Switch!");
        //   }
        // ).BackgroundColor(Colors.BlueGrey).Display(counter / 100 % 2 == 0);

        cx.Text($"AfterSwitch");
        cx.Button(
          static (ref Composition cx) => {
            cx.Text($"Click me {counterSignal.Value}");
          },
          static boundary => {
            counterSignal.Value++;
          },
          selected: true
        );
        cx.Text($"AfterButton");

        cx.Text($"Title2");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello").TextColor(new Color(counter % 20 / 20f, 0f, 0f));
          //if (cx.Conditional(counter % 200 == 0)) cx.Text($"World").Padding(100);
          cx.Text($"Another");
        }
        cx.Text($"Title2After");

        //if (cx.Conditional(counter / 100 % 2 == 0))
        cx.Boundary(InnerComposition);

        cx.TextField();

        cx.Text($"Title3");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello");
          cx.Text($"World");
        }
      }
    }

    [Composition]
    public static void _InnerComposition(ref Composition cx) {
      new ButtonSpecs {
        Label = "Click me from InnerComposition",
        OnClick = static x => {
          counterSignal.Value++;
          Debug.Log($"Click {counterSignal.Value} from InnerComposition!");
        }
      }.Compose(ref cx);
    }

    public static readonly SpecConfiguration DefaultFactory = new SpecConfiguration()
      .AddFactory<ButtonSpecs>(ButtonSpecDrawer);

    public static void ButtonSpecDrawer(ref Composition cx, in ButtonSpecs specs) {
      cx.Button(
        static (ref Composition cx) => {
          cx.Text("Click me Spec");
        },
        specs.OnClick
      );
    }

    public struct ButtonSpecs : ISpec {
      public string Label { get; set; }
      public Action<IBoundary> OnClick { get; set; }
    }

    // [CompositionBoundary]
    // public static partial void ClickRegion(
    //   this ref Composition cx
    // );
    //
    // public partial class ClickRegionState {
    //   protected override void OnAttach() {
    //     base.OnAttach();
    //   }
    // }

    private static readonly InlineComposable<StateFlag> ButtonContent =
      static (ref Composition cx, StateFlag state) => {
        var color = Colors.Black;
        if (state.Pressed()) color = Colors.Red;
        else if (state.Hovered()) color = Colors.Blue;

        cx.Text("Button").TextColor(color);
      };
  }


  [UxmlElement]
  public partial class ExampleVisualElement : VisualElement {
    public ExampleVisualElement() {
      var boundaryNode = new CompositionBoundaryNode { composable = ExampleCompositions.MyComposition };
      Add(boundaryNode);
      RecompositionScope.MarkDirty(boundaryNode);
      // schedule.Execute(() => {
      //     RecompositionScope.MarkDirty(boundaryNode);
      //   }
      // ).Every(0).Resume();
    }
  }
}