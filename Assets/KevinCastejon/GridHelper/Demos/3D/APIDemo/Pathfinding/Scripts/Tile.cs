using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid3DHelper.APIDemo.Pathfinding
{
    public enum TileMode
    {
        FADE,
        SEMIFADE,
        OPAQUE,
        OPAQUE_VARIANT
    }
    public class Tile : MonoBehaviour, ITile3D
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _semiFade;
        [SerializeField] private Material _fade;
        [SerializeField] private Material _opaque;
        [SerializeField] private Material _opaqueVariant;
        [SerializeField] [HideInInspector] private TileMode _tileMode;
        public bool IsWalkable { get => _tileMode != TileMode.OPAQUE_VARIANT; }

        public float Weight => 1f;

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        [ContextMenu("Set To Wall")]
        public void SetToWall()
        {
            TileMode = TileMode.OPAQUE_VARIANT;
        }
        [ContextMenu("Set To Walkable")]
        public void SetToWalkable()
        {
            TileMode = TileMode.FADE;
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
                    case TileMode.FADE:
                        _renderer.material = _fade;
                        break;
                    case TileMode.SEMIFADE:
                        _renderer.material = _semiFade;
                        break;
                    case TileMode.OPAQUE:
                        _renderer.material = _opaque;
                        break;
                    case TileMode.OPAQUE_VARIANT:
                        _renderer.material = _opaqueVariant;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}