using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Grid2DHelper.APIDemo.PathfindingDemo
{
    public enum TileMode
    {
        FLOOR,
        ACCESSIBLE,
        PATH,
        TARGET,
        WALL
    }
    public class Tile : MonoBehaviour, ITile
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _accessibleMat;
        [SerializeField] private Material _pathMat;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Transform _directionLabel;
        [SerializeField] private TextMeshPro _distanceLabel;
        [SerializeField] private bool _isWalkable = true;
        [SerializeField] [HideInInspector] private TileMode _tileMode;
        public bool IsWalkable
        {
            get
            {
                return _isWalkable;
            }

            set
            {
                _isWalkable = value;
                if (!_isWalkable)
                {
                    TileMode = TileMode.WALL;
                }
                else
                {
                    TileMode = TileMode.FLOOR;
                }
            }
        }

        public float Weight => 1f;

        public int X { get; set; }

        public int Y { get; set; }

        [ContextMenu("Set To Wall")]
        public void SetToWall()
        {
            IsWalkable = false;
        }
        [ContextMenu("Set To Walkable")]
        public void SetToWalkable()
        {
            IsWalkable = true;
        }
        public TileMode TileMode
        {
            get
            {
                return _tileMode;
            }

            set
            {
                _tileMode = value;
                switch (_tileMode)
                {
                    case TileMode.FLOOR:
                        _renderer.material = _floorMat;
                        break;
                    case TileMode.ACCESSIBLE:
                        _renderer.material = _accessibleMat;
                        break;
                    case TileMode.TARGET:
                        _renderer.material = _targetMat;
                        break;
                    case TileMode.PATH:
                        _renderer.material = _pathMat;
                        break;
                    case TileMode.WALL:
                        _renderer.material = _wallMat;
                        break;
                    default:
                        break;
                }
            }
        }
        public void ShowDistanceAndDirection(float distance, Vector2Int direction)
        {
            _directionLabel.gameObject.SetActive(true);
            _distanceLabel.gameObject.SetActive(true);
            _distanceLabel.text = distance.ToString("F2");
            _directionLabel.right = (Vector2)direction;
        }
        public void HideDistanceAndDirection()
        {
            _directionLabel.rotation = Quaternion.identity;
            _directionLabel.gameObject.SetActive(false);
            _distanceLabel.gameObject.SetActive(false);
        }
    }
}