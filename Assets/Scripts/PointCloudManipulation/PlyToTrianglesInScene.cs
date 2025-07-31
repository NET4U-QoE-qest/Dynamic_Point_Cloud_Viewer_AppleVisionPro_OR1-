using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlyToTrianglesInScene : MonoBehaviour
{
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

        LoadMeshFromScene();
        GenerateCrossTriangles();
    }

    void LoadMeshFromScene()
    {
        // Trova tutti i MeshFilter nei figli
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(includeInactive: true);

        Debug.Log($"[PLY] Trovati {filters.Length} MeshFilter nei figli (incluso se stesso).");

        // Scegli il primo che non sia quello attaccato a questo stesso GameObject
        MeshFilter sourceFilter = null;
        foreach (var mf in filters)
        {
            if (mf != GetComponent<MeshFilter>())
            {
                Debug.Log($"[PLY] Candidato trovato: {mf.gameObject.name}");
                sourceFilter = mf;
                break;
            }
        }

        if (sourceFilter == null || sourceFilter.sharedMesh == null)
        {
            Debug.LogError("[PLY] Nessuna mesh valida trovata nei figli di questo GameObject.");
            return;
        }

        Mesh sourceMesh = sourceFilter.sharedMesh;

        Debug.Log($"[PLY] Mesh selezionata da GameObject: '{sourceFilter.gameObject.name}'");
        Debug.Log($"[PLY] Vertici: {sourceMesh.vertexCount}, Bounds: {sourceMesh.bounds}");

        Vector3[] verts = sourceMesh.vertices;
        Color[] meshColors = sourceMesh.colors;

        points = new List<Vector3>(verts);
        colors = new List<Color>();

        for (int i = 0; i < verts.Length; i++)
        {
            colors.Add(i < meshColors.Length ? meshColors[i] : Color.white);
        }

        Debug.Log($"[PLY] Caricati {points.Count} punti dalla mesh '{sourceFilter.name}'.");
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

            // Triangolo XY
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

            // Triangolo YZ
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

        Debug.Log($"[PLY] Mesh aggiornata con {vertices.Count} vertici totali.");
    }

    static bool IsValid(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}
