using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Prompts {
  [CreateAssetMenu(fileName = "HelixSvgCollection", menuName = "Prompts/HelixSvgCollection", order = 0)]
  public class HelixSvgCollection : ScriptableObject {
    public VectorImage[] prompts;
  }
}