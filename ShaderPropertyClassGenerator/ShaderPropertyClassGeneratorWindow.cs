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
        /// 選択したものがShaderか調べる
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
        /// constの命名規則に合わせた命名を作成する
        /// </summary>
        /// <param name="input">Property名</param>
        private string ConvertToConstantName(string input)
        {
            input = input.TrimStart('_');
            string result = Regex.Replace(input, "(?<=.)([A-Z])", "_$1");
            return result.ToUpper();
        }

        /// <summary>
        /// プロパティを変数に定義したプレビューを表示する
        /// </summary>
        private void DrawPropertyPreview()
        {
            _resultText = string.Empty;

            // プロパティの数を取得
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

            GUILayout.Label("プロパティ検出結果");
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
                    if (GUILayout.Button("全選択"))
                    {
                        for (int i = 0; i < propertyCount; i++)
                        {
                            _propertyIsSelected[i] = true;
                        }
                    }

                    if (GUILayout.Button("選択解除"))
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
        /// クラス名の入力欄
        /// </summary>
        private void DrawSetClassName()
        {
            GUILayout.Label("クラス名");
            _className = GUILayout.TextField(_className);
        }

        /// <summary>
        /// クラスとして保存する
        /// </summary>
        private void DrawSaveClass()
        {
            if (GUILayout.Button("クラスを保存"))
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
        /// クリップボードに保存をする
        /// </summary>
        private void DrawSaveClipboard()
        {
            if (GUILayout.Button("クリップボードに保存"))
            {
                SetResultText();

                GUIUtility.systemCopyBuffer = _resultText;
            }
        }

        /// <summary>
        /// 最初の位置文字を大文字に変更する
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
        /// 結果のTextをSetする
        /// </summary>
        private void SetResultText()
        {
            // 一度初期化
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