using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridPainter : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase[] tiles;


    // Here we paint tiles at start.
    // We will paint based on environment and other stuff.
    void Start()
    {

        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
        {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                //Colours every other tile differently (tile 0 or tile 1)
                int tileIndex = ((System.Math.Abs(x%2) + System.Math.Abs(y%2))%2);
                
                tilemap.SetTile(tilePos, tiles[tileIndex]);
                // SET COLOUR OF TILES MANUALLY 
                //Tilemap.Colours
            }
        }
    }



}
