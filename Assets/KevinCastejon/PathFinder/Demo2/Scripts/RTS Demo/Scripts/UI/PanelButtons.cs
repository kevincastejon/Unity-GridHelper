using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RTS_Demo2
{
    public class PanelButtons : MonoBehaviour
    {
        private Animator _animator;
        [SerializeField] private Button _moveButton;
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _skipButton;
        private ButtonChecker _moveButtonChecker;
        private ButtonChecker _attackButtonChecker;
        private ButtonChecker _skipButtonChecker;

        public bool IsMoveButtonClicked { get => _moveButtonChecker.IsClicked; }
        public bool IsAttackButtonClicked { get => _attackButtonChecker.IsClicked; }
        public bool IsSkipButtonClicked { get => _skipButtonChecker.IsClicked; }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _moveButtonChecker = _moveButton.GetComponent<ButtonChecker>();
            _attackButtonChecker = _attackButton.GetComponent<ButtonChecker>();
            _skipButtonChecker = _skipButton.GetComponent<ButtonChecker>();
        }

        public void StartInPanel()
        {
            _animator.SetTrigger("In");
            _animator.SetFloat("PlayProgress", 0f);
        }
        public void DoInPanel(float progress)
        {
            _animator.SetFloat("PlayProgress", progress);
        }
        public void StopInPanel()
        {
            _animator.SetFloat("PlayProgress", 1f);
        }

        public void StartOutPanel()
        {
            _animator.SetTrigger("Out");
            _animator.SetFloat("PlayProgress", 0f);
        }
        public void DoOutPanel(float progress)
        {
            _animator.SetFloat("PlayProgress", progress);
        }
        public void StopOutPanel()
        {
            _animator.SetFloat("PlayProgress", 1f);
        }

        public void RefreshButtons(Character character)
        {
            _moveButton.interactable = !character.HasMoved && !character.IsPreparingMove;
            _attackButton.interactable = !character.HasAttacked && !character.IsPreparingAttack;
            _skipButton.interactable = true;
        }
        public void DisableAllButtons()
        {
            _moveButton.interactable = false;
            _attackButton.interactable = false;
            _skipButton.interactable = false;
        }
    }
}