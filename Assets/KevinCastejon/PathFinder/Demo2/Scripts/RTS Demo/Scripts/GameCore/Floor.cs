using KevinCastejon.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTS_Demo2
{
    public class Floor : MonoBehaviour, ITile
    {
        [SerializeField] private Material _targetMat;
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Material _pathMat;
        [SerializeField] private Material _hoveredFloorMat;
        [SerializeField] private Material _hoveredTargetMat;
        [SerializeField] private Material _hoveredPathMat;
        [SerializeField] private bool _isWalkable;
        private bool _isPath;
        private bool _isInViewRange = true;
        private bool _isHovered;
        private int _x;
        private int _y;
        private Renderer _renderer;
        private Transform _arrow;
        private Character _character;
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
                RefreshVisual();
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
                RefreshVisual();
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
                RefreshVisual();
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
        public Character Character { get => _character; set => _character = value; }
        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }
        public bool IsInViewRange
        {
            get
            {
                return _isInViewRange;
            }

            set
            {
                _isInViewRange = value;
                RefreshVisual();
            }
        }

        private void Awake()
        {
            _arrow = transform.GetChild(0);
            _renderer = GetComponent<Renderer>();
        }
        private void Start()
        {
            RefreshVisual();
        }
        private void OnValidate()
        {
            RefreshVisual();
        }
        private void RefreshVisual()
        {
            if (!_isWalkable)
            {
                GetComponent<Renderer>().material = _wallMat;
            }
            else if (_isPath)
            {
                if (_isHovered)
                {
                    GetComponent<Renderer>().material = _hoveredPathMat;
                }
                else
                {
                    GetComponent<Renderer>().material = _pathMat;
                }

            }
            else if (_character)
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
                else
                {
                    GetComponent<Renderer>().material = _floorMat;
                }
            }

            if (Application.isPlaying && _renderer)
            {
                float alpha = _isInViewRange ? 1f : 0.5f;
                _renderer.material.color = new Color(_renderer.material.color.r, _renderer.material.color.g, _renderer.material.color.b, alpha);
            }


        }
    }
}
