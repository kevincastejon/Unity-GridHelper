using KevinCastejon.GridHelper;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grid2DHelper.APIDemo.Playground
{
    public class PathfindingController : MonoBehaviour
    {
        public enum DemoType
        {
            DIRECT_PATHFINDING,
            DIRECT_PATHFINDING_ASYNC,
            PATHMAP,
            PATHMAP_ASYNC,
            PATHGRID_ASYNC,
        }
        [SerializeField] private TextMeshProUGUI _progressWindow;
        [SerializeField] private TMP_Dropdown _demoTypeDropDown;
        [SerializeField] private TMP_Dropdown _diagonalsPolicyDropDown;
        [SerializeField] private Toggle _flyToggle;
        [SerializeField] private Toggle _wallBelowToggle;
        [SerializeField] private Toggle _wallAsideToggle;
        [SerializeField] private Toggle _wallAboveToggle;
        [SerializeField] private Slider _diagonalsWeightSlider;
        [SerializeField] private Slider _maxDistanceSlider;
        [SerializeField] private Button _cancelBtn;
        [SerializeField] private DiagonalsPolicy _diagonalsPolicy;
        [SerializeField] private float _diagonalsWeight;
        [SerializeField] private MovementsPolicy _movementsPolicy;
        [SerializeField] private float _maxDistance;
        [SerializeField] private GridController _grid;
        private PathMap<Tile> _pathMap;
        private PathGrid<Tile> _pathGrid;
        private DemoType _demoType;
        private Tile _targetTile;
        private Tile _startTile;
        private bool _walling;
        private bool _startWalkableValue;
        private System.Threading.CancellationTokenSource _cts = new();

        private void SetCurrentDemoType(DemoType value)
        {
            if (_demoType == value)
            {
                return;
            }
            _demoType = value;
            switch (_demoType)
            {
                case DemoType.DIRECT_PATHFINDING:
                    _maxDistanceSlider.transform.parent.gameObject.SetActive(false);
                    DirectPathfind();
                    break;
                case DemoType.DIRECT_PATHFINDING_ASYNC:
                    _maxDistanceSlider.transform.parent.gameObject.SetActive(false);
                    DirectPathfindAsync();
                    break;
                case DemoType.PATHMAP:
                    _maxDistanceSlider.transform.parent.gameObject.SetActive(true);
                    GeneratePathMap();
                    break;
                case DemoType.PATHMAP_ASYNC:
                    GeneratePathMapAsync();
                    _maxDistanceSlider.transform.parent.gameObject.SetActive(true);
                    break;
                case DemoType.PATHGRID_ASYNC:
                    GeneratePathGridAsync();
                    _maxDistanceSlider.transform.parent.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
        public void SetMovementPolicy()
        {
            int value = 0;
            if (!_flyToggle.isOn)
            {
                if (!_wallBelowToggle.isOn && !_wallAsideToggle.isOn && !_wallAboveToggle.isOn)
                {
                    _flyToggle.isOn = true;
                }
                else
                {
                    if (_wallBelowToggle.isOn)
                    {
                        value += 1;
                    }
                    if (_wallAsideToggle.isOn)
                    {
                        value += 2;
                    }
                    if (_wallAboveToggle.isOn)
                    {
                        value += 4;
                    }
                }
            }
            _movementsPolicy = (MovementsPolicy)value;
        }
        private void Awake()
        {
            _maxDistanceSlider.value = _maxDistance;
            _demoTypeDropDown.value = (int)_demoType;
            _flyToggle.onValueChanged.AddListener((x) =>
            {
                SetMovementPolicy(); OnCriticalChange();
            });
            _wallBelowToggle.onValueChanged.AddListener((x) =>
            {
                SetMovementPolicy(); OnCriticalChange();
            });
            _wallAsideToggle.onValueChanged.AddListener((x) =>
            {
                SetMovementPolicy(); OnCriticalChange();
            });
            _wallAboveToggle.onValueChanged.AddListener((x) =>
            {
                SetMovementPolicy(); OnCriticalChange();
            });
            _maxDistanceSlider.onValueChanged.AddListener((x) =>
            {
                _maxDistance = x; OnCriticalChange();
            });
            _diagonalsWeightSlider.onValueChanged.AddListener((x) =>
            {
                _diagonalsWeight = x; OnCriticalChange();
            });
            _diagonalsPolicyDropDown.onValueChanged.AddListener((x) =>
            {
                _diagonalsPolicy = (DiagonalsPolicy)x; OnCriticalChange();
            });
            _demoTypeDropDown.onValueChanged.AddListener((x) =>
            {
                SetCurrentDemoType((DemoType)x);
            });

            _cancelBtn.onClick.AddListener(() => _cts.Cancel());
        }
        private void Start()
        {
            _targetTile = _grid.CenterTile;
            _startTile = _grid.StartTile;
            OnCriticalChange();
        }
        private void Update()
        {
            if ((_grid.JustEnteredTile && Input.GetMouseButton(0)) || (Input.GetMouseButtonDown(0) && _grid.HoveredTile != null && _grid.HoveredTile != _targetTile))
            {
                if (!_walling)
                {
                    _walling = true;
                    _startWalkableValue = !_grid.HoveredTile.IsWalkable;
                }
                if (_grid.HoveredTile.IsWalkable != _startWalkableValue)
                {
                    _grid.SetWalkable(_grid.HoveredTile, _startWalkableValue);
                    _grid.Refresh(null, new Tile[0], new Tile[0], null);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_walling)
                {
                    _walling = false;
                    OnCriticalChange();
                }
            }
            else if (Input.GetMouseButtonDown(1) && (_grid.HoveredTile != null && _grid.HoveredTile.IsWalkable && _grid.HoveredTile != _targetTile && _grid.HoveredTile != _startTile))
            {
                _targetTile = _grid.HoveredTile;
                OnTargetChange();
            }
            else if (Input.GetMouseButtonDown(2) && (_grid.HoveredTile != null && _grid.HoveredTile.IsWalkable && _grid.HoveredTile != _targetTile && _grid.HoveredTile != _startTile && _grid.ClampedHoveredTile != _targetTile))
            {
                _startTile = _grid.HoveredTile;
                OnStartChange();
            }
        }
        private void OnStartChange()
        {
            switch (_demoType)
            {
                case DemoType.DIRECT_PATHFINDING:
                    DirectPathfind();
                    break;
                case DemoType.DIRECT_PATHFINDING_ASYNC:
                    DirectPathfindAsync();
                    break;
                case DemoType.PATHMAP:
                case DemoType.PATHMAP_ASYNC:
                    GetPathFromPathMap();
                    break;
                case DemoType.PATHGRID_ASYNC:
                    GetPathFromPathGrid();
                    break;
                default:
                    break;
            }
        }
        private void OnTargetChange()
        {
            switch (_demoType)
            {
                case DemoType.DIRECT_PATHFINDING:
                    DirectPathfind();
                    break;
                case DemoType.DIRECT_PATHFINDING_ASYNC:
                    DirectPathfindAsync();
                    break;
                case DemoType.PATHMAP:
                    GeneratePathMap();
                    break;
                case DemoType.PATHMAP_ASYNC:
                    GeneratePathMapAsync();
                    break;
                case DemoType.PATHGRID_ASYNC:
                    GetPathFromPathGrid();
                    break;
                default:
                    break;
            }
        }
        private void OnCriticalChange()
        {
            switch (_demoType)
            {
                case DemoType.DIRECT_PATHFINDING:
                    DirectPathfind();
                    break;
                case DemoType.DIRECT_PATHFINDING_ASYNC:
                    DirectPathfindAsync();
                    break;
                case DemoType.PATHMAP:
                    GeneratePathMap();
                    break;
                case DemoType.PATHMAP_ASYNC:
                    GeneratePathMapAsync();
                    break;
                case DemoType.PATHGRID_ASYNC:
                    GeneratePathGridAsync();
                    break;
                default:
                    break;
            }
        }
        private void DirectPathfind()
        {
            _grid.Refresh(_targetTile, null, Pathfinding.CalculatePath(_grid.Map, _targetTile, _startTile, false, false, new PathfindingPolicy(_diagonalsPolicy, _diagonalsWeight, _movementsPolicy)), _startTile);
        }
        private async void DirectPathfindAsync()
        {
            _grid.MouseEnabled = false;
            _progressWindow.transform.parent.gameObject.SetActive(true);
            System.Progress<float> progressIndicator = new System.Progress<float>((progress) =>
            {
                _progressWindow.text = "Calculating path\n" + (progress * 100).ToString("F0") + "%";
            });
            _grid.Refresh(_targetTile, null, await Pathfinding.CalculatePathAsync(_grid.Map, _targetTile, _startTile, false, false, new PathfindingPolicy(_diagonalsPolicy, _diagonalsWeight, _movementsPolicy), MajorOrder.DEFAULT, progressIndicator, _cts.Token), _startTile);
            _progressWindow.transform.parent.gameObject.SetActive(false);
            _grid.MouseEnabled = true;
        }
        private void GeneratePathMap()
        {
            _pathMap = Pathfinding.GeneratePathMap(_grid.Map, _targetTile, _maxDistance, new PathfindingPolicy(_diagonalsPolicy, _diagonalsWeight, _movementsPolicy));
            _grid.Refresh(_targetTile, _pathMap.GetAccessibleTiles(false), _pathMap.IsTileAccessible(_startTile) ? _pathMap.GetPathToTarget(_startTile, false, false) : new Tile[0], _startTile);
        }
        private async void GeneratePathMapAsync()
        {
            _grid.MouseEnabled = false;
            _progressWindow.transform.parent.gameObject.SetActive(true);
            System.Progress<float> progressIndicator = new System.Progress<float>((progress) =>
            {
                _progressWindow.text = "Generating PathMap\n" + (progress * 100).ToString("F0") + "%";
            });
            _pathMap = await Pathfinding.GeneratePathMapAsync(_grid.Map, _targetTile, _maxDistance, new PathfindingPolicy(_diagonalsPolicy, _diagonalsWeight, _movementsPolicy), MajorOrder.DEFAULT, progressIndicator, _cts.Token);
            _grid.Refresh(_targetTile, _pathMap.GetAccessibleTiles(false), _pathMap.IsTileAccessible(_startTile) ? _pathMap.GetPathToTarget(_startTile, false, false) : new Tile[0], _startTile);
            _progressWindow.transform.parent.gameObject.SetActive(false);
            _grid.MouseEnabled = true;
        }
        private void GetPathFromPathMap()
        {
            _grid.Refresh(null, null, _pathMap.IsTileAccessible(_startTile) ? _pathMap.GetPathToTarget(_startTile, false, false) : new Tile[0], _startTile);
        }
        private async void GeneratePathGridAsync()
        {
            _grid.MouseEnabled = false;
            _progressWindow.transform.parent.gameObject.SetActive(true);
            System.Progress<float> progressIndicator = new System.Progress<float>((progress) =>
            {
                _progressWindow.text = "Generating PathGrid\n" + (progress * 100).ToString("F0") + "%";
            });
            _pathGrid = await Pathfinding.GeneratePathGridAsync(_grid.Map, new PathfindingPolicy(_diagonalsPolicy, _diagonalsWeight, _movementsPolicy), MajorOrder.DEFAULT, progressIndicator, _cts.Token);
            _grid.Refresh(_targetTile, new Tile[0], _pathGrid.GetPath(_startTile, _targetTile, false, false), _startTile);
            _progressWindow.transform.parent.gameObject.SetActive(false);
            _grid.MouseEnabled = true;
        }
        private void GetPathFromPathGrid()
        {
            _grid.Refresh(_targetTile, null, _pathGrid.GetPath(_startTile, _targetTile, false, false), _startTile);
        }
        private void OnApplicationQuit()
        {
            _cts.Cancel();
        }
    }
}