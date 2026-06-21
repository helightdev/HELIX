using NUnit.Framework;
using UnityEngine.UIElements;
using UnityEngine.UIElements.TestFramework;

namespace HELIX.Widgets.Tests {
  public class BasicUITest : UITestFixture {
    [Test]
    public void EditorPanelTest()
    {
      // Use the rootVisualElement property to add elements
      // to your UI.
      rootVisualElement.Add(new Button() { name = "MyButton" });

      // Ensure the panel's UI is up to date.
      simulate.FrameUpdate();

      Button button = rootVisualElement.Q<Button>("MyButton");
      Assert.That(button, Is.Not.Null);

      // Test steps.
      // ...
    }
  }
}