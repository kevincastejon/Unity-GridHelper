using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid3DHelper.Demos3D.TowerDefenseDemo3D
{
    public class Tile : MonoBehaviour, ITile3D
    {
        [SerializeField] private Material _airMat;
        [SerializeField] private Material _wallMat;
        [SerializeField] private Material _goalMat;
        [SerializeField] private bool _isWalkable;
        [SerializeField] private bool _isGoal;
        [SerializeField] private BoxCollider _collider;
        private int _x;
        private int _y;
        private int _z;
        private Renderer _renderer;

        public float Weight { get => 1f; }
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
        public int Z { get => _z; set => _z = value; }
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

        private void Awake()
        {
            //_arrow = transform.GetChild(0);
            _renderer = GetComponent<Renderer>();
            _collider.enabled = !_isWalkable;
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
                _renderer.material = _airMat;
            }
            else
            {
                _renderer.material = _wallMat;
            }

        }
    }
}
