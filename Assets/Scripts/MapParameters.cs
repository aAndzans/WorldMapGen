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

        [SerializeField]
        protected int seed;
        public int Seed
        {
            get { return seed; }
        }

        // Number of tiles in each axis
        [SerializeField]
        protected int width;
        public int Width
        {
            get { return width; }
        }

        [SerializeField]
        protected int height;
        public int Height
        {
            get { return height; }
        }

        [SerializeField]
        protected Vector2 tileScale;
        public Vector2 TileScale
        {
            get { return tileScale; }
        }

        [SerializeField]
        protected float oceanPercentage;
        public float OceanPercentage
        {
            get { return oceanPercentage; }
        }

        [SerializeField]
        protected bool rotateWest;
        public bool RotateWest
        {
            get { return rotateWest; }
        }

        [SerializeField]
        protected bool wrapX;
        public bool WrapX
        {
            get { return wrapX; }
        }

        [SerializeField]
        protected bool wrapY;
        public bool WrapY
        {
            get { return wrapY; }
        }

        [SerializeField]
        protected float noiseScale;
        public float NoiseScale
        {
            get { return noiseScale; }
        }

        [SerializeField]
        protected TileType[] tileTypes;
        public TileType[] TileTypes
        {
            get { return tileTypes; }
        }

        [SerializeField]
        protected float highPressureLatitude;
        public float HighPressureLatitude
        {
            get { return highPressureLatitude; }
        }

        [SerializeField]
        protected float lowPressureLatitude;
        public float LowPressureLatitude
        {
            get { return lowPressureLatitude; }
        }

        [SerializeField]
        protected float equatorTemperature;
        public float EquatorTemperature
        {
            get { return equatorTemperature; }
        }

        [SerializeField]
        protected float poleTemperature;
        public float PoleTemperature
        {
            get { return poleTemperature; }
        }

        [SerializeField]
        protected float temperatureLapseRate;
        public float TemperatureLapseRate
        {
            get { return temperatureLapseRate; }
        }

        [SerializeField]
        protected float equatorRainfall;
        public float EquatorRainfall
        {
            get { return equatorRainfall; }
        }

        [SerializeField]
        protected float equatorRainfallEvenness;
        public float EquatorRainfallEvenness
        {
            get { return equatorRainfallEvenness; }
        }

        [SerializeField]
        protected float midLatitudeRainfall;
        public float MidLatitudeRainfall
        {
            get { return midLatitudeRainfall; }
        }

        [SerializeField]
        protected float midLatitudeRainfallEvenness;
        public float MidLatitudeRainfallEvenness
        {
            get { return midLatitudeRainfallEvenness; }
        }

        [SerializeField]
        protected float rainfallOceanEFoldingDistance;
        public float RainfallOceanEFoldingDistance
        {
            get { return rainfallOceanEFoldingDistance; }
        }

        [SerializeField]
        protected float condensationRateMultiplier;
        public float CondensationRateMultiplier
        {
            get { return condensationRateMultiplier; }
        }

        [SerializeField]
        protected float saturationPressureExponentMultiplier;
        public float SaturationPressureExponentMultiplier
        {
            get { return saturationPressureExponentMultiplier; }
        }

        [SerializeField]
        protected float saturationPressureExponentDivisor;
        public float SaturationPressureExponentDivisor
        {
            get { return saturationPressureExponentDivisor; }
        }

        [SerializeField]
        protected float moistureScaleHeightDivisor;
        public float MoistureScaleHeightDivisor
        {
            get { return moistureScaleHeightDivisor; }
        }
    }
}