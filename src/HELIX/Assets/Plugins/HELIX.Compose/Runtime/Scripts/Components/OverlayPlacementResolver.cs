using UnityEngine;

namespace HELIX.Compose {
  internal static class OverlayPlacementResolver {
    internal static void ResolveViewport(
      OverlayPlacement placement,
      Rect host,
      float width,
      float height,
      float margin,
      out float x,
      out float y
    ) {
      var horizontal = placement switch {
        OverlayPlacement.TopStart or OverlayPlacement.BottomStart or OverlayPlacement.Start => 0f,
        OverlayPlacement.TopEnd or OverlayPlacement.BottomEnd or OverlayPlacement.End => 1f,
        _ => 0.5f
      };
      var vertical = placement switch {
        OverlayPlacement.TopStart or OverlayPlacement.Top or OverlayPlacement.TopEnd => 0f,
        OverlayPlacement.BottomStart or OverlayPlacement.Bottom or OverlayPlacement.BottomEnd => 1f,
        _ => 0.5f
      };
      x = Mathf.Lerp(margin, host.width - width - margin, horizontal);
      y = Mathf.Lerp(margin, host.height - height - margin, vertical);
    }

    internal static void ResolveAnchored(
      OverlayPlacement placement,
      Rect anchor,
      float width,
      float height,
      out float x,
      out float y
    ) {
      if (placement is OverlayPlacement.Before or OverlayPlacement.After) {
        x = placement == OverlayPlacement.Before ? anchor.xMin - width : anchor.xMax;
        y = anchor.center.y - height * 0.5f;
        return;
      }

      x = placement switch {
        OverlayPlacement.Above or OverlayPlacement.Below => anchor.center.x - width * 0.5f,
        OverlayPlacement.AboveEnd or OverlayPlacement.BelowEnd => anchor.xMax - width,
        _ => anchor.xMin
      };
      y = IsAbove(placement) ? anchor.yMin - height : anchor.yMax;
    }

    internal static void FlipToFit(
      OverlayPlacement placement,
      Rect anchor,
      Rect host,
      float width,
      float height,
      ref float x,
      ref float y
    ) {
      if (IsBelow(placement) && y + height > host.height) y = anchor.yMin - height;
      else if (IsAbove(placement) && y < 0f) y = anchor.yMax;

      if (placement == OverlayPlacement.After && x + width > host.width) x = anchor.xMin - width;
      else if (placement == OverlayPlacement.Before && x < 0f) x = anchor.xMax;
    }

    internal static float StackDirection(OverlayPlacement placement) {
      return placement is OverlayPlacement.BottomStart or OverlayPlacement.Bottom or OverlayPlacement.BottomEnd
        ? -1f
        : 1f;
    }

    internal static float FiniteOrZero(float value) {
      return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
    }

    private static bool IsAbove(OverlayPlacement placement) {
      return placement is OverlayPlacement.AboveStart or OverlayPlacement.Above or OverlayPlacement.AboveEnd;
    }

    private static bool IsBelow(OverlayPlacement placement) {
      return placement is OverlayPlacement.BelowStart or OverlayPlacement.Below or OverlayPlacement.BelowEnd;
    }
  }
}