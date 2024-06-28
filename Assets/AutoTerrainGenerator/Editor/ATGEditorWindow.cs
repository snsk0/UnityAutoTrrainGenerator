#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;
using AutoTerrainGenerator.Parameters;

namespace AutoTerrainGenerator.Editors
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

            //アセット
            public bool isCreateAsset;
            public string assetPath;
            public string assetName;
        }

        //入力
        [SerializeField]
        private HeightMapGeneratorParam _inputGeneratorData;

        //window情報
        private SerializedObject _serializedObject;
        private ATGWindowSettigs _windowSettings;
        private HeightMapGeneratorParam _generatorData;
        private bool _canInputData => _inputGeneratorData == null;

        //TODO 実装
        private float _step;

        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        //デシリアライズして設定取得
        private void OnEnable()
        {
            string windowJson = EditorUserSettings.GetConfigValue(nameof(_windowSettings));
            string generaterJson = EditorUserSettings.GetConfigValue(nameof(_generatorData));

            if(!string.IsNullOrEmpty(windowJson) && ! string.IsNullOrEmpty(generaterJson)) 
            {
                _windowSettings = JsonUtility.FromJson<ATGWindowSettigs>(windowJson);

                _generatorData = CreateInstance<HeightMapGeneratorParam>();
                JsonUtility.FromJsonOverwrite(generaterJson, _generatorData);

                string dataPath = EditorUserSettings.GetConfigValue(nameof(_inputGeneratorData));
                if (!string.IsNullOrEmpty(dataPath))
                {
                    _inputGeneratorData = AssetDatabase.LoadAssetAtPath<HeightMapGeneratorParam>(dataPath);
                }
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
                
                _generatorData = CreateInstance<HeightMapGeneratorParam>();
            }

            _serializedObject = new SerializedObject(this);
        }

        //シリアライズして保存する
        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue(nameof(_windowSettings), JsonUtility.ToJson(_windowSettings));
            EditorUserSettings.SetConfigValue(nameof(_generatorData), JsonUtility.ToJson(_generatorData));

            if(_inputGeneratorData != null)
            {
                EditorUserSettings.SetConfigValue(nameof(_inputGeneratorData), AssetDatabase.GetAssetPath(_inputGeneratorData));
            }
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            //設定値の読み込み
            EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_inputGeneratorData)), new GUIContent("設定入力"));

            if (!_canInputData)
            {
                GUI.enabled = false;
            }

            _windowSettings.isFoldoutNoise = EditorGUILayout.Foldout(_windowSettings.isFoldoutNoise, "Noise");
            if(_windowSettings.isFoldoutNoise)
            {
                _generatorData.noiseTypeIndex = EditorGUILayout.Popup(new GUIContent("ノイズ"), _generatorData.noiseTypeIndex, new[]
                {
                    new GUIContent("UnityEngine.Mathf.PerlinNoise")
                });

                switch(_generatorData.noiseTypeIndex)
                {
                    case 0:
                        _generatorData.generateType = (GenerateType)EditorGUILayout.EnumPopup(new GUIContent("アルゴリズム"), _generatorData.generateType);

                        _generatorData.seed = EditorGUILayout.IntField(new GUIContent("シード値", "シード値を設定します"), _generatorData.seed);

                        _generatorData.frequency = EditorGUILayout.FloatField(new GUIContent("周波数", "使用するノイズの周波数を設定します"), _generatorData.frequency);
                        MessageType type = MessageType.Info;
                        if(_generatorData.frequency > 256)
                        {
                            type = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoiseの周期は256なため\n256以上の数値にすると同様の地形が現れる可能性があります", type);

                        _generatorData.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("線形スケーリング", "線形スケーリングを有効化します"), 
                            _generatorData.isLinearScaling);

                        if (!_generatorData.isLinearScaling)
                        {
                            _generatorData.amplitude = EditorGUILayout.Slider(new GUIContent("振幅", "生成するHeightMapの振幅を設定します"),
                                _generatorData.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
                        }
                        else
                        {
                            EditorGUILayout.MinMaxSlider(new GUIContent("スケール範囲", "生成するHeightMapのスケール範囲を設定します"),
                                ref _generatorData.minLinearScale, ref _generatorData.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                            GUI.enabled = false;
                            EditorGUILayout.FloatField(new GUIContent("最低値", "振幅の最低値を表示します"), _generatorData.minLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("最高値", "振幅の最高値を表示します"), _generatorData.maxLinearScale);
                            EditorGUILayout.FloatField(new GUIContent("振幅", "振幅の値を表示します"), _generatorData.maxLinearScale - _generatorData.minLinearScale);
                            if (_canInputData)
                            {
                                GUI.enabled = true;
                            }
                        }

                        if (_generatorData.octaves > 0 && _generatorData.maxLinearScale == ATGMathf.MaxTerrainHeight)
                        {
                            EditorGUILayout.HelpBox("オクターブを利用する場合、振幅を1未満に設定してください\n地形が正しく生成されません\n0.5が推奨されます", MessageType.Error);
                        }

                        _generatorData.octaves = EditorGUILayout.IntField(new GUIContent("オクターブ", "非整数ブラウン運動を利用してオクターブの数値の回数ノイズを重ねます"), _generatorData.octaves);
                        break;
                }
            }

            _windowSettings.isFoldoutHeightMap = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightMap, "HeightMap");
            if (_windowSettings.isFoldoutHeightMap)
            {
                _generatorData.scale.x = EditorGUILayout.FloatField(new GUIContent("横幅", "HeightMapの横幅を設定します"), _generatorData.scale.x);
                _generatorData.scale.z = EditorGUILayout.FloatField(new GUIContent("奥行", "HeightMapの奥行を設定します"), _generatorData.scale.z);
                _generatorData.scale.y = EditorGUILayout.FloatField(new GUIContent("高さ", "HeightMapの高さを設定します"), _generatorData.scale.y);

                int[] resolutionExpArray = new int[ATGMathf.ResolutionExpRange];
                for(int i = 0; i < ATGMathf.ResolutionExpRange; i++)
                {
                    resolutionExpArray[i] = i + ATGMathf.MinResolutionExp;
                }
                _generatorData.resolutionExp = EditorGUILayout.IntPopup(new GUIContent("解像度", "HeightMapの解像度を設定します"), _generatorData.resolutionExp, 
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

            //更新
            _serializedObject.ApplyModifiedProperties();

            //Generatorの入力があった場合反映する
            if (_inputGeneratorData != null)
            {
                _generatorData = Instantiate(_inputGeneratorData);
            }
            GUI.enabled = true;

            if (GUILayout.Button(new GUIContent("テレインを生成する", "設定値からテレインを生成します")))
            {
                //Dataをコピーして渡す
                IHeightMapGenerator generator = new GeneratorByUnityPerlin();
                float[,] heightMap = generator.Generate(Instantiate(_generatorData));

                TerrainData data = TerrainGenerator.Generate(heightMap, _generatorData.scale);

                if (_windowSettings.isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _windowSettings.assetPath + "/" + _windowSettings.assetName + ".asset");
                }
            }

            if (GUILayout.Button(new GUIContent("設定値を出力する", "設定値をアセットファイルに保存します")))
            {
                string savePath = EditorUtility.SaveFilePanelInProject("Save", "settings", "asset", "");
                if(!string.IsNullOrEmpty(savePath)) 
                {
                    //値をコピーする
                    HeightMapGeneratorParam outputGeneratorData = Instantiate(_generatorData);

                    //出力する
                    AssetDatabase.CreateAsset(outputGeneratorData, savePath);
                }
            }
        }
    }
}
#endif
