using KevinCastejon.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TD_Demo
{
    public class Floor : MonoBehaviour, ITile
    {
        [SerializeField] private Material _floorMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Material _hoverMat;
        [SerializeField] private Material _goalMat;
        [SerializeField] private bool _isWalkable;
        [SerializeField] private bool _isGoal;
        private int _x;
        private int _y;
        private Renderer _renderer;
        private Transform _arrow;
        private Floor _next;

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
        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }
        public bool IsGoal
        {
            get
            {
                return _isGoal;
            }

            set
            {
                _isGoal = value;
                if (value)
                {
                    _isWalkable = true;
                    RefreshVisual();
                }
            }
        }
        public Floor Next
        {
            get
            {
                return _next;
            }

            set
            {
                _next = value;
                if (!_isWalkable || _next == null || _next == this)
                {
                    _arrow.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    _arrow.GetChild(0).gameObject.SetActive(true);
                    _arrow.rotation = Quaternion.LookRotation(_next.transform.position - transform.position);
                }
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
        private void OnMouseOver()
        {
            if (_isWalkable && !_isGoal)
            {
                _renderer.material = _hoverMat;
            }
        }

        private void OnMouseExit()
        {
            if (_isWalkable && !_isGoal)
            {
                _renderer.material = _floorMat;
            }
        }

        private void OnValidate()
        {
            RefreshVisual();
        }
        private void RefreshVisual()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }
            if (_isGoal)
            {
                _renderer.material = _goalMat;
            }
            else if (_isWalkable)
            {
                _renderer.material = _floorMat;
            }
            else
            {
                _renderer.material = _wallMat;
            }

        }
    }
}
