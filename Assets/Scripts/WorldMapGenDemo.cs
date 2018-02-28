using UnityEngine;
using UnityEngine.Tilemaps;
using WorldMapGen;

public class WorldMapGenDemo : MonoBehaviour
{
	void Start()
    {
        // Generate a map at start
        GetComponent<MapGenerator>().GenerateMap(GetComponent<Tilemap>());
	}
}
