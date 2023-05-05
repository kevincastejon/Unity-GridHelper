using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid3DHelper.APIDemo.Pathfinding
{
    public enum DemoType
    {
        ACCESSIBLE_TILES,
        PATH,
    }
    public class Grid3DMap : MonoBehaviour
    {
        [SerializeField] private Slider _sliderStartX;
        [SerializeField] private TextMeshProUGUI _sliderStartCountX;
        [SerializeField] private Slider _sliderStartY;
        [SerializeField] private TextMeshProUGUI _sliderStartCountY;
        [SerializeField] private Slider _sliderStartZ;
        [SerializeField] private TextMeshProUGUI _sliderStartCountZ;
        [SerializeField] private Slider _sliderStopX;
        [SerializeField] private TextMeshProUGUI _sliderStopCountX;
        [SerializeField] private Slider _sliderStopY;
        [SerializeField] private TextMeshProUGUI _sliderStopCountY;
        [SerializeField] private Slider _sliderStopZ;
        [SerializeField] private TextMeshProUGUI _sliderStopCountZ;
        private DemoType _demoType;
        private EdgesDiagonalsPolicy _edgesDiagoPolicy = EdgesDiagonalsPolicy.DIAGONAL_2FREE;
        private float _edgesDiagoWeight = (Vector2.up + Vector2.right).magnitude;
        private VerticesDiagonalsPolicy _verticesDiagoPolicy = VerticesDiagonalsPolicy.DIAGONAL_6FREE;
        private float _verticesDiagoWeight = (Vector3.up + Vector3.right + Vector3.forward).magnitude;
        private MovementPolicy _movementPolicy = MovementPolicy.WALL_BELOW;
        Tile[,,] _map = new Tile[12, 16, 18];
        Tile[] _path;
        private Tile _targetTile;
        private Tile _stopTile;
        private float _maxDistance = 6f;
        PathMap3D<Tile> _pathMap;

        public DemoType DemoType
        {
            get
            {
                return _demoType;
            }

            set
            {
                if (_demoType == value)
                {
                    return;
                }
                _demoType = value;
                if (_demoType == DemoType.PATH)
                {
                    SetRandomAccessibleStop();
                }
                Demo();
            }
        }

        private void SetRandomAccessibleStop()
        {
            if (_stopTile)
            {
                _stopTile.TileMode = TileMode.SEMIFADE;
            }
            Tile[] tiles = _pathMap.GetAccessibleTiles(false);
            _stopTile = tiles[Random.Range(0, tiles.Length)];
            _stopTile.TileMode = TileMode.OPAQUE;
            _sliderStopX.SetValueWithoutNotify(_stopTile.X);
            _sliderStopY.SetValueWithoutNotify(_stopTile.Y);
            _sliderStopZ.SetValueWithoutNotify(_stopTile.Z);
        }

        public float MaxDistance
        {
            get
            {
                return _maxDistance;
            }

            set
            {
                _maxDistance = value;
                CalculatePathMap();
            }
        }
        public float EdgesDiagoWeight
        {
            get
            {
                return _edgesDiagoWeight;
            }

            set
            {
                _edgesDiagoWeight = value;
                CalculatePathMap();
            }
        }
        public float VerticesDiagoWeight
        {
            get
            {
                return _verticesDiagoWeight;
            }

            set
            {
                _verticesDiagoWeight = value;
                CalculatePathMap();
            }
        }
        public EdgesDiagonalsPolicy EdgesDiagoPolicy
        {
            get
            {
                return _edgesDiagoPolicy;
            }

            set
            {
                _edgesDiagoPolicy = value;
                CalculatePathMap();
            }
        }
        public VerticesDiagonalsPolicy VerticesDiagoPolicy
        {
            get
            {
                return _verticesDiagoPolicy;
            }

            set
            {
                _verticesDiagoPolicy = value;
                CalculatePathMap();
            }
        }
        public MovementPolicy MovementPolicy
        {
            get
            {
                return _movementPolicy;
            }

            set
            {
                _movementPolicy = value;
                CalculatePathMap();
            }
        }

        public void SetDemoType(int enumIndex)
        {
            DemoType = (DemoType)enumIndex;
        }
        public void SetEdgesDiagonalsPolicy(int enumIndex)
        {
            EdgesDiagoPolicy = (EdgesDiagonalsPolicy)enumIndex;
        }
        public void SetVerticesDiagonalsPolicy(int enumIndex)
        {
            VerticesDiagoPolicy = (VerticesDiagonalsPolicy)enumIndex;
        }
        public void SetMovementPolicy(int enumIndex)
        {
            MovementPolicy = (MovementPolicy)enumIndex;
        }
        public void MoveStartX(int value)
        {
            if (_map[_targetTile.Y, value, _targetTile.Z].IsWalkable)
            {
                SetStart(_map[_targetTile.Y, value, _targetTile.Z]);
                _sliderStartCountX.text = value.ToString();
            }
            else
            {
                _sliderStartX.SetValueWithoutNotify(_targetTile.X);
                _sliderStartCountX.text = _targetTile.X.ToString();
            }
        }
        public void MoveStartY(int value)
        {
            if (_map[value, _targetTile.X, _targetTile.Z].IsWalkable)
            {
                SetStart(_map[value, _targetTile.X, _targetTile.Z]);
                _sliderStartCountY.text = value.ToString();
            }
            else
            {
                _sliderStartY.SetValueWithoutNotify(_targetTile.Y);
                _sliderStartCountY.text = _targetTile.Y.ToString();
            }
        }
        public void MoveStartZ(int value)
        {
            if (_map[_targetTile.Y, _targetTile.X, value].IsWalkable)
            {
                SetStart(_map[_targetTile.Y, _targetTile.X, value]);
                _sliderStartCountZ.text = value.ToString();
            }
            else
            {
                _sliderStartZ.SetValueWithoutNotify(_targetTile.Z);
                _sliderStartCountZ.text = _targetTile.Z.ToString();
            }
        }
        public void MoveStopX(int value)
        {
            if (_pathMap.IsTileAccessible(_map[_stopTile.Y, value, _stopTile.Z]))
            {
                SetStop(_map[_stopTile.Y, value, _stopTile.Z]);
                _sliderStopCountX.text = value.ToString();
            }
            else
            {
                _sliderStopX.SetValueWithoutNotify(_stopTile.X);
                _sliderStopCountX.text = _stopTile.X.ToString();
            }
        }
        public void MoveStopY(int value)
        {
            if (_pathMap.IsTileAccessible(_map[value, _stopTile.X, _stopTile.Z]))
            {
                SetStop(_map[value, _stopTile.X, _stopTile.Z]);
                _sliderStopCountY.text = value.ToString();
            }
            else
            {
                _sliderStopY.SetValueWithoutNotify(_stopTile.Y);
                _sliderStopCountY.text = _stopTile.Y.ToString();
            }
        }
        public void MoveStopZ(int value)
        {
            if (_pathMap.IsTileAccessible(_map[_stopTile.Y, _stopTile.X, value]))
            {
                SetStop(_map[_stopTile.Y, _stopTile.X, value]);
                _sliderStopCountZ.text = value.ToString();
            }
            else
            {
                _sliderStopZ.SetValueWithoutNotify(_stopTile.Z);
                _sliderStopCountZ.text = _stopTile.Z.ToString();
            }
        }

        private void Awake()
        {
            Tile[] tiles = FindObjectsOfType<Tile>();
            foreach (Tile tile in tiles)
            {
                tile.X = Mathf.RoundToInt(tile.transform.position.x);
                tile.Y = Mathf.RoundToInt(tile.transform.position.y);
                tile.Z = Mathf.RoundToInt(tile.transform.position.z);
                _map[tile.Y, tile.X, tile.Z] = tile;
            }
            _targetTile = _map[1, 8, 9];
            _targetTile.TileMode = TileMode.OPAQUE;
            CalculatePathMap();
        }

        private void SetStop(Tile tile)
        {
            if (!_pathMap.IsTileAccessible(tile))
            {
                return;
            }
            if (_stopTile)
            {
                _stopTile.TileMode = TileMode.FADE;
            }
            _stopTile = tile;
            Demo();
            _stopTile.TileMode = TileMode.OPAQUE;
        }
        private void SetStart(Tile tile)
        {
            if (_targetTile)
            {
                _targetTile.TileMode = TileMode.FADE;
            }
            _targetTile = tile;
            CalculatePathMap();
            _targetTile.TileMode = TileMode.OPAQUE;
        }

        private void CalculatePathMap()
        {
            _pathMap = Pathfinding3D.GeneratePathMap(_map, _targetTile, _maxDistance, _edgesDiagoPolicy, _edgesDiagoWeight, _verticesDiagoPolicy, _verticesDiagoWeight, _movementPolicy);
            if (_stopTile && !_pathMap.IsTileAccessible(_stopTile))
            {
                SetRandomAccessibleStop();
            }
            Demo();
        }
        private void Demo()
        {
            if (_path != null)
            {
                foreach (Tile tile in _path)
                {
                    tile.TileMode = TileMode.FADE;
                }
            }
            switch (_demoType)
            {
                case DemoType.ACCESSIBLE_TILES:
                    _path = _pathMap.GetAccessibleTiles(false);
                    break;
                case DemoType.PATH:
                    _path = _pathMap.GetPathFromTarget(_stopTile, false, false);
                    break;
                default:
                    break;
            }
            foreach (Tile tile in _path)
            {
                if (tile != _stopTile)
                {
                    tile.TileMode = TileMode.SEMIFADE;
                }
            }
        }
    }
}