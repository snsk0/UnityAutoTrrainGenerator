using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AutoTerrainGenerator
{
    internal class ATGSettingsData : ScriptableObject
    {
        private const string SettingAssetPath = "Assets/ATGSetting.asset";
        public const string HeightMapGeneratorsName = "_heightMapGenerators";

        [SerializeField]
        private List<MonoScript> _heightMapGenerators;
        public List<MonoScript> heightMapGenerators => _heightMapGenerators;

        internal static ATGSettingsData GetOrCreateData()
        {
            //ÉpÉXÇ©ÇÁì«Ç›çûÇ›
            ATGSettingsData settingData = AssetDatabase.LoadAssetAtPath<ATGSettingsData>(SettingAssetPath);

            if (settingData == null)
            {
                settingData = CreateInstance<ATGSettingsData>();
                settingData._heightMapGenerators = new List<MonoScript>();
                AssetDatabase.CreateAsset(settingData, SettingAssetPath);
            }

            return settingData;
        }
    }
}