using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TD_Demo
{

    public class Character : MonoBehaviour
    {
        Floor _target;
        public void SetStartTile(Floor floor)
        {
            _target = floor.Next;
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.transform.position.x, 1.5f, _target.transform.position.z), 2.5f * Time.deltaTime);
            if (Vector3.Distance(transform.position, new Vector3(_target.transform.position.x, 1.5f, _target.transform.position.z)) < 0.1f)
            {
                if (_target == _target.Next)
                {
                    Destroy(gameObject);
                }
                else
                {
                    _target = _target.Next;
                }
            }
        }
    }

}