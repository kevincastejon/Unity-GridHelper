
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KevinCastejon.GridHelper
{
    /// <summary>
    /// An interface that the user-defined tile class has to implement in order to work with most of this library's methods
    /// </summary>
    public interface ITile
    {
        /// <summary>
        /// Is the tile walkable (or "transparent" for line of sight)
        /// </summary>
        public bool IsWalkable
        {
            get;
        }
        /// <summary>
        /// The tile movement cost (minimum 1f)
        /// </summary>
        public float Weight
        {
            get;
        }
        /// <summary>
        /// The tile horizontal coordinate
        /// </summary>
        public int X
        {
            get;
        }
        /// <summary>
        /// The tile vertical coordinate
        /// </summary>
        public int Y
        {
            get;
        }
    }
    internal class Node<T> where T : ITile
    {
        internal Node(T tile)
        {
            _tile = tile;
            IsWalkable = tile != null && tile.IsWalkable;
            Weight = tile == null ? 1f : tile.Weight;
        }

        private T _tile;
        private Node<T> _next;
        private Vector2Int _nextDirection;
        private float _distanceToTarget;

        internal T Tile { get => _tile; set => _tile = value; }
        internal Node<T> NextNode { get => _next; set => _next = value; }
        internal Vector2Int NextDirection { get => _nextDirection; set => _nextDirection = value; }
        internal float DistanceToTarget { get => _distanceToTarget; set => _distanceToTarget = value; }
        internal bool IsWalkable { get; set; }
        internal float Weight { get; set; }
    }
    /// <summary>
    /// An object containing all the calculated paths data of a tile grid
    /// </summary>
    /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
    public class PathMap<T> where T : ITile
    {
        private readonly Dictionary<T, Node<T>> _dico;
        private readonly List<T> _accessibleTiles;
        private readonly T _target;
        private readonly float _maxDistance;

        internal PathMap(Dictionary<T, Node<T>> accessibleTilesDico, List<T> accessibleTiles, T target, float maxDistance)
        {
            _dico = accessibleTilesDico;
            _accessibleTiles = accessibleTiles;
            _target = target;
            _maxDistance = maxDistance;
        }
        /// <summary>
        /// The tile that has been used as the target to generate this PathMap
        /// </summary>
        public T Target { get => _target; }
        /// <summary>
        /// The maxDistance parameter value that has been used to generate this PathMap
        /// </summary>
        public float MaxDistance { get => _maxDistance; }
        /// <summary>
        /// Is the tile contained into the this PathMap. Usefull to check if the tile is usable with this PathMap's methods
        /// </summary>
        /// <param name="tile">The tile to check</param>
        /// <returns>A boolean that is true if the tile is contained on the PathMap, false otherwise</returns>
        public bool IsTileIntoPathMap(T tile)
        {
            if (tile == null)
            {
                return false;
            }
            return _dico.ContainsKey(tile);
        }
        /// <summary>
        /// Get the next tile on the path between the target and a tile.
        /// </summary>
        /// <param name="tile">A tile</param>
        /// <returns>A tile object</returns>
        public T GetNextTileFromTile(T tile)
        {
            if (!tile.IsWalkable)
            {
                throw new System.Exception("Do not call GetNextTileFromTile() method with unwalkable tile as parameter");
            }
            if (!_dico.ContainsKey(tile))
            {
                Debug.LogWarning("You're trying to get the next tile on the path to the target from a tile that is not contained into the PathMap");
                return default;
            }
            return _dico[tile].NextNode.Tile;
        }
        /// <summary>
        /// Get the next tile on the path between the target and a tile.
        /// </summary>
        /// <param name="tile">The tile</param>
        /// <returns>A Vector2Int direction</returns>
        public Vector2Int GetNextTileDirectionFromTile(T tile)
        {
            if (!tile.IsWalkable)
            {
                throw new System.Exception("Do not call GetNextTileDirectionFromTile() method with unwalkable tile as parameter");
            }
            if (!_dico.ContainsKey(tile))
            {
                Debug.LogWarning("You're trying to get the distance to the target from a tile that is not contained into the PathMap");
                return Vector2Int.zero;
            }
            return _dico[tile].NextDirection;
        }
        /// <summary>
        /// Get the distance to the target from a tile.
        /// </summary>
        /// <param name="tile">A tile object</param>
        /// <returns>The distance to the target</returns>
        public float GetDistanceToTargetFromTile(T tile)
        {
            if (!tile.IsWalkable)
            {
                throw new System.Exception("Do not call GetDistanceToTargetFromTile() method with unwalkable tile as parameter");
            }
            if (!_dico.ContainsKey(tile))
            {
                Debug.LogWarning("You're trying to get the distance to the target from a tile that is not contained into the PathMap");
                return -1f;
            }
            return _dico[tile].DistanceToTarget;
        }
        /// <summary>
        /// Get all the accessible tiles from the target tile
        /// </summary>
        /// <returns>An array of tiles</returns>
        public T[] GetAccessibleTiles()
        {
            return _accessibleTiles.ToArray();
        }
        /// <summary>
        /// Get all the tiles on the path from a tile to the target.
        /// </summary>
        /// <param name="tile">The start tile</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathToTarget(T tile)
        {
            if (!tile.IsWalkable)
            {
                throw new System.Exception("Do not call GetPathToTarget() method with unwalkable tile as parameter");
            }
            if (!_dico.ContainsKey(tile))
            {
                Debug.LogWarning("You're trying to calculate a path from a tile that is not contained into the PathMap");
                return new T[0];
            }
            Node<T> node = _dico[tile];
            List<T> tiles = new List<T>() { node.Tile };
            while (!EqualityComparer<T>.Default.Equals(node.Tile, _target))
            {
                node = node.NextNode;
                tiles.Add(node.Tile);
            }
            return tiles.Select(x => x).ToArray();
        }
        /// <summary>
        /// Get all the tiles on the path from the target to a tile.
        /// </summary>
        /// <param name="tile">The destination tile</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathFromTarget(T tile)
        {
            return GetPathToTarget(tile).Reverse().ToArray();
        }
    }
    /// <summary>
    /// Helper static class for 2D grid operations
    /// </summary>
    public static class GridHelper
    {
        private static bool GetTile<T>(T[,] map, int x, int y, out T tile) where T : ITile
        {
            if (x > -1 && y > -1 && x < map.GetLength(1) && y < map.GetLength(0))
            {
                tile = map[y, x];
                return true;
            }
            tile = default;
            return false;
        }
        private static List<T> GetTileNeighbours<T>(T[,] map, int x, int y, bool allowDiagonals) where T : ITile
        {
            List<T> nodes = new List<T>();
            T nei;
            bool leftWalkable = false;
            bool rightWalkable = false;
            bool topWalkable = false;
            bool bottomWalkable = false;
            if (GetTile(map, x - 1, y, out nei))
            {
                leftWalkable = nei != null && nei.IsWalkable;
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x, y - 1, out nei))
            {
                bottomWalkable = nei != null && nei.IsWalkable;
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x, y + 1, out nei))
            {
                topWalkable = nei != null && nei.IsWalkable;
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y, out nei))
            {
                rightWalkable = nei != null && nei.IsWalkable;
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x - 1, y - 1, out nei))
            {
                if (allowDiagonals && leftWalkable && bottomWalkable && nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x - 1, y + 1, out nei))
            {
                if (allowDiagonals && leftWalkable && topWalkable && nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y + 1, out nei))
            {
                if (allowDiagonals && rightWalkable && topWalkable && nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y - 1, out nei))
            {
                if (allowDiagonals && rightWalkable && bottomWalkable && nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }

            return nodes;
        }
        public class Extraction
        {
            /// <summary>
            /// Get all tiles contained into a rectangle around a tile
            /// </summary>
            /// <typeparam name="T">Any object type</typeparam>
            /// <param name="map">A two-dimensional array of any objects</param>
            /// <param name="center">The center tile</param>
            /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetTilesInARectangle<T>(T[,] map, T center, Vector2Int rectangleSize) where T : ITile
            {
                return GetTilesInARectangle(map, center, rectangleSize.x, rectangleSize.y);
            }
            /// <summary>
            /// Get all tiles contained into a rectangle around a tile
            /// </summary>
            /// <typeparam name="T">Any object type</typeparam>
            /// <param name="map">A two-dimensional array of any objects</param>
            /// <param name="center">The center tile</param>
            /// <param name="rectangleSizeX">The rectangle horizontal size</param>
            /// <param name="rectangleSizeY">The rectangle vertical size</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetTilesInARectangle<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY) where T : ITile
            {
                int top = Mathf.Max(center.Y - rectangleSizeY, 0),
                    bottom = Mathf.Min(center.Y + rectangleSizeY + 1, map.GetLength(0)),
                    left = Mathf.Max(center.X - rectangleSizeX, 0),
                    right = Mathf.Min(center.X + rectangleSizeX + 1, map.GetLength(1));
                List<T> list = new List<T>();
                for (int i = top; i < bottom; i++)
                {
                    for (int j = left; j < right; j++)
                    {
                        if (map[i, j] != null)
                        {
                            list.Add(map[i, j]);
                        }
                    }
                }
                return list.ToArray();
            }
            /// <summary>
            /// Get all walkable tiles contained into a rectangle around a tile
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="center">The center tile</param>
            /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetWalkableTilesInARectangle<T>(T[,] map, T center, Vector2Int rectangleSize) where T : ITile
            {
                return GetWalkableTilesInARectangle(map, center, rectangleSize.x, rectangleSize.y);
            }
            /// <summary>
            /// Get all walkable tiles contained into a rectangle around a tile
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="center">The center tile</param>
            /// <param name="rectangleSizeX">The rectangle horizontal size</param>
            /// <param name="rectangleSizeY">The rectangle vertical size</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetWalkableTilesInARectangle<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY) where T : ITile
            {
                int top = Mathf.Max(center.Y - rectangleSizeY, 0),
                    bottom = Mathf.Min(center.Y + rectangleSizeY + 1, map.GetLength(0)),
                    left = Mathf.Max(center.X - rectangleSizeX, 0),
                    right = Mathf.Min(center.X + rectangleSizeX + 1, map.GetLength(1));
                List<T> list = new List<T>();
                for (int i = top; i < bottom; i++)
                {
                    for (int j = left; j < right; j++)
                    {
                        if (map[i, j] != null && map[i, j].IsWalkable)
                        {
                            list.Add(map[i, j]);
                        }
                    }
                }
                return list.ToArray();
            }
            /// <summary>
            /// Get all tiles on a rectangle outline around a tile
            /// </summary>
            /// <typeparam name="T">Any object type</typeparam>
            /// <param name="map">A two-dimensional array of any objects</param>
            /// <param name="center">The center tile</param>
            /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetTilesOnARectangleOutline<T>(T[,] map, T center, Vector2Int rectangleSize) where T : ITile
            {
                return GetTilesOnARectangleOutline(map, center, rectangleSize.x, rectangleSize.y);
            }
            /// <summary>
            /// Get all tiles on a rectangle outline around a tile
            /// </summary>
            /// <typeparam name="T">Any object type</typeparam>
            /// <param name="map">A two-dimensional array of any objects</param>
            /// <param name="center">The center tile</param>
            /// <param name="rectangleSizeX">The rectangle horizontal size</param>
            /// <param name="rectangleSizeY">The rectangle vertical size</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetTilesOnARectangleOutline<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY) where T : ITile
            {
                int top = center.Y - rectangleSizeY,
                    bottom = center.Y + rectangleSizeY + 1,
                    left = center.X - rectangleSizeX,
                    right = center.X + rectangleSizeX + 1;
                List<T> list = new List<T>();
                for (int i = top; i < bottom; i++)
                {
                    for (int j = left; j < right; j++)
                    {
                        if (i < 0 || i >= map.GetLength(0) || j < 0 || j >= map.GetLength(1))
                        {
                            continue;
                        }
                        if (i == top || i == bottom - 1 || j == left || j == right - 1)
                        {
                            if (map[i, j] != null)
                            {
                                list.Add(map[i, j]);
                            }
                        }
                    }
                }
                return list.ToArray();
            }
            /// <summary>
            /// Get all walkable tiles on a rectangle outline around a tile
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="center">The center tile</param>
            /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetWalkableTilesOnARectangleOutline<T>(T[,] map, T center, Vector2Int rectangleSize) where T : ITile
            {
                return GetWalkableTilesOnARectangleOutline(map, center, rectangleSize.x, rectangleSize.y);
            }
            /// <summary>
            /// Get all walkable tiles on a rectangle outline around a tile
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="center">The center tile</param>
            /// <param name="rectangleSizeX">The rectangle horizontal size</param>
            /// <param name="rectangleSizeY">The rectangle vertical size</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetWalkableTilesOnARectangleOutline<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY) where T : ITile
            {
                int top = center.Y - rectangleSizeY,
                    bottom = center.Y + rectangleSizeY + 1,
                    left = center.X - rectangleSizeX,
                    right = center.X + rectangleSizeX + 1;
                List<T> list = new List<T>();
                for (int i = top; i < bottom; i++)
                {
                    for (int j = left; j < right; j++)
                    {
                        if (i < 0 || i >= map.GetLength(0) || j < 0 || j >= map.GetLength(1))
                        {
                            continue;
                        }
                        if (map[i, j] != null && map[i, j].IsWalkable && (i == top || i == bottom - 1 || j == left || j == right - 1))
                        {
                            list.Add(map[i, j]);
                        }
                    }
                }
                return list.ToArray();
            }
            /// <summary>
            /// Get all tiles contained into a radius around a tile
            /// </summary>
            /// <typeparam name="T">Any object type</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="center">The center tile</param>
            /// <param name="radius">The radius</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetTilesInARadius<T>(T[,] map, T center, int radius) where T : ITile
            {
                int top = Mathf.Max(center.Y - radius, 0),
                    bottom = Mathf.Min(center.Y + radius + 1, map.GetLength(0));
                List<T> list = new List<T>();
                for (int y = top; y < bottom; y++)
                {
                    int dy = y - center.Y;
                    float dx = Mathf.Sqrt((float)radius * radius - (float)dy * dy);
                    int left = Mathf.Max(Mathf.CeilToInt(center.X - dx), 0),
                        right = Mathf.Min(Mathf.FloorToInt(center.X + dx + 1), map.GetLength(1));
                    for (int x = left; x < right; x++)
                    {
                        if (map[y, x] != null)
                        {
                            list.Add(map[y, x]);
                        }
                    }
                }
                return list.ToArray();
            }
            /// <summary>
            /// Get all tiles on a radius outline around a tile
            /// </summary>
            /// <typeparam name="T">Any object type</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="center">The center tile </param>
            /// <param name="radius">The radius</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetTilesOnARadiusOutline<T>(T[,] map, T center, int radius) where T : ITile
            {
                int top = Mathf.Max(center.Y - radius, 0),
                    bottom = Mathf.Min(center.Y + radius + 1, map.GetLength(0));
                List<T> list = new List<T>();
                for (int y = top; y < bottom; y++)
                {
                    for (int r = 0; r <= Mathf.FloorToInt(radius * Mathf.Sqrt(0.5f)); r++)
                    {
                        int dd = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - r * r));
                        Vector2Int a = new Vector2Int(center.X - dd, center.Y + r);
                        if (a.y >= 0 && a.y < map.GetLength(0) && a.x >= 0 && a.x < map.GetLength(1) && map[a.y, a.x] != null && !list.Contains(map[a.y, a.x])) list.Add(map[a.y, a.x]);
                        Vector2Int b = new Vector2Int(center.X + dd, center.Y + r);
                        if (b.y >= 0 && b.y < map.GetLength(0) && b.x >= 0 && b.x < map.GetLength(1) && map[b.y, b.x] != null && !list.Contains(map[b.y, b.x])) list.Add(map[b.y, b.x]);
                        Vector2Int c = new Vector2Int(center.X - dd, center.Y - r);
                        if (c.y >= 0 && c.y < map.GetLength(0) && c.x >= 0 && c.x < map.GetLength(1) && map[c.y, c.x] != null && !list.Contains(map[c.y, c.x])) list.Add(map[c.y, c.x]);
                        Vector2Int d = new Vector2Int(center.X + dd, center.Y - r);
                        if (d.y >= 0 && d.y < map.GetLength(0) && d.x >= 0 && d.x < map.GetLength(1) && map[d.y, d.x] != null && !list.Contains(map[d.y, d.x])) list.Add(map[d.y, d.x]);
                        Vector2Int e = new Vector2Int(center.X + r, center.Y - dd);
                        if (e.y >= 0 && e.y < map.GetLength(0) && e.x >= 0 && e.x < map.GetLength(1) && map[e.y, e.x] != null && !list.Contains(map[e.y, e.x])) list.Add(map[e.y, e.x]);
                        Vector2Int f = new Vector2Int(center.X + r, center.Y + dd);
                        if (f.y >= 0 && f.y < map.GetLength(0) && f.x >= 0 && f.x < map.GetLength(1) && map[f.y, f.x] != null && !list.Contains(map[f.y, f.x])) list.Add(map[f.y, f.x]);
                        Vector2Int g = new Vector2Int(center.X - r, center.Y - dd);
                        if (g.y >= 0 && g.y < map.GetLength(0) && g.x >= 0 && g.x < map.GetLength(1) && map[g.y, g.x] != null && !list.Contains(map[g.y, g.x])) list.Add(map[g.y, g.x]);
                        Vector2Int h = new Vector2Int(center.X - r, center.Y + dd);
                        if (h.y >= 0 && h.y < map.GetLength(0) && h.x >= 0 && h.x < map.GetLength(1) && map[h.y, h.x] != null && !list.Contains(map[h.y, h.x])) list.Add(map[h.y, h.x]);
                    }
                }
                return list.ToArray();
            }
            /// <summary>
            /// Get all walkable tiles contained into a radius around a tile
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="center">The center tile</param>
            /// <param name="radius">The radius</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetWalkableTilesInARadius<T>(T[,] map, T center, int radius) where T : ITile
            {
                int top = Mathf.CeilToInt(center.Y - radius),
                    bottom = Mathf.FloorToInt(center.Y + radius),
                    left = Mathf.CeilToInt(center.X - radius),
                    right = Mathf.FloorToInt(center.X + radius);
                List<T> list = new List<T>();
                for (int y = top; y <= bottom; y++)
                {
                    for (int x = left; x <= right; x++)
                    {
                        float dx = center.X - x,
                        dy = center.Y - y;
                        float distance_squared = dx * dx + dy * dy;
                        if (distance_squared <= radius * radius && x >= 0 && y >= 0 && x < map.GetLength(1) && y < map.GetLength(0) && map[y, x].IsWalkable)
                        {
                            list.Add(map[y, x]);
                        }
                    }
                }
                return list.ToArray();
            }
            /// <summary>
            /// Get all walkable tiles on a radius outline around a tile
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="center">The center tile</param>
            /// <param name="radius">The radius</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetWalkableTilesOnARadiusOutline<T>(T[,] map, T center, int radius) where T : ITile
            {
                int top = Mathf.Max(center.Y - radius, 0),
                    bottom = Mathf.Min(center.Y + radius + 1, map.GetLength(0));
                List<T> list = new List<T>();
                for (int y = top; y < bottom; y++)
                {
                    for (int r = 0; r <= Mathf.FloorToInt(radius * Mathf.Sqrt(0.5f)); r++)
                    {
                        int dd = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - r * r));
                        Vector2Int a = new Vector2Int(center.X - dd, center.Y + r);
                        if (a.y >= 0 && a.y < map.GetLength(0) && a.x >= 0 && a.x < map.GetLength(1) && map[a.y, a.x] != null && map[a.y, a.x].IsWalkable && !list.Contains(map[a.y, a.x])) list.Add(map[a.y, a.x]);
                        Vector2Int b = new Vector2Int(center.X + dd, center.Y + r);
                        if (b.y >= 0 && b.y < map.GetLength(0) && b.x >= 0 && b.x < map.GetLength(1) && map[b.y, b.x] != null && map[b.y, b.x].IsWalkable && !list.Contains(map[b.y, b.x])) list.Add(map[b.y, b.x]);
                        Vector2Int c = new Vector2Int(center.X - dd, center.Y - r);
                        if (c.y >= 0 && c.y < map.GetLength(0) && c.x >= 0 && c.x < map.GetLength(1) && map[c.y, c.x] != null && map[c.y, c.x].IsWalkable && !list.Contains(map[c.y, c.x])) list.Add(map[c.y, c.x]);
                        Vector2Int d = new Vector2Int(center.X + dd, center.Y - r);
                        if (d.y >= 0 && d.y < map.GetLength(0) && d.x >= 0 && d.x < map.GetLength(1) && map[d.y, d.x] != null && map[d.y, d.x].IsWalkable && !list.Contains(map[d.y, d.x])) list.Add(map[d.y, d.x]);
                        Vector2Int e = new Vector2Int(center.X + r, center.Y - dd);
                        if (e.y >= 0 && e.y < map.GetLength(0) && e.x >= 0 && e.x < map.GetLength(1) && map[e.y, e.x] != null && map[e.y, e.x].IsWalkable && !list.Contains(map[e.y, e.x])) list.Add(map[e.y, e.x]);
                        Vector2Int f = new Vector2Int(center.X + r, center.Y + dd);
                        if (f.y >= 0 && f.y < map.GetLength(0) && f.x >= 0 && f.x < map.GetLength(1) && map[f.y, f.x] != null && map[f.y, f.x].IsWalkable && !list.Contains(map[f.y, f.x])) list.Add(map[f.y, f.x]);
                        Vector2Int g = new Vector2Int(center.X - r, center.Y - dd);
                        if (g.y >= 0 && g.y < map.GetLength(0) && g.x >= 0 && g.x < map.GetLength(1) && map[g.y, g.x] != null && map[g.y, g.x].IsWalkable && !list.Contains(map[g.y, g.x])) list.Add(map[g.y, g.x]);
                        Vector2Int h = new Vector2Int(center.X - r, center.Y + dd);
                        if (h.y >= 0 && h.y < map.GetLength(0) && h.x >= 0 && h.x < map.GetLength(1) && map[h.y, h.x] != null && map[h.y, h.x].IsWalkable && !list.Contains(map[h.y, h.x])) list.Add(map[h.y, h.x]);
                    }
                }
                return list.ToArray();
            }
            /// <summary>
            /// Get all tiles on a line between two tiles
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="start">The start tile</param>
            /// <param name="stop">The stop tile</param>
            /// <param name="maxDistance">The maximum distance from the start tile</param>
            /// <returns>An array of tiles</returns>
        }
        public class Raycasting
        {
            public static T[] GetTilesOnALine<T>(T[,] map, T start, T stop, float maxDistance = 0f) where T : ITile
            {
                Vector2Int p0 = new Vector2Int(start.X, start.Y);
                Vector2Int p1 = new Vector2Int(stop.X, stop.Y);
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
                    if (maxDistance > 0 && Vector2Int.Distance(new Vector2Int(p.x, p.y), new Vector2Int(start.X, start.Y)) > maxDistance)
                    {
                        break;
                    }
                    if (map[p.y, p.x] != null)
                    {
                        points.Add(map[p.y, p.x]);
                    }
                }
                return points.ToArray();
            }
            /// <summary>
            /// Get all walkable tiles on a line between two tiles
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="start">The start tile</param>
            /// <param name="stop">The stop tile</param>
            /// <param name="maxDistance">The maximum distance from the start tile</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetWalkableTilesOnALine<T>(T[,] map, T start, T stop, float maxDistance = 0f) where T : ITile
            {
                Vector2Int p0 = new Vector2Int(start.X, start.Y);
                Vector2Int p1 = new Vector2Int(stop.X, stop.Y);
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
                    if (maxDistance > 0 && Vector2Int.Distance(new Vector2Int(p.x, p.y), new Vector2Int(start.X, start.Y)) > maxDistance)
                    {
                        break;
                    }
                    if (map[p.y, p.x] == null || !map[p.y, p.x].IsWalkable)
                    {
                        continue;
                    }

                    points.Add(map[p.y, p.x]);
                }
                return points.ToArray();
            }
            /// <summary>
            /// Is the line of sight clear between two tiles
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="start">The start tile</param>
            /// <param name="stop">The stop tile</param>
            /// <returns>An array of tiles</returns>
            public static bool IsLineOfSightClear<T>(T[,] map, T start, T stop, float maxDistance = 0f) where T : ITile
            {
                Vector2Int p0 = new Vector2Int(start.X, start.Y);
                Vector2Int p1 = new Vector2Int(stop.X, stop.Y);
                int dx = p1.x - p0.x, dy = p1.y - p0.y;
                int nx = Mathf.Abs(dx), ny = Mathf.Abs(dy);
                int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

                Vector2Int p = new Vector2Int(p0.x, p0.y);
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
                    if (map[p.y, p.x] == null || !map[p.y, p.x].IsWalkable)
                    {
                        return false;
                    }
                    if (maxDistance > 0 && Vector2Int.Distance(new Vector2Int(p.x, p.y), new Vector2Int(start.X, start.Y)) > maxDistance)
                    {
                        return true;
                    }
                }
                return true;
            }
            /// <summary>
            /// Get all tiles on a line between two tiles
            /// </summary>
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="start">The start tile</param>
            /// <param name="stop">The stop tile</param>
            /// <param name="maxDistance">The maximum distance from the start tile</param>
            /// <returns>An array of tiles</returns>
            public static T[] GetLineOfSight<T>(T[,] map, T start, T stop, float maxDistance = 0f) where T : ITile
            {
                Vector2Int p0 = new Vector2Int(start.X, start.Y);
                Vector2Int p1 = new Vector2Int(stop.X, stop.Y);
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
                    if (map[p.y, p.x] == null || !map[p.y, p.x].IsWalkable || (maxDistance > 0 && Vector2Int.Distance(new Vector2Int(p.x, p.y), new Vector2Int(start.X, start.Y)) > maxDistance))
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
            /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
            /// <param name="map">A two-dimensional array of tiles</param>
            /// <param name="target">The target tile for the paths calculation</param>
            /// <param name="allowDiagonals">Allow diagonals movements</param>
            /// <param name="diagonalWeightRatio">Diagonal movement weight</param>
            /// <returns>A PathMap object</returns>
        }
        public class Pathfinding
        {
            public static PathMap<T> GeneratePathMap<T>(T[,] map, T target, float maxDistance = 0f, bool allowDiagonals = true, float diagonalWeightRatio = 1.5f) where T : ITile
            {
                if (!target.IsWalkable)
                {
                    throw new System.Exception("Do not try to generate a PathMap with an unwalkable tile as the target");
                }
                Node<T> targetNode = new Node<T>(target);
                Dictionary<T, Node<T>> accessibleTilesDico = new Dictionary<T, Node<T>>() { { target, targetNode } };
                List<T> accessibleTiles = new List<T>() { target };
                PriorityQueueUnityPort.PriorityQueue<Node<T>, float> frontier = new();
                frontier.Enqueue(targetNode, 0);
                targetNode.NextNode = targetNode;
                targetNode.DistanceToTarget = 0f;
                int limit = 0;
                while (frontier.Count > 0 && limit < 1000)
                {
                    Node<T> current = frontier.Dequeue();
                    List<T> neighbourgs = GetTileNeighbours(map, current.Tile.X, current.Tile.Y, allowDiagonals);
                    foreach (T neiTile in neighbourgs)
                    {
                        Node<T> nei = accessibleTilesDico.ContainsKey(neiTile) ? accessibleTilesDico[neiTile] : new Node<T>(neiTile);
                        bool isDiagonal = allowDiagonals && current.Tile.X != nei.Tile.X && current.Tile.Y != nei.Tile.Y;
                        float newDistance = current.DistanceToTarget + nei.Tile.Weight * (isDiagonal ? diagonalWeightRatio : 1f);
                        if (maxDistance > 0f && newDistance > maxDistance)
                        {
                            continue;
                        }
                        if (nei.NextNode == null || newDistance < nei.DistanceToTarget)
                        {
                            if (!accessibleTilesDico.ContainsKey(nei.Tile))
                            {
                                accessibleTilesDico.Add(nei.Tile, nei);
                                accessibleTiles.Add(nei.Tile);
                            }
                            frontier.Enqueue(nei, newDistance);
                            nei.NextNode = current;
                            nei.NextDirection = new Vector2Int(nei.NextNode.Tile.X > nei.Tile.X ? 1 : (nei.NextNode.Tile.X < nei.Tile.X ? -1 : 0), nei.NextNode.Tile.Y > nei.Tile.Y ? 1 : (nei.NextNode.Tile.Y < nei.Tile.Y ? -1 : 0));
                            nei.DistanceToTarget = newDistance;
                        }
                    }
                    limit++;
                }
                return new PathMap<T>(accessibleTilesDico, accessibleTiles, target, maxDistance);
            }
        }
    }
}
