using UnityEngine;

namespace WorldMapGen
{
    // User-specified properties for the map generator
    [System.Serializable]
    public class MapParameters
    {
        // Number of tiles in each axis
        [SerializeField]
        protected int width;
        public int Width
        {
            get { return width; }
            protected set { width = value; }
        }
        [SerializeField]
        protected int height;
        public int Height
        {
            get { return height; }
            protected set { height = value; }
        }
    }
}