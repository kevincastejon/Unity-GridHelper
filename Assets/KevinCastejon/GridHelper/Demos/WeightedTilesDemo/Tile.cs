using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace WeightedTilesDemo
{
    public enum TileState
    {
        PLAIN,
        FOREST,
    }
    public class Tile : MonoBehaviour, ITile
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _dirtMat;
        [SerializeField] private Material _sandMat;
        [SerializeField] private TextMeshPro _label;
        [SerializeField] [HideInInspector] private TileState _tileState;

        public bool IsWalkable { get => true; }
        public float Weight { get => _tileState == TileState.PLAIN ? GridMap.PLAIN_WEIGHT : GridMap.FOREST_WEIGHT; }
        public int X { get; set; }
        public int Y { get; set; }
        public TextMeshPro Label { get => _label; }
        public TileState TileState
        {
            get
            {
                return _tileState;
            }
            set
            {
                _tileState = value;
                SynchronizeMaterial();
            }
        }
        private void Awake()
        {
            _label.transform.parent.gameObject.SetActive(true);
        }
        [ContextMenu("Set To Dirt", true)]
        public bool SetToDirtValidation()
        {
            if (Application.isPlaying)
            {
                return false;
            }
            return true;
        }
        [ContextMenu("Set To Dirt")]
        public void SetToDirt()
        {
            if (Application.isPlaying)
            {
                return;
            }
            TileState = TileState.PLAIN;
        }
        [ContextMenu("Set To Sand", true)]
        public bool SetToSandValidation()
        {
            if (Application.isPlaying)
            {
                return false;
            }
            return true;
        }
        [ContextMenu("Set To Sand")]
        public void SetToSand()
        {
            if (Application.isPlaying)
            {
                return;
            }
            TileState = TileState.FOREST;
        }
        private void SynchronizeMaterial()
        {
            switch (_tileState)
            {
                case TileState.PLAIN:
                    GetComponent<Renderer>().material = _dirtMat;
                    break;
                case TileState.FOREST:
                    GetComponent<Renderer>().material = _sandMat;
                    break;
                default:
                    break;
            }
        }
        public void ShowDistance(float distance)
        {
            _label.transform.parent.right = Vector2.right;
            _label.text = distance.ToString("F1");
        }
        public void ShowDirection(Vector2Int direction)
        {
            _label.text = '\u2192'.ToString();
            _label.transform.parent.right = (Vector2)direction;
        }
        public void HideDistanceAndDirection()
        {
            transform.parent.rotation = Quaternion.identity;
            _label.text = "";
        }
    }
}
