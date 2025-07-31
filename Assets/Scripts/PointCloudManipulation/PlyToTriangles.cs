using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlyToCrossTriangles : MonoBehaviour
{
    [Header("PLY Resource Path (senza .ply.bytes)")]
    public string resourcePath = "PointClouds/BlueSpin_UVG_vox10_25_0_250_0000";

    [Header("Rendering")]
    public float pointSize = 0.005f;
    public Material vertexColorMaterial;

    private List<Vector3> points = new();
    private List<Color> colors = new();
    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh { name = "CrossTriangles", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = vertexColorMaterial;

        LoadPLYFromResources(resourcePath);
        GenerateCrossTriangles();
    }

    void GenerateCrossTriangles()
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Color> vertexColors = new();

        Vector3 baseRight = Vector3.right * pointSize;
        Vector3 baseUp = Vector3.up * pointSize;
        Vector3 baseForward = Vector3.forward * pointSize;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 center = points[i];
            Color col = (i < colors.Count) ? colors[i] : Color.white;

            // Triangolo 1 nel piano Right–Up (XY)
            Vector3 a1 = center - baseRight - baseUp;
            Vector3 b1 = center + baseRight - baseUp;
            Vector3 c1 = center + baseUp * 2f;

            int baseIndex1 = vertices.Count;
            vertices.Add(a1); vertexColors.Add(col);
            vertices.Add(b1); vertexColors.Add(col);
            vertices.Add(c1); vertexColors.Add(col);

            triangles.Add(baseIndex1);
            triangles.Add(baseIndex1 + 1);
            triangles.Add(baseIndex1 + 2);

            // Triangolo 2 nel piano Forward–Up (YZ)
            Vector3 a2 = center - baseForward - baseUp;
            Vector3 b2 = center + baseForward - baseUp;
            Vector3 c2 = center + baseUp * 2f;

            int baseIndex2 = vertices.Count;
            vertices.Add(a2); vertexColors.Add(col);
            vertices.Add(b2); vertexColors.Add(col);
            vertices.Add(c2); vertexColors.Add(col);

            triangles.Add(baseIndex2);
            triangles.Add(baseIndex2 + 1);
            triangles.Add(baseIndex2 + 2);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(vertexColors);
        mesh.RecalculateBounds();
    }




    void LoadPLYFromResources(string pathInResources)
    {
        TextAsset plyAsset = Resources.Load<TextAsset>(pathInResources);
        if (plyAsset == null)
        {
            Debug.LogError($"[PLY] File not found in Resources: {pathInResources}");
            return;
        }

        try
        {
            using MemoryStream ms = new(plyAsset.bytes);
            using BinaryReader reader = new(ms);

            string line;
            int vertexCount = 0;
            long dataStart = 0;

            while (true)
            {
                line = ReadLine(reader);
                if (line == null) return;

                if (line.StartsWith("element vertex"))
                    vertexCount = int.Parse(line.Split(' ')[2]);
                else if (line.StartsWith("end_header"))
                {
                    dataStart = ms.Position;
                    break;
                }
            }

            ms.Seek(dataStart, SeekOrigin.Begin);
            points.Clear(); colors.Clear();

            for (int i = 0; i < vertexCount; i++)
            {
                double x = reader.ReadDouble();
                double y = reader.ReadDouble();
                double z = reader.ReadDouble();

                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte r = reader.ReadByte();

                Vector3 v = new((float)x, (float)y, (float)z);
                if (!IsValid(v))
                {
                    Debug.LogWarning($"[PLY] Skipping invalid vertex {i}: {v}");
                    continue;
                }

                points.Add(v);
                colors.Add(new Color32(r, g, b, 255));
            }

            Debug.Log($"[PLY] Loaded {points.Count} valid vertices from: {resourcePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PLY] Error reading PLY: {ex.Message}");
        }
    }

    static string ReadLine(BinaryReader reader)
    {
        List<byte> bytes = new();
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            byte b = reader.ReadByte();
            if (b == '\n') break;
            if (b != '\r') bytes.Add(b);
        }
        return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
    }

    static bool IsValid(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}
