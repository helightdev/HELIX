using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Entrypoint : MonoBehaviour {

#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")]
  private static extern string GetQueryParam(string key);
#endif

  private void Start() {
    try {
      var previewId = ReadQueryParam("preview");
      var widgetExamples = FindWidgetExamples();

      if (widgetExamples == null) {
        Debug.LogWarning("WidgetExamples host not found; preview query parameter was ignored.");
        return;
      }

      if (!widgetExamples.ShowPreview(previewId)) {
        widgetExamples.ShowGallery();
        Debug.LogWarning($"Unknown preview ID '{previewId}'. Rendering widget gallery.");
        return;
      }

      Debug.Log(string.IsNullOrWhiteSpace(previewId)
        ? "Rendering widget gallery."
        : $"Rendering widget preview: {previewId}");
    } catch (System.Exception e) {
      Debug.LogError($"Error retrieving query parameter: {e.Message}");
    }
  }

  private static WidgetExamples FindWidgetExamples() {
    var uiDocument = FindFirstObjectByType<UIDocument>();
    return uiDocument?.rootVisualElement.Q<WidgetExamples>("NewTestWidget")
           ?? uiDocument?.rootVisualElement.Q<WidgetExamples>();
  }

  private static string ReadQueryParam(string key) {
#if UNITY_WEBGL && !UNITY_EDITOR
    return GetQueryParam(key);
#else
    return ReadQueryParamFromUrl(Application.absoluteURL, key);
#endif
  }

  private static string ReadQueryParamFromUrl(string url, string key) {
    if (string.IsNullOrEmpty(url)) return string.Empty;

    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return string.Empty;

    var query = uri.Query;
    if (query.StartsWith("?")) query = query.Substring(1);
    if (string.IsNullOrEmpty(query)) return string.Empty;

    foreach (var pair in query.Split('&')) {
      var parts = pair.Split(new[] { '=' }, 2);
      var pairKey = Uri.UnescapeDataString(parts[0]);
      if (!string.Equals(pairKey, key, StringComparison.OrdinalIgnoreCase)) continue;

      return parts.Length > 1 ? Uri.UnescapeDataString(parts[1].Replace("+", " ")) : string.Empty;
    }

    return string.Empty;
  }

}
