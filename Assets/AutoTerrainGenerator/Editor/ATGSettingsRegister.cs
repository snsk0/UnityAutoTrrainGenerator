#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace AutoTerrainGenerator.Editors 
{
    internal static class ATGSettingsRegister
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

                //NoiseReaderを処理
                EditorGUILayout.PropertyField(_serializedObject.FindProperty(ATGSettingsData.NoiseReadersName));
                for (int i = 0; i < _settingsData.noiseReader.Count; i++)
                {
                    MonoScript script = _settingsData.noiseReader[i];
                    if (script == null)
                    {
                        break;
                    }

                    Type scriptType = script.GetClass();

                    //クラスがない場合(enumのみなど)
                    if (scriptType == null)
                    {
                        _settingsData.heightMapGenerators[i] = null;
                        break;
                    }

                    //インターフェースが実装されているかを確認
                    if (!scriptType.GetInterfaces().Contains(typeof(INoiseReader)))
                    {
                        _settingsData.noiseReader[i] = null;
                    }
                    else
                    {
                        _settingsData.noiseReader[i] = script;
                    }
                }

                //Genenratorを処理
                EditorGUILayout.PropertyField(_serializedObject.FindProperty(ATGSettingsData.HeightMapGeneratorsName));
                for(int i = 0; i < _settingsData.heightMapGenerators.Count; i++)
                {
                    MonoScript script = _settingsData.heightMapGenerators[i];
                    if(script == null)
                    {
                        break;
                    }

                    Type scriptType = script.GetClass();

                    //クラスがない場合(enumのみなど)
                    if (scriptType == null)
                    {
                        _settingsData.heightMapGenerators[i] = null;
                        break;
                    }

                    if (!scriptType.IsSubclassOf(typeof(HeightMapGeneratorBase)))
                    {
                        _settingsData.heightMapGenerators[i] = null;
                    }
                    else
                    {
                        _settingsData.heightMapGenerators[i] = script;
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
