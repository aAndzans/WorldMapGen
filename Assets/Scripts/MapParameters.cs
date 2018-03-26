using UnityEngine;

namespace WorldMapGen
{
    // User-specified properties for the map generator
    [System.Serializable]
    public class MapParameters
    {
        [SerializeField]
        [Tooltip("If true, use Seed as the random seed for the generator.")]
        protected bool customSeed;
        public bool CustomSeed
        {
            get { return customSeed; }
            set { customSeed = value; }
        }

        [SerializeField]
        [Tooltip("A custom random seed for the generator. If CustomSeed is " +
                 "false, this is ignored.")]
        protected int seed;
        public int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        [SerializeField]
        [Tooltip("Number of tiles in the X axis.")]
        protected int width;
        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        [SerializeField]
        [Tooltip("Number of tiles in the Y axis.")]
        protected int height;
        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        [SerializeField]
        [Tooltip("Kilometers per tile in each dimension.")]
        protected Vector2 tileScale;
        public Vector2 TileScale
        {
            get { return tileScale; }
            set { tileScale = value; }
        }

        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Approximate portion of the world below sea level.")]
        protected float oceanCoverage;
        public float OceanCoverage
        {
            get { return oceanCoverage; }
            set { oceanCoverage = value; }
        }

        [SerializeField]
        [Tooltip("The real Earth rotates east. If this is true, the world " +
                 "will instead rotate west, which will flip wind directions.")]
        protected bool rotateWest;
        public bool RotateWest
        {
            get { return rotateWest; }
            set { rotateWest = value; }
        }

        [SerializeField]
        [Tooltip("If this is true, the map will wrap on the X axis.")]
        protected bool wrapX;
        public bool WrapX
        {
            get { return wrapX; }
            set { wrapX = value; }
        }

        [SerializeField]
        [Tooltip("If this is true, the map will wrap on the Y axis.")]
        protected bool wrapY;
        public bool WrapY
        {
            get { return wrapY; }
            set { wrapY = value; }
        }

        [SerializeField]
        [Tooltip("Amount by which to scale the elevation noise function on " +
                 "X and Y. If this is 1, the longer dimension of the map " +
                 "(in kilometers) corresponds to 1 unit in the noise " +
                 "function.")]
        protected float noiseScale;
        public float NoiseScale
        {
            get { return noiseScale; }
            set { noiseScale = value; }
        }

        [SerializeField]
        [Tooltip("Array of all possible tile types.")]
        protected TileType[] tileTypes;
        public TileType[] TileTypes
        {
            get { return tileTypes; }
            set { tileTypes = value; }
        }

        [SerializeField]
        [Range(0.0f, 90.0f)]
        [Tooltip("A latitude on both sides of the equator (in degrees) " +
                 "where pressure is high. The wind direction changes at " +
                 "this latitude.")]
        protected float highPressureLatitude;
        public float HighPressureLatitude
        {
            get { return highPressureLatitude; }
            set { highPressureLatitude = value; }
        }

        [SerializeField]
        [Range(0.0f, 90.0f)]
        [Tooltip("A latitude on both sides of the equator (in degrees) " +
                 "where pressure is low. The wind direction changes at " +
                 "this latitude. Precipitation is higher than at nearby " +
                 "latitudes.")]
        protected float lowPressureLatitude;
        public float LowPressureLatitude
        {
            get { return lowPressureLatitude; }
            set { lowPressureLatitude = value; }
        }

        [SerializeField]
        [Tooltip("Temperature at sea level at the equator (in °C).")]
        protected float equatorTemperature;
        public float EquatorTemperature
        {
            get { return equatorTemperature; }
            set { equatorTemperature = value; }
        }

        [SerializeField]
        [Tooltip("Temperature at sea level at the poles (in °C).")]
        [Warning("PoleIsWarmerThanEquator",
                 "Pole temperature is greater than equator temperature.")]
        protected float poleTemperature;
        public float PoleTemperature
        {
            get { return poleTemperature; }
            set { poleTemperature = value; }
        }
        // Return true if pole temperature is greater than equator temperature
        public virtual bool PoleIsWarmerThanEquator()
        {
            return poleTemperature > equatorTemperature;
        }

