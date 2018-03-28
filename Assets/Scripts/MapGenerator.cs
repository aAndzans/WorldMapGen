﻿using System.Collections.Generic;
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

            map.size = new Vector3Int(parameters.Width, parameters.Height, 1);
            currentMap = map;
            scaledSize = ScaleCoords(parameters.Width, parameters.Height);

            CreateTiles();
            GenerateElevation();
            GenerateTemperature();
            GenerateRainfall();
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

        // Reduce every tile's precipitation based on its distance to the ocean
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

            // Visit all tiles
            while (frontier.Count > 0)
            {
                // Get the tile currently being visited
                Vector2Int coords = frontier.Dequeue();
                Tile currentTile = (Tile)currentMap.GetTile(
                    new Vector3Int(coords.x, coords.y, 0));

                // Adjust the current tile's precipitation
                currentTile.Precipitation /= Mathf.Exp(
                    Mathf.Sqrt(WrappingSqrDistance(
                                ScaleCoords(coords),
                                ScaleCoords(currentTile.NearestOcean))) / 
                    parameters.RainfallOceanEFoldingDistance);

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
        }

        // If the given neighbour is closer to nearestOcean than to its current
        // nearest ocean tile, update its nearest ocean tile
        // If the neighbour's nearest ocean tile is not known, set it to
        // nearestOcean and add the neighbour to frontier
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
            if (!neighbor.HasNearestOcean() ||
                WrappingSqrDistance(neighborKmCoords,
                                    ScaleCoords(nearestOcean)) <
                WrappingSqrDistance(neighborKmCoords,
                                    ScaleCoords(neighbor.NearestOcean)))
            {
                if (!neighbor.HasNearestOcean())
                {
                    frontier.Enqueue(neighborCoords);
                }
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