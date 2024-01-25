using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class GridCoords : MonoBehaviour
{
    public Tilemap tilemap; // The board on the grid itself
    public Color outlineColor;
    public float outlineWidth;
    public Material material;
    public TMP_FontAsset font; // Text
    public int fontSize;

    // Start is called before the first frame update
    void Start()
    {
        float textWidth = 1;//text.Length * fontSize;
        float textHeight = 1;//fontSize;

        material.SetFloat("_OutlineWidth", outlineWidth); // adjust the value to your liking
        //material.SetFloat("_OutlineSoftness", 0.2f); // adjust the value to your liking
        material.SetColor("_OutlineColor", outlineColor);

        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
        {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            {
                if (x == 0 || y == 0)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0); //Makes new tile to overlay
                    TMP_Text textMesh = CreateTextMesh(tilePos, textWidth, textHeight, x, y, outlineColor);
                    //tilemap.SetTransformMatrix(tilePos, textMesh.transform.localToWorldMatrix);
                } 

            }
        }
    }

    TMP_Text CreateTextMesh(Vector3Int tilePos, float textWidth, float textHeight, int x, int y, Color outlineColor)
    {
        string textx;
        string texty;

        if (x<0) // Text of X position
        {
            textx = "-" + ((char)(System.Math.Abs(x+1)+'A')).ToString();
        } else {
            textx = ((char)(System.Math.Abs(x)+'A')).ToString();
        }

        texty = (y+1).ToString(); // Text of Y position

        if (x == 0 && y == 0) // Center tile has 2 coords in x and y -- (A, 1)
        {
            // The x coord tile
            GameObject textObjectx = new GameObject("Origin (X) Grid Coordinate (" + textx +","+ texty+")"); //Text
            textObjectx.transform.parent = transform;

            TMP_Text textMeshx = textObjectx.AddComponent<TextMeshPro>();
            textMeshx.font = font;
            textMeshx.fontSize = fontSize;
            textMeshx.fontStyle = FontStyles.Bold;
            textMeshx.color = Color.black;
            textMeshx.alignment = TextAlignmentOptions.Center;
            
            textMeshx.text = texty;

            // Size and Width of text
            RectTransform rectTransformx = textObjectx.GetComponent<RectTransform>();
            rectTransformx.sizeDelta = new Vector2(textWidth, textHeight);
            // Positioning and alignment
            Vector3 worldPosx = tilemap.CellToWorld(tilePos) + new Vector3(0f,0.25f,0.5f);
            textObjectx.transform.rotation = Quaternion.Euler(90, 0, 0);
            textObjectx.transform.position = worldPosx;
            // Rendering layer
            textMeshx.GetComponent<Renderer>().sortingLayerName = "Foreground";

            // The y coord tile   
            GameObject textObjecty = new GameObject("Origin (Y) Grid Coordinate (" + textx +","+ texty +")"); //Text
            textObjecty.transform.parent = transform;

            TMP_Text textMeshy = textObjecty.AddComponent<TextMeshPro>();
            textMeshy.font = font;
            textMeshy.fontSize = fontSize;
            textMeshy.fontStyle = FontStyles.Bold;
            textMeshy.color = Color.black;
            textMeshy.alignment = TextAlignmentOptions.Center;

            textMeshy.text = textx;

            // Size and Width of text
            RectTransform rectTransformy = textObjecty.GetComponent<RectTransform>();
            rectTransformy.sizeDelta = new Vector2(textWidth, textHeight);
            // Positioning and alignment
            Vector3 worldPosy = tilemap.CellToWorld(tilePos) + new Vector3(0.5f,0.25f,0f);
            textObjecty.transform.rotation =  Quaternion.Euler(90, 0, 0);
            textObjecty.transform.position = worldPosy;
            // Rendering layer
            textMeshy.GetComponent<Renderer>().sortingLayerName = "Foreground";

            return textMeshx;

        } else if (x==0) {  // Does 2 and more and 0 and less
            GameObject textObject = new GameObject("Grid Coordinate (" + textx +","+ texty +")"); //Text
            textObject.transform.parent = transform;

            TMP_Text textMesh = textObject.AddComponent<TextMeshPro>();
            textMesh.font = font;
            textMesh.fontSize = fontSize;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;

            textMesh.text = texty;

            // Size and Width of text
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(textWidth, textHeight);

            // Positioning and alignment
            Vector3 worldPos = tilemap.CellToWorld(tilePos) + new Vector3(0f,0.25f,0.5f);
            textObject.transform.rotation =  Quaternion.Euler(90, 0, 0);
            textObject.transform.position = worldPos;

            // Rendering layer
            textMesh.GetComponent<Renderer>().sortingLayerName = "Foreground";

            return textMesh;

        } else { // Does B and onwards and -A and backwards
            GameObject textObject = new GameObject("Grid Coordinate (" + textx +","+ texty+")"); //Text
            textObject.transform.parent = transform;

            TMP_Text textMesh = textObject.AddComponent<TextMeshPro>();
            textMesh.font = font;
            textMesh.fontSize = fontSize;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;

            textMesh.text = textx;

            // Size and Width of text
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(textWidth, textHeight);

            // Positioning and alignment
            Vector3 worldPos = tilemap.CellToWorld(tilePos) + new Vector3(0.5f,0.25f,0f);
            textObject.transform.rotation =  Quaternion.Euler(90, 0, 0);
            textObject.transform.position = worldPos;

            // Rendering layer
            textMesh.GetComponent<Renderer>().sortingLayerName = "Foreground";

            return textMesh;
        }

    }
    
}
