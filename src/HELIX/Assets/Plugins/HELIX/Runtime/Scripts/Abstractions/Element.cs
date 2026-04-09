using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HELIX.Abstractions {
    public class Element : VisualElement, IMultiChildContainer, ISingleChildContainer {
        public Element() { }

        public Element(string name) {
            this.name = name;
        }

        public virtual IEnumerable<VisualElement> Childs {
            get => Children();
            set {
                Clear();
                if (value == null) return;
                foreach (var child in value) Add(child);
            }
        }

        public virtual VisualElement Child {
            get {
                if (contentContainer.childCount == 0) return null;
                return contentContainer.ElementAt(0);
            }
            set {
                if (contentContainer.childCount > 0) { contentContainer.Clear(); }
                if (value != null) contentContainer.Add(value);
            }
        }
    }
}