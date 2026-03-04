using HELIX.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Input {
    public class GenericTextInput : BaseWidget {
        public GenericTextInput() {
            this.WithStylesheet("helix");
        }

        public class InputRenderer : TextField {

        }
    }
}