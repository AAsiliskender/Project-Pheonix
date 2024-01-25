using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public static class CalcFunctions
{
    public static readonly float tolerance = 0.00001f;



    // Function that adds arrays size 2 (vectors) together
    public static Vector2Int SumVector2IntArray(Vector2Int[] vectorArray)
    {
        Vector2Int totalSum = Vector2Int.zero;
        foreach(Vector2Int vector in vectorArray)
        {
            totalSum += vector;
        }
        return totalSum;
    }



    // Rounding down a float to integer (within tolerance)
    public static int RoundDown(float number)
    {
        float numAndTol = number + tolerance;
        int result = (int)UnityEngine.Mathf.Floor(numAndTol);
        return result;
    }

    // Rounding down a double to integer (within tolerance)
    public static int RoundDown(double number)
    {
        double numAndTol = number + tolerance;
        int result = (int)UnityEngine.Mathf.Floor((float)numAndTol);
        return result;
    }
    
    public static int[,] MultiplyMatrix (int[,] matrix1, int[,] matrix2)
    {
        int[,] newMatrix = new int[matrix1.GetLength(0), matrix2.GetLength(1)];

        for (int i = 0; i < matrix1.GetLength(0); i++)
        {
            for (int j = 0; j < matrix2.GetLength(1); j++)
            {
                newMatrix[i,j] = 0; // Set initial value to 0
                for (int k = 0; k < matrix1.GetLength(1); k++)
                {
                    newMatrix[i,j] = newMatrix[i,j] + matrix1[i,k]*matrix2[k,j]; // Row/Column dot product
                }
            }
        }
        

        return newMatrix;
    }

    public static Vector2Int VectorMatrixMult(int[,] matrix, Vector2Int vector) //Only works for 2x2
    {
        Vector2Int newVector = new Vector2Int (0,0);
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            newVector[i] = 0;
            for (int j = 0; j < matrix.GetLength(1); j++)
            {   
                newVector[i] = newVector[i] + matrix[j,i]*vector[j];
            }
        }

        return newVector;
    }

    public static Vector2Int[] VectorMatrixMult(int[,] matrix, Vector2Int[] vector) //Only works for 2x2 //Not sure if this is working
    {
        Vector2Int newVectorComponent = new Vector2Int (0,0);
        Vector2Int[] newVector = new Vector2Int[vector.Count()];
        for (int n = 0; n < vector.Count(); n++)
        {
            Vector2Int vectorComponent = vector[n];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                newVectorComponent[i] = 0;
                for (int j = 0; j < matrix.GetLength(1); j++)
                {   
                    newVectorComponent[i] = newVectorComponent[i] + matrix[i,j]*vectorComponent[j];
                }
            }
            newVector[n] = newVectorComponent;
        }
        

        return newVector;
    }

    public static bool TestVectorMatrixMult() // Unit test of matrix multiplication, arbitary value and 2x2,2x1 dims
    {
        int[,] matrixTest = new int[2,2] {{1,2},{-3,4}};
        Debug.Log("Matrix Test Elements: ");
        Debug.Log(matrixTest[0,0]);
        Debug.Log(matrixTest[0,1]);
        Debug.Log(matrixTest[1,0]);
        Debug.Log(matrixTest[1,1]);
        
        Vector2Int vectorTest = new Vector2Int(1,3);

        Vector2Int resultVector = VectorMatrixMult(matrixTest,vectorTest);

        Vector2Int expectedVector = new Vector2Int(-8,14);
        
        bool trueOrFalse = true;
        for (int i = 0; i < 2; i++)
        {
            if (resultVector[i] != expectedVector[i])
            {
                trueOrFalse = false;
                break;
            }
        }
        return trueOrFalse;
    }



    public static bool TestMatrixMult() // Unit test of matrix multiplication, arbitary value and dimensions
    {
        int[,] matrixOne = new int [2,3];
        int[,] matrixTwo = new int [3,4];
        int[,] matrixResult = new int [2,4]; 

        matrixOne = new int[2,3] {{1,3,5},{2,4,6}};
        matrixTwo = new int[3,4] {{5,8,11,1},{6,9,12,3},{7,10,15,5}};
        int[,] matrixExpected = new int[2,4] {{5+18+35, 8+27+50, 11+36+75, 1+9+25}, {10+24+42, 16+36+60, 22+48+90, 2+12+30}};

        matrixResult = MultiplyMatrix(matrixOne, matrixTwo);

        // Testing if true by element, true unless proven otherwise
        bool trueOrFalse = true;
        for (int i = 0; i < matrixResult.GetLength(0); i++)
        {
            for (int j = 0; j < matrixResult.GetLength(1); j++)
            {
                if (matrixResult[i,j] != matrixExpected[i,j])
                {
                    trueOrFalse = false;
                    break;
                }
            }
        }

        return trueOrFalse;
    }


}
