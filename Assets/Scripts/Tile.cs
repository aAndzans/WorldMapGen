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
        public float Precipitation { get; set; }

        // Distance to nearest ocean tile in km
        public float OceanDistance { get; set; }
    }
}