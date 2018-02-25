using UnityEngine;
using UnityEngine.Tilemaps;

namespace WorldMapGen
{
    // Class that procedurally generates a map
    public class MapGenerator : MonoBehaviour
    {
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

        protected virtual void GenerateHeightmap()
        {
            float perlinScale = Mathf.Max(parameters.Width, parameters.Height);
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    Tile newTile = new Tile();
                    currentMap.SetTile(new Vector3Int(j, i, 0), newTile);
                    newTile.Elevation = Mathf.PerlinNoise(i / perlinScale, j / perlinScale);
                    Debug.Log(newTile.Elevation);
                }
            }
        }

        // Procedurally generate a map, storing it in the given tilemap
        public virtual void GenerateMap(Tilemap map)
        {
            currentMap = map;
            GenerateHeightmap();
        }

        // Generate all tile properties that do not depend on other tiles
        protected virtual void GenerateInitialValues()
        {

        }

        // Use noise to generate an elevation corresponding to the given
        // coordinates
        protected virtual float ElevationAtCoords(int x, int y)
        {

        }

        // Calculate temperature based on latitude for the given Y coordinate
        protected virtual float TemperatureAtY(int y)
        {

        }

        // Adjust the given temperature based on the given elevation
        protected virtual float TemperatureAtElevation(
            float temperature, float elevation)
        {

        }

        // Calculate precipitation based on latitude for the given Y coordinate
        protected virtual float RainfallAtY(int y)
        {

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
    }
}