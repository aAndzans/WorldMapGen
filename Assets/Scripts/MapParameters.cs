using UnityEngine;

namespace WorldMapGen
{
    // User-specified properties for the map generator
    [System.Serializable]
    public class MapParameters
    {
        // If true, use a custom random seed
        [SerializeField]
        protected bool customSeed;
        public bool CustomSeed
        {
            get { return customSeed; }
            set { customSeed = value; }
        }

        // The custom random seed
        // If customSeed is false, this is ignored
        [SerializeField]
        protected int seed;
        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        // Number of tiles in X axis
        [SerializeField]
        protected int width;
        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        // Number of tiles in Y axis
        [SerializeField]
        protected int height;
        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        // Map scale in each axis (in km/tile)
        [SerializeField]
        protected Vector2 tileScale;
        public Vector2 TileScale
        {
            get { return tileScale; }
            set { tileScale = value; }
        }

        // Approximate portion (0-1) of the world that is below sea level
        [SerializeField]
        protected float oceanCoverage;
        public float OceanCoverage
        {
            get { return oceanCoverage; }
            set { oceanCoverage = value; }
        }

        // If true, the world rotates west (opposite the real Earth)
        [SerializeField]
        protected bool rotateWest;
        public bool RotateWest
        {
            get { return rotateWest; }
            set { rotateWest = value; }
        }

        // If true, the map wraps on the X axis
        [SerializeField]
        protected bool wrapX;
        public bool WrapX
        {
            get { return wrapX; }
            set { wrapX = value; }
        }

        // If true, the map wraps on the Y axis
        [SerializeField]
        protected bool wrapY;
        public bool WrapY
        {
            get { return wrapY; }
            set { wrapY = value; }
        }

        // Amount to scale the elevation noise function on X and Y
        // If this is 1, the longer dimension of the map corresponds to a
        // length of 1 in the noise function
        [SerializeField]
        protected float noiseScale;
        public float NoiseScale
        {
            get { return noiseScale; }
            set { noiseScale = value; }
        }

        // Every tile type that may be in the map
        [SerializeField]
        protected TileType[] tileTypes;
        public TileType[] TileTypes
        {
            get { return tileTypes; }
            set { tileTypes = value; }
        }

        // A latitude (in degrees) where pressure is high
        [SerializeField]
        protected float highPressureLatitude;
        public float HighPressureLatitude
        {
            get { return highPressureLatitude; }
            set { highPressureLatitude = value; }
        }

        // A latitude (in degrees) where pressure is low
        [SerializeField]
        protected float lowPressureLatitude;
        public float LowPressureLatitude
        {
            get { return lowPressureLatitude; }
            set { lowPressureLatitude = value; }
        }

        // Temperature at sea level at the equator (in °C)
        [SerializeField]
        protected float equatorTemperature;
        public float EquatorTemperature
        {
            get { return equatorTemperature; }
            set { equatorTemperature = value; }
        }

        // Temperature at sea level at the poles (in °C)
        [SerializeField]
        protected float poleTemperature;
        public float PoleTemperature
        {
            get { return poleTemperature; }
            set { poleTemperature = value; }
        }

        // Rate at which temperature decreases with elevation (in K/m)
        [SerializeField]
        protected float temperatureLapseRate;
        public float TemperatureLapseRate
        {
            get { return temperatureLapseRate; }
            set { temperatureLapseRate = value; }
        }

        // Maximum value of the term corresponding to the equator in the
        // formula for precipitation based on latitude
        [SerializeField]
        protected float equatorRainfall;
        public float EquatorRainfall
        {
            get { return equatorRainfall; }
            set { equatorRainfall = value; }
        }

        // The higher this value, the less precpitation decreases with distance
        // from the equator
        [SerializeField]
        protected float equatorRainfallEvenness;
        public float EquatorRainfallEvenness
        {
            get { return equatorRainfallEvenness; }
            set { equatorRainfallEvenness = value; }
        }

        // Maximum value of the term corresponding to lowPressureLatitude in
        // the formula for precipitation based on latitude
        [SerializeField]
        protected float midLatitudeRainfall;
        public float MidLatitudeRainfall
        {
            get { return midLatitudeRainfall; }
            set { midLatitudeRainfall = value; }
        }

        // The higher this value, the less precpitation decreases with distance
        // from lowPressureLatitude
        [SerializeField]
        protected float midLatitudeRainfallEvenness;
        public float MidLatitudeRainfallEvenness
        {
            get { return midLatitudeRainfallEvenness; }
            set { midLatitudeRainfallEvenness = value; }
        }

        // Distance from the ocean at which precpitation decreases e times
        [SerializeField]
        protected float rainfallOceanEFoldingDistance;
        public float RainfallOceanEFoldingDistance
        {
            get { return rainfallOceanEFoldingDistance; }
            set { rainfallOceanEFoldingDistance = value; }
        }

        // Multiplier used in the orographic precipitation formula
        // Combines several different constants
        [SerializeField]
        protected float condensationRateMultiplier;
        public float CondensationRateMultiplier
        {
            get { return condensationRateMultiplier; }
            set { condensationRateMultiplier = value; }
        }

        // Multiplier used in the exponent in the saturation vapour pressure
        // formula
        [SerializeField]
        protected float saturationPressureConst1;
        public float SaturationPressureConst1
        {
            get { return saturationPressureConst1; }
            set { saturationPressureConst1 = value; }
        }

        // Constant (in °C) added to the divisor in the exponent in the
        // saturation vapour pressure formula
        [SerializeField]
        protected float saturationPressureConst2;
        public float SaturationPressureConst2
        {
            get { return saturationPressureConst2; }
            set { saturationPressureConst2 = value; }
        }

        // Divisor (in K) used in the moisture scale height formula
        [SerializeField]
        protected float moistureScaleHeightDivisor;
        public float MoistureScaleHeightDivisor
        {
            get { return moistureScaleHeightDivisor; }
            set { moistureScaleHeightDivisor = value; }
        }

        // Sprite used for tiles that have no suitable tile type
        [SerializeField]
        protected Sprite invalidSprite;
        public Sprite InvalidSprite
        {
            get { return invalidSprite; }
            set { invalidSprite = value; }
        }
    }
}