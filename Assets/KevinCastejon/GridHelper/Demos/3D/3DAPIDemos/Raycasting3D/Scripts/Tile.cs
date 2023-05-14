using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid3DHelper.APIDemos.Raycasting
{
    public enum TileMode
    {
        AIR,
        PATH,
        TARGET,
        STOP,
        WALL
    }
    public class Tile : MonoBehaviour, ITile3D
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _pathMat;
        [SerializeField] private Material _airMat;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _stopMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] [HideInInspector] private TileMode _tileMode;
        public bool IsWalkable { get => _tileMode != TileMode.WALL; }

        public float Weight => 1f;

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        [ContextMenu("Set To Wall")]
        public void SetToWall()
        {
            TileMode = TileMode.WALL;
        }
        [ContextMenu("Set To Walkable")]
        public void SetToWalkable()
        {
            TileMode = TileMode.AIR;
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
                    case TileMode.AIR:
                        _renderer.material = _airMat;
                        break;
                    case TileMode.PATH:
                        _renderer.material = _pathMat;
                        break;
                    case TileMode.TARGET:
                        _renderer.material = _targetMat;
                        break;
                    case TileMode.STOP:
                        _renderer.material = _stopMat;
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