using System.Collections;
using System.Collections.Generic;
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
        private Shader _shader;
        private string _resultText;
        private string _savePath;
        string _indent = new string(' ', 4);


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

            DrawSaveClipboard(_resultText);

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

            for (int i = 0; i < propertyCount; i++)
            {
                string propertyName = ShaderUtil.GetPropertyName(_shader, i);

                _resultText += $"{_indent}public static readonly int {ConvertToConstantName(propertyName)}_ID = Shader.PropertyToID(\"{propertyName}\");";
                _resultText += Environment.NewLine;
            }

            GUILayout.Label("�v���p�e�B���o����");
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label(_resultText);
            }
        }

        /// <summary>
        /// �N���X���̓��͗�
        /// </summary>
        private void DrawSetClassName()
        {
            //using (new GUILayout.VerticalScope())
            {
                GUILayout.Label("�N���X��");

                _className = GUILayout.TextField(_className);
            }
        }

        /// <summary>
        /// �N���X�Ƃ��ĕۑ�����
        /// </summary>
        private void DrawSaveClass()
        {
            if (GUILayout.Button("�N���X��ۑ�"))
            {
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
        /// <param name="resultText">�ۑ��������e�L�X�g</param>
        private void DrawSaveClipboard(string resultText)
        {
            if (GUILayout.Button("�N���b�v�{�[�h�ɕۑ�"))
            {
                GUIUtility.systemCopyBuffer = resultText;
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
    }
}