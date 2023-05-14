using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace TacticalDemo
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
        [SerializeField] [HideInInspector] private TileState _tileState;

        public float Weight { get => 1f; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsWalkable { get => _tileState != TileState.WALL; }
        public bool IsTarget { get => _tileState == TileState.TARGET; }
        public bool IsPath { get => _tileState == TileState.PATH; }
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
        private void SynchronizeMaterial()
        {
            switch (_tileState)
            {
                case TileState.FLOOR:
                    _renderer.material = _floorMat;
                    break;
                case TileState.WALL:
                    _renderer.material = _wallMat;
                    break;
                case TileState.TARGET:
                    _renderer.material = _targetMat;
                    break;
                case TileState.PATH:
                    _renderer.material = _pathMat;
                    break;
                default:
                    break;
            }
        }
    }
}
