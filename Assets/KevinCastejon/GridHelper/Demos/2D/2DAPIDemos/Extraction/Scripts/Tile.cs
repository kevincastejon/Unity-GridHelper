using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid2DHelper.APIDemo.ExtractionDemo
{
    public enum TileMode
    {
        FLOOR,
        EXTRACTED,
        TARGET,
        WALL
    }
    public class Tile : MonoBehaviour, ITile
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _extractedMat;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] [HideInInspector] private TileMode _tileMode;
        public bool IsWalkable { get => _tileMode != TileMode.WALL; }

        public float Weight => 1f;

        public int X { get; set; }

        public int Y { get; set; }

        [ContextMenu("Set To Wall")]
        public void SetToWall()
        {
            TileMode = TileMode.WALL;
        }
        [ContextMenu("Set To Walkable")]
        public void SetToWalkable()
        {
            TileMode = TileMode.FLOOR;
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
                    case TileMode.EXTRACTED:
                        _renderer.material = _extractedMat;
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