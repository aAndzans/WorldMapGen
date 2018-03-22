using UnityEngine;
using UnityEngine.Tilemaps;
using WorldMapGen;

public class WorldMapGenDemo : MonoBehaviour
{
	void Start()
    {
        // Generate a map at start and print its seed
        int generatorSeed =
            GetComponent<MapGenerator>().GenerateMap(GetComponent<Tilemap>());
        Debug.Log("Map seed: " + generatorSeed);
    }
}
