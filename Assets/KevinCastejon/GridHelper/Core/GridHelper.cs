using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
/// <summary>
/// Utilitary API to help with operations on 2D grids such as tile extraction, raycasting, and pathfinding.
/// </summary>
namespace KevinCastejon.GridHelper
{
    public enum DefaultMajorOrder
    {
        ROW_MAJOR_ORDER,
        COLUMN_MAJOR_ORDER
    }
    /// <summary>
    /// Major order rule. A 2D grid has two ways of storing tiles, first rows then lines or the opposite.<br/>
    /// <b>DEFAULT :</b> Refers to the global setting <b>DefaultMajorOrder</b> value<br/>
    /// <b>ROW_MAJOR_ORDER :</b> YX. First index is rows, second is columns<br/>
    /// <b>COLUMN_MAJOR_ORDER :</b> XY. First index is columns, second is rows
    /// \image html MajorOrderSchema.png height=200px
    /// \sa KevinCastejon::GridHelper::GridGlobalSettings::DefaultMajorOrder<br/>
    /// </summary>
    public enum MajorOrder
    {
        DEFAULT,
        ROW_MAJOR_ORDER,
        COLUMN_MAJOR_ORDER
    }
    /// <summary>
    /// Represents the pathfinding diagonals permissiveness.<br/>
    /// When going diagonally from a tile <b>A</b> to tile <b>B</b> in 2D grid, there are two more tile involved, the ones that are both facing neighbours of the <b>A</b> and <b>B</b> tiles. You can allow diagonals movement depending on the walkable status of these tiles.<br/>
    /// <b>NONE :</b> no diagonal movement allowed<br/>
    /// <b>DIAGONAL_2FREE :</b> only diagonal movements, with two walkable facing neighbours common to the start and destination tiles, are allowed<br/>
    /// <b>DIAGONAL_1FREE :</b> only diagonal movements, with one or more walkable facing neighbour common to the start and destination tiles, are allowed<br/>
    /// <b>ALL_DIAGONALS :</b> all diagonal movements allowed<br/>
    /// \image html DiagonalsPolicySchema.png height=200px
    /// </summary>
    public enum DiagonalsPolicy
    {
        NONE,
        DIAGONAL_2FREE,
        DIAGONAL_1FREE,
        ALL_DIAGONALS,
    }
    /// <summary>
    /// Represents the movement permissiveness.<br/>
    /// It is useful to allow special movement, especially for side-view games, such as spiders that can walk on walls or roofs, or flying characters.<br/>
    /// Top-down view grid based games should no interest into using other value than the <b>FLY</b> value as they do not hold concept of "gravity" nor "up-and-down".<br/>
    /// Note that this parameter is a flag enumeration, so you can cumulate multiple states, the <b>FLY</b> state being the most permissive and making useless its combination with any other one.<br/>
    /// <b>FLY :</b> all walkable tiles can be walk thought<br/>
    /// <b>WALL_BELOW :</b> only walkable tiles that has a not-walkable lower neighbour can be walk thought<br/>
    /// <b>WALL_ASIDE :</b> only walkable tiles that has a not-walkable side neighbour can be walk thought<br/>
    /// <b>WALL_ABOVE :</b> only walkable tiles that has a not-walkable upper neighbour can be walk thought<br/>
    /// \image html MovementsPolicySchema.png height=200px
    /// </summary>
    [System.Flags]
    public enum MovementsPolicy
    {
        FLY = 0,
        WALL_BELOW = 1,
        WALL_ASIDE = 2,
        WALL_ABOVE = 4,
    }
    /// <summary>
    /// Settings related to allowed movements.
    /// </summary>
    [System.Serializable]
    public struct PathfindingPolicy
    {
        [SerializeField] private DiagonalsPolicy _diagonalsPolicy;
        [SerializeField] private float _diagonalsWeight;
        [SerializeField] private MovementsPolicy _movementsPolicy;

        /// <summary>
        /// Settings related to allowed movements.
        /// </summary>
        /// <param name="diagonalsPolicy">The DiagonalsPolicy</param>
        /// <param name="diagonalsWeight">The diagonals weight ratio</param>
        /// <param name="movementPolicy">The MovementPolicy</param>
        public PathfindingPolicy(DiagonalsPolicy diagonalsPolicy = DiagonalsPolicy.DIAGONAL_2FREE, float diagonalsWeight = 1.4142135623730950488016887242097f, MovementsPolicy movementPolicy = MovementsPolicy.FLY)
        {
            _diagonalsPolicy = diagonalsPolicy;
            _diagonalsWeight = diagonalsWeight;
            _movementsPolicy = ((int)movementPolicy) == -1 ? (MovementsPolicy)7 : movementPolicy;
        }
        /// <summary>
        /// The DiagonalsPolicy
        /// </summary>
        public DiagonalsPolicy DiagonalsPolicy { get => _diagonalsPolicy; set => _diagonalsPolicy = value; }
        /// <summary>
        /// The diagonals weight ratio
        /// </summary>
        public float DiagonalsWeight { get => _diagonalsWeight; set => _diagonalsWeight = value; }
        /// <summary>
        /// The MovementPolicy
        /// </summary>
        public MovementsPolicy MovementsPolicy { get => _movementsPolicy; set => _movementsPolicy = value; }
    }
    /// <summary>
    /// An interface that the user-defined tile object has to implement in order to work with most of this library's methods
    /// </summary>
    public interface ITile
    {
        /// <summary>
        /// Is the tile walkable (or "transparent" for line of sight and cone of vision methods)
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
    public interface IBakedTile
    {
        public bool IsWalkable { get; set; }
        public float Weight { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2Int NextNodeCoord { get; set; }
        public Vector2Int NextDirection { get; set; }
        public float DistanceToTarget { get; set; }
    }
    public interface IBakedPathMap<TIBakedTile> where TIBakedTile : IBakedTile
    {
        public List<TIBakedTile> AccessibleTiles { get; set; }
        public TIBakedTile Target { get; set; }
        public float MaxDistance { get; set; }
        public MajorOrder MajorOrder { get; set; }
    }
    public interface IBakedPathGrid<TIBakedPathMap, TIBakedTile> where TIBakedPathMap : IBakedPathMap<TIBakedTile> where TIBakedTile : IBakedTile
    {
        public List<TIBakedPathMap> Grid { get; set; }
    }
    /// <summary>
    /// Defines globals settings of the API
    /// </summary>
    public static class GridGlobalSettings
    {
        private static DefaultMajorOrder _defaultMajorOrder = DefaultMajorOrder.ROW_MAJOR_ORDER;

        /// <summary>
        /// The major order to use when the MajorOrder.DEFAULT is passed as a parameter. Default is ROW_MAJOR_ORDER.
        /// </summary>
        public static DefaultMajorOrder DefaultMajorOrder { get => _defaultMajorOrder; set => _defaultMajorOrder = value; }
    }
    /// <summary>
    /// Allows you to extract tiles on a grid.<br>Provides shape extraction (rectangles, circles, cones and lines) and neighbors extraction with a lot of parameters.
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
        /// Get tiles in a rectangle around a center tile.<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInARectangle<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeCenter = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            rectangleSize.x = rectangleSize.x < 1 ? 1 : rectangleSize.x;
            rectangleSize.y = rectangleSize.y < 1 ? 1 : rectangleSize.y;
            return ExtractRectangle(map, center, rectangleSize, includeCenter, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get tiles on a rectangle outline around a tile.<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="rectangleSize">The Vector2Int representing rectangle size</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnARectangleOutline<T>(T[,] map, T center, Vector2Int rectangleSize, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            rectangleSize.x = rectangleSize.x < 1 ? 1 : rectangleSize.x;
            rectangleSize.y = rectangleSize.y < 1 ? 1 : rectangleSize.y;
            return ExtractRectangleOutline(map, center, rectangleSize, includeWalls, majorOrder);
        }

        /// <summary>
        /// Get tiles in a circle around a tile.<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The circle radius</param>
        /// <param name="includeCenter">Include the center tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACircle<T>(T[,] map, T center, int radius, bool includeCenter = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            radius = radius < 1 ? 1 : radius;
            return ExtractCircleArcFilled(map, center, radius, includeCenter, includeWalls, majorOrder, 360f, Vector2.right);
        }
        /// <summary>
        /// Get tiles on a circle outline around a tile.<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="center">The center tile</param>
        /// <param name="radius">The circle radius</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnACircleOutline<T>(T[,] map, T center, int radius, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            radius = radius < 1 ? 1 : radius;
            return ExtractCircleArcOutline(map, center, radius, includeWalls, majorOrder, 360f, Vector2.right);
        }

