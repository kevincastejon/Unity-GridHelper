using UnityEngine;
namespace RTS_Demo
{
    public enum CharacterMovementState
    {
        IDLE,
        MOVING_TO_TILE,
        SWITCHING_TO_NEXT_TILE,
        MOVEMENT_OVER,
    }

    public class CharacterMovementStateMachine : MonoBehaviour
    {
        private CharacterMovementState _currentState;
        private Character _character;

        public CharacterMovementState CurrentState { get => _currentState; private set => _currentState = value; }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }
        private void Start()
        {
        }
        private void Update()
        {
            OnStateUpdate(CurrentState);
        }

        private void OnStateEnter(CharacterMovementState state)
        {
            switch (state)
            {
                case CharacterMovementState.IDLE:
                    OnEnterIdle();
                    break;
                case CharacterMovementState.MOVING_TO_TILE:
                    OnEnterMovingToTile();
                    break;
                case CharacterMovementState.SWITCHING_TO_NEXT_TILE:
                    OnEnterSwitchingToNextTile();
                    break;
                case CharacterMovementState.MOVEMENT_OVER:
                    OnEnterMovementOver();
                    break;
                default:
                    Debug.LogError("OnStateEnter: Invalid state " + state.ToString());
                    break;
            }
        }
        private void OnStateUpdate(CharacterMovementState state)
        {
            switch (state)
            {
                case CharacterMovementState.IDLE:
                    OnUpdateIdle();
                    break;
                case CharacterMovementState.MOVING_TO_TILE:
                    OnUpdateMovingToTile();
                    break;
                case CharacterMovementState.SWITCHING_TO_NEXT_TILE:
                    OnUpdateSwitchingToNextTile();
                    break;
                case CharacterMovementState.MOVEMENT_OVER:
                    OnUpdateMovementOver();
                    break;
                default:
                    Debug.LogError("OnStateUpdate: Invalid state " + state.ToString());
                    break;
            }
        }
        private void OnStateExit(CharacterMovementState state)
        {
            switch (state)
            {
                case CharacterMovementState.IDLE:
                    OnExitIdle();
                    break;
                case CharacterMovementState.MOVING_TO_TILE:
                    OnExitMovingToTile();
                    break;
                case CharacterMovementState.SWITCHING_TO_NEXT_TILE:
                    OnExitSwitchingToNextTile();
                    break;
                case CharacterMovementState.MOVEMENT_OVER:
                    OnExitMovementOver();
                    break;
                default:
                    Debug.LogError("OnStateExit: Invalid state " + state.ToString());
                    break;
            }
        }
        private void TransitionToState(CharacterMovementState toState)
        {
            OnStateExit(CurrentState);
            CurrentState = toState;
            OnStateEnter(toState);
        }

        private void OnEnterIdle()
        {
        }
        private void OnUpdateIdle()
        {
            if (_character.HasPath)
            {
                TransitionToState(CharacterMovementState.MOVING_TO_TILE);
            }
        }
        private void OnExitIdle()
        {
        }

        private void OnEnterMovingToTile()
        {
            _character.StartStepMove();
        }
        private void OnUpdateMovingToTile()
        {
            if (_character.IsStepOver)
            {
                if (_character.AllStepsOver)
                {
                    TransitionToState(CharacterMovementState.MOVEMENT_OVER);
                }
                else
                {
                    TransitionToState(CharacterMovementState.SWITCHING_TO_NEXT_TILE);
                }
                return;
            }
            _character.DoStepMove();
        }
        private void OnExitMovingToTile()
        {
            _character.StopStepMove();
        }

        private void OnEnterSwitchingToNextTile()
        {
            _character.SwitchToNextTileStep();
        }
        private void OnUpdateSwitchingToNextTile()
        {
            TransitionToState(CharacterMovementState.MOVING_TO_TILE);
        }

        private void OnExitSwitchingToNextTile()
        {
        }

        private void OnEnterMovementOver()
        {
            _character.MovementOver();
        }
        private void OnUpdateMovementOver()
        {
            TransitionToState(CharacterMovementState.IDLE);
        }
        private void OnExitMovementOver()
        {
        }

    }
}
