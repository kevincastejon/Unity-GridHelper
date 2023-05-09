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
        [SerializeField] private int _width = 16;
        [SerializeField] private int _height = 12;
        [SerializeField] private int _depth = 18;

        public void GenerateMap(UnityAction<GameObject> callback)
        {
            for (int y = 0; y < _height; y++)
            {
                GameObject line = new GameObject("Line (" + y + ")");
                line.transform.parent = transform;
                line.transform.localPosition = new Vector3(0f, y, 0f);
                callback(line);
                for (int x = 0; x < _width; x++)
                {
                    if (!_3D)
                    {
                        GameObject tile = Instantiate(_tilePrefab);
                        tile.name = "Tile (" + x + ")";
                        tile.transform.parent = line.transform;
                        tile.transform.localPosition = new Vector3(x, 0f, 0f);
                        callback(tile);
                        continue;
                    }
                    GameObject col = new GameObject("Column (" + x + ")");
                    col.transform.parent = line.transform;
                    col.transform.localPosition = new Vector3(x, 0f, 0f);
                    callback(col);
                    for (int z = 0; z < _depth; z++)
                    {
                        GameObject tile = Instantiate(_tilePrefab);
                        tile.name = "Tile ("+z+")";
                        tile.transform.parent = col.transform;
                        tile.transform.localPosition = new Vector3(0f, 0f, z);
                        callback(tile);
                    }
                }
            }
        }
    }
}