#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;

namespace AutoTerrainGenerator.Editor
{
    internal class ATGEditorWindow : EditorWindow
    {
        [Serializable]
        private struct ATGWindowSettigs
        {
            //GUI
            public bool isFoldoutNoise;
            public bool isFoldoutHeightMap;
            public bool isFoldoutAsset;

            //地形生成関連パラメータ
            public HeightMapGeneratorData generatorData;

            //アセット
            public bool isCreateAsset;
            public string assetPath;
            public string assetName;
        }

        private SerializedObject _serializedObject;
        private ATGWindowSettigs _windowSettings;
        private HeightMapGeneratorData generatorData => _windowSettings.generatorData;

        //TODO 実装
        private float _step;

        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        private void OnEnable()
        {
            string windowSettings = EditorUserSettings.GetConfigValue(nameof(_windowSettings));
            if (!string.IsNullOrEmpty(windowSettings))
            {
                _windowSettings = JsonUtility.FromJson<ATGWindowSettigs>(windowSettings);
            }
            else
            {
                _windowSettings = new ATGWindowSettigs();
                _windowSettings.isFoldoutNoise = true;
                _windowSettings.isFoldoutHeightMap = true;
                _windowSettings.isFoldoutAsset = true;
                _windowSettings.isCreateAsset = true;
                _windowSettings.assetPath = "Assets";
                _windowSettings.assetName = "Terrain";
                _windowSettings.generatorData = new HeightMapGeneratorData();
            }

            _serializedObject = new SerializedObject(this);
        }

