using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AutoTerrainGenerator
{
    internal class ATGSettingsData : ScriptableObject, IATGSettingProvider
    {
        public const string HeightMapGeneratorsName = "_heightMapGenerators";

        [SerializeField]
        private List<MonoScript> _heightMapGenerators;
        public List<MonoScript> heightMapGenerators => _heightMapGenerators;

        internal static ATGSettingsData GetOrCreateData()
        {
            //ÉpÉXÇ©ÇÁì«Ç›çûÇ›
            ATGSettingsData settingData = AssetDatabase.LoadAssetAtPath<ATGSettingsData>(IATGSettingProvider.SettingsPath);

            if (settingData == null)
            {
                settingData = CreateInstance<ATGSettingsData>();
                settingData._heightMapGenerators = new List<MonoScript>();
                AssetDatabase.CreateAsset(settingData, IATGSettingProvider.SettingsPath);
            }

            return settingData;
        }

        public List<IHeightMapGenerator> GetGenerators()
        {
            List<IHeightMapGenerator> heightMapGenerators = new List<IHeightMapGenerator>();

            foreach(MonoScript generatorScript in _heightMapGenerators)
            {
                if(generatorScript != null)
                {
                    IHeightMapGenerator generator = (IHeightMapGenerator)Activator.CreateInstance(generatorScript.GetClass());
                    heightMapGenerators.Add(generator);
                }
            }
            return heightMapGenerators;
        }
    }
}