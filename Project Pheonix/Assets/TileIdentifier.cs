using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class TileIdentifier : MonoBehaviour
{
    public Tilemap tilemap; // The board on the grid itself
    
    public TMP_FontAsset font; // Text
    public int fontSize;
    public string text;

    // Start is called before the first frame update
    void Start()
    {
        float textWidth = 1;//text.Length * fontSize;
        float textHeight = 1;//fontSize;

        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
        {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TMP_Text textMesh = CreateTextMesh(tilePos, textWidth, textHeight);
                //tilemap.SetTransformMatrix(tilePos, textMesh.transform.localToWorldMatrix);
            }
        }
    }

    TMP_Text CreateTextMesh(Vector3Int tilePos, float textWidth, float textHeight)
    {
        GameObject textObject = new GameObject("Grid Coordinate"); //Text
        textObject.transform.parent = transform;

        TMP_Text textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.font = font;
        textMesh.fontSize = fontSize;
        textMesh.text = text;
        textMesh.color = Color.black;
        textMesh.alignment = TextAlignmentOptions.Center;

        // Size and Width of text
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(textWidth, textHeight);

        Vector3 worldPos = tilemap.CellToWorld(tilePos) + new Vector3(0.5f,0.25f,0.5f);
        textObject.transform.rotation =  Quaternion.Euler(90, 0, 0);
        textObject.transform.position = worldPos;

        textMesh.GetComponent<Renderer>().sortingLayerName = "Foreground";

        return textMesh;
    }
}
