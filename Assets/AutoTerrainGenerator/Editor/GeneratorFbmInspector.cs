#if UNITY_EDITOR
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;

namespace AutoTerrainGenerator.Editors
{
    [CustomEditor(typeof(GeneratorFbm))]
    public class GeneratorFbmInspector : Editor
    {
        private void Awake()
        {
            SharedDefaultInspector.Awake(serializedObject, GetType());
        }

        private void OnDestroy()
        {
            SharedDefaultInspector.OnDestroy(serializedObject, GetType());
        }

        public override void OnInspectorGUI()
        {
            SharedDefaultInspector.OnInspectorGUI(serializedObject);
        }
    }
}
#endif
