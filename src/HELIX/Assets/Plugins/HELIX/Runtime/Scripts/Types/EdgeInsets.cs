using UnityEngine.UIElements;

namespace HELIX.Types {
  public static class EdgeInsets {
    public static readonly StyleLength4 Zero = new(0);

    public static StyleLength4 All(Length v) {
      return new StyleLength4(v);
    }

    public static StyleLength4 Symmetric(Length horizontal, Length vertical) {
      return new StyleLength4(horizontal, vertical, horizontal, vertical);
    }

    public static StyleLength4 Only(
      Length? left = null,
      Length? top = null,
      Length? right = null,
      Length? bottom = null
    ) {
      return new StyleLength4(
        left ?? Length.Auto(),
        top ?? Length.Auto(),
        right ?? Length.Auto(),
        bottom ?? Length.Auto()
      );
    }
  }
}