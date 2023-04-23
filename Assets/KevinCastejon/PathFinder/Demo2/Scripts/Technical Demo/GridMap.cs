using KevinCastejon.Pathfinding2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Technical_Demo2
{
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private bool _allowDiagonals;
        [SerializeField] [Range(0, 99)] private int _maxMovement;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Material _pathMat;
        [SerializeField] private Floor _target;
        private Floor[,] _map = new Floor[12, 10];
        private Floor _pathStart;
        private PathMap _pathMap;
        private Camera _camera;
        private bool _isPathfindingMode;
        private bool _firstDragValue;

        public Floor[,] Map { get => _map; }
        public int MaxMovement
        {
            get => _maxMovement;
            set
            {
                _maxMovement = value;
                ShowAccessibleTiles();
            }
        }

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
            // Doing some UI tricks
            FindObjectOfType<Slider>().value = _maxMovement;
            FindObjectOfType<Toggle>().isOn = _allowDiagonals;
            FindObjectOfType<SliderFloatToStringConverter>().ValueChanged(_maxMovement);
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
                // If middle-click
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
                else if (Input.GetMouseButtonDown(0))
                {
                    if (_isPathfindingMode)
                    {
                        if (clickedFloor.IsWalkable)
                        {
                            _pathStart = clickedFloor;
                            // Displaying the path to target
                            ShowPathToTarget();
                        }
                    }
                    else
                    {
                        if (!clickedFloor.IsTarget)
                        {
                            // Inverting the walkable state
                            clickedFloor.IsWalkable = !clickedFloor.IsWalkable;
                            _firstDragValue = clickedFloor.IsWalkable;
                            // Generating a path map
                            GeneratePathMap();
                            // Displaying the accessible tiles
                            ShowAccessibleTiles();
                        }
                    }
                }
                else if (Input.GetMouseButton(0))
                {
                    if (_isPathfindingMode)
                    {
                        if (clickedFloor.IsWalkable && _pathStart != clickedFloor)
                        {
                            _pathStart = clickedFloor;
                            // Displaying the path to target
                            ShowPathToTarget();
                        }
                    }
                    else
                    {
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
        private void GeneratePathMap()
        {
            // Generating a path map
            _pathMap = PathFinder.GeneratePathMap(_map, _target, _allowDiagonals);
        }
        private void ShowAccessibleTiles()
        {
            // Reseting all tiles
            ResetPaths();
            // Retrieving the accessible tiles
            Floor[] accessibleTiles = _pathMap.GetAccessibleTilesFromTarget<Floor>(_maxMovement);
            // For each accessible tile
            foreach (Floor floor in accessibleTiles)
            {
                // Set it as a path
                floor.IsPath = true;
            }
        }
        private void ShowPathToTarget()
        {
            // Reseting all tiles
            ResetPaths();
            if (_pathStart == null)
            {
                return;
            }
            // Retrieving the accessible tiles
            Floor[] pathToTarget = _pathMap.GetPathToTarget<Floor>(_pathStart);
            // For each accessible tile
            foreach (Floor floor in pathToTarget)
            {
                // Set it as a path
                floor.IsPath = true;
            }
        }
        private void ResetPaths()
        {
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    _map[i, j].IsPath = false;
                }
            }
        }
    }
}
