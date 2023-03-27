
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KevinCastejon.Pathfinding
{
    public interface ITile
    {
        public bool IsWalkable
        {
            get;
        }

        public int X
        {
            get;
        }

        public int Y
        {
            get;
        }

    }

    public class Node
    {
        private ITile _tile;

        public Node(ITile tile, int x, int y)
        {
            _tile = tile;
            _x = x;
            _y = y;
        }

        private bool _reached;
        private bool _isGoal;
        private Node _next;
        private int _distance;
        private int _x;
        private int _y;

        public ITile Tile { get => _tile; set => _tile = value; }
        public bool Reached { get => _reached; set => _reached = value; }
        public bool IsGoal { get => _isGoal; set => _isGoal = value; }
        public Node Next { get => _next; set => _next = value; }
        public int Distance { get => _distance; set => _distance = value; }
        public int X { get => _x; }
        public int Y { get => _y; }
    }
    public class PathMap
    {
        private Node[,] _map;
        private Node[] _flatMap;
        private ITile _target;

        public PathMap(Node[,] map, ITile target)
        {
            _target = target;
            _map = map;
            _flatMap = new Node[_map.GetLength(0) * _map.GetLength(1)];
            int it = 0;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    _flatMap[it] = map[i, j];
                    it++;
                }
            }
        }

        public ITile Target { get => _target; }
        public Node[,] Map { get => _map; }

        public T[] GetAccessibleTilesFromTarget<T>(int movementCount = 0) where T : ITile
        {
            return _flatMap.Where(n => movementCount > 0 ? n.Distance <= movementCount && n.Distance > 0 : n.Distance > 0).Select(n => (T)n.Tile).ToArray();
        }
        public T[] GetPathToTarget<T>(ITile tile) where T : ITile
        {
            Node firstNode = _flatMap.First(n => n.Tile == tile);
            Node node = firstNode;
            List<ITile> tiles = new List<ITile>() { node.Tile };
            while (node.Tile != _target)
            {

                node = node.Next;
                tiles.Add(node.Tile);
            }
            return tiles.Select(x => (T)x).ToArray();
        }
        public T[] GetPathFromTarget<T>(ITile tile) where T : ITile
        {
            return GetPathToTarget<T>(tile).Reverse().ToArray();
        }
    }
    public class PathFinder
    {
        private static Node GetNode(Node[,] map, int x, int y)
        {
            if (x > -1 && y > -1 && x < map.GetLength(1) && y < map.GetLength(0))
            {
                return map[y, x];
            }
            return null;
        }
        private static List<Node> GetNeighbours(Node[,] map, int x, int y, bool allowDiagonals)
        {
            List<Node> nodes = new List<Node>();
            Node nei;
            bool leftWalkable;
            bool rightWalkable;
            bool topWalkable;
            bool bottomWalkable;
            nei = GetNode(map, x - 1, y);
            leftWalkable = nei != null && nei.Tile.IsWalkable;
            if (nei != null && nei.Tile.IsWalkable)
            {
                nodes.Add(nei);
            }
            nei = GetNode(map, x, y - 1);
            bottomWalkable = nei != null && nei.Tile.IsWalkable;
            if (nei != null && nei.Tile.IsWalkable)
            {
                nodes.Add(nei);
            }
            nei = GetNode(map, x, y + 1);
            topWalkable = nei != null && nei.Tile.IsWalkable;
            if (nei != null && nei.Tile.IsWalkable)
            {
                nodes.Add(nei);
            }
            nei = GetNode(map, x + 1, y);
            rightWalkable = nei != null && nei.Tile.IsWalkable;
            if (nei != null && nei.Tile.IsWalkable)
            {
                nodes.Add(nei);
            }
            nei = GetNode(map, x - 1, y - 1);
            if (allowDiagonals && leftWalkable && bottomWalkable && nei != null && nei.Tile.IsWalkable)
            {
                nodes.Add(nei);
            }
            nei = GetNode(map, x - 1, y + 1);
            if (allowDiagonals && leftWalkable && topWalkable && nei != null && nei.Tile.IsWalkable)
            {
                nodes.Add(nei);
            }
            nei = GetNode(map, x + 1, y + 1);
            if (allowDiagonals && rightWalkable && topWalkable && nei != null && nei.Tile.IsWalkable)
            {
                nodes.Add(nei);
            }
            nei = GetNode(map, x + 1, y - 1);
            if (allowDiagonals && rightWalkable && bottomWalkable && nei != null && nei.Tile.IsWalkable)
            {
                nodes.Add(nei);
            }

            return nodes;
        }
        public static T[] GetTilesIntoACircle<T>(ITile[,] map, ITile start, int radius) where T : ITile
        {
            int top = Mathf.CeilToInt(start.Y - radius),
                bottom = Mathf.FloorToInt(start.Y + radius),
                left = Mathf.CeilToInt(start.X - radius),
                right = Mathf.FloorToInt(start.X + radius);
            List<T> list = new List<T>();
            for (int y = top; y <= bottom; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    float dx = start.X - x,
                    dy = start.Y - y;
                    float distance_squared = dx * dx + dy * dy;
                    if (distance_squared <= radius * radius && x >= 0 && y >= 0 && x < map.GetLength(1) && y < map.GetLength(0))
                    {
                        list.Add((T)map[y, x]);
                    }
                }
            }
            return list.ToArray();
        }
        public static bool IsLineOfSightClear(ITile[,] map, ITile start, ITile stop)
        {
            ITile[] los = GetLineOfSight<ITile>(map, start, stop);
            return los[los.Length - 1] == stop;
        }
        public static T[] GetLineOfSight<T>(ITile[,] map, ITile start, ITile stop, int maxDistance = 0) where T : ITile
        {
            Vector2Int p0 = new Vector2Int(start.X, start.Y);
            Vector2Int p1 = new Vector2Int(stop.X, stop.Y);
            int dx = p1.x - p0.x, dy = p1.y - p0.y;
            int nx = Mathf.Abs(dx), ny = Mathf.Abs(dy);
            int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

            Vector2Int p = new Vector2Int(p0.x, p0.y);
            List<T> points = new List<T> { (T)map[p.y, p.x] };
            for (int ix = 0, iy = 0; ix < nx || iy < ny;)
            {
                if ((0.5 + ix) / nx < (0.5 + iy) / ny)
                {
                    // next step is horizontal
                    p.x += sign_x;
                    ix++;
                }
                else
                {
                    // next step is vertical
                    p.y += sign_y;
                    iy++;
                }
                if (!map[p.y, p.x].IsWalkable || (maxDistance > 0 && Vector2Int.Distance(new Vector2Int(p.x, p.y), new Vector2Int(start.X, start.Y)) > maxDistance))
                {
                    break;
                }

                points.Add((T)map[p.y, p.x]);
            }
            return points.ToArray();
        }
        public static PathMap GeneratePathMap(ITile[,] graph, ITile target, bool allowDiagonals = true)
        {
            int height = graph.GetLength(0);
            int width = graph.GetLength(1);
            Node targetNode = null;
            Node[,] map = new Node[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    map[i, j] = new Node(graph[i, j], j, i);
                    if (graph[i, j] == target)
                    {
                        targetNode = map[i, j];
                    }
                }
            }
            List<Node> frontier = new List<Node>() { targetNode };
            targetNode.Reached = true;
            targetNode.Next = targetNode;
            targetNode.Distance = 0;
            while (frontier.Count > 0)
            {
                Node current = frontier[0];
                frontier.RemoveAt(0);
                List<Node> neibourgs = GetNeighbours(map, current.X, current.Y, allowDiagonals);
                foreach (Node nei in neibourgs)
                {
                    if (!nei.Reached)
                    {
                        frontier.Add(nei);
                        nei.Reached = true;
                        nei.Next = current;
                        nei.Distance = current.Distance + 1;
                    }
                }
            }
            return new PathMap(map, target);
        }
    }
}