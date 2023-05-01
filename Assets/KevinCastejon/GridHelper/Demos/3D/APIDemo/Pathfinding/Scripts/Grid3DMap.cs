using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grid3DHelper.APIDemo.Pathfinding
{
    public enum DemoType
    {
        ACCESSIBLE_TILES,
        PATH,
    }
    public class Grid3DMap : MonoBehaviour
    {
        private DemoType _demoType;
        Tile[,,] _map = new Tile[12, 16, 18];
        Tile[] _path;
        private Tile _targetTile;
        private Tile _stopTile;
        private float _maxDistance = 6f;
        PathMap3D<Tile> _pathMap;
        
        public DemoType DemoType
        {
            get
            {
                return _demoType;
            }

            set
            {
                _demoType = value;
                Demo();
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
                CalculatePathMap();
            }
        }

        public void MoveStartX(int value)
        {
            if (value >= 0 && value < _map.GetLength(1))
            {
                SetStart(_map[_targetTile.Y, value, _targetTile.Z]);
            }
        }
        public void MoveStartY(int value)
        {
            if (value >= 0 && value < _map.GetLength(0))
            {
                SetStart(_map[value, _targetTile.X, _targetTile.Z]);
            }
        }
        public void MoveStartZ(int value)
        {
            if (value >= 0 && value < _map.GetLength(2))
            {
                SetStart(_map[_targetTile.Y, _targetTile.X, value]);
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
            _targetTile = _map[6, 8, 1];
            _targetTile.TileMode = TileMode.OPAQUE;
            _stopTile = _map[6, 8, 16];
            _stopTile.TileMode = TileMode.OPAQUE;
            CalculatePathMap();
        }
        private void SetStop(Tile tile)
        {
            if (_stopTile)
            {
                _stopTile.TileMode = TileMode.FADE;
            }
            _stopTile = tile;
            Demo();
            _stopTile.TileMode = TileMode.OPAQUE;
        }
        private void SetStart(Tile tile)
        {
            if (_targetTile)
            {
                _targetTile.TileMode = TileMode.FADE;
            }
            _targetTile = tile;
            CalculatePathMap();
            _targetTile.TileMode = TileMode.OPAQUE;
        }
        private void CalculatePathMap()
        {
            _pathMap = Pathfinding3D.GeneratePathMap(_map, _targetTile, _maxDistance);
            Demo();
        }
        private void Demo()
        {
            if (_path != null)
            {
                foreach (Tile tile in _path)
                {
                    tile.TileMode = TileMode.FADE;
                }
            }
            switch (_demoType)
            {
                case DemoType.ACCESSIBLE_TILES:
                    _path = _pathMap.GetAccessibleTiles();
                    break;
                case DemoType.PATH:
                    _path = _pathMap.GetPathFromTarget(_stopTile, false, false);
                    break;
                default:
                    break;
            }
            foreach (Tile tile in _path)
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