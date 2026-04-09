using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HELIX.Abstractions {
    public interface ISingleChildContainer {
        VisualElement Child { get; set; }

        static VisualElement FirstGet(VisualElement target) {
            if (target.childCount == 0) return null;
            return target.ElementAt(0);
        }

        static void FirstSet(VisualElement target, VisualElement value) {
            if (target.childCount > 0) target.Clear();
            if (value != null) target.Add(value);
        }
        
        static VisualElement LastGet(VisualElement target, int without) {
            if (target.childCount <= without) return null;
            return target.ElementAt(target.childCount - 1);
        }

        static void LastSet(VisualElement target, int without, VisualElement element) {
            while (target.childCount > without) { target.RemoveAt(target.childCount - 1); }
            if (element != null) target.Add(element);
        }
    }

    public interface IMultiChildContainer {
        IEnumerable<VisualElement> Childs { get; set; }
    }

    public interface IElement {
        VisualElement Element { get; }
    }
}