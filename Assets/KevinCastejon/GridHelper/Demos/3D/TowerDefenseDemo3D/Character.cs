using KevinCastejon.GridHelper3D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Grid3DHelper.Demos3D.TowerDefenseDemo3D
{

    public class Character : MonoBehaviour
    {
        PathMap3D<Tile> _pathMap;
        Tile _target;
        public void Init(Tile startFloor, PathMap3D<Tile> pathMap)
        {
            _pathMap = pathMap;
            _target = _pathMap.GetNextTileFromTile(startFloor);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.transform.position.x, _target.transform.position.y + 0.5f, _target.transform.position.z), 2.5f * Time.deltaTime);
            if (Vector3.Distance(transform.position, new Vector3(_target.transform.position.x, _target.transform.position.y + 0.5f, _target.transform.position.z)) < 0.1f)
            {
                Tile nextTile = _pathMap.GetNextTileFromTile(_target);
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