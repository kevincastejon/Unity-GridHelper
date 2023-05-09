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
    public enum EdgesDiagonalsPolicy
    {
        NONE,
        DIAGONAL_2FREE,
        DIAGONAL_1FREE,
        ALL_DIAGONALS,
    }
    /// <summary>
    /// Represents the vertice diagonals permissiveness
    /// </summary>
    public enum VerticesDiagonalsPolicy
    {
        NONE,
        DIAGONAL_6FREE,
        DIAGONAL_5FREE,
        DIAGONAL_4FREE,
        DIAGONAL_3FREE,
        DIAGONAL_2FREE,
        DIAGONAL_1FREE,
        ALL_DIAGONALS,
    }
    /// <summary>
    /// Represents the movements permissiveness
    /// </summary>
    [System.Flags]
    public enum MovementPolicy
    {
        FLY = 0,
        WALL_BELOW = 1,
        WALL_ASIDE = 2,
        WALL_ABOVE = 4,
    }
    [System.Serializable]
    public class Pathfinding3DPolicy
    {
        [SerializeField] private EdgesDiagonalsPolicy _horizontalEdgesDiagonalsPolicy = EdgesDiagonalsPolicy.DIAGONAL_2FREE;
        [SerializeField] private float _horizontalEdgesDiagonalsWeight = 1.4142135623730950488016887242097f;
        [SerializeField] private EdgesDiagonalsPolicy _verticalEdgesDiagonalsPolicy = EdgesDiagonalsPolicy.DIAGONAL_1FREE;
        [SerializeField] private float _verticalEdgesDiagonalsWeight = 1.7320508075688772935274463415059f;
        [SerializeField] private VerticesDiagonalsPolicy _verticesDiagonalsPolicy = VerticesDiagonalsPolicy.DIAGONAL_6FREE;
        [SerializeField] private float _verticesDiagonalsWeight = (Vector3Int.right + Vector3Int.up + Vector3Int.forward).magnitude;
        [SerializeField] private MovementPolicy _movementPolicy = MovementPolicy.WALL_BELOW;

        public Pathfinding3DPolicy(EdgesDiagonalsPolicy horizontalEdgesDiagonalsPolicy = EdgesDiagonalsPolicy.DIAGONAL_2FREE, float horizontalEdgesDiagonalsWeight = 1.4142135623730950488016887242097f, EdgesDiagonalsPolicy verticalEdgesDiagonalsPolicy = EdgesDiagonalsPolicy.DIAGONAL_1FREE, float verticalEdgesDiagonalsWeight = 1.4142135623730950488016887242097f, VerticesDiagonalsPolicy verticesDiagonalsPolicy = VerticesDiagonalsPolicy.DIAGONAL_6FREE, float verticesDiagonalsWeight = 1.7320508075688772935274463415059f, MovementPolicy movementPolicy = MovementPolicy.WALL_BELOW)
        {
            _horizontalEdgesDiagonalsPolicy = horizontalEdgesDiagonalsPolicy;
            _horizontalEdgesDiagonalsWeight = horizontalEdgesDiagonalsWeight;
            _verticalEdgesDiagonalsPolicy = verticalEdgesDiagonalsPolicy;
            _verticalEdgesDiagonalsWeight = verticalEdgesDiagonalsWeight;
            _verticesDiagonalsPolicy = verticesDiagonalsPolicy;
            _verticesDiagonalsWeight = verticesDiagonalsWeight;
            _movementPolicy = movementPolicy;
        }

        public EdgesDiagonalsPolicy HorizontalEdgesDiagonalsPolicy { get => _horizontalEdgesDiagonalsPolicy; }
        public float HorizontalEdgesDiagonalsWeight { get => _horizontalEdgesDiagonalsWeight; }
        public EdgesDiagonalsPolicy VerticalEdgesDiagonalsPolicy { get => _verticalEdgesDiagonalsPolicy; }
        public float VerticalEdgesDiagonalsWeight { get => _verticalEdgesDiagonalsWeight; }
        public VerticesDiagonalsPolicy VerticesDiagonalsPolicy { get => _verticesDiagonalsPolicy; }
        public float VerticesDiagonalsWeight { get => _verticesDiagonalsWeight; }
        public MovementPolicy MovementPolicy { get => _movementPolicy; }
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
            Node3D<T> node = includeStart ? _dico[startTile] : _dico[startTile].NextNode;
            if (!includeTarget && EqualityComparer<T>.Default.Equals(node.Tile, _target))
            {
                return new T[0];
            }
            List<T> tiles = new List<T>() { node.Tile };
            while (!EqualityComparer<T>.Default.Equals(node.Tile, _target))
            {
                node = node.NextNode;
                if (includeTarget || !EqualityComparer<T>.Default.Equals(node.Tile, _target))
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
        private static bool GetLeftNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y, z, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y, z, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBottomNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x, y - 1, z, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetTopNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x, y + 1, z, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x, y, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x, y, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBottomNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y - 1, z, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftTopNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y + 1, z, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBottomNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y - 1, z, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightTopNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y + 1, z, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBottomBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x, y - 1, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetTopBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x, y + 1, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBottomFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x, y - 1, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetTopFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x, y + 1, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBottomBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y - 1, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBottomBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y - 1, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBottomFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y - 1, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBottomFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y - 1, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftTopBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y + 1, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightTopBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y + 1, z - 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftTopFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x - 1, y + 1, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightTopFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall) where T : ITile3D
        {
            if (GetTile(map, x + 1, y + 1, z + 1, out nei))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool CheckVDP(VerticesDiagonalsPolicy policy, bool valueA, bool valueB, bool valueC, bool valueD, bool valueE, bool valueF)
        {
            int count = (valueA ? 1 : 0) + (valueB ? 1 : 0) + (valueC ? 1 : 0) + (valueD ? 1 : 0) + (valueE ? 1 : 0) + (valueF ? 1 : 0);
            switch (policy)
            {
                case VerticesDiagonalsPolicy.NONE:
                    return false;
                case VerticesDiagonalsPolicy.DIAGONAL_6FREE:
                    return count == 6;
                case VerticesDiagonalsPolicy.DIAGONAL_5FREE:
                    return count >= 5;
                case VerticesDiagonalsPolicy.DIAGONAL_4FREE:
                    return count >= 4;
                case VerticesDiagonalsPolicy.DIAGONAL_3FREE:
                    return count >= 3;
                case VerticesDiagonalsPolicy.DIAGONAL_2FREE:
                    return count >= 2;
                case VerticesDiagonalsPolicy.DIAGONAL_1FREE:
                    return count >= 1;
                case VerticesDiagonalsPolicy.ALL_DIAGONALS:
                    return true;
                default:
                    return false;
            }
        }
        private static bool CheckEDP(EdgesDiagonalsPolicy policy, bool valueA, bool valueB)
        {
            switch (policy)
            {
                case EdgesDiagonalsPolicy.NONE:
                    return false;
                case EdgesDiagonalsPolicy.DIAGONAL_2FREE:
                    return valueA && valueB;
                case EdgesDiagonalsPolicy.DIAGONAL_1FREE:
                    return valueA || valueB;
                case EdgesDiagonalsPolicy.ALL_DIAGONALS:
                    return true;
                default:
                    return false;
            }
        }
        private static bool CheckMP<T>(MovementPolicy policy, T[,,] map, T tile) where T : ITile3D
        {
            int polCase = (int)policy;
            if (polCase == 0)
            {
                return true;
            }
            else if (polCase == 1)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true);
            }
            else if (polCase == 2)
            {
                return GetLeftNeighbour(map, tile.X, tile.Y, tile.Z, out T neiL, true) || GetRightNeighbour(map, tile.X, tile.Y, tile.Z, out T neiR, true) || GetBackNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true) || GetFrontNeighbour(map, tile.X, tile.Y, tile.Z, out T neiF, true);
            }
            else if (polCase == 3)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true) || GetLeftNeighbour(map, tile.X, tile.Y, tile.Z, out T neiL, true) || GetRightNeighbour(map, tile.X, tile.Y, tile.Z, out T neiR, true) || GetBackNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true) || GetFrontNeighbour(map, tile.X, tile.Y, tile.Z, out T neiF, true);
            }
            else if (polCase == 4)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true);
            }
            else if (polCase == 5)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true) || GetBottomNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true);
            }
            else if (polCase == 6)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true) || GetLeftNeighbour(map, tile.X, tile.Y, tile.Z, out T neiL, true) || GetRightNeighbour(map, tile.X, tile.Y, tile.Z, out T neiR, true) || GetBackNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true) || GetFrontNeighbour(map, tile.X, tile.Y, tile.Z, out T neiF, true);
            }
            else if (polCase == 7)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true) || GetTopNeighbour(map, tile.X, tile.Y, tile.Z, out T neiT, true) || GetLeftNeighbour(map, tile.X, tile.Y, tile.Z, out T neiL, true) || GetRightNeighbour(map, tile.X, tile.Y, tile.Z, out T neiR, true) || GetBackNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true) || GetFrontNeighbour(map, tile.X, tile.Y, tile.Z, out T neiF, true);
            }
            return true;
        }
        private static List<T> GetTileNeighbours<T>(T[,,] map, int x, int y, int z, Pathfinding3DPolicy pathfindingPolicy) where T : ITile3D
        {
            List<T> nodes = new List<T>();
            T nei;
            // 6
            bool leftWalkable = GetLeftNeighbour(map, x, y, z, out nei, false);
            if (leftWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            bool rightWalkable = GetRightNeighbour(map, x, y, z, out nei, false);
            if (rightWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            bool bottomWalkable = GetBottomNeighbour(map, x, y, z, out nei, false);
            if (bottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            bool topWalkable = GetTopNeighbour(map, x, y, z, out nei, false);
            if (topWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            bool backWalkable = GetBackNeighbour(map, x, y, z, out nei, false);
            if (backWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            bool frontWalkable = GetFrontNeighbour(map, x, y, z, out nei, false);
            if (frontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            //18
            bool leftBackWalkable = GetLeftBackNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, leftWalkable, backWalkable) && leftBackWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                leftBackWalkable = true;
            }
            bool rightBackWalkable = GetRightBackNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, rightWalkable, backWalkable) && rightBackWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                rightBackWalkable = true;
            }
            bool leftFrontWalkable = GetLeftFrontNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, leftWalkable, frontWalkable) && leftFrontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                leftFrontWalkable = true;
            }
            bool rightFrontWalkable = GetRightFrontNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, rightWalkable, frontWalkable) && rightFrontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                rightFrontWalkable = true;
            }
            bool leftBottomWalkable = GetLeftBottomNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, leftWalkable, bottomWalkable) && leftBottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                leftBottomWalkable = true;
            }
            bool rightBottomWalkable = GetRightBottomNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, rightWalkable, bottomWalkable) && rightBottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                rightBottomWalkable = true;
            }
            bool leftTopWalkable = GetLeftTopNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, leftWalkable, topWalkable) && leftTopWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                leftTopWalkable = true;
            }
            bool rightTopWalkable = GetRightTopNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, rightWalkable, topWalkable) && rightTopWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                rightTopWalkable = true;
            }
            bool bottomBackWalkable = GetBottomBackNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, bottomWalkable, backWalkable) && bottomBackWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                bottomBackWalkable = true;
            }
            bool topBackWalkable = GetTopBackNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, topWalkable, backWalkable) && topBackWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                topBackWalkable = true;
            }
            bool bottomFrontWalkable = GetBottomFrontNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, bottomWalkable, frontWalkable) && bottomFrontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                bottomFrontWalkable = true;
            }
            bool topFrontWalkable = GetTopFrontNeighbour(map, x, y, z, out nei, false);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, topWalkable, frontWalkable) && topFrontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
                topFrontWalkable = true;
            }
            // 26
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, bottomBackWalkable, backWalkable, leftBottomWalkable, leftWalkable, bottomWalkable, leftBackWalkable) && GetLeftBottomBackNeighbour(map, x, y, z, out nei, false) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, bottomBackWalkable, backWalkable, rightBottomWalkable, rightWalkable, bottomWalkable, rightBackWalkable) && GetRightBottomBackNeighbour(map, x, y, z, out nei, false) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, bottomFrontWalkable, frontWalkable, leftBottomWalkable, leftWalkable, bottomWalkable, leftFrontWalkable) && GetLeftBottomFrontNeighbour(map, x, y, z, out nei, false) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, bottomFrontWalkable, frontWalkable, rightBottomWalkable, rightWalkable, bottomWalkable, rightFrontWalkable) && GetRightBottomFrontNeighbour(map, x, y, z, out nei, false) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, topBackWalkable, backWalkable, leftTopWalkable, leftWalkable, topWalkable, leftBackWalkable) && GetLeftTopBackNeighbour(map, x, y, z, out nei, false) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, topBackWalkable, backWalkable, rightTopWalkable, rightWalkable, topWalkable, rightBackWalkable) && GetRightTopBackNeighbour(map, x, y, z, out nei, false) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, topFrontWalkable, frontWalkable, leftTopWalkable, leftWalkable, topWalkable, leftFrontWalkable) && GetLeftTopFrontNeighbour(map, x, y, z, out nei, false) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, topFrontWalkable, frontWalkable, rightTopWalkable, rightWalkable, topWalkable, rightFrontWalkable) && GetRightTopFrontNeighbour(map, x, y, z, out nei, false) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei))
            {
                nodes.Add(nei);
            }


            return nodes;
        }
        private static bool IsHorizontalEdgeDiagonal<T>(EdgesDiagonalsPolicy policy, Node3D<T> current, Node3D<T> next) where T : ITile3D
        {
            if (policy == EdgesDiagonalsPolicy.NONE)
            {
                return false;
            }
            return current.Tile.X != next.Tile.X && current.Tile.Y == next.Tile.Y && current.Tile.Z != next.Tile.Z;
        }
        private static bool IsVerticalEdgeDiagonal<T>(EdgesDiagonalsPolicy policy, Node3D<T> current, Node3D<T> next) where T : ITile3D
        {
            if (policy == EdgesDiagonalsPolicy.NONE)
            {
                return false;
            }
            return current.Tile.Y != next.Tile.Y && (current.Tile.Z != next.Tile.Z || current.Tile.X != next.Tile.X);
        }
        private static bool IsVerticeDiagonal<T>(VerticesDiagonalsPolicy policy, Node3D<T> current, Node3D<T> next) where T : ITile3D
        {
            if (policy == VerticesDiagonalsPolicy.NONE)
            {
                return false;
            }
            return current.Tile.X != next.Tile.X && current.Tile.Y != next.Tile.Y && current.Tile.Z != next.Tile.Z;
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
        public static PathMap3D<T> GeneratePathMap<T>(T[,,] map, T targetTile, float maxDistance = 0f, Pathfinding3DPolicy pathfindingPolicy = null) where T : ITile3D
        {
            if (!targetTile.IsWalkable)
            {
                throw new System.Exception("Do not try to generate a PathMap with an unwalkable tile as the target");
            }
            if (pathfindingPolicy == null)
            {
                pathfindingPolicy = new Pathfinding3DPolicy();
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
                List<T> neighbourgs = GetTileNeighbours(map, current.Tile.X, current.Tile.Y, current.Tile.Z, pathfindingPolicy);
                foreach (T neiTile in neighbourgs)
                {
                    Node3D<T> nei = accessibleTilesDico.ContainsKey(neiTile) ? accessibleTilesDico[neiTile] : new Node3D<T>(neiTile);
                    bool isHorizontalEdgeDiagonal = IsHorizontalEdgeDiagonal(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, current, nei);
                    bool isVerticalEdgeDiagonal = IsVerticalEdgeDiagonal(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, current, nei);
                    bool isVerticeDiagonal = IsVerticeDiagonal(pathfindingPolicy.VerticesDiagonalsPolicy, current, nei);
                    float newDistance = current.DistanceToTarget + (nei.Tile.Weight * (isVerticeDiagonal ? pathfindingPolicy.VerticesDiagonalsWeight : (isHorizontalEdgeDiagonal ? pathfindingPolicy.HorizontalEdgesDiagonalsWeight : (isVerticalEdgeDiagonal ? pathfindingPolicy.VerticalEdgesDiagonalsWeight : 1f))));
                   
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
                        nei.NextDirection = new Vector3Int(nei.NextNode.Tile.X > nei.Tile.X ? 1 : (nei.NextNode.Tile.X < nei.Tile.X ? -1 : 0), nei.NextNode.Tile.Y > nei.Tile.Y ? 1 : (nei.NextNode.Tile.Y < nei.Tile.Y ? -1 : 0), nei.NextNode.Tile.Z > nei.Tile.Z ? 1 : (nei.NextNode.Tile.Z < nei.Tile.Z ? -1 : 0));
                        nei.DistanceToTarget = newDistance;
                    }
                }
            }
            return new PathMap3D<T>(accessibleTilesDico, accessibleTiles, targetTile, maxDistance);
        }

    }
}
