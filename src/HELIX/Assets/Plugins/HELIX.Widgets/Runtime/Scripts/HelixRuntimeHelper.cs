using System;
using HELIX.Widgets.Prompts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace HELIX.Widgets {
  [ExecuteInEditMode]
  public class HelixRuntimeHelper : MonoBehaviour {
    private static HelixRuntimeHelper _instance;

    public static HelixRuntimeHelper Instance => _instance ? _instance : null;

    private IDisposable _inputDeviceListener;

    private void Awake() {
      _instance = this;
      if (Application.isPlaying) DontDestroyOnLoad(gameObject);
      gameObject.hideFlags = HideFlags.HideAndDontSave;
    }

    private void LateUpdate() {
      if (_instance != this) {
        if (Application.isEditor && !Application.isPlaying) DestroyImmediate(gameObject);
        else Destroy(gameObject);
        return;
      }

      if (ModificationBarrier.UseRuntimeHelper) ModificationBarrier.Run(() => { }); // Poll modification barrier
    }

    public void AddInputDeviceListener() {
      if (_inputDeviceListener != null) return;
      _inputDeviceListener = InputSystem.onAnyButtonPress.Call(OnInputPressed);
    }

    private void OnInputPressed(InputControl obj) {
      var platform = HelixInputHelper.DetectInputFromAction(obj);
      HelixInputController.Instance.SetValue(platform);
    }

    public void RemoveInputDeviceListener() {
      _inputDeviceListener?.Dispose();
      _inputDeviceListener = null;
    }

    private void OnDestroy() {
      RemoveInputDeviceListener();
      if (_instance == this) _instance = null;
    }

    public static HelixRuntimeHelper EnsureRunning() {
      if (_instance) return _instance;
      var obj = new GameObject("[HelixRuntimeHelper]");
      _instance = obj.AddComponent<HelixRuntimeHelper>();
      return _instance;
    }
  }
}