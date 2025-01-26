namespace Editor {
    using UnityEditor;
    using UnityEditor.UI;
    using UnityEngine.UI;

    namespace Astral.Editor {
        [CustomEditor(typeof(CustomCanvasScaler), true)]
        [CanEditMultipleObjects]
        public class AstralCanvasScalerEditor : CanvasScalerEditor {
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();

                serializedObject.Update();

                SerializedProperty scaleModeProperty = serializedObject.FindProperty("m_UiScaleMode");
                CanvasScaler.ScaleMode scaleMode = (CanvasScaler.ScaleMode)scaleModeProperty.intValue;

                if (scaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_scaleFactorMultiplier"));
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}