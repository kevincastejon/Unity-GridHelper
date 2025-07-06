using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid2DHelper.Demos.SideViewWEBGL_NoAsync
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _jumpForce = 3f;
        [SerializeField] private Transform _pivot;
        private Rigidbody2D _rigidbody;
        private float _direction;
        private bool _isGrounded = true;
        private Vector2Int _currentTile;
        public Vector2Int CurrentTile { get => _currentTile; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _currentTile = Vector2Int.RoundToInt(_pivot.position);
        }
        private void Update()
        {
            _direction = Input.GetAxisRaw("Horizontal");

            if (_direction > 0)
            {
                transform.right = Vector2.right;
            }
            else if (_direction < 0)
            {
                transform.right = Vector2.left;
            }
            if (_rigidbody.linearVelocity.y < 0.1f)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 0.2f);
                if (hits.Length == 2)
                {
                    _isGrounded = true;
                }
            }
            if (_isGrounded && Input.GetButtonDown("Jump"))
            {
                _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _isGrounded = false;
            }
            _currentTile = Vector2Int.RoundToInt(_pivot.position);
        }
        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = new Vector2(_direction * _speed, _rigidbody.linearVelocity.y);
        }
    }
}