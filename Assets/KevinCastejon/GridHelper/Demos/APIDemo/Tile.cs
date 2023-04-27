using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace APIDemo
{
    public enum TileState
    {
        FLOOR,
        WALL,
        TARGET,
        PATH
    }
    public class Tile : MonoBehaviour, ITile
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Material _pathMat;
        [SerializeField] private TextMeshPro _label;
        [SerializeField] [HideInInspector] private TileState _tileState;

        public float Weight { get => 1f; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsWalkable { get => _tileState != TileState.WALL; }
        public bool IsTarget { get => _tileState == TileState.TARGET; }
        public bool IsPath { get => _tileState == TileState.PATH; }
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
                if (_tileState == TileState.WALL || _tileState == TileState.TARGET)
                {
                    HideDistanceAndDirection();
                }
            }
        }
        private void Awake()
        {
            _label.transform.parent.gameObject.SetActive(true);
        }
        [ContextMenu("Set To Wall", true)]
        public bool SetToWallValidation()
        {
            if (Application.isPlaying || TileState == TileState.TARGET)
            {
                return false;
            }
            return true;
        }
        [ContextMenu("Set To Wall")]
        public void SetToWall()
        {
            if (Application.isPlaying || TileState == TileState.TARGET)
            {
                return;
            }
            TileState = TileState.WALL;
        }
        [ContextMenu("Set To Floor", true)]
        public bool SetToFloorValidation()
        {
            if (Application.isPlaying)
            {
                return false;
            }
            return true;
        }
        [ContextMenu("Set To Floor")]
        public void SetToFloor()
        {
            if (Application.isPlaying)
            {
                return;
            }
            TileState = TileState.FLOOR;
        }
        [ContextMenu("Set To Target", true)]
        public bool SetToTargetValidation()
        {
            if (Application.isPlaying)
            {
                return false;
            }
            return true;
        }
        [ContextMenu("Set To Target")]
        public void SetToTarget()
        {
            if (Application.isPlaying)
            {
                return;
            }
            TileState = TileState.TARGET;
        }
        private void SynchronizeMaterial()
        {
            switch (_tileState)
            {
                case TileState.FLOOR:
                    GetComponent<Renderer>().material = _floorMat;
                    break;
                case TileState.WALL:
                    GetComponent<Renderer>().material = _wallMat;
                    break;
                case TileState.TARGET:
                    GetComponent<Renderer>().material = _targetMat;
                    break;
                case TileState.PATH:
                    GetComponent<Renderer>().material = _pathMat;
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
            if (_tileState == TileState.TARGET)
            {
                HideDistanceAndDirection();
                return;
            }
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
