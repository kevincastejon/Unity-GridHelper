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
        private int _verticalDirection;
        public void Init(Tile startFloor, PathMap3D<Tile> pathMap)
        {
            _pathMap = pathMap;
            _target = _pathMap.GetNextTileFromTile(startFloor);
            _verticalDirection = _target.Y - startFloor.Y;
            if (_verticalDirection == 1)
            {
                transform.position += Vector3.up;
            }
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.transform.position.x, transform.position.y, _target.transform.position.z), 2.5f * Time.deltaTime);
            if (Vector3.Distance(transform.position, new Vector3(_target.transform.position.x, transform.position.y, _target.transform.position.z)) < 0.1f)
            {
                Tile nextTile = _pathMap.GetNextTileFromTile(_target);
                if (_target == nextTile)
                {
                    Destroy(gameObject);
                }
                else
                {
                    if (_verticalDirection == -1)
                    {
                        transform.position -= Vector3.up;
                    }
                    _verticalDirection = nextTile.Y - _target.Y;
                    _target = nextTile;
                    if (_verticalDirection == 1)
                    {
                        transform.position += Vector3.up;
                    }
                }
            }
        }
    }

}