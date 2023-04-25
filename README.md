# GridHelper

This package offers a utilitary API to help with operations on **grids** such as **tile extraction**, **line of sight**, **pathfinding**, etc...

Comes with several demo examples.

[See online documentation](https://kevincastejon.github.io/Unity-GridHelper/)

[Get the Unity package](https://github.com/kevincastejon/Unity-GridHelper/releases/latest)

[See my other Unity packages](https://assetstore.unity.com/publishers/46935)

**[Complete API Documentation](https://kevincastejon.fr/demos/Documentations/Unity-GridHelper/)**

## Usages

All you need to use this API is a two-dimensional array of tiles using *row major order* (first index is line, second is the column).

What is a *tile* ? Any object (custom class, struct, component, ...) that implements the very light **ITile** interface of this library. This interface requires a boolean **IsWalkable** property, a float **Weight** property and two int **X** and **Y** properties.

### - <u>Extractions</u>

You can extract tiles into a radius, or into a rectangle, around a tile.

- **GetTilesIntoARectangle**
```cs
YourCustomTileType[] tiles = GridHelper.GetTilesIntoARectangle(grid, centerTile, rectangleSize);
```
- **GetTilesIntoARadius**
```cs
YourCustomTileType[] tiles = GridHelper.GetTilesIntoARadius(grid, centerTile, radius);
```

You can also get only the walkable tiles into a radius/rectangle, around a tile.

- **GetWalkableTilesIntoARectangle**
```cs
YourCustomTileType[] tiles = GridHelper.GetWalkableTilesIntoARectangle(grid, centerTile, rectangleSize);
```
- **GetWalkableTilesIntoARadius**
```cs
YourCustomTileType[] tiles = GridHelper.GetWalkableTilesIntoARadius(grid, centerTile, radius);
```

You can also get only the tiles on the radius/rectangle outline.

- **GetTilesIntoARectangleOutline**
```cs
YourCustomTileType[] tiles = GridHelper.GetTilesIntoARectangleOutline(grid, centerTile, rectangleSize);
```
- **GetTilesIntoARadiusOutline**
```cs
YourCustomTileType[] tiles = GridHelper.GetTilesIntoARadiusOutline(grid, centerTile, radius);
```

Finally, you can also get only the walkable tiles on the radius/rectangle outline.

- **GetWalkableTilesIntoARectangleOutline**
```cs
YourCustomTileType[] tiles = GridHelper.GetWalkableTilesIntoARectangleOutline(grid, centerTile, rectangleSize);
```
- **GetWalkableTilesIntoARadiusOutline**
```cs
YourCustomTileType[] tiles = GridHelper.GetWalkableTilesIntoARadiusOutline(grid, centerTile, radius);
```

### - <u>Line</u>

You can get all the tiles on a line between two tiles

- **GetTilesOnALine**
```cs
YourCustomTileType[] tiles = GridHelper.GetTilesOnALine(grid, startTile, stopTile);
```

You can also get only the walkable tiles on a line between two tiles

- **GetWalkableTilesOnALine**
```cs
YourCustomTileType[] tiles = GridHelper.GetWalkableTilesOnALine(grid, startTile, stopTile);
```

You can get the line of sight between two tiles (a line that "stops" at the first encountered unwalkable tile)

- **GetLineOfSight**
```cs
YourCustomTileType[] tiles = GridHelper.GetLineOfSight(grid, startTile, stopTile);
```

You can know if the line of sight between two tiles is clear (has not encountered any unwalkable tile)

- **IsLineOfSightClear**
```cs
bool isLineClear = GridHelper.IsLineOfSightClear(grid, startTile, stopTile);
```

### - <u>Pathfinding</u>

You can get all the accessible tiles from a target tile. You can use a maximum movement cost or set it to 0 to have no limit. (This method uses a **Dijkstra** algorithm with early exit)

- **GetAccessibleTilesFromTarget**
```cs
YourCustomTileType[] tiles = GridHelper.GetAccessibleTilesFromTarget(maximumMovement);
```

The other part of the pathfinding section of this library generates and uses a **PathMap** object that holds all the calculated paths data for the entire grid.

This way of doing pathfinding is usefull for some usages (like Tower Defenses) because it calculates once all the paths between one tile and all the others. (These methods use **Dijkstra** algorithm)

To generate the **PathMap** object, use the **GeneratePathMap** method.

```cs
PathMap<YourCustomTileType> pathMap = GridHelper.GeneratePathMap(_grid, targetTile);
```

Once the **PathMap** object is generated, you can use its several and almost "*cost free*" methods and properties.

You can retrieve the tile that has been used as the target to generate this **PathMap**

- **Target**
```cs
YourCustomTileType tile = pathMap.Target;
```

You can get all the tiles on the path from a tile to the target.

- **GetPathToTarget**
```cs
YourCustomTileType[] tiles = pathMap.GetPathToTarget(startTile);
```

Or you can get all the tiles on the path from the target to a tile.

- **GetPathFromTarget**
```cs
YourCustomTileType[] tiles = pathMap.GetPathFromTarget(destinationTile);
```

### - <u>Pathfinding</u> - other PathMap features

You can get info on a specific tile through some **PathMap** methods.

Get the distance to the target from a tile.

- **GetDistanceToTargetFromTile**
```cs
float cost = pathMap.GetDistanceToTargetFromTile(tile);
```

You can get the next tile on the path between the target and a tile.

- **GetNextTileFromTile**
```cs
YourCustomTileType nextTile = pathMap.GetNextTileFromTile(tile);
```

You can get the next tile direction on the path between the target and a tile (in 2D grid coordinates).

- **GetNextTileDirectionFromTile**
```cs
Vector2 nextTileDirection = pathMap.GetNextTileDirectionFromTile(tile);
```
