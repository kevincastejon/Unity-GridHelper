using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Grid2DHelper.Demos.SideView
{
    public class GridController : MonoBehaviour
    {
        private Tilemap _tilemap;
        private Tile[,] _map;
        [SerializeField] private TextMeshProUGUI _generatingPanel;
        [SerializeField] private PlayerController _player;
        [SerializeField] private Follower _butterfly;
        [SerializeField] private Follower _spider;
        [SerializeField] private Follower _dog;
        [SerializeField] private MovementsPolicy _butterflyPolicy;
        [SerializeField] private MovementsPolicy _spiderPolicy;
        [SerializeField] private MovementsPolicy _dogPolicy;
        [SerializeField] private GameObject _mobsTilemaps;
        private System.Threading.CancellationTokenSource _cts = new();
        public Tile[,] Map { get => _map; }

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            _map = new Tile[_tilemap.size.y, _tilemap.size.x];
            for (int y = 0; y < _tilemap.size.y; y++)
            {
                for (int x = 0; x < _tilemap.size.x; x++)
                {
                    _map[y, x] = new Tile(x, y, _tilemap.GetTile(new Vector3Int(x, y)) == null);
                }
            }
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _mobsTilemaps.SetActive(true);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _mobsTilemaps.SetActive(false);
            }
        }
        private async void Start()
        {
            float butterflyProgress = 0f;
            float spiderProgress = 0f;
            float dogProgress = 0f;
            System.Progress<float> butterflyProgressIndicator = new System.Progress<float>((progress) =>
            {
                butterflyProgress = progress;
                _generatingPanel.text = "Generating PathGrid\n" +
                "Butterfly PathGrid" + (butterflyProgress * 100).ToString("F0") + "%\n" +
                "Spider PathGrid" + (spiderProgress * 100).ToString("F0") + "%\n" +
                "Dog PathGrid" + (dogProgress * 100).ToString("F0") + "%\n";
            });
            System.Progress<float> spiderProgressIndicator = new System.Progress<float>((progress) =>
            {
                spiderProgress = progress;
                _generatingPanel.text = "Generating PathGrid\n" +
                "Butterfly PathGrid" + (butterflyProgress * 100).ToString("F0") + "%\n" +
                "Spider PathGrid" + (spiderProgress * 100).ToString("F0") + "%\n" +
                "Dog PathGrid" + (dogProgress * 100).ToString("F0") + "%\n";
            });
            System.Progress<float> dogProgressIndicator = new System.Progress<float>((progress) =>
            {
                dogProgress = progress;
                _generatingPanel.text = "Generating PathGrid\n" +
                "Butterfly PathGrid" + (butterflyProgress * 100).ToString("F0") + "%\n" +
                "Spider PathGrid" + (spiderProgress * 100).ToString("F0") + "%\n" +
                "Dog PathGrid" + (dogProgress * 100).ToString("F0") + "%\n";
            });
            PathGrid<Tile> butterFlyGrid = await Pathfinding.GeneratePathGridAsync(_map, new PathfindingPolicy(DiagonalsPolicy.ALL_DIAGONALS, 1.41421354f, MovementsPolicy.FLY), MajorOrder.DEFAULT, butterflyProgressIndicator, _cts.Token);
            PathGrid<Tile> spiderGrid = await Pathfinding.GeneratePathGridAsync(_map, new PathfindingPolicy(DiagonalsPolicy.ALL_DIAGONALS, 1.41421354f, MovementsPolicy.WALL_CONTACT), MajorOrder.DEFAULT, spiderProgressIndicator, _cts.Token);
            PathGrid<Tile> dogGrid = await Pathfinding.GeneratePathGridAsync(_map, new PathfindingPolicy(DiagonalsPolicy.DIAGONAL_1FREE, 1.41421354f, MovementsPolicy.WALL_BELOW), MajorOrder.DEFAULT, dogProgressIndicator, _cts.Token);

            _generatingPanel.transform.parent.gameObject.SetActive(false);
            if (_butterfly)
            {
                _butterfly.PathGrid = butterFlyGrid;
                _butterfly.Player = _player;
                _butterfly.GridController = this;
                _butterfly.gameObject.SetActive(true);
            }
            if (_spider)
            {
                _spider.PathGrid = spiderGrid;
                _spider.Player = _player;
                _spider.GridController = this;
                _spider.gameObject.SetActive(true);
            }
            if (_dog)
            {
                _dog.PathGrid = dogGrid;
                _dog.Player = _player;
                _dog.GridController = this;
                _dog.gameObject.SetActive(true);
            }
            _player.gameObject.SetActive(true);
        }
    }
}
