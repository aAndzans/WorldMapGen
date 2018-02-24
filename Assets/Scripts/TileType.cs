using UnityEngine;

namespace WorldMapGen
{
    // Minimum and maximum float value
    [System.Serializable]
    public struct Range
    {
        // The minimum value
        [SerializeField]
        private float min;
        public float Min
        {
            get { return min; }
        }

        // The maximum value
        [SerializeField]
        private float max;
        public float Max
        {
            get { return max; }
        }

        // Return whether the given value is within this range
        public bool IsInRange(float value)
        {
            return value >= min && value <= max;
        }
    }

    // A type of tile and parameters that specify where it can be generated
    [System.Serializable]
    public class TileType
    {
        // The type's name
        [SerializeField]
        protected string name;
        public string Name
        {
            get { return name; }
        }

        // Ranges of elevations (m above sea level) where the type may appear
        [SerializeField]
        protected Range[] elevation;
        public Range[] Elevation
        {
            get { return elevation; }
        }

        // Ranges of temperatures (°C) where the type may appear
        [SerializeField]
        protected Range[] temperature;
        public Range[] Temperature
        {
            get { return temperature; }
        }

        // Ranges of precipitation amounts (mm/year) where the type may appear
        [SerializeField]
        protected Range[] precipitation;
        public Range[] Precipitation
        {
            get { return precipitation; }
        }

        // The sprite for this type
        [SerializeField]
        protected Sprite sprite;

        // If this type's elevation ranges include both positive and negative
        // values, return false
        public bool ElevationIsValid()
        {
            bool positive = false;  // Is the highest max value positive?
            bool negative = false;  // Is the lowest min value negative?

            foreach (Range range in elevation)
            {
                if (range.Min < 0.0f)
                {
                    negative = true;
                }

                if (range.Max > 0.0f)
                {
                    positive = true;
                }

                if (positive && negative)
                {
                    return false;
                }
            }
            return true;
        }

        // Return whether the given value is within any of the ranges in the
        // given array
        protected bool ValueInAnyRange(Range[] ranges, float value)
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

        // Return whether the given values for elevation, temperature and
        // precipitation are within this type's ranges
        public bool ValuesInRanges(
            float elevation, float temperature, float precipitation)
        {
            return 
                ValueInAnyRange(this.elevation, elevation) &&
                ValueInAnyRange(this.temperature, temperature) &&
                ValueInAnyRange(this.precipitation, precipitation);
        }
    }
}