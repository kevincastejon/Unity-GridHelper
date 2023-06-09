using KevinCastejon.GridHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TD_Demo
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private Character _charPrefab;
        private Floor[,] _map = new Floor[12, 11];
        private Floor _goalTile;
        private Camera _camera;
        PathMap<Floor> _pathMap;

        public void Awake()
        {
            Floor[] allFloors = GetComponentsInChildren<Floor>();
            // Referencing tiles into grid a dirty way (by position)
            foreach (Floor floor in allFloors)
            {
                int x = Mathf.RoundToInt(floor.transform.position.x);
                int y = Mathf.Abs(Mathf.RoundToInt(floor.transform.position.z));
                _map[y, x] = floor;
                floor.X = x;
                floor.Y = y;
                if (_map[y, x].IsGoal)
                {
                    _goalTile = _map[y, x];
                }
            }
        }
        private void Start()
        {
            // Referencing the camera
            _camera = Camera.main;
            _pathMap = Pathfinding.GeneratePathMap(_map, _goalTile);
        }
        private void Update()
        {
            // Detecting click on tile
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && Input.GetMouseButtonDown(0))
            {
                // Retrieving the Floor component
                Floor hitFloor = hit.collider.GetComponent<Floor>();
                if (hitFloor.IsWalkable)
                {
                    Character ch = Instantiate(_charPrefab, new Vector3(hitFloor.transform.position.x, 1.5f, hitFloor.transform.position.z), Quaternion.identity);
                    ch.Init(hitFloor, _pathMap);
                }
            }
        }
    }
}
