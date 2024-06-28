#if UNITY_EDITOR
using System;
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
        private HeightMapGeneratorBase _target;
        public HeightMapGeneratorBase target => _target;

        public virtual void OnInspectorGUI() { }

        internal static GeneratorEditor CreateEditor(HeightMapGeneratorBase target, Type type)
        {
            GeneratorEditor editor = (GeneratorEditor)Activator.CreateInstance(type);
            editor._target = target;
            editor._serializedObject = new SerializedObject(target);

            return editor;
        }
    }
}
