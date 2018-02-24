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
        }

        // The custom random seed
        // If customSeed is false, this is ignored
        [SerializeField]
        protected int seed;
        public int Seed
        {
            get { return seed; }
        }

        // Number of tiles in X axis
        [SerializeField]
        protected int width;
        public int Width
        {
            get { return width; }
        }

        // Number of tiles in Y axis
        [SerializeField]
        protected int height;
        public int Height
        {
            get { return height; }
        }

        // Map scale in each axis (in km/tile)
        [SerializeField]
        protected Vector2 tileScale;
        public Vector2 TileScale
        {
            get { return tileScale; }
        }

        // Approximate percentage of the world that is below sea level
        [SerializeField]
        protected float oceanPercentage;
        public float OceanPercentage
        {
            get { return oceanPercentage; }
        }

        // If true, the world rotates west (opposite the real Earth)
        [SerializeField]
        protected bool rotateWest;
        public bool RotateWest
        {
            get { return rotateWest; }
        }

        // If true, the map wraps on the X axis
        [SerializeField]
        protected bool wrapX;
        public bool WrapX
        {
            get { return wrapX; }
        }

        // If true, the map wraps on the Y axis
        [SerializeField]
        protected bool wrapY;
        public bool WrapY
        {
            get { return wrapY; }
        }

        // Amount to scale the elevation noise function on X and Y
        // If this is 1, the longer dimension of the map corresponds to a
        // length of 1 in the noise function
        [SerializeField]
        protected float noiseScale;
        public float NoiseScale
        {
            get { return noiseScale; }
        }

        // Every tile type that may be in the map
        [SerializeField]
        protected TileType[] tileTypes;
        public TileType[] TileTypes
        {
            get { return tileTypes; }
        }

        // A latitude (in degrees) where pressure is high
        [SerializeField]
        protected float highPressureLatitude;
        public float HighPressureLatitude
        {
            get { return highPressureLatitude; }
        }

        // A latitude (in degrees) where pressure is low
        [SerializeField]
        protected float lowPressureLatitude;
        public float LowPressureLatitude
        {
            get { return lowPressureLatitude; }
        }

        // Temperature at sea level at the equator (in °C)
        [SerializeField]
        protected float equatorTemperature;
        public float EquatorTemperature
        {
            get { return equatorTemperature; }
        }

        // Temperature at sea level at the poles (in °C)
        [SerializeField]
        protected float poleTemperature;
        public float PoleTemperature
        {
            get { return poleTemperature; }
        }

        // Rate at which temperature decreases with elevation (in K/m)
        [SerializeField]
        protected float temperatureLapseRate;
        public float TemperatureLapseRate
        {
            get { return temperatureLapseRate; }
        }

        // Maximum value of the term corresponding to the equator in the
        // formula for precipitation based on latitude
        [SerializeField]
        protected float equatorRainfall;
        public float EquatorRainfall
        {
            get { return equatorRainfall; }
        }

        // The higher this value, the less precpitation decreases with distance
        // from the equator
        [SerializeField]
        protected float equatorRainfallEvenness;
        public float EquatorRainfallEvenness
        {
            get { return equatorRainfallEvenness; }
        }

        // Maximum value of the term corresponding to lowPressureLatitude in
        // the formula for precipitation based on latitude
        [SerializeField]
        protected float midLatitudeRainfall;
        public float MidLatitudeRainfall
        {
            get { return midLatitudeRainfall; }
        }

        // The higher this value, the less precpitation decreases with distance
        // from lowPressureLatitude
        [SerializeField]
        protected float midLatitudeRainfallEvenness;
        public float MidLatitudeRainfallEvenness
        {
            get { return midLatitudeRainfallEvenness; }
        }

        // Distance from the ocean at which precpitation decreases e times
        [SerializeField]
        protected float rainfallOceanEFoldingDistance;
        public float RainfallOceanEFoldingDistance
        {
            get { return rainfallOceanEFoldingDistance; }
        }

        // Multiplier used in the orographic precipitation formula
        // Combines several different constants
        [SerializeField]
        protected float condensationRateMultiplier;
        public float CondensationRateMultiplier
        {
            get { return condensationRateMultiplier; }
        }

        // Multiplier used in the exponent in the saturation vapour pressure
        // formula
        [SerializeField]
        protected float saturationPressureExponentMultiplier;
        public float SaturationPressureExponentMultiplier
        {
            get { return saturationPressureExponentMultiplier; }
        }

        // Constant (in °C) added to the divisor in the exponent in the
        // saturation vapour pressure formula
        [SerializeField]
        protected float saturationPressureExponentDivisorTerm;
        public float SaturationPressureExponentDivisorTerm
        {
            get { return saturationPressureExponentDivisorTerm; }
        }

        // Divisor (in K) used in the moisture scale height formula
        [SerializeField]
        protected float moistureScaleHeightDivisor;
        public float MoistureScaleHeightDivisor
        {
            get { return moistureScaleHeightDivisor; }
        }
    }
}