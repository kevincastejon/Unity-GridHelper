using KevinCastejon.GridHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Technical_Demo
{
    public enum DemoType
    {
        EXTRACT_CIRCLE,
        EXTRACT_RECTANGLE,
        LINE_OF_SIGHT,
        LINE_OF_TILES,
        PATHFINDING_ACCESSIBLE,
        PATHFINDING_PATH,
    }
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private bool _allowDiagonals;
        [SerializeField] private bool _outline;
        [SerializeField] [Range(0f, 99f)] private float _maxMovement = 2;
        [SerializeField] [Range(1, 99)] private int _circleSize = 2;
        [SerializeField] [Range(1, 99)] private int _rectangleSizeX = 2;
        [SerializeField] [Range(1, 99)] private int _rectangleSizeY = 2;
        [SerializeField] [Range(1f, 2f)] private float _diagonalsWeight = 1.5f;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Material _pathMat;
        [SerializeField] private Floor _target;
        [SerializeField] private Slider _maxMovementSlider;
        [SerializeField] private TextMeshProUGUI _maxMovementLabel;
        [SerializeField] private Toggle _allowDiagonalsToggle;
        [SerializeField] private Toggle _outlineToggle;
        [SerializeField] private Slider _diagonalsWeightSlider;
        [SerializeField] private TextMeshProUGUI _diagonalsWeightLabel;
        [SerializeField] private Slider _extractRadiusSlider;
        [SerializeField] private TextMeshProUGUI _extractRadiusLabel;
        [SerializeField] private Slider _extractSizeXSlider;
        [SerializeField] private TextMeshProUGUI _extractSizeXLabel;
        [SerializeField] private Slider _extractSizeYSlider;
        [SerializeField] private TextMeshProUGUI _extractSizeYLabel;

        private Floor[,] _map = new Floor[12, 10];
        private Floor _pathStart;
        private PathMap<Floor> _pathMap;
        private Camera _camera;
        private DemoType _demoType;
        private bool _firstDragValue;
        private bool _showingDistances;
        private bool _showingDirections;

        public Floor[,] Map { get => _map; }
        public DemoType DemoType
        {
            get
            {
                return _demoType;
            }
            set
            {
                _demoType = value;
                RefreshVisuals();
            }
        }
        public float MaxMovement
        {
            get => _maxMovement;
            set
            {
                _maxMovement = value;
                ShowAccessibleTiles();
            }
        }
        public int CircleSize
        {
            get => _circleSize;
            set
            {
                _circleSize = value;
                ShowTilesIntoRadius();
            }
        }
        public bool Outline
        {
            get => _outline;
            set
            {
                _outline = value;
                if (_demoType == DemoType.EXTRACT_CIRCLE)
                {
                    ShowTilesIntoRadius();
                }
                else if (_demoType == DemoType.EXTRACT_RECTANGLE)
                {
                    ShowTilesIntoRectangle();
                }
            }
        }
        public int RectangleSizeX
        {
            get => _rectangleSizeX;
            set
            {
                _rectangleSizeX = value;
                ShowTilesIntoRectangle();
            }
        }
        public int RectangleSizeY
        {
            get => _rectangleSizeY;
            set
            {
                _rectangleSizeY = value;
                ShowTilesIntoRectangle();
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
                GeneratePathMap();
                RefreshVisuals();
            }
        }
        public float DiagonalsWeight
        {
            get => _diagonalsWeight;
            set
            {
                _diagonalsWeight = value;
                GeneratePathMap();
                RefreshVisuals();
            }
        }

        public void Awake()
        {
            // Referencing tiles a dirty way
            foreach (Transform child in transform)
            {
                int x = Mathf.RoundToInt(child.position.x);
                int y = Mathf.Abs(Mathf.RoundToInt(child.position.z));
                _map[y, x] = child.GetComponent<Floor>();
                _map[y, x].X = x;
                _map[y, x].Y = y;
            }
            // Doing some UI init
            _maxMovementSlider.value = _maxMovement;
            _maxMovementLabel.text = _maxMovement.ToString();
            _allowDiagonalsToggle.isOn = _allowDiagonals;
            _outlineToggle.isOn = _outline;
            _diagonalsWeightSlider.interactable = _allowDiagonals;
            _diagonalsWeightSlider.value = _diagonalsWeight;
            _diagonalsWeightLabel.text = _diagonalsWeight.ToString("F1");
            _extractRadiusSlider.value = _circleSize;
            _extractRadiusLabel.text = _circleSize.ToString();
            _extractSizeXSlider.value = _rectangleSizeX;
            _extractSizeXLabel.text = _rectangleSizeX.ToString();
            _extractSizeYSlider.value = _rectangleSizeY;
            _extractSizeYLabel.text = _rectangleSizeY.ToString();
        }
        private void Start()
        {
            // Referencing the camera
            _camera = Camera.main;
            // Generating a path map
            GeneratePathMap();
            // Displaying the accessible tiles
            ShowTilesIntoRadius();
        }
        private void Update()
        {
            // Detecting click on tile
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                // Retrieving the Floor component
                Floor clickedFloor = hit.collider.GetComponent<Floor>();
                switch (_demoType)
                {
                    case DemoType.EXTRACT_CIRCLE:
                        break;
                    case DemoType.EXTRACT_RECTANGLE:
                        break;
                    case DemoType.LINE_OF_SIGHT:
                        SetStartTile(clickedFloor);
                        RefreshVisuals();
                        break;
                    case DemoType.LINE_OF_TILES:
                        SetStartTile(clickedFloor);
                        RefreshVisuals();
                        break;
                    case DemoType.PATHFINDING_ACCESSIBLE:
                        break;
                    case DemoType.PATHFINDING_PATH:
                        SetStartTile(clickedFloor);
                        RefreshVisuals();
                        break;
                    default:
                        break;
                }
                // If middle-click
                if (Input.GetMouseButton(2))
                {
                    // Set the target
                    SetTargetTile(clickedFloor);
                }
                // Or if left-click just happenned
                else if (Input.GetMouseButtonDown(0))
                {
                    ChangeWallStateClick(clickedFloor);
                    RefreshVisuals();
                }
                // Or if left click is maintained
                else if (Input.GetMouseButton(0))
                {
                    ChangeWallStateDrag(clickedFloor);
                    RefreshVisuals();
                }
            }
        }

        // Set the current display mode
        public void SetMode(int demoType)
        {
            DemoType = (DemoType)demoType;
        }
        // Refresh the displays
        private void RefreshVisuals()
        {
            switch (_demoType)
            {
                case DemoType.EXTRACT_CIRCLE:
                    // Displaying the tiles into radius
                    ShowTilesIntoRadius();
                    break;
                case DemoType.EXTRACT_RECTANGLE:
                    // Displaying the tiles into radius
                    ShowTilesIntoRectangle();
                    break;
                case DemoType.LINE_OF_SIGHT:
                    // Displaying the line of sight
                    ShowLineOfSight();
                    break;
                case DemoType.LINE_OF_TILES:
                    // Displaying the line of tiles
                    ShowLineOfTiles();
                    break;
                case DemoType.PATHFINDING_ACCESSIBLE:
                    // Displaying the accessible tiles
                    ShowAccessibleTiles();
                    break;
                case DemoType.PATHFINDING_PATH:
                    // Displaying the path to target
                    ShowPathToTarget();
                    break;
                default:
                    break;
            }
            if (_showingDistances)
            {
                ShowDistances();
            }
            else if (_showingDirections)
            {
                ShowDirections();
            }
        }
        // Displays the line of tiles
        private void ShowLineOfTiles()
        {
            // Resetting all tiles
            ResetPaths();
            // If there is no start tile exit immediatelly
            if (_pathStart == null)
            {
                return;
            }
            // Retrieving the line of tiles
            Floor[] lineOfTiles = GridHelper.GetTilesOnALine(_map, _target, _pathStart);
            // For each tile along the line
            foreach (Floor floor in lineOfTiles)
            {
                // Set it as a path
                floor.IsPath = true;
            }
        }
        // Displays the line of sight
        private void ShowLineOfSight()
        {
            // Resetting all tiles
            ResetPaths();
            // If there is no start tile exit immediatelly
            if (_pathStart == null)
            {
                return;
            }
            // Retrieving the line of sight
            Floor[] lineOfSight = GridHelper.GetLineOfSight(_map, _target, _pathStart);
            // For each tile along the line of sight
            foreach (Floor floor in lineOfSight)
            {
                // Set it as a path
                floor.IsPath = true;
            }
        }
        // Displays the tiles into a square
        private void ShowTilesIntoRectangle()
        {
            // Resetting all tiles
            ResetPaths();
            // Retrieving the path
            Floor[] tilesIntoSquare = _outline ? GridHelper.GetTilesOnARectangleOutline(_map, _target, _rectangleSizeX, _rectangleSizeY) : GridHelper.GetTilesIntoARectangle(_map, _target, _rectangleSizeX, _rectangleSizeY);
            // For each tile along the path
            foreach (Floor floor in tilesIntoSquare)
            {
                // Set it as a path
                floor.IsPath = true;
            }
        }
        // Displays the tiles into a radius
        private void ShowTilesIntoRadius()
        {
            // Resetting all tiles
            ResetPaths();
            // Retrieving the path
            Floor[] tilesIntoRadius = _outline ? GridHelper.GetTilesOnARadiusOutline(_map, _target, _circleSize) : GridHelper.GetTilesIntoARadius(_map, _target, _circleSize);
            // For each tile along the path
            foreach (Floor floor in tilesIntoRadius)
            {
                // Set it as a path
                floor.IsPath = true;
            }
        }
        // Displays the accessible tiles from target
        private void ShowAccessibleTiles()
        {
            // Reseting all tiles visuals
            ResetPaths();
            // Retrieving the accessible tiles from the pathMap
            Floor[] accessibleTiles = _pathMap.GetAccessibleTilesFromTarget((float)_maxMovement);
            // For each accessible tile
            foreach (Floor floor in accessibleTiles)
            {
                // Set it as a path (visual)
                floor.IsPath = true;
            }
        }
        // Displays the path between start and target
        private void ShowPathToTarget()
        {
            // Reseting all tiles
            ResetPaths();
            // If there is no start tile exit immediatelly
            if (_pathStart == null)
            {
                return;
            }
            // Retrieving the path
            Floor[] pathToTarget = _pathMap.GetPathToTarget(_pathStart);
            // For each tile along the path
            foreach (Floor floor in pathToTarget)
            {
                // Set it as a path
                floor.IsPath = true;
            }
        }
        // Handle wall state changing on click
        private void ChangeWallStateClick(Floor clickedFloor)
        {
            // If this tile is not the target
            if (!clickedFloor.IsTarget)
            {
                // Inverting the walkable state
                clickedFloor.IsWalkable = !clickedFloor.IsWalkable;
                // Setting this value as the "drag value" for next tiles hovering
                _firstDragValue = clickedFloor.IsWalkable;
                // Generating a path map
                GeneratePathMap();
            }
        }
        // Handle wall state changing on drag
        private void ChangeWallStateDrag(Floor clickedFloor)
        {
            // If this tile is not the target and has not already the same walkable state
            if (!clickedFloor.IsTarget && clickedFloor.IsWalkable != _firstDragValue)
            {
                clickedFloor.IsWalkable = !clickedFloor.IsWalkable;
                // Generating a path map
                GeneratePathMap();

            }
        }
        // Set a tile as the start
        private void SetStartTile(Floor clickedFloor)
        {
            // If that tile is not already the start tile and is walkable
            if (clickedFloor != _pathStart && clickedFloor.IsWalkable)
            {
                // Setting this tile as the start 
                _pathStart = clickedFloor;
            }
        }
        // Set a tile as the target
        private void SetTargetTile(Floor clickedFloor)
        {
            // Checking that this floor is not already the target one and is walkable
            if (clickedFloor != _target && clickedFloor.IsWalkable)
            {
                // Unsetting the actual target
                _target.IsTarget = false;
                // Setting the new target
                _target = clickedFloor;
                _target.IsTarget = true;
                // Generating a path map
                GeneratePathMap();
                // Refreshing visuals
                RefreshVisuals();
            }
        }        
        // Resets the Dijkstra visuals
        public void ResetDijkstraVisuals()
        {
            _showingDistances = false;
            _showingDirections = false;
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    _map[i, j].Label.text = "";
                    _map[i, j].Label.rectTransform.parent.rotation = Quaternion.LookRotation(Vector3.right);
                }
            }
        }
        
        // Displays the distances on the tiles
        public void ShowDistances()
        {
            _showingDistances = true;
            _showingDirections = false;
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    if (_map[i, j] == null)
                    {
                        continue;
                    }
                    if (!_map[i, j].IsWalkable)
                    {
                        _map[i, j].Label.text = "";
                        _map[i, j].Label.rectTransform.parent.rotation = Quaternion.LookRotation(Vector3.right);
                        continue;
                    }
                    _map[i, j].Label.rectTransform.parent.rotation = Quaternion.LookRotation(Vector3.right);
                    _map[i, j].Label.text = _pathMap.GetMovementCostFromTile(_map[i, j]).ToString("F1");
                }
            }
        }
        // Displays the directions on the tiles
        public void ShowDirections()
        {
            _showingDirections = true;
            _showingDistances = false;
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    if (_map[i, j] == null)
                    {
                        continue;
                    }
                    if (!_map[i, j].IsWalkable || _pathMap.GetNextTileFromTile(_map[i, j]) == _map[i, j])
                    {
                        _map[i, j].Label.text = "";
                        _map[i, j].Label.rectTransform.parent.rotation = Quaternion.LookRotation(Vector3.right);
                        continue;
                    }
                    Vector3 pos = _pathMap.GetNextTileFromTile(_map[i, j]).transform.position;
                    Vector3 dir = pos - _map[i, j].transform.position;
                    Quaternion rot = Quaternion.LookRotation(dir);
                    _map[i, j].Label.text = '\u2192'.ToString();
                    _map[i, j].Label.rectTransform.parent.rotation = rot;
                }
            }
        }
        // Generates a PathMap
        private void GeneratePathMap()
        {
            // Generating a path map
            _pathMap = GridHelper.GeneratePathMap(_map, _target, _allowDiagonals, _diagonalsWeight);
        }
        // Resets the tiles path visuals
        private void ResetPaths()
        {
            // For each tile
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    // Setting path false (visual)
                    if (_map[i, j])
                    {
                        _map[i, j].IsPath = false;
                    }
                }
            }
        }
    }
}
