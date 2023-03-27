using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTS_Demo
{
    public class MoveTo : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _easing;
        private Vector3 _current;
        private Vector3 _target;
        public void StartMove(Vector3 target)
        {
            _current = transform.position;
            _target = new Vector3(target.x, transform.position.y, target.z);
        }
        public void DoMove(float progress)
        {
            transform.position = Vector3.Lerp(_current, _target, _easing.Evaluate(progress));
        }
        public void StopMove()
        {
            transform.position = _target;
        }
    }
}