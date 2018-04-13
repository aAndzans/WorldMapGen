namespace WorldMapGen
{
    // Class containing global constants and functions
    public static class Globals
    {
        // Difference between temperatures in °C and K
        public static float CelsiusToKelvin { get { return 273.15f; } }

        // Number of metres in a kilometre
        public static float KmToM { get { return 1000.0f; } }

        // Smallest allowed temperature in °C
        private static readonly float minTemperature;
        public static float MinTemperature { get { return minTemperature; } }

        static Globals()
        {
            // Set minimum temperature just above absolute zero
            minTemperature = IncrementFloat(-CelsiusToKelvin);
        }

        // Return the smallest float greater than x
        public static float IncrementFloat(float x)
        {
            // Do not change NaN or infinity
            if (float.IsNaN(x) || float.IsInfinity(x)) return x;
            // Incrementing bits will not work for 0
            if (x == 0.0f) return float.Epsilon;

            // Store bits in int
            byte[] bytes = System.BitConverter.GetBytes(x);
            int xInt = System.BitConverter.ToInt32(bytes, 0);

            // Increment or decrement bits depending on sign
            if (x > 0.0f) xInt++;
            else xInt--;

            // Convert new value back to float
            bytes = System.BitConverter.GetBytes(xInt);
            return System.BitConverter.ToSingle(bytes, 0);
        }

        // If coord is within the range [0,max), return coord
        // If it is outside that range and wrap is false, return -1
        // Otherwise return coord % max
        public static int WrappedCoord(int coord, int max, bool wrap)
        {
            if (coord < 0 || coord >= max)
            {
                if (wrap) return coord % max;
                return -1;
            }
            return coord;
        }
    }
}
