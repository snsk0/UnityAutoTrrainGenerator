#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AutoTerrainGenerator.Sample
{
    [CustomEditor(typeof(SampleArlg04))]
    public class SampleArlg04Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty amplitudeProperty = serializedObject.FindProperty("_amplitude");
            amplitudeProperty.floatValue = EditorGUILayout.Slider(new GUIContent("êUïù"), amplitudeProperty.floatValue, 0, 1);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_frequency"));

            SerializedProperty loadThresholdProperty = serializedObject.FindProperty("_loadThreshold");
            loadThresholdProperty.floatValue = EditorGUILayout.Slider(new GUIContent("loadThreshold"), loadThresholdProperty.floatValue, 0, amplitudeProperty.floatValue);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
