using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Grid2DHelper.Demos.SideView
{
    public class Follower : MonoBehaviour
    {
        [SerializeField] private TileBase _pathTile;
        [SerializeField] private Tilemap _tilemapPath;
        [SerializeField] private float _speed=5f;
        private GridController _gridController;
        private PlayerController _player;
        private PathGrid<Tile> _pathGrid;
        private Tile[] _completePath;
        private Tile _targetTile;
        private bool _isOnTile = true;
        public PathGrid<Tile> PathGrid { set => _pathGrid = value; }
        public GridController GridController { set => _gridController = value; }
        public PlayerController Player { set => _player = value; }

        private void Update()
        {
            if (_isOnTile)
            {

                Vector2Int currentPosition = Vector2Int.RoundToInt(transform.position);
                Tile currentTile = _gridController.Map[currentPosition.y, currentPosition.x];
                Tile destination = _gridController.Map[_player.CurrentTile.y, _player.CurrentTile.x];
                _tilemapPath.ClearAllTiles();
                _completePath = _pathGrid.GetPath(currentTile, destination, false, false);
                foreach (Tile tile in _completePath)
                {
                    _tilemapPath.SetTile(new Vector3Int(tile.X, tile.Y), _pathTile);
                }
                if (_pathGrid.GetNextTileFromTile(currentTile, destination, out Tile nextTile))
                {
                    _targetTile = nextTile;
                    _isOnTile = false;
                }
                else
                {
                    _targetTile = null;
                    _isOnTile = true;
                }
            }
            if (_targetTile != null)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2Int(_targetTile.X, _targetTile.Y), _speed * Time.deltaTime);
                if (transform.position == new Vector3(_targetTile.X, _targetTile.Y))
                {
                    _isOnTile = true;
                }
            }
        }
    }
}
