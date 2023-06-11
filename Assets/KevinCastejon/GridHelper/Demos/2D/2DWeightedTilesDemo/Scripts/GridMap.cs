using KevinCastejon.GridHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeightedTilesDemo
{
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private Transform _targetSphere;
        [SerializeField] private Slider _plainSlider;
        [SerializeField] private TextMeshProUGUI _plainLabel;
        [SerializeField] private Slider _forestSlider;
        [SerializeField] private TextMeshProUGUI _forestLabel;
        public static float PLAIN_WEIGHT = 1f;
        public static float FOREST_WEIGHT = 2f;
        private Camera _camera;
        private Tile[,] _map = new Tile[10, 12];
        private PathMap<Tile> _globalPathMap;
        private Tile _hoveredTile;
        private Tile _targetTile;
        private Tile _startTile;
        private Tile[] _pathTiles;
        private LineRenderer _lineRenderer;
        private bool _lastDragTileWasForest;
        private bool _showPaths;

        private void Awake()
        {
            Tile[] tiles = FindObjectsOfType<Tile>();
            foreach (Tile tile in tiles)
            {
                tile.X = Mathf.RoundToInt(tile.transform.position.x);
                tile.Y = Mathf.RoundToInt(tile.transform.position.y);
                _map[tile.Y, tile.X] = tile;
            }
            _targetTile = _map[Mathf.RoundToInt(_targetSphere.transform.position.y), Mathf.RoundToInt(_targetSphere.transform.position.x)];
            _camera = Camera.main;
            _lineRenderer = GetComponent<LineRenderer>();
        }
        private void Start()
        {
            //Ui init
            _plainSlider.value = PLAIN_WEIGHT;
            _forestSlider.value = FOREST_WEIGHT;
            _plainLabel.text = PLAIN_WEIGHT.ToString("F1");
            _forestLabel.text = FOREST_WEIGHT.ToString("F1");
            GenerateGlobalPathMap();
            ShowDistances();
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
                _hoveredTile = null;
                _startTile = null;
                _lineRenderer.positionCount = 0;
            }
        }

        public void SetPlainWeight(float weight)
        {
            PLAIN_WEIGHT = weight;
            OnTargetChange();
        }
        public void SetForestWeight(float weight)
        {
            FOREST_WEIGHT = weight;
            OnTargetChange();
        }
        private void OnTargetChange()
        {
            _startTile = null;
            _lineRenderer.positionCount = 0;
            _targetSphere.position = new Vector3(_targetTile.X, _targetTile.Y, -1f);
            GenerateGlobalPathMap();
            if (_showPaths)
            {
                ShowDirections();
            }
            else
            {
                ShowDistances();
            }
        }
        private void GetPath()
        {
            _pathTiles = _globalPathMap.GetPathFromTarget(_startTile);
            _lineRenderer.positionCount = _pathTiles.Length;
            _lineRenderer.SetPositions(_pathTiles.Select(t => new Vector3(t.transform.position.x, t.transform.position.y, -1f)).ToArray());
        }
        private void OnTileStateChange()
        {
            GenerateGlobalPathMap();
            GetPath();
            if (_showPaths)
            {
                ShowDirections();
            }
            else
            {
                ShowDistances();
            }
        }
        private void SetTarget(Tile tile)
        {
            if (tile == _targetTile)
            {
                return;
            }
            _targetTile = tile;
            OnTargetChange();
        }
        private void OnTileEntered(Tile hoveredTile)
        {
            if (hoveredTile == _startTile)
            {
                return;
            }
            _startTile = hoveredTile;
            GetPath();
        }
        private void OnTileClicked(Tile clickedTile)
        {
            clickedTile.TileState = clickedTile.TileState == TileState.PLAIN ? TileState.FOREST : TileState.PLAIN;
            _lastDragTileWasForest = clickedTile.TileState == TileState.FOREST;
            OnTileStateChange();
        }
        private void OnTileDragEntered(Tile enteredTile)
        {
            if ((_lastDragTileWasForest && enteredTile.TileState == TileState.FOREST) || (!_lastDragTileWasForest && enteredTile.TileState == TileState.PLAIN))
            {
                return;
            }
            enteredTile.TileState = enteredTile.TileState == TileState.PLAIN ? TileState.FOREST : TileState.PLAIN;
            _startTile = enteredTile;
            OnTileStateChange();
        }
        private void GenerateGlobalPathMap()
        {
            // Generating a path map
            _globalPathMap = Pathfinding.GeneratePathMap(_map, _targetTile, 0f, new PathfindingPolicy(DiagonalsPolicy.NONE));
        }
        public void ShowDistances()
        {
            _showPaths = false;
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    _map[i, j].ShowDistance(_globalPathMap.GetDistanceToTargetFromTile(_map[i, j]));
                }
            }
        }
        public void ShowDirections()
        {
            _showPaths = true;
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    _map[i, j].ShowDirection(_globalPathMap.GetNextTileDirectionFromTile(_map[i, j]));
                }
            }
        }
    }
}
