using KevinCastejon.GridHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RTS_Demo
{
    public class GridController : MonoBehaviour
    {
        private Character[] _characters;
        private Character[] _mobs;
        private int _currentCharacter = -1;
        private Floor[,] _map = new Floor[12, 10];
        private Camera _camera;
        private Floor _hoveredFloor;
        private Floor _target;
        private MoveTo _selectionCircle;
        private LineRenderer _lineRenderer;

        public Character CurrentCharacter { get => _characters[_currentCharacter]; }
        public Character CurrentMob { get => _mobs[_currentCharacter]; }
        public bool HasAllPlayersDead { get => _characters.Length == 0; }
        public bool HasAllMobsDead { get => _mobs.Length == 0; }
        public bool HasAllPlayersFinished { get => _characters.FirstOrDefault(p => !p.HasFinished) == null; }
        public bool HasAllMobsFinished { get => _mobs.FirstOrDefault(p => !p.HasFinished) == null; }
        public bool HasPlayerMoved { get => CurrentCharacter.HasMoved; }
        public bool HasPlayerAttacked { get => CurrentCharacter.HasAttacked; }
        public bool HasMobMoved { get => CurrentMob.HasMoved; }
        public bool HasMobAttacked { get => CurrentMob.HasAttacked; }
        public Floor PickMobTarget
        {
            get
            {
                Floor[] inViewRangeFloors = GridHelper.Extraction.GetTilesInARadius<Floor>(_map, CurrentMob.CurrentTile, CurrentMob.AttackRange);
                foreach (Character character in _characters)
                {
                    if (inViewRangeFloors.Contains(character.CurrentTile) && GridHelper.Raycasting.IsLineOfSightClear(_map, CurrentMob.CurrentTile, character.CurrentTile))
                    {
                        return character.CurrentTile;
                    }
                }
                return null;
            }
        }
        public Character CharacterClicked { get => (Input.GetMouseButtonDown(0) && _hoveredFloor != null && _hoveredFloor.Character != null && _hoveredFloor.Character.IsIA == false && !_hoveredFloor.Character.HasFinished && _hoveredFloor.Character != CurrentCharacter) ? _hoveredFloor.Character : null; }
        public Floor ReachableTileClicked { get => (Input.GetMouseButtonDown(0) && _hoveredFloor != null && CurrentCharacter.AccessibleTiles.Contains(_hoveredFloor)) ? _hoveredFloor : null; }
        public Floor AttackableTileClicked { get => (Input.GetMouseButtonDown(0) && _target != null && _target.Character != null && _target.Character.IsIA) ? _hoveredFloor : null; }
        public Floor[,] Map { get => _map; }

        public void Awake()
        {
            _lineRenderer = GetComponentInChildren<LineRenderer>();
            _selectionCircle = GetComponentInChildren<MoveTo>(true);
            Floor[] allFloors = GetComponentsInChildren<Floor>();
            // Referencing tiles into grid a dirty way (by position)
            foreach (Floor floor in allFloors)
            {
                int x = Mathf.RoundToInt(floor.transform.position.x);
                int y = Mathf.Abs(Mathf.RoundToInt(floor.transform.position.z));
                _map[y, x] = floor.GetComponent<Floor>();
                floor.X = x;
                floor.Y = y;
            }

            Character[] allChars = GetComponentsInChildren<Character>();
            _mobs = allChars.Where(c => c.IsIA).ToArray();
            _characters = allChars.Where(c => !c.IsIA).ToArray();
            // Referencing characters current tiles a dirty way (by position)
            foreach (Character charac in _characters)
            {
                int x = Mathf.RoundToInt(charac.transform.position.x);
                int y = Mathf.Abs(Mathf.RoundToInt(charac.transform.position.z));
                _map[y, x].Character = charac;
                charac.CurrentTile = _map[y, x];
            }
            foreach (Character charac in _mobs)
            {
                int x = Mathf.RoundToInt(charac.transform.position.x);
                int y = Mathf.Abs(Mathf.RoundToInt(charac.transform.position.z));
                _map[y, x].Character = charac;
                charac.CurrentTile = _map[y, x];
            }
        }
        private void Start()
        {
            // Referencing the camera
            _camera = Camera.main;
            // Calculating intial paths for all characters and mobs
            foreach (Character charac in _characters)
            {
                charac.PathMap = GridHelper.Pathfinding.GeneratePathMap(_map, charac.CurrentTile, charac.MaxMovement, charac.AllowDiagonals);
                charac.AccessibleTiles = charac.PathMap.GetAccessibleTiles();
            }
            foreach (Character charac in _mobs)
            {
                charac.PathMap = GridHelper.Pathfinding.GeneratePathMap(_map, charac.CurrentTile, charac.MaxMovement, charac.AllowDiagonals);
                charac.AccessibleTiles = charac.PathMap.GetAccessibleTiles();
            }
        }
        private void Update()
        {
            // Detecting click on tile
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                // Retrieving the Floor component
                Floor hitFloor = hit.collider.GetComponent<Floor>();
                if (hitFloor != _hoveredFloor)
                {
                    if (_hoveredFloor != null)
                    {
                        _hoveredFloor.IsHovered = false;
                    }
                    _hoveredFloor = hitFloor;
                    _hoveredFloor.IsHovered = true;
                }
            }
            else
            {
                if (_hoveredFloor != null)
                {
                    _hoveredFloor.IsHovered = false;
                }
                _hoveredFloor = null;
            }
        }

        public void SwitchRound()
        {
            foreach (Character character in _characters)
            {
                character.HasAttacked = false;
                character.HasMoved = false;
            }
            foreach (Character character in _mobs)
            {
                character.HasAttacked = false;
                character.HasMoved = false;
            }
        }

        public void StartSwitchCharacter(Character charac = null)
        {
            if (charac == null)
            {
                charac = _characters.First(x => !x.HasFinished);
            }
            _currentCharacter = Array.IndexOf(_characters, charac);
            charac.PathMap = GridHelper.Pathfinding.GeneratePathMap(_map, charac.CurrentTile, charac.MaxMovement, charac.AllowDiagonals);
            charac.AccessibleTiles = charac.PathMap.GetAccessibleTiles();
            _selectionCircle.gameObject.SetActive(true);
            _selectionCircle.StartMove(CurrentCharacter.transform.position);
        }
        public void DoSwitchCharacter(float progress)
        {
            _selectionCircle.DoMove(progress);
        }
        public void StopSwitchCharacter()
        {
            _selectionCircle.StopMove();
            _selectionCircle.transform.parent = CurrentCharacter.transform;
        }

        public void Skip()
        {
            CurrentCharacter.HasMoved = true;
            CurrentCharacter.HasAttacked = true;
        }

        public void StartPreparingMove()
        {
            CurrentCharacter.IsPreparingMove = true;
            ResetTiles(true);
            Floor[] accessibleTiles = CurrentCharacter.AccessibleTiles;
            foreach (Floor floor in accessibleTiles)
            {
                floor.IsPath = true;
            }
        }
        public void DoPreparingMove()
        {
            _lineRenderer.enabled = _hoveredFloor != null && CurrentCharacter.AccessibleTiles.Contains(_hoveredFloor);
            if (_lineRenderer.enabled)
            {
                Floor[] path = CurrentCharacter.PathMap.GetPathFromTarget(_hoveredFloor);
                _lineRenderer.positionCount = path.Length;
                _lineRenderer.SetPositions(path.Select(f => new Vector3(f.transform.position.x, 0.51f, f.transform.position.z)).ToArray());
            }
        }
        public void StopPreparingMove()
        {
            CurrentCharacter.IsPreparingMove = false;
            _lineRenderer.positionCount = 0;
            _lineRenderer.enabled = false;
            ResetTiles(true);
        }

        public void Move(Floor destination)
        {
            CurrentCharacter.StartMoving(destination);
        }

        public void StartPreparingAttack()
        {
            CurrentCharacter.IsPreparingAttack = true;
            ResetTiles(false, false, true, false);
            Floor[] inViewRangeFloors = GridHelper.Extraction.GetTilesInARadius<Floor>(_map, CurrentCharacter.CurrentTile, CurrentCharacter.AttackRange);
            foreach (Floor floor in inViewRangeFloors)
            {
                floor.IsInViewRange = true;
            }
        }
        public void DoPreparingAttack()
        {
            ResetTiles(true, false, false, false);
            _lineRenderer.enabled = false;
            if (_hoveredFloor)
            {
                Floor[] inLineSightFloors = GridHelper.Raycasting.GetLineOfSight<Floor>(_map, CurrentCharacter.CurrentTile, _hoveredFloor, CurrentCharacter.AttackRange);
                _target = inLineSightFloors.Length > 0 ? inLineSightFloors[inLineSightFloors.Length - 1] : null;
                foreach (Floor floor in inLineSightFloors)
                {
                    floor.IsPath = true;
                }
                _lineRenderer.enabled = true;
                _lineRenderer.positionCount = 2;
                _lineRenderer.SetPositions(new Vector3[] { new Vector3(CurrentCharacter.transform.position.x, 0.501f, CurrentCharacter.transform.position.z), new Vector3(inLineSightFloors[inLineSightFloors.Length - 1].transform.position.x, 0.501f, inLineSightFloors[inLineSightFloors.Length - 1].transform.position.z) });
            }
        }
        public void StopPreparingAttack()
        {
            CurrentCharacter.IsPreparingAttack = false;
            ResetTiles(true, false, true, true);
            _lineRenderer.positionCount = 0;
            _lineRenderer.enabled = false;
        }

        public void StartAttacking(Floor clickedTile)
        {
            CurrentCharacter.StartAttacking(clickedTile);
        }
        public void DoAttacking()
        {
            CurrentCharacter.DoAttacking();
            foreach (var item in _mobs)
            {
                if (item.Health == 0)
                {
                    _mobs = _mobs.Where(c => c != item).ToArray();
                    Destroy(item.gameObject);
                }
            }
        }

        public void StartIASwitchCharacter()
        {
            Character charac = _mobs.First(x => !x.HasFinished);
            charac.PathMap = GridHelper.Pathfinding.GeneratePathMap(_map, charac.CurrentTile, charac.MaxMovement, charac.AllowDiagonals);
            charac.AccessibleTiles = charac.PathMap.GetAccessibleTiles();
            _currentCharacter = Array.IndexOf(_mobs, charac);
            _selectionCircle.gameObject.SetActive(true);
            _selectionCircle.StartMove(CurrentMob.transform.position);
        }
        public void DoIASwitchCharacter(float progress)
        {
            _selectionCircle.DoMove(progress);
        }
        public void StopIASwitchCharacter()
        {
            _selectionCircle.StopMove();
            _selectionCircle.transform.parent = CurrentMob.transform;
        }

        public void StartIAPreparingMove()
        {
            CurrentMob.IsPreparingMove = true;
            ResetTiles(true);
            Floor[] accessibleTiles = CurrentMob.AccessibleTiles;
            foreach (Floor floor in accessibleTiles)
            {
                floor.IsPath = true;
            }
            _lineRenderer.enabled = true;
            _target = accessibleTiles[UnityEngine.Random.Range(0, accessibleTiles.Length)];
            Floor[] path = CurrentMob.PathMap.GetPathFromTarget(_target);
            _lineRenderer.positionCount = path.Length;
            _lineRenderer.SetPositions(path.Select(f => new Vector3(f.transform.position.x, 0.51f, f.transform.position.z)).ToArray());
        }
        public void StopIAPreparingMove()
        {
            CurrentMob.IsPreparingMove = false;
            _lineRenderer.positionCount = 0;
            _lineRenderer.enabled = false;
            ResetTiles(true);
        }

        public void IAMove()
        {
            CurrentMob.StartMoving(_target);
        }

        public void StartIAPreparingAttack()
        {
            CurrentMob.IsPreparingAttack = true;
            ResetTiles(false, false, true, false);
            Floor[] inViewRangeFloors = GridHelper.Extraction.GetTilesInARadius<Floor>(_map, CurrentMob.CurrentTile, CurrentMob.AttackRange);
            foreach (Floor floor in inViewRangeFloors)
            {
                floor.IsInViewRange = true;
            }
            Floor target = PickMobTarget;
            if (target)
            {
                _target = target;
                Floor[] inLineSightFloors = GridHelper.Raycasting.GetLineOfSight<Floor>(_map, CurrentMob.CurrentTile, target, CurrentMob.AttackRange);
                foreach (Floor floor in inLineSightFloors)
                {
                    floor.IsPath = true;
                }
                _lineRenderer.enabled = true;
                _lineRenderer.positionCount = 2;
                _lineRenderer.SetPositions(new Vector3[] { new Vector3(CurrentMob.transform.position.x, 0.501f, CurrentMob.transform.position.z), new Vector3(inLineSightFloors[inLineSightFloors.Length - 1].transform.position.x, 0.501f, inLineSightFloors[inLineSightFloors.Length - 1].transform.position.z) });
            }
            else
            {
                CurrentMob.HasAttacked = true;
            }
        }
        public void StopIAPreparingAttack()
        {
            CurrentMob.IsPreparingAttack = false;
            ResetTiles(true, false, true, true);
            _lineRenderer.positionCount = 0;
            _lineRenderer.enabled = false;
        }

        public void StartIAAttacking()
        {
            CurrentMob.StartAttacking(_target);
        }
        public void DoIAAttacking()
        {
            CurrentMob.DoAttacking();
            if (_target.Character.Health == 0)
            {
                _characters = _characters.Where(c => c != _target.Character).ToArray();
                Destroy(_target.Character.gameObject);
            }
        }

        private void ResetTiles(bool resetPath = false, bool resetPathTo = false, bool resetViewRange = false, bool resetViewRangeTo = false)
        {
            for (int i = 0; i < _map.GetLength(0); i++)
            {
                for (int j = 0; j < _map.GetLength(1); j++)
                {
                    if (resetPath)
                    {
                        _map[i, j].IsPath = resetPathTo;
                    }
                    if (resetViewRange)
                    {
                        _map[i, j].IsInViewRange = resetViewRangeTo;
                    }
                }
            }
        }
    }
}
