using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RTS_Demo
{
    public class GameController : MonoBehaviour
    {
        private GameUI _ui;
        private GridController _grid;
        private int _currentRound;
        [SerializeField] private Timer _timer;

        public bool HasAllPlayersFinished { get => _grid.HasAllPlayersFinished; }
        public bool HasAllMobsFinished { get => _grid.HasAllMobsFinished; }
        public bool HasAllPlayersDead { get => _grid.HasAllPlayersDead; }
        public bool HasAllMobsDead { get => _grid.HasAllMobsDead; }
        public bool HasPlayerMoved { get => _grid.HasPlayerMoved; }
        public bool HasPlayerAttacked { get => _grid.HasPlayerAttacked; }
        public bool HasMobMoved { get => _grid.HasMobMoved; }
        public bool HasMobAttacked { get => _grid.HasMobAttacked; }
        public bool IsTimerEnded { get => _timer.IsCompleted; }
        public bool IsMoveButtonClicked { get => _ui.IsMoveButtonClicked; }
        public bool IsAttackButtonClicked { get => _ui.IsAttackButtonClicked; }
        public Floor PickMobTarget { get => _grid.PickMobTarget; }
        public bool IsSkipButtonClicked { get => _ui.IsSkipButtonClicked; }
        public Character CharacterClicked { get => _grid.CharacterClicked; }
        public Floor ReachableTileClicked { get => _grid.ReachableTileClicked; }
        public Floor AttackableTileClicked { get => _grid.AttackableTileClicked; }

        private void Awake()
        {
            _grid = GetComponentInChildren<GridController>();
            _ui = GetComponentInChildren<GameUI>();
        }

        public void StartSwitchRound()
        {
            _currentRound++;
            _grid.SwitchRound();
            _ui.StartSlide("Round " + _currentRound);
            _timer.Start();
        }
        public void DoSwitchRound()
        {
            //_grid.DoSwitchRound();
            _ui.DoSlide(_timer.Progress);
        }
        public void StopSwitchRound()
        {
            //_grid.StopSwitchRound();
            _ui.StopSlide();
            _timer.Stop();
        }

        public void StartSwitchToPlayer()
        {
            //_grid.StartSwitchToPlayer();
            _ui.StartSlide("Player Turn");
            _ui.StartInPanelChar();
            _ui.StartInPanelButtons();
            _timer.Start();
        }
        public void DoSwitchToPlayer()
        {
            //_grid.DoSwitchToPlayer();
            _ui.DoSlide(_timer.Progress);
            _ui.DoInPanelChar(_timer.Progress);
            _ui.DoInPanelButtons(_timer.Progress);
        }
        public void StopSwitchToPlayer()
        {
            //_grid.StopSwitchToPlayer();
            _ui.StopSlide();
            _ui.StopInPanelChar();
            _ui.StopInPanelButtons();
            _timer.Stop();
        }

        public void StartSwitchCharacter(Character charac = null)
        {
            _grid.StartSwitchCharacter(charac);
            _ui.StartOutInPanelChar();
            _ui.StartSlide(_grid.CurrentCharacter.Name);
            _timer.Start();
        }
        public void DoSwitchCharacter()
        {
            if (_ui.CurrentCharacter != _grid.CurrentCharacter && _timer.Progress >= 0.5f)
            {
                _ui.CurrentCharacter = _grid.CurrentCharacter;
            }
            _grid.DoSwitchCharacter(_timer.Progress);
            _ui.DoOutInPanelChar(_timer.Progress);
            _ui.DoSlide(_timer.Progress);
        }
        public void StopSwitchCharacter()
        {
            _grid.StopSwitchCharacter();
            _ui.StopOutInPanelChar();
            _ui.StopSlide();
            _timer.Stop();
        }

        public void StartPreparingMove()
        {
            _grid.StartPreparingMove();
            _timer.Start();
            _ui.RefreshButtons();
            _ui.SetTitle("Move");
            _ui.ShowMoveDescription();
        }
        public void DoPreparingMove()
        {
            _grid.DoPreparingMove();
        }
        public void StopPreparingMove()
        {
            _grid.StopPreparingMove();
            _timer.Stop();
            _ui.DisableAllButtons();
            _ui.SetTitle("");
            _ui.HideDescriptions();
        }

        public void StartPreparingAttack()
        {
            _grid.StartPreparingAttack();
            _timer.Start();
            _ui.RefreshButtons();
            _ui.SetTitle("Attack");
            _ui.ShowAttackDescription();
        }
        public void DoPreparingAttack()
        {
            _grid.DoPreparingAttack();
        }
        public void StopPreparingAttack()
        {
            _grid.StopPreparingAttack();
            _timer.Stop();
            _ui.DisableAllButtons();
            _ui.SetTitle("");
            _ui.HideDescriptions();
        }

        public void StartSkipping()
        {
            _grid.Skip();
            _ui.StartSlide("SKIPPED");
            _timer.Start();
        }
        public void DoSkipping()
        {
            _ui.DoSlide(_timer.Progress);
        }
        public void StopSkipping()
        {
            _ui.StopSlide();
        }

        public void StartMoving(Floor destination)
        {
            _grid.Move(destination);
        }

        public void StartAttacking(Floor clickedTile)
        {
            _grid.StartAttacking(clickedTile);
        }
        public void DoAttacking()
        {
            _grid.DoAttacking();
        }

        public void StartClosingPlayerTurn()
        {
            _ui.CurrentCharacter = null;
            _ui.StartSlide("PLAYER TURN IS OVER");
            _ui.StartOutPanelChar();
            _ui.StartOutPanelButtons();
            _timer.Start();
        }
        public void DoClosingPlayerTurn()
        {
            _ui.DoSlide(_timer.Progress);
            _ui.DoOutPanelChar(_timer.Progress);
            _ui.DoOutPanelButtons(_timer.Progress);
        }
        public void StopClosingPlayerTurn()
        {
            _ui.StopSlide();
            _ui.StopOutPanelChar();
            _ui.StopOutPanelButtons();
        }


        public void StartSwitchingToIA()
        {
            _ui.StartSlide("IA Turn");
            _ui.StartInPanelChar();
            _timer.Start();
        }
        public void DoSwitchingToIA()
        {
            _ui.DoSlide(_timer.Progress);
            _ui.DoInPanelChar(_timer.Progress);
        }
        public void StopSwitchingToIA()
        {
            _ui.StopSlide();
            _ui.StopInPanelChar();
            _timer.Stop();
        }

        public void StartIASwitchCharacter()
        {
            _grid.StartIASwitchCharacter();
            _ui.StartOutInPanelChar();
            _ui.StartSlide(_grid.CurrentMob.Name);
            _timer.Start();
        }
        public void DoIASwitchCharacter()
        {
            if (_ui.CurrentCharacter != _grid.CurrentMob && _timer.Progress >= 0.5f)
            {
                _ui.CurrentCharacter = _grid.CurrentMob;
            }
            _grid.DoIASwitchCharacter(_timer.Progress);
            _ui.DoOutInPanelChar(_timer.Progress);
            _ui.DoSlide(_timer.Progress);
        }
        public void StopIASwitchCharacter()
        {
            _grid.StopIASwitchCharacter();
            _ui.StopOutInPanelChar();
            _ui.StopSlide();
            _timer.Stop();
        }

        public void StartIAPreparingMove()
        {
            _grid.StartIAPreparingMove();
            _ui.SetTitle("Move");
            _timer.Start();
        }
        public void StopIAPreparingMove()
        {
            _ui.SetTitle("");
            _grid.StopIAPreparingMove();
            _timer.Stop();
        }

        public void StartIAMoving()
        {
            _grid.IAMove();
        }

        public void StartIAPreparingAttack()
        {
            _grid.StartIAPreparingAttack();
            _ui.SetTitle("Attack");
            _timer.Start();
        }
        public void StopIAPreparingAttack()
        {
            _ui.SetTitle("");
            _grid.StopIAPreparingAttack();
            _timer.Stop();
        }

        public void StartIAAttacking()
        {
            _grid.StartIAAttacking();
        }
        public void DoIAAttacking()
        {
            _grid.DoIAAttacking();
        }

        public void StartClosingIATurn()
        {
            _ui.CurrentCharacter = null;
            _ui.StartSlide("IA TURN IS OVER");
            _ui.StartOutPanelChar();
            _timer.Start();
        }
        public void DoClosingIATurn()
        {
            _ui.DoSlide(_timer.Progress);
            _ui.DoOutPanelChar(_timer.Progress);
        }
        public void StopClosingIATurn()
        {
            _ui.StopSlide();
            _ui.StopOutPanelChar();
        }

        public void StartVictory()
        {
            _ui.CurrentCharacter = null;
            _ui.StartSlide("VICTORY");
            _ui.StartOutPanelChar();
            _ui.StartOutPanelButtons();
            _ui.DisableAllButtons();
            _timer.Start();
        }
        public void DoVictory()
        {
            _ui.DoSlide(_timer.Progress);
            _ui.DoOutPanelChar(_timer.Progress);
            _ui.DoOutPanelButtons(_timer.Progress);
        }
        public void StopVictory()
        {
            _ui.StopSlide();
            _ui.StopOutPanelChar();
            _ui.StopOutPanelButtons();
        }
        public void StartGameOver()
        {
            _ui.CurrentCharacter = null;
            _ui.StartSlide("GAME OvER");
            _ui.StartOutPanelChar();
            _timer.Start();
        }
        public void DoGameOver()
        {
            _ui.DoSlide(_timer.Progress);
            _ui.DoOutPanelChar(_timer.Progress);
        }
        public void StopGameOver()
        {
            _ui.StopSlide();
            _ui.StopOutPanelChar();
        }
    }
}
