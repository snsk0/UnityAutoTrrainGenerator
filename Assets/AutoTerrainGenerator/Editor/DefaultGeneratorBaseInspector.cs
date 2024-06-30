#if UNITY_EDITOR
using AutoTerrainGenerator.HeightMapGenerators;
using AutoTerrainGenerator.Parameters;
using UnityEditor;
using UnityEngine;

namespace AutoTerrainGenerator.Editors
{
    [CustomEditor(typeof(DefaultGeneratorBase), true)]
    public class DefaultGeneratorBaseInspector : Editor
    {
        //OnEnable�ɂ���ƃA�Z�b�g�Ƃ���Enable�ɂȂ����ꍇ�ɃG���[���N����
        protected virtual void Awake()
        {
            serializedObject.Update();

            string paramJson = EditorUserSettings.GetConfigValue(target.GetType().Name);
            SerializedProperty paramProperty = serializedObject.FindProperty("_param");

            //Json������ꍇ
            bool isDeserialized = false;
            if (!string.IsNullOrEmpty(paramJson))
            {
                //�f�V���A���C�Y�����s
                HeightMapGeneratorParam param = CreateInstance<HeightMapGeneratorParam>();
                JsonUtility.FromJsonOverwrite(paramJson, param);

                //���������ꍇ
                if (param != null)
                {
                    paramProperty.objectReferenceValue = param;
                    isDeserialized = true;
                }
            }

            //�f�V���A���C�Y�Ɏ��s�����ꍇ��������
            if (!isDeserialized)
            {
                paramProperty.objectReferenceValue = CreateInstance<HeightMapGeneratorParam>();
            }

            //path����A�Z�b�g��ǂݍ���
            string assetPath = EditorUserSettings.GetConfigValue(target.GetType().Name + ".input");
            if (!string.IsNullOrEmpty(assetPath))
            {
                serializedObject.FindProperty("_inputParam").objectReferenceValue = AssetDatabase.LoadAssetAtPath<HeightMapGeneratorParam>(assetPath);
            }
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnDestroy()
        {
            //�V���A���C�Y�����s
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;
            EditorUserSettings.SetConfigValue(target.GetType().Name, JsonUtility.ToJson(param));

            string assetPath = AssetDatabase.GetAssetPath(serializedObject.FindProperty("_inputParam").objectReferenceValue);
            EditorUserSettings.SetConfigValue(target.GetType().Name + ".input", assetPath);

            serializedObject.Update();
        }

        public override void OnInspectorGUI ()
        {
            if(target == null)
            {
                Debug.Log("targetNULL");
            }

            serializedObject.Update();

            //�p�����[�^�I�u�W�F�N�g���擾
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;

            //�ݒ�l�̓ǂݍ���
            SerializedProperty inputProperty = serializedObject.FindProperty("_inputParam");
            EditorGUILayout.PropertyField(inputProperty, new GUIContent("����", "HeightMapParam����͂��܂�"));
            if (inputProperty.objectReferenceValue != null)
            {
                GUI.enabled = false;

                //�ݒ�l�̏㏑��
                param = inputProperty.objectReferenceValue as HeightMapGeneratorParam;
            }

            param.seed = EditorGUILayout.IntField(new GUIContent("�V�[�h�l", "�V�[�h�l��ݒ肵�܂�"), param.seed);

            param.frequency = EditorGUILayout.FloatField(new GUIContent("���g��", "�g�p����m�C�Y�̎��g����ݒ肵�܂�"), param.frequency);
            MessageType type = MessageType.Info;
            if (param.frequency > 256)
            {
                type = MessageType.Warning;
            }
            EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoise�̎�����256�Ȃ���\n256�ȏ�̐��l�ɂ���Ɠ��l�̒n�`�������\��������܂�", type);

            param.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("���`�X�P�[�����O", "���`�X�P�[�����O��L�������܂�"), param.isLinearScaling);

            if (!param.isLinearScaling)
            {
                param.amplitude = EditorGUILayout.Slider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                    param.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
            }
            else
            {
                EditorGUILayout.MinMaxSlider(new GUIContent("�X�P�[���͈�", "��������HeightMap�̃X�P�[���͈͂�ݒ肵�܂�"),
                    ref param.minLinearScale, ref param.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                bool guiEnableTemp = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), param.minLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), param.maxLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�U��", "�U���̒l��\�����܂�"), param.maxLinearScale - param.minLinearScale);
                GUI.enabled = guiEnableTemp;
            }

            if (param.octaves > 0 && param.maxLinearScale == ATGMathf.MaxTerrainHeight)
            {
                EditorGUILayout.HelpBox("�I�N�^�[�u�𗘗p����ꍇ�A�U����1�����ɐݒ肵�Ă�������\n�n�`����������������܂���\n0.5����������܂�", MessageType.Error);
            }

            param.octaves = EditorGUILayout.IntField(new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�"), param.octaves);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("�ݒ�l���o�͂���", "�ݒ�l���A�Z�b�g�t�@�C���ɕۑ����܂�")))
            {
                string savePath = EditorUtility.SaveFilePanelInProject("Save", "parameters", "asset", "");
                if (!string.IsNullOrEmpty(savePath))
                {
                    //�l���R�s�[����
                    HeightMapGeneratorParam outputParam = Instantiate(param);

                    //�o�͂���
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
