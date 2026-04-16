using UnityEngine.UIElements;

namespace HELIX.Types
{
    public static class EdgeInsets {
        public static readonly StyleLength4 Zero = new(0);
        public static StyleLength4 All(Length v) => new(v);

        public static StyleLength4 Symmetric(Length horizontal, Length vertical) =>
            new(horizontal, vertical, horizontal, vertical);

        public static StyleLength4 Only(
            Length? left = null,
            Length? top = null,
            Length? right = null,
            Length? bottom = null
        ) =>
            new(left ?? Length.Auto(), top ?? Length.Auto(), right ?? Length.Auto(), bottom ?? Length.Auto());
    }
}