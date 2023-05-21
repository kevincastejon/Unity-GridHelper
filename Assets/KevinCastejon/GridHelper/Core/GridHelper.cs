using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Helper static classes for 2D grid operations
/// </summary>
namespace KevinCastejon.GridHelper
{
    public enum DefaultMajorOrder
    {
        ROW_MAJOR_ORDER,
        COLUMN_MAJOR_ORDER
    }
    /// <summary>
    /// Major order rule
    /// DEFAULT : The GridGlobalSettings.DefaultMajorOrder value
    /// ROW_MAJOR_ORDER : YX
    /// COLUMN_MAJOR_ORDER : XY
    /// </summary>
    public enum MajorOrder
    {
        DEFAULT,
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

    public static class GridGlobalSettings
    {
        private static DefaultMajorOrder _defaultMajorOrder = DefaultMajorOrder.ROW_MAJOR_ORDER;

        public static DefaultMajorOrder DefaultMajorOrder { get => _defaultMajorOrder; set => _defaultMajorOrder = value; }
    }
    /// <summary>
    /// Allows you to extract tiles on a grid
    /// </summary>
    public class Extraction
    {
        private static T[] ExtractRectangle<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeCenter, bool includeWalls, MajorOrder majorOrder) where T : ITile
        {
            Vector2Int min = GridUtils.ClampCoordsIntoGrid(map, center.X + -rectangleSize.x, center.Y - rectangleSize.y, majorOrder);
            Vector2Int max = GridUtils.ClampCoordsIntoGrid(map, center.X + rectangleSize.x, center.Y + rectangleSize.y, majorOrder);
            List<T> list = new();
            for (int i = min.y; i <= max.y; i++)
            {
                for (int j = min.x; j <= max.x; j++)
                {
                    T tile = GridUtils.GetTile(map, j, i, majorOrder);
                    if (tile != null && (includeWalls || tile.IsWalkable) && (includeCenter || !GridUtils.TileEquals(tile, center)))
                    {
                        list.Add(tile);
                    }
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractRectangleOutline<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeWalls, MajorOrder majorOrder) where T : ITile
        {
            Vector2Int min = GridUtils.ClampCoordsIntoGrid(map, center.X + -rectangleSize.x, center.Y - rectangleSize.y, majorOrder);
            Vector2Int max = GridUtils.ClampCoordsIntoGrid(map, center.X + rectangleSize.x, center.Y + rectangleSize.y, majorOrder);
            List<T> list = new();
            for (int j = min.x; j <= max.x; j++)
            {
                T tile = GridUtils.GetTile(map, j, min.y, majorOrder);
                if (tile != null && (includeWalls || tile.IsWalkable))
                {
                    list.Add(tile);
                }
                tile = GridUtils.GetTile(map, j, max.y, majorOrder);
                if (tile != null && (includeWalls || tile.IsWalkable))
                {
                    list.Add(tile);
                }
            }
            for (int i = min.y + 1; i <= max.y - 1; i++)
            {
                T tile = GridUtils.GetTile(map, min.x, i, majorOrder);
                if (tile != null && (includeWalls || tile.IsWalkable))
                {
                    list.Add(tile);
                }
                tile = GridUtils.GetTile(map, max.x, i, majorOrder);
                if (tile != null && (includeWalls || tile.IsWalkable))
                {
                    list.Add(tile);
                }
            }
            return list.ToArray();
        }
        private static T[] ExtractCircleArcFilled<T>(T[,] map, T center, int radius, bool includeCenter, bool includeWalls, MajorOrder majorOrder, float openingAngle, Vector2 direction) where T : ITile
        {
            int x = 0;
            int y = -radius;
            int F_M = 1 - radius;
            int d_e = 3;
            int d_ne = -(radius << 1) + 5;
            List<T> points = new List<T>(GetLineMirrors(map, center, x, y, openingAngle, direction, includeCenter, includeWalls, majorOrder));
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
                points = points.Concat(GetLineMirrors(map, center, x, y, openingAngle, direction, includeCenter, includeWalls, majorOrder)).ToList();
            }
            return points.ToArray();
        }
        private static T[] ExtractCircleArcOutline<T>(T[,] map, T center, int radius, bool includeWalls, MajorOrder majorOrder, float openingAngle, Vector2 direction) where T : ITile
        {
            int x = 0;
            int y = -radius;
            int F_M = 1 - radius;
            int d_e = 3;
            int d_ne = -(radius << 1) + 5;
            List<T> points = new List<T>(GetTileMirrors(map, center, x, y, openingAngle, direction, includeWalls, majorOrder));
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
                points = points.Concat(GetTileMirrors(map, center, x, y, openingAngle, direction, includeWalls, majorOrder)).ToList();
            }
            return points.ToArray();
        }
        private static T[] GetTileMirrors<T>(T[,] map, T centerTile, int x, int y, float openingAngle, Vector2 direction, bool includeWalls, MajorOrder majorOrder) where T : ITile
        {
            List<T> neis = new List<T>();
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X + x, centerTile.Y + y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X + x, centerTile.Y + y, majorOrder);
                if (includeWalls || nei.IsWalkable && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X - x, centerTile.Y + y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X - x, centerTile.Y + y, majorOrder);
                if (includeWalls || nei.IsWalkable && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X + x, centerTile.Y - y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X + x, centerTile.Y - y, majorOrder);
                if (includeWalls || nei.IsWalkable && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X - x, centerTile.Y - y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X - x, centerTile.Y - y, majorOrder);
                if (includeWalls || nei.IsWalkable && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X + y, centerTile.Y + x, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X + y, centerTile.Y + x, majorOrder);
                if (includeWalls || nei.IsWalkable && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X - y, centerTile.Y + x, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X - y, centerTile.Y + x, majorOrder);
                if (includeWalls || nei.IsWalkable && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X + y, centerTile.Y - x, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X + y, centerTile.Y - x, majorOrder);
                if (includeWalls || nei.IsWalkable && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X - y, centerTile.Y - x, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X - y, centerTile.Y - x, majorOrder);
                if (includeWalls || nei.IsWalkable && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            return neis.ToArray();
        }
        private static T[] GetLineMirrors<T>(T[,] map, T centerTile, int x, int y, float openingAngle, Vector2 direction, bool includeCenter, bool includeWalls, MajorOrder majorOrder) where T : ITile
        {
            List<T> neis = new List<T>();
            Vector2Int posLeft = GridUtils.ClampCoordsIntoGrid(map, x >= 0 ? centerTile.X - x : centerTile.X + x, centerTile.Y + y, majorOrder);
            Vector2Int posRight = GridUtils.ClampCoordsIntoGrid(map, x >= 0 ? centerTile.X + x : centerTile.X - x, centerTile.Y + y, majorOrder);
            for (int i = posLeft.x; i <= posRight.x; i++)
            {
                T nei = GridUtils.GetTile(map, i, posLeft.y, majorOrder);
                if ((includeWalls || nei.IsWalkable) && (includeCenter || !GridUtils.TileEquals(nei, centerTile)) && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            posLeft = GridUtils.ClampCoordsIntoGrid(map, x >= 0 ? centerTile.X - x : centerTile.X + x, centerTile.Y - y, majorOrder);
            posRight = GridUtils.ClampCoordsIntoGrid(map, x >= 0 ? centerTile.X + x : centerTile.X - x, centerTile.Y - y, majorOrder);
            for (int i = posLeft.x; i <= posRight.x; i++)
            {
                T nei = GridUtils.GetTile(map, i, posLeft.y, majorOrder);
                if ((includeWalls || nei.IsWalkable) && (includeCenter || !GridUtils.TileEquals(nei, centerTile)) && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            posLeft = GridUtils.ClampCoordsIntoGrid(map, y >= 0 ? centerTile.X - y : centerTile.X + y, centerTile.Y + x, majorOrder);
            posRight = GridUtils.ClampCoordsIntoGrid(map, y >= 0 ? centerTile.X + y : centerTile.X - y, centerTile.Y + x, majorOrder);
            for (int i = posLeft.x; i <= posRight.x; i++)
            {
                T nei = GridUtils.GetTile(map, i, posLeft.y, majorOrder);
                if ((includeWalls || nei.IsWalkable) && (includeCenter || !GridUtils.TileEquals(nei, centerTile)) && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            posLeft = GridUtils.ClampCoordsIntoGrid(map, y >= 0 ? centerTile.X - y : centerTile.X + y, centerTile.Y - x, majorOrder);
            posRight = GridUtils.ClampCoordsIntoGrid(map, y >= 0 ? centerTile.X + y : centerTile.X - y, centerTile.Y - x, majorOrder);
            for (int i = posLeft.x; i <= posRight.x; i++)
            {
                T nei = GridUtils.GetTile(map, i, posLeft.y, majorOrder);
                if ((includeWalls || nei.IsWalkable) && (includeCenter || !GridUtils.TileEquals(nei, centerTile)) && IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction))
                {
                    neis.Add(nei);
                }
            }
            return neis.ToArray();
        }
        private static bool IsInARectangle<T>(T[,] map, T tile, T center, Vector2Int rectangleSize, MajorOrder majorOrder) where T : ITile
        {
            Vector2Int min = GridUtils.ClampCoordsIntoGrid(map, center.X + -rectangleSize.x, center.Y - rectangleSize.y, majorOrder);
            Vector2Int max = GridUtils.ClampCoordsIntoGrid(map, center.X + rectangleSize.x, center.Y + rectangleSize.y, majorOrder);
            return tile.X >= min.x && tile.X <= max.x && tile.Y >= min.y && tile.Y <= max.y;
        }
        private static bool IsOnRectangleOutline<T>(T[,] map, T tile, T center, Vector2Int rectangleSize, MajorOrder majorOrder) where T : ITile
        {
            Vector2Int min = GridUtils.ClampCoordsIntoGrid(map, center.X + -rectangleSize.x, center.Y - rectangleSize.y, majorOrder);
            Vector2Int max = GridUtils.ClampCoordsIntoGrid(map, center.X + rectangleSize.x, center.Y + rectangleSize.y, majorOrder);
            return (tile.X == min.x && tile.Y <= max.y && tile.Y >= min.y) || (tile.X == max.x && tile.Y <= max.y && tile.Y >= min.y) || (tile.Y == min.y && tile.X <= max.x && tile.X >= min.x) || (tile.Y == max.y && tile.X <= max.x && tile.X >= min.x);
        }
        private static bool IsOnCircleArcFilled<T>(T[,] map, T tile, T center, int radius, float openingAngle, Vector2 direction, MajorOrder majorOrder) where T : ITile
        {
            int x = 0;
            int y = -radius;
            int F_M = 1 - radius;
            int d_e = 3;
            int d_ne = -(radius << 1) + 5;
            if (IsOnLineMirrors(map, tile, center, x, y, openingAngle, direction, majorOrder))
            {
                return true;
            }
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
                if (IsOnLineMirrors(map, tile, center, x, y, openingAngle, direction, majorOrder))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsOnCircleArcOutline<T>(T[,] map, T tile, T center, int radius, float openingAngle, Vector2 direction, MajorOrder majorOrder) where T : ITile
        {
            int x = 0;
            int y = -radius;
            int F_M = 1 - radius;
            int d_e = 3;
            int d_ne = -(radius << 1) + 5;
            if (IsOneOfTileMirrors(map, tile, center, x, y, openingAngle, direction, majorOrder))
            {
                return true;
            }
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
                if (IsOneOfTileMirrors(map, tile, center, x, y, openingAngle, direction, majorOrder))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsOneOfTileMirrors<T>(T[,] map, T tile, T centerTile, int x, int y, float openingAngle, Vector2 direction, MajorOrder majorOrder) where T : ITile
        {
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X + x, centerTile.Y + y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X + x, centerTile.Y + y, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X - x, centerTile.Y + y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X - x, centerTile.Y + y, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X + x, centerTile.Y - y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X + x, centerTile.Y - y, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X - x, centerTile.Y - y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X - x, centerTile.Y - y, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X + y, centerTile.Y + x, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X + y, centerTile.Y + x, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X - y, centerTile.Y + x, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X - y, centerTile.Y + x, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X + y, centerTile.Y - x, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X + y, centerTile.Y - x, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            if (GridUtils.AreCoordsIntoGrid(map, centerTile.X - y, centerTile.Y - x, majorOrder))
            {
                T nei = GridUtils.GetTile(map, centerTile.X - y, centerTile.Y - x, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsOnLineMirrors<T>(T[,] map, T tile, T centerTile, int x, int y, float openingAngle, Vector2 direction, MajorOrder majorOrder) where T : ITile
        {
            Vector2Int posLeft = GridUtils.ClampCoordsIntoGrid(map, x >= 0 ? centerTile.X - x : centerTile.X + x, centerTile.Y + y, majorOrder);
            Vector2Int posRight = GridUtils.ClampCoordsIntoGrid(map, x >= 0 ? centerTile.X + x : centerTile.X - x, centerTile.Y + y, majorOrder);
            for (int i = posLeft.x; i <= posRight.x; i++)
            {
                T nei = GridUtils.GetTile(map, i, posLeft.y, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            posLeft = GridUtils.ClampCoordsIntoGrid(map, x >= 0 ? centerTile.X - x : centerTile.X + x, centerTile.Y - y, majorOrder);
            posRight = GridUtils.ClampCoordsIntoGrid(map, x >= 0 ? centerTile.X + x : centerTile.X - x, centerTile.Y - y, majorOrder);
            for (int i = posLeft.x; i <= posRight.x; i++)
            {
                T nei = GridUtils.GetTile(map, i, posLeft.y, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            posLeft = GridUtils.ClampCoordsIntoGrid(map, y >= 0 ? centerTile.X - y : centerTile.X + y, centerTile.Y + x, majorOrder);
            posRight = GridUtils.ClampCoordsIntoGrid(map, y >= 0 ? centerTile.X + y : centerTile.X - y, centerTile.Y + x, majorOrder);
            for (int i = posLeft.x; i <= posRight.x; i++)
            {
                T nei = GridUtils.GetTile(map, i, posLeft.y, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            posLeft = GridUtils.ClampCoordsIntoGrid(map, y >= 0 ? centerTile.X - y : centerTile.X + y, centerTile.Y - x, majorOrder);
            posRight = GridUtils.ClampCoordsIntoGrid(map, y >= 0 ? centerTile.X + y : centerTile.X - y, centerTile.Y - x, majorOrder);
            for (int i = posLeft.x; i <= posRight.x; i++)
            {
                T nei = GridUtils.GetTile(map, i, posLeft.y, majorOrder);
                if (IsIntoAngle(centerTile.X, centerTile.Y, nei.X, nei.Y, openingAngle, direction) && GridUtils.TileEquals(nei, tile))
                {
                    return true;
                }
            }
            return false;
        }
        internal static bool IsIntoAngle(int tileAX, int tileAY, int tileBX, int tileBY, float openingAngle, Vector2 direction)
        {
            Vector2 realDirection = (new Vector2(tileBX, tileBY) - new Vector2(tileAX, tileAY)).normalized;
            float angleDiff = Vector2.Angle(realDirection, direction.normalized);
            return angleDiff <= openingAngle / 2;
        }

        /// <summary>
        /// Get tiles in a rectangle around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInARectangle<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeCenter = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return ExtractRectangle(map, center, rectangleSize, includeCenter, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get tiles on a rectangle outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnARectangleOutline<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return ExtractRectangleOutline(map, center, rectangleSize, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get tiles in a circle around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The circle radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACircle<T>(T[,] map, T center, int radius, bool includeCenter = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return ExtractCircleArcFilled(map, center, radius, includeCenter, includeWalls, majorOrder, 360f, Vector2.right);
        }
        /// <summary>
        /// Get tiles on a circle outline around a tile
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The circle radius</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnACircleOutline<T>(T[,] map, T center, int radius, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return ExtractCircleArcOutline(map, center, radius, includeWalls, majorOrder, 360f, Vector2.right);
        }
        /// <summary>
        /// Get tiles in a cone starting from a tile
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="start">The start tile</param>
        /// <param name="length">The cone length</param>
        /// <param name="openingAngle">The cone opening angle in degrees [0-360]</param>
        /// <param name="directionAngle">The cone direction angle in degrees  [0-360]. Default is 0, which represents a direction pointing to the right in 2D coordinates</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACone<T>(T[,] map, T start, int length, float openingAngle, float directionAngle = 0f, bool includeStart = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            directionAngle = Mathf.Clamp(directionAngle, 0f, 360f);
            return GetTilesInACone(map, start, length, openingAngle, Quaternion.AngleAxis(directionAngle, Vector3.back) * Vector2.right, includeStart, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get tiles in a cone starting from a tile
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="start">The start tile</param>
        /// <param name="length">The cone length</param>
        /// <param name="openingAngle">The cone opening angle in degrees [0-360]</param>
        /// <param name="direction">The Vector2 representing the cone direction. Note that an 'empty' Vector2 (Vector2.zero) will be treated as Vector2.right which is the default value</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACone<T>(T[,] map, T start, int length, float openingAngle, Vector2 direction = new Vector2(), bool includeStart = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 0f, 360f);
            direction = direction == Vector2.zero ? Vector2.right : direction;
            return ExtractCircleArcFilled(map, start, length, includeStart, includeWalls, majorOrder, openingAngle, direction);
        }

        /// <summary>
        /// Get neighbour of a tile if it exists
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="neighbourDirectionAngle">The cone direction angle in degrees  [0-360]. Default is 0, which represents a direction pointing to the right in 2D coordinates</param>
        /// <param name="neighbour">The neighbour of a tile</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>Returns true if the neighbour exists, false otherwise</returns>
        public static bool GetTileNeighbour<T>(T[,] map, T tile, float neighbourDirectionAngle, out T neighbour, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return GetTileNeighbour(map, tile, Vector2Int.RoundToInt(Quaternion.AngleAxis(neighbourDirectionAngle, Vector3.back) * Vector2.right), out neighbour, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get neighbour of a tile if it exists
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="neighbourDirection">The direction from the tile to the desired neighbour</param>
        /// <param name="neighbour">The neighbour of a tile</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>Returns true if the neighbour exists, false otherwise</returns>
        public static bool GetTileNeighbour<T>(T[,] map, T tile, Vector2Int neighbourDirection, out T neighbour, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            int x = neighbourDirection.x > 0 ? tile.X + 1 : (neighbourDirection.x < 0 ? tile.X - 1 : tile.X);
            int y = neighbourDirection.y > 0 ? tile.Y + 1 : (neighbourDirection.y < 0 ? tile.Y - 1 : tile.Y);

            if (GridUtils.AreCoordsIntoGrid(map, x, y, majorOrder))
            {
                T nei = GridUtils.GetTile(map, x, y, majorOrder);
                if (includeWalls || nei.IsWalkable)
                {
                    neighbour = nei;
                    return true;
                }
            }
            neighbour = default;
            return false;
        }
        /// <summary>
        /// Get the eight neighbours of a tile when they exist
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTileNeighbours<T>(T[,] map, T tile, bool includeWalls, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
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
                            neis.Add(nei);
                        }
                    }
                }
            }
            return neis.ToArray();
        }
        /// <summary>
        /// Get the four orthogonals neighbours of a tile when they exist
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTileOrthogonalsNeighbours<T>(T[,] map, T tile, bool includeWalls, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
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
        /// Get the four diagonals neighbours of a tile when they exist
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTileDiagonalsNeighbours<T>(T[,] map, T tile, bool includeWalls, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
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
        /// Is this tile in a rectangle or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="rectangleSize">The Vector2Int representing the rectangle size</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileInARectangle<T>(T[,] map, T tile, T center, Vector2Int rectangleSize, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return IsInARectangle(map, tile, center, rectangleSize, majorOrder);
        }
        /// <summary>
        /// Is this tile on a rectangle outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="rectangleSize">The Vector2Int representing the rectangle size</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOnARectangleOutline<T>(T[,] map, T tile, T center, Vector2Int rectangleSize, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return IsOnRectangleOutline(map, tile, center, rectangleSize, majorOrder);
        }
        /// <summary>
        /// Is this tile in a circle or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="radius">The circle radius</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileInACircle<T>(T[,] map, T tile, T center, int radius, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return IsOnCircleArcFilled(map, tile, center, radius, 360f, Vector2.right, majorOrder);
        }
        /// <summary>
        /// Is this tile on a circle outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="radius">The circle radius</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOnACircleOutline<T>(T[,] map, T tile, T center, int radius, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return IsOnCircleArcOutline(map, tile, center, radius, 360f, Vector2.right, majorOrder);
        }
        /// <summary>
        /// Is this tile on a circle outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="radius">The circle radius</param>
        /// <param name="openingAngle">The cone opening angle</param>
        /// <param name="directionAngle">The cone direction angle in degrees  [0-360]. Default is 0, which represents a direction pointing to the right in 2D coordinates</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileInACone<T>(T[,] map, T tile, T center, int radius, float openingAngle, float directionAngle = 0f, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            directionAngle = Mathf.Clamp(directionAngle, 0f, 360f);
            return IsOnCircleArcFilled(map, tile, center, radius, openingAngle, Quaternion.AngleAxis(directionAngle, Vector3.back) * Vector2.right, majorOrder);
        }
        /// <summary>
        /// Is this tile on a circle outline or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="radius">The circle radius</param>
        /// <param name="openingAngle">The cone opening angle</param>
        /// <param name="direction">The cone direction angle</param>
        /// <param name="majorOrder">The major order to use for the grid indexes</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileInACone<T>(T[,] map, T tile, T center, int radius, float openingAngle, Vector2 direction = new Vector2(), MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return IsOnCircleArcFilled(map, tile, center, radius, openingAngle, direction, majorOrder);
        }

        /// <summary>
        /// Is a tile the neighbour of another tile with the given direction.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tile">A tile</param>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <param name="neighbourDirectionAngle">The cone direction angle in degrees  [0-360]. Default is 0, which represents a direction pointing to the right in 2D coordinates</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileNeighbor<T>(T tile, T neighbour, float neighbourDirectionAngle) where T : ITile
        {
            return IsTileNeighbor(tile, neighbour, Vector2Int.RoundToInt(Quaternion.AngleAxis(neighbourDirectionAngle, Vector3.back) * Vector2.right));
        }
        /// <summary>
        /// Is a tile the neighbour of another tile with the given direction.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tile">A tile</param>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <param name="neighbourDirection">The direction from the tile to the expected neighbour</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileNeighbor<T>(T tile, T neighbour, Vector2Int neighbourDirection) where T : ITile
        {
            neighbourDirection.x = Mathf.Clamp(neighbourDirection.x, -1, 1);
            neighbourDirection.y = Mathf.Clamp(neighbourDirection.y, -1, 1);
            return neighbour.X == tile.X + neighbourDirection.x && neighbour.Y == tile.Y + neighbourDirection.y;
        }
        /// <summary>
        /// Is a tile an orthogonal neighbour of another tile.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tile">A tile</param>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOrthogonalNeighbor<T>(T tile, T neighbour) where T : ITile
        {
            return (tile.X == neighbour.X && (tile.Y == neighbour.Y + 1 || tile.Y == neighbour.Y - 1)) || tile.Y == neighbour.Y && (tile.X == neighbour.X + 1 || tile.X == neighbour.X - 1);
        }
        /// <summary>
        /// Is a tile an diagonal neighbour of another tile.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tile">A tile</param>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileDiagonalNeighbor<T>(T tile, T neighbour) where T : ITile
        {
            return Mathf.Abs(neighbour.X - tile.X) == 1 && Mathf.Abs(neighbour.Y - tile.Y) == 1;
        }
        /// <summary>
        /// Is a tile any neighbour of another tile.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tile">A tile</param>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileAnyNeighbor<T>(T tile, T neighbour) where T : ITile
        {
            return IsTileOrthogonalNeighbor(tile, neighbour) || IsTileDiagonalNeighbor(tile, neighbour);
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
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, false, true, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, float directionAngle, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, false, true, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, Vector2 direction, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, false, true, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all walkable tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnALine<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, false, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all walkable tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnALine<T>(T[,] map, T startTile, float directionAngle, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, false, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all walkable tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetWalkableTilesOnALine<T>(T[,] map, T startTile, Vector2 direction, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, false, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
            return isclear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, float directionAngle, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
            return isclear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <returns>An array of tiles</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, Vector2 direction, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
            return isclear;
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, T destinationTile, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, float directionAngle, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, Vector2 direction, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, true, false, out bool isclear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, T destinationTile, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, destinationTile, maxDistance, includeStart, includeDestination, true, false, out isLineClear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, float directionAngle, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, directionAngle, maxDistance, includeStart, includeDestination, true, false, out isLineClear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, Vector2 direction, out bool isLineClear, float maxDistance = 0f, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycast(map, startTile, direction, maxDistance, includeStart, includeDestination, true, false, out isLineClear, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The stop tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, float openingAngle, T destinationTile, float maxDistance = 0f, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Vector2Int endPos = new Vector2Int(destinationTile.X, destinationTile.Y);
            maxDistance = Mathf.Max(maxDistance, Vector2Int.Distance(new Vector2Int(startTile.X, startTile.Y), endPos));
            return GetConeOfVision(map, startTile, openingAngle, endPos - new Vector2(startTile.X, startTile.Y), maxDistance, includeStart, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="openingAngle">The angle of the line from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, float openingAngle, float directionAngle, float maxDistance = 0f, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Vector2Int endPos = new Vector2Int(Mathf.RoundToInt(startTile.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * maxDistance), Mathf.RoundToInt(startTile.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * maxDistance));
            return GetConeOfVision(map, startTile, openingAngle, endPos - new Vector2(startTile.X, startTile.Y), maxDistance, includeStart, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="direction">The direction of the cone from the start tile</param>
        /// <param name="maxDistance">The maximum distance from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, float openingAngle, Vector2 direction, float maxDistance = 0f, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
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
        private static List<T> GetTileNeighbours<T>(T[,] map, int x, int y, PathfindingPolicy pathfindingPolicy, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
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
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="targetTile">The target tile for the paths calculation</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The MajorOrder to use</param>
        /// <returns></returns>
        public static PathMap<T> GeneratePathMap<T>(T[,] map, T targetTile, float maxDistance = 0f, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
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
    /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
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
        /// <returns>A boolean value</returns>
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
            return _accessibleTiles.Where(t => !GridUtils.TileEquals(t, _target)).ToArray();
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
            if (!includeTarget && GridUtils.TileEquals(node.Tile, _target))
            {
                return new T[0];
            }
            List<T> tiles = new List<T>() { node.Tile };
            while (!GridUtils.TileEquals(node.Tile, _target))
            {
                node = node.NextNode;
                if (includeTarget || !GridUtils.TileEquals(node.Tile, _target))
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
    public static class GridUtils
    {
        public static bool TileEquals<T>(T tileA, T tileB) where T : ITile
        {
            return tileA.X == tileB.X && tileA.Y == tileB.Y;
        }
        public static Vector2Int ClampCoordsIntoGrid<T>(T[,] map, int x, int y, MajorOrder majorOrder) where T : ITile
        {
            ResolveMajorOrder(ref majorOrder);
            return new Vector2Int(Mathf.Clamp(x, 0, GetXLength(map, majorOrder) - 1), Mathf.Clamp(y, 0, GetYLength(map, majorOrder) - 1));
        }
        public static bool AreCoordsIntoGrid<T>(T[,] map, int x, int y, MajorOrder majorOrder) where T : ITile
        {
            ResolveMajorOrder(ref majorOrder);
            return x >= 0 && x < GetXLength(map, majorOrder) && y >= 0 && y < GetYLength(map, majorOrder);
        }
        public static T GetTile<T>(T[,] map, int x, int y, MajorOrder majorOrder) where T : ITile
        {
            ResolveMajorOrder(ref majorOrder);
            if (majorOrder == MajorOrder.ROW_MAJOR_ORDER)
            {
                return map[y, x];
            }
            else
            {
                return map[x, y];
            }
        }
        public static int GetXLength<T>(T[,] map, MajorOrder majorOrder) where T : ITile
        {
            ResolveMajorOrder(ref majorOrder);
            if (majorOrder == MajorOrder.ROW_MAJOR_ORDER)
            {
                return map.GetLength(1);
            }
            else
            {
                return map.GetLength(0);
            }
        }
        public static int GetYLength<T>(T[,] map, MajorOrder majorOrder) where T : ITile
        {
            ResolveMajorOrder(ref majorOrder);
            if (majorOrder == MajorOrder.ROW_MAJOR_ORDER)
            {
                return map.GetLength(0);
            }
            else
            {
                return map.GetLength(1);
            }
        }
        private static void ResolveMajorOrder(ref MajorOrder majorOrder)
        {
            majorOrder = majorOrder == MajorOrder.DEFAULT ? (MajorOrder)(((int)GridGlobalSettings.DefaultMajorOrder) + 1) : majorOrder;
        }
    }
}
