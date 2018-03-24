using UnityEngine;

namespace WorldMapGen
{
    // An individual tile in the generated map
    public class Tile : UnityEngine.Tilemaps.Tile
    {
        // The type of the tile
        public TileType Type { get; set; }

        // Metres above sea level
        public float Elevation { get; set; }

        // Annual average temperature in °C
        public float Temperature { get; set; }

        // Annual average precipitation in mm
        protected float precipitation;
        public float Precipitation
        {
            get { return precipitation; }
            // Limit precipitation to non-negative values
            set { precipitation = Mathf.Clamp(value, 0.0f, Mathf.Infinity); }
        }
    }
}