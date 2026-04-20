using HELIX.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Navigation.Transitions {
  [UxmlObject]
  public partial class SlidePageTransition : PageTransition {
    public SlidePageTransition() { }

    public SlidePageTransition(
      Vector2 slideDirection,
      int durationMs = 200,
      EasingMode easing = EasingMode.Linear,
      bool invertForPop = false
    ) {
      Duration = durationMs;
      Easing = easing;
      InvertForPop = invertForPop;
      SlideDirection = slideDirection;
    }

    public SlidePageTransition(
      Vector2 positiveSlideDirection,
      Vector2 negativeSlideDirection,
      int durationMs = 200,
      EasingMode easing = EasingMode.Linear,
      bool invertForPop = false
    ) {
      Duration = durationMs;
      Easing = easing;
      PositiveSlideDirection = positiveSlideDirection;
      NegativeSlideDirection = negativeSlideDirection;
      InvertForPop = invertForPop;
    }

    [UxmlAttribute] public int Duration { get; set; } = 200;

    [UxmlAttribute] public EasingMode Easing { get; set; } = EasingMode.Linear;

    [UxmlAttribute] public Vector2 PositiveSlideDirection { get; set; } = Vector2.up;

    [UxmlAttribute] public Vector2 NegativeSlideDirection { get; set; } = Vector2.up;

    public Vector2 SlideDirection {
      set {
        PositiveSlideDirection = value;
        NegativeSlideDirection = value;
      }
    }

    [UxmlAttribute] public bool InvertForPop { get; set; }

    public override void Start(PageTransitionContext context) {
      var added = context.modificationResult.AddedPages;
      var removed = context.modificationResult.RemovedPages;

      var slideDirection = PositiveSlideDirection * new Vector2(-1, 1);
      var negativeSlideDirection = NegativeSlideDirection * new Vector2(-1, 1);
      if (context.modificationResult.Type == NavModificationType.Pop && InvertForPop) {
        slideDirection = -slideDirection;
        negativeSlideDirection = -negativeSlideDirection;
      }

      foreach (var page in added) {
        page.style.display = DisplayStyle.Flex;
        page.style.translate = new Translate(
          new Length(slideDirection.x * -100, LengthUnit.Percent),
          new Length(slideDirection.y * -100, LengthUnit.Percent)
        );
      }

      foreach (var page in removed) {
        page.style.translate = new Translate();
        page.style.display = DisplayStyle.Flex;
      }

      context.ScheduledTransition(scheduler => scheduler.Tween(
          Duration,
          t => {
            t = Easing.Eval(t);
            foreach (var page in added) {
              page.style.translate = new Translate(
                new Length((1 - t) * slideDirection.x * 100, LengthUnit.Percent),
                new Length((1 - t) * slideDirection.y * 100, LengthUnit.Percent)
              );
            }

            foreach (var page in removed) {
              page.style.translate = new Translate(
                new Length(t * negativeSlideDirection.x * -100, LengthUnit.Percent),
                new Length(t * negativeSlideDirection.y * -100, LengthUnit.Percent)
              );
            }
          }
        )
      );
    }

    public override void Finish(PageTransitionContext context) {
      foreach (var page in context.modificationResult.AddedPages) page.style.translate = new Translate();

      foreach (var page in context.modificationResult.RemovedPages) page.style.translate = new Translate();
    }
  }
}