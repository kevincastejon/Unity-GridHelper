using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Helper static classes for 2D grid operations
/// </summary>
namespace KevinCastejon.GridHelper
{
    /// <summary>
    /// Major order rule
    /// ROW_MAJOR_ORDER : YX
    /// COLUMN_MAJOR_ORDER : XY
    /// </summary>
    public enum MajorOrder
    {
        ROW_MAJOR_ORDER,
        COLUMN_MAJOR_ORDER
    }
    /// <summary>
    /// Represents the diagonals permissiveness
    /// NONE : no diagonal movement allowed
    /// DIAGONAL_2FREE : only diagonal movements with two walkable common face neighbours of the start and destination tiles are allowed
    /// DIAGONAL_1FREE : only diagonal movements with one walkable common face neighbour of the start and destination tiles are allowed
    /// ALL_DIAGONALS : all diagonal movements allowed
    /// </summary>
    public enum DiagonalsPolicy
    {
        NONE,
        DIAGONAL_2FREE,
        DIAGONAL_1FREE,
        ALL_DIAGONALS,
    }
    /// <summary>
    /// Represents the movement permissiveness. It is useful to allow special movement, especially for side-view games, such as spiders that can walk on walls or roofs, or flying characters.
    /// Default is FLY. 
    /// Top-down view grid based games should not use other value than the default as they do not hold concept of "gravity" nor "up-and-down".
    /// FLY : all walkable tiles can be walk thought
    /// WALL_BELOW : only walkable tiles that has a not-walkable lower neighbour can be walk thought
    /// WALL_ASIDE : only walkable tiles that has a not-walkable side neighbour can be walk thought
    /// WALL_ABOVE : only walkable tiles that has a not-walkable upper neighbour can be walk thought
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
    public struct PathfindingPolicy
    {
        [SerializeField] private DiagonalsPolicy _diagonalsPolicy;
        [SerializeField] private float _diagonalsWeight;
        [SerializeField] private MovementPolicy _movementPolicy;

        /// <summary>
        /// Set of parameters to use for the pathfinding.
        /// </summary>
        /// <param name="diagonalsPolicy">The DiagonalsPolicy</param>
        /// <param name="diagonalsWeight">The diagonals weight ratio</param>
        /// <param name="movementPolicy">The MovementPolicy</param>
        public PathfindingPolicy(DiagonalsPolicy diagonalsPolicy = DiagonalsPolicy.DIAGONAL_2FREE, float diagonalsWeight = 1.4142135623730950488016887242097f, MovementPolicy movementPolicy = MovementPolicy.FLY)
        {
            _diagonalsPolicy = diagonalsPolicy;
            _diagonalsWeight = diagonalsWeight;
            _movementPolicy = movementPolicy;
        }

        public DiagonalsPolicy DiagonalsPolicy { get => _diagonalsPolicy; set => _diagonalsPolicy = value; }
        public float DiagonalsWeight { get => _diagonalsWeight; set => _diagonalsWeight = value; }
        public MovementPolicy MovementPolicy { get => _movementPolicy; set => _movementPolicy = value; }
    }
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
    /// An object containing all the calculated paths data from a target tile and all the others accessible tiles.
    /// </summary>
    /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
    public class PathMap<T> where T : ITile
    {
        private readonly Dictionary<T, Node<T>> _dico;
        private readonly List<T> _accessibleTiles;
        private readonly T _target;
        private readonly float _maxDistance;
        private readonly MajorOrder _majorOrder;

