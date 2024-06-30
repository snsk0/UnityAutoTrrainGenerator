#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using AutoTerrainGenerator.NoiseReaders;

namespace AutoTerrainGenerator.Editors
{
    internal class ATGEditorWindow : EditorWindow
    {

        //�ÓI���
        private static List<HeightMapGeneratorBase> _generators;
        private static Dictionary<HeightMapGeneratorBase, Editor> _generatorToInspector;

        //����������
        [InitializeOnLoadMethod]
        private static void InitializedOnLoad()
        {
            //�R���p�C���O�ɔj���ƃZ�[�u
            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                ReleaseData();
            };

            //�R���p�C����Ƀf�[�^���擾
            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                LoadData();
            };

            //�f�[�^�̕ۑ���Editor�j��
            EditorSceneManager.sceneOpening += (s, t) =>
            {
                ReleaseData();
            };

            //�f�[�^�̃��[�h�Ɛ���
            EditorSceneManager.sceneOpened += (s, t) =>
            {
                LoadData();
            };
        }

        //Secene�ύX��
        private static void LoadData()
        {
            //settingProvider���擾
            UnityEngine.Object providerObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(IATGSettingProvider.SettingsPath);
            if (providerObject != null)
            {
                IATGSettingProvider settingProvider = (IATGSettingProvider)Instantiate(providerObject);

                //�N���X�����L�[��generatorInstance���擾�A�ł���������㏑��
                _generators = settingProvider.GetGenerators();
                foreach (HeightMapGeneratorBase generator in _generators)
                {
                    string generatorJson = EditorUserSettings.GetConfigValue(generator.GetType().Name);
                    if (!string.IsNullOrEmpty(generatorJson))
                    {
                        JsonUtility.FromJsonOverwrite(generatorJson, generator);
                    }
                }
            }

            //������������
            _generatorToInspector = new Dictionary<HeightMapGeneratorBase, Editor>();

            //Attribute���t�����N���X���ꊇ�擾
            TypeCache.TypeCollection editorTypes = TypeCache.GetTypesWithAttribute<CustomEditor>();

            //generator�����v������̂����邩����
            bool hasInspector;
            foreach (HeightMapGeneratorBase generator in _generators)
            {
                hasInspector = false;

                foreach (Type editorType in editorTypes)
                {
                    //���t���N�V�������g�p����Attribute����^�[�Q�b�g�̃^�C�v���擾
                    CustomEditor attribute = editorType.GetCustomAttribute<CustomEditor>();
                    FieldInfo InspectedTypeField = attribute.GetType().GetField("m_InspectedType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    Type targetType = (Type)InspectedTypeField.GetValue(attribute);

                    //���t���N�V�������g�p���Ďq��Ώۂɂ��邩���擾
                    FieldInfo forChildsField = attribute.GetType().GetField("m_EditorForChildClasses", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    bool forChilds = (bool)forChildsField.GetValue(attribute);

                    //�q��Ώۂɂ��Ȃ��ꍇ
                    if (forChilds)
                    {
                        if (generator.GetType() == targetType)
                        {
                            //Editor�𐶐�����
                            Editor editor = Editor.CreateEditor(generator, editorType);

                            //���ł�Inspector����������Ă���ꍇ�㏑������
                            if (_generatorToInspector.ContainsKey(generator))
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
                    //�q��ΏۂƂ���ꍇ
                    else
                    {
                        if (generator.GetType().IsSubclassOf(targetType))
                        {
                            //���łɕR�Â����Ă���ꍇ�����Ƃ��ď���
                            if (_generatorToInspector.ContainsKey(generator))
                            {
                                hasInspector = true;
                                break;
                            }

                            //Editor�𐶐�����
                            Editor editor = Editor.CreateEditor(generator, editorType);

                            //�R�Â��Ċi�[
                            _generatorToInspector.Add(generator, editor);

                            hasInspector = true;
                            break;
                        }
                    }
                }

                //�C���X�y�N�^�g����������Ȃ������ꍇ�A�ʏ��Editor���i�[
                if (!hasInspector)
                {
                    Editor editor = Editor.CreateEditor(generator);
                    _generatorToInspector.Add(generator, editor);
                }
            }
        }

        //�f�[�^�j���ƃZ�[�u
        private static void ReleaseData()
        {
            //�ǂݍ���ł���generator��serialize���Ċi�[
            foreach (HeightMapGeneratorBase generator in _generators)
            {
                EditorUserSettings.SetConfigValue(generator.GetType().Name, JsonUtility.ToJson(generator));
            }

            //Editor���ɑS�Ĕj������
            foreach (Editor editor in _generatorToInspector.Values)
            {
                DestroyImmediate(editor);
            }

            //generator��Editor�̌�ɑS�Ĕj������
            foreach (HeightMapGeneratorBase generator in _generators)
            {
                DestroyImmediate(generator);
            }

        }

        //Window�J���Ƃ��̏���������
        [MenuItem("Window/AutoTerrainGenerator")]
        private static void Init()
        {
            GetWindow<ATGEditorWindow>("AutoTerrainGenerator");
        }

        [Serializable]
        private struct ATGWindowSettigs
        {
            //GUI
            public bool isFoldoutHeightMapGenerator;
            public bool isFoldoutTerrain;
            public bool isFoldoutAsset;

            //�C���f�b�N�X
            public int noiseReaderIndex;
            public int generatorIndex;

            //�e���C���p�����[�^
            public TerrainParameters parameters;

            //�A�Z�b�g
            public bool isCreateAsset;
            public string assetPath;
            public string assetName;
        }

        //window���
        private SerializedObject _serializedObject;
        private ATGWindowSettigs _windowSettings;
        private List<INoiseReader> _noiseReaders;

        //�f�V���A���C�Y���Đݒ�擾
        private void OnEnable()
        {
            //json�擾
            string windowJson = EditorUserSettings.GetConfigValue(nameof(_windowSettings));

            //�f�V���A���C�Y
            if(!string.IsNullOrEmpty(windowJson)) 
            {
                _windowSettings = JsonUtility.FromJson<ATGWindowSettigs>(windowJson);
            }
            //����������
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

            //settingProvider���擾
            UnityEngine.Object providerObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(IATGSettingProvider.SettingsPath);
            if(providerObject != null)
            {
                IATGSettingProvider settingProvider = (IATGSettingProvider)Instantiate(providerObject);

                //NoiseReaders���擾����
                _noiseReaders = settingProvider.GetNoiseReaders();
            }
        }

        //�V���A���C�Y���ĕۑ�����
        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue(nameof(_windowSettings), JsonUtility.ToJson(_windowSettings));
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            //�m�C�YGUIContent���쐬����
            List<GUIContent> gUIContents = new List<GUIContent>();
            foreach (INoiseReader noiseReader in _noiseReaders)
            {
                gUIContents.Add(new GUIContent(noiseReader.GetType().ToString()));
            }

            //�m�C�Y�ꗗ��\��
            _windowSettings.noiseReaderIndex = EditorGUILayout.IntPopup(
                new GUIContent("�m�C�Y"),
                _windowSettings.noiseReaderIndex,
                gUIContents.ToArray(),
                Enumerable.Range(0, gUIContents.Count).ToArray());

            //HeightMap�֘A����
            _windowSettings.isFoldoutHeightMapGenerator = EditorGUILayout.Foldout(_windowSettings.isFoldoutHeightMapGenerator, "HeightMapGenerator");
            if(_windowSettings.isFoldoutHeightMapGenerator)
            {
                //�A���S���Y����GUIContent���쐬����
                gUIContents.Clear();
                foreach (HeightMapGeneratorBase generator in _generators)
                {
                    gUIContents.Add(new GUIContent(generator.GetType().ToString()));
                }

                //�A���S���Y���̈ꗗ�\��
                _windowSettings.generatorIndex = EditorGUILayout.IntPopup(
                    new GUIContent("�A���S���Y��"), 
                    _windowSettings.generatorIndex, 
                    gUIContents.ToArray(), 
                    Enumerable.Range(0, gUIContents.Count).ToArray());

                //�I������index����Editor���Ăяo��
                _generatorToInspector[_generators[_windowSettings.generatorIndex]].OnInspectorGUI();
            }

            //Terrain�֘A����
            _windowSettings.isFoldoutTerrain = EditorGUILayout.Foldout(_windowSettings.isFoldoutTerrain, "Terrain");
            if (_windowSettings.isFoldoutTerrain)
            {
                TerrainParameters parameters = _windowSettings.parameters;
                parameters.scale.x = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̉�����ݒ肵�܂�"), parameters.scale.x);
                parameters.scale.z = EditorGUILayout.FloatField(new GUIContent("���s", "HeightMap�̉��s��ݒ肵�܂�"), parameters.scale.z);
                parameters.scale.y = EditorGUILayout.FloatField(new GUIContent("����", "HeightMap�̍�����ݒ肵�܂�"), parameters.scale.y);

                parameters.resolutionExp = EditorGUILayout.IntPopup(new GUIContent("�𑜓x", "HeightMap�̉𑜓x��ݒ肵�܂�"), parameters.resolutionExp, 
                    new[]
                {
                    new GUIContent("33�~33"),
                    new GUIContent("65�~65"),
                    new GUIContent("129�~129"),
                    new GUIContent("257�~257"),
                    new GUIContent("513�~513"),
                    new GUIContent("1025�~1025"),
                    new GUIContent("2049�~2049"),
                    new GUIContent("4097�~4097"),
                }, Enumerable.Range(ATGMathf.MinResolutionExp, ATGMathf.MaxResolutionExp).ToArray());
            }

            //Asset�֘A����
            _windowSettings.isFoldoutAsset = EditorGUILayout.Foldout(_windowSettings.isFoldoutAsset, "Assets");
            if (_windowSettings.isFoldoutAsset)
            {
                _windowSettings.isCreateAsset = EditorGUILayout.Toggle(new GUIContent("�A�Z�b�g�ۑ�", "Terrain Data���A�Z�b�g�Ƃ��ĕۑ����邩�ǂ������w�肵�܂�"), _windowSettings.isCreateAsset);

                if (_windowSettings.isCreateAsset)
                {
                    _windowSettings.assetName = EditorGUILayout.TextField(new GUIContent("�t�@�C����", "�ۑ�����Terrain Data�̃t�@�C�������w�肵�܂�"), (_windowSettings.assetName));

                    GUI.enabled = false;
                    EditorGUILayout.TextField(new GUIContent("�ۑ���", "Terrain Data��ۑ�����p�X��\�����܂�"), _windowSettings.assetPath);
                    GUI.enabled = true;

                    if(GUILayout.Button(new GUIContent("�ۑ�����w�肷��", "Terrain Data�̕ۑ�����t�H���_��I�����܂�")))
                    {
                        _windowSettings.assetPath = EditorUtility.OpenFolderPanel("�ۑ���I��", Application.dataPath, string.Empty);
                        string projectPath = Application.dataPath.Replace("Assets", string.Empty);

                        if(_windowSettings.assetPath == string.Empty)
                        {
                            _windowSettings.assetPath = Application.dataPath;
                        }

                        //���΃p�X���v�Z
                        Uri basisUri = new Uri(projectPath);
                        Uri absoluteUri = new Uri(_windowSettings.assetPath);
                        _windowSettings.assetPath = basisUri.MakeRelativeUri(absoluteUri).OriginalString;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Terrain Data��ۑ����Ȃ��ꍇ�A�o�͂��ꂽTerrain�̍Ďg�p������ɂȂ�܂�\n�ۑ����邱�Ƃ𐄏����܂�", MessageType.Warning);
                }
            }

            //�X�V
            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("�e���C���𐶐�����", "�ݒ�l����e���C���𐶐����܂�")))
            {
                //Data���R�s�[���ēn��
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
