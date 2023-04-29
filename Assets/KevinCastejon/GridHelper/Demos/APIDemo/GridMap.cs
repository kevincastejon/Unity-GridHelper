using KevinCastejon.GridHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace APIDemo
{
    public enum DemoType
    {
        EXTRACTION_RADIUS,
        EXTRACTION_RADIUS_OUTLINE,
        EXTRACTION_RECTANGLE,
        EXTRACTION_RECTANGLE_OUTLINE,
        LINE_OF_TILES,
        LINE_OF_SIGHT,
        PATHFINDING_ACCESSIBLE,
        PATHFINDING_PATH_TO_TARGET,
    }
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private bool _allowDiagonals;
        [SerializeField] [Range(0f, 99f)] private float _maxPathDistance = 2f;
        [SerializeField] [Range(0f, 99f)] private float _maxLineDistance = 0f;
        [SerializeField] [Range(1, 99)] private int _radius = 2;
        [SerializeField] [Range(1, 99)] private int _rectangleSizeX = 2;
        [SerializeField] [Range(1, 99)] private int _rectangleSizeY = 2;
        [SerializeField] [Range(1f, 2f)] private float _diagonalsWeight = 1.5f;
        [SerializeField] private Slider _maxDistanceSliderLineOfSight;
        [SerializeField] private Slider _maxDistanceSliderPath;
        [SerializeField] private TextMeshProUGUI _maxDistanceLabelLineOfSight;
        [SerializeField] private TextMeshProUGUI _maxDistanceLabelPath;
        [SerializeField] private Toggle _allowDiagonalsToggle;
        [SerializeField] private Slider _diagonalsWeightSlider;
        [SerializeField] private TextMeshProUGUI _diagonalsWeightLabel;
        [SerializeField] private Slider _extractRadiusSlider;
        [SerializeField] private TextMeshProUGUI _extractRadiusLabel;
        [SerializeField] private Slider _extractSizeXSlider;
        [SerializeField] private TextMeshProUGUI _extractSizeXLabel;
        [SerializeField] private Slider _extractSizeYSlider;
        [SerializeField] private TextMeshProUGUI _extractSizeYLabel;
        [SerializeField] private Image _extractLED;

        private Camera _camera;
        private DemoType _demoType;
        private Tile[,] _map = new Tile[10, 12];
        private PathMap<Tile> _globalPathMap;
        private Tile _hoveredTile;
        private Tile _targetTile;
        private Tile _startTile;
        private Tile[] _pathTiles;
        private bool _lastDragTileWasWall;

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
                if (_demoType == DemoType.PATHFINDING_ACCESSIBLE || _demoType == DemoType.PATHFINDING_PATH_TO_TARGET)
                {
                    HideDistancesAndDirections();
                }
                _demoType = value;
                OnDemoTypeChange();
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
                if (_demoType == DemoType.PATHFINDING_ACCESSIBLE)
                {
                    CleanPathTiles();
                    GenerateGlobalPathMap();
                    ShowDistances();
                    GetAccessibleTiles();
                    SetTilesPath();
                }
                else if (_demoType == DemoType.PATHFINDING_PATH_TO_TARGET)
                {
                    CleanPathTiles();
                    GenerateGlobalPathMap();
                    ShowDirections();
                    GetPathToTarget();
                    SetTilesPath();
                }

            }
        }
        public float MaxPathDistance
        {
            get
            {
                return _maxPathDistance;
            }

            set
            {
                _maxPathDistance = value;
                if (_demoType == DemoType.PATHFINDING_ACCESSIBLE)
                {
                    CleanPathTiles();
                    GenerateGlobalPathMap();
                    ShowDistances();
                    GetAccessibleTiles();
                    SetTilesPath();
                }
                else if (_demoType == DemoType.PATHFINDING_PATH_TO_TARGET)
                {
                    CleanPathTiles();
                    GenerateGlobalPathMap();
                    ShowDirections();
                    GetPathToTarget();
                    SetTilesPath();
                }
            }
        }
        public float MaxLineDistance
        {
            get
            {
                return _maxLineDistance;
            }

            set
            {
                _maxLineDistance = value;
                if (_demoType == DemoType.LINE_OF_TILES)
                {
                    CleanPathTiles();
                    GetWalkableTilesOnALine();
                    SetTilesPath();
                }
                else if (_demoType == DemoType.LINE_OF_SIGHT)
                {
                    CleanPathTiles();
                    GetLineOfSight();
                    SetTilesPath();
                }

            }
        }
        public int Radius
        {
            get
            {
                return _radius;
            }

            set
            {
                _radius = value;
                if (_demoType == DemoType.EXTRACTION_RADIUS)
                {
                    CleanPathTiles();
                    GetWalkableTilesInARadius();
                    SetTilesPath();
                }
                else if (_demoType == DemoType.EXTRACTION_RADIUS_OUTLINE)
                {
                    CleanPathTiles();
                    GetWalkableTilesInARadiusOutline();
                    SetTilesPath();
                }
            }
        }
        public int RectangleSizeX
        {
            get
            {
                return _rectangleSizeX;
            }
            set
            {
                _rectangleSizeX = value;
                if (_demoType == DemoType.EXTRACTION_RECTANGLE)
                {
                    CleanPathTiles();
                    GetWalkableTilesInARectangle();
                    SetTilesPath();
                }
                else if (_demoType == DemoType.EXTRACTION_RECTANGLE_OUTLINE)
                {
                    CleanPathTiles();
                    GetWalkableTilesInARectangleOutline();
                    SetTilesPath();
                }
            }
        }
        public int RectangleSizeY
        {
            get
            {
                return _rectangleSizeY;
            }

            set
            {
                _rectangleSizeY = value;
                if (_demoType == DemoType.EXTRACTION_RECTANGLE)
                {
                    CleanPathTiles();
                    GetWalkableTilesInARectangle();
                    SetTilesPath();
                }
                else if (_demoType == DemoType.EXTRACTION_RECTANGLE_OUTLINE)
                {
                    CleanPathTiles();
                    GetWalkableTilesInARectangleOutline();
                    SetTilesPath();
                }
            }
        }
        public float DiagonalsWeight
        {
            get
            {
                return _diagonalsWeight;
            }

            set
            {
                _diagonalsWeight = value;
                if (_demoType == DemoType.PATHFINDING_ACCESSIBLE)
                {
                    CleanPathTiles();
                    GenerateGlobalPathMap();
                    ShowDistances();
                    GetAccessibleTiles();
                    SetTilesPath();
                }
                else if (_demoType == DemoType.PATHFINDING_PATH_TO_TARGET)
                {
                    CleanPathTiles();
                    GenerateGlobalPathMap();
                    ShowDirections();
                    GetPathToTarget();
                    SetTilesPath();
                }
            }
        }

        private void Awake()
        {
            Tile[] tiles = FindObjectsOfType<Tile>();
            foreach (Tile tile in tiles)
            {
                tile.X = Mathf.RoundToInt(tile.transform.position.x);
                tile.Y = Mathf.RoundToInt(tile.transform.position.y);
                _map[tile.Y, tile.X] = tile;
                if (_map[tile.Y, tile.X].TileState == TileState.TARGET)
                {
                    _targetTile = _map[tile.Y, tile.X];
                }
            }
            // Doing some UI init
            _maxDistanceSliderLineOfSight.value = _maxLineDistance;
            _maxDistanceSliderPath.value = _maxPathDistance;
            _maxDistanceLabelLineOfSight.text = _maxLineDistance.ToString();
            _maxDistanceLabelPath.text = _maxPathDistance.ToString();
            _allowDiagonalsToggle.isOn = _allowDiagonals;
            _diagonalsWeightSlider.interactable = _allowDiagonals;
            _diagonalsWeightSlider.value = _diagonalsWeight;
            _diagonalsWeightLabel.text = _diagonalsWeight.ToString("F1");
            _extractRadiusSlider.value = _radius;
            _extractRadiusLabel.text = _radius.ToString();
            _extractSizeXSlider.value = _rectangleSizeX;
            _extractSizeXLabel.text = _rectangleSizeX.ToString();
            _extractSizeYSlider.value = _rectangleSizeY;
            _extractSizeYLabel.text = _rectangleSizeY.ToString();
            _camera = Camera.main;
        }
        private void Start()
        {
            OnDemoTypeChange();
        }
        private void Update()
        {
            // Detecting click on tile
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                // Retrieving the Tile component
                Tile tile = hit.collider.GetComponent<Tile>();

                // If middle-click
                if (Input.GetMouseButton(2))
                {
                    SetTarget(tile);
                }
                // Or if left-click just happenned
                else if (Input.GetMouseButtonDown(0))
                {
                    OnTileClicked(tile);
                }
                // Or if left click is maintained
                else if (Input.GetMouseButton(0))
                {
                    if (tile != _hoveredTile)
                    {
                        OnTileDragEntered(tile);
                    }
                }
                // Or if the tile is just hovered
                else
                {
                    if (tile != _hoveredTile)
                    {
                        OnTileEntered(tile);
                    }
                }
                _hoveredTile = tile;
            }
            else
            {
                _extractLED.color = Color.white;
                if (_demoType != DemoType.EXTRACTION_RADIUS && _demoType != DemoType.EXTRACTION_RADIUS_OUTLINE && _demoType != DemoType.EXTRACTION_RECTANGLE && _demoType != DemoType.EXTRACTION_RECTANGLE_OUTLINE && _demoType != DemoType.PATHFINDING_ACCESSIBLE)
                {
                    _hoveredTile = null;
                    _startTile = null;
                    CleanPathTiles();
                }
            }
        }

        private void OnDemoTypeChange()
        {
            _startTile = null;
            CleanPathTiles();
            switch (_demoType)
            {
                case DemoType.EXTRACTION_RADIUS:
                    GetWalkableTilesInARadius();
                    break;
                case DemoType.EXTRACTION_RADIUS_OUTLINE:
                    GetWalkableTilesInARadiusOutline();
                    break;
                case DemoType.EXTRACTION_RECTANGLE:
                    GetWalkableTilesInARectangle();
                    break;
                case DemoType.EXTRACTION_RECTANGLE_OUTLINE:
                    GetWalkableTilesInARectangleOutline();
                    break;
                case DemoType.LINE_OF_TILES:
                    GetWalkableTilesOnALine();
                    break;
                case DemoType.LINE_OF_SIGHT:
                    GetLineOfSight();
                    break;
                case DemoType.PATHFINDING_ACCESSIBLE:
                    GenerateGlobalPathMap();
                    ShowDistances();
                    GetAccessibleTiles();
                    break;
                case DemoType.PATHFINDING_PATH_TO_TARGET:
                    GenerateGlobalPathMap();
                    ShowDirections();
                    GetPathToTarget();
                    break;
                default:
                    break;
            }
            SetTilesPath();
        }
        private void OnTargetChange()
        {
            _startTile = null;
            CleanPathTiles();
            switch (_demoType)
            {
                case DemoType.EXTRACTION_RADIUS:
                    GetWalkableTilesInARadius();
                    break;
                case DemoType.EXTRACTION_RADIUS_OUTLINE:
                    GetWalkableTilesInARadiusOutline();
                    break;
                case DemoType.EXTRACTION_RECTANGLE:
                    GetWalkableTilesInARectangle();
                    break;
                case DemoType.EXTRACTION_RECTANGLE_OUTLINE:
                    GetWalkableTilesInARectangleOutline();
                    break;
                case DemoType.LINE_OF_TILES:
                    GetWalkableTilesOnALine();
                    break;
                case DemoType.LINE_OF_SIGHT:
                    GetLineOfSight();
                    break;
                case DemoType.PATHFINDING_ACCESSIBLE:
                    HideDistancesAndDirections();
                    GenerateGlobalPathMap();
                    ShowDistances();
                    GetAccessibleTiles();
                    break;
                case DemoType.PATHFINDING_PATH_TO_TARGET:
                    HideDistancesAndDirections();
                    GenerateGlobalPathMap();
                    ShowDirections();
                    GetPathToTarget();
                    break;
                default:
                    break;
            }
            SetTilesPath();
        }
        private void OnStartChange()
        {
            switch (_demoType)
            {
                case DemoType.EXTRACTION_RADIUS:
                    break;
                case DemoType.EXTRACTION_RADIUS_OUTLINE:
                    break;
                case DemoType.EXTRACTION_RECTANGLE:
                    break;
                case DemoType.EXTRACTION_RECTANGLE_OUTLINE:
                    break;
                case DemoType.LINE_OF_TILES:
                    CleanPathTiles();
                    GetWalkableTilesOnALine();
                    SetTilesPath();
                    break;
                case DemoType.LINE_OF_SIGHT:
                    CleanPathTiles();
                    GetLineOfSight();
                    SetTilesPath();
                    break;
                case DemoType.PATHFINDING_ACCESSIBLE:
                    CleanPathTiles();
                    GetAccessibleTiles();
                    SetTilesPath();
                    break;
                case DemoType.PATHFINDING_PATH_TO_TARGET:
                    CleanPathTiles();
                    GetPathToTarget();
                    SetTilesPath();
                    break;
                default:
                    break;
            }
        }
        private void OnWallChange()
        {
            CleanPathTiles();
            switch (_demoType)
            {
                case DemoType.EXTRACTION_RADIUS:
                    GetWalkableTilesInARadius();
                    break;
                case DemoType.EXTRACTION_RADIUS_OUTLINE:
                    GetWalkableTilesInARadiusOutline();
                    break;
                case DemoType.EXTRACTION_RECTANGLE:
                    GetWalkableTilesInARectangle();
                    break;
                case DemoType.EXTRACTION_RECTANGLE_OUTLINE:
                    GetWalkableTilesInARectangleOutline();
                    break;
                case DemoType.LINE_OF_TILES:
                    GetWalkableTilesOnALine();
                    break;
                case DemoType.LINE_OF_SIGHT:
                    GetLineOfSight();
                    break;
                case DemoType.PATHFINDING_ACCESSIBLE:
                    HideDistancesAndDirections();
                    GenerateGlobalPathMap();
                    ShowDistances();
                    GetAccessibleTiles();
                    break;
                case DemoType.PATHFINDING_PATH_TO_TARGET:
                    HideDistancesAndDirections();
                    GenerateGlobalPathMap();
                    ShowDirections();
                    GetPathToTarget();
                    break;
                default:
                    break;
            }
            SetTilesPath();
        }
        private void SetTarget(Tile tile)
        {
            if (!tile.IsWalkable || tile == _targetTile)
            {
                return;
            }
            _targetTile.TileState = TileState.FLOOR;
            _targetTile = tile;
            _targetTile.TileState = TileState.TARGET;
            OnTargetChange();
        }
        private void OnTileEntered(Tile hoveredTile)
        {
            switch (_demoType)
            {
                case DemoType.EXTRACTION_RADIUS:
                    _extractLED.color = Extraction.IsTileInARadius(_targetTile, hoveredTile, _radius) ? Color.green : Color.red;
                    break;
                case DemoType.EXTRACTION_RADIUS_OUTLINE:
                    _extractLED.color = Extraction.IsTileOnARadiusOutline(_targetTile, hoveredTile, _radius) ? Color.green : Color.red;
                    break;
                case DemoType.EXTRACTION_RECTANGLE:
                    _extractLED.color = Extraction.IsTileInARectangle(_targetTile, hoveredTile, _rectangleSizeX, _rectangleSizeY) ? Color.green : Color.red;
                    break;
                case DemoType.EXTRACTION_RECTANGLE_OUTLINE:
                    _extractLED.color = Extraction.IsTileOnARectangleOutline(_targetTile, hoveredTile, _rectangleSizeX, _rectangleSizeY) ? Color.green : Color.red;
                    break;
                default:
                    break;
            }
            if (hoveredTile == _startTile || (_demoType == DemoType.PATHFINDING_PATH_TO_TARGET && !hoveredTile.IsWalkable))
            {
                return;
            }
            _startTile = hoveredTile;
            OnStartChange();
        }
        private void OnTileClicked(Tile clickedTile)
        {
            if (clickedTile.TileState == TileState.TARGET)
            {
                return;
            }
            if (_demoType != DemoType.LINE_OF_SIGHT && _demoType != DemoType.LINE_OF_TILES && clickedTile == _startTile)
            {
                _startTile = null;
            }
            clickedTile.TileState = clickedTile.TileState == TileState.WALL ? TileState.FLOOR : TileState.WALL;
            _lastDragTileWasWall = clickedTile.TileState == TileState.WALL;
            if (clickedTile.TileState != TileState.WALL)
            {
                _startTile = clickedTile;
            }
            OnWallChange();
        }
        private void OnTileDragEntered(Tile clickedTile)
        {
            if (clickedTile.TileState == TileState.TARGET || (_lastDragTileWasWall && clickedTile.TileState == TileState.WALL) || (!_lastDragTileWasWall && (clickedTile.TileState == TileState.FLOOR || clickedTile.TileState == TileState.PATH)))
            {
                return;
            }
            if (clickedTile == _startTile)
            {
                _startTile = null;
            }
            clickedTile.TileState = clickedTile.TileState == TileState.WALL ? TileState.FLOOR : TileState.WALL;
            OnWallChange();
        }
        private void GetWalkableTilesInARadius()
        {
            _pathTiles = Extraction.GetWalkableTilesInARadius(_map, _targetTile, _radius);
        }
        private void GetWalkableTilesInARadiusOutline()
        {
            _pathTiles = Extraction.GetWalkableTilesOnARadiusOutline(_map, _targetTile, _radius);
        }
        private void GetWalkableTilesInARectangle()
        {
            _pathTiles = Extraction.GetWalkableTilesInARectangle(_map, _targetTile, _rectangleSizeX, _rectangleSizeY);
        }
        private void GetWalkableTilesInARectangleOutline()
        {
            _pathTiles = Extraction.GetWalkableTilesOnARectangleOutline(_map, _targetTile, _rectangleSizeX, _rectangleSizeY);
        }
        private void GetWalkableTilesOnALine()
        {
            if (_startTile == null)
            {
                return;
            }
            _pathTiles = Raycasting.GetWalkableTilesOnALine(_map, _targetTile, _startTile, _maxLineDistance);
        }
        private void GetLineOfSight()
        {
            if (_startTile == null)
            {
                return;
            }
            _pathTiles = Raycasting.GetLineOfSight(_map, _targetTile, _startTile, _maxLineDistance);
        }
        private void GetAccessibleTiles()
        {
            PathMap<Tile> pathMap = Pathfinding.GeneratePathMap(_map, _targetTile, _maxPathDistance, _allowDiagonals, _diagonalsWeight);
            _pathTiles = pathMap.GetAccessibleTiles();
        }
        private void GetPathToTarget()
        {
            if (!_globalPathMap.IsTileAccessible(_startTile))
            {
                return;
            }
            _pathTiles = _globalPathMap.GetPathToTarget(_startTile);
        }
        private void SetTilesPath()
        {
            if (_pathTiles == null)
            {
                return;
            }
            foreach (Tile tile in _pathTiles)
            {
                if (tile.IsTarget)
                {
                    continue;
                }
                tile.TileState = TileState.PATH;
            }
        }
        private void CleanPathTiles()
        {
            if (_pathTiles == null)
            {
                return;
            }
            foreach (Tile tile in _pathTiles)
            {
                if (tile.IsTarget || tile.TileState != TileState.PATH)
                {
                    continue;
                }
                tile.TileState = TileState.FLOOR;
            }
            _pathTiles = null;
        }
        public void SetType(int demoType)
        {
            DemoType = (DemoType)demoType;
        }
        private void GenerateGlobalPathMap()
        {
            // Generating a path map
            _globalPathMap = Pathfinding.GeneratePathMap(_map, _targetTile, 0f, _allowDiagonals, _diagonalsWeight);
        }
        private void ShowDistances()
        {
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    if (!_globalPathMap.IsTileAccessible(_map[i, j]))
                    {
                        continue;
                    }
                    _map[i, j].ShowDistance(_globalPathMap.GetDistanceToTargetFromTile(_map[i, j]));
                }
            }
        }
        private void ShowDirections()
        {
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    if (!_globalPathMap.IsTileAccessible(_map[i, j]))
                    {
                        continue;
                    }
                    _map[i, j].ShowDirection(_globalPathMap.GetNextTileDirectionFromTile(_map[i, j]));
                }
            }
        }
        private void HideDistancesAndDirections()
        {
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    _map[i, j].HideDistanceAndDirection();
                }
            }
        }
    }
}
