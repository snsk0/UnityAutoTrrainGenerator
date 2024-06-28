#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace AutoTerrainGenerator.Editors 
{
    internal static class ATGSettingRegister
    {
        private class ATGSettingsProvider : SettingsProvider
        {
            private ATGSettingsData _settingsData;
            private SerializedObject _serializedObject;

            public ATGSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords)
            {
                label = "AutoTerrainGenerator";
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                _settingsData = ATGSettingsData.GetOrCreateData();
                _serializedObject = new SerializedObject(_settingsData);
            }

            public override void OnGUI(string searchContext)
            {
                _serializedObject.Update();

                EditorGUILayout.PropertyField(_serializedObject.FindProperty(ATGSettingsData.HeightMapGeneratorsName));

                //classˆê——‚ğæ“¾‚µAGeneratorˆÈŠOnull‚ğ‘ã“ü‚·‚é
                bool isGenerator = false;
                for(int i = 0; i < _settingsData.heightMapGenerators.Count; i++)
                {
                    MonoScript script = _settingsData.heightMapGenerators[i];
                    if(script == null)
                    {
                        break;
                    }

                    Type scriptType = script.GetClass();
                    foreach(Type iType in scriptType.GetInterfaces())
                    {
                        isGenerator = iType == typeof(IHeightMapGenerator);
                        if (isGenerator)
                        {
                            break;
                        }
                    }

                    if (!isGenerator)
                    {
                        _settingsData.heightMapGenerators[i] = null;
                    }
                }

                _serializedObject.ApplyModifiedProperties();
            }
        }

        [SettingsProvider]
        internal static SettingsProvider CreateSettingProvider()
        {
            return new ATGSettingsProvider("Project/", SettingsScope.Project, new[] { "AutoTerrainGenerator", "ATG" });
        }
    }
}
#endif
