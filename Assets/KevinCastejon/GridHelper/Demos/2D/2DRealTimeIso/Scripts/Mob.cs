using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid2DHelper.Demos.RealtimeIso
{
    public class Mob : MonoBehaviour
    {
        [SerializeField] private float _speed = 1f;
        private GridMap _map;
        private Vector2Int _currentPosition;
        private Vector2Int _nextPosition;
        private Rigidbody _rigidbody;
        private void Awake()
        {
            _map = FindObjectOfType<GridMap>();
            _rigidbody = GetComponent<Rigidbody>();
        }
        private void Start()
        {
            _currentPosition = ShiftAxes(Vector3Int.RoundToInt(transform.position));
            _nextPosition = _map.GetNextPositionToPlayer(_currentPosition);
        }
        private void FixedUpdate()
        {
            Vector3 dir = new Vector3(_nextPosition.x, 0f, _nextPosition.y) - transform.position;
            _rigidbody.MovePosition(_rigidbody.position + (_speed * Time.deltaTime * dir.normalized));
            Vector2Int actualPos = ShiftAxes(Vector3Int.RoundToInt(transform.position));
            if (actualPos != _currentPosition)
            {
                _currentPosition = actualPos;
                _nextPosition = _map.GetNextPositionToPlayer(_currentPosition);
            }
        }
        private Vector2Int ShiftAxes(Vector3Int vector)
        {
            return new Vector2Int(vector.x, vector.z);
        }
        private void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                Destroy(gameObject);
            }
        }
    }
}