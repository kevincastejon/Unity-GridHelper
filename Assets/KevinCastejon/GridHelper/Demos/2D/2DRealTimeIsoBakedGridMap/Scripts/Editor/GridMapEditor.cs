using UnityEditor;
using UnityEngine;
namespace Grid2DHelper.Demos.RealtimeIsoBakedGridMap
{
    [CustomEditor(typeof(GridMap))]
    public class GridMapEditor : Editor
    {
        private SerializedProperty _pathGridGenerationProgress;
        private SerializedProperty _mobPrefab;
        private SerializedProperty _spawnDelay;
        private SerializedProperty _mobs;
        private SerializedProperty _scriptablePathGrid;

        private void OnEnable()
        {
            _pathGridGenerationProgress = serializedObject.FindProperty("_pathGridGenerationProgress");
            _mobPrefab = serializedObject.FindProperty("_mobPrefab");
            _spawnDelay = serializedObject.FindProperty("_spawnDelay");
            _mobs = serializedObject.FindProperty("_mobs");
            _scriptablePathGrid = serializedObject.FindProperty("_scriptablePathGrid");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate SerializedPathGrid"))
            {

            }
        }
    }
}
