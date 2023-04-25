using KevinCastejon.GridHelper;
using System.Linq;
using UnityEngine;

namespace RTS_Demo
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private string _name;
        [SerializeField] private bool _isIA;
        [SerializeField] private bool _allowDiagonals;
        [SerializeField] private int _maxMovement;
        [SerializeField] private int _attackRange;
        [SerializeField] private int _attackDamage;
        [SerializeField] private int _health;
        [SerializeField] private Timer _stepTimer;
        [SerializeField] private Transform _bulletPrefab;
        private PathMap<Floor> _pathMap;
        private Floor[] _accessibleTiles;
        private Floor _currentTile;
        private Floor[] _path;
        private Floor _target;
        private Transform _bullet;
        private GridController _gridController;
        private int _currentPathIndex;
        private bool _hasMoved;
        private bool _hasAttacked;
        private bool _isPreparingMove;
        private bool _isPreparingAttack;
        private bool _isMoving;
        private bool _isAttacking;

        public string Name { get => _name; }
        public bool IsIA { get => _isIA; }
        public Floor[] AccessibleTiles { get => _accessibleTiles.Where(f => f.Character == null).ToArray(); set => _accessibleTiles = value; }
        public bool HasPath { get => _path != null && _path[_path.Length - 1] != CurrentTile; }
        public bool HasFinished { get => HasAttacked && HasMoved; }
        public bool HasMoved { get => _hasMoved; set => _hasMoved = value; }
        public bool HasAttacked { get => _hasAttacked; set => _hasAttacked = value; }
        public bool AllowDiagonals { get => _allowDiagonals; }
        public int MaxMovement { get => _maxMovement; }
        public int AttackRange { get => _attackRange; }
        public int Health { get => _health; set => _health = Mathf.Max(0, value); }
        public Floor CurrentTile { get => _currentTile; set => _currentTile = value; }
        public PathMap<Floor> PathMap
        {
            get
            {
                return _pathMap;
            }

            set
            {
                _pathMap = value;
            }
        }
        public bool IsPreparingMove { get => _isPreparingMove; set => _isPreparingMove = value; }
        public bool IsPreparingAttack { get => _isPreparingAttack; set => _isPreparingAttack = value; }
        public bool IsMoving { get => _isMoving; set => _isMoving = value; }
        public bool IsAttacking { get => _isAttacking; set => _isAttacking = value; }
        public bool IsStepOver { get => _stepTimer.IsCompleted; }
        public bool AllStepsOver { get => _currentPathIndex == _path.Length - 1; }

        private void Awake()
        {
            _gridController = FindObjectOfType<GridController>();
        }
        public void StartAttacking(Floor target)
        {
            _target = target;
            _bullet = Instantiate(_bulletPrefab);
            _stepTimer.Start();
        }
        public void DoAttacking()
        {
            if (_hasAttacked)
            {
                return;
            }
            _bullet.position = Vector3.Lerp(new Vector3(transform.position.x, 1.5f, transform.position.z), new Vector3(_target.transform.position.x, 1.5f, _target.transform.position.z), _stepTimer.Progress);
            if (_stepTimer.IsCompleted)
            {
                _stepTimer.Stop();
                Destroy(_bullet.gameObject);
                _hasAttacked = true;
                _target.Character.Health -= _attackDamage;
            }
        }

        public void StartMoving(Floor destination)
        {
            _path = _pathMap.GetPathFromTarget(destination).Skip(1).ToArray();
        }

        public void StartStepMove()
        {
            _stepTimer.Start();
        }
        public void DoStepMove()
        {
            transform.position = Vector3.Lerp(new Vector3(_currentTile.transform.position.x, transform.position.y, _currentTile.transform.position.z), new Vector3(_path[_currentPathIndex].transform.position.x, transform.position.y, _path[_currentPathIndex].transform.position.z), _stepTimer.Progress);
        }
        public void StopStepMove()
        {
            transform.position = new Vector3(_path[_currentPathIndex].transform.position.x, transform.position.y, _path[_currentPathIndex].transform.position.z);
            _currentTile.Character = null;
            _currentTile = _path[_currentPathIndex];
            _path[_currentPathIndex].Character = this;
        }
        public void SwitchToNextTileStep()
        {
            _currentPathIndex++;
        }
        public void MovementOver()
        {
            _path = null;
            _currentPathIndex = 0;
            _hasMoved = true;
        }
    }
}
