using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

public class ShaderPropertyClassGeneratorWindow : EditorWindow
{
    private Shader _shader;
    private string _resultText;

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
        
        if(shader != null)
        {
            ShowWindow(shader);
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
        if(_shader == null)
        {
            return;
        }

        _resultText = string.Empty;

        // プロパティの数を取得
        int propertyCount = ShaderUtil.GetPropertyCount(_shader);

        for (int i = 0; i < propertyCount; i++)
        {
            string propertyName = ShaderUtil.GetPropertyName(_shader, i);
            string propertyType = ShaderUtil.GetPropertyType(_shader, i).ToString();

            _resultText += $"public static readonly int {ConvertToConstantName(propertyName)}_ID = Shader.PropertyToID(\"{propertyName}\");";
            _resultText += Environment.NewLine;
        }

        GUILayout.Label(_resultText);

        DrawSaveClipboard(_resultText);
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
    /// クリップボードに保存をする
    /// </summary>
    /// <param name="resultText">保存したいテキスト</param>
    private void DrawSaveClipboard(string resultText)
    {
        if (GUILayout.Button("クリップボードに保存"))
        {
            GUIUtility.systemCopyBuffer = resultText;
        }
    }
}
