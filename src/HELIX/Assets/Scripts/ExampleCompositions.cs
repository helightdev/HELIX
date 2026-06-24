using HELIX.Coloring;
using HELIX.NW;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace TestNamespace {
  public static partial class ExampleCompositions {
    public static ulong counter = 0;
    public static ulong clickCounter = 0;

    [Composition]
    private static void _MyComposition(ref Composition cx) {
      // using var exampleContext = cx.WriteContext<ExampleContext>();
      // exampleContext.value.counter = counter;

      using (cx.Flex(Axis.Vertical, main: Justify.Center, cross: Align.Center)) {
        cx.APPLY.Padding(10).Padding(20);
        var id = counter++;
        cx.Text($"Title");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello");
          cx.Text($"World");
        }

        // cx.Button(ButtonContent, static x => {
        //   clickCounter++;
        // }).Padding(20);


        if (cx.Conditional(counter / 100 % 2 == 0)) cx.Button("Click me Switch", static x => {
          clickCounter++;
          Debug.Log($"Click {clickCounter} Switch!");
        }).BackgroundColor(Colors.Green).Padding(20);


        cx.Button("Click me", static x => {
          clickCounter++;
          Debug.Log($"Click {clickCounter}!");
        });

        cx.Text($"Title2");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello").Padding(counter % 20);
          if (cx.Conditional(counter % 200 == 0)) cx.Text($"World").Padding(100);
          cx.Text($"Another");
        }

        //if (cx.Conditional(counter / 100 % 2 == 0)) cx.Boundary(ExampleCompositions2.MyComposition2);

        cx.Text($"Title3");
        using (cx.Flex(Axis.Horizontal)) {
          cx.Text($"Hello");
          cx.Text($"World");
        }
      }
    }

    private static readonly InlineComposable<StateFlag> ButtonContent = static (ref Composition cx, StateFlag state) => {
      var color = Colors.Black;
      if (state.Pressed()) color = Colors.Red;
      else if (state.Hovered()) color = Colors.Blue;

      cx.Text("Button").TextColor(color);
    };
  }


  [UxmlElement]
  public partial class ExampleVisualElement : VisualElement {
    public ExampleVisualElement() {
      var boundaryNode = new CompositionBoundaryNode {
        composable = ExampleCompositions.MyComposition
      };
      Add(boundaryNode);
      //RecompositionScope.MarkDirty(boundaryNode);
      schedule.Execute(() => {
        RecompositionScope.MarkDirty(boundaryNode);
      }).Every(0).Resume();
    }
  }
}