using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace RTS_Demo2
{
    public class PanelSlide : MonoBehaviour
    {
        private TextMeshProUGUI _label;
        private Animator _animator;

        private void Awake()
        {
            _label = GetComponentInChildren<TextMeshProUGUI>();
            _animator = GetComponentInChildren<Animator>();
        }

        public void StartSlide(string title)
        {
            _label.text = title;
            _animator.SetTrigger("Play");
            _animator.SetFloat("PlayProgress", 0f);
        }
        public void DoSlide(float progress)
        {
            _animator.SetFloat("PlayProgress", progress);
        }
        public void StopSlide()
        {
            _animator.SetFloat("PlayProgress", 0f);
        }
    }
}