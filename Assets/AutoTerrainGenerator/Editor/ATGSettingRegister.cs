#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AutoTerrainGenerator.Editor {
    internal static class ATGSettingRegister
    {
        private class ATGSettingsProvider : SettingsProvider
        {
            private ATGSettingData _settingData;
            private SerializedObject _serializedObject;

            public ATGSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords)
            {
                label = "AutoTerrainGenerator";
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                _settingData = ATGSettingData.GetOrCreateData();
                _serializedObject = new SerializedObject(_settingData);
            }

            public override void OnDeactivate()
            {
                if(_serializedObject != null)
                {
                    ATGSettingData.SetConfigData(_settingData);
                }
            }

            public override void OnGUI(string searchContext)
            {
                _serializedObject.Update();

                EditorGUILayout.PropertyField(_serializedObject.FindProperty(ATGSettingData.HeightMapGeneratorsName));

                //classàÍóóÇéÊìæÇµÅAGeneratorà»äOnullÇë„ì¸Ç∑ÇÈ
                bool isGenerator = false;
                for(int i = 0; i < _settingData.heightMapGenerators.Count; i++)
                {
                    MonoScript script = _settingData.heightMapGenerators[i];
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
                        _settingData.heightMapGenerators[i] = null;
                    }
                }

                _serializedObject.ApplyModifiedProperties();
            }


        }

        internal class ATGSettingData : ScriptableObject
        {
            public const string HeightMapGeneratorsName = "_heightMapGenerators";

            [SerializeField]
            private List<MonoScript> _heightMapGenerators;
            public List<MonoScript> heightMapGenerators => _heightMapGenerators;

            internal static ATGSettingData GetOrCreateData()
            {
                string dataJson = EditorUserSettings.GetConfigValue(nameof(ATGSettingData));
                ATGSettingData settingData = CreateInstance<ATGSettingData>();

                if (!string.IsNullOrEmpty(dataJson))
                {
                    JsonUtility.FromJsonOverwrite(dataJson, settingData);
                }
                else
                {
                    settingData._heightMapGenerators = new List<MonoScript>();
                }
                return settingData;
            }

            internal static void SetConfigData(ATGSettingData settingData)
            {
                string dataJson = JsonUtility.ToJson(settingData);
                EditorUserSettings.SetConfigValue(nameof(ATGSettingData), dataJson);
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
