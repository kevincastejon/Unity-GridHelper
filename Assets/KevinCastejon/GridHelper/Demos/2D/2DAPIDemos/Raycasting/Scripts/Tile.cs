using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid2DHelper.APIDemo.RaycastingDemo
{
    public enum TileMode
    {
        FLOOR,
        LINE,
        TARGET,
        WALL
    }
    public class Tile : MonoBehaviour, ITile
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _lineMat;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _wallMat;
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
                    case TileMode.TARGET:
                        _renderer.material = _targetMat;
                        break;
                    case TileMode.LINE:
                        _renderer.material = _lineMat;
                        break;
                    case TileMode.WALL:
                        _renderer.material = _wallMat;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}