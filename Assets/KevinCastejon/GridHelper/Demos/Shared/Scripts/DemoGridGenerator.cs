using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GridHelperDemoMisc
{
    public class DemoGridGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private bool _3D;
        [SerializeField] [Min(1)] private int _width = 71;
        [SerializeField] [Min(1)] private int _height = 65;
        [SerializeField] [Min(1)] private int _depth = 61;
    }
}