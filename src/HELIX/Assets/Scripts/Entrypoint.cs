using System.Runtime.InteropServices;
using UnityEngine;

public class Entrypoint : MonoBehaviour {

  [DllImport("__Internal")]
  private static extern string GetQueryParam(string key);

  private void Start() {
    try {
      var previewId = GetQueryParam("preview");

      switch(previewId) {
        case "WidgetA":
          // Enable Widget A
          break;
        case "WidgetB":
          // Enable Widget B
          break;
        default:
          // Render Default
          break;
      }

      Debug.Log($"Preview ID: {previewId}");
    } catch (System.Exception e) {
      Debug.LogError($"Error retrieving query parameter: {e.Message}");
      // Handle error, possibly render default view
    }
  }

}