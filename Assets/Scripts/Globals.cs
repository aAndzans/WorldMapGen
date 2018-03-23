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
    }
}
