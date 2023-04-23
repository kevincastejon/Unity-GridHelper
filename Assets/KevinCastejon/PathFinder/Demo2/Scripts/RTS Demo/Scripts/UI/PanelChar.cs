using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace RTS_Demo2
{
    public class PanelChar : MonoBehaviour
    {
        [SerializeField] private Image _diagonals;
        [SerializeField] private TextMeshProUGUI _moveRange;
        [SerializeField] private TextMeshProUGUI _attackRange;
        [SerializeField] private TextMeshProUGUI _health;
        [SerializeField] private TextMeshProUGUI _name;
        private Animator _animator;
        private Character _character;

        public Character Character
        {
            get
            {
                return _character;
            }

            set
            {
                _character = value;
                if (_character)
                {
                    _diagonals.color = _character.AllowDiagonals ? Color.green : Color.red;
                    _moveRange.text = _character.MaxMovement.ToString();
                    _attackRange.text = _character.AttackRange.ToString();
                    _health.text = _character.Health.ToString();
                    _name.text = _character.Name.ToString();
                }
                else
                {
                    _diagonals.color = Color.white;
                    _moveRange.text = "";
                    _attackRange.text = "";
                    _health.text = "";
                    _name.text = "";
                }
            }
        }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
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

        public void StartOutInPanel()
        {
            _animator.SetTrigger("OutIn");
            _animator.SetFloat("PlayProgress", 0f);
        }
        public void DoOutInPanel(float progress)
        {
            _animator.SetFloat("PlayProgress", progress);
        }
        public void StopOutInPanel()
        {
            _animator.SetFloat("PlayProgress", 1f);
        }
    }
}
