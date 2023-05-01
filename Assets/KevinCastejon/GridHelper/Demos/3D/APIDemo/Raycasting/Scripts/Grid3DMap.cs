using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grid3DHelper.APIDemo.Raycasting
{
    public enum DemoType
    {
        TILES_ON_A_LINE,
        LINE_OF_SIGHT,
    }
    public class Grid3DMap : MonoBehaviour
    {
        private DemoType _demoType;
        Tile[,,] _map = new Tile[12, 16, 18];
        Tile[] _tilesLine;
        private Tile _startTile;
        private Tile _stopTile;
        
        public DemoType DemoType
        {
            get
            {
                return _demoType;
            }

            set
            {
                _demoType = value;
                Raycast();
            }
        }

        public void MoveStartX(int value)
        {
            if (value >= 0 && value < _map.GetLength(1))
            {
                SetStart(_map[_startTile.Y, value, _startTile.Z]);
            }
        }
        public void MoveStartY(int value)
        {
            if (value >= 0 && value < _map.GetLength(0))
            {
                SetStart(_map[value, _startTile.X, _startTile.Z]);
            }
        }
        public void MoveStartZ(int value)
        {
            if (value >= 0 && value < _map.GetLength(2))
            {
                SetStart(_map[_startTile.Y, _startTile.X, value]);
            }
        }
        public void MoveStopX(int value)
        {
            if (value >= 0 && value < _map.GetLength(1))
            {
                SetStop(_map[_stopTile.Y, value, _stopTile.Z]);
            }
        }
        public void MoveStopY(int value)
        {
            if (value >= 0 && value < _map.GetLength(0))
            {
                SetStop(_map[value, _stopTile.X, _stopTile.Z]);
            }
        }
        public void MoveStopZ(int value)
        {
            if (value >= 0 && value < _map.GetLength(2))
            {
                SetStop(_map[_stopTile.Y, _stopTile.X, value]);
            }
        }


        private void Awake()
        {
            Tile[] tiles = FindObjectsOfType<Tile>();
            foreach (Tile tile in tiles)
            {
                tile.X = Mathf.RoundToInt(tile.transform.position.x);
                tile.Y = Mathf.RoundToInt(tile.transform.position.y);
                tile.Z = Mathf.RoundToInt(tile.transform.position.z);
                _map[tile.Y, tile.X, tile.Z] = tile;
            }
            _startTile = _map[6, 8, 1];
            _startTile.TileMode = TileMode.OPAQUE;
            _stopTile = _map[6, 8, 16];
            _stopTile.TileMode = TileMode.OPAQUE;
            Raycast();
        }
        private void SetStop(Tile tile)
        {
            if (_stopTile)
            {
                _stopTile.TileMode = TileMode.FADE;
            }
            _stopTile = tile;
            Raycast();
            _stopTile.TileMode = TileMode.OPAQUE;
        }
        private void SetStart(Tile tile)
        {
            if (_startTile)
            {
                _startTile.TileMode = TileMode.FADE;
            }
            _startTile = tile;
            Raycast();
            _startTile.TileMode = TileMode.OPAQUE;
        }
        private void Raycast()
        {
            if (_tilesLine != null)
            {
                foreach (Tile tile in _tilesLine)
                {
                    tile.TileMode = TileMode.FADE;
                }
            }
            switch (_demoType)
            {
                case DemoType.TILES_ON_A_LINE:
                    _tilesLine = Raycasting3D.GetWalkableTilesOnALine(_map, _startTile, _stopTile, 0f, false, false);
                    break;
                case DemoType.LINE_OF_SIGHT:
                    _tilesLine = Raycasting3D.GetLineOfSight(_map, _startTile, _stopTile, 0f, false, false);
                    break;
                default:
                    break;
            }
            foreach (Tile tile in _tilesLine)
            {
                tile.TileMode = TileMode.SEMIFADE;
            }
        }
        public void SetDemoType(int enumIndex)
        {
            DemoType = (DemoType)enumIndex;
        }
    }
}