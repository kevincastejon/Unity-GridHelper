using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid2DHelper.APIDemo.PathfindingDemo
{
    public class Grid2DMap : MonoBehaviour
    {
        [SerializeField] private Toggle _flyToggle;
        [SerializeField] private Toggle _wallBelowToggle;
        [SerializeField] private Toggle _wallAsideToggle;
        [SerializeField] private Toggle _wallAboveToggle;
        private Camera _camera;
        private Tile[,] _map = new Tile[21, 24];
        private Tile[] _accessibleTiles = new Tile[0];
        private Tile[] _path = new Tile[0];
        private PathMap<Tile> _pathMap;
        private DiagonalsPolicy _diagonalsPolicy;
        private MovementPolicy _movementPolicy;
        private Tile _startTile;
        private Tile _targetTile;
        private float _maxDistance = 0f;
        private float _diagonalsWeight = 1.4142135623730950488016887242097f;
        private bool _lastDragValue;
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
        public float MaxDistance
        {
            get
            {
                return _maxDistance;
            }

            set
            {
                _maxDistance = value;
                _startTile = null;
                ClearPathTiles();
                ClearAccessibleTiles();
                GetPathMap();
                ShowAccessibleTiles();
            }
        }

        public DiagonalsPolicy DiagonalsPolicy
        {
            get
            {
                return _diagonalsPolicy;
            }

            set
            {
                _diagonalsPolicy = value;
                _startTile = null;
                ClearPathTiles();
                ClearAccessibleTiles();
                GetPathMap();
                ShowAccessibleTiles();
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
                _startTile = null;
                ClearPathTiles();
                ClearAccessibleTiles();
                GetPathMap();
                ShowAccessibleTiles();
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
                _startTile = null;
                ClearPathTiles();
                ClearAccessibleTiles();
                GetPathMap();
                ShowAccessibleTiles();
            }
        }

        public void SetDiagonalsPolicy(int diagonalsPolicy)
        {
            DiagonalsPolicy = (DiagonalsPolicy)diagonalsPolicy;
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
            SetTarget(_targetTile);
        }
        private void Update()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            bool tileIsHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
            Tile tile = tileIsHit ? hit.collider.GetComponent<Tile>() : null;

            if (tileIsHit)
            {
                // Middle click
                if (Input.GetMouseButton(2))
                {
                    if (tile != _targetTile && tile.IsWalkable)
                    {
                        SetTarget(tile);
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
                    SwitchWalkableState(tile);
                }
                // Keep clicking
                else if (Input.GetMouseButton(0))
                {
                    if (tile.IsWalkable == _lastDragValue || tile == _targetTile)
                    {
                        return;
                    }
                    SwitchWalkableState(tile);
                }
                // Hovered without click
                else
                {
                    if (tile != _targetTile && _pathMap.IsTileAccessible(tile))
                    {
                        if (tile != _startTile)
                        {
                            SetStart(tile);
                        }
                    }
                    else
                    {
                        _startTile = null;
                        ClearPathTiles();
                    }
                }
            }
            // No tile is hit by mouse
            else
            {
                _startTile = null;
                ClearPathTiles();
            }
        }
        private void SwitchWalkableState(Tile tile)
        {
            bool newValue = !tile.IsWalkable;
            if (newValue)
            {
                ClearPathTiles();
                ClearAccessibleTiles();
                tile.IsWalkable = newValue;
                GetPathMap();
                ShowAccessibleTiles();
                if (_pathMap.IsTileAccessible(tile))
                {
                    _startTile = tile;
                    _startTile.TileMode = TileMode.PATH;
                    ShowPathTiles();
                }
            }
            else
            {
                ClearPathTiles();
                ClearAccessibleTiles();
                tile.IsWalkable = newValue;
                GetPathMap();
                ShowAccessibleTiles();
                _startTile = null;
                ClearPathTiles();
            }
        }
        private void SetTarget(Tile tile)
        {
            _startTile = null;
            ClearPathTiles();
            ClearAccessibleTiles();
            _targetTile.TileMode = TileMode.FLOOR;
            _targetTile = tile;
            _targetTile.TileMode = TileMode.TARGET;
            GetPathMap();
            ShowAccessibleTiles();
        }
        private void SetStart(Tile tile)
        {
            if (_startTile)
            {
                _startTile.TileMode = TileMode.FLOOR;
            }
            ClearPathTiles();
            _startTile = tile;
            _startTile.TileMode = TileMode.PATH;
            ShowPathTiles();
        }
        private void GetPathMap()
        {
            _pathMap = Pathfinding.GeneratePathMap(_map, _targetTile, _maxDistance, new PathfindingPolicy(_diagonalsPolicy, _diagonalsWeight, _movementPolicy));
        }
        private void ClearPathTiles()
        {
            if (_path != null)
            {
                foreach (Tile tile in _path)
                {
                    tile.TileMode = _pathMap.IsTileAccessible(tile) ? TileMode.ACCESSIBLE : TileMode.FLOOR;
                }
            }
            _path = new Tile[0];
        }
        private void ClearAccessibleTiles()
        {
            if (_accessibleTiles != null)
            {
                foreach (Tile tile in _accessibleTiles)
                {
                    tile.TileMode = TileMode.FLOOR;
                    tile.HideDistanceAndDirection();
                }
            }
            _accessibleTiles = new Tile[0];
        }
        private void ShowPathTiles()
        {
            _path = _pathMap.GetPathToTarget(_startTile, true, false);
            foreach (Tile tile in _path)
            {
                tile.TileMode = TileMode.PATH;
            }
        }
        private void ShowAccessibleTiles()
        {
            _accessibleTiles = _pathMap.GetAccessibleTiles(false);
            foreach (Tile tile in _accessibleTiles)
            {
                tile.TileMode = TileMode.ACCESSIBLE;
                tile.ShowDistanceAndDirection(_pathMap.GetDistanceToTargetFromTile(tile), _pathMap.GetNextTileDirectionFromTile(tile));
            }
        }
    }
}