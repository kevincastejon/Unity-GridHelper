using UnityEditor;
using UnityEngine;
namespace Grid2DHelper.Demos.RealtimeIsoBakedGridMap
{
    [CustomEditor(typeof(GridMap))]
    public class GridMapEditor : Editor
    {
        private SerializedProperty _pathGridGenerationProgress;
        private SerializedProperty _isGenerating;
        private GridMap _script;

        private void OnEnable()
        {
            _pathGridGenerationProgress = serializedObject.FindProperty("_pathGridGenerationProgress");
            _isGenerating = serializedObject.FindProperty("_isGenerating");

            _script = (GridMap)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginDisabledGroup(_isGenerating.boolValue);
            if (GUILayout.Button(_isGenerating.boolValue ? (_pathGridGenerationProgress.floatValue * 100).ToString("F0") + "%" : "Generate SerializedPathGrid"))
            {
                _script.RegisterTiles();
                _script.GeneratePathGrid();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
