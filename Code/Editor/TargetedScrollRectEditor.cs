using UnityEditor;
using UnityEditor.UI;

namespace UnityCommonFeatures
{
    [CustomEditor(typeof(TargetedScrollRect), true)]
    public class TargetedScrollRectEditor : ScrollRectEditor
    {
        SerializedProperty _magneticFloatVertical;
        SerializedProperty _magneticFloatHorizontal;

        protected override void OnEnable()
        {
            base.OnEnable();
            _magneticFloatVertical = serializedObject.FindProperty(nameof(_magneticFloatVertical));
            _magneticFloatHorizontal = serializedObject.FindProperty(nameof(_magneticFloatHorizontal));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(15);
            EditorGUILayout.PropertyField(_magneticFloatVertical);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_magneticFloatHorizontal);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
