using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WorldMapGen
{
    // Class that procedurally generates a map
    public class MapGenerator : MonoBehaviour
    {
        // Possible directions from which the wind can blow
        protected enum WindDirection
        {
            West,
            East
        }

        // Maximum X and Y offset for heightmap noise
        protected const float noiseMaxOffset = 1000.0f;

        // User-specified parameters for map generation
        [SerializeField]
        protected MapParameters parameters;
        public MapParameters Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        // Map currently being generated
        protected Tilemap currentMap;

        // Map's dimensions in km
        protected Vector2 scaledSize;

        // Procedurally generate a map, storing it in the given tilemap
        // Return the random seed used for generation
        public virtual int GenerateMap(Tilemap map)
        {
            // Store the existing random state
            Random.State oldState = Random.state;
            // Reinitialise the random state for the generator
            int randomSeed;
            if (parameters.CustomSeed)
            {
                // User-specified seed
                randomSeed = parameters.Seed;
            }
            else
            {
                // Seed from system time
                randomSeed = System.Environment.TickCount;
            }
            Random.InitState(randomSeed);

            currentMap = map;
            scaledSize = ScaleCoords(parameters.Width, parameters.Height);

            CreateTiles();
            GenerateElevation();
            GenerateTemperature();
            GenerateRainfall();
            GenerateRivers();
            SelectBiomes();

            currentMap.RefreshAllTiles();
            currentMap = null;

            // Restore the random state outside the generator
            Random.state = oldState;

            return randomSeed;
        }

        // Create all tile objects in the map
        protected virtual void CreateTiles()
        {
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    currentMap.SetTile(
                        new Vector3Int(j, i, 0),
                        ScriptableObject.CreateInstance<Tile>());
                }
            }
        }

        // Return the latitude in radians corresponding to the given Y
        // coordinate
        protected virtual float LatitudeAtY(int y)
        {
            return ((float)y / parameters.Height - 0.5f) * Mathf.PI;
        }

        // Scale the given tile coordinates to km
        protected virtual Vector2 ScaleCoords(int x, int y)
        {
            return new Vector2(x * parameters.TileScale.x,
                               y * parameters.TileScale.y);
        }

        // Scale the given tile coordinates to km
        protected virtual Vector2 ScaleCoords(Vector2Int coords)
        {
            return ScaleCoords(coords.x, coords.y);
        }

        // Return the angle corresponding to a particular tile in a wrapping
        // dimension
        protected virtual float WrappingAngle(int coord, int dimension)
        {
            return 2.0f * Mathf.PI * coord / dimension;
        }

        // Get the square of the distance between two sets of scaled
        // coordinates, taking into account wrapping parameters
        protected virtual float WrappingSqrDistance(Vector2 a, Vector2 b)
        {
            float dx = Mathf.Abs(a.x - b.x);
            if (parameters.WrapX && dx > scaledSize.x / 2.0f)
            {
                dx = scaledSize.x - dx;
            }

            float dy = Mathf.Abs(a.y - b.y);
            if (parameters.WrapY && dy > scaledSize.y / 2.0f)
            {
                dy = scaledSize.y - dy;
            }

            return dx * dx + dy * dy;
        }

        // Generate elevation for every tile
        protected virtual void GenerateElevation()
        {
            // Noise scale adjusted for each dimension
            Vector2 noiseScale = new Vector2(
                DimensionNoiseScale(
                    parameters.WrapX, parameters.Width,
                    parameters.TileScale.x, scaledSize.x, scaledSize.y),
                DimensionNoiseScale(
                    parameters.WrapY, parameters.Height,
                    parameters.TileScale.y, scaledSize.y, scaledSize.x));

            // Offsets used to randomise noise
            Vector4 noiseOffset =
                new Vector4(Random.Range(0.0f, noiseMaxOffset),
                            Random.Range(0.0f, noiseMaxOffset),
                            Random.Range(0.0f, noiseMaxOffset),
                            Random.Range(0.0f, noiseMaxOffset));

            // Sorted list of the raw noise values for each tile
            List<float> sortedNoise = new List<float>();

            // Generate raw noise
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));
                    if (parameters.WrapX && parameters.WrapY)
                    {
                        // Wrapping dimensions make a circle in the noise
                        float noiseAngleX =
                            WrappingAngle(j, parameters.Width);
                        float noiseAngleY =
                            WrappingAngle(i, parameters.Height);
                        currentTile.Elevation = SimplexNoise.Noise4D(
                            Mathf.Sin(noiseAngleX) * noiseScale.x +
                                noiseOffset.x,
                            Mathf.Cos(noiseAngleX) * noiseScale.x +
                                noiseOffset.y,
                            Mathf.Sin(noiseAngleY) * noiseScale.y +
                                noiseOffset.z,
                            Mathf.Cos(noiseAngleY) * noiseScale.y +
                                noiseOffset.w);
                    }
                    else if (parameters.WrapX)
                    {
                        float noiseAngle =
                            WrappingAngle(j, parameters.Width);
                        currentTile.Elevation = SimplexNoise.Noise3D(
                            Mathf.Sin(noiseAngle) * noiseScale.x +
                                noiseOffset.x,
                            Mathf.Cos(noiseAngle) * noiseScale.x +
                                noiseOffset.y,
                            i * noiseScale.y + noiseOffset.z);
                    }
                    else if (parameters.WrapY)
                    {
                        float noiseAngle =
                            WrappingAngle(i, parameters.Height);
                        currentTile.Elevation = SimplexNoise.Noise3D(
                            j * noiseScale.x + noiseOffset.x,
                            Mathf.Sin(noiseAngle) * noiseScale.y +
                                noiseOffset.y,
                            Mathf.Cos(noiseAngle) * noiseScale.y +
                                noiseOffset.z);
                    }
                    else
                    {
                        // Not wrapping
                        currentTile.Elevation = SimplexNoise.Noise2D(
                            j * noiseScale.x + noiseOffset.x,
                            i * noiseScale.y + noiseOffset.y);
                    }
                    currentTile.Elevation =
                        Mathf.Clamp01(currentTile.Elevation);

                    // Add the value to the sorted list
                    int sortedIndex =
                        sortedNoise.BinarySearch(currentTile.Elevation);
                    if (sortedIndex < 0) sortedIndex = ~sortedIndex;
                    sortedNoise.Insert(sortedIndex, currentTile.Elevation);
                }
            }

            // Value subtracted from noise to achieve correct ocean coverage
            float oceanOffset =
                sortedNoise[Mathf.FloorToInt(sortedNoise.Count *
                                             parameters.OceanCoverage)];

            // Elevation scale based on highest elevation among all tile types
            float elevationScale = float.NegativeInfinity;
            foreach (TileType type in parameters.TileTypes)
            {
                if (type.Elevation.Max > elevationScale)
                {
                    elevationScale = type.Elevation.Max;
                }
            }

            // Offset and scale all elevations
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));
                    currentTile.Elevation -= oceanOffset;
                    currentTile.Elevation *= elevationScale;
                    // Also scale based on maximum value of unscaled heightmap
                    currentTile.Elevation /= 1.0f - oceanOffset;
                }
            }
        }

        // Return the noise scale factor adjusted for a particular dimension
        // wrap: does the map wrap in this dimension?
        // dimension: number of tiles in this dimension
        // tileScale: tile scale in this dimension
        // scaledDimension: map size in this dimension in km
        // scaledOtherDimension: map size in the other dimension in km
        protected virtual float DimensionNoiseScale(
            bool wrap, int dimension, float tileScale,
            float scaledDimension, float scaledOtherDimension)
        {
            // The number of noise function units covered by one tile in this
            // dimension should be 1/(noise scale parameter*N), where N is the
            // number of tiles in the longer dimension
            // In the shorter dimension, the above value should also be
            // multiplied by the ratio of the tile scale in this dimension vs
            // the other dimension

            float scale = 1.0f / parameters.NoiseScale;

            if (wrap)
            {
                // In a wrapping dimension, one tile covers 2*pi/dimension
                // unscaled noise function units
                scale /= 2.0f * Mathf.PI;
                if (scaledOtherDimension > scaledDimension)
                {
                    scale *= scaledDimension;
                    scale /= scaledOtherDimension;
                }
            }
            else
            {
                // In a non-wrapping dimension, one tile covers one unscaled
                // noise function unit
                if (scaledDimension >= scaledOtherDimension)
                {
                    scale /= dimension;
                }
                else
                {
                    scale *= tileScale;
                    scale /= scaledOtherDimension;
                }
            }

            return scale;
        }

        // Generate temperature for every tile
        protected virtual void GenerateTemperature()
        {
            for (int i = 0; i < parameters.Height; i++)
            {
                // Temperature not adjusted for elevation
                float latitudeTemperature = TemperatureAtY(i);

                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));
                    if (currentTile.Elevation > 0.0f)
                    {
                        // For land tiles, adjust temperature for elevation
                        currentTile.Temperature =
                            TemperatureAtElevation(latitudeTemperature,
                                                   currentTile.Elevation);
                    }
                    else
                    {
                        currentTile.Temperature = latitudeTemperature;
                    }
                }
            }
        }

        // Generate precipitation for every tile
        protected virtual void GenerateRainfall()
        {
            GenerateLatitudeRainfall();
            AdjustRainfallForOceanDistance();
            AdjustOrographicRainfall();
        }

        // Generate precipitation for every tile based on its latitude
        protected virtual void GenerateLatitudeRainfall()
        {
            for (int i = 0; i < parameters.Height; i++)
            {
                // Precipitation for this row
                float latitudeRainfall = RainfallAtY(i);

                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));
                    currentTile.Precipitation = latitudeRainfall;
                }
            }
        }

        // Reduce every land tile's precipitation based on its distance to the
        // ocean
        protected virtual void AdjustRainfallForOceanDistance()
        {
            // Tile coordinates of tiles left to visit
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();

            // Add all ocean tiles to the queue
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));
                    if (currentTile.Elevation <= 0.0f)
                    {
                        Vector2Int coords = new Vector2Int(j, i);
                        // Ocean tile's nearest ocean tile is itself
                        currentTile.NearestOcean = coords;
                        frontier.Enqueue(coords);
                    }
                }
            }

            // Find the nearest ocean tile for all tiles using a Euclidean
            // distance transform
            while (frontier.Count > 0)
            {
                // Get the tile currently being visited
                Vector2Int coords = frontier.Dequeue();
                Tile currentTile = (Tile)currentMap.GetTile(
                    new Vector3Int(coords.x, coords.y, 0));

                // Update nearest ocean tile for all neighbours
                if (coords.x > 0 || parameters.WrapX)
                {
                    int neighborX =
                        coords.x > 0 ? coords.x : parameters.Width;
                    neighborX--;

                    CheckNeighborOceanDistance(
                        neighborX, coords.y, currentTile.NearestOcean,
                        frontier);
                }
                if (coords.x < parameters.Width - 1 || parameters.WrapX)
                {
                    int neighborX =
                        coords.x < parameters.Width - 1 ? coords.x + 1 : 0;

                    CheckNeighborOceanDistance(
                        neighborX, coords.y, currentTile.NearestOcean,
                        frontier);
                }
                if (coords.y > 0 || parameters.WrapY)
                {
                    int neighborY =
                        coords.y > 0 ? coords.y : parameters.Height;
                    neighborY--;

                    CheckNeighborOceanDistance(
                        coords.x, neighborY, currentTile.NearestOcean,
                        frontier);
                }
                if (coords.y < parameters.Height - 1 || parameters.WrapY)
                {
                    int neighborY =
                        coords.y < parameters.Height - 1 ? coords.y + 1 : 0;

                    CheckNeighborOceanDistance(
                        coords.x, neighborY, currentTile.NearestOcean,
                        frontier);
                }
            }

            // Adjust all land tiles' precipitation
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));
                    if (currentTile.Elevation > 0.0f)
                    {
                        currentTile.Precipitation /= Mathf.Exp(
                            Mathf.Sqrt(
                                WrappingSqrDistance(
                                    ScaleCoords(j, i),
                                    ScaleCoords(currentTile.NearestOcean))) /
                            parameters.RainfallOceanEFoldingDistance);
                    }
                }
            }
        }

        // If the given neighbour is closer to nearestOcean than to its current
        // nearest ocean tile (or if its nearest ocean tile is unknown), update
        // its nearest ocean tile and add the neighbour to frontier
        protected virtual void CheckNeighborOceanDistance(
            int neighborX, int neighborY, Vector2Int nearestOcean,
            Queue<Vector2Int> frontier)
        {
            // Get the neighbour
            Tile neighbor = (Tile)currentMap.GetTile(
                new Vector3Int(neighborX, neighborY, 0));
            Vector2Int neighborCoords = new Vector2Int(neighborX, neighborY);
            Vector2 neighborKmCoords = ScaleCoords(neighborCoords);

            // Should the neighbour be updated?
            if (neighbor.Elevation > 0.0f && (
                    !neighbor.HasNearestOcean() ||
                    WrappingSqrDistance(neighborKmCoords,
                                        ScaleCoords(nearestOcean)) <
                    WrappingSqrDistance(neighborKmCoords,
                                        ScaleCoords(neighbor.NearestOcean))))
            {
                frontier.Enqueue(neighborCoords);
                neighbor.NearestOcean = nearestOcean;
            }
        }

        // Apply effect of orographic precipitation to all land tiles
        protected virtual void AdjustOrographicRainfall()
        {
            for (int i = 0; i < parameters.Height; i++)
            {
                // Temperature at sea level for this row
                float seaLevelTemperature = TemperatureAtY(i);
                // Elevation of the previous tile in this row
                float prevElevation = 0.0f;
                // Wind direction in this row
                WindDirection windDirection = WindDirectionAtY(i);

                for (int j = 0; j < parameters.Width; j++)
                {
                    // Iterate left to right or right to left depending on wind
                    // direction
                    int x = windDirection == WindDirection.West ?
                        j : parameters.Width - j - 1;

                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(x, i, 0));

                    // Only apply to land tiles
                    if (currentTile.Elevation > 0.0f)
                    {
                        // If wrapping horizontally, apply orographic
                        // precipitation from the last tile in the row to the
                        // first
                        if (j == 0 && parameters.WrapX)
                        {
                            Tile prevTile = (Tile)currentMap.GetTile(
                                new Vector3Int(
                                    parameters.Width - x - 1, i, 0));
                            prevElevation = Mathf.Max(
                                0.0f, prevTile.Elevation);
                        }
                        // Add the effect of orographic precipitation
                        if (j > 0 || parameters.WrapX)
                        {
                            currentTile.Precipitation += OrographicRainfall(
                                currentTile.Elevation, prevElevation,
                                currentTile.Temperature, seaLevelTemperature);
                        }

                        prevElevation = currentTile.Elevation;
                    }
                    else
                    {
                        prevElevation = 0.0f;
                    }
                }
            }
        }

        // Return the wind direction at the given Y coordinate
        protected virtual WindDirection WindDirectionAtY(int y)
        {
            return WindDirectionAtLatitude(LatitudeAtY(y));
        }

        // Return the wind direction at the given latitude
        protected virtual WindDirection WindDirectionAtLatitude(float latitude)
        {
            float absLatitude = Mathf.Abs(latitude);

            if (absLatitude > Mathf.Deg2Rad * parameters.LowPressureLatitude ||
                absLatitude < Mathf.Deg2Rad * parameters.HighPressureLatitude)
            {
                if (parameters.RotateWest)
                {
                    // Opposite direction of Earth
                    return WindDirection.West;
                }
                else
                {
                    // Polar easterlies or trade winds
                    return WindDirection.East;
                }
            }
            if (parameters.RotateWest)
            {
                // Opposite direction of Earth
                return WindDirection.East;
            }
            // Westerlies
            return WindDirection.West;
        }

        // Calculate temperature based on latitude for the given Y coordinate
        protected virtual float TemperatureAtY(int y)
        {
            return TemperatureAtLatitude(LatitudeAtY(y));
        }

        // Calculate temperature based on latitude
        protected virtual float TemperatureAtLatitude(float latitude)
        {
            float sinLatitude = Mathf.Sin(latitude);
            return
                parameters.EquatorTemperature -
                (parameters.EquatorTemperature - parameters.PoleTemperature) *
                sinLatitude * sinLatitude;
        }

        // Adjust the given temperature based on the given elevation
        protected virtual float TemperatureAtElevation(
            float temperature, float elevation)
        {
            return temperature - elevation * parameters.TemperatureLapseRate;
        }

        // Calculate precipitation based on latitude for the given Y coordinate
        protected virtual float RainfallAtY(int y)
        {
            return RainfallAtLatitude(LatitudeAtY(y));
        }

        // Calculate precipitation based on latitude
        protected virtual float RainfallAtLatitude(float latitude)
        {
            // Terms to be squared
            float equatorSquareBase =
                latitude / parameters.EquatorRainfallEvenness;
            float midLatitudeSquareBase1 =
                (latitude - parameters.LowPressureLatitude * Mathf.Deg2Rad) /
                parameters.MidLatitudeRainfallEvenness;
            float midLatitudeSquareBase2 =
                (latitude + parameters.LowPressureLatitude * Mathf.Deg2Rad) /
                parameters.MidLatitudeRainfallEvenness;

            return parameters.MidLatitudeRainfall /
                   (1.0f + midLatitudeSquareBase1 * midLatitudeSquareBase1) +
                   parameters.EquatorRainfall /
                   (1.0f + equatorSquareBase * equatorSquareBase) +
                   parameters.MidLatitudeRainfall /
                   (1.0f + midLatitudeSquareBase2 * midLatitudeSquareBase2);
        }

        // Return the change in precipitation due to the effects of wind and
        // elevation
        protected virtual float OrographicRainfall(
            float elevation, float prevElevation,
            float temperature, float seaLevelTemperature)
        {
            temperature += Globals.CelsiusToKelvin;

            return
                parameters.CondensationRateMultiplier *
                Mathf.Exp(
                    parameters.SaturationPressureConst1 *
                    seaLevelTemperature /
                        (parameters.SaturationPressureConst2 +
                         seaLevelTemperature) -
                    elevation * parameters.MoistureScaleHeightDivisor *
                    parameters.TemperatureLapseRate /
                    (temperature * temperature)) *
                (elevation - prevElevation) /
                (parameters.TileScale.x * Globals.KmToM);
        }

        // Place rivers on the map
        protected virtual void GenerateRivers()
        {
            // In non-wrapping dimensions, there are more corners than tiles
            int cornersWidth = parameters.Width;
            if (!parameters.WrapX) cornersWidth++;
            int cornersHeight = parameters.Height;
            if (!parameters.WrapY) cornersHeight++;

            // Check all tile corners
            for (int i = 0; i < cornersHeight; i++)
            {
                for (int j = 0; j < cornersWidth; i++)
                {
                    // River tiles are at Z=1
                    RiverTile currentCorner =
                        currentMap.GetTile<RiverTile>(new Vector3Int(j, i, 1));
                    // Skip if there is already a river here or if next to
                    // ocean
                    if (currentCorner || CornerAtOcean(j, i)) continue;

                    // Certain probability of starting river
                    if (Random.value < RiverProbability(j, i))
                    {
                        ContinueRiver(j, i, 0);
                    }
                }
            }
        }

        // Place a river at the given corner, then determine the next corner in
        // the river and, if necessary, call this function for that corner
        // prevDirection is the direction that the previous corner is in
        protected virtual void ContinueRiver(
            int x, int y, RiverTile.Directions prevDirection)
        {
            // Create the new river tile and set its connection to the previous
            // one
            RiverTile newTile = ScriptableObject.CreateInstance<RiverTile>();
            newTile.Parameters = parameters;
            newTile.Connections = prevDirection;
            currentMap.SetTile(new Vector3Int(x, y, 1), newTile);

            // If the river is on a wrapping edge, place the same river tile on
            // the opposite side
            bool wrapX =
                parameters.WrapX && (x == 0 || x == parameters.Width);
            bool wrapY =
                parameters.WrapY && (y == 0 || y == parameters.Height);
            if (wrapX)
            {
                // Horizontally
                currentMap.SetTile(
                    new Vector3Int(parameters.Width - x, y, 1), newTile);
            }
            if (wrapY)
            {
                // Vertically
                currentMap.SetTile(
                    new Vector3Int(x, parameters.Height - y, 1), newTile);
            }
            if (wrapX && wrapY)
            {
                // Corner wrapping in both dimensions
                currentMap.SetTile(
                    new Vector3Int(parameters.Width - x,
                                   parameters.Height - y, 1), newTile);
            }

            // Stop if reached ocean
            if (CornerAtOcean(x, y)) return;

            // Next river tile is down the steepest downslope
            int nextX, nextY;
            SteepestSlope(x, y, out nextX, out nextY);

            // Stop if there is no downslope
            if (nextX == -1 || nextY == -1) return;

            // Try to get existing river tile at next position
            RiverTile nextCorner =
                currentMap.GetTile<RiverTile>(new Vector3Int(nextX, nextY, 1));

            // Check direction of next river tile
            // Left
            if (nextX < x)
            {
                // Set connection to the next river tile
                newTile.Connections |= RiverTile.Directions.Left;
                // If next river tile already exists, set its connection to the
                // current tile and stop
                if (nextCorner)
                    nextCorner.Connections |= RiverTile.Directions.Right;
                // Recursively continue river generation
                else ContinueRiver(nextX, nextY, RiverTile.Directions.Right);
            }
            // Right
            else if (nextX > x)
            {
                newTile.Connections |= RiverTile.Directions.Right;
                if (nextCorner)
                    nextCorner.Connections |= RiverTile.Directions.Left;
                else ContinueRiver(nextX, nextY, RiverTile.Directions.Left);
            }
            // Up
            else if (nextY < y)
            {
                newTile.Connections |= RiverTile.Directions.Up;
                if (nextCorner)
                    nextCorner.Connections |= RiverTile.Directions.Down;
                else ContinueRiver(nextX, nextY, RiverTile.Directions.Down);
            }
            // Down
            else if (nextY > y)
            {
                newTile.Connections |= RiverTile.Directions.Down;
                if (nextCorner)
                    nextCorner.Connections |= RiverTile.Directions.Up;
                else ContinueRiver(nextX, nextY, RiverTile.Directions.Up);
            }
        }

        // Output coordinates of the tiles adjacent to the given corner
        // coordinates
        // If applicable, the output coordinates are wrapped
        // For any coordinates that do not contain a tile, output -1
        // x: corner X coordinate; outputs X coordinate of tiles to the right
        // y: corner Y coordinate; outputs Y coordinate of tiles below
        // leftX: outputs X coordinate of tiles to the left
        // upY: outputs Y coordinate of tiles above
        protected virtual void WrappedCornerTiles(
            ref int x, ref int y, out int leftX, out int upY)
        {
            leftX = Globals.WrappedCoord(
                x - 1, parameters.Width, parameters.WrapX);
            upY = Globals.WrappedCoord(
                y - 1, parameters.Height, parameters.WrapY);
            x = Globals.WrappedCoord(x, parameters.Width, parameters.WrapX);
            y = Globals.WrappedCoord(y, parameters.Height, parameters.WrapY);
        }

        // Return true if any of the tiles around the given corner are ocean
        // tiles
        protected virtual bool CornerAtOcean(int x, int y)
        {
            // Get the coordinates of the adjacent tiles
            int leftX, upY;
            WrappedCornerTiles(ref x, ref y, out leftX, out upY);

            // Lower right
            return
                x != -1 && y != -1 &&
                currentMap.GetTile<Tile>(
                    new Vector3Int(x, y, 0)).Elevation < 0.0f ||
                // Lower left
                leftX != -1 && y != -1 &&
                currentMap.GetTile<Tile>(
                    new Vector3Int(leftX, y, 0)).Elevation < 0.0f ||
                // Upper right
                x != -1 && upY != -1 &&
                currentMap.GetTile<Tile>(
                    new Vector3Int(x, upY, 0)).Elevation < 0.0f ||
                // Upper left
                leftX != -1 && upY != -1 &&
                currentMap.GetTile<Tile>(
                    new Vector3Int(leftX, upY, 0)).Elevation < 0.0f;
        }

        // Calculate the average elevation of the 4 tiles around the given
        // corner coordinates
        protected virtual float CornerElevation(int x, int y)
        {
            // Get the coordinates of the adjacent tiles
            int leftX, upY;
            WrappedCornerTiles(ref x, ref y, out leftX, out upY);

            // Sum of tile elevations around this corner
            float sum = 0.0f;
            // Number of tiles around this corner
            int count = 0;

            // Lower right
            if (x != -1 && y != -1)
            {
                sum += currentMap.GetTile<Tile>(
                    new Vector3Int(x, y, 0)).Elevation;
                count++;
            }
            // Lower left
            if (leftX != -1 && y != -1)
            {
                sum += currentMap.GetTile<Tile>(
                    new Vector3Int(leftX, y, 0)).Elevation;
                count++;
            }
            // Upper right
            if (x != -1 && upY != -1)
            {
                sum += currentMap.GetTile<Tile>(
                    new Vector3Int(x, upY, 0)).Elevation;
                count++;
            }
            // Upper left
            if (leftX != -1 && upY != -1)
            {
                sum += currentMap.GetTile<Tile>(
                    new Vector3Int(leftX, upY, 0)).Elevation;
                count++;
            }

            return sum / count;
        }

        // From the given corner coordinates, calculate the steepest downslope
        // Output the coordinates of the corner down that slope to otherX and
        // otherY
        // If there are no downslopes, return 0 and set otherX and otherY to -1
        protected virtual float SteepestSlope(
            int x, int y, out int otherX, out int otherY)
        {
            // Steepest downslope found so far
            float maxSlope = 0.0f;
            float slope;
            // Elevation of the corner being checked
            float elevation = CornerElevation(x, y);

            otherX = -1;
            otherY = -1;

            // Left
            int adjacentX = Globals.WrappedCoord(
                x - 1, parameters.Width + 1, parameters.WrapX);
            if (adjacentX != -1)
            {
                slope = (elevation - CornerElevation(adjacentX, y)) /
                    (parameters.TileScale.x * Globals.KmToM);
                if (slope > maxSlope)
                {
                    maxSlope = slope;
                    otherX = adjacentX;
                    otherY = y;
                }
            }

            // Right
            adjacentX = Globals.WrappedCoord(
                x + 1, parameters.Width + 1, parameters.WrapX);
            if (adjacentX != -1)
            {
                slope = (elevation - CornerElevation(adjacentX, y)) /
                    (parameters.TileScale.x * Globals.KmToM);
                if (slope > maxSlope)
                {
                    maxSlope = slope;
                    otherX = adjacentX;
                    otherY = y;
                }
            }

            // Up
            int adjacentY = Globals.WrappedCoord(
                y - 1, parameters.Height + 1, parameters.WrapY);
            if (adjacentY != -1)
            {
                slope = (elevation - CornerElevation(x, adjacentY)) /
                    (parameters.TileScale.y * Globals.KmToM);
                if (slope > maxSlope)
                {
                    maxSlope = slope;
                    otherX = x;
                    otherY = adjacentY;
                }
            }

            // Down
            adjacentY = Globals.WrappedCoord(
                y + 1, parameters.Height + 1, parameters.WrapY);
            if (adjacentY != -1)
            {
                slope = (elevation - CornerElevation(x, adjacentY)) /
                    (parameters.TileScale.y * Globals.KmToM);
                if (slope > maxSlope)
                {
                    maxSlope = slope;
                    otherX = x;
                    otherY = adjacentY;
                }
            }

            return maxSlope;
        }

        // Calculate the average precipitation of the 4 tiles around the given
        // corner coordinates
        protected virtual float CornerPrecipitation(int x, int y)
        {
            // Get the coordinates of the adjacent tiles
            int leftX, upY;
            WrappedCornerTiles(ref x, ref y, out leftX, out upY);

            // Sum of tile precipitations around this corner
            float sum = 0.0f;
            // Number of tiles around this corner
            int count = 0;

            // Lower right
            if (x != -1 && y != -1)
            {
                sum += currentMap.GetTile<Tile>(
                    new Vector3Int(x, y, 0)).Precipitation;
                count++;
            }
            // Lower left
            if (leftX != -1 && y != -1)
            {
                sum += currentMap.GetTile<Tile>(
                    new Vector3Int(leftX, y, 0)).Precipitation;
                count++;
            }
            // Upper right
            if (x != -1 && upY != -1)
            {
                sum += currentMap.GetTile<Tile>(
                    new Vector3Int(x, upY, 0)).Precipitation;
                count++;
            }
            // Upper left
            if (leftX != -1 && upY != -1)
            {
                sum += currentMap.GetTile<Tile>(
                    new Vector3Int(leftX, upY, 0)).Precipitation;
                count++;
            }

            return sum / count;
        }

        // Calculate the probability of a river starting at the given corner
        // coordinates
        protected virtual float RiverProbability(int x, int y)
        {
            // Output arguments for slope function (not used)
            int outX, outY;

            return 4.0f *
                Mathf.Atan(parameters.RiverRainfallMultiplier *
                                  CornerPrecipitation(x, y)) *
                Mathf.Atan(parameters.RiverSlopeMultiplier * 
                           SteepestSlope(x, y, out outX, out outY)) /
                (Mathf.PI * Mathf.PI);
        }

        // Set an appropriate tile type and its sprite for each tile
        protected virtual void SelectBiomes()
        {
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));

                    // Find all valid tile types
                    List<TileType> possibleTypes = new List<TileType>();
                    foreach (TileType type in parameters.TileTypes)
                    {
                        if (type.ValuesInRanges(currentTile))
                        {
                            possibleTypes.Add(type);
                        }
                    }

                    if (possibleTypes.Count > 0)
                    {
                        // Randomly select a valid tile type
                        TileType selectedType =
                            possibleTypes[
                                Random.Range(0, possibleTypes.Count)];
                        currentTile.Type = selectedType;
                        currentTile.sprite = selectedType.Sprite;
                    }
                    else
                    {
                        // No valid tile type
                        currentTile.Type = null;
                        currentTile.sprite = parameters.InvalidSprite;
                    }
                }
            }
        }

        protected virtual void OnValidate()
        {
            // Validate the parameters
            parameters.Validate();
        }
    }
}