        /// <summary>
        /// Get tiles in a cone starting from a tile.<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="start">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACone<T>(T[,] map, T start, T destinationTile, float openingAngle, bool includeStart = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return GetTilesInACone(map, start, new Vector2Int(destinationTile.X, destinationTile.Y), openingAngle, includeStart, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get tiles in a cone starting from a tile.<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="start">The start tile</param>
        /// <param name="length">The cone length</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="directionAngle">The cone direction angle in degrees. 0 represents a direction pointing to the right in 2D coordinates</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACone<T>(T[,] map, T start, int length, float openingAngle, float directionAngle, bool includeStart = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return GetTilesInACone(map, start, length, openingAngle, Quaternion.AngleAxis(directionAngle, Vector3.back) * Vector2.right, includeStart, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get tiles in a cone starting from a tile.<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="start">The start tile</param>
        /// <param name="length">The cone length</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="direction">The Vector2 representing the cone direction. Note that an 'empty' Vector2 (Vector2.zero) will be treated as Vector2.right</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACone<T>(T[,] map, T start, int length, float openingAngle, Vector2 direction, bool includeStart = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            direction.Normalize();
            direction = direction == Vector2.zero ? Vector2.right : direction;
            return ExtractCircleArcFilled(map, start, length, includeStart, includeWalls, majorOrder, openingAngle, direction);
        }
        /// <summary>
        /// Get tiles in a cone starting from a tile.<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="start">The start tile</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesInACone<T>(T[,] map, T start, Vector2Int endPosition, float openingAngle, bool includeStart = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            Vector2 direction = endPosition - new Vector2(start.X, start.Y);
            return ExtractCircleArcFilled(map, start, Mathf.CeilToInt(direction.magnitude), includeStart, includeWalls, majorOrder, openingAngle, direction);
        }

        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, T destinationTile, bool allowDiagonals = true, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Vector2Int endPos = new Vector2Int(destinationTile.X, destinationTile.Y);
            return GetTilesOnALine(map, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, int length, float directionAngle, bool allowDiagonals = true, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int endPos = new Vector2Int(Mathf.RoundToInt(startTile.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * length), Mathf.RoundToInt(startTile.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * length));
            return GetTilesOnALine(map, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, int length, Vector2 direction, bool allowDiagonals = true, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int endPos = Vector2Int.RoundToInt(new Vector2(startTile.X, startTile.Y) + (direction.normalized * length));
            return GetTilesOnALine(map, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, includeWalls, majorOrder);
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetTilesOnALine<T>(T[,] map, T startTile, Vector2Int endPosition, bool allowDiagonals = true, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, bool includeWalls = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            HashSet<T> hashSet = new HashSet<T>();
            Raycasting.Raycast(map, startTile, endPosition, allowDiagonals, favorVertical, includeStart, includeDestination, false, includeWalls, out bool isClear, majorOrder, ref hashSet);
            return hashSet.ToArray();
        }

        /// <summary>
        /// Get neighbour of a tile if it exists
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="neighbourDirectionAngle">The neighbour direction angle in degrees [0-360]. 0 represents a direction pointing to the right in 2D coordinates</param>
        /// <param name="neighbour">The neighbour of a tile</param>
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
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
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
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
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
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
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
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
        /// <param name="includeWalls">Include the non-walkable tiles into the resulting array or not. Default true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
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
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
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
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
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
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
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
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOnACircleOutline<T>(T[,] map, T tile, T center, int radius, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return IsOnCircleArcOutline(map, tile, center, radius, 360f, Vector2.right, majorOrder);
        }

        /// <summary>
        /// Is this tile on a cone or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileInACone<T>(T[,] map, T tile, T center, T destinationTile, float openingAngle, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return IsTileInACone(map, tile, center, new Vector2Int(destinationTile.X, destinationTile.Y), openingAngle, majorOrder);
        }
        /// <summary>
        /// Is this tile on a cone or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileInACone<T>(T[,] map, T tile, T center, Vector2Int endPosition, float openingAngle, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            Vector2 direction = endPosition - new Vector2(center.X, center.Y);
            return IsOnCircleArcFilled(map, tile, center, Mathf.CeilToInt(direction.magnitude), openingAngle, direction, majorOrder);
        }
        /// <summary>
        /// Is this tile on a cone or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="length">The length of the cone</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="directionAngle">The cone direction angle in degrees. 0 represents a direction pointing to the right in 2D coordinates</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileInACone<T>(T[,] map, T tile, T center, int length, float openingAngle, float directionAngle, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            return IsOnCircleArcFilled(map, tile, center, length, openingAngle, Quaternion.AngleAxis(directionAngle, Vector3.back) * Vector2.right, majorOrder);
        }
        /// <summary>
        /// Is this tile on a cone or not.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="center">The center tile of the rectangle</param>
        /// <param name="length">The length of the cone</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="direction">The Vector2 representing the cone direction. Note that an 'empty' Vector2 (Vector2.zero) will be treated as Vector2.right</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileInACone<T>(T[,] map, T tile, T center, int length, float openingAngle, Vector2 direction, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            direction.Normalize();
            direction = direction == Vector2.zero ? Vector2.right : direction;
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            return IsOnCircleArcFilled(map, tile, center, length, openingAngle, direction, majorOrder);
        }

        /// <summary>
        /// Is a tile on a line
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="tile">A tile</param>
        /// <param name="start">The start tile of the line</param>
        /// <param name="destinationTile">The destination tile of the line</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOnALine<T>(T[,] map, T tile, T start, T destinationTile, bool allowDiagonals = true, bool favorVertical = false, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Vector2Int endPosition = new Vector2Int(destinationTile.X, destinationTile.Y);
            return IsTileOnALine(map, start, tile, endPosition, allowDiagonals, favorVertical, majorOrder);
        }
        /// <summary>
        /// Is a tile on a line
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tile">A tile</param>
        /// <param name="start">The start tile of the line</param>
        /// <param name="length">The length of the line</param>
        /// <param name="directionAngle">The cone direction angle in degrees. 0 represents a direction pointing to the right in 2D coordinates</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOnALine<T>(T[,] map, T tile, T start, int length, float directionAngle, bool allowDiagonals = true, bool favorVertical = false, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int endPosition = new Vector2Int(Mathf.RoundToInt(start.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * length), Mathf.RoundToInt(start.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * length));
            return IsTileOnALine(map, start, tile, endPosition, allowDiagonals, favorVertical, majorOrder);
        }
        /// <summary>
        /// Is a tile on a line
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tile">A tile</param>
        /// <param name="start">The center tile of the rectangle</param>
        /// <param name="length">The length of the line</param>
        /// <param name="direction">The Vector2 representing the cone direction. Note that an 'empty' Vector2 (Vector2.zero) will be treated as Vector2.right</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOnALine<T>(T[,] map, T tile, T start, int length, Vector2 direction, bool allowDiagonals = true, bool favorVertical = false, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int endPosition = Vector2Int.RoundToInt(new Vector2(start.X, start.Y) + (direction.normalized * length));
            return IsTileOnALine(map, start, tile, endPosition, allowDiagonals, favorVertical, majorOrder);
        }
        /// <summary>
        /// Is a tile on a line
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tile">A tile</param>
        /// <param name="start">The start tile of the line</param>
        /// <param name="endPosition">The line destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOnALine<T>(T[,] map, T start, T tile, Vector2Int endPosition, bool allowDiagonals = true, bool favorVertical = false, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return Raycasting.IsTileOnALine(map, start, tile, endPosition, allowDiagonals, favorVertical, false, majorOrder);
        }

        /// <summary>
        /// Is a tile the neighbour of another tile with the given direction.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <param name="center">A tile</param>
        /// <param name="neighbourDirectionAngle">The cone direction angle in degrees  [0-360]. 0 represents a direction pointing to the right in 2D coordinates</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileNeighbor<T>(T neighbour, T center, float neighbourDirectionAngle) where T : ITile
        {
            return IsTileNeighbor(neighbour, center, Vector2Int.RoundToInt(Quaternion.AngleAxis(neighbourDirectionAngle, Vector3.back) * Vector2.right));
        }
        /// <summary>
        /// Is a tile the neighbour of another tile with the given direction.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <param name="center">A tile</param>
        /// <param name="neighbourDirection">The position of the expected neighbour from the tile</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileNeighbor<T>(T neighbour, T center, Vector2Int neighbourDirection) where T : ITile
        {
            int x = neighbourDirection.x > 0 ? center.X + 1 : (neighbourDirection.x < 0 ? center.X - 1 : center.X);
            int y = neighbourDirection.y > 0 ? center.Y + 1 : (neighbourDirection.y < 0 ? center.Y - 1 : center.Y);
            return neighbour.X == x && neighbour.Y == y;
        }
        /// <summary>
        /// Is a tile an orthogonal neighbour of another tile.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <param name="center">A tile</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileOrthogonalNeighbor<T>(T neighbour, T center) where T : ITile
        {
            return (center.X == neighbour.X && (center.Y == neighbour.Y + 1 || center.Y == neighbour.Y - 1)) || center.Y == neighbour.Y && (center.X == neighbour.X + 1 || center.X == neighbour.X - 1);
        }
        /// <summary>
        /// Is a tile an diagonal neighbour of another tile.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <param name="center">A tile</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileDiagonalNeighbor<T>(T neighbour, T center) where T : ITile
        {
            return Mathf.Abs(neighbour.X - center.X) == 1 && Mathf.Abs(neighbour.Y - center.Y) == 1;
        }
        /// <summary>
        /// Is a tile any neighbour of another tile.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="neighbour">The tile to check as a neighbour</param>
        /// <param name="center">A tile</param>
        /// <returns>A boolean value</returns>
        public static bool IsTileAnyNeighbor<T>(T neighbour, T center) where T : ITile
        {
            return IsTileOrthogonalNeighbor(center, neighbour) || IsTileDiagonalNeighbor(center, neighbour);
        }
    }
    /// <summary>
    /// Allows you to cast lines of sight and cones of vision on a grid
    /// </summary>
    public class Raycasting
    {
        internal static bool IsTileOnALine<T>(T[,] map, T startTile, T tile, Vector2Int endPosition, bool allowDiagonals, bool favorVertical, bool breakOnWalls, MajorOrder majorOrder) where T : ITile
        {
            if (GridUtils.TileEquals(startTile, tile))
            {
                return true;
            }
            Vector2Int p0 = new Vector2Int(startTile.X, startTile.Y);
            Vector2Int p1 = endPosition;
            int dx = p1.x - p0.x;
            int dy = p1.y - p0.y;
            int nx = Mathf.Abs(dx);
            int ny = Mathf.Abs(dy);
            int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

            Vector2Int p = new Vector2Int(p0.x, p0.y);
            for (int ix = 0, iy = 0; ix < nx || iy < ny;)
            {
                int decision = (1 + 2 * ix) * ny - (1 + 2 * iy) * nx;
                if (!allowDiagonals && decision == 0)
                {
                    decision = favorVertical ? 1 : -1;
                }
                if (decision == 0)
                {
                    // next step is diagonal
                    p.x += sign_x;
                    p.y += sign_y;
                    ix++;
                    iy++;
                }
                else if (decision < 0)
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
                breakIt = breakIt ? true : !GridUtils.AreCoordsIntoGrid(map, p.x, p.y, majorOrder);
                T currentTile = breakIt ? default : GridUtils.GetTile(map, p.x, p.y, majorOrder);
                breakIt = breakIt ? true : currentTile == null || !currentTile.IsWalkable;
                bool continueIt = breakIt ? true : false;
                continueIt = continueIt ? true : currentTile == null;
                if (breakIt)
                {
                    break;
                }
                if (continueIt)
                {
                    continue;
                }
                if (GridUtils.TileEquals(currentTile, tile))
                {
                    return true;
                }
            }
            return false;
        }
        internal static void Raycast<T>(T[,] map, T startTile, Vector2Int endPosition, bool allowDiagonals, bool favorVertical, bool includeStart, bool includeDestination, bool breakOnWalls, bool includeWalls, out bool isClear, MajorOrder majorOrder, ref HashSet<T> results) where T : ITile
        {
            Vector2Int p0 = new Vector2Int(startTile.X, startTile.Y);
            Vector2Int p1 = endPosition;
            int dx = p1.x - p0.x;
            int dy = p1.y - p0.y;
            int nx = Mathf.Abs(dx);
            int ny = Mathf.Abs(dy);
            int sign_x = dx > 0 ? 1 : -1, sign_y = dy > 0 ? 1 : -1;

            Vector2Int p = new Vector2Int(p0.x, p0.y);
            if (includeStart)
            {
                results.Add(GridUtils.GetTile(map, p.x, p.y, majorOrder));
            }
            isClear = true;
            for (int ix = 0, iy = 0; ix < nx || iy < ny;)
            {
                int decision = (1 + 2 * ix) * ny - (1 + 2 * iy) * nx;
                if (!allowDiagonals && decision == 0)
                {
                    decision = favorVertical ? 1 : -1;
                }
                if (decision == 0)
                {
                    // next step is diagonal
                    p.x += sign_x;
                    p.y += sign_y;
                    ix++;
                    iy++;
                }
                else if (decision < 0)
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
                breakIt = breakIt ? true : !GridUtils.AreCoordsIntoGrid(map, p.x, p.y, majorOrder);
                T tile = breakIt ? default : GridUtils.GetTile(map, p.x, p.y, majorOrder);
                if (tile != null && !tile.IsWalkable)
                {
                    isClear = false;
                }
                breakIt = breakIt ? true : tile == null || !tile.IsWalkable;
                breakIt = breakIt ? true : !includeDestination && new Vector2Int(p.x, p.y) == p1;
                bool continueIt = breakIt ? true : false;
                continueIt = continueIt ? true : tile == null;
                continueIt = continueIt ? true : !includeWalls && !tile.IsWalkable;
                //continueIt = continueIt ? true : excludes != null && excludes.Contains(tile);
                if (breakIt)
                {
                    break;
                }
                if (continueIt)
                {
                    continue;
                }
                results.Add(GridUtils.GetTile(map, p.x, p.y, majorOrder));
            }
        }
        private static void ConeCast<T>(T[,] map, T center, int radius, float openingAngle, Vector2 direction, ref bool isClear, bool includeStart, ref HashSet<T> resultList, MajorOrder majorOrder) where T : ITile
        {
            bool lineClear = true;
            direction.Normalize();
            int x = 0;
            int y = -radius;
            int F_M = 1 - radius;
            int d_e = 3;
            int d_ne = -(radius << 1) + 5;
            RaycastToMirrorPositions(map, center, x, y, openingAngle, direction, ref lineClear, includeStart, ref resultList, majorOrder);
            if (!lineClear)
            {
                isClear = false;
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
                RaycastToMirrorPositions(map, center, x, y, openingAngle, direction, ref isClear, includeStart, ref resultList, majorOrder);
                if (!lineClear)
                {
                    isClear = false;
                }
            }
        }
        private static void RaycastToMirrorPositions<T>(T[,] map, T centerTile, int x, int y, float openingAngle, Vector2 direction, ref bool isClear, bool includeStart, ref HashSet<T> resultList, MajorOrder majorOrder) where T : ITile
        {
            bool lineClear = true;
            Vector2Int nei = new Vector2Int(centerTile.X + x, centerTile.Y + y);
            if (Extraction.IsIntoAngle(centerTile.X, centerTile.Y, nei.x, nei.y, openingAngle, direction))
            {
                Raycast(map, centerTile, new Vector2Int(nei.x, nei.y), false, false, includeStart, true, true, false, out lineClear, majorOrder, ref resultList);
            }
            if (!lineClear)
            {
                isClear = false;
            }
            nei = new Vector2Int(centerTile.X - x, centerTile.Y + y);
            if (Extraction.IsIntoAngle(centerTile.X, centerTile.Y, nei.x, nei.y, openingAngle, direction))
            {
                Raycast(map, centerTile, new Vector2Int(nei.x, nei.y), false, false, includeStart, true, true, false, out lineClear, majorOrder, ref resultList);
            }
            if (!lineClear)
            {
                isClear = false;
            }
            nei = new Vector2Int(centerTile.X + x, centerTile.Y - y);
            if (Extraction.IsIntoAngle(centerTile.X, centerTile.Y, nei.x, nei.y, openingAngle, direction))
            {
                Raycast(map, centerTile, new Vector2Int(nei.x, nei.y), false, false, includeStart, true, true, false, out lineClear, majorOrder, ref resultList);
                if (!lineClear)
                {
                    isClear = false;
                }
            }
            nei = new Vector2Int(centerTile.X - x, centerTile.Y - y);
            if (Extraction.IsIntoAngle(centerTile.X, centerTile.Y, nei.x, nei.y, openingAngle, direction))
            {
                Raycast(map, centerTile, new Vector2Int(nei.x, nei.y), false, false, includeStart, true, true, false, out lineClear, majorOrder, ref resultList);
                if (!lineClear)
                {
                    isClear = false;
                }
            }
            nei = new Vector2Int(centerTile.X + y, centerTile.Y + x);
            if (Extraction.IsIntoAngle(centerTile.X, centerTile.Y, nei.x, nei.y, openingAngle, direction))
            {
                Raycast(map, centerTile, new Vector2Int(nei.x, nei.y), false, false, includeStart, true, true, false, out lineClear, majorOrder, ref resultList);
                if (!lineClear)
                {
                    isClear = false;
                }
            }
            nei = new Vector2Int(centerTile.X - y, centerTile.Y + x);
            if (Extraction.IsIntoAngle(centerTile.X, centerTile.Y, nei.x, nei.y, openingAngle, direction))
            {
                Raycast(map, centerTile, new Vector2Int(nei.x, nei.y), false, false, includeStart, true, true, false, out lineClear, majorOrder, ref resultList);
                if (!lineClear)
                {
                    isClear = false;
                }
            }
            nei = new Vector2Int(centerTile.X + y, centerTile.Y - x);
            if (Extraction.IsIntoAngle(centerTile.X, centerTile.Y, nei.x, nei.y, openingAngle, direction))
            {
                Raycast(map, centerTile, new Vector2Int(nei.x, nei.y), false, false, includeStart, true, true, false, out lineClear, majorOrder, ref resultList);
                if (!lineClear)
                {
                    isClear = false;
                }
            }
            nei = new Vector2Int(centerTile.X - y, centerTile.Y - x);
            if (Extraction.IsIntoAngle(centerTile.X, centerTile.Y, nei.x, nei.y, openingAngle, direction))
            {
                Raycast(map, centerTile, new Vector2Int(nei.x, nei.y), false, false, includeStart, true, true, false, out lineClear, majorOrder, ref resultList);
                if (!lineClear)
                {
                    isClear = false;
                }
            }
        }

        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, T destinationTile, bool allowDiagonals = true, bool favorVertical = false, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            GetLineOfSight(map, out bool isClear, startTile, destinationTile, allowDiagonals, favorVertical, false, false, majorOrder);
            return isClear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, int length, float directionAngle, bool allowDiagonals = true, bool favorVertical = false, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            GetLineOfSight(map, out bool isClear, startTile, length, directionAngle, allowDiagonals, favorVertical, false, false, majorOrder);
            return isClear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, int length, Vector2 direction, bool allowDiagonals = true, bool favorVertical = false, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            GetLineOfSight(map, out bool isClear, startTile, length, direction, allowDiagonals, favorVertical, false, false, majorOrder);
            return isClear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsLineOfSightClear<T>(T[,] map, T startTile, Vector2Int endPosition, bool allowDiagonals = true, bool favorVertical = false, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            GetLineOfSight(map, out bool isClear, startTile, endPosition, allowDiagonals, favorVertical, false, false, majorOrder);
            return isClear;
        }

        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsConeOfVisionClear<T>(T[,] map, T startTile, float openingAngle, T destinationTile, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            GetConeOfVision(map, out bool clear, startTile, openingAngle, destinationTile, true, majorOrder);
            return clear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the cone</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsConeOfVisionClear<T>(T[,] map, T startTile, int length, float openingAngle, float directionAngle, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            GetConeOfVision(map, out bool clear, startTile, length, openingAngle, directionAngle, true, majorOrder);
            return clear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the cone</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsConeOfVisionClear<T>(T[,] map, T startTile, int length, float openingAngle, Vector2 direction, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            GetConeOfVision(map, out bool clear, startTile, length, openingAngle, direction, true, majorOrder);
            return clear;
        }
        /// <summary>
        /// Is the line of sight clear between two tiles
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool IsConeOfVisionClear<T>(T[,] map, T startTile, float openingAngle, Vector2Int endPosition, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            GetConeOfVision(map, out bool clear, startTile, openingAngle, endPosition, true, majorOrder);
            return clear;
        }

        /// <summary>
        /// Get all tiles on a line of sight from a start tile.<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, T destinationTile, bool allowDiagonals = false, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Vector2Int endPos = new Vector2Int(destinationTile.X, destinationTile.Y);
            return GetLineOfSight(map, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line of sight from a start tile.<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, int length, float directionAngle, bool allowDiagonals = false, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int endPos = new Vector2Int(Mathf.RoundToInt(startTile.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * length), Mathf.RoundToInt(startTile.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * length));
            return GetLineOfSight(map, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line of sight from a start tile.<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, int length, Vector2 direction, bool allowDiagonals = false, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int endPos = Vector2Int.RoundToInt(new Vector2(startTile.X, startTile.Y) + (direction.normalized * length));
            return GetLineOfSight(map, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line of sight from a start tile.<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, T startTile, Vector2Int endPosition, bool allowDiagonals = false, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            HashSet<T> hashSet = new HashSet<T>();
            Raycast(map, startTile, endPosition, allowDiagonals, favorVertical, includeStart, includeDestination, true, false, out bool isClear, majorOrder, ref hashSet);
            return hashSet.ToArray();
        }
        /// <summary>
        /// Get all tiles on a line of sight from a start tile.<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="isClear">Is the line of sight clear (no non-walkable tile encountered)</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static T[] GetLineOfSight<T>(T[,] map, out bool isClear, T startTile, T destinationTile, bool allowDiagonals = false, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            Vector2Int endPos = new Vector2Int(destinationTile.X, destinationTile.Y);
            return GetLineOfSight(map, out isClear, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line of sight from a start tile.<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="isClear">Is the line of sight clear (no non-walkable tile encountered)</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, out bool isClear, T startTile, int length, float directionAngle, bool allowDiagonals = false, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int endPos = new Vector2Int(Mathf.RoundToInt(startTile.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * length), Mathf.RoundToInt(startTile.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * length));
            return GetLineOfSight(map, out isClear, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line of sight from a start tile.<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="isClear">Is the line of sight clear (no non-walkable tile encountered)</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, out bool isClear, T startTile, int length, Vector2 direction, bool allowDiagonals = false, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int endPos = Vector2Int.RoundToInt(new Vector2(startTile.X, startTile.Y) + (direction.normalized * length));
            return GetLineOfSight(map, out isClear, startTile, endPos, allowDiagonals, favorVertical, includeStart, includeDestination, majorOrder);
        }
        /// <summary>
        /// Get all tiles on a line of sight from a start tile.<br/>
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="isClear">Is the line of sight clear (no non-walkable tile encountered)</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="allowDiagonals">Allows the diagonals or not. Default is true</param>
        /// <param name="favorVertical">If diagonals are disabled then favor vertical when a diagonal should have been used. False will favor horizontal and is the default value.</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetLineOfSight<T>(T[,] map, out bool isClear, T startTile, Vector2Int endPosition, bool allowDiagonals = false, bool favorVertical = false, bool includeStart = true, bool includeDestination = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            HashSet<T> hashSet = new HashSet<T>();
            Raycast(map, startTile, endPosition, allowDiagonals, favorVertical, includeStart, includeDestination, true, false, out isClear, majorOrder, ref hashSet);
            return hashSet.ToArray();
        }

        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="destinationTile">The destination tile at the end of the cone</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, float openingAngle, T destinationTile, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            Vector2Int startPos = new Vector2Int(startTile.X, startTile.Y);
            Vector2Int endPos = new Vector2Int(destinationTile.X, destinationTile.Y);
            Vector2 direction = endPos - startPos;
            HashSet<T> lines = new HashSet<T>();
            bool isClear = true;
            ConeCast(map, startTile, Mathf.CeilToInt(direction.magnitude), openingAngle, direction, ref isClear, includeStart, ref lines, majorOrder);
            return lines.ToArray();
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, int length, float openingAngle, float directionAngle, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int startPos = new Vector2Int(startTile.X, startTile.Y);
            Vector2Int endPos = new Vector2Int(Mathf.RoundToInt(startTile.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * length), Mathf.RoundToInt(startTile.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * length));
            Vector2 direction = endPos - startPos;
            HashSet<T> lines = new HashSet<T>();
            bool isClear = true;
            ConeCast(map, startTile, length, openingAngle, direction, ref isClear, includeStart, ref lines, majorOrder);
            return lines.ToArray();
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, int length, float openingAngle, Vector2 direction, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            HashSet<T> lines = new HashSet<T>();
            bool isClear = true;
            ConeCast(map, startTile, length, openingAngle, direction, ref isClear, includeStart, ref lines, majorOrder);
            return lines.ToArray();
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, T startTile, float openingAngle, Vector2Int endPosition, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            Vector2Int startPos = new Vector2Int(startTile.X, startTile.Y);
            Vector2 direction = endPosition - startPos;
            HashSet<T> lines = new HashSet<T>();
            bool isClear = true;
            ConeCast(map, startTile, Mathf.FloorToInt(direction.magnitude), openingAngle, direction, ref isClear, includeStart, ref lines, majorOrder);
            return lines.ToArray();
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="isClear">Is the line of sight clear (no non-walkable tile encountered)</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="destinationTile">The destination tile at the end of the cone</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, out bool isClear, T startTile, float openingAngle, T destinationTile, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            Vector2Int startPos = new Vector2Int(startTile.X, startTile.Y);
            Vector2Int endPos = new Vector2Int(destinationTile.X, destinationTile.Y);
            int radius = Mathf.CeilToInt(Vector2Int.Distance(startPos, endPos));
            Vector2 direction = endPos - startPos;
            HashSet<T> lines = new HashSet<T>();
            isClear = true;
            ConeCast(map, startTile, Mathf.FloorToInt(direction.magnitude), openingAngle, direction, ref isClear, includeStart, ref lines, majorOrder);
            return lines.ToArray();
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="isClear">Is the line of sight clear (no non-walkable tile encountered)</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="directionAngle">The angle of the line from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, out bool isClear, T startTile, int length, float openingAngle, float directionAngle, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            Vector2Int startPos = new Vector2Int(startTile.X, startTile.Y);
            Vector2Int endPos = new Vector2Int(Mathf.RoundToInt(startTile.X + Mathf.Cos(directionAngle * Mathf.Deg2Rad) * length), Mathf.RoundToInt(startTile.Y + Mathf.Sin(directionAngle * Mathf.Deg2Rad) * length));
            Vector2 direction = endPos - startPos;
            HashSet<T> lines = new HashSet<T>();
            isClear = true;
            ConeCast(map, startTile, length, openingAngle, direction, ref isClear, includeStart, ref lines, majorOrder);
            return lines.ToArray();
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="isClear">Is the line of sight clear (no non-walkable tile encountered)</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="length">The length of the line</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="direction">The direction of the line from the start tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, out bool isClear, T startTile, int length, float openingAngle, Vector2 direction, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            float magnitude = new Vector2Int(map.GetLength(0), map.GetLength(1)).magnitude;
            if (length > magnitude || Mathf.Approximately(length, 0f))
            {
                length = Mathf.CeilToInt(magnitude);
            }
            HashSet<T> lines = new HashSet<T>();
            isClear = true;
            ConeCast(map, startTile, length, openingAngle, direction, ref isClear, includeStart, ref lines, majorOrder);
            return lines.ToArray();
        }
        /// <summary>
        /// Get all visible tiles from a start tile's cone of vision<br/>
        /// Note that the order of the tiles into the returned array is not guaranteed.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="isClear">Is the line of sight clear (no non-walkable tile encountered)</param>
        /// <param name="startTile">The start tile</param>
        /// <param name="openingAngle">The cone opening angle in degrees [1-360]</param>
        /// <param name="endPosition">The destination virtual coordinates (do not need to be into grid range)</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] GetConeOfVision<T>(T[,] map, out bool isClear, T startTile, float openingAngle, Vector2Int endPosition, bool includeStart = true, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            openingAngle = Mathf.Clamp(openingAngle, 1f, 360f);
            Vector2Int startPos = new Vector2Int(startTile.X, startTile.Y);
            Vector2 direction = endPosition - startPos;
            HashSet<T> lines = new HashSet<T>();
            isClear = true;
            ConeCast(map, startTile, Mathf.CeilToInt(direction.magnitude), openingAngle, direction, ref isClear, includeStart, ref lines, majorOrder);
            return lines.ToArray();
        }
    }

    /// <summary>
    /// Allows you to calculate paths between tiles.<br>
    /// This API offers several way of doing pathfinding.<br>
    /// You can calculate the path directly every time you need (with the **CalculatePath** method), but this can become heavy if you do it too frequently.<br>
    /// Instead, you can generate objects that will hold multiple paths data that can be reused later. There is two types of objects that you can generate:<br>
    /// - **PathMap** - Will calculate and hold all the paths **to a specific tile from every accessible tiles**
    /// - **PathGrid** - Will calculate and hold all the paths **between each tiles on the entire grid**
    /// 
    /// *Note that, obviously, any path calculation is valid as long as the walkable state of the tiles remain unchanged*
    /// </summary>
    public class Pathfinding
    {
        private static bool GetTile<T>(T[,] map, int x, int y, out T tile, MajorOrder majorOrder) where T : ITile
        {
            if (x > -1 && y > -1 && x < GridUtils.GetHorizontalLength(map, majorOrder) && y < GridUtils.GetVerticalLength(map, majorOrder))
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
        private static bool CheckMP<T>(MovementsPolicy policy, T[,] map, T tile, MajorOrder majorOrder) where T : ITile
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
            if (leftWalkable && CheckMP(pathfindingPolicy.MovementsPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool rightWalkable = GetRightNeighbour(map, x, y, out nei, false, majorOrder);
            if (rightWalkable && CheckMP(pathfindingPolicy.MovementsPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool bottomWalkable = GetBottomNeighbour(map, x, y, out nei, false, majorOrder);
            if (bottomWalkable && CheckMP(pathfindingPolicy.MovementsPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool topWalkable = GetTopNeighbour(map, x, y, out nei, false, majorOrder);
            if (topWalkable && CheckMP(pathfindingPolicy.MovementsPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }

            bool leftBottomWalkable = GetLeftBottomNeighbour(map, x, y, out nei, false, majorOrder);
            if (CheckDP(pathfindingPolicy.DiagonalsPolicy, leftWalkable, bottomWalkable) && leftBottomWalkable && CheckMP(pathfindingPolicy.MovementsPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool rightBottomWalkable = GetRightBottomNeighbour(map, x, y, out nei, false, majorOrder);
            if (CheckDP(pathfindingPolicy.DiagonalsPolicy, rightWalkable, bottomWalkable) && rightBottomWalkable && CheckMP(pathfindingPolicy.MovementsPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool leftTopWalkable = GetLeftTopNeighbour(map, x, y, out nei, false, majorOrder);
            if (CheckDP(pathfindingPolicy.DiagonalsPolicy, leftWalkable, topWalkable) && leftTopWalkable && CheckMP(pathfindingPolicy.MovementsPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }
            bool rightTopWalkable = GetRightTopNeighbour(map, x, y, out nei, false, majorOrder);
            if (CheckDP(pathfindingPolicy.DiagonalsPolicy, rightWalkable, topWalkable) && rightTopWalkable && CheckMP(pathfindingPolicy.MovementsPolicy, map, nei, majorOrder))
            {
                nodes.Add(nei);
            }

            return nodes;
        }

        /// <summary>
        /// Generates asynchronously a PathMap object that will contain all the pre-calculated paths data between a target tile and all the accessible tiles from this target
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="targetTile">The target tile for the paths calculation</param>
        /// <param name="maxDistance">Optional parameter limiting the maximum movement distance from the target tile. 0 means no limit and is the default value</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <param name="progress">An optional IProgress object to get the generation progression</param>
        /// <param name="cancelToken">An optional CancellationToken object to cancel the generation</param>
        /// <returns>A PathMap object</returns>
        public static Task<PathMap<T>> GeneratePathMapAsync<T>(T[,] map, T targetTile, float maxDistance = 0f, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT, IProgress<float> progress = null, CancellationToken cancelToken = default) where T : ITile
        {
            Task<PathMap<T>> task = Task.Run(() =>
            {
                if (targetTile == null || !targetTile.IsWalkable)
                {
                    throw new Exception("Do not try to generate a PathMap with an unwalkable (or null) tile as the target");
                }
                Node<T> targetNode = new(targetTile);
                Dictionary<T, Node<T>> accessibleTilesDico = new() { { targetTile, targetNode } };
                List<T> accessibleTiles = new() { targetTile };
                PriorityQueueUnityPort.PriorityQueue<Node<T>, float> frontier = new();
                frontier.Enqueue(targetNode, 0);
                targetNode.NextNode = targetNode;
                targetNode.DistanceToTarget = 0f;
                int totalCount = map.GetLength(0) * map.GetLength(1);
                while (frontier.Count > 0)
                {
                    Node<T> current = frontier.Dequeue();
                    List<T> neighbourgs = GetTileNeighbours(map, current.Tile.X, current.Tile.Y, pathfindingPolicy, majorOrder);
                    foreach (T neiTile in neighbourgs)
                    {
                        if (cancelToken != default && cancelToken.IsCancellationRequested)
                        {
                            cancelToken.ThrowIfCancellationRequested();
                        }
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
                                float progressRatio = (float)accessibleTiles.Count / totalCount;
                                progress?.Report(progressRatio);
                            }
                            frontier.Enqueue(nei, newDistance);
                            nei.NextNode = current;
                            nei.NextDirection = new Vector2Int(nei.NextNode.Tile.X > nei.Tile.X ? 1 : (nei.NextNode.Tile.X < nei.Tile.X ? -1 : 0), nei.NextNode.Tile.Y > nei.Tile.Y ? 1 : (nei.NextNode.Tile.Y < nei.Tile.Y ? -1 : 0));
                            nei.DistanceToTarget = newDistance;
                        }
                    }
                }
                return new PathMap<T>(accessibleTilesDico, accessibleTiles, targetTile, maxDistance, majorOrder);
            });
            return task;
        }
        /// <summary>
        /// Generates a PathMap object that will contain all the pre-calculated paths data between a target tile and all the accessible tiles from this target
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="targetTile">The target tile for the paths calculation</param>
        /// <param name="maxDistance">Optional parameter limiting the maximum movement distance from the target tile. 0 means no limit and is the default value</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A PathMap object</returns>
        public static PathMap<T> GeneratePathMap<T>(T[,] map, T targetTile, float maxDistance = 0f, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            if (targetTile == null || !targetTile.IsWalkable)
            {
                throw new Exception("Do not try to generate a PathMap with an unwalkable (or null) tile as the target");
            }
            if (!CheckMP(pathfindingPolicy.MovementsPolicy, map, targetTile, majorOrder))
            {
                return new PathMap<T>(new Dictionary<T, Node<T>>(), new List<T>(), targetTile, maxDistance, majorOrder);
            }
            Node<T> targetNode = new(targetTile);
            Dictionary<T, Node<T>> accessibleTilesDico = new() { { targetTile, targetNode } };
            List<T> accessibleTiles = new() { targetTile };
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
        /// <summary>
        /// Generates asynchronously a PathGrid object that will contain all the pre-calculated paths data between each tile into the grid.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <param name="progress">An optional IProgress object to get the generation progression</param>
        /// <param name="cancelToken">An optional CancellationToken object to cancel the generation</param>
        /// <returns>A PathGrid object</returns>
        public static Task<PathGrid<T>> GeneratePathGridAsync<T>(T[,] map, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT, IProgress<float> progress = null, CancellationToken cancelToken = default) where T : ITile
        {
            Task<PathGrid<T>> task = Task.Run(() =>
            {
                PathMap<T>[,] grid = new PathMap<T>[map.GetLength(0), map.GetLength(1)];
                int maxI = map.GetLength(0);
                int maxJ = map.GetLength(1);
                int treatedCount = 0;
                int totalCount = (maxI * maxJ);
                for (int i = 0; i < maxI; i++)
                {
                    for (int j = 0; j < maxJ; j++)
                    {
                        if (cancelToken != default && cancelToken.IsCancellationRequested)
                        {
                            cancelToken.ThrowIfCancellationRequested();
                        }
                        treatedCount++;
                        T tile = map[i, j];
                        if (tile.IsWalkable)
                        {
                            grid[i, j] = GeneratePathMap(map, tile, 0f, pathfindingPolicy, majorOrder);
                            float progressRatio = (float)treatedCount / totalCount;
                            progress?.Report(progressRatio);
                        }
                    }
                }
                return new PathGrid<T>(grid);
            });
            return task;
        }
        /// <summary>
        /// Generates a PathGrid object that will contain all the pre-calculated paths data between each tile into the grid.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A PathGrid object</returns>
        public static PathGrid<T> GeneratePathGrid<T>(T[,] map, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            PathMap<T>[,] grid = new PathMap<T>[map.GetLength(0), map.GetLength(1)];
            int maxI = map.GetLength(0);
            int maxJ = map.GetLength(1);
            for (int i = 0; i < maxI; i++)
            {
                for (int j = 0; j < maxJ; j++)
                {
                    T tile = map[i, j];
                    if (tile.IsWalkable)
                    {
                        grid[i, j] = GeneratePathMap(map, tile, 0f, pathfindingPolicy, majorOrder);
                    }
                }
            }
            return new PathGrid<T>(grid);
        }
        /// <summary>
        /// Calculates asynchronously a path between a start tile and the closest of many destination tiles. If there is no path from the start tile to each destination tile then an empty array is returned.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile for the paths calculation</param>
        /// <param name="destinationTiles">A set of tiles to reach, only the closest accessible one will be actually reached</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static Task<T[]> CalculatePathAsync<T>(T[,] map, T startTile, T[] destinationTiles, bool includeStart = true, bool includeDestination = true, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT, IProgress<float> progress = null, CancellationToken ct = default) where T : ITile
        {
            Debug.Log("CALCULATE");
            Task<T[]> task = Task.Run(() =>
            {
                Debug.Log("TASK 1");
                if (startTile == null || !startTile.IsWalkable || destinationTiles == null || destinationTiles.Count(x => !x.IsWalkable) > 0)
                {
                    throw new Exception("Do not try to generate a path with an unwalkable (or null) tile as the start or destination");
                }
                Node<T> targetNode = new(startTile);
                Dictionary<T, Node<T>> accessibleTilesDico = new() { { startTile, targetNode } };
                List<T> accessibleTiles = new() { startTile };
                PriorityQueueUnityPort.PriorityQueue<Node<T>, float> frontier = new();
                frontier.Enqueue(targetNode, 0);
                targetNode.NextNode = targetNode;
                targetNode.DistanceToTarget = 0f;
                int totalCount = map.GetLength(0) * map.GetLength(1);
                while (frontier.Count > 0)
                {
                    Node<T> current = frontier.Dequeue();
                    List<T> neighbourgs = GetTileNeighbours(map, current.Tile.X, current.Tile.Y, pathfindingPolicy, majorOrder);
                    foreach (T neiTile in neighbourgs)
                    {
                        if (ct != default && ct.IsCancellationRequested)
                        {
                            ct.ThrowIfCancellationRequested();
                        }
                        Node<T> nei = accessibleTilesDico.ContainsKey(neiTile) ? accessibleTilesDico[neiTile] : new Node<T>(neiTile);
                        bool isDiagonal = current.Tile.X != nei.Tile.X && current.Tile.Y != nei.Tile.Y;
                        float newDistance = current.DistanceToTarget + nei.Tile.Weight * (isDiagonal ? pathfindingPolicy.DiagonalsWeight : 1f);
                        if (nei.NextNode == null || newDistance < nei.DistanceToTarget)
                        {
                            if (!accessibleTilesDico.ContainsKey(nei.Tile))
                            {
                                accessibleTilesDico.Add(nei.Tile, nei);
                                accessibleTiles.Add(nei.Tile);
                                float progressRatio = (float)accessibleTiles.Count / totalCount;
                                progress?.Report(progressRatio);
                                if (destinationTiles.Contains(nei.Tile))
                                {
                                    nei.NextNode = current;
                                    nei.NextDirection = new Vector2Int(nei.NextNode.Tile.X > nei.Tile.X ? 1 : (nei.NextNode.Tile.X < nei.Tile.X ? -1 : 0), nei.NextNode.Tile.Y > nei.Tile.Y ? 1 : (nei.NextNode.Tile.Y < nei.Tile.Y ? -1 : 0));
                                    nei.DistanceToTarget = newDistance;
                                    Node<T> pathNode = nei;
                                    List<T> path = new();
                                    while (pathNode != targetNode)
                                    {
                                        if ((includeStart || !GridUtils.TileEquals(pathNode.Tile, startTile)) && (includeDestination || !destinationTiles.Contains(pathNode.Tile)))
                                        {
                                            path.Add(pathNode.Tile);
                                        }
                                        pathNode = pathNode.NextNode;
                                    }
                                    return path.ToArray();
                                }
                            }
                            frontier.Enqueue(nei, newDistance);
                            nei.NextNode = current;
                            nei.NextDirection = new Vector2Int(nei.NextNode.Tile.X > nei.Tile.X ? 1 : (nei.NextNode.Tile.X < nei.Tile.X ? -1 : 0), nei.NextNode.Tile.Y > nei.Tile.Y ? 1 : (nei.NextNode.Tile.Y < nei.Tile.Y ? -1 : 0));
                            nei.DistanceToTarget = newDistance;
                        }
                    }
                }
                return new T[0];
            });
            return task;
        }
        /// <summary>
        /// Calculates a path between a start tile and the closest of many destination tiles. If there is no path from the start tile to each destination tile then an empty array is returned.
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile for the paths calculation</param>
        /// <param name="destinationTiles">A set of tiles to reach, only the closest accessible one will be actually reached</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] CalculatePath<T>(T[,] map, T startTile, T[] destinationTiles, bool includeStart = true, bool includeDestination = true, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            if (startTile == null || !startTile.IsWalkable || destinationTiles == null || destinationTiles.Count(x => !x.IsWalkable) > 0)
            {
                throw new Exception("Do not try to generate a path with an unwalkable (or null) tile as the start or destination");
            }
            Node<T> targetNode = new(startTile);
            Dictionary<T, Node<T>> accessibleTilesDico = new() { { startTile, targetNode } };
            List<T> accessibleTiles = new() { startTile };
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
                    if (nei.NextNode == null || newDistance < nei.DistanceToTarget)
                    {
                        if (!accessibleTilesDico.ContainsKey(nei.Tile))
                        {
                            accessibleTilesDico.Add(nei.Tile, nei);
                            accessibleTiles.Add(nei.Tile);

                            if (destinationTiles.Contains(nei.Tile))
                            {
                                nei.NextNode = current;
                                nei.NextDirection = new Vector2Int(nei.NextNode.Tile.X > nei.Tile.X ? 1 : (nei.NextNode.Tile.X < nei.Tile.X ? -1 : 0), nei.NextNode.Tile.Y > nei.Tile.Y ? 1 : (nei.NextNode.Tile.Y < nei.Tile.Y ? -1 : 0));
                                nei.DistanceToTarget = newDistance;
                                Node<T> pathNode = nei;
                                List<T> path = new();
                                while (pathNode != targetNode)
                                {
                                    if ((includeStart || !GridUtils.TileEquals(pathNode.Tile, startTile)) && (includeDestination || !destinationTiles.Contains(pathNode.Tile)))
                                    {
                                        path.Add(pathNode.Tile);
                                    }
                                    pathNode = pathNode.NextNode;
                                }
                                return path.ToArray();
                            }
                        }
                        frontier.Enqueue(nei, newDistance);
                        nei.NextNode = current;
                        nei.NextDirection = new Vector2Int(nei.NextNode.Tile.X > nei.Tile.X ? 1 : (nei.NextNode.Tile.X < nei.Tile.X ? -1 : 0), nei.NextNode.Tile.Y > nei.Tile.Y ? 1 : (nei.NextNode.Tile.Y < nei.Tile.Y ? -1 : 0));
                        nei.DistanceToTarget = newDistance;
                    }
                }
            }
            return new T[0];
        }
        /// <summary>
        /// Calculates asynchronously a path between a start tile and a destination tile. If there is no path between the start and destination then an empty array is returned
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile for the paths calculation</param>
        /// <param name="destinationTile">The destination tile for the paths calculation</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static Task<T[]> CalculatePathAsync<T>(T[,] map, T startTile, T destinationTile, bool includeStart = true, bool includeDestination = true, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT, IProgress<float> progress = null, CancellationToken ct = default) where T : ITile
        {
            return CalculatePathAsync(map, startTile, new T[] { destinationTile }, includeStart, includeDestination, pathfindingPolicy, majorOrder);
        }
        /// <summary>
        /// Calculates a path between a start tile and a destination tile. If there is no path between the start and destination then an empty array is returned
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array of tiles</param>
        /// <param name="startTile">The start tile for the paths calculation</param>
        /// <param name="destinationTile">The destination tile for the paths calculation</param>
        /// <param name="pathfindingPolicy">The PathfindingPolicy object to use</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>An array of tiles</returns>
        public static T[] CalculatePath<T>(T[,] map, T startTile, T destinationTile, bool includeStart = true, bool includeDestination = true, PathfindingPolicy pathfindingPolicy = default, MajorOrder majorOrder = MajorOrder.DEFAULT) where T : ITile
        {
            return CalculatePath(map, startTile, new T[] { destinationTile }, includeStart, includeDestination, pathfindingPolicy, majorOrder);
        }
    }
    internal class Node<T> where T : ITile
    {

        private T _tile;
        private Node<T> _next;
        private Vector2Int _nextDirection;
        private float _distanceToTarget;
        internal Node(T tile)
        {
            _tile = tile;
            IsWalkable = tile != null && tile.IsWalkable;
            Weight = tile == null ? 1f : Mathf.Max(tile.Weight, 1f);
        }

        internal T Tile { get => _tile; set => _tile = value; }
        internal Node<T> NextNode { get => _next; set => _next = value; }
        internal Vector2Int NextDirection { get => _nextDirection; set => _nextDirection = value; }
        internal float DistanceToTarget { get => _distanceToTarget; set => _distanceToTarget = value; }
        internal bool IsWalkable { get; set; }
        internal float Weight { get; set; }

        internal TIBakedTile ToBakedTile<TIBakedTile>() where TIBakedTile : IBakedTile
        {
            TIBakedTile tile = default;
            tile.IsWalkable = IsWalkable;
            tile.Weight = Weight;
            tile.X = Tile.X;
            tile.Y = Tile.Y;
            tile.NextNodeCoord = new(NextNode.Tile.X, NextNode.Tile.Y);
            tile.NextDirection = NextNode.NextDirection;
            tile.DistanceToTarget = NextNode.DistanceToTarget;
            return tile;
        }
        //internal static Node<T> FromBakedTile<TIBakedTile>(TIBakedTile bakedTile) where TIBakedTile : IBakedTile
        //{
        //    Node<T> node = default;
        //    node.Tile = bakedTile.
        //}
    }
    /// <summary>
    /// An object containing all the pre-calculated paths data between each tiles into the grid.
    /// </summary>
    /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
    public class PathGrid<T> where T : ITile
    {
        private readonly PathMap<T>[,] _grid;

        internal PathGrid(PathMap<T>[,] grid)
        {
            _grid = grid;
        }

        /// <summary>
        /// The MajorOrder parameter value that has been used to generate this PathGrid
        /// </summary>
        public MajorOrder MajorOrder { get; }

        /// <summary>
        /// Is there a path between a start tile and a destination tile
        /// </summary>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <returns>A boolean value</returns>
        public bool IsPath(T startTile, T destinationTile)
        {
            if (startTile == null || !startTile.IsWalkable || destinationTile == null || !destinationTile.IsWalkable)
            {
                throw new Exception("Do not call PathGrid methods with non-walkable (or null) tiles");
            }
            PathMap<T> pathMap = GridUtils.GetTile(_grid, destinationTile.X, destinationTile.Y, MajorOrder);
            return pathMap.IsTileAccessible(startTile);
        }
        /// <summary>
        /// Get all the accessible tiles from a target tile
        /// </summary>
        /// <param name="tile">The target tile</param>
        /// <param name="includeTarget">Include the target tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public T[] GetAccessibleTilesFromTile(T tile, bool includeTarget = true)
        {
            if (tile == null || !tile.IsWalkable)
            {
                throw new Exception("Do not call PathGrid methods with non-walkable (or null) tiles");
            }
            PathMap<T> pathMap = GridUtils.GetTile(_grid, tile.X, tile.Y, MajorOrder);
            return pathMap.GetAccessibleTiles(includeTarget);
        }
        /// <summary>
        /// Get the next tile on the path between a start tile and a destination tile
        /// </summary>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <returns>A tile object</returns>
        public bool GetNextTileFromTile(T startTile, T destinationTile, out T nextTile)
        {
            if (startTile == null || !startTile.IsWalkable || destinationTile == null || !destinationTile.IsWalkable)
            {
                throw new Exception("Do not call PathGrid methods with non-walkable (or null) tiles");
            }
            PathMap<T> pathMap = GridUtils.GetTile(_grid, destinationTile.X, destinationTile.Y, MajorOrder);
            if (!pathMap.IsTileAccessible(startTile))
            {
                nextTile = default;
                return false;
            }
            nextTile = pathMap.GetNextTileFromTile(startTile);
            return true;
        }
        /// <summary>
        /// Get the next tile on the path between a start tile and a destination tile
        /// </summary>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <returns>A Vector2Int direction</returns>
        public Vector2Int GetNextTileDirectionFromTile(T startTile, T destinationTile)
        {
            if (startTile == null || !startTile.IsWalkable || destinationTile == null || !destinationTile.IsWalkable)
            {
                throw new Exception("Do not call PathGrid methods with non-walkable (or null) tiles");
            }
            PathMap<T> pathMap = GridUtils.GetTile(_grid, destinationTile.X, destinationTile.Y, MajorOrder);
            return pathMap.GetNextTileDirectionFromTile(startTile);
        }
        /// <summary>
        /// Get the distance (movement cost) from a start tile to a destination tile.
        /// </summary>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <returns>The distance to the target</returns>
        public float GetDistanceBetweenTiles(T startTile, T destinationTile)
        {
            if (startTile == null || !startTile.IsWalkable || destinationTile == null || !destinationTile.IsWalkable)
            {
                throw new Exception("Do not call PathGrid methods with non-walkable (or null) tiles");
            }
            PathMap<T> pathMap = GridUtils.GetTile(_grid, destinationTile.X, destinationTile.Y, MajorOrder);
            return pathMap.GetDistanceToTargetFromTile(startTile);
        }
        /// <summary>
        /// Get all the tiles on the path from a start tile to a destination tile. If there is no path between the two tiles then an empty array will be returned.
        /// </summary>
        /// <param name="startTile">The start tile</param>
        /// <param name="destinationTile">The destination tile</param>
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeDestination">Include the target tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPath(T startTile, T destinationTile, bool includeStart = true, bool includeDestination = true)
        {
            if (startTile == null || !startTile.IsWalkable || destinationTile == null || !destinationTile.IsWalkable)
            {
                throw new Exception("Do not call PathGrid methods with non-walkable (or null) tiles");
            }
            PathMap<T> pathMap = GridUtils.GetTile(_grid, destinationTile.X, destinationTile.Y, MajorOrder);
            if (!pathMap.IsTileAccessible(startTile))
            {
                return new T[0];
            }
            return pathMap.GetPathToTarget(startTile, includeStart, includeDestination);
        }
        public TIBakedPathGrid ToBakedPathGrid<TIBakedPathGrid, TIBakedPathMap, TIBakedTile>() where TIBakedPathGrid : IBakedPathGrid<TIBakedPathMap, TIBakedTile> where TIBakedPathMap : IBakedPathMap<TIBakedTile> where TIBakedTile : IBakedTile
        {
            TIBakedPathGrid grid = default;
            grid.Grid = new();
            for (int i = 0; i < _grid.GetLength(0); i++)
            {
                for (int j = 0; j < _grid.GetLength(1); j++)
                {
                    grid.Grid.Add(_grid[i, j].ToBakedPathMap<TIBakedPathMap, TIBakedTile>());
                }
            }
            return grid;
        }
    }
    /// <summary>
    /// An object containing all the pre-calculated paths data between a target tile and all the accessible tiles from this target.
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
        /// The maximum distance of the accesibles tiles from the target
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
                throw new Exception("Do not call PathMap method with an inaccessible tile");
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
                throw new Exception("Do not call PathMap method with an inaccessible tile");
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
                throw new Exception("Do not call PathMap method with an inaccessible tile");
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
        /// <param name="includeStart">Include the start tile into the resulting array or not. Default is true</param>
        /// <param name="includeTarget">Include the target tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathToTarget(T startTile, bool includeStart = true, bool includeTarget = true)
        {
            if (!IsTileAccessible(startTile))
            {
                throw new Exception("Do not call PathMap method with an inaccessible tile");
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
        /// <param name="includeDestination">Include the destination tile into the resulting array or not. Default is true</param>
        /// <param name="includeTarget">Include the target tile into the resulting array or not</param>
        /// <returns>An array of tiles</returns>
        public T[] GetPathFromTarget(T destinationTile, bool includeDestination = true, bool includeTarget = true)
        {
            return GetPathToTarget(destinationTile, includeDestination, includeTarget).Reverse().ToArray();
        }
        public TIBakedPathMap ToBakedPathMap<TIBakedPathMap, TIBakedTile>() where TIBakedPathMap : IBakedPathMap<TIBakedTile> where TIBakedTile : IBakedTile
        {
            TIBakedPathMap tile = default;
            tile.AccessibleTiles = _dico.Select(x => x.Value.ToBakedTile<TIBakedTile>()).ToList();
            tile.Target = _dico[Target].ToBakedTile<TIBakedTile>();
            tile.MaxDistance = MaxDistance;
            tile.MajorOrder = MajorOrder;
            return tile;
        }
    }
    /// <summary>
    /// Some utilitary methods
    /// </summary>
    public static class GridUtils
    {
        /// <summary>
        /// Compare two tiles to check if they share the same coordinates values
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (needs to implement the ITile interface)</typeparam>
        /// <param name="tileA">The first tile</param>
        /// <param name="tileB">The second tile</param>
        /// <returns>A boolean value</returns>
        public static bool TileEquals<T>(T tileA, T tileB) where T : ITile
        {
            return tileA.X == tileB.X && tileA.Y == tileB.Y;
        }
        /// <summary>
        /// Return clamped coordinates into the grid
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (no need to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array</param>
        /// <param name="x">Horizontal coordinate to clamp</param>
        /// <param name="y">Vertical coordinate to clamp</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A Vector2Int representing the clamped coordinates</returns>
        public static Vector2Int ClampCoordsIntoGrid<T>(T[,] map, int x, int y, MajorOrder majorOrder)
        {
            ResolveMajorOrder(ref majorOrder);
            return new Vector2Int(Mathf.Clamp(x, 0, GetHorizontalLength(map, majorOrder) - 1), Mathf.Clamp(y, 0, GetVerticalLength(map, majorOrder) - 1));
        }
        /// <summary>
        /// Check if specific coordinates are into the grid range
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (no need to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array</param>
        /// <param name="x">Horizontal coordinate to check</param>
        /// <param name="y">Vertical coordinate to check</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A boolean value</returns>
        public static bool AreCoordsIntoGrid<T>(T[,] map, int x, int y, MajorOrder majorOrder)
        {
            ResolveMajorOrder(ref majorOrder);
            return x >= 0 && x < GetHorizontalLength(map, majorOrder) && y >= 0 && y < GetVerticalLength(map, majorOrder);
        }
        /// <summary>
        /// Returns a tile with automatic handling of the majorOrder
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (no need to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array</param>
        /// <param name="x">Horizontal coordinate of the tile</param>
        /// <param name="y">Vertical coordinate of the tile</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>A tile</returns>
        public static T GetTile<T>(T[,] map, int x, int y, MajorOrder majorOrder)
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
        /// <summary>
        /// Returns the horizontal length of a grid
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (no need to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>The horizontal length of a grid</returns>
        public static int GetHorizontalLength<T>(T[,] map, MajorOrder majorOrder)
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
        /// <summary>
        /// Returns the vertical length of a grid
        /// </summary>
        /// <typeparam name="T">The user-defined type representing a tile (no need to implement the ITile interface)</typeparam>
        /// <param name="map">A two-dimensional array</param>
        /// <param name="majorOrder">The major order rule to use for the grid indexes. Default is MajorOrder.DEFAULT (see KevinCastejon::GridHelper::MajorOrder)</param>
        /// <returns>The vertical length of a grid</returns>
        public static int GetVerticalLength<T>(T[,] map, MajorOrder majorOrder)
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
