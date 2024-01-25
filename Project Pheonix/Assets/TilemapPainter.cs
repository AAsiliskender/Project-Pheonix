using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapPainter : MonoBehaviour //RANDOM TILEPAINTER
{
    public Tilemap tilemap;
    public TileBase[] tiles;

    void Start()
    {
        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
        {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                int tileIndex = Random.Range(0, tiles.Length);
                tilemap.SetTile(tilePos, tiles[tileIndex]);
            }
        }
    }
}
