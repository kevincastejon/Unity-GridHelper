using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Technical_Demo
{
    public enum MatrixVisualType
    {
        FLOOR,
        WALL,
        TARGET
    }
    //[CustomEditor(typeof(GridMap))]
    public class GridMapEditor : Editor
    {
        private SerializedProperty _allowDiagonals;
        private SerializedProperty _maxMovement;
        private SerializedProperty _targetMat;
        private SerializedProperty _floorMat;
        private SerializedProperty _wallMat;
        private SerializedProperty _pathMat;
        private SerializedProperty _target;
        private MatrixVisualType _matrixType;
        private GridMap _object;

        private void OnEnable()
        {
            _allowDiagonals = serializedObject.FindProperty("_allowDiagonals");
            _maxMovement = serializedObject.FindProperty("_maxMovement");
            _targetMat = serializedObject.FindProperty("_targetMat");
            _floorMat = serializedObject.FindProperty("_floorMat");
            _wallMat = serializedObject.FindProperty("_wallMat");
            _pathMat = serializedObject.FindProperty("_pathMat");
            _target = serializedObject.FindProperty("_target");
            _object = (GridMap)target;
            _object.Awake();
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying || _object.gameObject.scene.name == null || _object.gameObject.scene.name == _object.gameObject.name)
            {
                return;
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(_targetMat);
            EditorGUILayout.PropertyField(_floorMat);
            EditorGUILayout.PropertyField(_wallMat);
            EditorGUILayout.PropertyField(_pathMat);
            EditorGUILayout.PropertyField(_maxMovement);
            _allowDiagonals.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Allow Diagonals", "Allow Diagonals"), _allowDiagonals.boolValue);
            EditorGUILayout.LabelField("Map editing");
            _matrixType = (MatrixVisualType)EditorGUILayout.EnumPopup(_matrixType);
            for (int i = 0; i < 12; i++)
            {
                Rect rect = EditorGUILayout.GetControlRect(false);

                for (int j = 0; j < 10; j++)
                {
                    Rect area = new Rect(rect.x + (EditorGUIUtility.singleLineHeight * j), rect.y, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
                    SerializedObject so;
                    SerializedObject soRenderer;
                    bool newWalkableValue;
                    SerializedObject serializedFloor = new SerializedObject(_object.Map[i, j]);
                    Renderer renderer = (Renderer)serializedFloor.FindProperty("_renderer").objectReferenceValue;
                    bool isTarget = serializedFloor.FindProperty("_isTarget").boolValue;
                    bool isWalkable = serializedFloor.FindProperty("_isWalkable").boolValue;
                    switch (_matrixType)
                    {
                        case MatrixVisualType.FLOOR:
                            EditorGUI.BeginDisabledGroup(isTarget);
                            so = new SerializedObject(_object.Map[i, j]);
                            so.Update();
                            soRenderer = new SerializedObject(renderer);
                            soRenderer.Update();
                            newWalkableValue = EditorGUI.Toggle(area, isWalkable);
                            so.FindProperty("_isWalkable").boolValue = newWalkableValue;
                            so.ApplyModifiedProperties();
                            soRenderer.FindProperty("m_Materials").GetArrayElementAtIndex(0).objectReferenceValue = GetMaterial(so);
                            soRenderer.ApplyModifiedProperties();
                            EditorGUI.EndDisabledGroup();
                            break;
                        case MatrixVisualType.WALL:
                            EditorGUI.BeginDisabledGroup(isTarget);
                            so = new SerializedObject(_object.Map[i, j]);
                            so.Update();
                            soRenderer = new SerializedObject(renderer);
                            soRenderer.Update();
                            newWalkableValue = !EditorGUI.Toggle(area, !isWalkable);
                            so.FindProperty("_isWalkable").boolValue = newWalkableValue;
                            so.ApplyModifiedProperties();
                            soRenderer.FindProperty("m_Materials").GetArrayElementAtIndex(0).objectReferenceValue = GetMaterial(so);
                            soRenderer.ApplyModifiedProperties();
                            EditorGUI.EndDisabledGroup();
                            break;
                        case MatrixVisualType.TARGET:
                            EditorGUI.BeginDisabledGroup(!isWalkable || isTarget);
                            so = new SerializedObject(_object.Map[i, j]);
                            so.Update();
                            soRenderer = new SerializedObject(renderer);
                            soRenderer.Update();
                            EditorGUI.BeginChangeCheck();
                            so.FindProperty("_isTarget").boolValue = EditorGUI.Toggle(area, isTarget);
                            bool hasChanged = EditorGUI.EndChangeCheck();
                            so.ApplyModifiedProperties();
                            soRenderer.FindProperty("m_Materials").GetArrayElementAtIndex(0).objectReferenceValue = GetMaterial(so);
                            soRenderer.ApplyModifiedProperties();
                            if (hasChanged)
                            {
                                so = new SerializedObject(_target.objectReferenceValue);
                                so.Update();
                                soRenderer = new SerializedObject(new SerializedObject(_target.objectReferenceValue).FindProperty("_renderer").objectReferenceValue);
                                soRenderer.Update();
                                so.FindProperty("_isTarget").boolValue = false;
                                so.ApplyModifiedProperties();
                                soRenderer.FindProperty("m_Materials").GetArrayElementAtIndex(0).objectReferenceValue = GetMaterial(so);
                                soRenderer.ApplyModifiedProperties();
                                _target.objectReferenceValue = _object.Map[i, j];
                            }
                            EditorGUI.EndDisabledGroup();
                            break;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        private Object GetMaterial(SerializedObject so)
        {
            if (!so.FindProperty("_isWalkable").boolValue)
            {
                return _wallMat.objectReferenceValue;
            }
            else if (so.FindProperty("_isTarget").boolValue)
            {
                return _targetMat.objectReferenceValue;
            }
            else if (so.FindProperty("_isPath").boolValue)
            {
                return _pathMat.objectReferenceValue;
            }
            else
            {
                return _floorMat.objectReferenceValue;
            }
        }
    }
}
