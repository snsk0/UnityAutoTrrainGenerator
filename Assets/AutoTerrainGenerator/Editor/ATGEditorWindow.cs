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

            //アルゴリズム指定
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

            //settingProviderからアルゴリズムを取得
            UnityEngine.Object providerObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(IATGSettingProvider.SettingsPath);
            if(providerObject != null)
            {
                IATGSettingProvider settingProvider = (IATGSettingProvider)Instantiate(providerObject);

                //インスタンスを取得する
                _generators = settingProvider.GetGenerators();

                //クラス名をキーにgeneratorInstanceを取得、できたら情報を上書き
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
                    FieldInfo fieldInfo = attribute.GetType().GetField("m_InspectedType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    Type targetType = (Type)fieldInfo.GetValue(attribute);

                    if (generator.GetType() == targetType)
                    {
                        //Editorを生成する
                        Editor editor = Editor.CreateEditor(generator, editorType);

                        //紐づけて格納
                        _generatorToInspector.Add(generator, editor);

                        hasInspector = true;
                        break;
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

            _windowSettings.isFoldoutHeightMapGenerator = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightMapGenerator, "HeightMapGenerator");
            if(_windowSettings.isFoldoutHeightMapGenerator)
            {
                //アルゴリズムのGUIContentを作成する
                List<GUIContent> gUIContents = new List<GUIContent>();
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
