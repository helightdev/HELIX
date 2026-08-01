using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Editor {
  internal static class UIElementsDebuggerBridge {
    private const BindingFlags _allStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags _allInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static void Open(VisualElement element) {
      if (element?.panel == null) {
        Debug.LogWarning("Cannot inspect a boundary element which is not attached to a panel.");
        return;
      }
      try {
        const string typeName = "UnityEditor.UIElements.Debugger.UIElementsDebugger";
        var debuggerType = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(typeName, false)).FirstOrDefault(type => type != null) ??
                           Assembly.Load("UnityEditor.UIElementsModule").GetType(typeName, true);
        var debugger = debuggerType.GetMethod("CreateDebuggerWindow", _allStatic)?.Invoke(null, null) as EditorWindow;
        if (debugger == null) throw new MissingMethodException(debuggerType.FullName, "CreateDebuggerWindow");
        debugger.Show();
        InspectWhenInitialized(debugger, new WeakReference<VisualElement>(element), 5);
      } catch (Exception exception) {
        Debug.LogWarning($"Could not open the UI Toolkit Debugger: {exception.GetBaseException().Message}");
      }
    }

    private static void InspectWhenInitialized(EditorWindow debugger, WeakReference<VisualElement> element, int attempts) {
      EditorApplication.delayCall += () => {
        if (debugger == null || !element.TryGetTarget(out var target)) return;
        try {
          var implementation = debugger.GetType().GetField("m_DebuggerImpl", _allInstance)?.GetValue(debugger);
          if (implementation == null) {
            if (attempts > 0) InspectWhenInitialized(debugger, element, attempts - 1);
            return;
          }
          var inspect = implementation.GetType().GetMethod("InspectElement", _allInstance);
          if (inspect == null) throw new MissingMethodException(implementation.GetType().FullName, "InspectElement");
          inspect.Invoke(implementation, new object[] { target });
          debugger.GetType().GetMethod("ScrollToSelection", _allInstance)?.Invoke(debugger, null);
          debugger.Focus();
        } catch (Exception exception) {
          Debug.LogWarning($"Could not select the boundary in the UI Toolkit Debugger: {exception.GetBaseException().Message}");
        }
      };
    }
  }
}