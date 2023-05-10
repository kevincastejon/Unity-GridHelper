using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid3DHelper.APIDemo.Extraction
{
    public enum DemoType
    {
        EXTRACT_SPHERE,
        EXTRACT_SPHERE_OUTLINE,
        EXTRACT_CUBOID,
        EXTRACT_CUBOID_OUTLINE,
    }
    public class Grid3DMap : MonoBehaviour
    {
        [SerializeField] private Slider _sliderStartX;
        [SerializeField] private TextMeshProUGUI _sliderStartCountX;
        [SerializeField] private Slider _sliderStartY;
        [SerializeField] private TextMeshProUGUI _sliderStartCountY;
        [SerializeField] private Slider _sliderStartZ;
        [SerializeField] private TextMeshProUGUI _sliderStartCountZ;
        [SerializeField] private int _radius = 2;
        [SerializeField] private Vector3Int _size = Vector3Int.one * 2;

        private Tile[,,] _map = new Tile[12, 16, 18];
        private Tile[] _extractedTiles;
        private Tile _targetTile;
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
                Extract();
            }
        }
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
        public void SetDemoType(int demoType)
        {
            DemoType = (DemoType)demoType;
        }
        public void MoveStartX(int value)
        {
            if (_map[_targetTile.Y, value, _targetTile.Z].IsWalkable)
            {
                SetStart(_map[_targetTile.Y, value, _targetTile.Z]);
                _sliderStartCountX.text = value.ToString();
            }
            else
            {
                _sliderStartX.SetValueWithoutNotify(_targetTile.X);
                _sliderStartCountX.text = _targetTile.X.ToString();
            }
        }
        public void MoveStartY(int value)
        {
            if (_map[value, _targetTile.X, _targetTile.Z].IsWalkable)
            {
                SetStart(_map[value, _targetTile.X, _targetTile.Z]);
                _sliderStartCountY.text = value.ToString();
            }
            else
            {
                _sliderStartY.SetValueWithoutNotify(_targetTile.Y);
                _sliderStartCountY.text = _targetTile.Y.ToString();
            }
        }
        public void MoveStartZ(int value)
        {
            if (_map[_targetTile.Y, _targetTile.X, value].IsWalkable)
            {
                SetStart(_map[_targetTile.Y, _targetTile.X, value]);
                _sliderStartCountZ.text = value.ToString();
            }
            else
            {
                _sliderStartZ.SetValueWithoutNotify(_targetTile.Z);
                _sliderStartCountZ.text = _targetTile.Z.ToString();
            }
        }
        public void SetRectangleSizeX(int value)
        {
            _size.x = value;
            Extract();
        }
        public void SetRectangleSizeY(int value)
        {
            _size.y = value;
            Extract();
        }
        public void SetRectangleSizeZ(int value)
        {
            _size.z = value;
            Extract();
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
            _targetTile = _map[4, 8, 1];
            _targetTile.TileMode = TileMode.TARGET;
            _sliderStartY.SetValueWithoutNotify(4);
            _sliderStartCountY.text = "4";
            _sliderStartX.SetValueWithoutNotify(8);
            _sliderStartCountX.text = "8";
            _sliderStartZ.SetValueWithoutNotify(1);
            _sliderStartCountZ.text = "1";
            Extract();
        }
        private void SetStart(Tile tile)
        {
            _targetTile.TileMode = TileMode.AIR;
            _targetTile = tile;
            Extract();
            _targetTile.TileMode = TileMode.TARGET;
        }
        private void Extract()
        {
            ClearTiles();
            switch (_demoType)
            {
                case DemoType.EXTRACT_SPHERE:
                    ExtractSphere();
                    break;
                case DemoType.EXTRACT_SPHERE_OUTLINE:
                    ExtractSphereOutline();
                    break;
                case DemoType.EXTRACT_CUBOID:
                    ExtractCuboid();
                    break;
                case DemoType.EXTRACT_CUBOID_OUTLINE:
                    ExtractCuboidOutline();
                    break;
                default:
                    break;
            }
        }
        private void ClearTiles()
        {
            if (_extractedTiles != null)
            {
                foreach (Tile tile in _extractedTiles)
                {
                    tile.TileMode = TileMode.AIR;
                }
            }
        }
        private void ExtractSphere()
        {
            _extractedTiles = Extraction3D.GetWalkableTilesInASphere(_map, _targetTile, _radius, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.ACCESSIBLE;
            }
        }
        private void ExtractSphereOutline()
        {
            _extractedTiles = Extraction3D.GetWalkableTilesOnASphereOutline(_map, _targetTile, _radius);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.ACCESSIBLE;
            }
        }
        private void ExtractCuboid()
        {
            _extractedTiles = Extraction3D.GetWalkableTilesInACuboid(_map, _targetTile, _size, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.ACCESSIBLE;
            }
        }
        private void ExtractCuboidOutline()
        {
            _extractedTiles = Extraction3D.GetWalkableTilesOnACuboidOutline(_map, _targetTile, _size);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.ACCESSIBLE;
            }
        }
    }
}