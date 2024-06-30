#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.NoiseReaders;

namespace AutoTerrainGenerator.Editors
{
    internal class ATGEditorWindow : EditorWindow
    {
        [Serializable]
        private struct ATGWindowSettigs
        {
            //GUI
            public bool isFoldoutHeightMapGenerator;
            public bool isFoldoutTerrain;
            public bool isFoldoutAsset;

            //インデックス
            public int noiseReaderIndex;
            public int generatorIndex;

            //テレインパラメータ
            public TerrainParameters parameters;

            //アセット
            public bool isCreateAsset;
            public string assetPath;
            public string assetName;
        }

        //window情報
        private SerializedObject _serializedObject;
        private ATGWindowSettigs _windowSettings;
        private List<INoiseReader> _noiseReaders;
        private List<HeightMapGeneratorBase> _generators;
        private Dictionary<HeightMapGeneratorBase, Editor> _generatorToInspector;

        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        //デシリアライズして設定取得
        private void OnEnable()
        {
            //json取得
            string windowJson = EditorUserSettings.GetConfigValue(nameof(_windowSettings));

            //デシリアライズ
            if(!string.IsNullOrEmpty(windowJson)) 
            {
                _windowSettings = JsonUtility.FromJson<ATGWindowSettigs>(windowJson);
            }
            //初期化処理
            else
            {
                _windowSettings = new ATGWindowSettigs();
                _windowSettings.isFoldoutHeightMapGenerator = true;
                _windowSettings.isFoldoutTerrain = true;
                _windowSettings.isFoldoutAsset = true;
                _windowSettings.isCreateAsset = true;
                _windowSettings.assetPath = "Assets";
                _windowSettings.assetName = "Terrain";
                _windowSettings.parameters = new TerrainParameters();
            }

            _serializedObject = new SerializedObject(this);

            //settingProviderを取得
            UnityEngine.Object providerObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(IATGSettingProvider.SettingsPath);
            if(providerObject != null)
            {
                IATGSettingProvider settingProvider = (IATGSettingProvider)Instantiate(providerObject);

                //NoiseReadersを取得する
                _noiseReaders = settingProvider.GetNoiseReaders();

                //クラス名をキーにgeneratorInstanceを取得、できたら情報を上書き
                _generators = settingProvider.GetGenerators();
                foreach(HeightMapGeneratorBase generator in _generators)
                {
                    string generatorJson = EditorUserSettings.GetConfigValue(generator.GetType().Name);
                    if(!string.IsNullOrEmpty(generatorJson))
                    {
                        JsonUtility.FromJsonOverwrite(generatorJson, generator);
                    }
                }
            }

            //辞書を初期化
            _generatorToInspector = new Dictionary<HeightMapGeneratorBase, Editor>();

            //Attributeが付いたクラスを一括取得
            TypeCache.TypeCollection editorTypes = TypeCache.GetTypesWithAttribute<CustomEditor>();

            //generatorから一致するものがあるか検索
            bool hasInspector;
            foreach(HeightMapGeneratorBase generator in _generators)
            {
                if(generator == null)
                {
                    Debug.LogWarning("BreakObject");
                    break;
                }
                hasInspector = false;

                foreach(Type editorType in editorTypes)
                {
                    //リフレクションを使用してAttributeからターゲットのタイプを取得
                    CustomEditor attribute = editorType.GetCustomAttribute<CustomEditor>();
                    FieldInfo InspectedTypeField = attribute.GetType().GetField("m_InspectedType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    Type targetType = (Type)InspectedTypeField.GetValue(attribute);

                    //リフレクションを使用して子を対象にするかを取得
                    FieldInfo forChildsField = attribute.GetType().GetField("m_EditorForChildClasses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    bool forChilds = (bool)forChildsField.GetValue(attribute);

                    //子を対象にしない場合
                    if (forChilds)
                    {
                        if (generator.GetType() == targetType)
                        {
                            //Editorを生成する
                            Editor editor = Editor.CreateEditor(generator, editorType);

                            //すでにInspectorが生成されている場合上書きする
                            if(_generatorToInspector.ContainsKey(generator))
                            {
                                _generatorToInspector[generator] = editor;
                            }
                            else
                            {
                                _generatorToInspector.Add(generator, editor);
                            }

                            hasInspector = true;
                            break;
                        }
                    }
                    //子を対象とする場合
                    else
                    {
                        if (generator.GetType().IsSubclassOf(targetType))
                        {
                            //すでに紐づけられている場合成功として処理
                            if (_generatorToInspector.ContainsKey(generator))
                            {
                                hasInspector = true;
                                break;
                            }

                            //Editorを生成する
                            Editor editor = Editor.CreateEditor(generator, editorType);

                            //紐づけて格納
                            _generatorToInspector.Add(generator, editor);

                            hasInspector = true;
                            break;
                        }
                    }
                }

                //インスペクタ拡張が見つからなかった場合、通常のEditorを格納
                if (!hasInspector)
                {
                    Editor editor = Editor.CreateEditor(generator);
                    _generatorToInspector.Add(generator, editor);
                }
            }
        }

        //シリアライズして保存する
        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue(nameof(_windowSettings), JsonUtility.ToJson(_windowSettings));

            //読み込んでいるgeneratorをserializeして格納
            foreach(HeightMapGeneratorBase generator in _generators)
            {
                EditorUserSettings.SetConfigValue(generator.GetType().Name, JsonUtility.ToJson(generator));
            }

            //Editorを先に全て破棄する
            foreach(Editor editor in _generatorToInspector.Values)
            {
                DestroyImmediate(editor);
            }

            //generatorをEditorの後に全て破棄する
            foreach (HeightMapGeneratorBase generator in _generators)
            {
                DestroyImmediate(generator);
            }
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            //ノイズGUIContentを作成する
            List<GUIContent> gUIContents = new List<GUIContent>();
            foreach (INoiseReader noiseReader in _noiseReaders)
            {
                gUIContents.Add(new GUIContent(noiseReader.GetType().ToString()));
            }

            //ノイズ一覧を表示
            _windowSettings.noiseReaderIndex = EditorGUILayout.IntPopup(
                new GUIContent("ノイズ"),
                _windowSettings.noiseReaderIndex,
                gUIContents.ToArray(),
                Enumerable.Range(0, gUIContents.Count).ToArray());

            //HeightMap関連項目
            _windowSettings.isFoldoutHeightMapGenerator = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightMapGenerator, "HeightMapGenerator");
            if(_windowSettings.isFoldoutHeightMapGenerator)
            {
                //アルゴリズムのGUIContentを作成する
                gUIContents.Clear();
                foreach (HeightMapGeneratorBase generator in _generators)
                {
                    gUIContents.Add(new GUIContent(generator.GetType().ToString()));
                }

                //アルゴリズムの一覧表示
                _windowSettings.generatorIndex = EditorGUILayout.IntPopup(
                    new GUIContent("アルゴリズム"), 
                    _windowSettings.generatorIndex, 
                    gUIContents.ToArray(), 
                    Enumerable.Range(0, gUIContents.Count).ToArray());

                //選択したindexからEditorを呼び出す
                _generatorToInspector[_generators[_windowSettings.generatorIndex]].OnInspectorGUI();
            }

            //Terrain関連項目
            _windowSettings.isFoldoutTerrain = EditorGUILayout.Foldout(_windowSettings.isFoldoutTerrain, "Terrain");
            if (_windowSettings.isFoldoutTerrain)
            {
                TerrainParameters parameters = _windowSettings.parameters;
                parameters.scale.x = EditorGUILayout.FloatField(new GUIContent("横幅", "HeightMapの横幅を設定します"), parameters.scale.x);
                parameters.scale.z = EditorGUILayout.FloatField(new GUIContent("奥行", "HeightMapの奥行を設定します"), parameters.scale.z);
                parameters.scale.y = EditorGUILayout.FloatField(new GUIContent("高さ", "HeightMapの高さを設定します"), parameters.scale.y);

                parameters.resolutionExp = EditorGUILayout.IntPopup(new GUIContent("解像度", "HeightMapの解像度を設定します"), parameters.resolutionExp, 
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
                }, Enumerable.Range(ATGMathf.MinResolutionExp, ATGMathf.MaxResolutionExp).ToArray());
            }

            //Asset関連項目
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

            if (GUILayout.Button(new GUIContent("テレインを生成する", "設定値からテレインを生成します")))
            {
                //Dataをコピーして渡す
                HeightMapGeneratorBase generator = _generators[_windowSettings.generatorIndex];
                float[,] heightMap = generator.Generate(new UnityPerlinNoise(), _windowSettings.parameters.resolution);

                TerrainData data = TerrainGenerator.Generate(heightMap, _windowSettings.parameters.scale);

                if (_windowSettings.isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _windowSettings.assetPath + "/" + _windowSettings.assetName + ".asset");
                }
            }
        }
    }
}
#endif
