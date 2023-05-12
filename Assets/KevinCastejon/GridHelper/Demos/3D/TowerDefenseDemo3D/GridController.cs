using KevinCastejon.GridHelper3D;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid3DHelper.Demos3D.TowerDefenseDemo3D
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private Character _charPrefab;
        private Tile[,,] _map = new Tile[4, 11, 12];
        private Tile _goalTile;
        private Camera _camera;
        PathMap3D<Tile> _pathMap;

        public void Awake()
        {
            Tile[] allFloors = GetComponentsInChildren<Tile>();
            // Referencing tiles into grid a dirty way (by position)
            foreach (Tile floor in allFloors)
            {
                int x = Mathf.RoundToInt(floor.transform.position.x);
                int y = Mathf.RoundToInt(floor.transform.position.y);
                int z = Mathf.RoundToInt(floor.transform.position.z);
                _map[y, x, z] = floor;
                floor.X = x;
                floor.Y = y;
                floor.Z = z;
                if (_map[y, x, z].IsGoal)
                {
                    _goalTile = _map[y, x, z];
                }
            }
        }
        private void Start()
        {
            // Referencing the camera
            _camera = Camera.main;
            _pathMap = Pathfinding3D.GeneratePathMap(_map, _goalTile, 0f, new Pathfinding3DPolicy(EdgesDiagonalsPolicy.DIAGONAL_2FREE, 1.41421354f, EdgesDiagonalsPolicy.DIAGONAL_1FREE, 1.14121354f, VerticesDiagonalsPolicy.DIAGONAL_6FREE, 1.73205078f, MovementPolicy.WALL_BELOW));
        }
        private void Update()
        {
            // Detecting click on tile
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && Input.GetMouseButton(0))
            {
                // Retrieving the Floor component
                Tile hitFloor = hit.collider.GetComponent<Tile>();
                Extraction3D.GetTileNeighbour(_map, hitFloor, Vector3Int.up, out Tile targetFloor);
                if (_pathMap.IsTileAccessible(targetFloor))
                {
                    Character ch = Instantiate(_charPrefab, new Vector3(targetFloor.transform.position.x, targetFloor.transform.position.y + 0.5f, targetFloor.transform.position.z), Quaternion.identity);
                    ch.Init(targetFloor, _pathMap);
                }
            }
        }
    }
}
