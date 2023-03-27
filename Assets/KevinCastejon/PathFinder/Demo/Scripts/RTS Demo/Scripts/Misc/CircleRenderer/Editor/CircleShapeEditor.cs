using UnityEngine;
using UnityEditor;
namespace RTS_Demo
{
    [CustomEditor(typeof(CircleRenderer))]
    public class CircleShapeEditor : Editor
    {
        private void OnEnable()
        {
            _script = target as CircleRenderer;
        }

        public override void OnInspectorGUI()
        {
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                DrawDefaultInspector();

                if (changeScope.changed)
                {
                    _script.UpdateCircle();
                }
            }
        }

        private CircleRenderer _script;
    }
}