using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Grid2DHelper.Demos.RealtimeIsoBakedGridMap
{
    public class Tile : MonoBehaviour, ITile
    {
        [SerializeField] private GameObject _wall;
        [SerializeField] private GameObject _fog;
        [SerializeField] private GameObject _shoot;
        [SerializeField] private GameObject _aim;
        [SerializeField] private bool _isWalkable = true;
        [SerializeField] private int _x;
        [SerializeField] private int _y;
        private bool _isHighlighted;
        private bool _isShoot;
        private bool _isAim;

        public bool IsWalkable
        {
            get
            {
                return _isWalkable;
            }
            set
            {
                _isWalkable = value;
                _wall.SetActive(!_isWalkable);
                _fog.SetActive(_isWalkable);
            }
        }
        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }
            set
            {
                if (_isHighlighted == value)
                {
                    return;
                }
                _isHighlighted = value;
                _fog.SetActive(!_isHighlighted);
            }
        }
        public bool IsShoot
        {
            get
            {
                return _isShoot;
            }
            set
            {
                if (_isShoot == value)
                {
                    return;
                }
                _isShoot = value;
                _shoot.SetActive(_isShoot);
            }
        }
        public bool IsAim
        {
            get
            {
                return _isAim;
            }
            set
            {
                if (_isAim == value)
                {
                    return;
                }
                _isAim = value;
                _aim.SetActive(_isAim);
            }
        }

        public float Weight => 1f;

        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }

        [ContextMenu("SetWalkable")]
        public void SetWalkable()
        {
            IsWalkable = true;
        }
        [ContextMenu("SetNonWalkable")]
        public void SetNonWalkable()
        {
            IsWalkable = false;
        }
    }
}