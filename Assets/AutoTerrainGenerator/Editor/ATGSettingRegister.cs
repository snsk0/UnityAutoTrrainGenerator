#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class ATGSettingRegister
{
    private class ATGSettingsProvider : SettingsProvider
    {
        public ATGSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords)
        {
            label = "AutoTerrainGenerator";
        }

        public override void OnGUI(string searchContext)
        {
            Debug.Log(searchContext);
        }
    }


    [SettingsProvider]
    internal static SettingsProvider CreateSettingProvider()
    {
        return new ATGSettingsProvider("Project/", SettingsScope.Project, new[] { "AutoTerrainGenerator", "ATG" });
    }
}
#endif
