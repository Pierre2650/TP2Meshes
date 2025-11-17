using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using Debug = UnityEngine.Debug;

public class TP2Modelisation : MonoBehaviour
{

    private MeshFilter myMF;
    public string fileName;
    public int nbTrianglesToRemove;

    private List<int> triangles = new List<int>();
    private List<Vector3> vertices = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myMF = GetComponent<MeshFilter>();
        myMF.mesh.Clear();

        readOFF();
        exportFile();
    }


    private void readOFF()
    {
        string path = "Assets\\";
        path += fileName;
     

        if (!File.Exists(path))
        {
            Debug.LogError("File Doesn't exist in the assets folder");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        string[] parts = lines[1].Split(' ');
        int nbVertices = int.Parse(parts[0]);
        int nbTriangles = int.Parse(parts[1]);

        Vector3[] tempVertices = new Vector3[nbVertices];
        Vector3 center = Vector3.zero;


        int lineIndex = 2;
        float max = 0;
        for (int j = 0; j < tempVertices.Length; lineIndex++, j++) {

            parts = lines[lineIndex].Split(' ');

            Vector3 aVertice = new Vector3(float.Parse(parts[0],CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));

            if(Mathf.Abs(aVertice.x) > max) { max = Mathf.Abs(aVertice.x); }
            if ( Mathf.Abs(aVertice.y) > max) { max = Mathf.Abs(aVertice.y); }
            if (Mathf.Abs(aVertice.z) > max){ max = Mathf.Abs(aVertice.z); }

            center += aVertice;
            tempVertices[j] = aVertice;
        
        }

        vertices = tempVertices.ToList();

        center = new Vector3(center.x / nbVertices, center.y / nbVertices, center.z / nbVertices);

        for (int j = 0; j < tempVertices.Length; j++)
        {
            tempVertices[j] -= center; 
            tempVertices[j] = new Vector3(tempVertices[j].x / max, tempVertices[j].y / max, tempVertices[j].z / max);
        }


        Vector3[] trianglesNormals = new Vector3[nbTriangles];
        for (int i = 0 ;  i < nbTriangles; lineIndex++, i++)
        {
            parts = lines[lineIndex].Split(' ');    

            triangles.Add(int.Parse(parts[1]));
            triangles.Add(int.Parse(parts[2]));
            triangles.Add(int.Parse(parts[3]));

        

            Vector3 A = tempVertices[int.Parse(parts[2])]  -  tempVertices[int.Parse(parts[1])];
            Vector3 B = tempVertices[int.Parse(parts[3])] - tempVertices[int.Parse(parts[1])];


            Vector3 Normal = Vector3.Cross(A, B);
            trianglesNormals[i] = Normal.normalized;

            //Debug.Log("normal = "+ Normal);
        }


        Vector3[] verticesNormals = new Vector3[tempVertices.Length];
        for (int i = 0; i < tempVertices.Length; i++)
        {
            int nbNormals = 0;
            Vector3 aVerticeNormal = Vector3.zero;
            for (int j = 0, k = 0; j < trianglesNormals.Length; j++, k+=3)
            {
               
                    if (triangles[k] == i) { aVerticeNormal += trianglesNormals[j]; nbNormals++; }
                    if (triangles[k + 1] == i) { aVerticeNormal += trianglesNormals[j]; nbNormals++; }
                    if (triangles[k + 2] == i) { aVerticeNormal += trianglesNormals[j]; nbNormals++; }

            }

            verticesNormals[i] = (aVerticeNormal / nbNormals).normalized;
        }




        myMF.mesh.vertices = tempVertices;
        myMF.mesh.triangles = triangles.ToArray();
        myMF.mesh.normals = verticesNormals;

    }


    private void exportFile()
    {
        int nbTriToRemove = nbTrianglesToRemove;
        string path = "Assets\\";
        path += fileName;


        if (!File.Exists(path))
        {
            Debug.LogError("File Doesn't exist in the assets folder");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        string[] parts = lines[1].Split(' ');
        int nbVertices = int.Parse(parts[0]);
        int nbTriangles = int.Parse(parts[1]);

        if (nbTriToRemove > nbTriangles)
        {
            Debug.LogError("number of triangles to remove is bigger than the amount of triangles in the file");
            return;
        }


        int nbRemovedVertices = 0;
        int j = 0, k = 0;
        for (; j < nbTriToRemove; j++, k += 3)
        {

            for (int i = 0; i < vertices.Count; i++)
            {
                if (triangles[k] == i && !float.IsNegativeInfinity(vertices[i].x))
                { vertices[i] = Vector3.negativeInfinity; nbRemovedVertices++; }

                if (triangles[k + 1] == i && !float.IsNegativeInfinity(vertices[i].x))
                { vertices[i] = Vector3.negativeInfinity; nbRemovedVertices++; }

                if (triangles[k + 2] == i && !float.IsNegativeInfinity(vertices[i].x))
                { vertices[i] = Vector3.negativeInfinity; nbRemovedVertices++; }

            }

            triangles[k] = -1;
            triangles[k + 1] = -1;
            triangles[k + 2] = -1;
        }



        for (; j < nbTriangles; j++, k += 3)
        {

            for (int i = 0; i < vertices.Count; i++)
            {
                if (triangles[k] == i && float.IsNegativeInfinity(vertices[i].x))
                {
                    triangles[k] = -1;
                    triangles[k + 1] = -1;
                    triangles[k + 2] = -1;

                    nbTriToRemove++;
                    break;
                }

                if (triangles[k + 1] == i && float.IsNegativeInfinity(vertices[i].x))
                {
                    triangles[k] = -1;
                    triangles[k + 1] = -1;
                    triangles[k + 2] = -1;
             
                    nbTriToRemove++;
                    break;
                }

                if (triangles[k + 2] == i &&  float.IsNegativeInfinity(vertices[i].x))
                {
                    triangles[k] = -1;
                    triangles[k + 1] = -1;
                    triangles[k + 2] = -1;

                    nbTriToRemove++;
                    break;
                }

            }

        }




        if (nbVertices - nbRemovedVertices == 0)
        {
           
            triangles.Clear();

            nbVertices = 0;
            nbTriangles = 0;
        }
        else
        {
            nbVertices -= nbRemovedVertices;
            nbTriangles -= nbTriToRemove;
        }

        string[] newFile = new string[2 + nbVertices + nbTriangles];
        newFile[0] = "OFF";
        newFile[1] = nbVertices + " " + nbTriangles + " 0";


        int nbRemoved = 0;
        int verticeIndex = 0;
        int lineIndex = 2;

        printTriangles();

        foreach (Vector3 v in vertices)
        {
            if (float.IsNegativeInfinity(v.x))
            {
                nbRemoved++;
            }
            else
            {
                newFile[lineIndex] = v.x.ToString(CultureInfo.InvariantCulture)+" "+v.y.ToString(CultureInfo.InvariantCulture) + " "+v.z.ToString(CultureInfo.InvariantCulture);
                lineIndex++;

               for(int y = 0; y < triangles.Count; y++ )
                {
                    if (triangles[y] == verticeIndex)
                    {
                        triangles[y] -= nbRemoved;
                    }
                }
            }

            verticeIndex++;
        }

        printTriangles();


        for (int y = 0; y < triangles.Count; y+=3)
        {
            if( triangles[y] != -1)
            {
                newFile[lineIndex] = "3 "+ triangles[y] + " " + triangles[y+1] + " " + triangles[y+2];
                lineIndex++;
            }
        }


        string newPath = "Assets\\Export.off";
        File.WriteAllLines(newPath, newFile);


    }

    private void printTriangles()
    {
        

        for (int y = 0; y < triangles.Count; y += 3)
        {
            //Debug.Log("tri = " + triangles[y] + " " + triangles[y + 1] + " " + triangles[y]);
        }

    }

    private void OnDrawGizmos()
    {
        if (myMF == null || myMF.mesh == null)
            return;

        Mesh mesh = myMF.mesh;
        Vector3[] verts = mesh.vertices;
        Vector3[] norms = mesh.normals;

        Gizmos.color = Color.blue;

        for (int i = 0; i < verts.Length; i++)
        {
            // Convert from local mesh space to world space
            Vector3 worldPos = transform.TransformPoint(verts[i]);
            Vector3 worldNormal = transform.TransformDirection(norms[i]);

            // Draw the normal as a small line
            Gizmos.DrawLine(worldPos, worldPos + worldNormal * 0.2f);
        }
    }


}
