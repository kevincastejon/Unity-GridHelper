using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace GridHelperDemoMisc
{
    [CustomEditor(typeof(DemoGridGenerator))]
    public class DemoGridGeneratorEditor : Editor
    {
        private SerializedProperty _tilePrefab;
        private SerializedProperty _3D;
        private SerializedProperty _width;
        private SerializedProperty _height;
        private SerializedProperty _depth;

        private DemoGridGenerator _object;


        private void OnEnable()
        {
            _tilePrefab = serializedObject.FindProperty("_tilePrefab");
            _3D = serializedObject.FindProperty("_3D");
            _width = serializedObject.FindProperty("_width");
            _height = serializedObject.FindProperty("_height");
            _depth = serializedObject.FindProperty("_depth");
            _object = (DemoGridGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_tilePrefab);
            EditorGUILayout.PropertyField(_3D);
            EditorGUILayout.PropertyField(_width);
            EditorGUILayout.PropertyField(_height);
            if (_3D.boolValue)
            {
                EditorGUILayout.PropertyField(_depth);
            }
            if (GUILayout.Button("Generate"))
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Generated tiles");
                var undoGroupIndex = Undo.GetCurrentGroup();
                _object.GenerateMap((o)=> Undo.RegisterCreatedObjectUndo(o, ""));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}