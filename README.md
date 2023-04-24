# GridHelper

This package offers several utilitary classes to help with operations on **grids** such as **tile extraction**, **line of sight**, **pathfinding**, etc...

Keep using your own tile objects as long as they implement the very light **ITile** interface of this library.

Comes with several demo examples.

[See online documentation](https://kevincastejon.github.io/Unity-GridHelper/)

[Get the Unity package](https://github.com/kevincastejon/Unity-GridHelper/releases/latest)

[See my other Unity packages](https://assetstore.unity.com/publishers/46935)

**[Complete API Documentation](https://kevincastejon.fr/demos/Documentations/Unity-GridHelper/)**

## Usages

This library is using a generic type for the user-defined tile type that has to implement the **ITile** interface. This interface requires a boolean **IsWalkable** property, a float **Weight** property and two int **X** and **Y** properties.

### - Extractions

You can extract tiles into a radius, or into a rectangle, around a tile.

- **GetTilesIntoARectangle**
```cs
GridHelper.GetTilesIntoARectangle(grid, centerTile, rectangleSize);
```
- **GetTilesIntoARadius**
```cs
GridHelper.GetTilesIntoARadius(grid, centerTile, radius);
```

You can also get only the walkable tiles.

- **GetWalkableTilesIntoARectangle**
```cs
GridHelper.GetWalkableTilesIntoARectangle(grid, centerTile, rectangleSize);
```
- **GetWalkableTilesIntoARadius**
```cs
GridHelper.GetWalkableTilesIntoARadius(grid, centerTile, radius);
```

You can also get only the tiles on the radius/rectangle outline.

- **GetTilesIntoARectangleOutline**
```cs
GridHelper.GetTilesIntoARectangleOutline(grid, centerTile, rectangleSize);
```
- **GetTilesIntoARadiusOutline**
```cs
GridHelper.GetTilesIntoARadiusOutline(grid, centerTile, radius);
```

Finally, you can also get only the walkable tiles on the radius/rectangle outline.

- **GetWalkableTilesIntoARectangleOutline**
```cs
GridHelper.GetWalkableTilesIntoARectangleOutline(grid, centerTile, rectangleSize);
```
- **GetWalkableTilesIntoARadiusOutline**
```cs
GridHelper.GetWalkableTilesIntoARadiusOutline(grid, centerTile, radius);
```

### - Line

You can get all the tiles on a line between two tiles

- **GetTilesOnALine**
```cs
GridHelper.GetTilesOnALine(grid, startTile, stopTile);
```

You can also get only the walkable tiles...

- **GetWalkableTilesOnALine**
```cs
GridHelper.GetWalkableTilesOnALine(grid, startTile, stopTile);
```

You can get the line of sight between two tiles (a line that "stops" at the first encountered unwalkable tile)

- **GetLineOfSight**
```cs
GridHelper.GetLineOfSight(grid, startTile, stopTile);
```

You can know if the line of sight between two tiles is clear (has not encountered any unwalkable tile)

- **IsLineOfSightClear**
```cs
GridHelper.IsLineOfSightClear(grid, startTile, stopTile);
```

### - Pathfinding

The pathfinding part of this library uses **Dijkstra** algorithms for generating a **PathMap** object that holds all the calculated paths data for the entire grid. This way of doing pathfinding is usefull for some usages because it calculates once all the paths between one tile and all the others.

To generate the **PathMap** object, use the **GeneratePathMap** method. The **PathMap** class is using a generic type for the user-defined tile type so you have to explicit that type on declaration.

```cs
PathMap<Tile> pathMap = GridHelper.GeneratePathMap(_grid, targetTile);
```

Once the **PathMap** object is generated, you can use its several and almost "*cost free*" methods and properties.

You can retrieve the tile that has been used as the target to generate this **PathMap**

- **Target**
```cs
pathMap.Target;
```

You can get all the accessible tiles from the target tile. You can use a int maximum movement steps count (number of tiles), a float maximum movement cost ("distance" of the path taking account of the tiles weights) or no maximum at all (pass 0 as parameter or just do not pass any parameter).

- **GetAccessibleTilesFromTarget**
```cs
pathMap.GetAccessibleTilesFromTarget(maximumMovement);
```

You can get all the tiles on the path from a tile to the target.

- **GetPathToTarget**
```cs
pathMap.GetPathToTarget(startTile);
```

Or you can get all the tiles on the path from the target to a tile.

- **GetPathFromTarget**
```cs
pathMap.GetPathFromTarget(destinationTile);
```

### Pathfinding - other PathMap features

You can get info a specific tile through some **PathMap** methods.

You can get the number of steps on the path between the target and a tile.

- **GetMovementStepsFromTile**
```cs
pathMap.GetMovementStepsFromTile(tile);
```

You can get the movement cost on the path between the target and a tile.

- **GetMovementCostFromTile**
```cs
pathMap.GetMovementCostFromTile(tile);
```

You can get the next tile on the path between the target and a tile.

- **GetNextTileFromTile**
```cs
pathMap.GetNextTileFromTile(tile);
```

You can get the next tile direction on the path between the target and a tile.

- **GetNextTileDirectionFromTile**
```cs
pathMap.GetNextTileDirectionFromTile(tile);
```
