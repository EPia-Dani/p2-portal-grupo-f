using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var mono = (MonoBehaviour)target;
            var methods = mono.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var button = method.GetCustomAttribute<ButtonAttribute>();
                if (button == null) continue;

                EditorGUILayout.Space(button.spacing);

                var text = string.IsNullOrEmpty(button.buttonText) ? ObjectNames.NicifyVariableName(method.Name) : button.buttonText;
                if (GUILayout.Button(text))
                {
                    method.Invoke(mono, null);
                }

                return;
            }
        }
    }
}