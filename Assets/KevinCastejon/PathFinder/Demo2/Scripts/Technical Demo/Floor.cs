using KevinCastejon.Pathfinding2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Technical_Demo2
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
        [SerializeField] private bool _isPath;
        [SerializeField] private bool _isHovered;
        private int _x;
        private int _y;
        private Transform _arrow;
        private Vector2 _flow;

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
        public bool IsHovered
        {
            get
            {
                return _isHovered;
            }
            set
            {
                _isHovered = value;
                SynchronizeMaterial();
            }
        }
        public Vector2 Flow
        {
            get
            {
                return _flow;
            }
            set
            {
                _flow = value;
                if (!_isWalkable || _flow == Vector2.zero)
                {
                    _arrow.gameObject.SetActive(false);
                }
                else
                {
                    _arrow.gameObject.SetActive(true);
                    _arrow.rotation = Quaternion.LookRotation(_flow);
                }
            }
        }
        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }

        public float Weight => 1f;

        private void OnValidate()
        {
            SynchronizeMaterial();
        }
        private void Awake()
        {
            _arrow = transform.GetChild(0);
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
