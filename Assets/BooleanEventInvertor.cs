using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BooleanEventInvertor : MonoBehaviour
{
    [SerializeField] private UnityEvent<bool> _onChange;

    public void OnChange(bool value)
    {
        _onChange.Invoke(!value);
    }
}
