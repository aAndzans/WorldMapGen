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

        // Coordinates of every ocean tile in km
        protected List<Vector2> oceanCoords;

        // Procedurally generate a map, storing it in the given tilemap
        public virtual void GenerateMap(Tilemap map)
        {
            map.size = new Vector3Int(parameters.Width, parameters.Height, 1);
            currentMap = map;
            oceanCoords = new List<Vector2>();

            CreateTiles();
            GenerateElevation();
            GenerateTemperature();
            GenerateRainfall();
            SelectBiomes();

            currentMap.RefreshAllTiles();
            currentMap = null;
            oceanCoords.Clear();
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

        // Scale the given coordinates to km
        protected virtual Vector2 ScaleCoords(int x, int y)
        {
            return new Vector2(x * parameters.TileScale.x,
                               y * parameters.TileScale.y);
        }

        // Generate elevation for every tile
        protected virtual void GenerateElevation()
        {
            float noiseScale = parameters.NoiseScale *
                               Mathf.Max(parameters.Width, parameters.Height);
            Vector2 noiseOffset =
                new Vector2(Random.Range(0.0f, noiseMaxOffset),
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
                    currentTile.Elevation =
                        Mathf.PerlinNoise(j / noiseScale + noiseOffset.x,
                                          i / noiseScale + noiseOffset.y);

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
            float elevationScale = -Mathf.Infinity;
            foreach (TileType type in parameters.TileTypes)
            {
                foreach (Range range in type.Elevation)
                {
                    if (range.Max > elevationScale)
                    {
                        elevationScale = range.Max;
                    }
                }
            }
            // Also scale based on maximum value of unscaled heightmap
            elevationScale /= 1.0f - oceanOffset;

            // Offset and scale all elevations
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));
                    currentTile.Elevation -= oceanOffset;
                    currentTile.Elevation *= elevationScale;

                    // Store coordinates of all ocean tiles
                    if (currentTile.Elevation <= 0.0f)
                    {
                        oceanCoords.Add(ScaleCoords(j, i));
                    }
                }
            }
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
                    currentTile.Temperature =
                        TemperatureAtElevation(latitudeTemperature,
                                               currentTile.Elevation);
                }
            }
        }

        // Generate precipitation for every tile
        protected virtual void GenerateRainfall()
        {
            for (int i = 0; i < parameters.Height; i++)
            {
                float latitudeRainfall = RainfallAtY(i);

                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile currentTile =
                        (Tile)currentMap.GetTile(new Vector3Int(j, i, 0));
                    currentTile.Precipitation = latitudeRainfall;

                    // For land tiles, reduce precipitation based on their
                    // distance to the ocean
                    if (currentTile.Elevation > 0.0f)
                    {
                        currentTile.Precipitation /=
                            RainfallOceanDistanceRatio(j, i);
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
                (latitude - parameters.LowPressureLatitude) /
                parameters.MidLatitudeRainfallEvenness;
            float midLatitudeSquareBase2 =
                (latitude + parameters.LowPressureLatitude) /
                parameters.MidLatitudeRainfallEvenness;

            return parameters.MidLatitudeRainfall /
                   (1.0f + midLatitudeSquareBase1 * midLatitudeSquareBase1) +
                   parameters.EquatorRainfall /
                   (1.0f + equatorSquareBase * equatorSquareBase) +
                   parameters.MidLatitudeRainfall /
                   (1.0f + midLatitudeSquareBase2 * midLatitudeSquareBase2);
        }

        // Return the ratio by which to divide a tile's precipitation based on
        // its distance to the nearest ocean tile
        protected virtual float RainfallOceanDistanceRatio(int x, int y)
        {
            float shortestSqDist = Mathf.Infinity;
            Vector2 currentTile = ScaleCoords(x, y);

            // Find the square distance to the nearest ocean tile
            foreach (Vector2 oceanTile in oceanCoords)
            {
                float sqDist = (oceanTile - currentTile).sqrMagnitude;
                if (sqDist < shortestSqDist)
                {
                    shortestSqDist = sqDist;
                }
            }

            return Mathf.Exp(Mathf.Sqrt(shortestSqDist) /
                             parameters.RainfallOceanEFoldingDistance);
        }

        // Adjust the precipitation at all land tiles based on the effects of
        // wind and elevation
        protected virtual void AdjustOrographicRainfall()
        {

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
    }
}