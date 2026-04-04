using UnityEngine.UIElements;

namespace HELIX.Types
{
    public static class EdgeInsets {
        public static readonly StyleLength4 Zero = new(0);
        public static StyleLength4 All(Length v) => new(v);

        public static StyleLength4 Symmetric(Length horizontal, Length vertical) =>
            new(horizontal, vertical, horizontal, vertical);

        public static StyleLength4 Only(
            Length left = default,
            Length top = default,
            Length right = default,
            Length bottom = default
        ) =>
            new(left, top, right, bottom);
    }
}