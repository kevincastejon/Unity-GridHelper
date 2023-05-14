using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Helper static classes for 3D grid operations
/// </summary>
namespace KevinCastejon.GridHelper3D
{
    /// <summary>
    /// Major order rule
    /// </summary>
    public enum MajorOrder
    {
        XYZ,
        XZY,
        YXZ,
        YZX,
        ZXY,
        ZYX,
    }
    /// <summary>
    /// Represents the edges diagonals permissiveness
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
    /// <summary>
    /// Set of parameters to use for the pathfinding
    /// </summary>
    [System.Serializable]
    public struct Pathfinding3DPolicy
    {
        [SerializeField] private EdgesDiagonalsPolicy _horizontalEdgesDiagonalsPolicy;
        [SerializeField] private float _horizontalEdgesDiagonalsWeight;
        [SerializeField] private EdgesDiagonalsPolicy _verticalEdgesDiagonalsPolicy;
        [SerializeField] private float _verticalEdgesDiagonalsWeight;
        [SerializeField] private VerticesDiagonalsPolicy _verticesDiagonalsPolicy;
        [SerializeField] private float _verticesDiagonalsWeight;
        [SerializeField] private MovementPolicy _movementPolicy;

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

        public EdgesDiagonalsPolicy HorizontalEdgesDiagonalsPolicy { get => _horizontalEdgesDiagonalsPolicy; set => _horizontalEdgesDiagonalsPolicy = value; }
        public float HorizontalEdgesDiagonalsWeight { get => _horizontalEdgesDiagonalsWeight; set => _horizontalEdgesDiagonalsWeight = value; }
        public EdgesDiagonalsPolicy VerticalEdgesDiagonalsPolicy { get => _verticalEdgesDiagonalsPolicy; set => _verticalEdgesDiagonalsPolicy = value; }
        public float VerticalEdgesDiagonalsWeight { get => _verticalEdgesDiagonalsWeight; set => _verticalEdgesDiagonalsWeight = value; }
        public VerticesDiagonalsPolicy VerticesDiagonalsPolicy { get => _verticesDiagonalsPolicy; set => _verticesDiagonalsPolicy = value; }
        public float VerticesDiagonalsWeight { get => _verticesDiagonalsWeight; set => _verticesDiagonalsWeight = value; }
        public MovementPolicy MovementPolicy { get => _movementPolicy; set => _movementPolicy = value; }
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
        private readonly Pathfinding3DPolicy _pathfindingPolicy;

