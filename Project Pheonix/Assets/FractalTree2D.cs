using System.Collections;
using System.Linq;
using UnityEngine;

public class FractalTree2D : MonoBehaviour
{
    public int maxIterations = 5;
    public float lengthRatio = 0.7f;
    public float angle = 30f;
    public float startAngleSpread = 150f;
    public Material material;

    private int currentNodeID = 0;
    private int currentNodeTier = 0;
    private int currentNodeOptionID = 0;

    public Mesh mesh2d;
    private Vector3[] vertices;
    private int[] indices;
    private int currentIndex;

    void Start()
    {
        mesh2d = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().material = material;

        Vector3 startPos = transform.position + new Vector3(6,6,6) * 5f;
        Vector3 endPos = startPos + Vector3.up * 5f; // Initial vertical trunk

        DrawFractalTree2D(startPos, endPos, maxIterations);

        UpdateMesh2D();
    }

    void DrawFractalTree2D(Vector3 start, Vector3 end, int iterations)
    {
        if (iterations <= 0)
            return;

        AddVertex2D(start);
        AddVertex2D(end);

        Vector3 direction = end - start;
        Vector3 newEnd1 = end + Quaternion.Euler(angle, 0, 0) * direction * lengthRatio;
        Vector3 newEnd2 = end + Quaternion.Euler(-angle, 0, 0) * direction * lengthRatio;

        DrawFractalTree2D(end, newEnd1, iterations - 1);
        DrawFractalTree2D(end, newEnd2, iterations - 1);
    }

    void DrawSecondaryLine(Vector3 start, Vector3 end)
    {
        // Implement your code to draw secondary lines (dashed lines) here
    }


    //float branchInnerAngle = (iterations == maxIterations) ? 30 : (i-Mathf.FloorToInt(numBranches/2)) * 5;

    // Adjust the branchCrossAngle for the initial split to form a tetrahedral arrangement
    //float branchCrossAngle = (iterations == maxIterations) ? i*120f : (i-Mathf.FloorToInt(numBranches/2)) * 0;

    int CalculateBranchCount2D(int remainingIterations)
    {
        // You can implement your logic here to dynamically calculate the number of branches based on the state
        // For simplicity, I'm using a linear relationship here. You might want to replace this with your own logic.
        if(remainingIterations == maxIterations){return 4;}

        return 2;
        //return Mathf.Max(2, maxIterations - remainingIterations + 2);
    }

    void AddVertex2D(Vector3 vertex)
    {
        if (vertices == null || currentIndex >= vertices.Length)
        {
            // Expand the arrays as needed
            int newSize = (vertices != null) ? vertices.Length * 2 : 4;
            System.Array.Resize(ref vertices, newSize);
        }

        vertices[currentIndex++] = vertex;
    }

    void UpdateMesh2D()
    {
        mesh2d.Clear();
        mesh2d.vertices = vertices;

        // Create an index array for the Lines topology
        int[] indices = new int[currentIndex];
        for (int i = 0; i < currentIndex; i++)
        {
            indices[i] = i;
        }

        mesh2d.SetIndices(indices, MeshTopology.Lines, 0);
        mesh2d.RecalculateBounds();
    }

}

public class FractalTreePart2D
{
    // Note: The end of the part is the development node concerned
    public int ID { get; private set; }
    public BranchType2D Type { get; private set; }
    public float Angle { get; private set; }
    public float CrossAngle { get; private set; }
    public int BranchOptionNumber; // The number of the branch along its tier (works as horizontal ID)
    public int BranchTier; // The height of the branch (works as vertical ID)

    public FractalTreePart2D(int id, BranchType2D type, float angle, float crossAngle)
    {
        ID = id;
        Type = type;
        Angle = angle; //Angle relative to original branch;
        CrossAngle = 0; //crossAngle;

    }
}

public enum BranchType2D
{
    Core,
    Self,
    People,
    World,
    Past,
    Combo
}