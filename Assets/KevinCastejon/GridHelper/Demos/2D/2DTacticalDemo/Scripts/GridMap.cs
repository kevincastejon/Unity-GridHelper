using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
namespace TacticalDemo
{
    public class GridMap : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private TextMeshProUGUI _playerLabel;
        [SerializeField] private TextMeshProUGUI _modeLabel;
        [SerializeField] private TextMeshProUGUI _description;
        private Tile[,] _map = new Tile[10, 12];
        private Tile[] _pathTiles;
        private Tile _hoveredTile;
        private Tile _moveDestinationTile;
        private Character _green;
        private Character _red;
        private Character _currentCharacter;
        private Camera _camera;
        private bool _isAttackMode;
        private bool _isSelectingMode = true;
        private float _maxDistanceMove = 5f;
        private float _maxDistanceAttack = 7f;

        Tile CurrentCharacterTile { get => _currentCharacter.CurrentTile; }
        Tile CurrentOpponentTile { get => _currentCharacter == _green ? _red.CurrentTile : _green.CurrentTile; }

        private void Awake()
        {
            Tile[] tiles = FindObjectsOfType<Tile>();
            foreach (Tile tile in tiles)
            {
                tile.X = Mathf.RoundToInt(tile.transform.position.x);
                tile.Y = Mathf.RoundToInt(tile.transform.position.z);
                _map[tile.Y, tile.X] = tile;
            }
            _camera = Camera.main;
        }
        void Start()
        {
            _green = GameObject.FindGameObjectWithTag("Green").GetComponent<Character>();
            _red = GameObject.FindGameObjectWithTag("Red").GetComponent<Character>();
            _green.PathMap = Pathfinding.GeneratePathMap(_map, _map[Mathf.RoundToInt(_green.transform.position.z), Mathf.RoundToInt(_green.transform.position.x)], _maxDistanceMove);
            _red.PathMap = Pathfinding.GeneratePathMap(_map, _map[Mathf.RoundToInt(_red.transform.position.z), Mathf.RoundToInt(_red.transform.position.x)], _maxDistanceMove);
            _currentCharacter = _green;
            OnNewTurn();
        }
        private void SwitchCharacter()
        {
            _currentCharacter = _currentCharacter == _green ? _red : _green;
            OnNewTurn();
        }
        private void OnNewTurn()
        {
            _playerLabel.text = _currentCharacter.name;
            OnMoveMode();
        }
        private void OnMoveMode()
        {
            _modeLabel.text = "Move";
            _description.text = "Click an accessible tile to move\nor press Enter to skip";
            _lineRenderer.positionCount = 0;
            _lineRenderer.startColor = Color.white;
            _lineRenderer.endColor = Color.white;
            Tile[] accessibleTiles = _currentCharacter.PathMap.GetAccessibleTiles();
            SetPathTiles(accessibleTiles);
        }
        private void OnAttackMode()
        {
            _modeLabel.text = "Attack";
            _description.text = "Click a visible opponent tile to attack\nor press Enter to skip";
            Tile[] tilesOnRadius = Extraction.GetTilesInACircle(_map, CurrentCharacterTile, Mathf.FloorToInt(_maxDistanceAttack), false, false);
            SetPathTiles(tilesOnRadius);
        }
        void Update()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);
            Tile hoveredTile = isHit ? hit.collider.GetComponent<Tile>() : null;
            if (hoveredTile && hoveredTile.IsWalkable)
            {
                if (_hoveredTile != hoveredTile)
                {
                    _hoveredTile = hoveredTile;
                    OnTileEntered();
                }
            }
            else
            {
                _hoveredTile = null;
                _lineRenderer.positionCount = 0;
            }
            if (_isAttackMode)
            {
                if (_isSelectingMode)
                {
                    DoAttackSelect();
                }
                else
                {
                    DoAttack();
                }
            }
            else
            {
                if (_isSelectingMode)
                {
                    DoMoveSelect();
                }
                else
                {
                    DoMove();
                }
            }
        }
        private void DoMoveSelect()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                _isAttackMode = true;
                OnAttackMode();
            }
            if (_hoveredTile)
            {
                if (Input.GetMouseButtonDown(0) && _currentCharacter.PathMap.IsTileAccessible(_hoveredTile) && _hoveredTile != CurrentOpponentTile)
                {
                    _lineRenderer.positionCount = 0;
                    _currentCharacter.Move(_currentCharacter.PathMap.GetPathFromTarget(_hoveredTile));
                    _isSelectingMode = false;
                    _moveDestinationTile = _hoveredTile;
                }
            }
        }
        private void DoMove()
        {
            if (!_currentCharacter.IsMoving)
            {
                _isSelectingMode = true;
                _isAttackMode = true;
                _currentCharacter.PathMap = Pathfinding.GeneratePathMap(_map, _moveDestinationTile, _maxDistanceMove);
                _moveDestinationTile = null;
                OnAttackMode();
            }
        }
        private void OnTileEntered()
        {
            if (!_isAttackMode)
            {
                if (_currentCharacter.PathMap.IsTileAccessible(_hoveredTile))
                {
                    Tile[] path = _currentCharacter.PathMap.GetPathToTarget(_hoveredTile);
                    _lineRenderer.positionCount = path.Length;
                    _lineRenderer.SetPositions(path.Select(t => new Vector3(t.transform.position.x, 0.05f, t.transform.position.z)).ToArray());
                }
                else
                {
                    _lineRenderer.positionCount = 0;
                }
            }
            else
            {
                if (Extraction.IsTileInACircle(_map,CurrentCharacterTile, _hoveredTile, Mathf.FloorToInt(_maxDistanceAttack)))
                {
                    _lineRenderer.positionCount = 2;
                    Tile[] lineOfSight = Raycasting.GetLineOfSight(_map, CurrentCharacterTile, _hoveredTile, out bool isLineClear, _maxDistanceAttack, false);
                    _lineRenderer.startColor = isLineClear ? Color.green : Color.red;
                    _lineRenderer.endColor = isLineClear ? Color.green : Color.red;
                    _lineRenderer.SetPositions(new Vector3[] { new Vector3(_currentCharacter.transform.position.x, 0.05f, _currentCharacter.transform.position.z), new Vector3(lineOfSight[lineOfSight.Length - 1].transform.position.x, 0.05f, lineOfSight[lineOfSight.Length - 1].transform.position.z) });
                }
                else
                {
                    _lineRenderer.positionCount = 0;
                }
            }
        }
        private void DoAttackSelect()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                _isSelectingMode = true;
                _isAttackMode = false;
                SwitchCharacter();
            }
            if (_hoveredTile)
            {
                if (Input.GetMouseButtonDown(0) && _hoveredTile == CurrentOpponentTile && Raycasting.IsLineOfSightClear(_map, CurrentCharacterTile, _hoveredTile, _maxDistanceAttack, false))
                {
                        _lineRenderer.positionCount = 0;
                        _currentCharacter.Attack(_hoveredTile);
                        _isSelectingMode = false;
                }
            }
        }
        private void DoAttack()
        {
            if (!_currentCharacter.IsMoving)
            {
                _isSelectingMode = true;
                _isAttackMode = false;
                SwitchCharacter();
            }
        }
        private void SetPathTiles(Tile[] pathTiles)
        {
            if (_pathTiles != null)
            {
                foreach (Tile tile in _pathTiles)
                {
                    tile.TileState = TileState.FLOOR;
                }
            }
            _pathTiles = null;
            if (pathTiles != null)
            {
                foreach (Tile tile in pathTiles)
                {
                    tile.TileState = TileState.PATH;
                }
                _pathTiles = pathTiles;
            }
        }
    }

}