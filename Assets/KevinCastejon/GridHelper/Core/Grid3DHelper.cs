using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Helper static classes for 2D grid operations
/// </summary>
namespace KevinCastejon.GridHelper3D
{
    /// <summary>
    /// Represents the diagonals permissiveness
    /// </summary>
    public enum DiagonalsPolicy
    {
        NONE,
        DIAGONAL_1FREE,
        DIAGONAL_2FREE,
        ALL_DIAGONALS,
    }
    /// <summary>
    /// Represents the vertice diagonals permissiveness
    /// </summary>
    public enum VerticeDiagonalsPolicy
    {
        NONE,
        DIAGONAL_1FREE,
        DIAGONAL_2FREE,
        DIAGONAL_3FREE,
        DIAGONAL_4FREE,
        DIAGONAL_5FREE,
        DIAGONAL_6FREE,
        ALL_DIAGONALS,
    }
    /// <summary>
    /// Represents the vertical movements permissiveness
    /// </summary>
    [System.Serializable]
    public struct VerticalMovementPolicy
    {
        [SerializeField] private bool _canFly;
        [SerializeField] private bool _canClimb;
        [SerializeField] private int _climbLimit;
    }
    /// <summary>
    /// An interface that the user-defined tile object has to implement in order to work with most of this library's methods
    /// </summary>
    public interface ITile3D
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
        /// <summary>
        /// The tile depth coordinate
        /// </summary>
        public int Z
        {
            get;
        }
    }
    internal class Node3D<T> where T : ITile3D
    {
        internal Node3D(T tile)
        {
            _tile = tile;
            IsWalkable = tile != null && tile.IsWalkable;
            Weight = tile == null ? 1f : Mathf.Max(tile.Weight, 1f);
        }

        private T _tile;
        private Node3D<T> _next;
        private Vector3Int _nextDirection;
        private float _distanceToTarget;

        internal T Tile { get => _tile; set => _tile = value; }
        internal Node3D<T> NextNode { get => _next; set => _next = value; }
        internal Vector3Int NextDirection { get => _nextDirection; set => _nextDirection = value; }
        internal float DistanceToTarget { get => _distanceToTarget; set => _distanceToTarget = value; }
        internal bool IsWalkable { get; set; }
        internal float Weight { get; set; }
    }
    /// <summary>
    /// An object containing all the calculated paths data of a tile grid
    /// </summary>
    /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
    public class PathMap3D<T> where T : ITile3D
    {
        private readonly Dictionary<T, Node3D<T>> _dico;
        private readonly List<T> _accessibleTiles;
        private readonly T _target;
        private readonly float _maxDistance;

        internal PathMap3D(Dictionary<T, Node3D<T>> accessibleTilesDico, List<T> accessibleTiles, T target, float maxDistance)
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
        /// <returns>A Vector3Int direction</returns>
        public Vector3Int GetNextTileDirectionFromTile(T tile)
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
            Node3D<T> node = _dico[startTile];
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
    public class Extraction3D
    {
        private static T[] ExtractCuboid<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, bool includeCenter, bool includeWalls) where T : ITile3D
        {
            int bottom = Mathf.Max(center.Y - cuboidSizeY, 0),
                top = Mathf.Min(center.Y + cuboidSizeY + 1, map.GetLength(0)),
                left = Mathf.Max(center.X - cuboidSizeX, 0),
                right = Mathf.Min(center.X + cuboidSizeX + 1, map.GetLength(1)),
                back = Mathf.Max(center.Z - cuboidSizeZ, 0),
                front = Mathf.Min(center.Z + cuboidSizeZ + 1, map.GetLength(2));
            List<T> list = new List<T>();
            for (int i = bottom; i < top; i++)
            {
                for (int j = left; j < right; j++)
                {
                    for (int k = back; k < front; k++)
                    {
                        if (map[i, j, k] != null && (includeWalls || map[i, j, k].IsWalkable) && (includeCenter || !EqualityComparer<T>.Default.Equals(map[i, j, k], center)))
                        {
                            list.Add(map[i, j, k]);
                        }
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractCuboidOutline<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, bool includeWalls) where T : ITile3D
        {
            int bottom = center.Y - cuboidSizeY,
                top = center.Y + cuboidSizeY + 1,
                left = center.X - cuboidSizeX,
                right = center.X + cuboidSizeX + 1,
                back = center.Z - cuboidSizeZ,
                front = center.Z + cuboidSizeZ + 1;
            List<T> list = new List<T>();
            for (int i = bottom; i < top; i++)
            {
                for (int j = left; j < right; j++)
                {
                    for (int k = back; k < front; k++)
                    {
                        if (i < 0 || i >= map.GetLength(0) || j < 0 || j >= map.GetLength(1) || k < 0 || k >= map.GetLength(2))
                        {
                            continue;
                        }
                        if (map[i, j, k] != null && (includeWalls || map[i, j, k].IsWalkable) && (i == top - 1 || i == bottom || j == left || j == right - 1 || k == back || k == front - 1))
                        {
                            list.Add(map[i, j, k]);
                        }
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractSphere<T>(T[,,] map, T center, int radius, bool includeCenter, bool includeWalls) where T : ITile3D
        {
            int bottom = Mathf.Max(center.Y - radius, 0),
                top = Mathf.Min(center.Y + radius + 1, map.GetLength(0)),
                left = Mathf.Max(center.X - radius, 0),
                right = Mathf.Min(center.X + radius + 1, map.GetLength(1)),
                back = Mathf.Max(center.Z - radius, 0),
                front = Mathf.Min(center.Z + radius + 1, map.GetLength(2));

            List<T> list = new List<T>();
            for (int y = bottom; y < top; y++)
            {
                for (int x = left; x < right; x++)
                {
                    for (int z = back; z < front; z++)
                    {
                        if (map[y, x, z] != null && Vector3Int.Distance(new Vector3Int(center.X, center.Y, center.Z), new Vector3Int(map[y, x, z].X, map[y, x, z].Y, map[y, x, z].Z)) <= radius && (includeWalls || map[y, x, z].IsWalkable) && (includeCenter || !EqualityComparer<T>.Default.Equals(map[y, x, z], center)))
                        {
                            list.Add(map[y, x, z]);
                        }
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractSphereOutline<T>(T[,,] map, T center, int radius, bool includeWalls) where T : ITile3D
        {
            int bottom = Mathf.Max(center.Y - radius, 0),
      top = Mathf.Min(center.Y + radius + 1, map.GetLength(0));
            List<T> list = new List<T>();
            for (int y = bottom; y < top; y++)
            {
                for (int x = center.X - radius; x <= center.X + radius; x++)
                {
                    for (int z = center.Z - radius; z <= center.Z + radius; z++)
                    {
                        if (x < 0 || x >= map.GetLength(1) ||
                            y < 0 || y >= map.GetLength(0) ||
                            z < 0 || z >= map.GetLength(2))
                        {
                            continue;
                        }

                        T tile = map[y, x, z];
                        if (tile == null || (!includeWalls && !tile.IsWalkable))
                        {
                            continue;
                        }

                        if (IsOnSphereOutline(center.X, center.Y, center.Z, radius, x, y, z))
                        {
                            list.Add(tile);
                        }
                    }
                }
            }
            return list.ToArray();
        }
        private static bool IsOnSphereOutline(int centerX, int centerY, int centerZ, int radius, int x, int y, int z)
        {
            int dx = x - centerX;
            int dy = y - centerY;
            int dz = z - centerZ;

            int distanceSquared = dx * dx + dy * dy + dz * dz;

            return distanceSquared >= (radius - 1) * (radius - 1) && distanceSquared <= radius * radius;
        }
        /// <summary>
        /// Get all tiles contained into a cuboid around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of any objects</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSize">The Vector3Int representing cuboid size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACuboid<T>(T[,,] map, T center, Vector3Int cuboidSize, bool includeCenter = true) where T : ITile3D
        {
            return GetTilesInACuboid(map, center, cuboidSize.x, cuboidSize.y, cuboidSize.z, includeCenter);
        }
        /// <summary>
        /// Get all tiles contained into a cuboid around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of any objects</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSizeX">The cuboid horizontal size</param>
        /// <param name="cuboidSizeY">The cuboid vertical size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACuboid<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, bool includeCenter = true) where T : ITile3D
        {
            return ExtractCuboid(map, center, cuboidSizeX, cuboidSizeY, cuboidSizeZ, includeCenter, true);
        }
        /// <summary>
        /// Get all walkable tiles contained into a cuboid around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSize">The Vector3Int representing cuboid size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesInACuboid<T>(T[,,] map, T center, Vector3Int cuboidSize, bool includeCenter = true) where T : ITile3D
        {
            return GetWalkableTilesInACuboid(map, center, cuboidSize.x, cuboidSize.y, cuboidSize.z, includeCenter);
        }
        /// <summary>
        /// Get all walkable tiles contained into a cuboid around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSizeX">The cuboid horizontal size</param>
        /// <param name="cuboidSizeY">The cuboid vertical size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesInACuboid<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, bool includeCenter = true) where T : ITile3D
        {
            return ExtractCuboid(map, center, cuboidSizeX, cuboidSizeY, cuboidSizeZ, includeCenter, false);
        }
        /// <summary>
        /// Get all tiles on a cuboid outline around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of any objects</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSize">The Vector3Int representing cuboid size</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnACuboidOutline<T>(T[,,] map, T center, Vector3Int cuboidSize) where T : ITile3D
        {
            return GetTilesOnACuboidOutline(map, center, cuboidSize.x, cuboidSize.y, cuboidSize.z);
        }
        /// <summary>
        /// Get all tiles on a cuboid outline around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of any objects</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSizeX">The cuboid horizontal size</param>
        /// <param name="cuboidSizeY">The cuboid vertical size</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnACuboidOutline<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ) where T : ITile3D
        {
            return ExtractCuboidOutline(map, center, cuboidSizeX, cuboidSizeY, cuboidSizeZ, true);
        }
        /// <summary>
        /// Get all walkable tiles on a cuboid outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSize">The Vector3Int representing cuboid size</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnACuboidOutline<T>(T[,,] map, T center, Vector3Int cuboidSize) where T : ITile3D
        {
            return GetWalkableTilesOnACuboidOutline(map, center, cuboidSize.x, cuboidSize.y, cuboidSize.z);
        }
        /// <summary>
        /// Get all walkable tiles on a cuboid outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSizeX">The cuboid horizontal size</param>
        /// <param name="cuboidSizeY">The cuboid vertical size</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnACuboidOutline<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ) where T : ITile3D
        {
            return ExtractCuboidOutline(map, center, cuboidSizeX, cuboidSizeY, cuboidSizeZ, false);
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
        public static T[] GetTilesInASphere<T>(T[,,] map, T center, int radius, bool includeCenter = true) where T : ITile3D
        {
            return ExtractSphere(map, center, radius, includeCenter, true);
        }
        /// <summary>
        /// Get all tiles on a radius outline around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile </param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnASphereOutline<T>(T[,,] map, T center, int radius) where T : ITile3D
        {
            return ExtractSphereOutline(map, center, radius, true);
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
        public static T[] GetWalkableTilesInASphere<T>(T[,,] map, T center, int radius, bool includeCenter = true) where T : ITile3D
        {
            return ExtractSphere(map, center, radius, includeCenter, false);
        }
        /// <summary>
        /// Get all walkable tiles on a radius outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnASphereOutline<T>(T[,,] map, T center, int radius) where T : ITile3D
        {
            return ExtractSphereOutline(map, center, radius, false);
        }
        /// <summary>
        /// Is this tile contained into a cuboid or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="cuboidSize">The cuboid size</param>
        /// <returns>A boolean that is true if the tile is contained into the cuboid, false otherwise</returns>
        public static bool IsTileInACuboid<T>(T center, T tile, Vector3Int cuboidSize) where T : ITile3D
        {
            return IsTileInACuboid(center, tile, cuboidSize.x, cuboidSize.y, cuboidSize.z);
        }
        /// <summary>
        /// Is this tile contained into a cuboid or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="cuboidSizeX">The cuboid horizontal size</param>
        /// <param name="cuboidSizeY">The cuboid vertical size</param>
        /// <returns>A boolean that is true if the tile is contained into the cuboid, false otherwise</returns>
        public static bool IsTileInACuboid<T>(T center, T tile, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ) where T : ITile3D
        {
            int bottom = center.Y - cuboidSizeY;
            int top = center.Y + cuboidSizeY;
            int left = center.X - cuboidSizeX;
            int right = center.X + cuboidSizeX;
            int back = center.Z - cuboidSizeZ;
            int front = center.Z + cuboidSizeZ;
            return tile.X >= left && tile.X <= right && tile.Y >= bottom && tile.Y <= top && tile.Z >= back && tile.Z <= front;
        }
        /// <summary>
        /// Is this tile is on a cuboid outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="cuboidSize">The cuboid size</param>
        /// <returns>A boolean that is true if the tile is on the cuboid outline, false otherwise</returns>
        public static bool IsTileOnACuboidOutline<T>(T center, T tile, Vector3Int cuboidSize) where T : ITile3D
        {
            return IsTileOnACuboidOutline(center, tile, cuboidSize.x, cuboidSize.y, cuboidSize.z);
        }
        /// <summary>
        /// Is this tile is on a cuboid outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="cuboidSizeX">The cuboid horizontal size</param>
        /// <param name="cuboidSizeY">The cuboid vertical size</param>
        /// <returns>A boolean that is true if the tile is on the cuboid outline, false otherwise</returns>
        public static bool IsTileOnACuboidOutline<T>(T center, T tile, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ) where T : ITile3D
        {
            int bottom = center.Y - cuboidSizeY,
                top = center.Y + cuboidSizeY,
                left = center.X - cuboidSizeX,
                right = center.X + cuboidSizeX,
                back = center.Z - cuboidSizeZ,
                front = center.Z + cuboidSizeZ;
            bool xOk = (tile.X == left && tile.Y <= top && tile.Y >= bottom && tile.Z <= front && tile.Z >= back) || (tile.X == right && tile.Y <= top && tile.Y >= bottom && tile.Z <= front && tile.Z >= back);
            bool yOk = (tile.Y == bottom && tile.X <= right && tile.X >= left && tile.Z <= front && tile.Z >= back) || (tile.Y == top && tile.X <= right && tile.X >= left && tile.Z <= front && tile.Z >= back);
            bool zOk = (tile.Z == back && tile.X <= right && tile.X >= left && tile.Y <= top && tile.Y >= bottom) || (tile.Z == front && tile.X <= right && tile.X >= left && tile.Y <= top && tile.Y >= bottom);
            return xOk || yOk || zOk;
        }
        /// <summary>
        /// Is this tile contained into a radius or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="radius">The radius</param>
        /// <returns>A boolean that is true if the tile is contained into the radius, false otherwise</returns>
        public static bool IsTileInASphere<T>(T center, T tile, int radius) where T : ITile3D
        {
            int bottom = center.Y - radius;
            int top = center.Y + radius + 1;
            int left = center.X - radius;
            int right = center.X + radius + 1;
            int back = center.Z - radius;
            int front = center.Z + radius + 1;
            return tile.X >= left && tile.X <= right && tile.Y >= bottom && tile.Y <= top && tile.Z >= back && tile.Z <= front && Vector3Int.Distance(new Vector3Int(center.X, center.Y, center.Z), new Vector3Int(tile.X, tile.Y, tile.Z)) <= radius;
        }
        /// <summary>
        /// Is this tile on a radius outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="radius">The radius</param>
        /// <returns>A boolean that is true if the tile on a radius outline , false otherwise</returns>
        public static bool IsTileOnASphereOutline<T>(T center, T tile, int radius) where T : ITile3D
        {
            throw new System.NotImplementedException("This method is not available yet");
            //int bottom = center.Y - radius;
            //int top = center.Y + radius + 1;
            //for (int y = bottom; y < top; y++)
            //{
            //    for (int r = 0; r <= Mathf.FloorToInt(radius * Mathf.Sqrt(0.5f)); r++)
            //    {
            //        int dd = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - r * r));
            //        Vector3Int a = new Vector3Int(center.X - dd, center.Y + r);
            //        if (a.y == tile.Y && a.x == tile.X) return true;
            //        Vector3Int b = new Vector3Int(center.X + dd, center.Y + r);
            //        if (b.y == tile.Y && b.x == tile.X) return true;
            //        Vector3Int c = new Vector3Int(center.X - dd, center.Y - r);
            //        if (c.y == tile.Y && c.x == tile.X) return true;
            //        Vector3Int d = new Vector3Int(center.X + dd, center.Y - r);
            //        if (d.y == tile.Y && d.x == tile.X) return true;
            //        Vector3Int e = new Vector3Int(center.X + r, center.Y - dd);
            //        if (e.y == tile.Y && e.x == tile.X) return true;
            //        Vector3Int f = new Vector3Int(center.X + r, center.Y + dd);
            //        if (f.y == tile.Y && f.x == tile.X) return true;
            //        Vector3Int g = new Vector3Int(center.X - r, center.Y - dd);
            //        if (g.y == tile.Y && g.x == tile.X) return true;
            //        Vector3Int h = new Vector3Int(center.X - r, center.Y + dd);
            //        if (h.y == tile.Y && h.x == tile.X) return true;
            //    }
            //}
            //return false;
        }
    }
    /// <summary>
    /// Allows you to raycast lines of tiles and lines of sights on a grid
    /// </summary>
    public class Raycasting3D
    {
        private static T[] Raycast<T>(T[,,] map, T startTile, T destinationTile, float maxDistance, bool includeStart, bool includeDestination, bool breakOnWalls, bool includeWalls, out bool isLineClear) where T : ITile3D
        {
            Vector3Int p0 = new Vector3Int(startTile.X, startTile.Y, startTile.Z);
            Vector3Int p1 = new Vector3Int(destinationTile.X, destinationTile.Y, destinationTile.Z);
            int dx = p1.x - p0.x, dy = p1.y - p0.y, dz = p1.z - p0.z;
            int nx = Mathf.Abs(dx), ny = Mathf.Abs(dy), nz = Mathf.Abs(dz);
            int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1, sign_z = dz > 0 ? 1 : -1;

            Vector3Int p = new Vector3Int(p0.x, p0.y, p0.z);
            List<T> points = new List<T>();
            if (includeStart)
            {
                points.Add(map[p.y, p.x, p.z]);
            }
            isLineClear = true;
            for (int ix = 0, iy = 0, iz = 0; ix < nx || iy < ny || iz < nz;)
            {
                if ((0.5 + ix) / nx < (0.5 + iy) / ny && (0.5 + ix) / nx < (0.5 + iz) / nz)
                {
                    // next step is horizontal
                    p.x += sign_x;
                    ix++;
                }
                else if ((0.5 + iy) / ny < (0.5 + iz) / nz)
                {
                    // next step is vertical on Y
                    p.y += sign_y;
                    iy++;
                }
                else
                {
                    // next step is vertical on Z
                    p.z += sign_z;
                    iz++;
                }
                bool breakIt = false;
                breakIt = breakIt ? true : breakOnWalls && (map[p.y, p.x, p.z] == null || !map[p.y, p.x, p.z].IsWalkable);
                isLineClear = !breakIt;
                breakIt = breakIt ? true : (maxDistance > 0f && Vector3Int.Distance(new Vector3Int(p.x, p.y, p.z), new Vector3Int(startTile.X, startTile.Y, startTile.Z)) > maxDistance);
                bool continueIt = false;
                continueIt = continueIt ? true : map[p.y, p.x, p.z] == null;
                continueIt = continueIt ? true : !includeWalls && !map[p.y, p.x, p.z].IsWalkable;
                continueIt = continueIt ? true : !includeDestination && Equals(map[p.y, p.x, p.z], destinationTile);
                if (breakIt)
                {
                    break;
                }
                if (continueIt)
                {
                    continue;
                }
                points.Add(map[p.y, p.x, p.z]);
            }
            return points.ToArray();
            //List<T> line = new List<T>();

            //// Determine the direction of the line
            //int dx = destinationTile.X - startTile.X;
            //int dy = destinationTile.Y - startTile.Y;
            //int dz = destinationTile.Z - startTile.Z;

            //// Determine the number of steps along each axis
            //int steps = Mathf.Max(Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)), Mathf.Abs(dz));

            //// Calculate the increment along each axis for each step
            //float xInc = (float)dx / steps;
            //float yInc = (float)dy / steps;
            //float zInc = (float)dz / steps;

            //// Traverse the line and add each cell to the list
            //float x = startTile.X;
            //float y = startTile.Y;
            //float z = startTile.Z;

            //isLineClear = false;
            //for (int i = 0; i <= steps; i++)
            //{
            //    int xIndex = (int)Mathf.Round(x);
            //    int yIndex = (int)Mathf.Round(y);
            //    int zIndex = (int)Mathf.Round(z);

            //    x += xInc;
            //    y += yInc;
            //    z += zInc;

            //    //bool breakIt = false;
            //    //breakIt = breakIt ? true : breakOnWalls && (map[yIndex, xIndex, zIndex] == null || !map[yIndex, xIndex, zIndex].IsWalkable);
            //    //isLineClear = !breakIt;
            //    //breakIt = breakIt ? true : (maxDistance > 0f && Vector3Int.Distance(new Vector3Int(xIndex, yIndex, zIndex), new Vector3Int(startTile.X, startTile.Y, startTile.Z)) > maxDistance);
            //    //bool continueIt = false;
            //    //continueIt = continueIt ? true : map[yIndex, xIndex, zIndex] == null;
            //    //continueIt = continueIt ? true : !includeWalls && !map[yIndex, xIndex, zIndex].IsWalkable;
            //    //continueIt = continueIt ? true : !includeStart && Equals(map[yIndex, xIndex, zIndex], startTile);
            //    //continueIt = continueIt ? true : !includeDestination && Equals(map[yIndex, xIndex, zIndex], destinationTile);
            //    //if (breakIt)
            //    //{
            //    //    break;
            //    //}
            //    //if (continueIt)
            //    //{
            //    //    continue;
            //    //}
            //    line.Add(map[yIndex, xIndex, zIndex]);
            //    // Determine the next coordinate to visit
            //    T nextCell = map[
            //        (int)(y + yInc),
            //        (int)(x + xInc),
            //        (int)(z + zInc)
            //    ];

            //    // If the next coordinate is diagonal, adjust it to be orthogonally adjacent
            //    if (Mathf.Abs(nextCell.X - x) > 0 && Mathf.Abs(nextCell.Y - y) > 0 && Mathf.Abs(nextCell.Z - z) > 0)
            //    {
            //        if (Mathf.Abs(dx) >= Mathf.Max(Mathf.Abs(dy), Mathf.Abs(dz)))
            //        {
            //            nextCell.Y = y;
            //        }
            //        else if (Mathf.Abs(dy) >= Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz)))
            //        {
            //            nextCell.X = x;
            //        }
            //        else
            //        {
            //            nextCell.Z = z;
            //        }
            //    }

            //    // Update the current coordinate
            //    x = nextCell.X;
            //    y = nextCell.Y;
            //    z = nextCell.Z;
            //}
            //return line.ToArray();
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
        public static T[] GetTilesOnALine<T>(T[,,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile3D
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
        public static T[] GetWalkableTilesOnALine<T>(T[,,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile3D
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
        public static bool IsLineOfSightClear<T>(T[,,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile3D
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
        public static T[] GetLineOfSight<T>(T[,,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile3D
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
        public static T[] GetLineOfSight<T>(T[,,] map, T startTile, T destinationTile, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true) where T : ITile3D
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out isLineClear);
        }
    }
    /// <summary>
    /// Allows you to calculate paths on a grid
    /// </summary>
    public class Pathfinding3D
    {
        private static bool GetTile<T>(T[,,] map, int x, int y, int z, out T tile) where T : ITile3D
        {
            if (x > -1 && x < map.GetLength(1) && y > -1 && y < map.GetLength(0) && z > -1 && z < map.GetLength(2))
            {
                tile = map[y, x, z];
                return true;
            }
            tile = default;
            return false;
        }
        private static List<T> GetTileNeighbours<T>(T[,,] map, int x, int y, int z, bool allowDiagonals = true) where T : ITile3D
        {
            List<T> nodes = new List<T>();
            T nei;
            bool leftWalkable = false;
            bool rightWalkable = false;
            bool topWalkable = false;
            bool bottomWalkable = false;
            bool backWalkable = false;
            bool frontWalkable = false;
            if (GetTile(map, x - 1, y, z, out nei))
            {
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                    leftWalkable = true;
                }
            }
            if (GetTile(map, x, y - 1, z, out nei))
            {
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                    bottomWalkable = true;
                }
            }
            if (GetTile(map, x, y + 1, z, out nei))
            {
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                    topWalkable = true;
                }
            }
            if (GetTile(map, x + 1, y, z, out nei))
            {
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                    rightWalkable = true;
                }
            }
            if (GetTile(map, x, y, z - 1, out nei))
            {
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                    backWalkable = true;
                }
            }
            if (GetTile(map, x, y, z + 1, out nei))
            {
                if (nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                    frontWalkable = true;
                }
            }
            //18
            if (GetTile(map, x - 1, y - 1, z, out nei))
            {
                if (/*allowDiagonals && leftWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x - 1, y + 1, z, out nei))
            {
                if (/*allowDiagonals && leftWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y + 1, z, out nei))
            {
                if (/*allowDiagonals && rightWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y - 1, z, out nei))
            {
                if (/*allowDiagonals && rightWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }

            if (GetTile(map, x, y - 1, z+1, out nei))
            {
                if (/*allowDiagonals && leftWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x, y - 1, z-1, out nei))
            {
                if (/*allowDiagonals && leftWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x, y + 1, z+1, out nei))
            {
                if (/*allowDiagonals && rightWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x, y + 1, z-1, out nei))
            {
                if (/*allowDiagonals && rightWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }

            if (GetTile(map, x - 1, y, z + 1, out nei))
            {
                if (/*allowDiagonals && leftWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x - 1, y, z - 1, out nei))
            {
                if (/*allowDiagonals && leftWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y, z + 1, out nei))
            {
                if (/*allowDiagonals && rightWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y, z - 1, out nei))
            {
                if (/*allowDiagonals && rightWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }

            //26
            if (GetTile(map, x - 1, y-1, z + 1, out nei))
            {
                if (/*allowDiagonals && leftWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x - 1, y-1, z - 1, out nei))
            {
                if (/*allowDiagonals && leftWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x - 1, y+1, z + 1, out nei))
            {
                if (/*allowDiagonals && rightWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x - 1, y+1, z - 1, out nei))
            {
                if (/*allowDiagonals && rightWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }

            if (GetTile(map, x + 1, y - 1, z + 1, out nei))
            {
                if (/*allowDiagonals && leftWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y - 1, z - 1, out nei))
            {
                if (/*allowDiagonals && leftWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y + 1, z + 1, out nei))
            {
                if (/*allowDiagonals && rightWalkable && topWalkable && */nei != null && nei.IsWalkable)
                {
                    nodes.Add(nei);
                }
            }
            if (GetTile(map, x + 1, y + 1, z - 1, out nei))
            {
                if (/*allowDiagonals && rightWalkable && bottomWalkable && */nei != null && nei.IsWalkable)
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
        public static PathMap3D<T> GeneratePathMap<T>(T[,,] map, T targetTile, float maxDistance = 0f) where T : ITile3D
        {
            if (!targetTile.IsWalkable)
            {
                throw new System.Exception("Do not try to generate a PathMap with an unwalkable tile as the target");
            }
            Node3D<T> targetNode = new Node3D<T>(targetTile);
            Dictionary<T, Node3D<T>> accessibleTilesDico = new Dictionary<T, Node3D<T>>() { { targetTile, targetNode } };
            List<T> accessibleTiles = new List<T>() { targetTile };
            PriorityQueueUnityPort.PriorityQueue<Node3D<T>, float> frontier = new();
            frontier.Enqueue(targetNode, 0);
            targetNode.NextNode = targetNode;
            targetNode.DistanceToTarget = 0f;
            
            while (frontier.Count > 0)
            {
                Node3D<T> current = frontier.Dequeue();
                List<T> neighbourgs = GetTileNeighbours(map, current.Tile.X, current.Tile.Y, current.Tile.Z);
                foreach (T neiTile in neighbourgs)
                {
                    Node3D<T> nei = accessibleTilesDico.ContainsKey(neiTile) ? accessibleTilesDico[neiTile] : new Node3D<T>(neiTile);
                    //bool isDiagonal = allowDiagonals && current.Tile.X != nei.Tile.X && current.Tile.Y != nei.Tile.Y;
                    float newDistance = current.DistanceToTarget + nei.Tile.Weight;// * (isDiagonal ? diagonalWeightRatio : 1f);
                    if (maxDistance > 0f && newDistance > maxDistance)
                    {
                        Debug.Log("SKIP TILE CAUSE OF DISTANCE");
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
                        nei.NextDirection = new Vector3Int(nei.NextNode.Tile.X > nei.Tile.X ? 1 : (nei.NextNode.Tile.X < nei.Tile.X ? -1 : 0), nei.NextNode.Tile.Y > nei.Tile.Y ? 1 : (nei.NextNode.Tile.Y < nei.Tile.Y ? -1 : 0), nei.NextNode.Tile.Z > nei.Tile.Z ? 1 : (nei.NextNode.Tile.Z < nei.Tile.Z ? -1 : 0));
                        nei.DistanceToTarget = newDistance;
                    }
                }
            }
            return new PathMap3D<T>(accessibleTilesDico, accessibleTiles, targetTile, maxDistance);
        }
    }
}
