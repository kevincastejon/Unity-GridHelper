using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace KevinCastejon.GridHelperDemoMisc
{
    [RequireComponent(typeof(Slider))]
    public class SliderFloatToStringConverter : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> _onChange;
        public void ValueChangedF0(float value)
        {
            _onChange.Invoke(Mathf.FloorToInt(value).ToString());
        }
        public void ValueChangedF1(float value)
        {
            _onChange.Invoke(value.ToString("F1"));
        }
        public void ValueChangedF2(float value)
        {
            _onChange.Invoke(value.ToString("F2"));
        }
        public void ValueChangedF3(float value)
        {
            _onChange.Invoke(value.ToString("F2"));
        }
        public void ValueChangedF4(float value)
        {
            _onChange.Invoke(value.ToString("F2"));
        }
        public void ValueChanged(float value)
        {
            _onChange.Invoke(value.ToString());
        }
    }
}