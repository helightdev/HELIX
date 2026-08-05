using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public enum NavigationTransitionRole : byte { Entering, Exiting }

  public readonly struct NavigationTransitionFrame {
    internal NavigationTransitionFrame(
      NavigationOperationKind operation,
      NavigationDirection direction,
      NavigationTransitionRole role,
      float progress
    ) {
      Operation = operation;
      Direction = direction;
      Role = role;
      Progress = progress;
    }

    public NavigationOperationKind Operation { get; }
    public NavigationDirection Direction { get; }
    public NavigationTransitionRole Role { get; }
    public float Progress { get; }
    public bool IsPop => Operation is NavigationOperationKind.Pop or NavigationOperationKind.PopTo;
    public bool IsBackward => Direction == NavigationDirection.Backward ||
                              Direction == NavigationDirection.Automatic && IsPop;
  }

  public interface INavigationTransition {
    int DurationMs { get; }
    EasingMode Easing { get; }
    void Apply(VisualElement element, in NavigationTransitionFrame frame);
  }

  public static class NavigationTransitions {
    public static readonly INavigationTransition Instant = new InstantNavigationTransition();
    public static readonly INavigationTransition Fade = new FadeNavigationTransition(160, EasingMode.EaseOut);
    public static readonly INavigationTransition SlideHorizontal = new SlideNavigationTransition(
      Vector2.right, 220, EasingMode.EaseOut
    );
    public static readonly INavigationTransition Default = Fade;

    public static INavigationTransition FadeWith(
      int durationMs = 160,
      EasingMode easing = EasingMode.EaseOut
    ) => new FadeNavigationTransition(durationMs, easing);

    public static INavigationTransition Slide(
      Vector2 direction,
      int durationMs = 220,
      EasingMode easing = EasingMode.EaseOut
    ) => new SlideNavigationTransition(direction, durationMs, easing);

    internal static void Reset(VisualElement element) {
      if (element == null) return;
      element.style.opacity = 1f;
      element.style.translate = new Translate();
      element.style.scale = new Scale(Vector3.one);
    }

    private sealed class InstantNavigationTransition : INavigationTransition {
      public int DurationMs => 0;
      public EasingMode Easing => EasingMode.Linear;
      public void Apply(VisualElement element, in NavigationTransitionFrame frame) => Reset(element);
    }

    private sealed class FadeNavigationTransition : INavigationTransition {
      internal FadeNavigationTransition(int durationMs, EasingMode easing) {
        DurationMs = Mathf.Max(0, durationMs);
        Easing = easing;
      }

      public int DurationMs { get; }
      public EasingMode Easing { get; }

      public void Apply(VisualElement element, in NavigationTransitionFrame frame) {
        if (element == null) return;
        element.style.opacity = frame.Role == NavigationTransitionRole.Entering
          ? frame.Progress
          : 1f - frame.Progress;
      }
    }

    private sealed class SlideNavigationTransition : INavigationTransition {
      private readonly Vector2 _direction;

      internal SlideNavigationTransition(Vector2 direction, int durationMs, EasingMode easing) {
        _direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        DurationMs = Mathf.Max(0, durationMs);
        Easing = easing;
      }

      public int DurationMs { get; }
      public EasingMode Easing { get; }

      public void Apply(VisualElement element, in NavigationTransitionFrame frame) {
        if (element == null) return;
        var direction = frame.IsBackward ? -_direction : _direction;
        var distance = frame.Role == NavigationTransitionRole.Entering
          ? 100f * (1f - frame.Progress)
          : -100f * frame.Progress;
        element.style.translate = new Translate(
          new Length(direction.x * distance, LengthUnit.Percent),
          new Length(direction.y * distance, LengthUnit.Percent)
        );
      }
    }
  }
}
