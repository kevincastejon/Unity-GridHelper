using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid3DHelper.APIDemo.Raycasting
{
    public enum DemoType
    {
        LINE_OF_TILES,
        LINE_OF_SIGHT,
    }
    public class Grid3DMap : MonoBehaviour
    {
        [SerializeField] private Slider _sliderStartX;
        [SerializeField] private TextMeshProUGUI _sliderStartCountX;
        [SerializeField] private Slider _sliderStartY;
        [SerializeField] private TextMeshProUGUI _sliderStartCountY;
        [SerializeField] private Slider _sliderStartZ;
        [SerializeField] private TextMeshProUGUI _sliderStartCountZ;
        [SerializeField] private Slider _sliderStopX;
        [SerializeField] private TextMeshProUGUI _sliderStopCountX;
        [SerializeField] private Slider _sliderStopY;
        [SerializeField] private TextMeshProUGUI _sliderStopCountY;
        [SerializeField] private Slider _sliderStopZ;
        [SerializeField] private TextMeshProUGUI _sliderStopCountZ;
        [SerializeField] private float _maxDistance;

        private Tile[,,] _map = new Tile[12, 16, 18];
        private Tile[] _lineOfTiles;
        private Tile _targetTile;
        private Tile _stopTile;
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
                CalculateLine();
            }
        }
        public void SetDemoType(int demoType)
        {
            DemoType = (DemoType)demoType;
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
                CalculateLine();
            }
        }

        public void MoveStartX(int value)
        {
            if (_map[_targetTile.Y, value, _targetTile.Z].IsWalkable && _map[_targetTile.Y, value, _targetTile.Z] != _stopTile)
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
            if (_map[value, _targetTile.X, _targetTile.Z].IsWalkable && _map[value, _targetTile.X, _targetTile.Z] != _stopTile)
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
            if (_map[_targetTile.Y, _targetTile.X, value].IsWalkable && _map[_targetTile.Y, _targetTile.X, value] != _stopTile)
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

        public void MoveStopX(int value)
        {
            if (_map[_stopTile.Y, value, _stopTile.Z].IsWalkable && _map[_stopTile.Y, value, _stopTile.Z] != _targetTile)
            {
                SetStop(_map[_stopTile.Y, value, _stopTile.Z]);
                _sliderStopCountX.text = value.ToString();
            }
            else
            {
                _sliderStopX.SetValueWithoutNotify(_stopTile.X);
                _sliderStopCountX.text = _stopTile.X.ToString();
            }
        }
        public void MoveStopY(int value)
        {
            if (_map[value, _stopTile.X, _stopTile.Z].IsWalkable && _map[value, _stopTile.X, _stopTile.Z] != _targetTile)
            {
                SetStop(_map[value, _stopTile.X, _stopTile.Z]);
                _sliderStopCountY.text = value.ToString();
            }
            else
            {
                _sliderStopY.SetValueWithoutNotify(_stopTile.Y);
                _sliderStopCountY.text = _stopTile.Y.ToString();
            }
        }
        public void MoveStopZ(int value)
        {
            if (_map[_stopTile.Y, _stopTile.X, value].IsWalkable && _map[_stopTile.Y, _stopTile.X, value] != _targetTile)
            {
                SetStop(_map[_stopTile.Y, _stopTile.X, value]);
                _sliderStopCountZ.text = value.ToString();
            }
            else
            {
                _sliderStopZ.SetValueWithoutNotify(_stopTile.Z);
                _sliderStopCountZ.text = _stopTile.Z.ToString();
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
            _targetTile = _map[4, 8, 1];
            _targetTile.TileMode = TileMode.TARGET;
            _sliderStartY.SetValueWithoutNotify(4);
            _sliderStartCountY.text = "4";
            _sliderStartX.SetValueWithoutNotify(8);
            _sliderStartCountX.text = "8";
            _sliderStartZ.SetValueWithoutNotify(1);
            _sliderStartCountZ.text = "1";
            _stopTile = _map[4, 8, 16];
            _stopTile.TileMode = TileMode.STOP;
            _sliderStopY.SetValueWithoutNotify(4);
            _sliderStopCountY.text = "4";
            _sliderStopX.SetValueWithoutNotify(8);
            _sliderStopCountX.text = "8";
            _sliderStopZ.SetValueWithoutNotify(16);
            _sliderStopCountZ.text = "16";
            CalculateLine();
        }

        private void SetStop(Tile tile)
        {
            _stopTile.TileMode = TileMode.AIR;
            _stopTile = tile;
            _stopTile.TileMode = TileMode.STOP;
            CalculateLine();
        }
        private void SetStart(Tile tile)
        {
            _targetTile.TileMode = TileMode.AIR;
            _targetTile = tile;
            _targetTile.TileMode = TileMode.TARGET;
            CalculateLine();
        }
        private void CalculateLine()
        {
            ClearLine();
            if (_demoType == DemoType.LINE_OF_TILES)
            {
                ShowLineOfTiles();
            }
            else
            {
                ShowLineOfSight();
            }
        }
        private void ClearLine()
        {
            if (_lineOfTiles != null)
            {
                foreach (Tile tile in _lineOfTiles)
                {
                    if (tile != _stopTile)
                    {
                        tile.TileMode = TileMode.AIR;
                    }
                }
            }
        }
        private void ShowLineOfTiles()
        {
            _lineOfTiles = Raycasting3D.GetWalkableTilesOnALine(_map, _targetTile, _stopTile, _maxDistance, false, false);
            foreach (Tile tile in _lineOfTiles)
            {
                tile.TileMode = TileMode.PATH;
            }
        }
        private void ShowLineOfSight()
        {
            _lineOfTiles = Raycasting3D.GetLineOfSight(_map, _targetTile, _stopTile, _maxDistance, false, false);
            foreach (Tile tile in _lineOfTiles)
            {
                tile.TileMode = TileMode.PATH;
            }
        }
    }
}