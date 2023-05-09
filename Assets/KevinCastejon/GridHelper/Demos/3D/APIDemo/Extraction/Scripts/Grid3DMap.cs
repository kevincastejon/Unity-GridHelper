using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Grid3DHelper.APIDemo.Extraction
{
    public enum DemoType
    {
        SPHERE,
        SPHERE_OUTLINE,
        CUBOID,
        CUBOID_OUTLINE,
    }
    public class Grid3DMap : MonoBehaviour
    {
        private int _radius = 2;
        private int _sizeX = 2;
        private int _sizeY = 2;
        private int _sizeZ = 2;
        private DemoType _demoType;
        Tile[,,] _map = new Tile[12, 16, 18];
        Tile[] _extractedTiles;
        private Tile _targetTile;

        public int Radius
        {
            get
            {
                return _radius;
            }

            set
            {
                _radius = value;
                Extract();
            }
        }

        public int SizeX
        {
            get
            {
                return _sizeX;
            }

            set
            {
                _sizeX = value;
                Extract();
            }
        }
        public int SizeY
        {
            get
            {
                return _sizeY;
            }

            set
            {
                _sizeY = value;
                Extract();
            }
        }
        public int SizeZ
        {
            get
            {
                return _sizeZ;
            }

            set
            {
                _sizeZ = value;
                Extract();
            }
        }

        public DemoType DemoType
        {
            get
            {
                return _demoType;
            }

            set
            {
                _demoType = value;
                Extract();
            }
        }

        public void MoveTargetX(int value)
        {
            if (value >= 0 && value < _map.GetLength(1))
            {
                SetTarget(_map[_targetTile.Y, value, _targetTile.Z]);
            }
        }
        public void MoveTargetY(int value)
        {
            if (value >= 0 && value < _map.GetLength(0))
            {
                SetTarget(_map[value, _targetTile.X, _targetTile.Z]);
            }
        }
        public void MoveTargetZ(int value)
        {
            if (value >= 0 && value < _map.GetLength(2))
            {
                SetTarget(_map[_targetTile.Y, _targetTile.X, value]);
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
            SetTarget(_map[Mathf.FloorToInt(_map.GetLength(0) * 0.5f), Mathf.FloorToInt(_map.GetLength(1) * 0.5f), Mathf.FloorToInt(_map.GetLength(2) * 0.5f)]);
        }

        private void SetTarget(Tile tile)
        {
            if (_targetTile)
            {
                _targetTile.TileMode = TileMode.FADE;
            }
            _targetTile = tile;
            Extract();
            _targetTile.TileMode = TileMode.OPAQUE;
        }
        private void Extract()
        {
            if (_extractedTiles != null)
            {
                foreach (Tile tile in _extractedTiles)
                {
                    tile.TileMode = TileMode.FADE;
                }
            }
            switch (_demoType)
            {
                case DemoType.CUBOID:
                    _extractedTiles = Extraction3D.GetTilesInACuboid(_map, _targetTile, _sizeX, _sizeY, _sizeZ, false);
                    break;
                case DemoType.CUBOID_OUTLINE:
                    _extractedTiles = Extraction3D.GetTilesOnACuboidOutline(_map, _targetTile, _sizeX, _sizeY, _sizeZ);
                    break;
                case DemoType.SPHERE:
                    _extractedTiles = Extraction3D.GetTilesInASphere(_map, _targetTile, _radius, false);
                    Debug.Log(_extractedTiles.Length);
                    break;
                case DemoType.SPHERE_OUTLINE:
                    _extractedTiles = Extraction3D.GetTilesOnASphereOutline(_map, _targetTile, _radius);
                    Debug.Log(_extractedTiles.Length);
                    break;
                default:
                    break;
            }
            foreach (Tile tile in _extractedTiles)
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