#if UNITY_EDITOR 
using UnityEngine;
using UnityEditor;

namespace AutoTerrainGenerator.Editor
{
    public class ATGEditorWindow : EditorWindow
    {
        private SerializedObject _serializedObject;
        private bool _isFoldHeightMap;
        private bool _isFoldAsset;

        [SerializeField] private bool _isCreateAsset;
        [SerializeField] private string _assetPath;
        [SerializeField] private Vector3 _scale;
        [SerializeField] private int _resolution;


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

            _isFoldHeightMap = EditorGUILayout.Foldout(_isFoldHeightMap, "HeightMap");
            if (_isFoldHeightMap)
            {
                _scale.x = EditorGUILayout.FloatField(new GUIContent("横幅", "HeightMapの横幅を設定します"), _scale.x);
                _scale.z = EditorGUILayout.FloatField(new GUIContent("奥行", "HeightMapの奥行を設定します"), _scale.z);
                _scale.y = EditorGUILayout.FloatField(new GUIContent("高さ", "HeightMapの高さを設定します"), _scale.y);

                EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_resolution)),
                    new GUIContent("解像度", "HeightMapの解像度を設定します"));

                if (!(IsOfForm2NPlus1(_resolution)))
                {
                    EditorGUILayout.HelpBox("解像度は2の累乗+1を満たすように設定してください\n正しく地形が生成されません", MessageType.Warning);
                }
            }

            _isFoldAsset = EditorGUILayout.Foldout(_isFoldAsset, "Assets");
            if (_isFoldAsset)
            {
                EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_isCreateAsset)),
                    new GUIContent("アセット保存", "Terrain Dataをアセットとして保存するかどうかを指定します"));

                if (_isCreateAsset)
                {
                    EditorGUILayout.PropertyField(_serializedObject.FindProperty(nameof(_assetPath)),
                    new GUIContent("パス", "Terrain Dataを保存するパスを指定します"));
                }
                else
                {
                    EditorGUILayout.HelpBox("Terrain Dataを保存しない場合、出力されたTerrainの再使用が困難になります\n保存することを推奨します", MessageType.Warning);
                }
            }

            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Generate Terrain"))
            {
                var data = TerrainGenerator.Generate(_scale, _resolution);

                if (_isCreateAsset)
                {
                    AssetDatabase.CreateAsset(data, _assetPath + "/Terrain.asset");
                }
            }
        }

        /*
         * 仮コード
         */
        private bool IsOfForm2NPlus1(int x)
        {
            if (x <= 1)
            {
                return false;
            }
            int y = x - 1;
            return IsPowerOfTwo(y);
        }

        private bool IsPowerOfTwo(int n)
        {
            return n > 0 && (n & (n - 1)) == 0;
        }
    }
}
#endif
