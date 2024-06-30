using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AutoTerrainGenerator
{
    internal class ATGSettingsData : ScriptableObject, IATGSettingProvider
    {
        public const string HeightMapGeneratorsName = "_heightMapGenerators";
        public const string NoiseReadersName = "_noiseReaders";

        [SerializeField]
        private List<MonoScript> _heightMapGenerators;
        public List<MonoScript> heightMapGenerators => _heightMapGenerators;

        [SerializeField]
        private List<MonoScript> _noiseReaders;
        public List<MonoScript> noiseReader => _noiseReaders;

        internal static ATGSettingsData GetOrCreateData()
        {
            //パスから読み込み
            ATGSettingsData settingData = AssetDatabase.LoadAssetAtPath<ATGSettingsData>(IATGSettingProvider.SettingsPath);

            if (settingData == null)
            {
                settingData = CreateInstance<ATGSettingsData>();
                settingData._heightMapGenerators = new List<MonoScript>();
                settingData._noiseReaders = new List<MonoScript>();
                AssetDatabase.CreateAsset(settingData, IATGSettingProvider.SettingsPath);
            }

            return settingData;
        }

        public List<HeightMapGeneratorBase> GetGenerators()
        {
            List<HeightMapGeneratorBase> heightMapGenerators = new List<HeightMapGeneratorBase>();

            foreach(MonoScript generatorScript in _heightMapGenerators)
            {
                if(generatorScript != null)
                {
                    //Generatorデータの読み込み
                    HeightMapGeneratorBase generator = CreateInstance(generatorScript.GetClass()) as HeightMapGeneratorBase;
                    heightMapGenerators.Add(generator);
                }
            }
            return heightMapGenerators;
        }

        public List<INoiseReader> GetNoiseReaders()
        {
            List<INoiseReader> noiseReaders = new List<INoiseReader>();

            foreach (MonoScript noiseScript in _noiseReaders)
            {
                if (noiseScript != null)
                {
                    //Generatorデータの読み込み
                    INoiseReader noiseReader = Activator.CreateInstance(noiseScript.GetClass()) as INoiseReader;
                    noiseReaders.Add(noiseReader);
                }
            }
            return noiseReaders;
        }
    }
}