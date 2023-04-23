using KevinCastejon.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Technical_Demo
{
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private bool _allowDiagonals;
        [SerializeField] [Range(0, 99)] private int _maxMovement;
        [SerializeField] [Range(1f, 2f)] private float _diagonalsWeight = 1.5f;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Material _pathMat;
        [SerializeField] private Floor _target;
        [SerializeField] private Slider _maxMovementSlider;
        [SerializeField] private TextMeshProUGUI _maxMovementLabel;
        [SerializeField] private Toggle _allowDiagonalsToggle;
        [SerializeField] private Slider _diagonalsWeightSlider;
        [SerializeField] private TextMeshProUGUI _diagonalsWeightLabel;

        private Floor[,] _map = new Floor[12, 10];
        private Floor _pathStart;
        private PathMap<Floor> _pathMap;
        private Camera _camera;
        private bool _isPathfindingMode;
        private bool _firstDragValue;

        public Floor[,] Map { get => _map; }
        public bool IsPathfindingMode
        {
            get
            {
                return _isPathfindingMode;
            }
            set
            {
                _isPathfindingMode = value;
                if (_isPathfindingMode)
                {
                    ShowPathToTarget();
                }
                else
                {
                    ShowAccessibleTiles();
                }
            }
        }
        public int MaxMovement
        {
            get => _maxMovement;
            set
            {
                _maxMovement = value;
                ShowAccessibleTiles();
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
                if (_isPathfindingMode)
                {
                    ShowPathToTarget();
                }
                else
                {
                    ShowAccessibleTiles();
                }
            }
        }
        public float DiagonalsWeight
        {
            get => _diagonalsWeight;
            set
            {
                _diagonalsWeight = value;
                GeneratePathMap();
                if (_isPathfindingMode)
                {
                    ShowPathToTarget();
                }
                else
                {
                    ShowAccessibleTiles();
                }
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
            _diagonalsWeightSlider.interactable = _allowDiagonals;
            _diagonalsWeightSlider.value = _diagonalsWeight;
            _diagonalsWeightLabel.text = _diagonalsWeight.ToString("F1");
        }
        private void Start()
        {
            // Referencing the camera
            _camera = Camera.main;
            // Generating a path map
            GeneratePathMap();
            // Displaying the accessible tiles
            ShowAccessibleTiles();
        }
        private void Update()
        {
            // Detecting click on tile
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                // Retrieving the Floor component
                Floor clickedFloor = hit.collider.GetComponent<Floor>();
                // If middle-click (setting the target)
                if (Input.GetMouseButton(2))
                {
                    // Checking that this floor is not already the target one
                    if (clickedFloor != _target && clickedFloor.IsWalkable)
                    {
                        // Unsetting the actual target
                        _target.IsTarget = false;
                        // Setting the new target
                        _target = clickedFloor;
                        _target.IsTarget = true;
                        // Generating a path map
                        GeneratePathMap();
                        // Checking the current mode
                        if (_isPathfindingMode)
                        {
                            // Displaying the path to target
                            ShowPathToTarget();
                        }
                        else
                        {
                            // Displaying the accessible tiles
                            ShowAccessibleTiles();
                        }
                    }
                }
                // If left-click just happenned
                else if (Input.GetMouseButtonDown(0))
                {
                    // If Pathfinding mode
                    if (_isPathfindingMode)
                    {
                        // If that tile is walkable
                        if (clickedFloor.IsWalkable)
                        {
                            // Setting this tile as the start 
                            _pathStart = clickedFloor;
                            // Displaying the path to target
                            ShowPathToTarget();
                        }
                    }
                    // If Accessible Tiles mode
                    else
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
                            // Displaying the accessible tiles
                            ShowAccessibleTiles();
                        }
                    }
                }
                //If left click is maintained
                else if (Input.GetMouseButton(0))
                {
                    // If Pathfinding mode
                    if (_isPathfindingMode)
                    {
                        // If that tile is walkable and not the already the starting one
                        if (clickedFloor.IsWalkable && _pathStart != clickedFloor)
                        {
                            // Setting this tile as start
                            _pathStart = clickedFloor;
                            // Displaying the path to target
                            ShowPathToTarget();
                        }
                    }
                    // If Accessible Tiles mode
                    else
                    {
                        // If this tile is not the target and has not already the same walkable state
                        if (!clickedFloor.IsTarget && clickedFloor.IsWalkable != _firstDragValue)
                        {
                            clickedFloor.IsWalkable = !clickedFloor.IsWalkable;
                            // Generating a path map
                            GeneratePathMap();
                            // Displaying the accessible tiles
                            ShowAccessibleTiles();
                        }
                    }
                }
            }
        }
        // Generates a PathMap
        private void GeneratePathMap()
        {
            // Generating a path map
            _pathMap = PathFinder.GeneratePathMap(_map, _target, _allowDiagonals, _diagonalsWeight);
        }
        // Display the accessible tiles from target
        private void ShowAccessibleTiles()
        {
            // Reseting all tiles visuals
            ResetPaths();
            // Retrieving the accessible tiles from the pathMap
            Floor[] accessibleTiles = _pathMap.GetAccessibleTilesFromTarget(_maxMovement);
            // For each accessible tile
            foreach (Floor floor in accessibleTiles)
            {
                // Set it as a path (visual)
                floor.IsPath = true;
            }
        }
        // Display the path between start and target
        private void ShowPathToTarget()
        {
            // Reseting all tiles
            ResetPaths();
            // If there is no start tile exit immediatelly
            if (_pathStart == null)
            {
                return;
            }
            // Retrieving the accessible tiles
            Floor[] pathToTarget = _pathMap.GetPathToTarget(_pathStart);
            // For each accessible tile
            foreach (Floor floor in pathToTarget)
            {
                // Set it as a path
                floor.IsPath = true;
            }
        }
        // Reset the tiles path visuals
        private void ResetPaths()
        {
            // For each tile
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    // Setting path false (visual)
                    _map[i, j].IsPath = false;
                }
            }
        }
    }
}
