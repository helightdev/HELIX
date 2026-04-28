using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Prompts.Kenny {
  [CreateAssetMenu(fileName = "SVGS", menuName = "Prompts/KennyPromptSvgCollection", order = 0)]
  public class KennyPromptSvgCollection : ScriptableObject {
    public VectorImage[] prompts;
  }
}