using UnityEngine;

namespace HELIX.Widgets {
    [ExecuteInEditMode]
    public class HelixRuntimeHelper : MonoBehaviour {
        private static HelixRuntimeHelper _instance;

        public static HelixRuntimeHelper Instance => _instance ? _instance : null;

        private void Awake() {
            _instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        private void LateUpdate() {
            if (_instance != this || !ModificationBarrier.UseRuntimeHelper) {
                if (Application.isEditor && !Application.isPlaying) DestroyImmediate(gameObject);
                else Destroy(gameObject);
                return;
            }

            ModificationBarrier.Run(() => { }); // Poll modification barrier
        }

        private void OnDestroy() {
            if (_instance == this) _instance = null;
        }

        public static void EnsureRunning() {
            if (_instance) return;
            var obj = new GameObject("[HelixRuntimeHelper]");
            _instance = obj.AddComponent<HelixRuntimeHelper>();
        }
    }
}