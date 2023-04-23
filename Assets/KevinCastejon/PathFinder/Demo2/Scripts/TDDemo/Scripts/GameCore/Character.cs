using KevinCastejon.Pathfinding2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TD_Demo2
{

    public class Character : MonoBehaviour
    {
        Floor _target;
        PathMap _pathMap;
        public void Init(Floor startFloor, PathMap pathMap)
        {
            _target = (Floor)pathMap.GetNodeFromTile(startFloor).Next.Tile;
            _pathMap = pathMap;
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.transform.position.x, 1.5f, _target.transform.position.z), 2.5f * Time.deltaTime);
            if (Vector3.Distance(transform.position, new Vector3(_target.transform.position.x, 1.5f, _target.transform.position.z)) < 0.1f)
            {
                Node node = _pathMap.GetNodeFromTile(_target);
                if (_target == (Floor)node.Next.Tile)
                {
                    Destroy(gameObject);
                }
                else
                {
                    _target = (Floor)node.Next.Tile;
                }
            }
        }
    }

}