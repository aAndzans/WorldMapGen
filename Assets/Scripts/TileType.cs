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
            set { min = value; }
        }

        // The maximum value
        [SerializeField]
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
            set { name = value; }
        }

        // Ranges of elevations (in m above sea level) where the type may
        // appear
        [SerializeField]
        protected Range[] elevation;
        public Range[] Elevation
        {
            get { return elevation; }
            set { elevation = value; }
        }

        // Ranges of temperatures (in °C) where the type may appear
        [SerializeField]
        protected Range[] temperature;
        public Range[] Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        // Ranges of precipitation amounts (in mm/year) where the type may
        // appear
        [SerializeField]
        protected Range[] precipitation;
        public Range[] Precipitation
        {
            get { return precipitation; }
            set { precipitation = value; }
        }

        // The sprite for this type
        [SerializeField]
        protected Sprite sprite;
        public Sprite Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }

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