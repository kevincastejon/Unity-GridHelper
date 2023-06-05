using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnableThreadSupport : EditorWindow
{
    [MenuItem("Window/WebGL Thread Support Window", false, 222)]
    internal static void OpenWindow()
    {
        EditorWindow window = GetWindow(typeof(EnableThreadSupport));
        window.titleContent = new GUIContent("Physics");
    }
    private void OnGUI()
    {
        PlayerSettings.WebGL.threadsSupport = EditorGUILayout.ToggleLeft("WebGL Thread Support", PlayerSettings.WebGL.threadsSupport);
    }
}
