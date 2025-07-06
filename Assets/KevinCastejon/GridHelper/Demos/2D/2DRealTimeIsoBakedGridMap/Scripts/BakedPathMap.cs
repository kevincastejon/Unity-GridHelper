using KevinCastejon.GridHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Grid2DHelper.Demos.RealtimeIsoBakedGridMap
{
    [CreateAssetMenu(fileName = "BakedPathMap", menuName = "Scriptable Objects/BakedPathMap")]
    public class BakedPathMap : ScriptableObject
    {
        [SerializeField] private List<BakedTile> _accessibleTiles;
        [SerializeField] private BakedTile _target;
        [SerializeField] private float _maxDistance;
        [SerializeField] private MajorOrder _majorOrder;

        public BakedPathMap(List<BakedTile> accessibleTiles, BakedTile target, float maxDistance, MajorOrder majorOrder)
        {
            _accessibleTiles = accessibleTiles;
            _target = target;
            _maxDistance = maxDistance;
            _majorOrder = majorOrder;
        }
        public List<BakedTile> AccessibleTiles { get => _accessibleTiles; set => _accessibleTiles = value; }
        public BakedTile Target { get => _target; set => _target = value; }
        public float MaxDistance { get => _maxDistance; set => _maxDistance = value; }
        public MajorOrder MajorOrder { get => _majorOrder; set => _majorOrder = value; }
    }
    [Serializable]
    public struct BakedTile
    {
        [SerializeField] private bool _isWalkable;
        [SerializeField] private float _weight;
        [SerializeField] private int _x;
        [SerializeField] private int _y;
        [SerializeField] private Vector2Int _nextNodeCoord;
        [SerializeField] private Vector2Int _nextDirection;
        [SerializeField] private float _distanceToTarget;

        public BakedTile(bool isWalkable, float weight, int x, int y, Vector2Int nextNodeCoord, Vector2Int nextDirection, float distanceToTarget)
        {
            _isWalkable = isWalkable;
            _weight = weight;
            _x = x;
            _y = y;
            _nextNodeCoord = nextNodeCoord;
            _nextDirection = nextDirection;
            _distanceToTarget = distanceToTarget;
        }
        //public BakedTile(Node<Tile> node)
        //{
        //    _isWalkable = node.IsWalkable;
        //    _weight = node.Weight;
        //    _x = node.Tile.X;
        //    _y = node.Tile.Y;
        //    _nextNodeCoord = new(node.NextNode.Tile.X, node.NextNode.Tile.Y);
        //    _nextDirection = node.NextDirection;
        //    _distanceToTarget = node.DistanceToTarget;
        //}
        public bool IsWalkable { get => _isWalkable; set => _isWalkable = value; }
        public float Weight { get => _weight; set => _weight = value; }
        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }
        public Vector2Int NextNodeCoord { get => _nextNodeCoord; set => _nextNodeCoord = value; }
        public Vector2Int NextDirection { get => _nextDirection; set => _nextDirection = value; }
        public float DistanceToTarget { get => _distanceToTarget; set => _distanceToTarget = value; }
    }
}
