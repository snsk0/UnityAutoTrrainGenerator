using AutoTerrainGenerator.HeightMapGenerators;
using AutoTerrainGenerator.Parameters;
using UnityEngine;
using UnityEditor;

namespace AutoTerrainGenerator.Editors
{
    [ATGCustomEditor(typeof(GeneratorByUnityPerlin))]
    public class UnityPerlinInspector : GeneratorEditor
    {
        private HeightMapGeneratorParam _generatorData = new HeightMapGeneratorParam();

        public override void OnInspectorGUI()
        {
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
            }

            if (_generatorData.octaves > 0 && _generatorData.maxLinearScale == ATGMathf.MaxTerrainHeight)
            {
                EditorGUILayout.HelpBox("オクターブを利用する場合、振幅を1未満に設定してください\n地形が正しく生成されません\n0.5が推奨されます", MessageType.Error);
            }

            _generatorData.octaves = EditorGUILayout.IntField(new GUIContent("オクターブ", "非整数ブラウン運動を利用してオクターブの数値の回数ノイズを重ねます"), _generatorData.octaves);
        }
    }
}
