using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid2DHelper.APIDemo.ExtractionDemo
{
    public enum DemoType
    {
        EXTRACT_CIRCLE,
        EXTRACT_CIRCLE_OUTLINE,
        EXTRACT_RECTANGLE,
        EXTRACT_RECTANGLE_OUTLINE,
        EXTRACT_CONE,
        EXTRACT_LINE,
        NEIGHBOR,
        NEIGHBORS_ORTHO,
        NEIGHBORS_DIAGONALS,
        NEIGHBORS_ALL,
    }
    public class Grid2DMap : MonoBehaviour
    {
        [SerializeField] private Image _circleLED;
        [SerializeField] private Image _circleOutlineLED;
        [SerializeField] private Image _rectangleLED;
        [SerializeField] private Image _rectangleOutlineLED;
        [SerializeField] private Image _coneLED;
        [SerializeField] private Image _lineLED;
        [SerializeField] private Image _neiLED;
        [SerializeField] private Image _neiOrthoLED;
        [SerializeField] private Image _neiDiagoLED;
        [SerializeField] private Image _neiAnyLED;
        private bool _allowDiagonals = true;
        private bool _favorVertical = false;
        private int _radius = 5;
        private float _direction = 0f;
        private float _angle = 90f;
        private Vector2Int _size = Vector2Int.one * 5;
        private Camera _camera;
        private Tile[,] _map = new Tile[65, 71];
        private Tile[] _extractedTiles;
        private Tile _targetTile;
        private Tile _hoveredTile;
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
        public float Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value;
                Extract();
            }
        }
        public float Angle
        {
            get
            {
                return _angle;
            }

            set
            {
                _angle = value;
                Extract();
            }
        }

        public bool AllowDiagonals
        {
            get
            {
                return _allowDiagonals;
            }

            set
            {
                _allowDiagonals = value;
                Extract();
            }
        }
        public bool FavorVertical
        {
            get
            {
                return _favorVertical;
            }

            set
            {
                _favorVertical = value;
                Extract();
            }
        }

        public void SetDemoType(int demoType)
        {
            DemoType = (DemoType)demoType;
        }
        public void SetCuboidSizeX(int value)
        {
            _size.x = value;
            Extract();
        }
        public void SetCuboidSizeY(int value)
        {
            _size.y = value;
            Extract();
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
            _targetTile = _map[15, 17];
            _targetTile.TileMode = TileMode.TARGET;
            Extract();
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
                    SetStart(tile);
                    _hoveredTile = tile;
                    ShowLED();
                }
                else if (_hoveredTile == null || !GridUtils.TileEquals(tile, _hoveredTile))
                {
                    _hoveredTile = tile;
                    ShowLED();
                }

            }
            else
            {
                _hoveredTile = null;
                ClearLED();
            }
        }

        private void SetStart(Tile tile)
        {
            if (GridUtils.TileEquals(_targetTile, tile))
            {
                return;
            }
            _targetTile.TileMode = TileMode.FLOOR;
            _targetTile = tile;
            Extract();
            _targetTile.TileMode = TileMode.TARGET;
        }

        private void ShowLED()
        {
            switch (_demoType) 
            {
                case DemoType.EXTRACT_CIRCLE:
                    _circleLED.color = Extraction.IsTileInACircle(_map, _hoveredTile, _targetTile, _radius) ? Color.green : Color.red;
                    break;
                case DemoType.EXTRACT_CIRCLE_OUTLINE:
                    _circleOutlineLED.color = Extraction.IsTileOnACircleOutline(_map, _hoveredTile, _targetTile, _radius) ? Color.green : Color.red;
                    break;
                case DemoType.EXTRACT_RECTANGLE:
                    _rectangleLED.color = Extraction.IsTileInARectangle(_map, _hoveredTile, _targetTile, _size) ? Color.green : Color.red;
                    break;
                case DemoType.EXTRACT_RECTANGLE_OUTLINE:
                    _rectangleOutlineLED.color = Extraction.IsTileOnARectangleOutline(_map, _hoveredTile, _targetTile, _size) ? Color.green : Color.red;
                    break;
                case DemoType.EXTRACT_CONE:
                    _coneLED.color = Extraction.IsTileInACone(_map, _hoveredTile, _targetTile, _radius, _angle, _direction) ? Color.green : Color.red;
                    break;
                case DemoType.EXTRACT_LINE:
                    _lineLED.color = Extraction.IsTileOnALine(_map, _hoveredTile, _targetTile, _radius, _direction, _allowDiagonals, _favorVertical) ? Color.green : Color.red;
                    break;
                case DemoType.NEIGHBOR:
                    _neiLED.color = Extraction.IsTileNeighbor(_targetTile, _hoveredTile, _direction) ? Color.green : Color.red;
                    break;
                case DemoType.NEIGHBORS_ORTHO:
                    _neiOrthoLED.color = Extraction.IsTileOrthogonalNeighbor(_targetTile, _hoveredTile) ? Color.green : Color.red;
                    break;
                case DemoType.NEIGHBORS_DIAGONALS:
                    _neiDiagoLED.color = Extraction.IsTileDiagonalNeighbor(_targetTile, _hoveredTile) ? Color.green : Color.red;
                    break;
                case DemoType.NEIGHBORS_ALL:
                    _neiAnyLED.color = Extraction.IsTileAnyNeighbor(_targetTile, _hoveredTile) ? Color.green : Color.red;
                    break;
                default:
                    break;
            }
        }
        private void ClearLED()
        {
            switch (_demoType)
            {
                case DemoType.EXTRACT_CIRCLE:
                    _circleLED.color = Color.red;
                    break;
                case DemoType.EXTRACT_CIRCLE_OUTLINE:
                    _circleOutlineLED.color = Color.red;
                    break;
                case DemoType.EXTRACT_RECTANGLE:
                    _rectangleLED.color = Color.red;
                    break;
                case DemoType.EXTRACT_RECTANGLE_OUTLINE:
                    _rectangleOutlineLED.color = Color.red;
                    break;
                case DemoType.EXTRACT_CONE:
                    _coneLED.color = Color.red;
                    break;
                case DemoType.EXTRACT_LINE:
                    _lineLED.color = Color.red;
                    break;
                case DemoType.NEIGHBOR:
                    _neiLED.color = Color.red;
                    break;
                case DemoType.NEIGHBORS_ORTHO:
                    _neiOrthoLED.color = Color.red;
                    break;
                case DemoType.NEIGHBORS_DIAGONALS:
                    _neiDiagoLED.color = Color.red;
                    break;
                case DemoType.NEIGHBORS_ALL:
                    _neiAnyLED.color = Color.red;
                    break;
                default:
                    break;
            }
        }

        private void Extract()
        {
            ClearTiles();
            switch (_demoType)
            {
                case DemoType.EXTRACT_CIRCLE:
                    ExtractCircle();
                    break;
                case DemoType.EXTRACT_CIRCLE_OUTLINE:
                    ExtractCircleOutline();
                    break;
                case DemoType.EXTRACT_RECTANGLE:
                    ExtractRectangle();
                    break;
                case DemoType.EXTRACT_RECTANGLE_OUTLINE:
                    ExtractRectangleOutline();
                    break;
                case DemoType.EXTRACT_CONE:
                    ExtractCone();
                    break;
                case DemoType.EXTRACT_LINE:
                    ExtractLine();
                    break;
                case DemoType.NEIGHBOR:
                    ExtractNeighbor();
                    break;
                case DemoType.NEIGHBORS_ORTHO:
                    ExtractOrthoNeighbors();
                    break;
                case DemoType.NEIGHBORS_DIAGONALS:
                    ExtractDiagonalsNeighbors();
                    break;
                case DemoType.NEIGHBORS_ALL:
                    ExtractNeighbors();
                    break;
                default:
                    break;
            }
        }

        private void ExtractCircle()
        {
            _extractedTiles = Extraction.GetTilesInACircle(_map, _targetTile, _radius, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }
        private void ExtractCircleOutline()
        {
            _extractedTiles = Extraction.GetTilesOnACircleOutline(_map, _targetTile, _radius, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }
        private void ExtractRectangle()
        {
            _extractedTiles = Extraction.GetTilesInARectangle(_map, _targetTile, _size, false, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }
        private void ExtractRectangleOutline()
        {
            _extractedTiles = Extraction.GetTilesOnARectangleOutline(_map, _targetTile, _size, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }
        private void ExtractCone()
        {
            _extractedTiles = Extraction.GetTilesInACone(_map, _targetTile, _radius, _angle, _direction, false, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }
        private void ExtractLine()
        {
            _extractedTiles = Extraction.GetTilesOnALine(_map, _targetTile, _radius, _direction, _allowDiagonals, _favorVertical, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }
        private void ExtractNeighbor()
        {
            Extraction.GetTileNeighbour(_map, _targetTile, _direction, out Tile neighbor);
            _extractedTiles = new Tile[] { neighbor };
            neighbor.TileMode = TileMode.EXTRACTED;
        }
        private void ExtractOrthoNeighbors()
        {
            _extractedTiles = Extraction.GetTileOrthogonalsNeighbours(_map, _targetTile, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }
        private void ExtractDiagonalsNeighbors()
        {
            _extractedTiles = Extraction.GetTileDiagonalsNeighbours(_map, _targetTile, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }
        private void ExtractNeighbors()
        {
            _extractedTiles = Extraction.GetTileNeighbours(_map, _targetTile, false);
            foreach (Tile tile in _extractedTiles)
            {
                tile.TileMode = TileMode.EXTRACTED;
            }
        }

        private void ClearTiles()
        {
            if (_extractedTiles != null)
            {
                foreach (Tile tile in _extractedTiles)
                {
                    tile.TileMode = TileMode.FLOOR;
                }
            }
        }
    }
}