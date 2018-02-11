using UnityEngine;

namespace WorldMapGen
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField]
        protected MapParameters parameters;

        // Map currently being generated
        protected Tile[,] generatedMap;

        protected virtual void GenerateHeightmap()
        {
            float perlinScale = Mathf.Max(parameters.Width, parameters.Height);
            for (int i = 0; i < parameters.Height; i++)
            {
                for (int j = 0; j < parameters.Width; j++)
                {
                    generatedMap[i, j] = new Tile();
                    generatedMap[i, j].Height = Mathf.PerlinNoise(i / perlinScale, j / perlinScale);
                    Debug.Log(generatedMap[i, j].Height);
                }
            }
        }

        public virtual Tile[,] Generate()
        {
            generatedMap = new Tile[parameters.Height, parameters.Width];
            GenerateHeightmap();
            return generatedMap;
        }

        public void GenerateTest()
        {
            Generate();
        }
    }
}