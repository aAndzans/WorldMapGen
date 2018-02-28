using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WorldMapGen
{
    // Class that procedurally generates a map
    public class MapGenerator : MonoBehaviour
    {
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

        // Number of tiles corresponding to 1 unit in the noise function
        protected float noiseScale;
        // Noise function offset
        protected Vector2 noiseOffset;

        // Amount to scale generated elevation
        protected float elevationScale;

        // Procedurally generate a map, storing it in the given tilemap
        public virtual void GenerateMap(Tilemap map)
        {
            map.size = new Vector3Int(parameters.Width, parameters.Height, 1);
            currentMap = map;

            GenerateInitialValues();
            AdjustRainfallForOceanDistance();
            AdjustOrographicRainfall();
            SelectBiomes();

            currentMap = null;
        }

        // Generate all tile properties that do not depend on other tiles
        protected virtual void GenerateInitialValues()
        {
            InitElevationGenerator();

            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile newTile = ScriptableObject.CreateInstance<Tile>();
                    // Generate elevation
                    newTile.Elevation = ElevationAtCoords(j, i);
                    // Place the tile in the map
                    currentMap.SetTile(new Vector3Int(j, i, 0), newTile);
                    Debug.Log(newTile.Elevation);
                }
            }
        }

        // Initialise the variables used for generating elevation
        protected virtual void InitElevationGenerator()
        {
            noiseScale = parameters.NoiseScale *
                         Mathf.Max(parameters.Width, parameters.Height);
            noiseOffset = new Vector2(Random.Range(0.0f, noiseMaxOffset),
                                      Random.Range(0.0f, noiseMaxOffset));

            // Elevation scale based on highest elevation among all tile types
            elevationScale = -Mathf.Infinity;
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
            elevationScale /= (1.0f - parameters.OceanPercentage);
        }

        // Use noise to generate an elevation corresponding to the given
        // coordinates
        protected virtual float ElevationAtCoords(int x, int y)
        {
            return (Mathf.PerlinNoise(
                        x / noiseScale + noiseOffset.x,
                        y / noiseScale + noiseOffset.y) -
                    parameters.OceanPercentage) * elevationScale;
        }

        // Calculate temperature based on latitude for the given Y coordinate
        protected virtual float TemperatureAtY(int y)
        {
            return 0.0f;
        }

        // Adjust the given temperature based on the given elevation
        protected virtual float TemperatureAtElevation(
            float temperature, float elevation)
        {
            return 0.0f;
        }

        // Calculate precipitation based on latitude for the given Y coordinate
        protected virtual float RainfallAtY(int y)
        {
            return 0.0f;
        }

        // Reduce the precipitation at all land tiles based on their distance
        // to the nearest ocean tile
        protected virtual void AdjustRainfallForOceanDistance()
        {

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
                        TileType selectedType = possibleTypes[
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