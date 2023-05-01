using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid3DHelper.APIDemo.Extraction
{
    public enum TileMode
    {
        FADE,
        SEMIFADE,
        OPAQUE
    }
    public class Tile : MonoBehaviour, ITile3D
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _semiFade;
        [SerializeField] private Material _fade;
        [SerializeField] private Material _opaque;
        private TileMode _tileMode;
        public bool IsWalkable { get => false; }

        public float Weight => 1f;

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }
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
                    default:
                        break;
                }
            }
        }
    }
}