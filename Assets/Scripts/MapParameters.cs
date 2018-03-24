using UnityEngine;

namespace WorldMapGen
{
    // User-specified properties for the map generator
    [System.Serializable]
    public class MapParameters
    {
        // Attribute that causes a property to show a warning when a condition
        // is met
        public class WarningAttribute : PropertyAttribute
        {
            // Name of the function that is the condition for the warning (must
            // return bool and take no parameters)
            public string ConditionFunction { get; private set; }
            // The text of the warning message
            public string Message { get; private set; }

            public WarningAttribute(string conditionFunction, string message)
            {
                ConditionFunction = conditionFunction;
                Message = message;
            }
        }

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
        [Range(0.0f, 1.0f)]
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
        [Range(0.0f, 90.0f)]
        protected float highPressureLatitude;
        public float HighPressureLatitude
        {
            get { return highPressureLatitude; }
            set { highPressureLatitude = value; }
        }

        // A latitude (in degrees) where pressure is low
        [SerializeField]
        [Range(0.0f, 90.0f)]
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

        // Rate at which temperature decreases with elevation (in K/m)
        [SerializeField]
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

        // Multiplier used in the orographic precipitation formula
        // Combines several different constants
        [SerializeField]
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

        // Restrict parameters to valid values
        public virtual void Validate()
        {
            // Map size must be positive
            width = Mathf.Clamp(width, 1, int.MaxValue);
            height = Mathf.Clamp(height, 1, int.MaxValue);

            // Tile scale must be positive and finite
            tileScale.x = Mathf.Clamp(
                tileScale.x, float.Epsilon, float.MaxValue);
            tileScale.y = Mathf.Clamp(
                tileScale.y, float.Epsilon, float.MaxValue);

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