#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;

namespace AutoTerrainGenerator.Editor
{
    internal class ATGEditorWindow : EditorWindow
    {         
        private SerializedObject _serializedObject;

        //GUI
        private bool _isFoldoutNoise = true;
        private bool _isFoldoutHeightMap = true;
        private bool _isFoldoutAsset = true;
        private bool _isAdvancedAmplitude = false;

        //ノイズ変数
        private int _noiseTypeIndex;
        private float _frequency;
        private float _maxAmplitude;
        private float _minAmplitude;
        private int _seed;
        private int _octaves;

        //ハイトマップ
        private int _resolutionExp = ATGMathf.MinResolutionExp;

        //テレイン
        private Vector3 _scale;

        //アセット
        private bool _isCreateAsset;
        private string _assetPath = "Assets";
        private string _assetName = "Terrain";

        //TODO 実装
        private float _step;


        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }

        private void OnDisable()
        {
            
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _isFoldoutNoise = EditorGUILayout.Foldout(_isFoldoutNoise, "Noise");
            if(_isFoldoutNoise)
            {
                _noiseTypeIndex = EditorGUILayout.Popup(new GUIContent("ノイズ"), _noiseTypeIndex, new[]
                {
                    new GUIContent("UnityEngine.Mathf.PerlinNoise")
                });

                switch(_noiseTypeIndex)
                {
                    case 0:
                        _seed = EditorGUILayout.IntField(new GUIContent("シード値", "シード値を設定します"), _seed);

                        _frequency = EditorGUILayout.FloatField(new GUIContent("周波数", "使用するノイズの周波数を設定します"), _frequency);
                        MessageType type = MessageType.Info;
                        if(_frequency > 256)
                        {
                            type = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoiseの周期は256なため\n256以上の数値にすると同様の地形が現れる可能性があります", type);

                        _isAdvancedAmplitude = EditorGUILayout.Toggle(new GUIContent("高度な振幅設定", "振幅について高度な設定を有効化するかどうかを設定します"), _isAdvancedAmplitude);

                        if (!_isAdvancedAmplitude)
                        {
                            _maxAmplitude = EditorGUILayout.Slider(new GUIContent("振幅", "生成するHeightMapの振幅を設定します"),
                                _maxAmplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
                        }
                        else
                        {
                            EditorGUILayout.MinMaxSlider(new GUIContent("振幅", "生成するHeightMapの振幅を設定します"),
                                ref _minAmplitude, ref _maxAmplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                            GUI.enabled = false;
                            EditorGUILayout.FloatField(new GUIContent("最低値", "振幅の最低値を表示します"), _minAmplitude);
                            EditorGUILayout.FloatField(new GUIContent("最高値", "振幅の最高値を表示します"), _maxAmplitude);
                            GUI.enabled = true;
                        }

                        if (_octaves > 0 && _maxAmplitude == ATGMathf.MaxTerrainHeight)
                        {
                            EditorGUILayout.HelpBox("オクターブを利用する場合、振幅を1未満に設定してください\n地形が正しく生成されません\n0.5が推奨されます", MessageType.Error);
                        }

                        _octaves = EditorGUILayout.IntField(new GUIContent("オクターブ", "非整数ブラウン運動を利用してオクターブの数値の回数ノイズを重ねます\n0以下に設定すると無効になります"), _octaves);
                        break;
                }
            }

            _isFoldoutHeightMap = EditorGUILayout.Foldout(_isFoldoutHeightMap, "HeightMap");
            if (_isFoldoutHeightMap)
            {
                _scale.x = EditorGUILayout.FloatField(new GUIContent("横幅", "HeightMapの横幅を設定します"), _scale.x);
                _scale.z = EditorGUILayout.FloatField(new GUIContent("奥行", "HeightMapの奥行を設定します"), _scale.z);
                _scale.y = EditorGUILayout.FloatField(new GUIContent("高さ", "HeightMapの高さを設定します"), _scale.y);

                int[] resolutionExpArray = new int[ATGMathf.ResolutionExpRange];
                for(int i = 0; i < ATGMathf.ResolutionExpRange; i++)
                {
                    resolutionExpArray[i] = i + ATGMathf.MinResolutionExp;
                }
                _resolutionExp = EditorGUILayout.IntPopup(new GUIContent("解像度", "HeightMapの解像度を設定します"), _resolutionExp, 
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

            _isFoldoutAsset = EditorGUILayout.Foldout(_isFoldoutAsset, "Assets");
            if (_isFoldoutAsset)
            {
                _isCreateAsset = EditorGUILayout.Toggle(new GUIContent("アセット保存", "Terrain Dataをアセットとして保存するかどうかを指定します"), _isCreateAsset);

                if (_isCreateAsset)
                {
                    EditorGUILayout.TextField(new GUIContent("ファイル名", "保存するTerrain Dataのファイル名を指定します"), (_assetName));

                    GUI.enabled = false;
                    EditorGUILayout.TextField(new GUIContent("保存先", "Terrain Dataを保存するパスを表示します"), _assetPath);
                    GUI.enabled = true;

                    if(GUILayout.Button(new GUIContent("保存先を指定する", "Terrain Dataの保存するフォルダを選択します")))
                    {
                        _assetPath = EditorUtility.OpenFolderPanel("保存先選択", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if(_assetPath == string.Empty)
                        {
                            _assetPath = Application.dataPath;
                        }

                        //相対パスを計算
                        Uri basisUri = new Uri(projectPath);
                        Uri absoluteUri = new Uri(_assetPath);
                        _assetPath = basisUri.MakeRelativeUri(absoluteUri).OriginalString;
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
                var map = new GeneratorByUnityPerlin().Generate(_seed, _resolutionExp, _frequency, _minAmplitude, _maxAmplitude, _octaves);
                var data = TerrainGenerator.Generate(map, _scale);

                if (_isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _assetPath + "/" + _assetName + ".asset");
                }
            }
        }
    }
}
#endif
