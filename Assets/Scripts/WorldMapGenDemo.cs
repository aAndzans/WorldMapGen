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

        Vector2 mapHalfSize = new Vector2(
            baseTilemap.size.x * baseTilemap.layoutGrid.cellSize.x,
            baseTilemap.size.y * baseTilemap.layoutGrid.cellSize.y) / 2.0f;
        // Move the camera to the centre of the map
        Camera.main.transform.position = new Vector3(
            mapHalfSize.x, mapHalfSize.y, Camera.main.transform.position.z);
        // Zoom the camera to fit the map
        if (mapHalfSize.x / Camera.main.aspect > mapHalfSize.y)
        {
            Camera.main.orthographicSize = mapHalfSize.x / Camera.main.aspect;
        }
        else
        {
            Camera.main.orthographicSize = mapHalfSize.y;
        }
    }
}
