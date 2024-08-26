using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

namespace AyahaGraphicDevelopTools.ShaderPropertyClassGenerator
{
    public class ShaderPropertyClassGeneratorWindow : EditorWindow
    {
        [SerializeField]
        private static string _className = String.Empty;

        [SerializeField]
        private bool[] _propertyIsSelected;

        private Shader _shader;
        private string[] _displayTextArray;
        private string _resultText;
        private string _savePath;
        private string _indent = new string(' ', 4);
        private bool _isOnce = false;

        /// <summary>
        /// �I���������̂�Shader�����ׂ�
        /// </summary>
        [MenuItem("Assets/Create/ShaderPropertyClass", true)]
        private static bool CheckSelectObjectIsShader()
        {
            return Selection.activeObject is Shader;
        }

        [MenuItem("Assets/Create/ShaderPropertyClass", priority = 83)]
        private static void ShaderPropertyClass()
        {
            var shader = Selection.activeObject as Shader;

            if (shader != null)
            {
                ShowWindow(shader);

                string[] shaderNameParts = shader.name.Split('/');
                if (shaderNameParts.Length > 1)
                {
                    _className = ToTopUpper(shaderNameParts[1]) + "Property";
                }
            }
        }

        public static void ShowWindow(Shader shader)
        {
            var window = GetWindow<ShaderPropertyClassGeneratorWindow>("PropertyClassGeneratorWindow");
            window.titleContent = new GUIContent("PropertyClassGeneratorWindow");
            window._shader = shader;
        }

        private void OnGUI()
        {
            if (_shader == null)
            {
                return;
            }

            DrawPropertyPreview();

            DrawSaveClipboard();

            EditorGUILayout.Space();

            DrawSetClassName();

            EditorGUILayout.Space();

            DrawSaveClass();
        }

        /// <summary>
        /// const�̖����K���ɍ��킹���������쐬����
        /// </summary>
        /// <param name="input">Property��</param>
        private string ConvertToConstantName(string input)
        {
            input = input.TrimStart('_');
            string result = Regex.Replace(input, "(?<=.)([A-Z])", "_$1");
            return result.ToUpper();
        }

        /// <summary>
        /// �v���p�e�B��ϐ��ɒ�`�����v���r���[��\������
        /// </summary>
        private void DrawPropertyPreview()
        {
            _resultText = string.Empty;

            // �v���p�e�B�̐����擾
            int propertyCount = ShaderUtil.GetPropertyCount(_shader);

            if (!_isOnce)
            {
                _propertyIsSelected = new bool[propertyCount];
                _displayTextArray = new string[propertyCount];

                for (int i = 0; i < propertyCount; i++)
                {
                    _propertyIsSelected[i] = true;
                    _displayTextArray[i] = string.Empty;
                }
                _isOnce = true;
            }

            GUILayout.Label("�v���p�e�B���o����");
            using (new GUILayout.VerticalScope("Box"))
            {
                for (int i = 0; i < propertyCount; i++)
                {
                    string propertyName = ShaderUtil.GetPropertyName(_shader, i);
                    string displayText = $"{_indent}public static readonly int {ConvertToConstantName(propertyName)}_ID = Shader.PropertyToID(\"{propertyName}\");";

                    using (new GUILayout.HorizontalScope())
                    {
                        _propertyIsSelected[i] = GUILayout.Toggle(_propertyIsSelected[i], displayText);
                        _displayTextArray[i] = displayText;
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("�S�I��"))
                    {
                        for (int i = 0; i < propertyCount; i++)
                        {
                            _propertyIsSelected[i] = true;
                        }
                    }

                    if (GUILayout.Button("�I������"))
                    {
                        for (int i = 0; i < propertyCount; i++)
                        {
                            _propertyIsSelected[i] = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �N���X���̓��͗�
        /// </summary>
        private void DrawSetClassName()
        {
            GUILayout.Label("�N���X��");
            _className = GUILayout.TextField(_className);
        }

        /// <summary>
        /// �N���X�Ƃ��ĕۑ�����
        /// </summary>
        private void DrawSaveClass()
        {
            if (GUILayout.Button("�N���X��ۑ�"))
            {
                SetResultText();

                if (String.IsNullOrEmpty(_className) || String.IsNullOrEmpty(_resultText))
                {
                    return;
                }

                string className = _className;
                StringBuilder classBuilder = new StringBuilder();
                classBuilder.AppendLine("using UnityEngine;");
                classBuilder.AppendLine("public static class " + className);
                classBuilder.AppendLine("{");
                classBuilder.AppendLine(_resultText);
                classBuilder.AppendLine("}");

                string selectPath = AssetDatabase.GetAssetPath(_shader);
                _savePath = Path.GetDirectoryName(selectPath);

                string classPath = Path.Combine(_savePath, className + ".cs");
                File.WriteAllText(classPath, classBuilder.ToString());
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// �N���b�v�{�[�h�ɕۑ�������
        /// </summary>
        private void DrawSaveClipboard()
        {
            if (GUILayout.Button("�N���b�v�{�[�h�ɕۑ�"))
            {
                SetResultText();

                GUIUtility.systemCopyBuffer = _resultText;
            }
        }

        /// <summary>
        /// �ŏ��̈ʒu������啶���ɕύX����
        /// </summary>
        private static string ToTopUpper(string value)
        {
            if (value.Length <= 0)
            {
                return string.Empty;
            }

            return char.ToUpper(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// ���ʂ�Text��Set����
        /// </summary>
        private void SetResultText()
        {
            // ��x������
            _resultText = string.Empty;

            for (int i = 0; i < _displayTextArray.Length; i++)
            {
                if (_propertyIsSelected[i])
                {
                    _resultText += _displayTextArray[i];
                    _resultText += Environment.NewLine;
                }
            }
        }
    }
}