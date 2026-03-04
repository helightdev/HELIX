using System.Collections.Generic;
using System.Linq;
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
            get => Children().FirstOrDefault();
            set {
                Clear();
                if (value != null) Add(value);
            }
        }
    }
}