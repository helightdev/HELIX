using HELIX.Abstractions;
using HELIX.Extensions;
using HELIX.Widgets.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Navigation {
    
    [UxmlElement]
    public partial class ScaffoldElement : SingleChildWidgetBaseElement<Scaffold> {
        private readonly VisualElement _body;
        private readonly VisualElement _overlay;

        public ScaffoldElement() {
            _body = new Element("Body").Stretched().AddTo(hierarchy);
            _overlay = new Element("Overlay").Stretched().Pickable(false).AddTo(hierarchy);
        }

        public override VisualElement contentContainer => _body;

        public OverlayEntry AddOverlay(VisualElement element) {
            var entry = new OverlayEntry(element);
            _overlay.Add(entry);
            return entry;
        }

        public OverlayEntry AddOverlay(VisualElement element, Vector2 localPosition, Vector2 size) {
            var entry = new OverlayEntry(element, localPosition, size);
            _overlay.Add(entry);
            return entry;
        }

        public OverlayEntry AddAnchoredOverlay(VisualElement anchor, VisualElement overlay, bool link = true) {
            var entry = new OverlayEntry(overlay, anchor, link);
            _overlay.Add(entry);
            return entry;
        }

        public void RemoveOverlay(OverlayEntry entry) {
            if (entry == null) return;
            if (entry.parent != _overlay) return;
            entry.OnRemove();
            entry.RemoveFromHierarchy();
        }

        public static ScaffoldElement Get(VisualElement context) {
            return context.GetFirstAncestorOfType<ScaffoldElement>();
        }
    }
    
    public class Scaffold : SingleChildWidget {
        public override IWidgetElement CreateElement() => ReconcileInto(new ScaffoldElement());
    }

    public class OverlayEntry : VisualElement {
        private readonly IVisualElementScheduledItem _updateItem;

        public OverlayEntry(VisualElement element) {
            this.Stretched();
            element.AddTo(this);
        }

        public OverlayEntry(VisualElement element, Vector2 localPosition, Vector2 size) {
            this.Positioned(left: localPosition.x, top: localPosition.y)
                .Sized(size.x, size.y)
                .Tight()
                .Pickable(false);
            element.AddTo(this);
        }

        public OverlayEntry(VisualElement element, VisualElement anchor, bool link) {
            this.Tight().Pickable(false);
            element.AddTo(this);
            Reanchor(anchor);
            if (link) _updateItem = schedule.Execute(() => { Reanchor(anchor); }).Every(0);
        }

        public void Reanchor(VisualElement anchor) {
            var localPos = layout.position;
            var size = layout.size;
            if (anchor?.panel != null && anchor.layout != default && parent != null) {
                var worldPos = anchor.LocalToWorld(Vector2.zero);
                localPos = parent.WorldToLocal(worldPos);
                size = anchor.layout.size;
            }

            style.left = localPos.x;
            style.top = localPos.y;
            style.width = size.x;
            style.height = size.y;
            style.position = Position.Absolute;
        }

        public void OnRemove() {
            _updateItem?.Pause();
        }

        public void Pop() {
            ScaffoldElement.Get(this)?.RemoveOverlay(this);
        }
        
        public static OverlayEntry Nearest(VisualElement context) {
            return context.GetFirstAncestorOfType<OverlayEntry>();
        }
    }
}