#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Utils
{
    [InitializeOnLoad]
    public static class PrePlayModeRecompiler
    {
        private const string EnableKey = "PrePlayModeRecompiler_Enabled";
        private static bool _isEnabled;
        private static bool _isWaitingForCompilation;

        static PrePlayModeRecompiler()
        {
            LoadSettings();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        private static void LoadSettings()
        {
            _isEnabled = EditorPrefs.GetBool(EnableKey, false);
        }

        private static void SaveSettings()
        {
            EditorPrefs.SetBool(EnableKey, _isEnabled);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!_isEnabled) return;

            if (state == PlayModeStateChange.ExitingEditMode && !_isWaitingForCompilation)
            {
                Debug.Log("[PrePlayModeRecompiler] Intercepted play mode - refreshing assets first...");

                _isWaitingForCompilation = true;
                EditorApplication.isPlaying = false;

                // Just refresh assets - this will trigger compilation if needed
                AssetDatabase.Refresh();

                EditorApplication.isPlaying = true;
            }
        }

        private static void OnCompilationFinished(object obj)
        {
            if (!_isEnabled || !_isWaitingForCompilation) return;

            _isWaitingForCompilation = false;

            EditorApplication.isPlaying = true;
        }

        #region Menu Items

        [MenuItem("Tools/Pre-PlayMode Recompiler/Enable")]
        private static void EnableRecompiler()
        {
            _isEnabled = true;
            SaveSettings();
            Debug.Log("[PrePlayModeRecompiler] ENABLED - scripts will recompile before entering play mode");
        }

        [MenuItem("Tools/Pre-PlayMode Recompiler/Disable")]
        private static void DisableRecompiler()
        {
            _isEnabled = false;
            _isWaitingForCompilation = false;
            SaveSettings();
            Debug.Log("[PrePlayModeRecompiler] DISABLED");
        }

        [MenuItem("Tools/Pre-PlayMode Recompiler/Enable", true)]
        private static bool EnableRecompilerValidate()
        {
            return !_isEnabled;
        }

        [MenuItem("Tools/Pre-PlayMode Recompiler/Disable", true)]
        private static bool DisableRecompilerValidate()
        {
            return _isEnabled;
        }

        #endregion
    }
}
#endif