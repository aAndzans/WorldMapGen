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
        protected float temperature;
        public float Temperature
        {
            get { return temperature; }
            // Limit temperature above absolute zero
            set
            {
                temperature = Mathf.Clamp(
                    value, Globals.MinTemperature, float.PositiveInfinity);
            }
        }

        // Annual average precipitation in mm
        protected float precipitation;
        public float Precipitation
        {
            get { return precipitation; }
            // Limit precipitation to non-negative values
            set
            {
                precipitation = Mathf.Clamp(
                    value, 0.0f, float.PositiveInfinity);
            }
        }

        // Tile coordinates of nearest ocean tile
        // (-1,-1) means nearest ocean tile is unknown
        public Vector2Int NearestOcean { get; set; }

        public Tile() : base()
        {
            // Nearest ocean tile is initially unknown
            NearestOcean = new Vector2Int(-1, -1);
        }

        // Return true if this tile has a known nearest ocean tile
        public bool HasNearestOcean()
        {
            return NearestOcean.x != -1 && NearestOcean.y != -1;
        }
    }
}