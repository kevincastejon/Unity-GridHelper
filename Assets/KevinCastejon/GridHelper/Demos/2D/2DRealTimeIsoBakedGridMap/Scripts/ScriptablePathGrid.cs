using KevinCastejon.GridHelper;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Grid2DHelper.Demos.RealtimeIsoBakedGridMap
{
    [CreateAssetMenu(fileName = "ScriptablePathGrid", menuName = "Scriptable Objects/ScriptablePathGrid")]
    public class ScriptablePathGrid : ScriptableObject
    {
        [SerializeField] private SerializedPathGrid _pathGrid;

        public SerializedPathGrid PathGrid { get => _pathGrid; set => _pathGrid = value; }
    }
}
