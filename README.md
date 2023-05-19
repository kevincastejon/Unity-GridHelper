# GridHelper

This package offers utilitary API to help with operations on **2D and 3D grids** such as tile **extraction**, **raycasting**, and **pathfinding**.

Comes with several demo examples.

[See online documentation](https://kevincastejon.github.io/Unity-GridHelper/)

[Get the Unity package](https://github.com/kevincastejon/Unity-GridHelper/releases/latest)

[See my other Unity packages](https://assetstore.unity.com/publishers/46935)

**[Complete API Documentation](https://kevincastejon.fr/demos/Documentations/Unity-GridHelper/)**

---
## Usages

All you need to use this API is a two-dimensional array (three-dimensional for 3D API) of tiles.

What is a *tile* ? Any object (custom class, struct, component, ...) that implements the very light **ITile** interface of this library (**ITile3D** for the 3D API). This interface requires four properties getters (five for 3D API):
- *bool* **IsWalkable** . Must return if the tile can be walk/see throught (for pathfinding/raycasting)
- *float* **Weight** . Must return the tile's weight (the 'cost' it will takes to walk throught that tile). Minimum is 1.
- *int* **X** . Must return the horizontal position of the tile into the grid
- *int* **Y** . Must return the vertical position of the tile into the grid
- *int* **Z** . Must return the depth position of the tile into the grid (only for the 3D API and its **ITile3D** interface)

This API is using a namespace so you have to add a using instruction to the scripts that will need this library:
```cs
using KevinCastejon.GridHelper;
```
or for the 3D API:
```cs
using KevinCastejon.GridHelper3D;
```
---
---
## 2D API
---
---
### - <u>Extraction</u>
---
You can always specify a *majorOrder* parameter that tells which indexes order to use for the grid. Default is ROW_MAJOR_ORDER (YX)

You can extract tiles in a circle, or in a rectangle, around a tile.

- **GetTilesInARectangle**
```cs
YourCustomTileType[] tiles = Extraction.GetTilesInARectangle(grid, centerTile, rectangleSize);
```
- **GetTilesInACircle**
```cs
YourCustomTileType[] tiles = Extraction.GetTilesInACircle(grid, centerTile, radius);
```

You can also get only the walkable tiles in a circle/rectangle, around a tile.

- **GetWalkableTilesInARectangle**
```cs
YourCustomTileType[] tiles = Extraction.GetWalkableTilesInARectangle(grid, centerTile, rectangleSize);
```
- **GetWalkableTilesInACircle**
```cs
YourCustomTileType[] tiles = Extraction.GetWalkableTilesInACircle(grid, centerTile, radius);
```

You can also get only the tiles on the circle/rectangle outline.

- **GetTilesOnARectangleOutline**
```cs
YourCustomTileType[] tiles = Extraction.GetTilesOnARectangleOutline(grid, centerTile, rectangleSize);
```
- **GetTilesOnACircleOutline**
```cs
YourCustomTileType[] tiles = Extraction.GetTilesOnACircleOutline(grid, centerTile, radius);
```

Finally, you can also get only the walkable tiles on the circle/rectangle outline.

- **GetWalkableTilesOnARectangleOutline**
```cs
YourCustomTileType[] tiles = Extraction.GetWalkableTilesOnARectangleOutline(grid, centerTile, rectangleSize);
```
- **GetWalkableTilesOnACircleOutline**
```cs
YourCustomTileType[] tiles = Extraction.GetWalkableTilesOnACircleOutline(grid, centerTile, radius);
```

---

You can get neighbour of a tile (if it exists).

- **GetTileNeighbour**
```cs
YourCustomTileType upperNeighbour = Extraction.GetTileNeighbour(tile, Vector2Int.up);
```

---

Besides from extracting tiles, you can know if a specific tile is contained into a circle/rectangle or not. Same with the outlines.

- **IsTileInARectangle**
```cs
bool isTileInARectangle = Extraction3D.IsTileInARectangle(centerTile, tile, rectangleSize);
```
- **IsTileInARectangleOutline**
```cs
bool isTileInARectangleOutline = Extraction3D.IsTileInARectangleOutline(centerTile, tile, rectangleSize);
```
- **IsTileInACircle**
```cs
bool isTileInACircle = Extraction3D.IsTileInACircle(centerTile, tile, radius);
```
- **IsTileInACircleOutline**
```cs
bool isTileInACircleOutline = Extraction3D.IsTileInACircleOutline(centerTile, tile, radius);
```

---
### - <u>Raycasting</u>
---
You can always specify a *majorOrder* parameter that tells which indexes order to use for the grid. Default is ROW_MAJOR_ORDER (YX)

You can get all the tiles on a line between two tiles

- **GetTilesOnALine**
```cs
YourCustomTileType[] tiles = Raycasting.GetTilesOnALine(grid, startTile, destinationTile);
```

You can also get only the walkable tiles on a line between two tiles

- **GetWalkableTilesOnALine**
```cs
YourCustomTileType[] tiles = Raycasting.GetWalkableTilesOnALine(grid, startTile, destinationTile);
```

---

You can get the line of sight between two tiles (a line that "stops" at the first encountered unwalkable tile)

- **GetLineOfSight**
```cs
YourCustomTileType[] tiles = Raycasting.GetLineOfSight(grid, startTile, destinationTile);
```

You can know if the line of sight between two tiles is clear (has not encountered any unwalkable tile)

- **IsLineOfSightClear**
```cs
bool isLineClear = Raycasting.IsLineOfSightClear(grid, startTile, destinationTile);
```

---
### - <u>Pathfinding</u>
---
The pathfinding part of this library generates a **PathMap** object that holds all the calculated paths data.

This way of doing pathfinding is useful for some usages (like Tower Defenses and more) because it calculates once all the paths between one tile, called the "**target**", and all the others accessible tiles. (The **PathMap** generation uses **Dijkstra** algorithm).

To generate the **PathMap** object, use the **GeneratePathMap** method that needs the *grid* and the *target* tile from which to calculate the paths, as parameters.

You can use an optional *maxDistance* parameter that limits the paths calculation to an amount of distance (movement 'cost' including the tiles weights). Default is 0 and means no distance limit (paths to all accessible tiles on the entire grid will be calculated).

You can specify a *pathfindingPolicy* parameter that holds parameters relating to diagonals and allowed movements. (see **PathfindingPolicy**)

You can specify a *majorOrder* parameter that tells which indexes order to use for the grid. Default is ROW_MAJOR_ORDER (YX)

```cs
PathMap<YourCustomTileType> pathMap = Pathfinding.GeneratePathMap(grid, targetTile);
```

---
### - <u>PathMap</u>
---
Once the **PathMap** object is generated, you can use its several and almost "*cost free*" methods and properties.

---

You can retrieve the tile that has been used as the target to generate this **PathMap**

- **Target**
```cs
YourCustomTileType tile = pathMap.Target;
```

You can retrieve the *maxDistance* parameter value that has been used to generate this **PathMap**

- **MaxDistance**
```cs
float maxDistance = pathMap.MaxDistance;
```

You can retrieve the **majorOrder** parameter value that has been used to generate this **PathMap**

- **MajorOrder**
```cs
MajorOrder majorOrder = pathMap.MajorOrder;
```
---
You can get all the accessible tiles from the target tile.

- **GetAccessibleTiles**
```cs
YourCustomTileType[] tiles = GridHelper.GetAccessibleTiles();
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

---
### - <u>PathMap</u> - other features
---
You can get info on a specific tile through some **PathMap** methods.


You can know if a tile is accessible from the target tile. This is useful before calling the following **PathMap** methods that only takes an accessible tile as parameter.

- **IsTileAccessible**
```cs
bool isTileAccessible = pathMap.IsTileAccessible(tile);
```

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

---
### - <u>PathfindingPolicy</u>

The **PathfindingPolicy** object holds settings relating to diagonals and allowed movements.

You can set the **DiagonalsPolicy** that represents the diagonals permissiveness. When going diagonally from a tile A to tile B in 2D grid, there are two more tile involved, the ones that are both facing neighbours of the A and B tiles. You can allow diagonals movement depending on the walkable status of these tiles.
- **DiagonalsPolicy**
```cs
pathfindingPolicy.DiagonalsPolicy = DiagonalsPolicy.ALL_DIAGONALS;
```
  - **NONE** : no diagonal movement allowed
  - **DIAGONAL_2FREE** : only diagonal movements, with two walkable facing neighbours common to the start and destination tiles, are allowed
  - **DIAGONAL_1FREE** : only diagonal movements, with one walkable facing neighbour common to the start and destination tiles, are allowed
  - **ALL_DIAGONALS** : all diagonal movements allowed

![DiagonalsPolicySchema](Assets/KevinCastejon/GridHelper/Documentation/DiagonalsPolicySchema.png)

---

You can set the diagonals weight ratio multiplier that will increase the tile's weight when moving to it diagonally.

Minimum is 1. Default is 1.4142135623730950488016887242097.

Note that setting diagonals weight to 1 can lead to unpredictable behaviours on pathfinding as a diagonal move has same cost than orthogonal one so the paths could become "serrated" (but still the shortests!).
- **DiagonalsWeight**
```cs
pathfindingPolicy.DiagonalsWeight = 1.5f;
```

---

You can set the **MovementPolicy** that represents the movement permissiveness. It is useful to allow special movement, especially for side-view games, such as spiders that can walk on walls or roofs, or flying characters. Default is FLY. Top-down view grid based games should not use other value than the default as they do not hold concept of "gravity" nor "up-and-down".

Note that this parameter is a flag enumeration, so you can cumulate multiple states, the FLY state being the most permissive and making useless its combination with any other one.
- **MovementPolicy**
```cs
pathfindingPolicy.MovementPolicy = MovementPolicy.ALL_DIAGONALS;
```
  - **FLY** : all walkable tiles can be walk thought
  - **WALL_BELOW** : the walkable tiles that has a not-walkable lower neighbour can be walk thought
  - **WALL_ASIDE** : the walkable tiles that has a not-walkable side neighbour can be walk thought
  - **WALL_ABOVE** : the walkable tiles that has a not-walkable upper neighbour can be walk thought

![MovementPolicySchema](Assets/KevinCastejon/GridHelper/Documentation/MovementsPolicySchema.png)


---
## 3D API

---
### - <u>Extraction3D</u>

You can always specify a *majorOrder* parameter that tells which indexes order to use for the grid. Default is YXZ

You can extract tiles in a sphere, or in a cuboid, around a tile.

- **GetTilesInACuboid**
```cs
YourCustomTileType[] tiles = Extraction3D.GetTilesInACuboid(grid, centerTile, rectangleSize);
```
- **GetTilesInASphere**
```cs
YourCustomTileType[] tiles = Extraction3D.GetTilesInASphere(grid, centerTile, radius);
```

You can also get only the walkable tiles in a sphere/cuboid, around a tile.

- **GetWalkableTilesInACuboid**
```cs
YourCustomTileType[] tiles = Extraction3D.GetWalkableTilesInACuboid(grid, centerTile, rectangleSize);
```
- **GetWalkableTilesInASphere**
```cs
YourCustomTileType[] tiles = Extraction3D.GetWalkableTilesInASphere(grid, centerTile, radius);
```

You can also get only the tiles on the sphere/cuboid outline.

- **GetTilesOnACuboidOutline**
```cs
YourCustomTileType[] tiles = Extraction3D.GetTilesOnACuboidOutline(grid, centerTile, rectangleSize);
```
- **GetTilesOnASphereOutline**
```cs
YourCustomTileType[] tiles = Extraction3D.GetTilesOnASphereOutline(grid, centerTile, radius);
```

Finally, you can also get only the walkable tiles on the sphere/cuboid outline.

- **GetWalkableTilesOnACuboidOutline**
```cs
YourCustomTileType[] tiles = Extraction3D.GetWalkableTilesOnACuboidOutline(grid, centerTile, rectangleSize);
```
- **GetWalkableTilesOnASphereOutline**
```cs
YourCustomTileType[] tiles = Extraction3D.GetWalkableTilesOnASphereOutline(grid, centerTile, radius);
```

You can get neighbour of a tile (if it exists).

- **GetTileNeighbour**
```cs
YourCustomTileType frontNeighbour = Extraction3D.GetTileNeighbour(tile, Vector3Int.forward);
```

Besides from extracting tiles, you can know if a specific tile is contained into a sphere/cuboid or not. Same with the outlines.

- **IsTileInACuboid**
```cs
bool isTileInACuboid = Extraction3D.IsTileInACuboid(centerTile, tile, rectangleSize);
```
- **IsTileInACuboidOutline**
```cs
bool isTileInACuboidOutline = Extraction3D.IsTileInACuboidOutline(centerTile, tile, rectangleSize);
```
- **IsTileInASphere**
```cs
bool isTileInASphere = Extraction3D.IsTileInASphere(centerTile, tile, radius);
```
- **IsTileInASphereOutline**
```cs
bool isTileInASphereOutline = Extraction3D.IsTileInASphereOutline(centerTile, tile, radius);
```

---
### - <u>Raycasting3D</u>

You can always specify a *majorOrder* parameter that tells which indexes order to use for the grid. Default is YXZ

You can get all the tiles on a line between two tiles

- **GetTilesOnALine**
```cs
YourCustomTileType[] tiles = Raycasting3D.GetTilesOnALine(grid, startTile, destinationTile);
```

You can also get only the walkable tiles on a line between two tiles

- **GetWalkableTilesOnALine**
```cs
YourCustomTileType[] tiles = Raycasting3D.GetWalkableTilesOnALine(grid, startTile, destinationTile);
```

You can get the line of sight between two tiles (a line that "stops" at the first encountered unwalkable tile)

- **GetLineOfSight**
```cs
YourCustomTileType[] tiles = Raycasting3D.GetLineOfSight(grid, startTile, destinationTile);
```

You can know if the line of sight between two tiles is clear (has not encountered any unwalkable tile)

- **IsLineOfSightClear**
```cs
bool isLineClear = Raycasting3D.IsLineOfSightClear(grid, startTile, destinationTile);
```

---
### - <u>Pathfinding3D</u>

The pathfinding part of this library generates a **PathMap3D** object that holds all the calculated paths data.

This way of doing pathfinding is useful for some usages (like Tower Defenses and more) because it calculates once all the paths between one tile, called the "**target**", and all the others accessible tiles. (The **PathMap3D** generation uses **Dijkstra** algorithm).

To generate the **PathMap3D** object, use the **GeneratePathMap** method that needs the *grid* and the *target* tile from which to calculate the paths, as parameters.

You can use an optional *maxDistance* parameter that limits the paths calculation to an amount of distance (movement 'cost' including the tiles weights). Default is 0 and means no distance limit (paths to all accessible tiles on the entire grid will be calculated).

You can specify a *pathfindingPolicy* parameter that holds parameters relating to diagonals and allowed movements. (see **Pathfinding3DPolicy**)

You can specify a *majorOrder* parameter that tells which indexes order to use for the grid. Default is YXZ

```cs
PathMap3D<YourCustomTileType> pathMap = Pathfinding3D.GeneratePathMap(grid, targetTile, maxDistance);
```

---
### - <u>PathMap3D</u>

Once the **PathMap3D** object is generated, you can use its several and almost "*cost free*" methods and properties.

You can retrieve the tile that has been used as the target to generate this **PathMap3D**

- **Target**
```cs
YourCustomTileType tile = pathMap.Target;
```

You can retrieve the *maxDistance* parameter value that has been used to generate this **PathMap3D**

- **MaxDistance**
```cs
float maxDistance = pathMap.MaxDistance;
```

You can retrieve the **majorOrder** parameter value that has been used to generate this **PathMap3D**

- **MajorOrder**
```cs
MajorOrder3D majorOrder = pathMap.MajorOrder;
```

You can get all the accessible tiles from the target tile.

- **GetAccessibleTiles**
```cs
YourCustomTileType[] tiles = GridHelper.GetAccessibleTiles();
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

---
### - <u>PathMap3D</u> - other features

You can get info on a specific tile through some **PathMap3D** methods.


You can know if a tile is accessible from the target tile. This is useful before calling the following **PathMap3D** methods that only takes an accessible tile as parameter.

- **IsTileAccessible**
```cs
bool isTileAccessible = pathMap.IsTileAccessible(tile);
```

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
Vector3 nextTileDirection = pathMap.GetNextTileDirectionFromTile(tile);
```
