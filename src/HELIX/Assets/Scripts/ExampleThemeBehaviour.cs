using UnityEngine;

public class ExampleThemeBehaviour : MonoBehaviour {
    public ExampleThemeComponent component;

    private void Awake() {
        component.ApplyGlobal();
    }

    [ContextMenu("Push")]
    public void Push() {
        component.ApplyGlobal();
    }
}