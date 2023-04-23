using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RTS_Demo2
{
    [RequireComponent(typeof(Button))]
    public class ButtonChecker : MonoBehaviour
    {
        private Button _button;
        private bool _isClicked;
        private bool _isClickedLastValue;

        public bool IsClicked { get => _isClicked; }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Start()
        {
            _button.onClick.AddListener(() => _isClicked = true);
        }

        private void Update()
        {
            if (_isClickedLastValue && _isClicked)
            {
                _isClicked = false;
            }

            _isClickedLastValue = _isClicked;
        }
    }
}
