using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridHelperDemoMisc
{
    public class DemoGridGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private Texture2D _textureSource;
        [SerializeField] private bool _3D;
        [SerializeField] [Min(1)] private int _width = 71;
        [SerializeField] [Min(1)] private int _height = 65;
        [SerializeField] [Min(1)] private int _depth = 61;
        private void PropsDebugger()
        {
            Debug.Log(_width + _height + _depth);
        }
    }
}