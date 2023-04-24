using KevinCastejon.GridHelper;
using UnityEngine;
namespace MinimalDemo
{
    // A simple Tile object implementing the ITile interface
    public struct Tile : ITile
    {
        private string _name;
        public Tile(string name, bool isWalkable, float weight = 1f)
        {
            _name = name;
            IsWalkable = isWalkable;
            Weight = weight;
        }
        public bool IsWalkable { get; set; }
        public float Weight { get; set; }
        public override string ToString()
        {
            if (IsWalkable)
            {
                return _name;
            }
            else
            {
                return '\u25A1'.ToString();
            }
        }
    }
    public class MinimalDemo : MonoBehaviour
    {
        // Declaring the grid
        private Tile[,] _grid;
        private void Awake()
        {
            // Any tile on the map
            Tile anyTile = new Tile("S", true);
            // The target tile
            Tile target = new Tile("T", true);
            // Feeding the grid
            _grid = new Tile[6, 5] {
                { new Tile("0", true), new Tile("1", true), new Tile("2", true),  new Tile("3", true), new Tile("4", true) },
                { new Tile("5", true), new Tile("6", true), new Tile("7", false), new Tile("8", true), new Tile("9", true) },
                { new Tile("10", true),new Tile("11", true),new Tile("12", false),new Tile("13", true),new Tile("14", true) },
                { new Tile("15", true),new Tile("16", true),new Tile("17", false),new Tile("18", true),new Tile("19", true) },
                { new Tile("20", true),new Tile("21", true),new Tile("22", false),new Tile("23", true),new Tile("24", true) },
                { anyTile             ,new Tile("25", true),new Tile("26", false),new Tile("27", true),target }
            };
            // Generating the path map (costly)
            PathMap<Tile> pathMap = GridHelper.GeneratePathMap(_grid, target);
            // Getting the path between the target and any tile on the map ("free" cost)
            Tile[] path = pathMap.GetPathToTarget(anyTile);
            // Displaying grid into console
            Debug.Log("Grid");
            for (int i = 0; i < _grid.GetLength(0); i++)
            {
                string lineStr = "";
                for (int j = 0; j < _grid.GetLength(1); j++)
                {
                    lineStr += "[" + _grid[i, j] + "] ";
                }
                Debug.Log(lineStr);
            }
            Debug.Log("");
            string pathStr = "";
            // Displaying path into console
            Debug.Log("Path from [S] to [T]");
            for (int i = 0; i < path.Length; i++)
            {
                pathStr += "[" + path[i] + "] ";
                if (i < path.Length - 1)
                {
                    pathStr += "-> ";
                }
            }
            Debug.Log(pathStr);
        }
    }
}