#if UNITY_EDITOR
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;

namespace AutoTerrainGenerator.Editors
{
    [CustomEditor(typeof(GeneratorRidge))]
    public class GeneratorRidgeInspector : Editor
    {
        private void Awake()
        {
            SharedDefaultInspector.OnEnable(serializedObject, GetType());
        }

        private void OnDestroy()
        {
            SharedDefaultInspector.OnDisable(serializedObject, GetType());
        }

        public override void OnInspectorGUI()
        {
            SharedDefaultInspector.OnInspectorGUI(serializedObject);
        }
    }
}
#endif