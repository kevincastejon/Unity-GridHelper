using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid2DHelper.APIDemo.RaycastingDemo
{
    public enum DemoType
    {
        LINE_OF_TILES,
        LINE_OF_SIGHT,
        CONE_OF_VISION,
    }
    public class Grid2DMap : MonoBehaviour
    {
        [SerializeField] private Image _losClearLED;
        [SerializeField] private Image _covClearLED;
        private float _openingAngle = 90f;
        private float _directionAngle = 0f;
        private Camera _camera;
        private Tile[,] _map = new Tile[65, 71];
        private Tile[] _line = new Tile[0];
        private Tile _stopTile;
        private Tile _targetTile;
        private int _maxDistance = 0;
        private bool _allowDiagonals;
        private bool _favorVertical;
        private bool _lastDragValue;
        private bool _directionAndDistanceMode;
        private bool _angleDirectionMode;
        private DemoType _demoType;
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
                GetLine();
            }
        }
        public float OpeningAngle
        {
            get
            {
                return _openingAngle;
            }

            set
            {
                _openingAngle = value;
                GetLine();
            }
        }
        public float DirectionAngle
        {
            get
            {
                return _directionAngle;
            }

            set
            {
                _directionAngle = value;
                GetLine();
            }
        }
        public bool AllowDiagonals
        {
            get
            {
                return _allowDiagonals;
            }

            set
            {
                _allowDiagonals = value;
                GetLine();
            }
        }
        public bool FavorVertical
        {
            get
            {
                return _favorVertical;
            }

            set
            {
                _favorVertical = value;
                GetLine();
            }
        }
        public int MaxDistance
        {
            get
            {
                return _maxDistance;
            }

            set
            {
                _maxDistance = value;
                GetLine();
            }
        }

        public bool DirectionAndDistanceMode
        {
            get
            {
                return _directionAndDistanceMode;
            }

            set
            {
                _directionAndDistanceMode = value;
                GetLine();
            }
        }
        public bool AngleDirectionMode
        {
            get
            {
                return _angleDirectionMode;
            }

            set
            {
                _angleDirectionMode = value;
                GetLine();
            }
        }

        public void SetDemoType(int demoType)
        {
            DemoType = (DemoType)demoType;
        }
        private void Awake()
        {
            _camera = Camera.main;
            Tile[] tiles = FindObjectsOfType<Tile>();
            foreach (Tile tile in tiles)
            {
                tile.X = Mathf.RoundToInt(tile.transform.position.x);
                tile.Y = Mathf.RoundToInt(tile.transform.position.y);
                _map[tile.Y, tile.X] = tile;
            }
            _targetTile = _map[4, 8];
            _targetTile.TileMode = TileMode.TARGET;
        }
        private void Update()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            bool tileIsHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
            Tile tile = tileIsHit ? hit.collider.GetComponent<Tile>() : null;

            if (tileIsHit)
            {
                bool refresh = !_directionAndDistanceMode || !_angleDirectionMode;
                // Middle click
                if (Input.GetMouseButton(2))
                {
                    if (tile != _targetTile && tile.IsWalkable)
                    {
                        if (refresh)
                        {
                            _stopTile = null;
                            ClearTiles();
                        }
                        SetStart(tile);
                        if (!refresh)
                        {
                            GetLine();
                        }
                    }
                }
                // Just clicked
                else if (Input.GetMouseButtonDown(0))
                {
                    if (tile == _targetTile)
                    {
                        return;
                    }
                    _lastDragValue = !tile.IsWalkable;
                    if (_lastDragValue)
                    {
                        ClearTiles();
                        tile.IsWalkable = _lastDragValue;
                        _stopTile = tile;
                        GetLine();
                    }
                    else
                    {
                        _stopTile = null;
                        ClearTiles();
                        tile.IsWalkable = _lastDragValue;
                    }
                    if (!refresh)
                    {
                        GetLine();
                    }
                }
                // Keep clicking
                else if (Input.GetMouseButton(0))
                {
                    if (tile.IsWalkable == _lastDragValue || tile == _targetTile)
                    {
                        return;
                    }
                    if (_lastDragValue)
                    {
                        ClearTiles();
                        tile.IsWalkable = _lastDragValue;
                        _stopTile = tile;
                        GetLine();
                    }
                    else
                    {
                        _stopTile = null;
                        ClearTiles();
                        tile.IsWalkable = _lastDragValue;
                    }
                    if (!refresh)
                    {
                        GetLine();
                    }
                }
                // Hovered without click
                else
                {
                    if (tile != _targetTile && tile.IsWalkable)
                    {
                        if (tile != _stopTile)
                        {
                            ClearTiles();
                            _stopTile = tile;
                            GetLine();
                        }
                    }
                    else if (!_directionAndDistanceMode || !_angleDirectionMode)
                    {
                        _stopTile = null;
                        ClearTiles();
                    }
                }
            }
            // No tile is hit by mouse
            else if (!_directionAndDistanceMode || !_angleDirectionMode)
            {
                _stopTile = null;
                ClearTiles();
            }
        }

        private void SetStart(Tile tile)
        {
            _targetTile.TileMode = TileMode.FLOOR;
            _targetTile = tile;
            _targetTile.TileMode = TileMode.TARGET;
        }
        private void GetLine()
        {
            if (_stopTile != null || (_directionAndDistanceMode && _angleDirectionMode))
            {

                ClearTiles();
                switch (_demoType)
                {
                    case DemoType.LINE_OF_TILES:
                        GetLineOfTiles();
                        break;
                    case DemoType.LINE_OF_SIGHT:
                        GetLineOfSight();
                        break;
                    case DemoType.CONE_OF_VISION:
                        GetConeOfVision();
                        break;
                    default:
                        break;
                }
            }
        }
        private void ClearTiles()
        {
            if (_line != null)
            {
                foreach (Tile tile in _line)
                {
                    if (tile != _targetTile)
                    {
                        tile.TileMode = TileMode.FLOOR;
                    }
                }
            }
            _line = new Tile[0];
            _losClearLED.color = Color.red;
        }
        private void GetLineOfTiles()
        {
            if (_directionAndDistanceMode)
            {
                if (_angleDirectionMode)
                {
                    _line = Raycasting.GetTilesOnALine(_map, _targetTile, _directionAngle, _maxDistance, _allowDiagonals, _favorVertical, false, true, false);
                }
                else
                {
                    Vector2 dir = new Vector2(_stopTile.X, _stopTile.Y) - new Vector2(_targetTile.X, _targetTile.Y);
                    _line = Raycasting.GetTilesOnALine(_map, _targetTile, dir, _maxDistance, _allowDiagonals, _favorVertical, false, true, false);
                }
            }
            else
            {
                _line = Raycasting.GetTilesOnALine(_map, _targetTile, _stopTile, _allowDiagonals, _favorVertical, false, true, false);
            }
            foreach (Tile tile in _line)
            {
                tile.TileMode = TileMode.LINE;
            }
        }
        private void GetLineOfSight()
        {
            bool isLosClear;
            if (_directionAndDistanceMode)
            {
                if (_angleDirectionMode)
                {
                    _line = Raycasting.GetLineOfSight(_map, out isLosClear, _targetTile, _directionAngle, _maxDistance, _allowDiagonals, _favorVertical, false, true);
                }
                else
                {
                    Vector2 dir = new Vector2(_stopTile.X, _stopTile.Y) - new Vector2(_targetTile.X, _targetTile.Y);
                    _line = Raycasting.GetLineOfSight(_map, out isLosClear, _targetTile, dir, _maxDistance, _allowDiagonals, _favorVertical, false, true);
                }
            }
            else
            {
                _line = Raycasting.GetLineOfSight(_map, out isLosClear, _targetTile, _stopTile, _allowDiagonals, _favorVertical, false, true);
            }
            foreach (Tile tile in _line)
            {
                tile.TileMode = TileMode.LINE;
            }
            _losClearLED.color = isLosClear ? Color.green : Color.red;
        }

        private void GetConeOfVision()
        {
            bool isCovClear;
            if (_directionAndDistanceMode)
            {
                if (_angleDirectionMode)
                {
                    _line = Raycasting.GetConeOfVision(_map, out isCovClear, _targetTile, _openingAngle, _directionAngle, (int)_maxDistance, false);
                }
                else
                {
                    Vector2 dir = new Vector2(_stopTile.X, _stopTile.Y) - new Vector2(_targetTile.X, _targetTile.Y);
                    _line = Raycasting.GetConeOfVision(_map, out isCovClear, _targetTile, _openingAngle, dir, (int)_maxDistance, false);
                }
            }
            else
            {
                _line = Raycasting.GetConeOfVision(_map, out isCovClear, _targetTile, _openingAngle, _stopTile, false);
            }
            foreach (Tile tile in _line)
            {
                tile.TileMode = TileMode.LINE;
            }
            _covClearLED.color = isCovClear ? Color.green : Color.red;
        }
    }
}