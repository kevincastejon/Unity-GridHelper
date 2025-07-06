using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
namespace Grid2DHelper.Demos.RealtimeIsoBakedGridMap
{
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private int _mapWidth;
        [SerializeField] private int _mapHeight;
        [SerializeField] private Mob _mobPrefab;
        [SerializeField] private float _spawnDelay;
        [SerializeField] private Transform _mobs;
        private float _nextSpawnTime;
        private Tile[,] _map;
        private Tile[] _highlightedTiles = new Tile[0];
        private Tile[] _shootTiles = new Tile[0];
        private PathGrid<Tile> _pathGrid;
        private PlayerController _player;
        private Camera _camera;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        public PathGrid<Tile> PathGrid { get => _pathGrid; }
        public Tile[,] Map { get => _map; }
        private void Awake()
        {
            _player = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
            _map = new Tile[_mapHeight, _mapWidth];
            _camera = Camera.main;
        }
        private async void Start()
        {
            Tile[] tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
            foreach (Tile tile in tiles)
            {
                tile.X = Mathf.RoundToInt(tile.transform.position.x);
                tile.Y = Mathf.RoundToInt(tile.transform.position.z);
                _map[tile.Y, tile.X] = tile;
            }
            System.Progress<float> progressIndicator = new System.Progress<float>((progress) =>
            {
                _text.text = "Calculating all paths between each tiles on the map\n" + (progress * 100).ToString("F0") + "%";
            });
            try
            {
                _pathGrid = await Pathfinding.GeneratePathGridAsync(_map, new PathfindingPolicy(), MajorOrder.DEFAULT, progressIndicator, _cts.Token);
                Destroy(_text.transform.parent.parent.gameObject);
                _player.gameObject.SetActive(true);
                OnPlayerTileChange();
            }
            catch (System.Exception)
            {
                Debug.Log("PathGrid generation was cancelled");
            }
        }

        private void SpawnMob()
        {
            bool done = false;
            while (!done)
            {
                Tile mobTile = _map[Random.Range(0, _map.GetLength(0)), Random.Range(0, _map.GetLength(1))];
                if (mobTile.IsWalkable && !GridUtils.TileEquals(mobTile, GetPlayerTile()) && _pathGrid.IsPath(mobTile, GetPlayerTile()))
                {
                    done = true;
                    Instantiate(_mobPrefab, new Vector3(mobTile.X, 0f, mobTile.Y), Quaternion.identity, _mobs);
                }
            }
        }

        private void Update()
        {
            if (_pathGrid == null)
            {
                return;
            }
            if (Time.time >= _nextSpawnTime)
            {
                SpawnMob();
                _nextSpawnTime = Time.time + _spawnDelay;
            }
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && Input.GetMouseButton(1))
            {
                _player.Laser.gameObject.SetActive(true);
                Vector3 direction = (hit.point - _player.Laser.position).normalized;
                Vector2 direction2D = new Vector2(direction.x, direction.z);
                ClearShootTiles();
                _shootTiles = Raycasting.GetLineOfSight(_map, GetPlayerTile(), 20, direction2D, false, false, false, true);
                foreach (Tile tile in _shootTiles)
                {
                    if (Input.GetMouseButton(0))
                    {
                        tile.IsShoot = true;
                        foreach (Transform mob in _mobs)
                        {
                            if (GridUtils.TileEquals(_map[Mathf.RoundToInt(mob.position.z), Mathf.RoundToInt(mob.position.x)], tile))
                            {
                                Destroy(mob.gameObject);
                            }
                        }
                    }
                    else
                    {
                        tile.IsAim = true;
                    }
                }
                _player.Laser.rotation = Quaternion.LookRotation(direction.normalized);
                if (_shootTiles.Length > 0)
                {
                    _player.Laser.localScale = new Vector3(_player.Laser.localScale.x, _player.Laser.localScale.y, Vector3.Distance(_player.transform.position, _shootTiles[_shootTiles.Length - 1].transform.position));
                }
            }
            else
            {
                ClearShootTiles();
                _player.Laser.gameObject.SetActive(false);
            }
        }
        public void OnPlayerTileChange()
        {
            ClearTiles();
            _highlightedTiles = Raycasting.GetConeOfVision(_map, GetPlayerTile(), 20, 360f, 0f);
            foreach (Tile tile in _highlightedTiles)
            {
                tile.IsHighlighted = true;
            }
        }
        private void ClearTiles()
        {
            foreach (Tile tile in _highlightedTiles)
            {
                tile.IsHighlighted = false;
            }
        }
        private void ClearShootTiles()
        {
            foreach (Tile tile in _shootTiles)
            {
                tile.IsAim = false;
                tile.IsShoot = false;
            }
        }
        private Tile GetPlayerTile()
        {
            return _map[_player.CurrentPosition.y, _player.CurrentPosition.x];
        }
        private void OnApplicationQuit()
        {
            _cts.Cancel();
        }
        public Vector2Int GetNextPositionToPlayer(Vector2Int currentPosition)
        {
            _pathGrid.GetNextTileFromTile(_map[currentPosition.y, currentPosition.x], GetPlayerTile(),out Tile nextTile);
            return new Vector2Int(nextTile.X, nextTile.Y);
        }
    }
}
