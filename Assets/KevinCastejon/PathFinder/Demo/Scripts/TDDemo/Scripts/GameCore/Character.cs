using KevinCastejon.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TD_Demo
{

    public class Character : MonoBehaviour
    {
        PathMap<Floor> _pathMap;
        Floor _target;
        public void Init(Floor startFloor, PathMap<Floor> pathMap)
        {
            _pathMap = pathMap;
            _target = _pathMap.GetNodeFromTile(startFloor).Next.Tile;
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.transform.position.x, 1.5f, _target.transform.position.z), 2.5f * Time.deltaTime);
            if (Vector3.Distance(transform.position, new Vector3(_target.transform.position.x, 1.5f, _target.transform.position.z)) < 0.1f)
            {
                Floor nextTile = _pathMap.GetNodeFromTile(_target).Next.Tile;
                if (_target == nextTile)
                {
                    Destroy(gameObject);
                }
                else
                {
                    _target = nextTile;
                }
            }
        }
    }

}