        private void OnDisable()
        {
            //デシリアライズして保存
            EditorUserSettings.SetConfigValue(nameof(_windowSettings), JsonUtility.ToJson(_windowSettings));
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _windowSettings.isFoldoutNoise = EditorGUILayout.Foldout(_windowSettings.isFoldoutNoise, "Noise");
            if(_windowSettings.isFoldoutNoise)
            {
                _windowSettings.generatorData.noiseTypeIndex = EditorGUILayout.Popup(new GUIContent("ノイズ"), _windowSettings.generatorData.noiseTypeIndex, new[]
                {
                    new GUIContent("UnityEngine.Mathf.PerlinNoise")
                });

                switch(_windowSettings.generatorData.noiseTypeIndex)
                {
                    case 0:
                        generatorData.seed = EditorGUILayout.IntField(new GUIContent("シード値", "シード値を設定します"), generatorData.seed);

                        generatorData.frequency = EditorGUILayout.FloatField(new GUIContent("周波数", "使用するノイズの周波数を設定します"), generatorData.frequency);
                        MessageType type = MessageType.Info;
                        if(generatorData.frequency > 256)
                        {
                            type = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoiseの周期は256なため\n256以上の数値にすると同様の地形が現れる可能性があります", type);

                        generatorData.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("線形スケーリング", "線形スケーリングを有効化します"), 
                            generatorData.isLinearScaling);

                        if (!generatorData.isLinearScaling)
                        {
                            generatorData.amplitude = EditorGUILayout.Slider(new GUIContent("振幅", "生成するHeightMapの振幅を設定します"),
                                generatorData.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
                        }
                        else
                        {
                            EditorGUILayout.MinMaxSlider(new GUIContent("スケール範囲", "生成するHeightMapのスケール範囲を設定します"),
                                ref generatorData.minLinearScale, ref generatorData.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                            GUI.enabled = false;
                            EditorGUILayout.FloatField(new GUIContent("最低値", "振幅の最低値を表示します"), generatorData.minLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("最高値", "振幅の最高値を表示します"), generatorData.maxLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("振幅", "振幅の値を表示します"), generatorData.maxLinearScale - generatorData.minLinearScale);
                            GUI.enabled = true;
                        }

                        if (generatorData.octaves > 0 && generatorData.maxLinearScale == ATGMathf.MaxTerrainHeight)
                        {
                            EditorGUILayout.HelpBox("オクターブを利用する場合、振幅を1未満に設定してください\n地形が正しく生成されません\n0.5が推奨されます", MessageType.Error);
                        }

                        generatorData.octaves = EditorGUILayout.IntField(new GUIContent("オクターブ", "非整数ブラウン運動を利用してオクターブの数値の回数ノイズを重ねます"), generatorData.octaves);
                        break;
                }
            }

            _windowSettings.isFoldoutHeightMap = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightMap, "HeightMap");
            if (_windowSettings.isFoldoutHeightMap)
            {
                generatorData.scale.x = EditorGUILayout.FloatField(new GUIContent("横幅", "HeightMapの横幅を設定します"), generatorData.scale.x);
                generatorData.scale.z = EditorGUILayout.FloatField(new GUIContent("奥行", "HeightMapの奥行を設定します"), generatorData.scale.z);
                generatorData.scale.y = EditorGUILayout.FloatField(new GUIContent("高さ", "HeightMapの高さを設定します"), generatorData.scale.y);

                int[] resolutionExpArray = new int[ATGMathf.ResolutionExpRange];
                for(int i = 0; i < ATGMathf.ResolutionExpRange; i++)
                {
                    resolutionExpArray[i] = i + ATGMathf.MinResolutionExp;
                }
                generatorData.resolutionExp = EditorGUILayout.IntPopup(new GUIContent("解像度", "HeightMapの解像度を設定します"), generatorData.resolutionExp, 
                    new[]
                {
                    new GUIContent("33×33"),
                    new GUIContent("65×65"),
                    new GUIContent("129×129"),
                    new GUIContent("257×257"),
                    new GUIContent("513×513"),
                    new GUIContent("1025×1025"),
                    new GUIContent("2049×2049"),
                    new GUIContent("4097×4097"),
                }, resolutionExpArray);
            }

            _windowSettings.isFoldoutAsset = EditorGUILayout.Foldout(_windowSettings.isFoldoutAsset, "Assets");
            if (_windowSettings.isFoldoutAsset)
            {
                _windowSettings.isCreateAsset = EditorGUILayout.Toggle(new GUIContent("アセット保存", "Terrain Dataをアセットとして保存するかどうかを指定します"), _windowSettings.isCreateAsset);

                if (_windowSettings.isCreateAsset)
                {
                    _windowSettings.assetName = EditorGUILayout.TextField(new GUIContent("ファイル名", "保存するTerrain Dataのファイル名を指定します"), (_windowSettings.assetName));

                    GUI.enabled = false;
                    EditorGUILayout.TextField(new GUIContent("保存先", "Terrain Dataを保存するパスを表示します"), _windowSettings.assetPath);
                    GUI.enabled = true;

                    if(GUILayout.Button(new GUIContent("保存先を指定する", "Terrain Dataの保存するフォルダを選択します")))
                    {
                        _windowSettings.assetPath = EditorUtility.OpenFolderPanel("保存先選択", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if(_windowSettings.assetPath == string.Empty)
                        {
                            _windowSettings.assetPath = Application.dataPath;
                        }

                        //相対パスを計算
                        Uri basisUri = new Uri(projectPath);
                        Uri absoluteUri = new Uri(_windowSettings.assetPath);
                        _windowSettings.assetPath = basisUri.MakeRelativeUri(absoluteUri).OriginalString;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Terrain Dataを保存しない場合、出力されたTerrainの再使用が困難になります\n保存することを推奨します", MessageType.Warning);
                }
            }

            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("テレインを生成する", "設定値からテレインを生成します")))
            {
                IHeightMapGenerator generator = new GeneratorByUnityPerlin();
                float[,] heightMap = generator.Generate(generatorData);

                TerrainData data = TerrainGenerator.Generate(heightMap, generatorData.scale);

                if (_windowSettings.isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _windowSettings.assetPath + "/" + _windowSettings.assetName + ".asset");
                }
            }
        }
    }
}
#endif
