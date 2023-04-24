
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KevinCastejon.GridHelper
{
    /// <summary>
    /// An interface that the user defined tile class has to implement in order to work with this library
    /// </summary>
    public interface ITile
    {
        /// <summary>
        /// Is the tile walkable
        /// </summary>
        public bool IsWalkable
        {
            get;
        }
        /// <summary>
        /// The tile movement weight (minimum 1f)
        /// </summary>
        public float Weight
        {
            get;
        }
    }
    /// <summary>
    /// An object containing all the precalculated paths data for a tile
    /// </summary>
    /// <typeparam name="T">The user defined tile type that implements the ITile interface</typeparam>
    public class Node<T> where T : ITile
    {
        internal Node(T tile, int x, int y)
        {
            _tile = tile;
            _x = x;
            _y = y;
        }

        private T _tile;
        private Node<T> _next;
        private Vector2 _nextDirection;
        private int _movementSteps;
        private float _movementCosts;
        private readonly int _x;
        private readonly int _y;

        /// <summary>
        /// The corresponding tile
        /// </summary>
        public T Tile { get => _tile; internal set => _tile = value; }
        /// <summary>
        /// The next Node on the path
        /// </summary>
        public Node<T> NextNode { get => _next; internal set => _next = value; }
        /// <summary>
        /// The direction of the next Node on the path (in 2D grid coordinates)
        /// </summary>
        public Vector2 NextDirection { get => _nextDirection; internal set => _nextDirection = value; }
        /// <summary>
        /// The number of steps along the path between this node and the target
        /// </summary>
        public int MovementSteps { get => _movementSteps; internal set => _movementSteps = value; }
        /// <summary>
        /// The movement cost along the path between this node and the target
        /// </summary>
        public float MovementCosts { get => _movementCosts; internal set => _movementCosts = value; }
        /// <summary>
        /// The node X coordinate
        /// </summary>     
        public int X { get => _x; }
        /// <summary>
        /// The node Y coordinate
        /// </summary>     
        public int Y { get => _y; }
    }
    /// <summary>
    /// An object containing all the precalculated paths data of a tile grid
    /// </summary>
    /// <typeparam name="T">The user defined tile type that implements the ITile interface</typeparam>
    public class PathMap<T> where T : ITile
    {
        private readonly Node<T>[,] _map;
        private readonly Node<T>[] _flatMap;
        private readonly Dictionary<T, Node<T>> _dico;
        private readonly T _target;

        internal PathMap(Node<T>[,] map, T target)
        {
            _target = target;
            _map = map;
            _flatMap = new Node<T>[_map.GetLength(0) * _map.GetLength(1)];
            _dico = new Dictionary<T, Node<T>>();
            int it = 0;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    _flatMap[it] = map[i, j];
                    _dico.Add(map[i, j].Tile, map[i, j]);
                    it++;
                }
            }
        }

        /// <summary>
        /// The target tile for this PathMap
        /// </summary>
        public T Target { get => _target; }
        /// <summary>
        /// Get the corresponding Node object for a tile
        /// </summary>
        /// <param name="tile">A tile object</param>
        /// <returns>A Node object</returns>
        public Node<T> GetNodeFromTile(T tile)
        {
            return _dico[tile];
        }
        /// <summary>
        /// Get the next tile along the path for a tile
        /// </summary>
        /// <param name="tile">A tile object</param>
        /// <returns>A tile object</returns>
        public T GetNextTileFromTile(T tile)
        {
            return _dico[tile].NextNode.Tile;
        }
        /// <summary>
        /// Get the direction of the next tile along the path for a tile
        /// </summary>
        /// <param name="tile">A tile object</param>
        /// <returns>A Vector2 direction</returns>
        public Vector2 GetNextTileDirectionFromTile(T tile)
        {
            return _dico[tile].NextDirection;
        }
        /// <summary>
        /// Get the tile movement steps count along the path between the target and a tile
        /// </summary>
        /// <param name="tile">A tile object</param>
        /// <returns>The movement steps count</returns>
        public int GetMovementStepsFromTile(T tile)
        {
            return _dico[tile].MovementSteps;
        }
        /// <summary>
        /// Get the tile movement cost count along the path between the target and a tile
        /// </summary>
        /// <param name="tile">A tile object</param>
        /// <returns>The movement cost</returns>
        public float GetMovementCostFromTile(T tile)
        {
            return _dico[tile].MovementCosts;
        }
        /// <summary>
        /// Get the grid coordinates for a tile
        /// </summary>
        /// <param name="tile">A tile object</param>
        /// <returns>The movement cost</returns>
        public Vector2 GetCoordinatesFromTile(T tile)
        {
            Node<T> node = _dico[tile];
            return new Vector2(node.X, node.Y);
        }
        /// <summary>
        /// Get all accessible tiles from target within maximum movement step
        /// </summary>
        /// <param name="movementStep"></param>
        /// <returns>An array of tiles</returns>
        public T[] GetAccessibleTilesFromTarget(int movementStep = 0)
        {
            return _flatMap.Where(n => movementStep > 0 ? n.MovementSteps <= movementStep && n.MovementSteps > 0 : n.MovementSteps > 0).Select(n => (T)n.Tile).ToArray();
        }
        /// <summary>
        /// Get all accessible tiles from target within maximum movement cost
        /// </summary>
        /// <param name="movementCost"></param>
        /// <returns>An array of tiles</returns>
        public T[] GetAccessibleTilesFromTarget(float movementCost = 0f)
        {
            return _flatMap.Where(n => movementCost > 0 ? n.MovementCosts <= movementCost && n.MovementCosts > 0 : n.MovementCosts > 0).Select(n => (T)n.Tile).ToArray();
        }
        /// <summary>
        /// Get all tiles along the path between a tile and the target
        /// </summary>
        /// <param name="tile">The start tile</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathToTarget(T tile)
        {
            Node<T> firstNode = _flatMap.First(n => EqualityComparer<T>.Default.Equals(n.Tile, tile));
            return GetPathToTarget(firstNode.X, firstNode.Y);
        }
        /// <summary>
        /// Get all tiles along the path between a tile and the target
        /// </summary>
        /// <param name="tileX">The start tile x coordinate</param>
        /// <param name="tileY">The start tile y coordinate</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathToTarget(int tileX, int tileY)
        {
            Node<T> node = _map[tileY, tileX];
            List<T> tiles = new List<T>() { node.Tile };
            while (!EqualityComparer<T>.Default.Equals(node.Tile, _target))
            {
                node = node.NextNode;
                tiles.Add(node.Tile);
            }
            return tiles.Select(x => x).ToArray();
        }
        /// <summary>
        /// Get all tiles along the path between the target and a tile
        /// </summary>
        /// <param name="tile">The destination tile</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathFromTarget(T tile)
        {
            return GetPathToTarget(tile).Reverse().ToArray();
        }
        /// <summary>
        /// Get all tiles along the path between the target and a tile
        /// </summary>
        /// <param name="tileX">The destination tile x coordinate</param>
        /// <param name="tileY">The destination tile y coordinate</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathFromTarget(int tileX, int tileY)
        {
            return GetPathToTarget(tileX, tileY).Reverse().ToArray();
        }
    }
    /// <summary>
    /// Helper static class for 2D grid operations
    /// </summary>
    public static class GridHelper
    {
        private static Node<T> GetNode<T>(Node<T>[,] map, int x, int y) where T : ITile
        {
            if (x > -1 && y > -1 && x < map.GetLength(1) && y < map.GetLength(0))
            {
                return map[y, x];
            }
            return null;
        }
        private static List<Node<T>> GetNeighbours<T>(Node<T>[,] map, int x, int y, bool allowDiagonals) where T : ITile
        {
            List<Node<T>> nodes = new List<Node<T>>();
            Node<T> nei;
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

        /// <summary>
        /// Get all tiles contained into a radius around a tile
        /// </summary>
        /// <typeparam name="T">The user defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tile</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesIntoARadius<T>(T[,] map, T center, int radius) where T : ITile
        {
            int startX = 0;
            int startY = 0;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (EqualityComparer<T>.Default.Equals(map[i, j], center))
                    {
                        startX = j;
                        startY = i;
                    }
                }
            }
            return GetTilesIntoARadius(map, startX, startY, radius);
        }
        /// <summary>
        /// Get all tiles contained into a radius around a tile
        /// </summary>
        /// <typeparam name="T">The user defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tile</param>
        /// <param name="centerX">The center tile x coordinate</param>
        /// <param name="centerY">The center tile y coordinate</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesIntoARadius<T>(T[,] map, int centerX, int centerY, int radius) where T : ITile
        {
            int top = Mathf.CeilToInt(centerY - radius),
                bottom = Mathf.FloorToInt(centerY + radius),
                left = Mathf.CeilToInt(centerX - radius),
                right = Mathf.FloorToInt(centerX + radius);
            List<T> list = new List<T>();
            for (int y = top; y <= bottom; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    float dx = centerX - x,
                    dy = centerY - y;
                    float distance_squared = dx * dx + dy * dy;
                    if (distance_squared <= radius * radius && x >= 0 && y >= 0 && x < map.GetLength(1) && y < map.GetLength(0))
                    {
                        list.Add((T)map[y, x]);
                    }
                }
            }
            return list.ToArray();
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tile</param>
        /// <param name="start">The start tile</param>
        /// <param name="stop">The stop tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T start, T stop) where T : ITile
        {
            T[] los = GetLineOfSight(map, start, stop);
            return EqualityComparer<T>.Default.Equals(los[los.Length - 1], stop);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tile</param>
        /// <param name="start">The start tile</param>
        /// <param name="stop">The stop tile</param>
        /// <param name="maxDistance">The maximum number of returned tiles along the line</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T start, T stop, int maxDistance = 0) where T : ITile
        {
            int startX = 0;
            int startY = 0;
            int stopX = 0;
            int stopY = 0;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (EqualityComparer<T>.Default.Equals(map[i, j], start))
                    {
                        startX = j;
                        startY = i;
                    }
                    if (EqualityComparer<T>.Default.Equals(map[i, j], stop))
                    {
                        stopX = j;
                        stopY = i;
                    }
                }
            }
            return GetLineOfSight<T>(map, startX, startY, stopX, stopY, maxDistance);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tile</param>
        /// <param name="startX">The start tile x coordinate</param>
        /// <param name="startY">The start tile y coordinate</param>
        /// <param name="stopX">The stop tile x coordinate</param>
        /// <param name="stopY">The stop tile y coordinate</param>
        /// <param name="maxDistance">The maximum number of returned tiles along the line</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, int startX, int startY, int stopX, int stopY, int maxDistance = 0) where T : ITile
        {
            Vector2Int p0 = new Vector2Int(startX, startY);
            Vector2Int p1 = new Vector2Int(stopX, stopY);
            int dx = p1.x - p0.x, dy = p1.y - p0.y;
            int nx = Mathf.Abs(dx), ny = Mathf.Abs(dy);
            int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

            Vector2Int p = new Vector2Int(p0.x, p0.y);
            List<T> points = new List<T> { map[p.y, p.x] };
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
                if (!map[p.y, p.x].IsWalkable || (maxDistance > 0 && Vector2Int.Distance(new Vector2Int(p.x, p.y), new Vector2Int(startX, startY)) > maxDistance))
                {
                    break;
                }

                points.Add(map[p.y, p.x]);
            }
            return points.ToArray();
        }
        /// <summary>
        /// Generates a PathMap object that will contain all the precalculated paths data for the entire grid
        /// </summary>
        /// <typeparam name="T">The user defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tile</param>
        /// <param name="target">The target tile for the paths calculation</param>
        /// <param name="allowDiagonals">Allow diagonals movements</param>
        /// <param name="diagonalWeightRatio">Diagonal movement weight</param>
        /// <returns>A PathMap object</returns>
        public static PathMap<T> GeneratePathMap<T>(T[,] map, T target, bool allowDiagonals = true, float diagonalWeightRatio = 1.5f) where T : ITile
        {
            int height = map.GetLength(0);
            int width = map.GetLength(1);
            Node<T> targetNode = null;
            Node<T>[,] nodeMap = new Node<T>[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    nodeMap[i, j] = new Node<T>(map[i, j], j, i);
                    if (EqualityComparer<T>.Default.Equals(map[i, j], target))
                    {
                        targetNode = nodeMap[i, j];
                    }
                }
            }
            PriorityQueueUnityPort.PriorityQueue<Node<T>, float> frontier = new();
            frontier.Enqueue(targetNode, 0);
            targetNode.NextNode = targetNode;
            targetNode.MovementSteps = 0;
            while (frontier.Count > 0)
            {
                Node<T> current = frontier.Dequeue();
                List<Node<T>> neibourgs = GetNeighbours(nodeMap, current.X, current.Y, allowDiagonals);
                foreach (Node<T> nei in neibourgs)
                {
                    bool isDiagonal = allowDiagonals && current.X != nei.X && current.Y != nei.Y;
                    float newDistance = current.MovementCosts + nei.Tile.Weight * (isDiagonal ? diagonalWeightRatio : 1f);
                    if (nei.NextNode == null || newDistance < nei.MovementCosts)
                    {
                        frontier.Enqueue(nei, newDistance);
                        nei.NextNode = current;
                        nei.NextDirection = new Vector2(nei.NextNode.X > nei.X ? 1 : (nei.NextNode.X < nei.X ? -1 : 0f), nei.NextNode.Y > nei.Y ? 1 : (nei.NextNode.Y < nei.Y ? -1 : 0f));
                        nei.MovementSteps = current.MovementSteps + 1;
                        nei.MovementCosts = newDistance;
                    }
                }
            }
            return new PathMap<T>(nodeMap, target);
        }
    }
}