        [SerializeField]
        [Tooltip("Rate at which temperature decreases with elevation (in " +
                 "kelvins/metre).")]
        [Warning(
            "TemperatureLapseRateIsNegative",
            "Temperature lapse rate is negative. This will cause the " +
            "relationship between elevation and temperature to be the " +
            "inverse of what it is in the real world.")]
        protected float temperatureLapseRate;
        public float TemperatureLapseRate
        {
            get { return temperatureLapseRate; }
            set { temperatureLapseRate = value; }
        }
        // Return true if temperature lapse rate is negative
        public virtual bool TemperatureLapseRateIsNegative()
        {
            return temperatureLapseRate < 0.0f;
        }

        [SerializeField]
        [Tooltip("When calculating precipitation based on latitude, this " +
                 "determines precipitation at the equator.")]
        protected float equatorRainfall;
        public float EquatorRainfall
        {
            get { return equatorRainfall; }
            set { equatorRainfall = value; }
        }

        [SerializeField]
        [Tooltip("The higher this value, the less precpitation decreases " +
                 "with distance from the equator.")]
        protected float equatorRainfallEvenness;
        public float EquatorRainfallEvenness
        {
            get { return equatorRainfallEvenness; }
            set { equatorRainfallEvenness = value; }
        }

        [SerializeField]
        [Tooltip("When calculating precipitation based on latitude, this " +
                 "determines precipitation at LowPressureLatitude.")]
        protected float midLatitudeRainfall;
        public float MidLatitudeRainfall
        {
            get { return midLatitudeRainfall; }
            set { midLatitudeRainfall = value; }
        }

        [SerializeField]
        [Tooltip("The higher this value, the less precpitation decreases " +
                 "with distance from LowPressureLatitude.")]
        protected float midLatitudeRainfallEvenness;
        public float MidLatitudeRainfallEvenness
        {
            get { return midLatitudeRainfallEvenness; }
            set { midLatitudeRainfallEvenness = value; }
        }

        [SerializeField]
        [Tooltip("Distance from the ocean (in kilometers) at which " +
                 "precpitation decreases e times.")]
        [Warning(
            "RainfallOceanEFoldingDistanceIsNegative",
            "Precipitation e-folding distance from the ocean is negative. " +
            "This will cause the relationship between distance from the " +
            "ocean and precipitation to be the inverse of what it is in the " +
            "real world.")]
        protected float rainfallOceanEFoldingDistance;
        public float RainfallOceanEFoldingDistance
        {
            get { return rainfallOceanEFoldingDistance; }
            set { rainfallOceanEFoldingDistance = value; }
        }
        // Return true if the e-folding distance is negative
        public virtual bool RainfallOceanEFoldingDistanceIsNegative()
        {
            return rainfallOceanEFoldingDistance < 0.0f;
        }

        [SerializeField]
        [Tooltip("Multiplier for orographic precipitation (the effect of " +
                 "wind and elevation on precipitation).")]
        [Warning(
            "CondensationRateMultiplierIsNegative",
            "Condensation rate multiplier is negative. This will cause the " +
            "effects of orographic precipitation to be the inverse of what " +
            "they are in the real world.")]
        protected float condensationRateMultiplier;
        public float CondensationRateMultiplier
        {
            get { return condensationRateMultiplier; }
            set { condensationRateMultiplier = value; }
        }
        // Return true if the condensation rate multiplier is negative
        public virtual bool CondensationRateMultiplierIsNegative()
        {
            return condensationRateMultiplier < 0.0f;
        }

        [SerializeField]
        [Tooltip("Constant used in the formula for saturation vapour " +
                 "pressure. Increases orographic precipitation.")]
        protected float saturationPressureConst1;
        public float SaturationPressureConst1
        {
            get { return saturationPressureConst1; }
            set { saturationPressureConst1 = value; }
        }

