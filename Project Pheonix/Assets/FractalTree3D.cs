using System.Collections;
using System.Linq;
using UnityEngine;

public class FractalTree3D : MonoBehaviour
{
    public int maxIterations = 5;
    public float lengthRatio = 0.7f;
    public float startInnerAngleSpread = 30f;
    public float startCrossAngleSpread = 20f;
    public Material material;

    private int currentNodeID = 0;
    private int currentNodeTier = 0;
    private int currentNodeOptionID = 0;

    public Mesh mesh;
    private Vector3[] vertices;
    private int[] indices;
    private int currentIndex;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;

        currentIndex = 0;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 5f; // Initial vertical trunk

        FractalTreePart rootPart = new FractalTreePart(currentNodeID, BranchType.Core, 0, 0);
        DrawFractalTree(rootPart, startPos, endPos, maxIterations, startInnerAngleSpread, startCrossAngleSpread);
        //DrawFractalTree(startPos, endPos, maxIterations, startInnerAngleSpread, startCrossAngleSpread); 

        UpdateMesh();
    }

    void DrawFractalTree(FractalTreePart parentPart, Vector3 start, Vector3 end, int iterations, float currentInnerAngle, float currentCrossAngle)
    {
        if (iterations <= 0)
            return;

        currentNodeID++;
        currentNodeTier = maxIterations - iterations;
        currentNodeOptionID = 0;

        int numBranches = CalculateBranchCount(iterations); // To improve and make a bit more complex

        AddVertex(start);
        AddVertex(end);

        Vector3 direction = end - start;

        float innerAngleSpread = 120f; // Maintain 120-degree spacing for main branches

        for (int i = 0; i < numBranches; i++)
        {
            BranchType currentBranchType = parentPart.Type;


            float branchInnerAngle = (iterations == maxIterations) ? 30 : (i-Mathf.FloorToInt(numBranches/2)) * 5;//i * 5;

            // Use a shifted angle for combo branch to position its splits between two main branches
            float comboBranchAngleShift = 60f;
            float comboBranchSplitAngle = (i * innerAngleSpread + comboBranchAngleShift) % 360f;

            // Adjust the branchCrossAngle for the initial split to form a tetrahedral arrangement
            float branchCrossAngle = (iterations == maxIterations) ? i * 120f : (i-Mathf.FloorToInt(numBranches/2)) * 0;

            if(iterations == maxIterations)
            {
                if (i == 0) {branchInnerAngle = 0; currentBranchType = BranchType.Combo;}
                else if (i==1) {branchInnerAngle = 30; currentBranchType = BranchType.Self;}
                else if (i==2) {branchInnerAngle = 30; currentBranchType = BranchType.People;}
                else if (i==3) {branchInnerAngle = 30; currentBranchType = BranchType.World;} //TODO: ADD PAST

                branchCrossAngle = i * 120f;
            }

            Vector3 rotationAxis = direction.normalized;

            Vector3 newDirection = Quaternion.AngleAxis(branchCrossAngle, rotationAxis) * Quaternion.Euler(branchInnerAngle, comboBranchSplitAngle, 0) * direction;

            Vector3 newEnd = end + newDirection * lengthRatio;

            FractalTreePart currentPart = new FractalTreePart(currentNodeID++, currentBranchType, 0, 0);


            // Recursive calls with adjusted starting points and angles
            DrawFractalTree(currentPart, end, newEnd, iterations - 1, branchInnerAngle, branchCrossAngle);

            // Draw secondary lines for prerequisites (use your dashed line rendering method here)
            DrawSecondaryLine(end, newEnd);


            
        }
    }

    void DrawSecondaryLine(Vector3 start, Vector3 end)
    {
        // Implement your code to draw secondary lines (dashed lines) here
    }


    //float branchInnerAngle = (iterations == maxIterations) ? 30 : (i-Mathf.FloorToInt(numBranches/2)) * 5;

    // Adjust the branchCrossAngle for the initial split to form a tetrahedral arrangement
    //float branchCrossAngle = (iterations == maxIterations) ? i*120f : (i-Mathf.FloorToInt(numBranches/2)) * 0;

    int CalculateBranchCount(int remainingIterations)
    {
        // You can implement your logic here to dynamically calculate the number of branches based on the state
        // For simplicity, I'm using a linear relationship here. You might want to replace this with your own logic.
        if(remainingIterations == maxIterations){return 4;}

        return 2;
        //return Mathf.Max(2, maxIterations - remainingIterations + 2);
    }

    void AddVertex(Vector3 vertex)
    {
        if (vertices == null || currentIndex >= vertices.Length)
        {
            // Expand the arrays as needed
            int newSize = (vertices != null) ? vertices.Length * 2 : 4;
            System.Array.Resize(ref vertices, newSize);
        }

        vertices[currentIndex++] = vertex;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;

        // Create an index array for the Lines topology
        int[] indices = new int[currentIndex];
        for (int i = 0; i < currentIndex; i++)
        {
            indices[i] = i;
        }

        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
    }

    IEnumerator DrawDelayed(FractalTreePart parentPart, Vector3 start, Vector3 end, int iterations, float currentInnerAngle, float currentCrossAngle)
    {
        yield return new WaitForSeconds(0.1f);
        DrawFractalTree(parentPart, start, end, iterations, currentInnerAngle, currentCrossAngle);
    }
}

public class FractalTreePart
{
    // Note: The end of the part is the development node concerned
    public int ID { get; private set; }
    public BranchType Type { get; private set; }
    public float InnerAngle { get; private set; }
    public float CrossAngle { get; private set; }
    public int BranchOptionNumber; // The number of the branch along its tier (works as horizontal ID)
    public int BranchTier; // The height of the branch (works as vertical ID)

    public FractalTreePart(int id, BranchType type, float innerAngle, float crossAngle)
    {
        ID = id;
        Type = type;
        InnerAngle = 0;//innerAngle;
        CrossAngle = 0;//crossAngle;

    }
}

public enum BranchType
{
    Core,
    Self,
    People,
    World,
    Past,
    Combo
}