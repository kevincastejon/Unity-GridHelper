using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Helper static classes for 2D grid operations
/// </summary>
namespace KevinCastejon.GridHelper
{
    /// <summary>
    /// An interface that the user-defined tile object has to implement in order to work with most of this library's methods
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
            Weight = tile == null ? 1f : Mathf.Max(tile.Weight, 1f);
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
        /// Is the tile is accessible from the target into this this PathMap. Usefull to check if the tile is usable as a parameter for this PathMap's methods.
        /// </summary>
        /// <param name="tile">The tile to check</param>
        /// <returns>A boolean that is true if the tile is contained into the PathMap, false otherwise</returns>
        public bool IsTileAccessible(T tile)
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
            if (!IsTileAccessible(tile))
            {
                throw new System.Exception("Do not call PathMap method with an inaccessible tile");
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
            if (!IsTileAccessible(tile))
            {
                throw new System.Exception("Do not call PathMap method with an inaccessible tile");
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
            if (!IsTileAccessible(tile))
            {
                throw new System.Exception("Do not call PathMap method with an inaccessible tile");
            }
            return _dico[tile].DistanceToTarget;
        }
        /// <summary>
        /// Get all the accessible tiles from the target tile
        /// </summary>
        /// <param name="includeTarget">Include the target tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public T[] GetAccessibleTiles(bool includeTarget = true)
        {
            if (includeTarget)
            {
                return _accessibleTiles.ToArray();
            }
            return _accessibleTiles.Where(t => !EqualityComparer<T>.Default.Equals(t, _target)).ToArray();
        }
        /// <summary>
        /// Get all the tiles on the path from a tile to the target.
        /// </summary>
        /// <param name="startTile">The start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeTarget">Include the target tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathToTarget(T startTile, bool includeStart = true, bool includeTarget = true)
        {
            if (!startTile.IsWalkable)
            {
                throw new System.Exception("Do not call GetPathToTarget() method with unwalkable tile as parameter");
            }
            if (!IsTileAccessible(startTile))
            {
                throw new System.Exception("Do not call PathMap method with an inaccessible tile");
            }
            Node<T> node = _dico[startTile];
            List<T> tiles = new List<T>() { node.Tile };
            while (!EqualityComparer<T>.Default.Equals(node.Tile, _target))
            {
                node = node.NextNode;
                if (includeStart || !EqualityComparer<T>.Default.Equals(node.Tile, startTile) && includeTarget || !EqualityComparer<T>.Default.Equals(node.Tile, _target))
                {
                    tiles.Add(node.Tile);
                }
            }
            return tiles.Select(x => x).ToArray();
        }
        /// <summary>
        /// Get all the tiles on the path from the target to a tile.
        /// </summary>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <param name="includeTarget">Include the target tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathFromTarget(T destinationTile, bool includeDestination = true, bool includeTarget = true)
        {
            return GetPathToTarget(destinationTile, includeDestination, includeTarget).Reverse().ToArray();
        }
    }
    /// <summary>
    /// Allows you to extract tiles on a grid using rectangular or circular shapes
    /// </summary>
    public class Extraction
    {
        private static T[] ExtractRectangle<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, bool includeCenter, bool includeWalls) where T : ITile
        {
            int bottom = Mathf.Max(center.Y - rectangleSizeY, 0),
                top = Mathf.Min(center.Y + rectangleSizeY + 1, map.GetLength(0)),
                left = Mathf.Max(center.X - rectangleSizeX, 0),
                right = Mathf.Min(center.X + rectangleSizeX + 1, map.GetLength(1));
            List<T> list = new List<T>();
            for (int i = bottom; i < top; i++)
            {
                for (int j = left; j < right; j++)
                {
                    if (map[i, j] != null && (includeWalls || map[i, j].IsWalkable) && (includeCenter || !EqualityComparer<T>.Default.Equals(map[i, j], center)))
                    {
                        list.Add(map[i, j]);
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractRectangleOutline<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, bool includeWalls) where T : ITile
        {
            int bottom = center.Y - rectangleSizeY,
                top = center.Y + rectangleSizeY + 1,
                left = center.X - rectangleSizeX,
                right = center.X + rectangleSizeX + 1;
            List<T> list = new List<T>();
            for (int i = bottom; i < top; i++)
            {
                for (int j = left; j < right; j++)
                {
                    if (i < 0 || i >= map.GetLength(0) || j < 0 || j >= map.GetLength(1))
                    {
                        continue;
                    }
                    if (map[i, j] != null && (includeWalls || map[i, j].IsWalkable) && (i == top - 1 || i == bottom || j == left || j == right - 1))
                    {
                        list.Add(map[i, j]);
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractRadius<T>(T[,] map, T center, int radius, bool includeCenter, bool includeWalls) where T : ITile
        {
            int bottom = Mathf.Max(center.Y - radius, 0),
                top = Mathf.Min(center.Y + radius + 1, map.GetLength(0));
            List<T> list = new List<T>();
            for (int y = bottom; y < top; y++)
            {
                int dy = y - center.Y;
                float dx = Mathf.Sqrt((float)radius * radius - (float)dy * dy);
                int left = Mathf.Max(Mathf.CeilToInt(center.X - dx), 0),
                    right = Mathf.Min(Mathf.FloorToInt(center.X + dx + 1), map.GetLength(1));
                for (int x = left; x < right; x++)
                {
                    if (map[y, x] != null && (includeWalls || map[y, x].IsWalkable) && (includeCenter || !EqualityComparer<T>.Default.Equals(map[y, x], center)))
                    {
                        list.Add(map[y, x]);
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractRadiusOutline<T>(T[,] map, T center, int radius, bool includeWalls) where T : ITile
        {
            int bottom = Mathf.Max(center.Y - radius, 0),
                top = Mathf.Min(center.Y + radius + 1, map.GetLength(0));
            List<T> list = new List<T>();
            for (int y = bottom; y < top; y++)
            {
                for (int r = 0; r <= Mathf.FloorToInt(radius * Mathf.Sqrt(0.5f)); r++)
                {
                    int dd = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - r * r));
                    Vector2Int a = new Vector2Int(center.X - dd, center.Y + r);
                    if (a.y >= 0 && a.y < map.GetLength(0) && a.x >= 0 && a.x < map.GetLength(1) && map[a.y, a.x] != null && (includeWalls || map[a.y, a.x].IsWalkable) && !list.Contains(map[a.y, a.x])) list.Add(map[a.y, a.x]);
                    Vector2Int b = new Vector2Int(center.X + dd, center.Y + r);
                    if (b.y >= 0 && b.y < map.GetLength(0) && b.x >= 0 && b.x < map.GetLength(1) && map[b.y, b.x] != null && (includeWalls || map[b.y, b.x].IsWalkable) && !list.Contains(map[b.y, b.x])) list.Add(map[b.y, b.x]);
                    Vector2Int c = new Vector2Int(center.X - dd, center.Y - r);
                    if (c.y >= 0 && c.y < map.GetLength(0) && c.x >= 0 && c.x < map.GetLength(1) && map[c.y, c.x] != null && (includeWalls || map[c.y, c.x].IsWalkable) && !list.Contains(map[c.y, c.x])) list.Add(map[c.y, c.x]);
                    Vector2Int d = new Vector2Int(center.X + dd, center.Y - r);
                    if (d.y >= 0 && d.y < map.GetLength(0) && d.x >= 0 && d.x < map.GetLength(1) && map[d.y, d.x] != null && (includeWalls || map[d.y, d.x].IsWalkable) && !list.Contains(map[d.y, d.x])) list.Add(map[d.y, d.x]);
                    Vector2Int e = new Vector2Int(center.X + r, center.Y - dd);
                    if (e.y >= 0 && e.y < map.GetLength(0) && e.x >= 0 && e.x < map.GetLength(1) && map[e.y, e.x] != null && (includeWalls || map[e.y, e.x].IsWalkable) && !list.Contains(map[e.y, e.x])) list.Add(map[e.y, e.x]);
                    Vector2Int f = new Vector2Int(center.X + r, center.Y + dd);
                    if (f.y >= 0 && f.y < map.GetLength(0) && f.x >= 0 && f.x < map.GetLength(1) && map[f.y, f.x] != null && (includeWalls || map[f.y, f.x].IsWalkable) && !list.Contains(map[f.y, f.x])) list.Add(map[f.y, f.x]);
                    Vector2Int g = new Vector2Int(center.X - r, center.Y - dd);
                    if (g.y >= 0 && g.y < map.GetLength(0) && g.x >= 0 && g.x < map.GetLength(1) && map[g.y, g.x] != null && (includeWalls || map[g.y, g.x].IsWalkable) && !list.Contains(map[g.y, g.x])) list.Add(map[g.y, g.x]);
                    Vector2Int h = new Vector2Int(center.X - r, center.Y + dd);
                    if (h.y >= 0 && h.y < map.GetLength(0) && h.x >= 0 && h.x < map.GetLength(1) && map[h.y, h.x] != null && (includeWalls || map[h.y, h.x].IsWalkable) && !list.Contains(map[h.y, h.x])) list.Add(map[h.y, h.x]);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get all tiles contained into a rectangle around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of any objects</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInARectangle<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeCenter = true) where T : ITile
        {
            return GetTilesInARectangle(map, center, rectangleSize.x, rectangleSize.y, includeCenter);
        }
        /// <summary>
        /// Get all tiles contained into a rectangle around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of any objects</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSizeX">The rectangle horizontal size</param>
        /// <param name="rectangleSizeY">The rectangle vertical size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInARectangle<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, bool includeCenter = true) where T : ITile
        {
            return ExtractRectangle(map, center, rectangleSizeX, rectangleSizeY, includeCenter, true);
        }
        /// <summary>
        /// Get all walkable tiles contained into a rectangle around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesInARectangle<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeCenter = true) where T : ITile
        {
            return GetWalkableTilesInARectangle(map, center, rectangleSize.x, rectangleSize.y, includeCenter);
        }
        /// <summary>
        /// Get all walkable tiles contained into a rectangle around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSizeX">The rectangle horizontal size</param>
        /// <param name="rectangleSizeY">The rectangle vertical size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesInARectangle<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, bool includeCenter = true) where T : ITile
        {
            return ExtractRectangle(map, center, rectangleSizeX, rectangleSizeY, includeCenter, false);
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
            return ExtractRectangleOutline(map, center, rectangleSizeX, rectangleSizeY, true);
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
            return ExtractRectangleOutline(map, center, rectangleSizeX, rectangleSizeY, false);
        }
        /// <summary>
        /// Get all tiles contained into a radius around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInARadius<T>(T[,] map, T center, int radius, bool includeCenter = true) where T : ITile
        {
            return ExtractRadius(map, center, radius, includeCenter, true);
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
            return ExtractRadiusOutline(map, center, radius, true);
        }
        /// <summary>
        /// Get all walkable tiles contained into a radius around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesInARadius<T>(T[,] map, T center, int radius, bool includeCenter = true) where T : ITile
        {
            return ExtractRadius(map, center, radius, includeCenter, false);
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
            return ExtractRadiusOutline(map, center, radius, false);
        }
        /// <summary>
        /// Is this tile contained into a rectangle or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="rectangleSize">The rectangle size</param>
        /// <returns>A boolean that is true if the tile is contained into the rectangle, false otherwise</returns>
        public static bool IsTileInARectangle<T>(T center, T tile, Vector2Int rectangleSize) where T : ITile
        {
            return IsTileInARectangle(center, tile, rectangleSize.x, rectangleSize.y);
        }
        /// <summary>
        /// Is this tile contained into a rectangle or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="rectangleSizeX">The rectangle horizontal size</param>
        /// <param name="rectangleSizeY">The rectangle vertical size</param>
        /// <returns>A boolean that is true if the tile is contained into the rectangle, false otherwise</returns>
        public static bool IsTileInARectangle<T>(T center, T tile, int rectangleSizeX, int rectangleSizeY) where T : ITile
        {
            int bottom = center.Y - rectangleSizeY;
            int top = center.Y + rectangleSizeY;
            int left = center.X - rectangleSizeX;
            int right = center.X + rectangleSizeX;
            return tile.X >= left && tile.X <= right && tile.Y >= bottom && tile.Y <= top;
        }
        /// <summary>
        /// Is this tile is on a rectangle outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="rectangleSize">The rectangle size</param>
        /// <returns>A boolean that is true if the tile is on the rectangle outline, false otherwise</returns>
        public static bool IsTileOnARectangleOutline<T>(T center, T tile, Vector2Int rectangleSize) where T : ITile
        {
            return IsTileOnARectangleOutline(center, tile, rectangleSize.x, rectangleSize.y);
        }
        /// <summary>
        /// Is this tile is on a rectangle outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="rectangleSizeX">The rectangle horizontal size</param>
        /// <param name="rectangleSizeY">The rectangle vertical size</param>
        /// <returns>A boolean that is true if the tile is on the rectangle outline, false otherwise</returns>
        public static bool IsTileOnARectangleOutline<T>(T center, T tile, int rectangleSizeX, int rectangleSizeY) where T : ITile
        {
            int bottom = center.Y - rectangleSizeY,
                top = center.Y + rectangleSizeY,
                left = center.X - rectangleSizeX,
                right = center.X + rectangleSizeX;
            return (tile.X == left && tile.Y <= top && tile.Y >= bottom) || (tile.X == right && tile.Y <= top && tile.Y >= bottom) || (tile.Y == bottom && tile.X <= right && tile.X >= left) || (tile.Y == top && tile.X <= right && tile.X >= left);
        }
        /// <summary>
        /// Is this tile contained into a radius or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="radius">The radius</param>
        /// <returns>A boolean that is true if the tile is contained into the radius, false otherwise</returns>
        public static bool IsTileInARadius<T>(T center, T tile, int radius) where T : ITile
        {
            int bottom = center.Y - radius;
            int top = center.Y + radius + 1;
            int left = center.X - radius;
            int right = center.X + radius + 1;
            return tile.X >= left && tile.X <= right && tile.Y >= bottom && tile.Y <= top && Vector2Int.Distance(new Vector2Int(center.X, center.Y), new Vector2Int(tile.X, tile.Y)) <= radius;
        }
        /// <summary>
        /// Is this tile on a radius outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="radius">The radius</param>
        /// <returns>A boolean that is true if the tile on a radius outline , false otherwise</returns>
        public static bool IsTileOnARadiusOutline<T>(T center, T tile, int radius) where T : ITile
        {
            int bottom = center.Y - radius;
            int top = center.Y + radius + 1;
            for (int y = bottom; y < top; y++)
            {
                for (int r = 0; r <= Mathf.FloorToInt(radius * Mathf.Sqrt(0.5f)); r++)
                {
                    int dd = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - r * r));
                    Vector2Int a = new Vector2Int(center.X - dd, center.Y + r);
                    if (a.y==tile.Y && a.x == tile.X) return true;
                    Vector2Int b = new Vector2Int(center.X + dd, center.Y + r);
                    if (b.y==tile.Y && b.x == tile.X) return true;
                    Vector2Int c = new Vector2Int(center.X - dd, center.Y - r);
                    if (c.y==tile.Y && c.x == tile.X) return true;
                    Vector2Int d = new Vector2Int(center.X + dd, center.Y - r);
                    if (d.y==tile.Y && d.x == tile.X) return true;
                    Vector2Int e = new Vector2Int(center.X + r, center.Y - dd);
                    if (e.y==tile.Y && e.x == tile.X) return true;
                    Vector2Int f = new Vector2Int(center.X + r, center.Y + dd);
                    if (f.y==tile.Y && f.x == tile.X) return true;
                    Vector2Int g = new Vector2Int(center.X - r, center.Y - dd);
                    if (g.y==tile.Y && g.x == tile.X) return true;
                    Vector2Int h = new Vector2Int(center.X - r, center.Y + dd);
                    if (h.y==tile.Y && h.x == tile.X) return true;
                }
            }
            return false;
        }
    }
    /// <summary>
    /// Allows you to raycast lines of tiles and lines of sights on a grid
    /// </summary>
    public class Raycasting
    {
        private static T[] Raycast<T>(T[,] map, T startTile, T destinationTile, float maxDistance, bool includeStart, bool includeDestination, bool breakOnWalls, bool includeWalls, out bool isLineClear) where T : ITile
        {
            Vector2Int p0 = new Vector2Int(startTile.X, startTile.Y);
            Vector2Int p1 = new Vector2Int(destinationTile.X, destinationTile.Y);
            int dx = p1.x - p0.x, dy = p1.y - p0.y;
            int nx = Mathf.Abs(dx), ny = Mathf.Abs(dy);
            int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

            Vector2Int p = new Vector2Int(p0.x, p0.y);
            List<T> points = new List<T> { map[p.y, p.x] };
            isLineClear = true;
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
                bool breakIt = false;
                breakIt = breakIt ? true : breakOnWalls && (map[p.y, p.x] == null || !map[p.y, p.x].IsWalkable);
                isLineClear = !breakIt;
                breakIt = breakIt ? true : (maxDistance > 0f && Vector2Int.Distance(new Vector2Int(p.x, p.y), new Vector2Int(startTile.X, startTile.Y)) > maxDistance);
                bool continueIt = false;
                continueIt = continueIt ? true : map[p.y, p.x] == null;
                continueIt = continueIt ? true : !includeWalls && !map[p.y, p.x].IsWalkable;
                continueIt = continueIt ? true : !includeStart && Equals(map[p.y, p.x], includeStart);
                continueIt = continueIt ? true : !includeDestination || Equals(map[p.y, p.x], includeDestination);
                if (breakIt)
                {
                    break;
                }
                if (continueIt)
                {
                    continue;
                }
                points.Add(map[p.y, p.x]);
            }
            return points.ToArray();
        }

        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, false, true, out bool isclear);
        }
        /// <summary>
        /// Get all walkable tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnALine<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, false, false, out bool isclear);
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile
        {
            Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out bool isclear);
            return isclear;
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out bool isclear);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, T destinationTile, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out isLineClear);
        }
    }
    /// <summary>
    /// Allows you to calculate paths on a grid
    /// </summary>
    public class Pathfinding
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

        /// <summary>
        /// Generates a PathMap object that will contain all the precalculated paths data for the entire grid
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="targetTile">The target tile for the paths calculation</param>
        /// <param name="allowDiagonals">Allow diagonals movements</param>
        /// <param name="diagonalWeightRatio">Diagonal movement weight</param>
        /// <returns>A PathMap object</returns>
        public static PathMap<T> GeneratePathMap<T>(T[,] map, T targetTile, float maxDistance = 0f, bool allowDiagonals = true, float diagonalWeightRatio = 1.5f) where T : ITile
        {
            if (!targetTile.IsWalkable)
            {
                throw new System.Exception("Do not try to generate a PathMap with an unwalkable tile as the target");
            }
            Node<T> targetNode = new Node<T>(targetTile);
            Dictionary<T, Node<T>> accessibleTilesDico = new Dictionary<T, Node<T>>() { { targetTile, targetNode } };
            List<T> accessibleTiles = new List<T>() { targetTile };
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
            return new PathMap<T>(accessibleTilesDico, accessibleTiles, targetTile, maxDistance);
        }
    }
}
