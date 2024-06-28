#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AutoTerrainGenerator
{
    public abstract class GeneratorEditor
    {
#if UNITY_EDITOR
        private SerializedObject _serializedObject;
        public SerializedObject serializedObject => _serializedObject;
#endif
        private IHeightMapGenerator _target;
        public IHeightMapGenerator target => _target;

        public virtual void OnInspectorGUI() { }
      }
}
