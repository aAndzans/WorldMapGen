using UnityEngine;

namespace WorldMapGen
{
    // Minimum and maximum float value
    [System.Serializable]
    public struct Range
    {
        [SerializeField]
        [Tooltip("The lowest value in the range.")]
        private float min;
        public float Min
        {
            get { return min; }
            set { min = value; }
        }

        [SerializeField]
        [Tooltip("The highest value in the range.")]
        private float max;
        public float Max
        {
            get { return max; }
            set { max = value; }
        }

        // Return whether the given value is within this range
        public bool IsInRange(float value)
        {
            return value >= min && value <= max;
        }

        // Restrict max to be at least min and restrict the range within
        // [clampMin, clampMax]
        public void Validate(float clampMin = float.NegativeInfinity,
                             float clampMax = float.PositiveInfinity)
        {
            min = Mathf.Clamp(min, clampMin, clampMax);
            max = Mathf.Clamp(max, min, clampMax);
        }
    }

    // A type of tile and parameters that specify where it can be generated
    [System.Serializable]
    public class TileType
    {
        [SerializeField]
        [Tooltip("The name of the tile type.")]
        protected string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [SerializeField]
        [Tooltip("Range of possible elevations (in metres above sea level) " +
                 "for this tile type.")]
        [Warning(
            "ElevationIsPositiveAndNegative",
            "This type includes both positive and negative elevations. This " +
            "may cause unrealistic results because the generator treats " +
            "positive elevations as land and negative elevations as ocean.")]
        protected Range elevation;
        public Range Elevation
        {
            get { return elevation; }
            set { elevation = value; }
        }
        // If this type's elevation range includes both positive and negative
        // values, return true
        public virtual bool ElevationIsPositiveAndNegative()
        {
            return elevation.Min < 0.0f && elevation.Max > 0.0f;
        }

        [SerializeField]
        [Tooltip("Range of possible temperatures (in °C) for this tile type.")]
        protected Range temperature;
        public Range Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        [SerializeField]
        [Tooltip("Range of possible precipitation amounts (in millimetres " +
                 "per year) for this tile type.")]
        protected Range precipitation;
        public Range Precipitation
        {
            get { return precipitation; }
            set { precipitation = value; }
        }

        [SerializeField]
        [Tooltip("The sprite used to draw this tile type.")]
        protected Sprite sprite;
        public Sprite Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }

        // Return whether the given value is within any of the ranges in the
        // given array
        protected virtual bool ValueInAnyRange(Range[] ranges, float value)
        {
            foreach (Range range in ranges)
            {
                if (range.IsInRange(value))
                {
                    return true;
                }
            }
            return false;
        }

        // Return whether the tile's climate is within this type's ranges
        public virtual bool ValuesInRanges(Tile tile)
        {
            return
                elevation.IsInRange(tile.Elevation) &&
                temperature.IsInRange(tile.Temperature) &&
                precipitation.IsInRange(tile.Precipitation);
        }

        // Validate all ranges
        public virtual void Validate()
        {
            // Elevation must not be positive infinity because the highest
            // value determines actual elevation
            elevation.Validate(float.NegativeInfinity, float.MaxValue);
            temperature.Validate();
            precipitation.Validate();
        }
    }
}