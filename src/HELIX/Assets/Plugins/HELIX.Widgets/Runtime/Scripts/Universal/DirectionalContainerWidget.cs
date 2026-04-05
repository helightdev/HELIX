using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public abstract class DirectionalContainerWidget : Widget {
        public IReadOnlyList<Widget> children;
        public Align crossAxisAlign = Align.Center;
        public float gap = 0f;
        public Justify mainAxisAlign = Justify.FlexStart;
        public bool reverse = false;
    }
}