using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridScan : MonoBehaviour
{
    public Tilemap tilemap;
    public int xcoord;
    public int ycoord;
    public Sprite sprite_used;

    // Start is called before the first frame update
    void Start()
    {
        

        Debug.Log(tilemap);
        Debug.Log(tilemap.cellBounds);
        Debug.Log(tilemap.cellBounds.max.x);
        Debug.Log(tilemap.cellBounds.max.y);
        Debug.Log(tilemap.cellBounds.min.x);
        Debug.Log(tilemap.cellBounds.min.y);


        //Tilemap tilemap = GameObject.Find("TilemapTest").GetComponent<Tilemap>();
        tilemap.GetComponent<Tilemap>();
        Vector3Int tilePosition = new Vector3Int(xcoord, ycoord, 0);
        TileBase tile = tilemap.GetTile(tilePosition);

        if (tile != null) {
            Debug.Log("There is a tile at position (" + xcoord + ", " + ycoord + ")");
        } else {
            Debug.Log("There is no tile at position (" + xcoord + ", " + ycoord + ")");
        }

        GameObject search = new GameObject("spriteesss");
        search.AddComponent<SpriteRenderer>();
        SpriteRenderer spriterenderer = search.GetComponent<SpriteRenderer>();
        spriterenderer.sprite = sprite_used;

        search.transform.rotation =  Quaternion.Euler(90, 0, 0);
        search.transform.position = new Vector3((float)xcoord + 0.5f, 0.1f, (float)ycoord + 0.5f);
        search.GetComponent<Renderer>().sortingLayerName = "Foreground";

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
