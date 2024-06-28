using System.Collections.Generic;

namespace AutoTerrainGenerator {
    internal interface IATGSettingProvider
    {
        public const string SettingsPath = "Assets/ATGSettings.asset";
        List<IHeightMapGenerator> GetGenerators();
    }
}
