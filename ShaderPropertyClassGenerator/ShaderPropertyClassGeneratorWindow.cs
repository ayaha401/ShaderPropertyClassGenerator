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

        // �v���p�e�B�̐����擾
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
}
