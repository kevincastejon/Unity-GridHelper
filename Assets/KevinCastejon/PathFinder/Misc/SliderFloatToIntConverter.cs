using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderFloatToIntConverter : MonoBehaviour
{
    [SerializeField] private UnityEvent<int> _onChange;

    public void ValueChanged(float value)
    {
        _onChange.Invoke(Mathf.RoundToInt(value));
    }
}
