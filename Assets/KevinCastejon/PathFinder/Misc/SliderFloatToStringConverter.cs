using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
[RequireComponent(typeof(Slider))]
public class SliderFloatToStringConverter : MonoBehaviour
{
    [SerializeField] private UnityEvent<string> _onChange;

    public void ValueChanged(float value)
    {
        _onChange.Invoke(value.ToString());
    }
}