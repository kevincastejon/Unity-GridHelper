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
    }
    public class Grid2DMap : MonoBehaviour
    {

        private Camera _camera;
        private Tile[,] _map = new Tile[21, 24];
        private Tile[] _line = new Tile[0];
        private Tile _stopTile;
        private Tile _targetTile;
        private float _maxDistance = 0f;
        private bool _lastDragValue;
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
            }
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
                // Middle click
                if (Input.GetMouseButton(2))
                {
                    if (tile != _targetTile && tile.IsWalkable)
                    {
                        _stopTile = null;
                        ClearTiles();
                        SetStart(tile);
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
                    else
                    {
                        _stopTile = null;
                        ClearTiles();
                    }
                }
            }
            // No tile is hit by mouse
            else
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
            ClearTiles();
            switch (_demoType)
            {
                case DemoType.LINE_OF_TILES:
                    GetLineOfTiles();
                    break;
                case DemoType.LINE_OF_SIGHT:
                    GetLineOfSight();
                    break;
                default:
                    break;
            }
        }
        private void ClearTiles()
        {
            if (_line != null)
            {
                foreach (Tile tile in _line)
                {
                    tile.TileMode = TileMode.FLOOR;
                }
            }
            _line = new Tile[0];
        }
        private void GetLineOfTiles()
        {
            _line = Raycasting.GetWalkableTilesOnALine(_map, _targetTile, _stopTile, _maxDistance, false);
            foreach (Tile tile in _line)
            {
                tile.TileMode = TileMode.LINE;
            }
        }
        private void GetLineOfSight()
        {
            _line = Raycasting.GetLineOfSight(_map, _targetTile, _stopTile, _maxDistance, false);
            foreach (Tile tile in _line)
            {
                tile.TileMode = TileMode.LINE;
            }
        }
    }
}