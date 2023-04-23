using UnityEngine;
namespace RTS_Demo2
{
    public enum GameState
    {
        SWITCHING_ROUND,
        SWITCHING_TO_PLAYER,
        SWITCHING_CHARACTER,
        SKIPPING,
        PREPARING_MOVE,
        PREPARING_ATTACK,
        MOVING,
        ATTACKING,
        CLOSING_PLAYER_TURN,
        SWITCHING_TO_IA,
        IA_THINKING,
        IA_SWITCHING_CHARACTER,
        IA_PREPARING_MOVE,
        IA_PREPARING_ATTACK,
        IA_ATTACKING,
        IA_MOVING,
        CLOSING_IA_TURN,
        VICTORY,
        GAME_OVER,
    }

    public class GameStateMachine : MonoBehaviour
    {
        private GameState _currentState;
        private GameState _lastState = GameState.PREPARING_MOVE;
        private GameController _controller;
        private Character _clickedChar;
        private Floor _clickedTile;
        public GameState CurrentState { get => _currentState; private set => _currentState = value; }

        private void Awake()
        {
            _controller = GetComponent<GameController>();
        }
        private void Start()
        {
            OnStateEnter(_currentState);
        }
        private void Update()
        {
            OnStateUpdate(CurrentState);
        }

        private void OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.SWITCHING_ROUND:
                    OnEnterSwitchingRound();
                    break;
                case GameState.SWITCHING_TO_PLAYER:
                    OnEnterSwitchingToPlayer();
                    break;
                case GameState.SWITCHING_CHARACTER:
                    OnEnterSwitchingCharacter();
                    break;
                case GameState.SKIPPING:
                    OnEnterSkipping();
                    break;
                case GameState.PREPARING_MOVE:
                    OnEnterPreparingMove();
                    break;
                case GameState.PREPARING_ATTACK:
                    OnEnterPreparingAttack();
                    break;
                case GameState.MOVING:
                    OnEnterMoving();
                    break;
                case GameState.ATTACKING:
                    OnEnterAttacking();
                    break;
                case GameState.CLOSING_PLAYER_TURN:
                    OnEnterClosingPlayerTurn();
                    break;
                case GameState.SWITCHING_TO_IA:
                    OnEnterSwitchingToIA();
                    break;
                case GameState.IA_THINKING:
                    OnEnterIAThinking();
                    break;
                case GameState.IA_PREPARING_MOVE:
                    OnEnterIAPreparingMove();
                    break;
                case GameState.IA_PREPARING_ATTACK:
                    OnEnterIAPreparingAttack();
                    break;
                case GameState.IA_MOVING:
                    OnEnterIAMoving();
                    break;
                case GameState.IA_ATTACKING:
                    OnEnterIAAttacking();
                    break;
                case GameState.IA_SWITCHING_CHARACTER:
                    OnEnterIASwitchingCharacter();
                    break;
                case GameState.CLOSING_IA_TURN:
                    OnEnterClosingIATurn();
                    break;
                case GameState.VICTORY:
                    OnEnterVictory();
                    break;
                case GameState.GAME_OVER:
                    OnEnterGameOver();
                    break;
                default:
                    Debug.LogError("OnStateEnter: Invalid state " + state.ToString());
                    break;
            }
        }
        private void OnStateUpdate(GameState state)
        {
            switch (state)
            {
                case GameState.SWITCHING_TO_PLAYER:
                    OnUpdateSwitchingToPlayer();
                    break;
                case GameState.SWITCHING_ROUND:
                    OnUpdateSwitchingRound();
                    break;
                case GameState.SWITCHING_CHARACTER:
                    OnUpdateSwitchingCharacter();
                    break;
                case GameState.SKIPPING:
                    OnUpdateSkipping();
                    break;
                case GameState.PREPARING_MOVE:
                    OnUpdatePreparingMove();
                    break;
                case GameState.PREPARING_ATTACK:
                    OnUpdatePreparingAttack();
                    break;
                case GameState.MOVING:
                    OnUpdateMoving();
                    break;
                case GameState.ATTACKING:
                    OnUpdateAttacking();
                    break;
                case GameState.CLOSING_PLAYER_TURN:
                    OnUpdateClosingPlayerTurn();
                    break;
                case GameState.SWITCHING_TO_IA:
                    OnUpdateSwitchingToIA();
                    break;
                case GameState.IA_THINKING:
                    OnUpdateIAThinking();
                    break;
                case GameState.IA_PREPARING_MOVE:
                    OnUpdateIAPreparingMove();
                    break;
                case GameState.IA_PREPARING_ATTACK:
                    OnUpdateIAPreparingAttack();
                    break;
                case GameState.IA_MOVING:
                    OnUpdateIAMoving();
                    break;
                case GameState.IA_ATTACKING:
                    OnUpdateIAAttacking();
                    break;
                case GameState.IA_SWITCHING_CHARACTER:
                    OnUpdateIASwitchingCharacter();
                    break;
                case GameState.CLOSING_IA_TURN:
                    OnUpdateClosingIATurn();
                    break;
                case GameState.VICTORY:
                    OnUpdateVictory();
                    break;
                case GameState.GAME_OVER:
                    OnUpdateGameOver();
                    break;
                default:
                    Debug.LogError("OnStateUpdate: Invalid state " + state.ToString());
                    break;
            }
        }
        private void OnStateExit(GameState state)
        {
            switch (state)
            {
                case GameState.SWITCHING_TO_PLAYER:
                    OnExitSwitchingToPlayer();
                    break;
                case GameState.SWITCHING_ROUND:
                    OnExitSwitchingRound();
                    break;
                case GameState.SWITCHING_CHARACTER:
                    OnExitSwitchingCharacter();
                    break;
                case GameState.SKIPPING:
                    OnExitSkipping();
                    break;
                case GameState.PREPARING_MOVE:
                    OnExitPreparingMove();
                    break;
                case GameState.PREPARING_ATTACK:
                    OnExitPreparingAttack();
                    break;
                case GameState.MOVING:
                    OnExitMoving();
                    break;
                case GameState.ATTACKING:
                    OnExitAttacking();
                    break;
                case GameState.CLOSING_PLAYER_TURN:
                    OnExitClosingPlayerTurn();
                    break;
                case GameState.SWITCHING_TO_IA:
                    OnExitSwitchingToIA();
                    break;
                case GameState.IA_THINKING:
                    OnExitIAThinking();
                    break;
                case GameState.IA_PREPARING_MOVE:
                    OnExitIAPreparingMove();
                    break;
                case GameState.IA_PREPARING_ATTACK:
                    OnExitIAPreparingAttack();
                    break;
                case GameState.IA_MOVING:
                    OnExitIAMoving();
                    break;
                case GameState.IA_ATTACKING:
                    OnExitIAAttacking();
                    break;
                case GameState.IA_SWITCHING_CHARACTER:
                    OnExitIASwitchingCharacter();
                    break;
                case GameState.CLOSING_IA_TURN:
                    OnExitClosingIATurn();
                    break;
                case GameState.VICTORY:
                    OnExitVictory();
                    break;
                case GameState.GAME_OVER:
                    OnExitGameOver();
                    break;
                default:
                    Debug.LogError("OnStateExit: Invalid state " + state.ToString());
                    break;
            }
        }
        private void TransitionToState(GameState toState)
        {
            OnStateExit(CurrentState);
            CurrentState = toState;
            OnStateEnter(toState);
        }

        private void OnEnterSwitchingRound()
        {
            _controller.StartSwitchRound();
        }
        private void OnUpdateSwitchingRound()
        {
            if (_controller.IsTimerEnded)
            {
                TransitionToState(GameState.SWITCHING_TO_PLAYER);
                return;
            }

            _controller.DoSwitchRound();
        }
        private void OnExitSwitchingRound()
        {
            _controller.StopSwitchRound();
        }

        private void OnEnterSwitchingToPlayer()
        {
            _controller.StartSwitchToPlayer();
        }
        private void OnUpdateSwitchingToPlayer()
        {
            if (_controller.IsTimerEnded)
            {
                TransitionToState(GameState.SWITCHING_CHARACTER);
                return;
            }

            _controller.DoSwitchToPlayer();
        }
        private void OnExitSwitchingToPlayer()
        {
            _controller.StopSwitchToPlayer();
        }

        private void OnEnterSwitchingCharacter()
        {
            _controller.StartSwitchCharacter(_clickedChar);
            _clickedChar = null;
        }
        private void OnUpdateSwitchingCharacter()
        {
            if (_controller.IsTimerEnded)
            {
                if (_lastState == GameState.PREPARING_MOVE)
                {
                    if (!_controller.HasPlayerMoved)
                    {
                        TransitionToState(GameState.PREPARING_MOVE);
                        return;
                    }
                    else
                    {
                        TransitionToState(GameState.PREPARING_ATTACK);
                        return;
                    }
                }
                else if (_lastState == GameState.PREPARING_ATTACK)
                {
                    if (!_controller.HasPlayerAttacked)
                    {
                        TransitionToState(GameState.PREPARING_ATTACK);
                        return;
                    }
                    else
                    {
                        TransitionToState(GameState.PREPARING_MOVE);
                        return;
                    }
                }
            }
            _controller.DoSwitchCharacter();
        }
        private void OnExitSwitchingCharacter()
        {
            _controller.StopSwitchCharacter();
        }

        private void OnEnterSkipping()
        {
            _controller.StartSkipping();
        }
        private void OnUpdateSkipping()
        {
            if (_controller.IsTimerEnded)
            {
                if (_controller.HasAllPlayersFinished)
                {
                    TransitionToState(GameState.CLOSING_PLAYER_TURN);
                    return;
                }
                else
                {
                    TransitionToState(GameState.SWITCHING_CHARACTER);
                    return;
                }
            }
            _controller.DoSkipping();
        }
        private void OnExitSkipping()
        {
            _controller.StopSkipping();
        }

        private void OnEnterPreparingMove()
        {
            _controller.StartPreparingMove();
        }
        private void OnUpdatePreparingMove()
        {
            if (_controller.IsSkipButtonClicked)
            {
                TransitionToState(GameState.SKIPPING);
                return;
            }
            Character clickedChar = _controller.CharacterClicked;
            if (clickedChar)
            {
                _clickedChar = clickedChar;
                TransitionToState(GameState.SWITCHING_CHARACTER);
                return;
            }
            if (!_controller.HasPlayerAttacked && _controller.IsAttackButtonClicked)
            {
                TransitionToState(GameState.PREPARING_ATTACK);
                return;
            }
            Floor clickedTile = _controller.ReachableTileClicked;
            if (clickedTile)
            {
                _clickedTile = clickedTile;
                TransitionToState(GameState.MOVING);
                return;
            }

            _controller.DoPreparingMove();
        }
        private void OnExitPreparingMove()
        {
            _controller.StopPreparingMove();
            _lastState = GameState.PREPARING_MOVE;
        }

        private void OnEnterPreparingAttack()
        {
            _controller.StartPreparingAttack();
        }
        private void OnUpdatePreparingAttack()
        {
            if (_controller.IsSkipButtonClicked)
            {
                TransitionToState(GameState.SKIPPING);
                return;
            }
            Character clickedChar = _controller.CharacterClicked;
            if (clickedChar)
            {
                _clickedChar = clickedChar;
                TransitionToState(GameState.SWITCHING_CHARACTER);
                return;
            }
            if (!_controller.HasPlayerMoved && _controller.IsMoveButtonClicked)
            {
                TransitionToState(GameState.PREPARING_MOVE);
                return;
            }
            Floor clickedTile = _controller.AttackableTileClicked;
            if (clickedTile)
            {
                _clickedTile = clickedTile;
                TransitionToState(GameState.ATTACKING);
                return;
            }

            _controller.DoPreparingAttack();
        }
        private void OnExitPreparingAttack()
        {
            _controller.StopPreparingAttack();
            _lastState = GameState.PREPARING_ATTACK;
        }

        private void OnEnterMoving()
        {
            _controller.StartMoving(_clickedTile);
        }
        private void OnUpdateMoving()
        {
            if (_controller.HasPlayerMoved)
            {
                if (!_controller.HasPlayerAttacked)
                {
                    TransitionToState(GameState.PREPARING_ATTACK);
                    return;
                }
                else if (_controller.HasAllPlayersFinished)
                {
                    TransitionToState(GameState.CLOSING_PLAYER_TURN);
                    return;
                }
                else
                {
                    TransitionToState(GameState.SWITCHING_CHARACTER);
                    return;
                }
            }
        }
        private void OnExitMoving()
        {
        }

        private void OnEnterAttacking()
        {
            _controller.StartAttacking(_clickedTile);
        }
        private void OnUpdateAttacking()
        {
            if (_controller.HasPlayerAttacked)
            {
                if (_controller.HasAllMobsDead)
                {
                    TransitionToState(GameState.VICTORY);
                    return;
                }
                else if (!_controller.HasPlayerMoved)
                {
                    TransitionToState(GameState.PREPARING_MOVE);
                    return;
                }
                else if (_controller.HasAllPlayersFinished)
                {
                    TransitionToState(GameState.CLOSING_PLAYER_TURN);
                    return;
                }
                else
                {
                    TransitionToState(GameState.SWITCHING_CHARACTER);
                    return;
                }
            }
            _controller.DoAttacking();
        }
        private void OnExitAttacking()
        {
        }

        private void OnEnterClosingPlayerTurn()
        {
            _controller.StartClosingPlayerTurn();
        }
        private void OnUpdateClosingPlayerTurn()
        {
            if (_controller.IsTimerEnded)
            {
                TransitionToState(GameState.SWITCHING_TO_IA);
                return;
            }

            _controller.DoClosingPlayerTurn();
        }
        private void OnExitClosingPlayerTurn()
        {
            _controller.StopClosingPlayerTurn();
        }

        private void OnEnterSwitchingToIA()
        {
            _controller.StartSwitchingToIA();
        }
        private void OnUpdateSwitchingToIA()
        {
            if (_controller.IsTimerEnded)
            {
                TransitionToState(GameState.IA_SWITCHING_CHARACTER);
                return;
            }
            _controller.DoSwitchingToIA();
        }
        private void OnExitSwitchingToIA()
        {
            _controller.StopSwitchingToIA();
        }

        private void OnEnterIASwitchingCharacter()
        {
            _controller.StartIASwitchCharacter();
        }
        private void OnUpdateIASwitchingCharacter()
        {
            if (_controller.IsTimerEnded)
            {
                TransitionToState(GameState.IA_THINKING);
                return;
            }
            _controller.DoIASwitchCharacter();
        }
        private void OnExitIASwitchingCharacter()
        {
            _controller.StopIASwitchCharacter();
        }

        private void OnEnterIAThinking()
        {
        }
        private void OnUpdateIAThinking()
        {
            if (_controller.PickMobTarget)
            {
                TransitionToState(GameState.IA_PREPARING_ATTACK);
                return;
            }
            else
            {
                TransitionToState(GameState.IA_PREPARING_MOVE);
                return;
            }
        }
        private void OnExitIAThinking()
        {
        }

        private void OnEnterIAPreparingMove()
        {
            _controller.StartIAPreparingMove();
        }
        private void OnUpdateIAPreparingMove()
        {
            if (_controller.IsTimerEnded)
            {
                TransitionToState(GameState.IA_MOVING);
                return;
            }
        }
        private void OnExitIAPreparingMove()
        {
            _controller.StopIAPreparingMove();
        }

        private void OnEnterIAPreparingAttack()
        {
            _controller.StartIAPreparingAttack();
        }
        private void OnUpdateIAPreparingAttack()
        {
            if (_controller.IsTimerEnded)
            {
                if (_controller.PickMobTarget)
                {
                    TransitionToState(GameState.IA_ATTACKING);
                    return;
                }
                else if (!_controller.HasMobMoved)
                {
                    TransitionToState(GameState.IA_PREPARING_MOVE);
                    return;
                }
                else if (_controller.HasAllMobsFinished)
                {
                    TransitionToState(GameState.CLOSING_IA_TURN);
                    return;
                }
                else
                {
                    TransitionToState(GameState.IA_SWITCHING_CHARACTER);
                    return;
                }
            }

        }
        private void OnExitIAPreparingAttack()
        {
            _controller.StopIAPreparingAttack();
        }

        private void OnEnterIAMoving()
        {
            _controller.StartIAMoving();
        }
        private void OnUpdateIAMoving()
        {
            if (_controller.HasMobMoved)
            {
                if (!_controller.HasMobAttacked)
                {
                    TransitionToState(GameState.IA_PREPARING_ATTACK);
                    return;
                }
                else if (_controller.HasAllMobsFinished)
                {
                    TransitionToState(GameState.CLOSING_IA_TURN);
                    return;
                }
                else
                {
                    TransitionToState(GameState.IA_SWITCHING_CHARACTER);
                    return;
                }
            }
        }
        private void OnExitIAMoving()
        {
        }

        private void OnEnterIAAttacking()
        {
            _controller.StartIAAttacking();
        }
        private void OnUpdateIAAttacking()
        {
            if (_controller.HasMobAttacked)
            {
                if (_controller.HasAllPlayersDead)
                {
                    TransitionToState(GameState.GAME_OVER);
                    return;
                }
                else if (!_controller.HasMobMoved)
                {
                    TransitionToState(GameState.IA_PREPARING_MOVE);
                    return;
                }
                else if (_controller.HasAllMobsFinished)
                {
                    TransitionToState(GameState.CLOSING_IA_TURN);
                    return;
                }
                else
                {
                    TransitionToState(GameState.IA_SWITCHING_CHARACTER);
                    return;
                }
            }
            _controller.DoIAAttacking();
        }
        private void OnExitIAAttacking()
        {
        }

        private void OnEnterClosingIATurn()
        {
            _controller.StartClosingIATurn();
        }
        private void OnUpdateClosingIATurn()
        {
            if (_controller.IsTimerEnded)
            {
                TransitionToState(GameState.SWITCHING_ROUND);
                return;
            }
            _controller.DoClosingIATurn();
        }
        private void OnExitClosingIATurn()
        {
            _controller.StopClosingIATurn();
        }

        private void OnEnterVictory()
        {
            _controller.StartVictory();
        }
        private void OnUpdateVictory()
        {
            _controller.DoVictory();
        }
        private void OnExitVictory()
        {
            _controller.StopVictory();
        }

        private void OnEnterGameOver()
        {
            _controller.StartGameOver();
        }
        private void OnUpdateGameOver()
        {
            _controller.DoGameOver();
        }
        private void OnExitGameOver()
        {
            _controller.StopGameOver();
        }
    }
}
