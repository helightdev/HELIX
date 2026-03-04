using HELIX.Abstractions;
using HELIX.Extensions;
using HELIX.Types;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Layout {
    [UxmlElement]
    public partial class FlexAlign : SingleChildContainerWidget {
        private readonly VisualElement _bottomSpacer;
        private readonly VisualElement _leftSpacer;
        private readonly VisualElement _rightSpacer;
        private readonly VisualElement _slot;
        private readonly VisualElement _topSpacer;

        private float _horizontalAlign = 0.5f;
        private float _verticalAlign = 0.5f;

        public FlexAlign() {
            this.FlexContainer();
            _topSpacer = new Element("top-spacer");

            var verticalAlignElement = new Element("Vertical")
                .FlexContainer(Axis.Horizontal)
                .Sized(new Length(100, LengthUnit.Percent))
                .Tight();
            _bottomSpacer = new Element("bottom-spacer");
            _leftSpacer = new Element("left-spacer");
            _slot = new Element("Slot").Tight();
            _rightSpacer = new Element("right-spacer");

            hierarchy.Add(_topSpacer);
            hierarchy.Add(verticalAlignElement);
            hierarchy.Add(_bottomSpacer);

            verticalAlignElement.hierarchy.Add(_leftSpacer);
            verticalAlignElement.hierarchy.Add(_slot);
            verticalAlignElement.hierarchy.Add(_rightSpacer);

            Refresh();
        }

        public override VisualElement contentContainer => _slot;

        [UxmlAttribute]
        public float HorizontalAlign {
            get => _horizontalAlign;
            set {
                _horizontalAlign = value;
                Refresh();
            }
        }

        [UxmlAttribute]
        public float VerticalAlign {
            get => _verticalAlign;
            set {
                _verticalAlign = value;
                Refresh();
            }
        }

        public void Refresh() {
            var vertical = math.remap(-1f, 1f, 0f, 1f, _verticalAlign);
            var horizontal = math.remap(-1f, 1f, 0f, 1f, _horizontalAlign);
            _topSpacer.style.flexGrow = vertical * 100;
            _bottomSpacer.style.flexGrow = (1 - vertical) * 100;
            _leftSpacer.style.flexGrow = horizontal * 100;
            _rightSpacer.style.flexGrow = (1 - horizontal) * 100;
        }
    }
}