        internal PathMap3D(Dictionary<T, Node3D<T>> accessibleTilesDico, List<T> accessibleTiles, T target, float maxDistance, Pathfinding3DPolicy pathfindingPolicy)
        {
            _dico = accessibleTilesDico;
            _accessibleTiles = accessibleTiles;
            _target = target;
            _maxDistance = maxDistance;
            _pathfindingPolicy = pathfindingPolicy;
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
        /// The Pathfinding3DPolicy parameter value that has been used to generate this PathMap
        /// </summary>
        public Pathfinding3DPolicy PathfindingPolicy { get => _pathfindingPolicy; }
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
        private static T[] ExtractCuboid<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, bool includeCenter, bool includeWalls, MajorOrder majorOrder) where T : ITile3D
        {
            int bottom = Mathf.Max(center.Y - cuboidSizeY, 0),
                top = Mathf.Min(center.Y + cuboidSizeY + 1, Utils.GetYLength(map, majorOrder)),
                left = Mathf.Max(center.X - cuboidSizeX, 0),
                right = Mathf.Min(center.X + cuboidSizeX + 1, Utils.GetXLength(map, majorOrder)),
                back = Mathf.Max(center.Z - cuboidSizeZ, 0),
                front = Mathf.Min(center.Z + cuboidSizeZ + 1, Utils.GetZLength(map, majorOrder));
            List<T> list = new List<T>();
            for (int i = bottom; i < top; i++)
            {
                for (int j = left; j < right; j++)
                {
                    for (int k = back; k < front; k++)
                    {
                        T tile = Utils.GetTile(map, j, i, k, majorOrder);
                        if (tile != null && (includeWalls || tile.IsWalkable) && (includeCenter || !EqualityComparer<T>.Default.Equals(tile, center)))
                        {
                            list.Add(tile);
                        }
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractCuboidOutline<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, bool includeWalls, MajorOrder majorOrder) where T : ITile3D
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
                        if (i < 0 || i >= Utils.GetYLength(map, majorOrder) || j < 0 || j >= Utils.GetXLength(map, majorOrder) || k < 0 || k >= Utils.GetZLength(map, majorOrder))
                        {
                            continue;
                        }
                        T tile = Utils.GetTile(map, j, i, k, majorOrder);
                        if (tile != null && (includeWalls || tile.IsWalkable) && (i == top - 1 || i == bottom || j == left || j == right - 1 || k == back || k == front - 1))
                        {
                            list.Add(tile);
                        }
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractSphere<T>(T[,,] map, T center, int radius, bool includeCenter, bool includeWalls, MajorOrder majorOrder) where T : ITile3D
        {
            int bottom = Mathf.Max(center.Y - radius, 0),
                top = Mathf.Min(center.Y + radius + 1, Utils.GetYLength(map, majorOrder)),
                left = Mathf.Max(center.X - radius, 0),
                right = Mathf.Min(center.X + radius + 1, Utils.GetXLength(map, majorOrder)),
                back = Mathf.Max(center.Z - radius, 0),
                front = Mathf.Min(center.Z + radius + 1, Utils.GetZLength(map, majorOrder));

            List<T> list = new List<T>();
            for (int y = bottom; y < top; y++)
            {
                for (int x = left; x < right; x++)
                {
                    for (int z = back; z < front; z++)
                    {
                        T tile = Utils.GetTile(map, x, y, z, majorOrder);
                        if (tile != null && Vector3Int.Distance(new Vector3Int(center.X, center.Y, center.Z), new Vector3Int(tile.X, tile.Y, tile.Z)) <= radius && (includeWalls || tile.IsWalkable) && (includeCenter || !EqualityComparer<T>.Default.Equals(tile, center)))
                        {
                            list.Add(tile);
                        }
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractSphereOutline<T>(T[,,] map, T center, int radius, bool includeWalls, MajorOrder majorOrder) where T : ITile3D
        {
            int bottom = Mathf.Max(center.Y - radius, 0);
            int top = Mathf.Min(center.Y + radius + 1, Utils.GetYLength(map, majorOrder));
            List<T> list = new List<T>();
            for (int y = bottom; y < top; y++)
            {
                for (int x = center.X - radius; x <= center.X + radius; x++)
                {
                    for (int z = center.Z - radius; z <= center.Z + radius; z++)
                    {
                        if (x < 0 || x >= Utils.GetXLength(map, majorOrder) ||
                            y < 0 || y >= top ||
                            z < 0 || z >= Utils.GetZLength(map, majorOrder))
                        {
                            continue;
                        }

                        T tile = map[y, x, z];
                        if (tile == null || (!includeWalls && !tile.IsWalkable))
                        {
                            continue;
                        }

                        if (!EqualityComparer<T>.Default.Equals(tile, center) && IsOnSphereOutline(center.X, center.Y, center.Z, radius, x, y, z))
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
        public static T[] GetTilesInACuboid<T>(T[,,] map, T center, Vector3Int cuboidSize, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
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
        public static T[] GetTilesInACuboid<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return ExtractCuboid(map, center, cuboidSizeX, cuboidSizeY, cuboidSizeZ, includeCenter, true, majorOrder);
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
        public static T[] GetWalkableTilesInACuboid<T>(T[,,] map, T center, Vector3Int cuboidSize, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return GetWalkableTilesInACuboid(map, center, cuboidSize.x, cuboidSize.y, cuboidSize.z, includeCenter, majorOrder);
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
        public static T[] GetWalkableTilesInACuboid<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return ExtractCuboid(map, center, cuboidSizeX, cuboidSizeY, cuboidSizeZ, includeCenter, false, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a cuboid outline around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of any objects</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSize">The Vector3Int representing cuboid size</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnACuboidOutline<T>(T[,,] map, T center, Vector3Int cuboidSize, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return GetTilesOnACuboidOutline(map, center, cuboidSize.x, cuboidSize.y, cuboidSize.z, majorOrder);
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
        public static T[] GetTilesOnACuboidOutline<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return ExtractCuboidOutline(map, center, cuboidSizeX, cuboidSizeY, cuboidSizeZ, true, majorOrder);
        }
        /// <summary>
        /// Get all walkable tiles on a cuboid outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="cuboidSize">The Vector3Int representing cuboid size</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnACuboidOutline<T>(T[,,] map, T center, Vector3Int cuboidSize, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return GetWalkableTilesOnACuboidOutline(map, center, cuboidSize.x, cuboidSize.y, cuboidSize.z, majorOrder);
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
        public static T[] GetWalkableTilesOnACuboidOutline<T>(T[,,] map, T center, int cuboidSizeX, int cuboidSizeY, int cuboidSizeZ, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return ExtractCuboidOutline(map, center, cuboidSizeX, cuboidSizeY, cuboidSizeZ, false, majorOrder);
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
        public static T[] GetTilesInASphere<T>(T[,,] map, T center, int radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return ExtractSphere(map, center, radius, includeCenter, true, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a radius outline around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile </param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnASphereOutline<T>(T[,,] map, T center, int radius, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return ExtractSphereOutline(map, center, radius, true, majorOrder);
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
        public static T[] GetWalkableTilesInASphere<T>(T[,,] map, T center, int radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return ExtractSphere(map, center, radius, includeCenter, false, majorOrder);
        }
        /// <summary>
        /// Get all walkable tiles on a radius outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnASphereOutline<T>(T[,,] map, T center, int radius, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return ExtractSphereOutline(map, center, radius, false, majorOrder);
        }
        /// <summary>
        /// Get neighbour of a tile if it exists
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="neighbourDirection">The direction from the tile to the desired neighbour</param>
        /// <param name="neighbour">The neighbour of a tile</param>
        /// <returns></returns>
        public static bool GetTileNeighbour<T>(T[,,] map, T tile, Vector3Int neighbourDirection, out T neighbour, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            int x = neighbourDirection.x > 0 ? tile.X + 1 : (neighbourDirection.x < 0 ? tile.X - 1 : tile.X);
            int y = neighbourDirection.y > 0 ? tile.Y + 1 : (neighbourDirection.y < 0 ? tile.Y - 1 : tile.Y);
            int z = neighbourDirection.z > 0 ? tile.Z + 1 : (neighbourDirection.z < 0 ? tile.Z - 1 : tile.Z);
            if (neighbourDirection.x < 0 && tile.X - 1 < 0 || neighbourDirection.x > 0 && tile.X + 1 >= Utils.GetXLength(map, majorOrder))
            {
                neighbour = default;
                return false;
            }
            if (neighbourDirection.y < 0 && tile.Y - 1 < 0 || neighbourDirection.y > 0 && tile.Y + 1 >= Utils.GetYLength(map, majorOrder))
            {
                neighbour = default;
                return false;
            }
            if (neighbourDirection.z < 0 && tile.Z - 1 < 0 || neighbourDirection.z > 0 && tile.Z + 1 >= Utils.GetZLength(map, majorOrder))
            {
                neighbour = default;
                return false;
            }
            neighbour = Utils.GetTile(map, x, y, z, majorOrder);
            return true;
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
            return IsOnSphereOutline(center.X, center.Y, center.Z, radius, tile.X, tile.Y, tile.Z);
        }
    }
    /// <summary>
    /// Allows you to raycast lines of tiles and lines of sights on a grid
    /// </summary>
    public class Raycasting3D
    {
        private static T[] Raycast<T>(T[,,] map, T startTile, T destinationTile, float maxDistance, bool includeStart, bool includeDestination, bool breakOnWalls, bool includeWalls, out bool isLineClear, MajorOrder majorOrder) where T : ITile3D
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
                points.Add(Utils.GetTile(map, p.x, p.y, p.z, majorOrder));
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
                T tile = Utils.GetTile(map, p.x, p.y, p.z, majorOrder);
                breakIt = breakIt ? true : breakOnWalls && (tile == null || !tile.IsWalkable);
                isLineClear = !breakIt;
                breakIt = breakIt ? true : (maxDistance > 0f && Vector3Int.Distance(new Vector3Int(p.x, p.y, p.z), new Vector3Int(startTile.X, startTile.Y, startTile.Z)) > maxDistance);
                bool continueIt = false;
                continueIt = continueIt ? true : tile == null;
                continueIt = continueIt ? true : !includeWalls && !tile.IsWalkable;
                continueIt = continueIt ? true : !includeDestination && Equals(tile, destinationTile);
                if (breakIt)
                {
                    break;
                }
                if (continueIt)
                {
                    continue;
                }
                points.Add(tile);
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
        public static T[] GetTilesOnALine<T>(T[,,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, false, true, out bool isclear, majorOrder);
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
        public static T[] GetWalkableTilesOnALine<T>(T[,,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, false, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
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
        public static T[] GetLineOfSight<T>(T[,,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
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
        public static T[] GetLineOfSight<T>(T[,,] map, T startTile, T destinationTile, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out isLineClear, majorOrder);
        }
    }
    /// <summary>
    /// Allows you to calculate paths on a grid
    /// </summary>
    public class Pathfinding3D
    {
        private static bool GetTile<T>(T[,,] map, int x, int y, int z, out T tile, MajorOrder majorOrder) where T : ITile3D
        {
            if (x > -1 && x < Utils.GetXLength(map, majorOrder) && y > -1 && y < Utils.GetYLength(map, majorOrder) && z > -1 && z < Utils.GetZLength(map, majorOrder))
            {
                tile = Utils.GetTile(map, x, y, z, majorOrder);
                return true;
            }
            tile = default;
            return false;
        }
        private static bool GetLeftNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y, z, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y, z, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBottomNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x, y - 1, z, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetTopNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x, y + 1, z, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x, y, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x, y, z + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBottomNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y - 1, z, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftTopNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y + 1, z, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBottomNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y - 1, z, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightTopNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y + 1, z, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBottomBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x, y - 1, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetTopBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x, y + 1, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBottomFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x, y - 1, z + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetTopFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x, y + 1, z + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y, z + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y, z + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBottomBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y - 1, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBottomBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y - 1, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBottomFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y - 1, z + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBottomFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y - 1, z + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftTopBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y + 1, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightTopBackNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y + 1, z - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftTopFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x - 1, y + 1, z + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightTopFrontNeighbour<T>(T[,,] map, int x, int y, int z, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile3D
        {
            if (GetTile(map, x + 1, y + 1, z + 1, out nei, majorOrder))
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
        private static bool CheckMP<T>(MovementPolicy policy, T[,,] map, T tile, MajorOrder majorOrder) where T : ITile3D
        {
            int polCase = (int)policy;
            if (polCase == 0)
            {
                return true;
            }
            else if (polCase == 1)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true, majorOrder);
            }
            else if (polCase == 2)
            {
                return GetLeftNeighbour(map, tile.X, tile.Y, tile.Z, out T neiL, true, majorOrder) || GetRightNeighbour(map, tile.X, tile.Y, tile.Z, out T neiR, true, majorOrder) || GetBackNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true, majorOrder) || GetFrontNeighbour(map, tile.X, tile.Y, tile.Z, out T neiF, true, majorOrder);
            }
            else if (polCase == 3)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true, majorOrder) || GetLeftNeighbour(map, tile.X, tile.Y, tile.Z, out T neiL, true, majorOrder) || GetRightNeighbour(map, tile.X, tile.Y, tile.Z, out T neiR, true, majorOrder) || GetBackNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true, majorOrder) || GetFrontNeighbour(map, tile.X, tile.Y, tile.Z, out T neiF, true, majorOrder);
            }
            else if (polCase == 4)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true, majorOrder);
            }
            else if (polCase == 5)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true, majorOrder) || GetBottomNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true, majorOrder);
            }
            else if (polCase == 6)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true, majorOrder) || GetLeftNeighbour(map, tile.X, tile.Y, tile.Z, out T neiL, true, majorOrder) || GetRightNeighbour(map, tile.X, tile.Y, tile.Z, out T neiR, true, majorOrder) || GetBackNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true, majorOrder) || GetFrontNeighbour(map, tile.X, tile.Y, tile.Z, out T neiF, true, majorOrder);
            }
            else if (polCase == 7)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, tile.Z, out T nei, true, majorOrder) || GetTopNeighbour(map, tile.X, tile.Y, tile.Z, out T neiT, true, majorOrder) || GetLeftNeighbour(map, tile.X, tile.Y, tile.Z, out T neiL, true, majorOrder) || GetRightNeighbour(map, tile.X, tile.Y, tile.Z, out T neiR, true, majorOrder) || GetBackNeighbour(map, tile.X, tile.Y, tile.Z, out T neiB, true, majorOrder) || GetFrontNeighbour(map, tile.X, tile.Y, tile.Z, out T neiF, true, majorOrder);
            }
            return true;
        }
        private static List<T> GetTileNeighbours<T>(T[,,] map, int x, int y, int z, Pathfinding3DPolicy pathfindingPolicy, MajorOrder majorOrder) where T : ITile3D
        {
            List<T> nodes = new List<T>();
            T nei;
            // 6
            bool leftWalkable = GetLeftNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (leftWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool rightWalkable = GetRightNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (rightWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool bottomWalkable = GetBottomNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (bottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool topWalkable = GetTopNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (topWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool backWalkable = GetBackNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (backWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool frontWalkable = GetFrontNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (frontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            //18
            bool leftBackWalkable = GetLeftBackNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, leftWalkable, backWalkable) && leftBackWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                leftBackWalkable = true;
            }
            bool rightBackWalkable = GetRightBackNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, rightWalkable, backWalkable) && rightBackWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                rightBackWalkable = true;
            }
            bool leftFrontWalkable = GetLeftFrontNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, leftWalkable, frontWalkable) && leftFrontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                leftFrontWalkable = true;
            }
            bool rightFrontWalkable = GetRightFrontNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.HorizontalEdgesDiagonalsPolicy, rightWalkable, frontWalkable) && rightFrontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                rightFrontWalkable = true;
            }
            bool leftBottomWalkable = GetLeftBottomNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, leftWalkable, bottomWalkable) && leftBottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                leftBottomWalkable = true;
            }
            bool rightBottomWalkable = GetRightBottomNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, rightWalkable, bottomWalkable) && rightBottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                rightBottomWalkable = true;
            }
            bool leftTopWalkable = GetLeftTopNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, leftWalkable, topWalkable) && leftTopWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                leftTopWalkable = true;
            }
            bool rightTopWalkable = GetRightTopNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, rightWalkable, topWalkable) && rightTopWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                rightTopWalkable = true;
            }
            bool bottomBackWalkable = GetBottomBackNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, bottomWalkable, backWalkable) && bottomBackWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                bottomBackWalkable = true;
            }
            bool topBackWalkable = GetTopBackNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, topWalkable, backWalkable) && topBackWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                topBackWalkable = true;
            }
            bool bottomFrontWalkable = GetBottomFrontNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, bottomWalkable, frontWalkable) && bottomFrontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                bottomFrontWalkable = true;
            }
            bool topFrontWalkable = GetTopFrontNeighbour(map, x, y, z, out nei, false, majorOrder);
            if (CheckEDP(pathfindingPolicy.VerticalEdgesDiagonalsPolicy, topWalkable, frontWalkable) && topFrontWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
                topFrontWalkable = true;
            }
            // 26
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, bottomBackWalkable, backWalkable, leftBottomWalkable, leftWalkable, bottomWalkable, leftBackWalkable) && GetLeftBottomBackNeighbour(map, x, y, z, out nei, false, majorOrder) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, bottomBackWalkable, backWalkable, rightBottomWalkable, rightWalkable, bottomWalkable, rightBackWalkable) && GetRightBottomBackNeighbour(map, x, y, z, out nei, false, majorOrder) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, bottomFrontWalkable, frontWalkable, leftBottomWalkable, leftWalkable, bottomWalkable, leftFrontWalkable) && GetLeftBottomFrontNeighbour(map, x, y, z, out nei, false, majorOrder) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, bottomFrontWalkable, frontWalkable, rightBottomWalkable, rightWalkable, bottomWalkable, rightFrontWalkable) && GetRightBottomFrontNeighbour(map, x, y, z, out nei, false, majorOrder) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, topBackWalkable, backWalkable, leftTopWalkable, leftWalkable, topWalkable, leftBackWalkable) && GetLeftTopBackNeighbour(map, x, y, z, out nei, false, majorOrder) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, topBackWalkable, backWalkable, rightTopWalkable, rightWalkable, topWalkable, rightBackWalkable) && GetRightTopBackNeighbour(map, x, y, z, out nei, false, majorOrder) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, topFrontWalkable, frontWalkable, leftTopWalkable, leftWalkable, topWalkable, leftFrontWalkable) && GetLeftTopFrontNeighbour(map, x, y, z, out nei, false, majorOrder) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            if (CheckVDP(pathfindingPolicy.VerticesDiagonalsPolicy, topFrontWalkable, frontWalkable, rightTopWalkable, rightWalkable, topWalkable, rightFrontWalkable) && GetRightTopFrontNeighbour(map, x, y, z, out nei, false, majorOrder) && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
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
        /// Generates a PathMap object that will contain all the pre-calculated paths data between a target tile and all the accessible tiles from this target
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="targetTile">The target tile for the paths calculation</param>
        /// <param name="allowDiagonals">Allow diagonals movements</param>
        /// <param name="diagonalWeightRatio">Diagonal movement weight</param>
        /// <returns>A PathMap object</returns>
        public static PathMap3D<T> GeneratePathMap<T>(T[,,] map, T targetTile, float maxDistance = 0f, Pathfinding3DPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.YXZ) where T : ITile3D
        {
            if (!targetTile.IsWalkable)
            {
                throw new System.Exception("Do not try to generate a PathMap with an unwalkable tile as the target");
            }
            Node3D<T> targetNode = new(targetTile);
            Dictionary<T, Node3D<T>> accessibleTilesDico = new() { { targetTile, targetNode } };
            List<T> accessibleTiles = new() { targetTile };
            PriorityQueueUnityPort.PriorityQueue<Node3D<T>, float> frontier = new();
            frontier.Enqueue(targetNode, 0);
            targetNode.NextNode = targetNode;
            targetNode.DistanceToTarget = 0f;

            while (frontier.Count > 0)
            {
                Node3D<T> current = frontier.Dequeue();
                List<T> neighbourgs = GetTileNeighbours(map, current.Tile.X, current.Tile.Y, current.Tile.Z, pathfindingPolicy, majorOrder);
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
            return new PathMap3D<T>(accessibleTilesDico, accessibleTiles, targetTile, maxDistance, pathfindingPolicy);
        }

    }
    internal static class Utils
    {
        internal static T GetTile<T>(T[,,] map, int x, int y, int z, MajorOrder majorOrder) where T : ITile3D
        {
            if (majorOrder == MajorOrder.XYZ)
            {
                return map[x, y, z];
            }
            else if (majorOrder == MajorOrder.XZY)
            {
                return map[x, z, y];
            }
            else if (majorOrder == MajorOrder.YXZ)
            {
                return map[y, x, z];
            }
            else if (majorOrder == MajorOrder.YZX)
            {
                return map[y, z, x];
            }
            else if (majorOrder == MajorOrder.ZXY)
            {
                return map[z, x, y];
            }
            else
            {
                return map[z, y, x];
            }
        }
        internal static int GetXLength<T>(T[,,] map, MajorOrder majorOrder) where T : ITile3D
        {
            if (majorOrder == MajorOrder.XYZ || majorOrder == MajorOrder.XZY)
            {
                return map.GetLength(0);
            }
            else if (majorOrder == MajorOrder.YXZ || majorOrder == MajorOrder.ZXY)
            {
                return map.GetLength(1);
            }
            else
            {
                return map.GetLength(2);
            }
        }
        internal static int GetYLength<T>(T[,,] map, MajorOrder majorOrder) where T : ITile3D
        {
            if (majorOrder == MajorOrder.XZY || majorOrder == MajorOrder.ZXY)
            {
                return map.GetLength(2);
            }
            else if (majorOrder == MajorOrder.YXZ || majorOrder == MajorOrder.YZX)
            {
                return map.GetLength(0);
            }
            else
            {
                return map.GetLength(1);
            }
        }
        internal static int GetZLength<T>(T[,,] map, MajorOrder majorOrder) where T : ITile3D
        {
            if (majorOrder == MajorOrder.XYZ || majorOrder == MajorOrder.YXZ)
            {
                return map.GetLength(2);
            }
            else if (majorOrder == MajorOrder.XZY || majorOrder == MajorOrder.YZX)
            {
                return map.GetLength(1);
            }
            else
            {
                return map.GetLength(0);
            }
        }
    }
}
