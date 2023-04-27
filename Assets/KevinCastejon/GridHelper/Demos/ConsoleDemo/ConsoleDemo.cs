using KevinCastejon.Grid2DHelper;
using UnityEngine;
namespace ConsoleDemo
{
    // A simple Tile object implementing the ITile interface
    public struct Tile : ITile
    {
        private string _name;
        public Tile(string name, int x, int y, bool isWalkable, float weight = 1f)
        {
            _name = name;
            X = x;
            Y = y;
            IsWalkable = isWalkable;
            Weight = weight;
        }
        public bool IsWalkable { get; set; }
        public float Weight { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
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
    public class ConsoleDemo : MonoBehaviour
    {
        // Declaring the grid
        private Tile[,] _grid;
        private void Awake()
        {
            // The target tile
            Tile target = new Tile("T",4,5, true);
            // Any tile on the map (here [5;0])
            Tile anyTile = new Tile("S",0,5, true);
            // Feeding the grid manually
            _grid = new Tile[6, 5] {
                { new Tile("0", 0,0, true), new Tile("1",1,0, true), new Tile("2",2,0, true),  new Tile("3",3,0, true), new Tile("4",4,0, true) },
                { new Tile("5", 0,1, true), new Tile("6",1,1, true), new Tile("7",2,1, false), new Tile("8",3,1, true), new Tile("9",4,1, true) },
                { new Tile("10",0,2, true),new Tile("11",1,2, true),new Tile("12",2,2, false),new Tile("13",3,2, true),new Tile("14",4,2, true) },
                { new Tile("15",0,3, true),new Tile("16",1,3, true),new Tile("17",2,3, false),new Tile("18",3,3, true),new Tile("19",4,3, true) },
                { new Tile("20",0,4, true),new Tile("21",1,4, true),new Tile("22",2,4, false),new Tile("23",3,4, true),new Tile("24",4,4, true) },
                { anyTile                 ,new Tile("25",1,5, true),new Tile("26",2,5, false),new Tile("27",3,5, true),target }
            };
            // Generating the path map (costly)
            PathMap<Tile> pathMap = Grid2DHelper.Pathfinding.GeneratePathMap(_grid, target);
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