        [SerializeField]
        [Tooltip("Constant used in the formula for saturation vapour " +
                 "pressure. Decreases orographic precipitation.")]
        protected float saturationPressureConst2;
        public float SaturationPressureConst2
        {
            get { return saturationPressureConst2; }
            set { saturationPressureConst2 = value; }
        }

        [SerializeField]
        [Tooltip("Constant used in the formula for moisture scale height. " +
                 "Decreases orographic precipitation.")]
        protected float moistureScaleHeightDivisor;
        public float MoistureScaleHeightDivisor
        {
            get { return moistureScaleHeightDivisor; }
            set { moistureScaleHeightDivisor = value; }
        }

        [SerializeField]
        [Tooltip("Sprite used for tiles whose elevation, temperature and " +
                 "precipitation do not correspond to any of the types in " +
                 "TileTypes.")]
        protected Sprite invalidSprite;
        public Sprite InvalidSprite
        {
            get { return invalidSprite; }
            set { invalidSprite = value; }
        }

        // Restrict parameters to valid values
        public virtual void Validate()
        {
            // Map size must be positive
            width = Mathf.Clamp(width, 1, int.MaxValue);
            height = Mathf.Clamp(height, 1, int.MaxValue);

            // Tile scale must be positive, and the total length in
            // each dimension must be finite
            tileScale.x = Mathf.Clamp(
                tileScale.x, float.Epsilon, float.MaxValue / width);
            tileScale.y = Mathf.Clamp(
                tileScale.y, float.Epsilon, float.MaxValue / height);

            // Ocean coverage must be at least 0 (all land) and less than 1
            oceanCoverage = Mathf.Clamp(
                oceanCoverage, 0.0f, 1.0f - 1.0f / (width * height));

            // Validate all tile types
            foreach (TileType type in tileTypes) type.Validate();

            // High/low pressure latitudes must be from 0 to 90°
            highPressureLatitude = Mathf.Clamp(
                highPressureLatitude, 0.0f, 90.0f);
            // The low pressure latitude is also no less than the high pressure
            // latitude
            lowPressureLatitude = Mathf.Clamp(
                lowPressureLatitude, highPressureLatitude, 90.0f);

            // Temperatures must be above absolute zero and finite
            equatorTemperature = Mathf.Clamp(
                equatorTemperature, Globals.MinTemperature, float.MaxValue);
            poleTemperature = Mathf.Clamp(
                poleTemperature, Globals.MinTemperature, float.MaxValue);

            // Rainfall must not be negative
            equatorRainfall = Mathf.Clamp(
                equatorRainfall, 0.0f, float.PositiveInfinity);
            midLatitudeRainfall = Mathf.Clamp(
                midLatitudeRainfall, 0.0f, float.PositiveInfinity);

            // The rainfall evenness variables must not be 0 to avoid divide by
            // 0
            // Because they are eventually squared, there is no need to allow
            // negative values
            equatorRainfallEvenness = Mathf.Clamp(
                equatorRainfallEvenness, float.Epsilon,
                float.PositiveInfinity);
            midLatitudeRainfallEvenness = Mathf.Clamp(
                midLatitudeRainfallEvenness, float.Epsilon,
                float.PositiveInfinity);

            // e-folding distance must not be 0 to avoid divide by 0
            if (rainfallOceanEFoldingDistance == 0.0f)
                rainfallOceanEFoldingDistance = float.Epsilon;

            // If saturationPressureConst2 is in the range between
            // poleTemperature and equatorTemperature, divide by 0 is possible
            float lowTemperature = Mathf.Min(
                equatorTemperature, poleTemperature);
            float highTemperature = Mathf.Max(
                equatorTemperature, poleTemperature);
            if (saturationPressureConst2 >= lowTemperature &&
                saturationPressureConst2 <= highTemperature)
            {
                // Set value just above the range
                saturationPressureConst2 =
                    Globals.IncrementFloat(highTemperature);
            }
        }
    }
}