using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid3DHelper.APIDemos.Pathfinding
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
        [SerializeField] private TMP_Dropdown _horizontalEdgesDiagonalsPolicyDropdown;
        [SerializeField] private Slider _horizontalEdgesDiagonalsWeightSlider;
        [SerializeField] private TextMeshProUGUI _horizontalEdgesDiagonalsWeightCount;
        [SerializeField] private TMP_Dropdown _verticalEdgesDiagonalsPolicyDropdown;
        [SerializeField] private Slider _verticalEdgesDiagonalsWeightSlider;
        [SerializeField] private TextMeshProUGUI _verticalEdgesDiagonalsWeightCount;
        [SerializeField] private TMP_Dropdown _verticesDiagonalsPolicyDropdown;
        [SerializeField] private Slider _verticesDiagonalsWeightSlider;
        [SerializeField] private TextMeshProUGUI _verticesDiagonalsWeightCount;
        [SerializeField] private Toggle _flyToggle;
        [SerializeField] private Toggle _wallBelowToggle;
        [SerializeField] private Toggle _wallAsideToggle;
        [SerializeField] private Toggle _wallAboveToggle;
        [SerializeField] private Pathfinding3DPolicy _pathfindingPolicy;

        private float _maxDistance = 6f;

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
        public float HorizontalEdgesDiagonalsWeight
        {
            get
            {
                return _pathfindingPolicy.HorizontalEdgesDiagonalsWeight;
            }

            set
            {
                _pathfindingPolicy.HorizontalEdgesDiagonalsWeight = value;
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
            }
        }
        public float VerticalEdgesDiagonalsWeight
        {
            get
            {
                return _pathfindingPolicy.VerticalEdgesDiagonalsWeight;
            }

            set
            {
                _pathfindingPolicy.VerticalEdgesDiagonalsWeight = value;
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
            }
        }
        public float VerticesDiagonalsWeight
        {
            get
            {
                return _pathfindingPolicy.VerticesDiagonalsWeight;
            }

            set
            {
                _pathfindingPolicy.VerticesDiagonalsWeight = value;
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
            }
        }
        public EdgesDiagonals3DPolicy HorizontalEdgesDiagonalsPolicy
        {
            get
            {
                return _pathfindingPolicy.HorizontalEdgesDiagonalsPolicy;
            }

            set
            {
                _pathfindingPolicy.HorizontalEdgesDiagonalsPolicy = value;
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
            }
        }
        public EdgesDiagonals3DPolicy VerticalEdgesDiagonalsPolicy
        {
            get
            {
                return _pathfindingPolicy.VerticalEdgesDiagonalsPolicy;
            }

            set
            {
                _pathfindingPolicy.VerticalEdgesDiagonalsPolicy = value;
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
            }
        }
        public VerticesDiagonals3DPolicy VerticesDiagonalsPolicy
        {
            get
            {
                return _pathfindingPolicy.VerticesDiagonalsPolicy;
            }

            set
            {
                _pathfindingPolicy.VerticesDiagonalsPolicy = value;
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
            }
        }
        public Movement3DPolicy MovementPolicy
        {
            get
            {
                return _pathfindingPolicy.MovementPolicy;
            }

            set
            {
                _pathfindingPolicy.MovementPolicy = value;
                ClearPath();
                ClearAccessibleTiles();
                CalculatePathMap();
                ShowAccessibleTiles();
                ShowPath();
            }
        }

        public void SetHorizontalEdgesDiagonalsPolicy(int enumIndex)
        {
            HorizontalEdgesDiagonalsPolicy = (EdgesDiagonals3DPolicy)enumIndex;
        }
        public void SetVerticalEdgesDiagonalsPolicy(int enumIndex)
        {
            VerticalEdgesDiagonalsPolicy = (EdgesDiagonals3DPolicy)enumIndex;
        }
        public void SetVerticesDiagonalsPolicy(int enumIndex)
        {
            VerticesDiagonalsPolicy = (VerticesDiagonals3DPolicy)enumIndex;
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
            MovementPolicy = (Movement3DPolicy)value;
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
            _sliderStartY.SetValueWithoutNotify(1);
            _sliderStartCountY.text = "1";
            _sliderStartX.SetValueWithoutNotify(8);
            _sliderStartCountX.text = "8";
            _sliderStartZ.SetValueWithoutNotify(9);
            _sliderStartCountZ.text = "9";
            _stopTile = _map[4, 2, 9];
            _stopTile.TileMode = TileMode.STOP;
            _sliderStopY.SetValueWithoutNotify(4);
            _sliderStopCountY.text = "4";
            _sliderStopX.SetValueWithoutNotify(2);
            _sliderStopCountX.text = "2";
            _sliderStopZ.SetValueWithoutNotify(9);
            _sliderStopCountZ.text = "9";
            _horizontalEdgesDiagonalsPolicyDropdown.SetValueWithoutNotify((int)_pathfindingPolicy.HorizontalEdgesDiagonalsPolicy);
            _horizontalEdgesDiagonalsWeightCount.text = _pathfindingPolicy.HorizontalEdgesDiagonalsWeight.ToString("F2");
            _verticalEdgesDiagonalsPolicyDropdown.SetValueWithoutNotify((int)_pathfindingPolicy.VerticalEdgesDiagonalsPolicy);
            _verticalEdgesDiagonalsWeightCount.text = _pathfindingPolicy.VerticalEdgesDiagonalsWeight.ToString("F2");
            _verticesDiagonalsPolicyDropdown.SetValueWithoutNotify((int)_pathfindingPolicy.VerticesDiagonalsPolicy);
            _horizontalEdgesDiagonalsWeightSlider.SetValueWithoutNotify(_pathfindingPolicy.HorizontalEdgesDiagonalsWeight);
            _verticalEdgesDiagonalsWeightSlider.SetValueWithoutNotify(_pathfindingPolicy.VerticalEdgesDiagonalsWeight);
            _verticesDiagonalsWeightCount.text = _pathfindingPolicy.VerticesDiagonalsWeight.ToString("F2");
            _verticesDiagonalsWeightSlider.SetValueWithoutNotify(_pathfindingPolicy.VerticesDiagonalsWeight);
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
            _targetTile.TileMode = TileMode.AIR;
            _targetTile = tile;
            _targetTile.TileMode = TileMode.TARGET;
            CalculatePathMap();
            ShowAccessibleTiles();
            ShowPath();
        }

        private void CalculatePathMap()
        {
            _pathMap = Pathfinding3D.GeneratePathMap(_map, _targetTile, _maxDistance, _pathfindingPolicy);
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
                    tile.TileMode = TileMode.AIR;
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