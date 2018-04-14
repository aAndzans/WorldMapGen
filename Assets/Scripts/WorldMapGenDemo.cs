using UnityEngine;
using UnityEngine.Tilemaps;
using WorldMapGen;

public class WorldMapGenDemo : MonoBehaviour
{
    // Tilemap to contain land and ocean tiles
    [SerializeField]
    private Tilemap baseTilemap;

    // Tilemap to contain river tiles (should be offset by half of tile size to
    // the lower left)
    [SerializeField]
    private Tilemap riverTilemap;

    void Start()
    {
        // Generate a map at start and print its seed
        int generatorSeed = GetComponent<MapGenerator>().GenerateMap(
            baseTilemap, riverTilemap);
        Debug.Log("Map seed: " + generatorSeed);
    }
}
