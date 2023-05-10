using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TacticalDemo
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private string _name;
        [SerializeField] private Transform _bulletPrefab;
        private PathMap<Tile> _pathMap;
        private Tile[] _currentPath;
        private int _currentPathIndex;
        private bool _isAttacking;
        private Transform _bullet;

        public PathMap<Tile> PathMap { get => _pathMap; set => _pathMap = value; }
        public Tile CurrentTile { get => _pathMap.Target; }
        public bool IsMoving { get => _currentPath != null; }
        public string Name { get => _name; }

        private void Update()
        {
            if (_currentPath != null)
            {
                if (!_isAttacking)
                {
                    if (Vector3.Distance(ToFixedY(_currentPath[_currentPathIndex].transform.position), ToFixedY(transform.position)) <= 0.1f)
                    {
                        if (_currentPathIndex == _currentPath.Length - 1)
                        {
                            _currentPath = null;
                            _currentPathIndex = 0;
                        }
                        else
                        {
                            _currentPathIndex++;
                        }
                    }
                    else
                    {
                        transform.Translate((ToFixedY(_currentPath[_currentPathIndex].transform.position) - ToFixedY(transform.position)).normalized * Time.deltaTime * 4f);
                    }
                }
                else
                {
                    if (Vector3.Distance(ToFixedY(_currentPath[_currentPathIndex].transform.position), ToFixedY(_bullet.position)) <= 0.1f)
                    {
                        if (_currentPathIndex == _currentPath.Length - 1)
                        {
                            _currentPath = null;
                            _currentPathIndex = 0;
                            Destroy(_bullet.gameObject);
                        }
                        else
                        {
                            _currentPathIndex++;
                        }
                    }
                    else
                    {
                        _bullet.Translate((ToFixedY(_currentPath[_currentPathIndex].transform.position) - ToFixedY(_bullet.position)).normalized * Time.deltaTime * 8f);
                    }
                }
            }
        }

        public void Move(Tile[] tiles)
        {
            _currentPath = tiles;
            _isAttacking = false;
        }
        public void Attack(Tile tile)
        {
            _currentPath = new Tile[] { tile };
            _isAttacking = true;
            _bullet = Instantiate(_bulletPrefab, ToFixedY(transform.position, 1f), Quaternion.identity);
        }

        private Vector3 ToFixedY(Vector3 vector, float yValue = 0f)
        {
            return new Vector3(vector.x, yValue, vector.z);
        }
    }
}