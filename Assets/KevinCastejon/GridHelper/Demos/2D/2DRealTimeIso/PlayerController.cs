using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Grid2DHelper.Demos.RealtimeIso
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private UnityEvent _onTileChange;
        [SerializeField] private Transform _laser;
        private CharacterController _characterController;
        private Vector2Int _currentPosition;

        public Vector2Int CurrentPosition { get => _currentPosition; }
        public Transform Laser { get => _laser; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _currentPosition = ShiftAxes(Vector3Int.RoundToInt(transform.position));
        }
        private void Update()
        {
            Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            direction = Vector3.ClampMagnitude(direction, 1f);
            _characterController.Move(_speed * Time.deltaTime * direction);
            Vector2Int actualPos = ShiftAxes(Vector3Int.RoundToInt(transform.position));
            if (actualPos != _currentPosition)
            {
                _currentPosition = actualPos;
                _onTileChange.Invoke();
            }
        }

        private Vector2Int ShiftAxes(Vector3Int vector)
        {
            return new Vector2Int(vector.x, vector.z);
        }
    }
}