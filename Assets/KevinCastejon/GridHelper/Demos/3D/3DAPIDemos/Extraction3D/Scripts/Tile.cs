using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid3DHelper.APIDemos.Extraction
{
    public enum TileMode
    {
        AIR,
        EXTRACTED,
        TARGET,
        WALL
    }
    public class Tile : MonoBehaviour, ITile3D
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _airMat;
        [SerializeField] private Material _extractedMat;
        [SerializeField] private Material _targetMat;
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