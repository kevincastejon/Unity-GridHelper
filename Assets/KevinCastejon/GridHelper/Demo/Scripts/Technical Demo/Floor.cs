using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Technical_Demo
{
    public class Floor : MonoBehaviour, ITile
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Material _pathMat;
        [SerializeField] private Material _hoveredFloorMat;
        [SerializeField] private Material _hoveredTargetMat;
        [SerializeField] private bool _isWalkable;
        [SerializeField] private bool _isTarget;
        [SerializeField] private bool _isHovered;
        private bool _isPath;
        private int _x;
        private int _y;
        private TextMeshPro _label;

        public bool IsWalkable
        {
            get
            {
                return _isWalkable;
            }
            set
            {
                _isWalkable = value;
                SynchronizeMaterial();
            }
        }
        public float Weight { get => 1f; }
        public bool IsTarget
        {
            get
            {
                return _isTarget;
            }
            set
            {
                _isTarget = value;
                SynchronizeMaterial();
            }
        }
        public bool IsPath
        {
            get
            {
                return _isPath;
            }
            set
            {
                _isPath = value;
                SynchronizeMaterial();
            }
        }
        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }
        public TextMeshPro Label { get => _label; }

        private void OnValidate()
        {
            SynchronizeMaterial();
        }
        private void Awake()
        {
            _label = GetComponentInChildren<TextMeshPro>();
        }
        private void Start()
        {
            SynchronizeMaterial();
        }
        private void SynchronizeMaterial()
        {
            if (_isTarget)
            {
                if (_isHovered)
                {
                    GetComponent<Renderer>().material = _hoveredTargetMat;
                }
                else
                {
                    GetComponent<Renderer>().material = _targetMat;
                }
            }
            else
            {
                if (_isHovered)
                {
                    GetComponent<Renderer>().material = _hoveredFloorMat;
                }
                else if (!_isWalkable)
                {
                    GetComponent<Renderer>().material = _wallMat;
                }
                else if (_isPath)
                {
                    GetComponent<Renderer>().material = _pathMat;
                }
                else if (_isWalkable)
                {
                    GetComponent<Renderer>().material = _floorMat;
                }
            }
        }
    }
}
