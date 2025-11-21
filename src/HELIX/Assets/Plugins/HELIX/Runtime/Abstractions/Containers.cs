using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HELIX.Abstractions {
    public interface ISingleChildContainer {
        VisualElement Child { get; set; }
    }

    public interface IMultiChildContainer {
        IEnumerable<VisualElement> Childs { get; set; }
    }
}