#if UNITY_EDITOR
using System;
using AutoTerrainGenerator.Parameters;
using UnityEditor;
using UnityEngine;

namespace AutoTerrainGenerator.Editors
{
    internal static class SharedDefaultInspector
    {
        internal static void OnEnable(SerializedObject serializedObject, Type type)
        {
            serializedObject.Update();

            string paramJson = EditorUserSettings.GetConfigValue(type.Name);
            SerializedProperty paramProperty = serializedObject.FindProperty("_param");

            //Jsonがある場合
            bool isDeserialized = false;
            if (!string.IsNullOrEmpty(paramJson))
            {
                //デシリアライズを実行
                HeightMapGeneratorParam param = ScriptableObject.CreateInstance<HeightMapGeneratorParam>();
                JsonUtility.FromJsonOverwrite(paramJson, param);

                //成功した場合
                if (param != null)
                {
                    paramProperty.objectReferenceValue = param;
                    isDeserialized = true;
                }
            }

            //デシリアライズに失敗した場合生成する
            if (!isDeserialized) 
            {
                paramProperty.objectReferenceValue = ScriptableObject.CreateInstance<HeightMapGeneratorParam>();
            }

            //pathからアセットを読み込む
            string assetPath = EditorUserSettings.GetConfigValue(type.Name + ".input");
            if (!string.IsNullOrEmpty(assetPath))
            {
                serializedObject.FindProperty("_inputParam").objectReferenceValue = AssetDatabase.LoadAssetAtPath<HeightMapGeneratorBase>(assetPath);
            }
            serializedObject.ApplyModifiedProperties();
        }

        internal static void OnDisable(SerializedObject serializedObject, Type type) 
        {
            //シリアライズを実行
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;
            EditorUserSettings.SetConfigValue(type.Name, JsonUtility.ToJson(param));

            string assetPath = AssetDatabase.GetAssetPath(serializedObject.FindProperty("_inputParam").objectReferenceValue);
            EditorUserSettings.SetConfigValue(type.Name + ".input", assetPath);

            serializedObject.Update();
        }

        internal static void OnInspectorGUI(SerializedObject serializedObject)
        {
            serializedObject.Update();

            //パラメータオブジェクトを取得
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;

            //設定値の読み込み
            SerializedProperty inputProperty = serializedObject.FindProperty("_inputParam");
            EditorGUILayout.PropertyField(inputProperty, new GUIContent("入力", "HeightMapParamを入力します"));
            if(inputProperty.objectReferenceValue != null)
            {
                GUI.enabled = false;

                //設定値の上書き
                param = inputProperty.objectReferenceValue as HeightMapGeneratorParam;
            }

            param.seed = EditorGUILayout.IntField(new GUIContent("シード値", "シード値を設定します"), param.seed);

            param.frequency = EditorGUILayout.FloatField(new GUIContent("周波数", "使用するノイズの周波数を設定します"), param.frequency);
            MessageType type = MessageType.Info;
            if (param.frequency > 256)
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

                bool guiEnableTemp = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("最低値", "振幅の最低値を表示します"), param.minLinearScale);
                EditorGUILayout.FloatField(new GUIContent("最高値", "振幅の最高値を表示します"), param.maxLinearScale);
                EditorGUILayout.FloatField(new GUIContent("振幅", "振幅の値を表示します"), param.maxLinearScale - param.minLinearScale);
                GUI.enabled = guiEnableTemp;
            }

            if (param.octaves > 0 && param.maxLinearScale == ATGMathf.MaxTerrainHeight)
            {
                EditorGUILayout.HelpBox("オクターブを利用する場合、振幅を1未満に設定してください\n地形が正しく生成されません\n0.5が推奨されます", MessageType.Error);
            }

            param.octaves = EditorGUILayout.IntField(new GUIContent("オクターブ", "非整数ブラウン運動を利用してオクターブの数値の回数ノイズを重ねます"), param.octaves);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("設定値を出力する", "設定値をアセットファイルに保存します")))
            {
                string savePath = EditorUtility.SaveFilePanelInProject("Save", "parameters", "asset", "");
                if (!string.IsNullOrEmpty(savePath))
                {
                    //値をコピーする
                    HeightMapGeneratorParam outputParam = UnityEngine.Object.Instantiate(param);

                    //出力する
                    AssetDatabase.CreateAsset(outputParam, savePath);
                }
            }

            if (inputProperty.objectReferenceValue != null)
            {
                GUI.enabled = true;
            }
        }
    }
}
#endif
