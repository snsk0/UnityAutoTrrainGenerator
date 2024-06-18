#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace AutoTerrainGenerator.Editor
{
    public class ATGEditorWindow : EditorWindow
    {
        private const int MinResolutionEx = 5;
         
        private SerializedObject _serializedObject;

        private bool _isFoldNoise = true;
        private bool _isFoldHeightMap = true;
        private bool _isFoldAsset = true;

        private int _noiseType;
        [SerializeField] private float _noiseScale;
        [SerializeField] private int _seed;
        [SerializeField] private int _octaves;
        [SerializeField] private float _persistance;
        [SerializeField] private bool _isCreateAsset;
        [SerializeField] private string _assetPath = "Assets";
        private Vector3 _scale;
        private int _selectedResolutionEx;
        [SerializeField] private float _step;


        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            _isFoldNoise = EditorGUILayout.Foldout(_isFoldNoise, "Noise");
            if(_isFoldNoise)
            {
                _noiseType = EditorGUILayout.Popup(new GUIContent("ノイズ"), _noiseType, new[]
                {
                    new GUIContent("UnityEngine.Mathf.PerlinNoise")
                });

                switch(_noiseType)
                {
                    case 0:
                        EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_noiseScale)),
                            new GUIContent("スケール値", "使用するノイズのスケール値を設定します"));

                        MessageType type = MessageType.Info;
                        if(_noiseScale > 256)
                        {
                            type = MessageType.Warning;
                        }
                        EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoiseの周期は256なため\n256以上の数値にすると同様の地形が現れる可能性があります。", type);

                        EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_seed)),
                            new GUIContent("シード値", "使用するノイズのシード値を設定します"));

                        EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_octaves)),
                            new GUIContent("オクターブ", "非整数ブラウン運動を利用してオクターブの数値の回数ノイズを重ねます\n0以下に設定すると無効になります"));

                        EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_persistance)),
                            new GUIContent("影響度", "重ねるノイズの影響度を指定します\n一般的に0.5が推奨されます\nまた、必ず0以上1未満の数値を設定してください"));
                        break;
                }
            }

            _isFoldHeightMap = EditorGUILayout.Foldout(_isFoldHeightMap, "HeightMap");
            if (_isFoldHeightMap)
            {
                _scale.x = EditorGUILayout.FloatField(new GUIContent("横幅", "HeightMapの横幅を設定します"), _scale.x);
                _scale.z = EditorGUILayout.FloatField(new GUIContent("奥行", "HeightMapの奥行を設定します"), _scale.z);
                _scale.y = EditorGUILayout.FloatField(new GUIContent("高さ", "HeightMapの高さを設定します"), _scale.y);

                _selectedResolutionEx = EditorGUILayout.Popup(new GUIContent("解像度"), _selectedResolutionEx, new[]
                {
                    new GUIContent("33×33"),
                    new GUIContent("65×65"),
                    new GUIContent("129×129"),
                    new GUIContent("257×257"),
                    new GUIContent("513×513"),
                    new GUIContent("1025×1025"),
                    new GUIContent("2049×2049"),
                    new GUIContent("4097×4097"),
                });

                EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_step)),
                            new GUIContent("離散値", "HeightMapを離散化します\n0以上の数値に設定することで機能します"));
            }

            _isFoldAsset = EditorGUILayout.Foldout(_isFoldAsset, "Assets");
            if (_isFoldAsset)
            {
                EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_isCreateAsset)),
                    new GUIContent("アセット保存", "Terrain Dataをアセットとして保存するかどうかを指定します"));

                if (_isCreateAsset)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_assetPath)),
                        new GUIContent("保存先", "Terrain Dataを保存するパスを表示します"));
                    GUI.enabled = true;

                    if(GUILayout.Button(new GUIContent("保存先を指定する", "Terrain Dataの保存するフォルダを選択します")))
                    {
                        _assetPath = EditorUtility.OpenFolderPanel("保存先選択", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if(_assetPath == string.Empty)
                        {
                            _assetPath = Application.dataPath;
                        }

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
                var data = TerrainGenerator.Generate(_scale, _selectedResolutionEx + MinResolutionEx, _noiseScale, _seed, _octaves, _persistance, _step);

                if (_isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _assetPath + "/Terrain.asset");
                }
            }
        }
    }
}
#endif