        internal PathMap(Dictionary<T, Node<T>> accessibleTilesDico, List<T> accessibleTiles, T target, float maxDistance, MajorOrder majorOrder)
        {
            _dico = accessibleTilesDico;
            _accessibleTiles = accessibleTiles;
            _target = target;
            _maxDistance = maxDistance;
            _majorOrder = majorOrder;
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
        /// The MajorOrder parameter value that has been used to generate this PathMap
        /// </summary>
        public MajorOrder MajorOrder { get => _majorOrder; }

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
        /// <returns>A Vector2Int direction</returns>
        public Vector2Int GetNextTileDirectionFromTile(T tile)
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
            Node<T> node = includeStart ? _dico[startTile] : _dico[startTile].NextNode;
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
    /// Allows you to extract tiles on a grid
    /// </summary>
    public class Extraction
    {
        private static T[] ExtractRectangle<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, bool includeCenter, bool includeWalls, MajorOrder majorOrder) where T : ITile
        {
            int bottom = Mathf.Max(center.Y - rectangleSizeY, 0),
                top = Mathf.Min(center.Y + rectangleSizeY + 1, GridUtils.GetYLength(map, majorOrder)),
                left = Mathf.Max(center.X - rectangleSizeX, 0),
                right = Mathf.Min(center.X + rectangleSizeX + 1, GridUtils.GetXLength(map, majorOrder));
            List<T> list = new List<T>();
            for (int i = bottom; i < top; i++)
            {
                for (int j = left; j < right; j++)
                {
                    T tile = GridUtils.GetTile(map, j, i, majorOrder);
                    if (tile != null && (includeWalls || tile.IsWalkable) && (includeCenter || !EqualityComparer<T>.Default.Equals(tile, center)))
                    {
                        list.Add(tile);
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractRectangleOutline<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, bool includeWalls, MajorOrder majorOrder) where T : ITile
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
                    if (i < 0 || i >= GridUtils.GetYLength(map, majorOrder) || j < 0 || j >= GridUtils.GetXLength(map, majorOrder))
                    {
                        continue;
                    }
                    T tile = GridUtils.GetTile(map, j, i, majorOrder);
                    if (tile != null && (includeWalls || tile.IsWalkable) && (i == top - 1 || i == bottom || j == left || j == right - 1))
                    {
                        list.Add(tile);
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractCircleArc<T>(T[,] map, T center, float radius, bool includeCenter, bool includeWalls, MajorOrder majorOrder, float angle, Vector2 direction) where T : ITile
        {
            int bottom = Mathf.RoundToInt(Mathf.Max(center.Y - radius, 0)),
                top = Mathf.RoundToInt(Mathf.Min(center.Y + radius + 1, GridUtils.GetYLength(map, majorOrder)));
            List<T> list = new List<T>();
            for (int y = bottom; y < top; y++)
            {
                int dy = y - center.Y;
                float dx = Mathf.Sqrt((float)radius * radius - (float)dy * dy);
                int left = Mathf.Max(Mathf.CeilToInt(center.X - dx), 0),
                    right = Mathf.Min(Mathf.FloorToInt(center.X + dx + 1), GridUtils.GetXLength(map, majorOrder));
                for (int x = left; x < right; x++)
                {
                    T tile = GridUtils.GetTile(map, x, y, majorOrder);
                    if (tile != null && IsIntoAngle(center.X, center.Y, tile.X, tile.Y, angle, direction) && (includeWalls || tile.IsWalkable) && (includeCenter || !EqualityComparer<T>.Default.Equals(tile, center)))
                    {
                        list.Add(tile);
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractCircleArcOutline<T>(T[,] map, T center, float radius, bool includeWalls, MajorOrder majorOrder, float angle, Vector2 direction) where T : ITile
        {
            int bottom = Mathf.RoundToInt(Mathf.Max(center.Y - radius, 0)),
                top = Mathf.RoundToInt(Mathf.Min(center.Y + radius + 1, GridUtils.GetYLength(map, majorOrder)));
            int right = GridUtils.GetXLength(map, majorOrder);
            List<T> list = new List<T>();
            for (int y = bottom; y < top; y++)
            {
                for (int r = 0; r <= Mathf.FloorToInt(radius * Mathf.Sqrt(0.5f)); r++)
                {
                    int dd = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - r * r));
                    Vector2Int a = new Vector2Int(center.X - dd, center.Y + r);
                    if (a.y >= 0 && a.y < top && a.x >= 0 && a.x < right && GridUtils.GetTile(map, a.x, a.y, majorOrder) != null && IsIntoAngle(center.X, center.Y, GridUtils.GetTile(map, a.x, a.y, majorOrder).X, GridUtils.GetTile(map, a.x, a.y, majorOrder).Y, angle, direction) && (includeWalls || GridUtils.GetTile(map, a.x, a.y, majorOrder).IsWalkable) && !list.Contains(GridUtils.GetTile(map, a.x, a.y, majorOrder))) list.Add(GridUtils.GetTile(map, a.x, a.y, majorOrder));
                    a = new Vector2Int(center.X + dd, center.Y + r);
                    if (a.y >= 0 && a.y < top && a.x >= 0 && a.x < right && GridUtils.GetTile(map, a.x, a.y, majorOrder) != null && IsIntoAngle(center.X, center.Y, GridUtils.GetTile(map, a.x, a.y, majorOrder).X, GridUtils.GetTile(map, a.x, a.y, majorOrder).Y, angle, direction) && (includeWalls || GridUtils.GetTile(map, a.x, a.y, majorOrder).IsWalkable) && !list.Contains(GridUtils.GetTile(map, a.x, a.y, majorOrder))) list.Add(GridUtils.GetTile(map, a.x, a.y, majorOrder));
                    a = new Vector2Int(center.X - dd, center.Y - r);
                    if (a.y >= 0 && a.y < top && a.x >= 0 && a.x < right && GridUtils.GetTile(map, a.x, a.y, majorOrder) != null && IsIntoAngle(center.X, center.Y, GridUtils.GetTile(map, a.x, a.y, majorOrder).X, GridUtils.GetTile(map, a.x, a.y, majorOrder).Y, angle, direction) && (includeWalls || GridUtils.GetTile(map, a.x, a.y, majorOrder).IsWalkable) && !list.Contains(GridUtils.GetTile(map, a.x, a.y, majorOrder))) list.Add(GridUtils.GetTile(map, a.x, a.y, majorOrder));
                    a = new Vector2Int(center.X + dd, center.Y - r);
                    if (a.y >= 0 && a.y < top && a.x >= 0 && a.x < right && GridUtils.GetTile(map, a.x, a.y, majorOrder) != null && IsIntoAngle(center.X, center.Y, GridUtils.GetTile(map, a.x, a.y, majorOrder).X, GridUtils.GetTile(map, a.x, a.y, majorOrder).Y, angle, direction) && (includeWalls || GridUtils.GetTile(map, a.x, a.y, majorOrder).IsWalkable) && !list.Contains(GridUtils.GetTile(map, a.x, a.y, majorOrder))) list.Add(GridUtils.GetTile(map, a.x, a.y, majorOrder));
                    a = new Vector2Int(center.X + r, center.Y - dd);
                    if (a.y >= 0 && a.y < top && a.x >= 0 && a.x < right && GridUtils.GetTile(map, a.x, a.y, majorOrder) != null && IsIntoAngle(center.X, center.Y, GridUtils.GetTile(map, a.x, a.y, majorOrder).X, GridUtils.GetTile(map, a.x, a.y, majorOrder).Y, angle, direction) && (includeWalls || GridUtils.GetTile(map, a.x, a.y, majorOrder).IsWalkable) && !list.Contains(GridUtils.GetTile(map, a.x, a.y, majorOrder))) list.Add(GridUtils.GetTile(map, a.x, a.y, majorOrder));
                    a = new Vector2Int(center.X + r, center.Y + dd);
                    if (a.y >= 0 && a.y < top && a.x >= 0 && a.x < right && GridUtils.GetTile(map, a.x, a.y, majorOrder) != null && IsIntoAngle(center.X, center.Y, GridUtils.GetTile(map, a.x, a.y, majorOrder).X, GridUtils.GetTile(map, a.x, a.y, majorOrder).Y, angle, direction) && (includeWalls || GridUtils.GetTile(map, a.x, a.y, majorOrder).IsWalkable) && !list.Contains(GridUtils.GetTile(map, a.x, a.y, majorOrder))) list.Add(GridUtils.GetTile(map, a.x, a.y, majorOrder));
                    a = new Vector2Int(center.X - r, center.Y - dd);
                    if (a.y >= 0 && a.y < top && a.x >= 0 && a.x < right && GridUtils.GetTile(map, a.x, a.y, majorOrder) != null && IsIntoAngle(center.X, center.Y, GridUtils.GetTile(map, a.x, a.y, majorOrder).X, GridUtils.GetTile(map, a.x, a.y, majorOrder).Y, angle, direction) && (includeWalls || GridUtils.GetTile(map, a.x, a.y, majorOrder).IsWalkable) && !list.Contains(GridUtils.GetTile(map, a.x, a.y, majorOrder))) list.Add(GridUtils.GetTile(map, a.x, a.y, majorOrder));
                    a = new Vector2Int(center.X - r, center.Y + dd);
                    if (a.y >= 0 && a.y < top && a.x >= 0 && a.x < right && GridUtils.GetTile(map, a.x, a.y, majorOrder) != null && IsIntoAngle(center.X, center.Y, GridUtils.GetTile(map, a.x, a.y, majorOrder).X, GridUtils.GetTile(map, a.x, a.y, majorOrder).Y, angle, direction) && (includeWalls || GridUtils.GetTile(map, a.x, a.y, majorOrder).IsWalkable) && !list.Contains(GridUtils.GetTile(map, a.x, a.y, majorOrder))) list.Add(GridUtils.GetTile(map, a.x, a.y, majorOrder));
                }
            }
            return list.ToArray();
        }
        private static T[] BresenhamExtractCircleArcOutline<T>(T[,] map, T center, float radius, bool includeWalls, MajorOrder majorOrder, float angle, Vector2 direction) where T : ITile
        {
            int intRadius = Mathf.RoundToInt(radius);
            List<T> points = new List<T>();
            int x = 0;
            int y = -intRadius;
            int F_M = 1 - intRadius;
            int d_e = 3;
            int d_ne = -(intRadius << 1) + 5;
            points = points.Concat(GetTileNeighbours(map, center, includeWalls, majorOrder)).ToList();
            while (x < -y)
            {
                if (F_M <= 0)
                {
                    F_M += d_e;
                }
                else
                {
                    F_M += d_ne;
                    d_ne += 2;
                    y += 1;
                }
                d_e += 2;
                d_ne += 2;
                x += 1;
                if (GridUtils.AreCoordsIntoGrid(map, x, y, majorOrder))
                {
                    points = points.Concat(GetTileNeighbours(map, GridUtils.GetTile(map, x, y, majorOrder), includeWalls, majorOrder)).ToList();
                }
            }
            return points.ToArray();
        }
        internal static bool IsIntoAngle(int tileAX, int tileAY, int tileBX, int tileBY, float directionAngle, Vector2 direction)
        {
            Vector2 realDirection = (new Vector2(tileBX, tileBY) - new Vector2(tileAX, tileAY)).normalized;
            float angleDiff = Vector2.Angle(realDirection, direction.normalized);
            return angleDiff <= directionAngle / 2;
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
        public static T[] GetTilesInARectangle<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return GetTilesInARectangle(map, center, rectangleSize.x, rectangleSize.y, includeCenter, majorOrder);
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
        public static T[] GetTilesInARectangle<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return ExtractRectangle(map, center, rectangleSizeX, rectangleSizeY, includeCenter, true, majorOrder);
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
        public static T[] GetWalkableTilesInARectangle<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return GetWalkableTilesInARectangle(map, center, rectangleSize.x, rectangleSize.y, includeCenter, majorOrder);
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
        public static T[] GetWalkableTilesInARectangle<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return ExtractRectangle(map, center, rectangleSizeX, rectangleSizeY, includeCenter, false, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a rectangle outline around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of any objects</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnARectangleOutline<T>(T[,] map, T center, Vector2Int rectangleSize, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return GetTilesOnARectangleOutline(map, center, rectangleSize.x, rectangleSize.y, majorOrder);
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
        public static T[] GetTilesOnARectangleOutline<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return ExtractRectangleOutline(map, center, rectangleSizeX, rectangleSizeY, true, majorOrder);
        }
        /// <summary>
        /// Get all walkable tiles on a rectangle outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnARectangleOutline<T>(T[,] map, T center, Vector2Int rectangleSize, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return GetWalkableTilesOnARectangleOutline(map, center, rectangleSize.x, rectangleSize.y, majorOrder);
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
        public static T[] GetWalkableTilesOnARectangleOutline<T>(T[,] map, T center, int rectangleSizeX, int rectangleSizeY, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return ExtractRectangleOutline(map, center, rectangleSizeX, rectangleSizeY, false, majorOrder);
        }
        /// <summary>
        /// Get all tiles contained into a circle around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACircle<T>(T[,] map, T center, float radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return ExtractCircleArc(map, center, radius, includeCenter, true, majorOrder, 360f, Vector2.right);
        }
        /// <summary>
        /// Get all tiles on a circle outline around a tile
        /// </summary>
        /// <typeparam name="T">Any object type</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile </param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnACircleOutline<T>(T[,] map, T center, float radius, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return ExtractCircleArcOutline(map, center, radius, true, majorOrder, 360f, Vector2.right);
        }
        public static T[] BresenhamGetWalkableTilesOnACircleOutline<T>(T[,] map, T center, float radius, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return BresenhamExtractCircleArcOutline(map, center, radius, false, majorOrder, 360f, Vector2.right);
        }
        /// <summary>
        /// Get all walkable tiles contained into a circle around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesInACircle<T>(T[,] map, T center, float radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return ExtractCircleArc(map, center, radius, includeCenter, false, majorOrder, 360f, Vector2.right);
        }
        /// <summary>
        /// Get all walkable tiles on a circle outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnACircleOutline<T>(T[,] map, T center, float radius, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return ExtractCircleArcOutline(map, center, radius, false, majorOrder, 360f, Vector2.right);
        }
        /// <summary>
        /// Get all tiles contained into a cone around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACone<T>(T[,] map, T center, int radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER, float angle = 360f, float direction = 0) where T : ITile
        {
            direction = Mathf.Clamp(direction, 0f, 360f);
            return GetTilesInACone(map, center, radius, includeCenter, majorOrder, angle, Quaternion.AngleAxis(direction, Vector3.back) * Vector2.right);
        }
        /// <summary>
        /// Get all tiles contained into a cone around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACone<T>(T[,] map, T center, int radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER, float angle = 360f, Vector2 direction = new Vector2()) where T : ITile
        {
            angle = Mathf.Clamp(angle, 0f, 360f);
            if (direction == Vector2.zero)
            {
                direction = Vector2.right;
            }
            return ExtractCircleArc(map, center, radius, includeCenter, true, majorOrder, angle, direction);
        }
        /// <summary>
        /// Get all walkable tiles contained into a cone around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesInACone<T>(T[,] map, T center, int radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER, float angle = 360f, float direction = 0) where T : ITile
        {
            direction = Mathf.Clamp(direction, 0f, 360f);
            return GetWalkableTilesInACone(map, center, radius, includeCenter, majorOrder, angle, Quaternion.AngleAxis(direction, Vector3.back) * Vector2.right);
        }
        /// <summary>
        /// Get all walkable tiles contained into a cone around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesInACone<T>(T[,] map, T center, int radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER, float angle = 360f, Vector2 direction = new Vector2()) where T : ITile
        {
            angle = Mathf.Clamp(angle, 0f, 360f);
            if (direction == Vector2.zero)
            {
                direction = Vector2.right;
            }
            float directionAngle = Vector2.SignedAngle(Vector2.right, direction);
            T[] arc = ExtractCircleArc(map, center, radius, includeCenter, false, majorOrder, angle, direction);
            T[] line1 = Raycasting.Raycast(map, center, directionAngle - (angle * 0.5f), radius, includeCenter, false, false, false, out bool islineclear, majorOrder);
            T[] line2 = Raycasting.Raycast(map, center, directionAngle + (angle * 0.5f), radius, includeCenter, false, false, false, out bool islineclear2, majorOrder);
            return arc.Concat(line1).Concat(line2).ToArray();
        }
        /// <summary>
        /// Get all tiles on a cone outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnAConeOutline<T>(T[,] map, T center, int radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER, float angle = 360f, Vector2 direction = new Vector2()) where T : ITile
        {
            angle = Mathf.Clamp(angle, 0f, 360f);
            if (direction == Vector2.zero)
            {
                direction = Vector2.right;
            }
            float directionAngle = Vector2.SignedAngle(Vector2.right, direction);
            T[] arc = ExtractCircleArcOutline(map, center, radius, true, majorOrder, angle, direction);
            T[] line1 = Raycasting.Raycast(map, center, directionAngle - (angle * 0.5f), radius, includeCenter, false, false, false, out bool islineclear, majorOrder);
            T[] line2 = Raycasting.Raycast(map, center, directionAngle + (angle * 0.5f), radius, includeCenter, false, false, false, out bool islineclear2, majorOrder);
            return arc.Concat(line1).Concat(line2).ToArray();
        }
        /// <summary>
        /// Get all tiles on a cone outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnAConeOutline<T>(T[,] map, T center, int radius, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER, float angle = 360f, float direction = 0f) where T : ITile
        {
            direction = Mathf.Clamp(direction, 0f, 360f);
            return GetTilesOnAConeOutline(map, center, radius, false, majorOrder, angle, Quaternion.AngleAxis(direction, Vector3.back) * Vector2.right);
        }
        /// <summary>
        /// Get all walkable tiles on a cone outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnAConeOutline<T>(T[,] map, T center, int radius, bool includeCenter = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER, float angle = 360f, Vector2 direction = new Vector2()) where T : ITile
        {
            if (direction == Vector2.zero)
            {
                direction = Vector2.right;
            }
            float directionAngle = Vector2.SignedAngle(Vector2.right, direction);
            T[] arc = ExtractCircleArcOutline(map, center, radius, false, majorOrder, angle, direction);
            T[] line1 = Raycasting.Raycast(map, center, directionAngle - (angle * 0.5f), radius, includeCenter, false, false, false, out bool islineclear, majorOrder);
            T[] line2 = Raycasting.Raycast(map, center, directionAngle + (angle * 0.5f), radius, includeCenter, false, false, false, out bool islineclear2, majorOrder);
            return arc.Concat(line1).Concat(line2).ToArray();
        }
        /// <summary>
        /// Get all walkable tiles on a cone outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The radius</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnAConeOutline<T>(T[,] map, T center, int radius, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER, float angle = 360f, float direction = 0f) where T : ITile
        {
            angle = Mathf.Clamp(angle, 0f, 360f);
            direction = Mathf.Clamp(direction, 0f, 360f);
            return GetWalkableTilesOnAConeOutline(map, center, radius, false, majorOrder, angle, Quaternion.AngleAxis(direction, Vector3.back) * Vector2.right);
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
        public static bool GetTileNeighbour<T>(T[,] map, T tile, Vector3Int neighbourDirection, out T neighbour, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            int x = neighbourDirection.x > 0 ? tile.X + 1 : (neighbourDirection.x < 0 ? tile.X - 1 : tile.X);
            int y = neighbourDirection.y > 0 ? tile.Y + 1 : (neighbourDirection.y < 0 ? tile.Y - 1 : tile.Y);
            if (neighbourDirection.x < 0 && tile.X - 1 < 0 || neighbourDirection.x > 0 && tile.X + 1 >= GridUtils.GetXLength(map, majorOrder))
            {
                neighbour = default;
                return false;
            }
            if (neighbourDirection.y < 0 && tile.Y - 1 < 0 || neighbourDirection.y > 0 && tile.Y + 1 >= GridUtils.GetYLength(map, majorOrder))
            {
                neighbour = default;
                return false;
            }
            neighbour = GridUtils.GetTile(map, x, y, majorOrder);
            return true;
        }
        /// <summary>
        /// Get neighbour of a tile if it exists
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="neighbour">The neighbour of a tile</param>
        /// <returns></returns>
        public static T[] GetTileNeighbours<T>(T[,] map, T tile, bool includeWalls, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            List<T> neis = new List<T>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    if (GridUtils.AreCoordsIntoGrid(map, tile.X + i, tile.Y + j, majorOrder))
                    {
                        T nei = GridUtils.GetTile(map, tile.X + i, tile.Y + j, majorOrder);
                        if (includeWalls || nei.IsWalkable)
                        {
                            neis.Add(GridUtils.GetTile(map, tile.X + i, tile.Y + j, majorOrder));
                        }
                    }
                }
            }
            return neis.ToArray();
        }
        /// <summary>
        /// Get neighbour of a tile if it exists
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="neighbour">The neighbour of a tile</param>
        /// <returns></returns>
        public static T[] GetTileFacesNeighbours<T>(T[,] map, T tile, bool includeWalls, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            List<T> neis = new List<T>();
            if (GridUtils.AreCoordsIntoGrid(map, tile.X - 1, tile.Y + 0, majorOrder))
            {
                T nei = GridUtils.GetTile(map, tile.X - 1, tile.Y + 0, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, tile.X + 1, tile.Y + 0, majorOrder))
            {
                T nei = GridUtils.GetTile(map, tile.X + 1, tile.Y + 0, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, tile.X + 0, tile.Y - 1, majorOrder))
            {
                T nei = GridUtils.GetTile(map, tile.X + 0, tile.Y - 1, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, tile.X + 0, tile.Y + 1, majorOrder))
            {
                T nei = GridUtils.GetTile(map, tile.X + 0, tile.Y + 1, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neis.Add(nei);
                }
            }
            return neis.ToArray();
        }
        /// <summary>
        /// Get neighbour of a tile if it exists
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="neighbour">The neighbour of a tile</param>
        /// <returns></returns>
        public static T[] GetTileDiagonalsNeighbours<T>(T[,] map, T tile, bool includeWalls, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            List<T> neis = new List<T>();

            if (GridUtils.AreCoordsIntoGrid(map, tile.X - 1, tile.Y - 1, majorOrder))
            {
                T nei = GridUtils.GetTile(map, tile.X - 1, tile.Y - 1, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, tile.X - 1, tile.Y + 1, majorOrder))
            {
                T nei = GridUtils.GetTile(map, tile.X - 1, tile.Y + 1, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, tile.X + 1, tile.Y - 1, majorOrder))
            {
                T nei = GridUtils.GetTile(map, tile.X + 1, tile.Y - 1, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, tile.X + 1, tile.Y + 1, majorOrder))
            {
                T nei = GridUtils.GetTile(map, tile.X + 1, tile.Y + 1, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neis.Add(nei);
                }
            }
            return neis.ToArray();
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
        /// Is this tile contained into a circle or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="radius">The radius</param>
        /// <returns>A boolean that is true if the tile is contained into the radius, false otherwise</returns>
        public static bool IsTileInACircle<T>(T center, T tile, int radius) where T : ITile
        {
            int bottom = center.Y - radius;
            int top = center.Y + radius + 1;
            int left = center.X - radius;
            int right = center.X + radius + 1;
            return tile.X >= left && tile.X <= right && tile.Y >= bottom && tile.Y <= top && Vector2Int.Distance(new Vector2Int(center.X, center.Y), new Vector2Int(tile.X, tile.Y)) <= radius;
        }
        /// <summary>
        /// Is this tile on a circle outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="center">The center tile</param>
        /// <param name="tile">To tile to check</param>
        /// <param name="radius">The radius</param>
        /// <returns>A boolean that is true if the tile on a circle outline , false otherwise</returns>
        public static bool IsTileOnACircleOutline<T>(T center, T tile, int radius) where T : ITile
        {
            int bottom = center.Y - radius;
            int top = center.Y + radius + 1;
            for (int y = bottom; y < top; y++)
            {
                for (int r = 0; r <= Mathf.FloorToInt(radius * Mathf.Sqrt(0.5f)); r++)
                {
                    int dd = Mathf.FloorToInt(Mathf.Sqrt(radius * radius - r * r));
                    Vector2Int a = new Vector2Int(center.X - dd, center.Y + r);
                    if (a.y == tile.Y && a.x == tile.X) return true;
                    Vector2Int b = new Vector2Int(center.X + dd, center.Y + r);
                    if (b.y == tile.Y && b.x == tile.X) return true;
                    Vector2Int c = new Vector2Int(center.X - dd, center.Y - r);
                    if (c.y == tile.Y && c.x == tile.X) return true;
                    Vector2Int d = new Vector2Int(center.X + dd, center.Y - r);
                    if (d.y == tile.Y && d.x == tile.X) return true;
                    Vector2Int e = new Vector2Int(center.X + r, center.Y - dd);
                    if (e.y == tile.Y && e.x == tile.X) return true;
                    Vector2Int f = new Vector2Int(center.X + r, center.Y + dd);
                    if (f.y == tile.Y && f.x == tile.X) return true;
                    Vector2Int g = new Vector2Int(center.X - r, center.Y - dd);
                    if (g.y == tile.Y && g.x == tile.X) return true;
                    Vector2Int h = new Vector2Int(center.X - r, center.Y + dd);
                    if (h.y == tile.Y && h.x == tile.X) return true;
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
        internal static T[] Raycast<T>(T[,] map, T startTile, T endTile, float maxDistance, bool includeStart, bool includeDestination, bool breakOnWalls, bool includeWalls, out bool isLineClear, MajorOrder majorOrder) where T : ITile
        {
            if (Mathf.Approximately(maxDistance, 0f))
            {
                maxDistance = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            }
            Vector2Int endPos = new Vector2Int(endTile.X, endTile.Y);
            return Raycast(map, startTile, endPos, maxDistance, includeStart, includeDestination, breakOnWalls, includeWalls, out isLineClear, majorOrder);
        }
        internal static T[] Raycast<T>(T[,] map, T startTile, float directionAngle, float maxDistance, bool includeStart, bool includeDestination, bool breakOnWalls, bool includeWalls, out bool isLineClear, MajorOrder majorOrder) where T : ITile
        {
            if (Mathf.Approximately(maxDistance, 0f))
            {
                maxDistance = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            }
            Vector2Int endPos = new Vector2Int(Mathf.RoundToInt(startTile.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * maxDistance), Mathf.RoundToInt(startTile.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * maxDistance));
            return Raycast(map, startTile, endPos, maxDistance, includeStart, includeDestination, breakOnWalls, includeWalls, out isLineClear, majorOrder);
        }
        internal static T[] Raycast<T>(T[,] map, T startTile, Vector2 direction, float maxDistance, bool includeStart, bool includeDestination, bool breakOnWalls, bool includeWalls, out bool isLineClear, MajorOrder majorOrder) where T : ITile
        {
            if (Mathf.Approximately(maxDistance, 0f))
            {
                maxDistance = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            }
            Vector2Int endPos = Vector2Int.RoundToInt(new Vector2(startTile.X, startTile.Y) + (direction.normalized * maxDistance));
            return Raycast(map, startTile, endPos, maxDistance, includeStart, includeDestination, breakOnWalls, includeWalls, out isLineClear, majorOrder);
        }
        internal static T[] Raycast<T>(T[,] map, T startTile, Vector2Int endPosition, float maxDistance, bool includeStart, bool includeDestination, bool breakOnWalls, bool includeWalls, out bool isLineClear, MajorOrder majorOrder) where T : ITile
        {
            if (Mathf.Approximately(maxDistance, 0f))
            {
                maxDistance = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            }
            Vector2Int p0 = new Vector2Int(startTile.X, startTile.Y);
            Vector2Int p1 = endPosition;
            int dx = p1.x - p0.x, dy = p1.y - p0.y;
            int nx = Mathf.Abs(dx), ny = Mathf.Abs(dy);
            int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

            Vector2Int p = new Vector2Int(p0.x, p0.y);
            List<T> points = new List<T>();
            if (includeStart)
            {
                points.Add(GridUtils.GetTile(map, p.x, p.y, majorOrder));
            }
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
                breakIt = breakIt ? true : p.x < 0 || p.x >= GridUtils.GetXLength(map, majorOrder) || p.y < 0 || p.y >= GridUtils.GetYLength(map, majorOrder);
                T tile = breakIt ? default : GridUtils.GetTile(map, p.x, p.y, majorOrder);
                breakIt = breakIt ? true : breakOnWalls && (tile == null || !tile.IsWalkable);
                breakIt = breakIt ? true : !includeDestination && new Vector2Int(p.x, p.y) == p1;
                breakIt = breakIt ? true : (Vector2Int.Distance(new Vector2Int(p.x, p.y), new Vector2Int(startTile.X, startTile.Y)) > maxDistance);
                isLineClear = !breakIt;
                bool continueIt = breakIt ? true : false;
                continueIt = continueIt ? true : tile == null;
                continueIt = continueIt ? true : !includeWalls && !tile.IsWalkable;
                if (breakIt)
                {
                    break;
                }
                if (continueIt)
                {
                    continue;
                }
                points.Add(GridUtils.GetTile(map, p.x, p.y, majorOrder));
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
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, false, true, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, float directionAngle, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, false, true, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, Vector2 direction, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, false, true, out bool isclear, majorOrder);
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
        public static T[] GetWalkableTilesOnALine<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, false, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all walkable tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnALine<T>(T[,] map, T startTile, float directionAngle, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, false, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all walkable tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnALine<T>(T[,] map, T startTile, Vector2 direction, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, false, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
            return isclear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, float directionAngle, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
            return isclear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, Vector2 direction, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
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
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, float directionAngle, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, Vector2 direction, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
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
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, T destinationTile, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out isLineClear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, float directionAngle, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, true, false, out isLineClear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, Vector2 direction, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            return Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, true, false, out isLineClear, majorOrder);
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
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, float openingAngle, T destinationTile, float maxDistance = 0f, bool includeStart = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            Vector2Int endPos = new Vector2Int(destinationTile.X, destinationTile.Y);
            maxDistance = Mathf.Max(maxDistance, Vector2Int.Distance(new Vector2Int(startTile.X, startTile.Y), endPos));
            return GetConeOfVision(map, startTile, openingAngle, endPos - new Vector2(startTile.X, startTile.Y), maxDistance, includeStart, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="openingAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, float openingAngle, float directionAngle, float maxDistance = 0f, bool includeStart = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            Vector2Int endPos = new Vector2Int(Mathf.RoundToInt(startTile.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * maxDistance), Mathf.RoundToInt(startTile.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * maxDistance));
            return GetConeOfVision(map, startTile, openingAngle, endPos - new Vector2(startTile.X, startTile.Y), maxDistance, includeStart, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the cone from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, float openingAngle, Vector2 direction, float maxDistance = 0f, bool includeStart = true, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            if (Mathf.Approximately(maxDistance, 0f))
            {
                maxDistance = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            }
            direction.Normalize();
            Vector2 o = new Vector2(startTile.X, startTile.Y);
            List<T> returnedArray = new List<T>();
            int bottom = Mathf.RoundToInt(startTile.Y - maxDistance);
            int top = Mathf.RoundToInt(startTile.Y + maxDistance + 1);
            List<Vector2Int> list = new List<Vector2Int>();
            for (int y = bottom; y < top; y++)
            {
                for (int r = 0; r <= Mathf.FloorToInt(maxDistance * Mathf.Sqrt(0.5f)); r++)
                {
                    int dd = Mathf.FloorToInt(Mathf.Sqrt(maxDistance * maxDistance - r * r));
                    Vector2Int a = new Vector2Int(startTile.X - dd, startTile.Y + r);
                    if (Extraction.IsIntoAngle(startTile.X, startTile.Y, a.x, a.y, openingAngle, direction) && !list.Contains(a))
                    {
                        list.Add(a);
                        T[] line = Raycast(map, startTile, a - o, maxDistance, includeStart, true, true, false, out bool isLineClear, majorOrder);
                        foreach (T lineTile in line)
                        {
                            if (!returnedArray.Contains(lineTile))
                            {
                                returnedArray.Add(lineTile);
                            }
                        }
                    }
                    a = new Vector2Int(startTile.X + dd, startTile.Y + r);
                    if (Extraction.IsIntoAngle(startTile.X, startTile.Y, a.x, a.y, openingAngle, direction) && !list.Contains(a))
                    {
                        list.Add(a);
                        T[] line = Raycast(map, startTile, a - o, maxDistance, includeStart, true, true, false, out bool isLineClear, majorOrder);
                        foreach (T lineTile in line)
                        {
                            if (!returnedArray.Contains(lineTile))
                            {
                                returnedArray.Add(lineTile);
                            }
                        }
                    }
                    a = new Vector2Int(startTile.X - dd, startTile.Y - r);
                    if (Extraction.IsIntoAngle(startTile.X, startTile.Y, a.x, a.y, openingAngle, direction) && !list.Contains(a))
                    {
                        list.Add(a);
                        T[] line = Raycast(map, startTile, a - o, maxDistance, includeStart, true, true, false, out bool isLineClear, majorOrder);
                        foreach (T lineTile in line)
                        {
                            if (!returnedArray.Contains(lineTile))
                            {
                                returnedArray.Add(lineTile);
                            }
                        }
                    }
                    a = new Vector2Int(startTile.X + dd, startTile.Y - r);
                    if (Extraction.IsIntoAngle(startTile.X, startTile.Y, a.x, a.y, openingAngle, direction) && !list.Contains(a))
                    {
                        list.Add(a);
                        T[] line = Raycast(map, startTile, a - o, maxDistance, includeStart, true, true, false, out bool isLineClear, majorOrder);
                        foreach (T lineTile in line)
                        {
                            if (!returnedArray.Contains(lineTile))
                            {
                                returnedArray.Add(lineTile);
                            }
                        }
                    }
                    a = new Vector2Int(startTile.X + r, startTile.Y - dd);
                    if (Extraction.IsIntoAngle(startTile.X, startTile.Y, a.x, a.y, openingAngle, direction) && !list.Contains(a))
                    {
                        list.Add(a);
                        T[] line = Raycast(map, startTile, a - o, maxDistance, includeStart, true, true, false, out bool isLineClear, majorOrder);
                        foreach (T lineTile in line)
                        {
                            if (!returnedArray.Contains(lineTile))
                            {
                                returnedArray.Add(lineTile);
                            }
                        }
                    }
                    a = new Vector2Int(startTile.X + r, startTile.Y + dd);
                    if (Extraction.IsIntoAngle(startTile.X, startTile.Y, a.x, a.y, openingAngle, direction) && !list.Contains(a))
                    {
                        list.Add(a);
                        T[] line = Raycast(map, startTile, a - o, maxDistance, includeStart, true, true, false, out bool isLineClear, majorOrder);
                        foreach (T lineTile in line)
                        {
                            if (!returnedArray.Contains(lineTile))
                            {
                                returnedArray.Add(lineTile);
                            }
                        }
                    }
                    a = new Vector2Int(startTile.X - r, startTile.Y - dd);
                    if (Extraction.IsIntoAngle(startTile.X, startTile.Y, a.x, a.y, openingAngle, direction) && !list.Contains(a))
                    {
                        list.Add(a);
                        T[] line = Raycast(map, startTile, a - o, maxDistance, includeStart, true, true, false, out bool isLineClear, majorOrder);
                        foreach (T lineTile in line)
                        {
                            if (!returnedArray.Contains(lineTile))
                            {
                                returnedArray.Add(lineTile);
                            }
                        }
                    }
                    a = new Vector2Int(startTile.X - r, startTile.Y + dd);
                    if (Extraction.IsIntoAngle(startTile.X, startTile.Y, a.x, a.y, openingAngle, direction) && !list.Contains(a))
                    {
                        list.Add(a);
                        T[] line = Raycast(map, startTile, a - o, maxDistance, includeStart, true, true, false, out bool isLineClear, majorOrder);
                        foreach (T lineTile in line)
                        {
                            if (!returnedArray.Contains(lineTile))
                            {
                                returnedArray.Add(lineTile);
                            }
                        }
                    }
                }
            }
            return returnedArray.ToArray();
        }
    }
    /// <summary>
    /// Allows you to calculate paths on a grid
    /// </summary>
    public class Pathfinding
    {
        private static bool GetTile<T>(T[,] map, int x, int y, out T tile, MajorOrder majorOrder) where T : ITile
        {
            if (x > -1 && y > -1 && x < GridUtils.GetXLength(map, majorOrder) && y < GridUtils.GetYLength(map, majorOrder))
            {
                tile = GridUtils.GetTile(map, x, y, majorOrder);
                return true;
            }
            tile = default;
            return false;
        }
        private static bool GetLeftNeighbour<T>(T[,] map, int x, int y, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile
        {
            if (GetTile(map, x - 1, y, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightNeighbour<T>(T[,] map, int x, int y, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile
        {
            if (GetTile(map, x + 1, y, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetBottomNeighbour<T>(T[,] map, int x, int y, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile
        {
            if (GetTile(map, x, y - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetTopNeighbour<T>(T[,] map, int x, int y, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile
        {
            if (GetTile(map, x, y + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftBottomNeighbour<T>(T[,] map, int x, int y, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile
        {
            if (GetTile(map, x - 1, y - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetLeftTopNeighbour<T>(T[,] map, int x, int y, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile
        {
            if (GetTile(map, x - 1, y + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightBottomNeighbour<T>(T[,] map, int x, int y, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile
        {
            if (GetTile(map, x + 1, y - 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool GetRightTopNeighbour<T>(T[,] map, int x, int y, out T nei, bool lookForWall, MajorOrder majorOrder) where T : ITile
        {
            if (GetTile(map, x + 1, y + 1, out nei, majorOrder))
            {
                if (nei != null && (lookForWall != nei.IsWalkable))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool CheckDP(DiagonalsPolicy policy, bool valueA, bool valueB)
        {
            switch (policy)
            {
                case DiagonalsPolicy.NONE:
                    return false;
                case DiagonalsPolicy.DIAGONAL_2FREE:
                    return valueA && valueB;
                case DiagonalsPolicy.DIAGONAL_1FREE:
                    return valueA || valueB;
                case DiagonalsPolicy.ALL_DIAGONALS:
                    return true;
                default:
                    return false;
            }
        }
        private static bool CheckMP<T>(MovementPolicy policy, T[,] map, T tile, MajorOrder majorOrder) where T : ITile
        {
            int polCase = (int)policy;
            if (polCase == 0)
            {
                return true;
            }
            else if (polCase == 1)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, out T nei, true, majorOrder);
            }
            else if (polCase == 2)
            {
                return GetLeftNeighbour(map, tile.X, tile.Y, out T neiL, true, majorOrder) || GetRightNeighbour(map, tile.X, tile.Y, out T neiR, true, majorOrder);
            }
            else if (polCase == 3)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, out T nei, true, majorOrder) || GetLeftNeighbour(map, tile.X, tile.Y, out T neiL, true, majorOrder) || GetRightNeighbour(map, tile.X, tile.Y, out T neiR, true, majorOrder);
            }
            else if (polCase == 4)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, out T nei, true, majorOrder);
            }
            else if (polCase == 5)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, out T nei, true, majorOrder) || GetBottomNeighbour(map, tile.X, tile.Y, out T neiB, true, majorOrder);
            }
            else if (polCase == 6)
            {
                return GetTopNeighbour(map, tile.X, tile.Y, out T nei, true, majorOrder) || GetLeftNeighbour(map, tile.X, tile.Y, out T neiL, true, majorOrder) || GetRightNeighbour(map, tile.X, tile.Y, out T neiR, true, majorOrder);
            }
            else if (polCase == 7)
            {
                return GetBottomNeighbour(map, tile.X, tile.Y, out T nei, true, majorOrder) || GetTopNeighbour(map, tile.X, tile.Y, out T neiT, true, majorOrder) || GetLeftNeighbour(map, tile.X, tile.Y, out T neiL, true, majorOrder) || GetRightNeighbour(map, tile.X, tile.Y, out T neiR, true, majorOrder);
            }
            return true;
        }
        private static List<T> GetTileNeighbours<T>(T[,] map, int x, int y, PathfindingPolicy pathfindingPolicy, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
        {
            List<T> nodes = new List<T>();
            T nei;

            bool leftWalkable = GetLeftNeighbour(map, x, y, out nei, false, majorOrder);
            if (leftWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool rightWalkable = GetRightNeighbour(map, x, y, out nei, false, majorOrder);
            if (rightWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool bottomWalkable = GetBottomNeighbour(map, x, y, out nei, false, majorOrder);
            if (bottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool topWalkable = GetTopNeighbour(map, x, y, out nei, false, majorOrder);
            if (topWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }

            bool leftBottomWalkable = GetLeftBottomNeighbour(map, x, y, out nei, false, majorOrder);
            if (CheckDP(pathfindingPolicy.DiagonalsPolicy, leftWalkable, bottomWalkable) && leftBottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool rightBottomWalkable = GetRightBottomNeighbour(map, x, y, out nei, false, majorOrder);
            if (CheckDP(pathfindingPolicy.DiagonalsPolicy, rightWalkable, bottomWalkable) && rightBottomWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool leftTopWalkable = GetLeftTopNeighbour(map, x, y, out nei, false, majorOrder);
            if (CheckDP(pathfindingPolicy.DiagonalsPolicy, leftWalkable, topWalkable) && leftTopWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool rightTopWalkable = GetRightTopNeighbour(map, x, y, out nei, false, majorOrder);
            if (CheckDP(pathfindingPolicy.DiagonalsPolicy, rightWalkable, topWalkable) && rightTopWalkable && CheckMP(pathfindingPolicy.MovementPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }

            return nodes;
        }

        /// <summary>
        /// Generates a PathMap object that will contain all the pre-calculated paths data between a target tile and all the accessible tiles from this target
        /// </summary>
        /// <typeparam name="T">The user-defined tile type that implements the ITile interface</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="targetTile">The target tile for the paths calculation</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The MajorOrder to use</param>
        /// <returns></returns>
        public static PathMap<T> GeneratePathMap<T>(T[,] map, T targetTile, float maxDistance = 0f, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.ROW_MAJOR_ORDER) where T : ITile
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
            while (frontier.Count > 0)
            {
                Node<T> current = frontier.Dequeue();
                List<T> neighbourgs = GetTileNeighbours(map, current.Tile.X, current.Tile.Y, pathfindingPolicy, majorOrder);
                foreach (T neiTile in neighbourgs)
                {
                    Node<T> nei = accessibleTilesDico.ContainsKey(neiTile) ? accessibleTilesDico[neiTile] : new Node<T>(neiTile);
                    bool isDiagonal = current.Tile.X != nei.Tile.X && current.Tile.Y != nei.Tile.Y;
                    float newDistance = current.DistanceToTarget + nei.Tile.Weight * (isDiagonal ? pathfindingPolicy.DiagonalsWeight : 1f);
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
            }
            return new PathMap<T>(accessibleTilesDico, accessibleTiles, targetTile, maxDistance, majorOrder);
        }
    }
    internal static class GridUtils
    {
        internal static bool AreCoordsIntoGrid<T>(T[,] map, int x, int y, MajorOrder majorOrder) where T : ITile
        {
            return x - 1 >= 0 && x + 1 < GetXLength(map, majorOrder) && y - 1 >= 0 && y + 1 < GetYLength(map, majorOrder);
        }
        internal static T GetTile<T>(T[,] map, int x, int y, MajorOrder majorOrder) where T : ITile
        {
            if (majorOrder == MajorOrder.ROW_MAJOR_ORDER)
            {
                return map[y, x];
            }
            else
            {
                return map[x, y];
            }
        }
        internal static int GetXLength<T>(T[,] map, MajorOrder majorOrder) where T : ITile
        {
            if (majorOrder == MajorOrder.ROW_MAJOR_ORDER)
            {
                return map.GetLength(1);
            }
            else
            {
                return map.GetLength(0);
            }
        }
        internal static int GetYLength<T>(T[,] map, MajorOrder majorOrder) where T : ITile
        {
            if (majorOrder == MajorOrder.ROW_MAJOR_ORDER)
            {
                return map.GetLength(0);
            }
            else
            {
                return map.GetLength(1);
            }
        }
    }
}
