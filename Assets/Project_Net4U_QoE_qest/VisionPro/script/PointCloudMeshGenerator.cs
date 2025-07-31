using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloudMeshGenerator : MonoBehaviour
{
    public List<Vector3> Points = new List<Vector3>();
    public List<Color> Colors = new List<Color>();
    public float PointSize = 0.01f;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Color> vertexColors = new List<Color>();
        List<int> triangles = new List<int>();

        int index = 0;

        foreach (var point in Points)
        {
            // Create small triangle for each point
            Vector3 p1 = point + new Vector3(-PointSize, -PointSize, 0);
            Vector3 p2 = point + new Vector3(PointSize, -PointSize, 0);
            Vector3 p3 = point + new Vector3(0, PointSize, 0);

            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);

            Color color = (Colors.Count > index) ? Colors[index] : Color.white;
            vertexColors.Add(color);
            vertexColors.Add(color);
            vertexColors.Add(color);

            int vIndex = index * 3;
            triangles.Add(vIndex);
            triangles.Add(vIndex + 1);
            triangles.Add(vIndex + 2);

            index++;
        }

        mesh.SetVertices(vertices);
        mesh.SetColors(vertexColors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
