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
        [SerializeField] private Toggle _flyToggle;
        [SerializeField] private Toggle _wallBelowToggle;
        [SerializeField] private Toggle _wallAsideToggle;
        [SerializeField] private Toggle _wallAboveToggle;

        private float _maxDistance = 6f;
        private EdgesDiagonalsPolicy _edgesDiagoPolicy = EdgesDiagonalsPolicy.DIAGONAL_2FREE;
        private float _edgesDiagoWeight = (Vector2.up + Vector2.right).magnitude;
        private VerticesDiagonalsPolicy _verticesDiagoPolicy = VerticesDiagonalsPolicy.DIAGONAL_6FREE;
        private float _verticesDiagoWeight = (Vector3.up + Vector3.right + Vector3.forward).magnitude;
        private MovementPolicy _movementPolicy = MovementPolicy.WALL_BELOW;

        Tile[,,] _map = new Tile[12, 16, 18];
        Tile[] _accessibleTiles;
        Tile[] _path;

        private Tile _targetTile;
        private Tile _stopTile;
        PathMap3D<Tile> _pathMap;

        public float MaxDistance
        {
            get
            {
                return _maxDistance;
            }

            set
            {
                _maxDistance = value;
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
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
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
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
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
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
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
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
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
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
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
            }
        }

        public void SetEdgesDiagonalsPolicy(int enumIndex)
        {
            EdgesDiagoPolicy = (EdgesDiagonalsPolicy)enumIndex;
        }
        public void SetVerticesDiagonalsPolicy(int enumIndex)
        {
            VerticesDiagoPolicy = (VerticesDiagonalsPolicy)enumIndex;
        }
        public void SetMovementPolicy()
        {
            int value = 0;
            if (!_flyToggle.isOn)
            {
                if (!_wallBelowToggle.isOn && !_wallAsideToggle.isOn && !_wallAboveToggle.isOn)
                {
                    _flyToggle.isOn = true;
                }
                else
                {
                    if (_wallBelowToggle.isOn)
                    {
                        value += 1;
                    }
                    if (_wallAsideToggle.isOn)
                    {
                        value += 2;
                    }
                    if (_wallAboveToggle.isOn)
                    {
                        value += 4;
                    }
                }
            }
            MovementPolicy = (MovementPolicy)value;
        }

        public void MoveStartX(int value)
        {
            if (_map[_targetTile.Y, value, _targetTile.Z].IsWalkable && _map[_targetTile.Y, value, _targetTile.Z] != _stopTile)
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
            if (_map[value, _targetTile.X, _targetTile.Z].IsWalkable && _map[value, _targetTile.X, _targetTile.Z] != _stopTile)
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
            if (_map[_targetTile.Y, _targetTile.X, value].IsWalkable && _map[_targetTile.Y, _targetTile.X, value] != _stopTile)
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
            if (_map[_stopTile.Y, value, _stopTile.Z].IsWalkable && _map[_stopTile.Y, value, _stopTile.Z] != _targetTile)
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
            if (_map[value, _stopTile.X, _stopTile.Z].IsWalkable && _map[value, _stopTile.X, _stopTile.Z] != _targetTile)
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
            if (_map[_stopTile.Y, _stopTile.X, value].IsWalkable && _map[_stopTile.Y, _stopTile.X, value] != _targetTile)
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
            _targetTile.TileMode = TileMode.TARGET;
            _stopTile = _map[1, 2, 2];
            _stopTile.TileMode = TileMode.STOP;
            CalculatePathMap();
            ShowAccessibleTiles();
            ShowPath();
        }

        private void SetStop(Tile tile)
        {
            ClearPath();
            _stopTile.TileMode = _pathMap.IsTileAccessible(_stopTile) ? TileMode.ACCESSIBLE : TileMode.AIR;
            _stopTile = tile;
            _stopTile.TileMode = TileMode.STOP;
            ShowPath();
        }
        private void SetStart(Tile tile)
        {
            ClearAccessibleTiles();
            ClearPath();
            _targetTile.TileMode = _pathMap.IsTileAccessible(_targetTile) ? TileMode.ACCESSIBLE : TileMode.AIR;
            _targetTile = tile;
            _targetTile.TileMode = TileMode.TARGET;
            CalculatePathMap();
            ShowAccessibleTiles();
            ShowPath();
        }

        private void CalculatePathMap()
        {
            _pathMap = Pathfinding3D.GeneratePathMap(_map, _targetTile, _maxDistance, _edgesDiagoPolicy, _edgesDiagoWeight, _verticesDiagoPolicy, _verticesDiagoWeight, _movementPolicy);
        }

        private void ClearAccessibleTiles()
        {
            if (_accessibleTiles != null)
            {
                foreach (Tile tile in _accessibleTiles)
                {
                    if (tile != _stopTile)
                    {
                        tile.TileMode = TileMode.AIR;
                    }
                }
            }
        }
        private void ShowAccessibleTiles()
        {
            _accessibleTiles = _pathMap.GetAccessibleTiles(false);
            foreach (Tile tile in _accessibleTiles)
            {
                if (tile != _stopTile)
                {
                    tile.TileMode = TileMode.ACCESSIBLE;
                }
            }
        }

        private void ClearPath()
        {
            if (_path != null)
            {
                foreach (Tile tile in _path)
                {
                    tile.TileMode = _pathMap.IsTileAccessible(tile) ? TileMode.ACCESSIBLE : TileMode.AIR;
                }
            }
        }
        private void ShowPath()
        {
            if (_pathMap.IsTileAccessible(_stopTile))
            {
                _path = _pathMap.GetPathFromTarget(_stopTile, false, false);
                foreach (Tile tile in _path)
                {
                    tile.TileMode = TileMode.PATH;
                }
            }
        }
    }
}