using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AnimatePlyWithTriangles : MonoBehaviour
{
    [Header("Durata visibilità di ciascun PLY (in secondi)")]
    public float frameDuration = 1.0f;

    [Header("Dimensione triangoli per punto")]
    public float pointSize = 0.005f;

    [Header("Materiale per i triangoli")]
    public Material vertexColorMaterial;

    [Header("Nome della scena successiva (vuoto = nessuna)")]
    public string scenaSuccessiva = "";

    private List<GameObject> plyFrames = new();

    void Start()
    {
        // Trova tutti i MeshFilter nei figli
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(includeInactive: true);

        foreach (MeshFilter mf in filters)
        {
            if (mf.gameObject == gameObject) continue;

            GameObject originalGO = mf.gameObject;

            // Crea una copia dell'oggetto con triangoli croce
            GameObject triangleGO = new GameObject(originalGO.name + "_tri");
            triangleGO.transform.SetParent(originalGO.transform.parent);
            triangleGO.transform.position = originalGO.transform.position;
            triangleGO.transform.rotation = originalGO.transform.rotation;
            triangleGO.transform.localScale = originalGO.transform.localScale;

            var mfNew = triangleGO.AddComponent<MeshFilter>();
            var mrNew = triangleGO.AddComponent<MeshRenderer>();
            mrNew.sharedMaterial = vertexColorMaterial;

            // Usa .mesh per avere una copia scrivibile della mesh
            Mesh readableMesh = null;
            try
            {
                readableMesh = mf.mesh;
            }
            catch
            {
                Debug.LogError($"[PLY] La mesh di {mf.name} non è leggibile. Verifica l'import o il caricamento.");
                continue;
            }

            GenerateCrossTriangles(readableMesh, mfNew, pointSize);

            triangleGO.SetActive(false);
            plyFrames.Add(triangleGO);
        }

        if (plyFrames.Count == 0)
        {
            Debug.LogWarning("[PLY] Nessun frame trovato.");
            return;
        }

        Debug.Log($"[PLY] Trovati {plyFrames.Count} frame. Avvio sequenza.");
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        for (int i = 0; i < plyFrames.Count; i++)
        {
            GameObject current = plyFrames[i];
            current.SetActive(true);
            Debug.Log($"[PLY] Frame {i + 1}/{plyFrames.Count}: {current.name}");
            yield return new WaitForSeconds(frameDuration);
            current.SetActive(false);
        }

        if (!string.IsNullOrEmpty(scenaSuccessiva))
        {
            SceneTransitionState.scenaSuccessiva = scenaSuccessiva;
            SceneManager.LoadScene("Scene_Empty", LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("[PLY] Sequenza completata. Nessuna scena successiva specificata.");
        }
    }

    void GenerateCrossTriangles(Mesh sourceMesh, MeshFilter targetFilter, float size)
    {
        if (sourceMesh == null || sourceMesh.vertexCount == 0)
        {
            Debug.LogWarning("[PLY] Mesh vuota o nulla.");
            return;
        }

        Vector3[] points = sourceMesh.vertices;
        Color[] meshColors = sourceMesh.colors;

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Color> vertexColors = new();

        Vector3 baseRight = Vector3.right * size;
        Vector3 baseUp = Vector3.up * size;
        Vector3 baseForward = Vector3.forward * size;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 center = points[i];
            Color col = (i < meshColors.Length) ? meshColors[i] : Color.white;

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

        Mesh mesh = new Mesh
        {
            name = "CrossTriangles",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(vertexColors);
        mesh.RecalculateBounds();

        targetFilter.sharedMesh = mesh;
    }
}

