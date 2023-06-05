using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Grid2DHelper.APIDemo.Playground_WEBGL_NoAsync
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private Transform _selectionOutline;
        [SerializeField] private Transform _clampedSelectionOutline;
        private readonly Color _floorColor = Color.white;
        private readonly Color _centerColor = Color.red;
        private readonly Color _wallColor = Color.black;
        private readonly Color _highlightedColor = Color.cyan;
        private readonly Color _pathColor = Color.green;
        private readonly Color _startColor = Color.magenta;
        private Camera _camera;
        private Tilemap _tileMap;
        private Tile[,] _map;
        private Tile _centerTile;
        private Tile _startTile;
        private Tile[] _highlightedTiles;
        private Tile[] _pathTiles;
        private Tile _hoveredTile;
        private Tile _clampedHoveredTile;
        private bool _justEnteredTile;
        private bool _justEnteredClampedTile;
        private bool _mouseEnabled = true;

        public Tile[,] Map { get => _map; }
        public Tile HoveredTile { get => _hoveredTile; }
        public bool JustEnteredTile { get => _justEnteredTile; }
        public bool JustEnteredClampedTile { get => _justEnteredClampedTile; }
        public Tile CenterTile { get => _centerTile; }
        public Tile StartTile { get => _startTile; }
        public Tile ClampedHoveredTile { get => _clampedHoveredTile; }
        public bool MouseEnabled { get => _mouseEnabled; set => _mouseEnabled = value; }

        private void Awake()
        {
            _camera = Camera.main;
            _tileMap = GetComponent<Tilemap>();
            _map = new Tile[_tileMap.size.y, _tileMap.size.x];
            for (int y = 0; y < _tileMap.size.y; y++)
            {
                for (int x = 0; x < _tileMap.size.x; x++)
                {
                    Color tile = _tileMap.GetColor(new Vector3Int(x, y));
                    if (tile == _centerColor)
                    {
                        _map[y, x] = new Tile(x, y, true);
                        _centerTile = _map[y, x];
                    }
                    else if (tile == _startColor)
                    {
                        _map[y, x] = new Tile(x, y, true);
                        _startTile = _map[y, x];
                    }
                    else
                    {
                        _map[y, x] = new Tile(x, y, tile == _floorColor);
                    }
                }
            }
        }
        private void Update()
        {
            _justEnteredTile = false; ;
            _justEnteredClampedTile = false;
            if (!_mouseEnabled)
            {
                _hoveredTile = null;
                _clampedHoveredTile = null;
                return;
            }
            Vector2Int mouseWorldPos = Vector2Int.RoundToInt(_camera.ScreenToWorldPoint(Input.mousePosition));
            Vector2Int clampedMouseWorldPos = new Vector2Int(Mathf.Clamp(mouseWorldPos.x, 0, _tileMap.size.x - 1), Mathf.Clamp(mouseWorldPos.y, 0, _tileMap.size.y - 1));
            if (mouseWorldPos.x >= 0 && mouseWorldPos.x < _tileMap.size.x && mouseWorldPos.y >= 0 && mouseWorldPos.y < _tileMap.size.y)
            {
                Tile tile = _map[mouseWorldPos.y, mouseWorldPos.x];
                if (_hoveredTile == null || !GridUtils.TileEquals(tile, _hoveredTile))
                {
                    _hoveredTile = tile;
                    _justEnteredTile = true;
                    if (_selectionOutline)
                    {
                        _selectionOutline.position = (Vector3Int)mouseWorldPos;
                    }
                }
            }
            else
            {
                _hoveredTile = null;
            }
            Tile clampedTile = _map[clampedMouseWorldPos.y, clampedMouseWorldPos.x];
            if (_clampedHoveredTile == null || !GridUtils.TileEquals(clampedTile, _clampedHoveredTile))
            {
                _clampedHoveredTile = clampedTile;
                _justEnteredClampedTile = true;
                if (_clampedSelectionOutline)
                {
                    _clampedSelectionOutline.position = (Vector3Int)clampedMouseWorldPos;
                }
            }
        }
        public void SetWalkable(Tile tile, bool isWalkable)
        {
            ClearPathTiles();
            ClearHighlightedTiles();
            tile.IsWalkable = isWalkable;
            _tileMap.SetColor(new Vector3Int(tile.X, tile.Y), isWalkable ? _floorColor : _wallColor);
        }
        public void Refresh(Tile center = null, Tile[] highlightedTiles = null, Tile[] pathTiles = null, Tile start = null)
        {
            TintCenter(center);
            TintStart(start);
            TintHighlightedTiles(highlightedTiles);
            TintPathTiles(pathTiles);
        }
        private void TintCenter(Tile tile)
        {
            if (tile == null || _centerTile == tile)
            {
                return;
            }
            ClearCenter();
            _centerTile = tile;
            _tileMap.SetColor(new Vector3Int(_centerTile.X, _centerTile.Y), _centerColor);
        }
        private void TintStart(Tile tile)
        {
            if (tile == null || _startTile == tile)
            {
                return;
            }
            ClearStart();
            _startTile = tile;
            _tileMap.SetColor(new Vector3Int(_startTile.X, _startTile.Y), _startColor);
        }
        private void TintHighlightedTiles(Tile[] highlightedTiles)
        {
            if (highlightedTiles == null)
            {
                return;
            }
            ClearHighlightedTiles();
            _highlightedTiles = highlightedTiles;
            for (int i = 0; i < _highlightedTiles.Length; i++)
            {
                if (_highlightedTiles[i] != _startTile)
                {
                    _tileMap.SetColor(new Vector3Int(_highlightedTiles[i].X, _highlightedTiles[i].Y), _highlightedColor);
                }
            }
        }
        private void TintPathTiles(Tile[] pathTiles)
        {
            if (pathTiles == null)
            {
                return;
            }
            ClearPathTiles();
            _pathTiles = pathTiles;
            for (int i = 0; i < _pathTiles.Length; i++)
            {
                _tileMap.SetColor(new Vector3Int(_pathTiles[i].X, _pathTiles[i].Y), _pathColor);
            }
        }
        private void ClearCenter()
        {
            _tileMap.SetColor(new Vector3Int(_centerTile.X, _centerTile.Y), _floorColor);
        }
        private void ClearStart()
        {
            if (_highlightedTiles != null && _highlightedTiles.Contains(_startTile))
            {
                _tileMap.SetColor(new Vector3Int(_startTile.X, _startTile.Y), _highlightedColor);
            }
            else
            {
                _tileMap.SetColor(new Vector3Int(_startTile.X, _startTile.Y), _floorColor);
            }
        }
        private void ClearHighlightedTiles()
        {
            if (_highlightedTiles != null)
            {
                for (int i = 0; i < _highlightedTiles.Length; i++)
                {
                    if (_highlightedTiles[i] != _centerTile && _highlightedTiles[i] != _startTile)
                    {
                        _tileMap.SetColor(new Vector3Int(_highlightedTiles[i].X, _highlightedTiles[i].Y), _floorColor);
                    }
                }
                _highlightedTiles = null;
            }
        }
        private void ClearPathTiles()
        {
            if (_pathTiles != null)
            {
                for (int i = 0; i < _pathTiles.Length; i++)
                {
                    if (_pathTiles[i] != _centerTile && _pathTiles[i] != _startTile)
                    {
                        if (_highlightedTiles != null && _highlightedTiles.Contains(_pathTiles[i]))
                        {
                            _tileMap.SetColor(new Vector3Int(_pathTiles[i].X, _pathTiles[i].Y), _highlightedColor);
                        }
                        else
                        {
                            _tileMap.SetColor(new Vector3Int(_pathTiles[i].X, _pathTiles[i].Y), _floorColor);
                        }
                    }
                }
                _pathTiles = null;
            }
        }
    }
}