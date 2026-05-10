using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
  public abstract class SingletonModifier : Modifier {
    public abstract void Hook(VisualElement element);
    public abstract void Unhook(VisualElement element);

    public override void Apply(VisualElement element) {
      Hook(element);
    }

    public override void Reset(VisualElement element) {
      Unhook(element);
    }

    public override void Apply(Modifier prev, VisualElement element) {
      if (prev is SingletonModifier prevSingleton) {
        if (ReferenceEquals(this, prevSingleton)) return;
        prevSingleton.Unhook(element);
      }

      Hook(element);
    }
  }
}