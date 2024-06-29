#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;
using AutoTerrainGenerator.Parameters;
using AutoTerrainGenerator.Attributes;

namespace AutoTerrainGenerator.Editors
{
    [ATGCustomEditor(typeof(GeneratorByUnityPerlin))]
    public class UnityPerlinInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //パラメータオブジェクトを取得
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;

            param.seed = EditorGUILayout.IntField(new GUIContent("シード値", "シード値を設定します"), param.seed);

            param.frequency = EditorGUILayout.FloatField(new GUIContent("周波数", "使用するノイズの周波数を設定します"), param.frequency);
            MessageType type = MessageType.Info;
            if(param.frequency > 256)
            {
                type = MessageType.Warning;
            }
            EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoiseの周期は256なため\n256以上の数値にすると同様の地形が現れる可能性があります", type);

            param.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("線形スケーリング", "線形スケーリングを有効化します"), param.isLinearScaling);

            if (!param.isLinearScaling)
            {
                param.amplitude = EditorGUILayout.Slider(new GUIContent("振幅", "生成するHeightMapの振幅を設定します"),
                    param.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
            }
            else
            {
                EditorGUILayout.MinMaxSlider(new GUIContent("スケール範囲", "生成するHeightMapのスケール範囲を設定します"),
                    ref param.minLinearScale, ref param.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("最低値", "振幅の最低値を表示します"), param.minLinearScale);
                EditorGUILayout.FloatField(new GUIContent("最高値", "振幅の最高値を表示します"), param.maxLinearScale);
                EditorGUILayout.FloatField(new GUIContent("振幅", "振幅の値を表示します"), param.maxLinearScale - param.minLinearScale);
                GUI.enabled = true;
            }

            if (param.octaves > 0 && param.maxLinearScale == ATGMathf.MaxTerrainHeight)
            {
                EditorGUILayout.HelpBox("オクターブを利用する場合、振幅を1未満に設定してください\n地形が正しく生成されません\n0.5が推奨されます", MessageType.Error);
            }

            param.octaves = EditorGUILayout.IntField(new GUIContent("オクターブ", "非整数ブラウン運動を利用してオクターブの数値の回数ノイズを重ねます"), param.octaves);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
