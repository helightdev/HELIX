using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public abstract class DirectionalContainerWidget : MultiChildWidget {
        public Align crossAxisAlign = Align.Center;
        public float gap = 0f;
        public Justify mainAxisAlign = Justify.FlexStart;
        public bool reverse = false;
    }
}