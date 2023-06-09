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
            EditorGUI.BeginDisabledGroup(_tilePrefab.objectReferenceValue == null);
            if (GUILayout.Button("Generate"))
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Generated tiles");
                var undoGroupIndex = Undo.GetCurrentGroup();
                for (int y = 0; y < _height.intValue; y++)
                {
                    GameObject line = new GameObject("Line (" + y + ")");
                    line.transform.parent = _object.transform;
                    line.transform.localPosition = new Vector3(0f, y, 0f);
                    Undo.RegisterCreatedObjectUndo(line, "");
                    for (int x = 0; x < _width.intValue; x++)
                    {
                        if (!_3D.boolValue)
                        {
                            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(_tilePrefab.objectReferenceValue);
                            tile.name = "Tile (" + x + ")";
                            tile.transform.parent = line.transform;
                            tile.transform.localPosition = new Vector3(x, 0f, 0f);
                            Undo.RegisterCreatedObjectUndo(tile, "");
                            continue;
                        }
                        GameObject col = new("Column (" + x + ")");
                        col.transform.parent = line.transform;
                        col.transform.localPosition = new Vector3(x, 0f, 0f);
                        Undo.RegisterCreatedObjectUndo(col, "");
                        for (int z = 0; z < _depth.intValue; z++)
                        {
                            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(_tilePrefab.objectReferenceValue);
                            tile.name = "Tile (" + z + ")";
                            tile.transform.parent = col.transform;
                            tile.transform.localPosition = new Vector3(0f, 0f, z);
                            Undo.RegisterCreatedObjectUndo(tile, "");
                        }
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
}