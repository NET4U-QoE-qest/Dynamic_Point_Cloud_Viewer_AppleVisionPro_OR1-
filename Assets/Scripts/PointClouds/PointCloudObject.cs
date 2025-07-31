using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PointCloudObject
{
    public string pcName;
    public PCObjectType objectType = PCObjectType.Unknown;
    public int[] qualities = new int[] { 1, 2, 3, 4 };

    // Nuova definizione: Lista dei path ai file PLY per ogni frame (frame-on-demand)
    public List<string> framePaths = new List<string>();

    // Mesh statiche (se ancora necessarie)
    public Dictionary<int, Mesh[]> meshes = new Dictionary<int, Mesh[]>();
    public Dictionary<int, Material[]> meshMaterials = new Dictionary<int, Material[]>();

    public void LoadAssetsFromResources()
    {
        if (string.IsNullOrEmpty(pcName))
        {
            Debug.LogWarning($"[PCObject] pcName is null or empty, cannot load.");
            return;
        }

        framePaths.Clear();

        // Aggiorna framePaths caricando percorsi (esempio per risolvere errore)
        foreach (int q in qualities)
        {
            string pcPath = $"PointClouds/{pcName}/q{q}/PointClouds";
            var loadedMeshes = Resources.LoadAll<Mesh>(pcPath);
            foreach (var mesh in loadedMeshes)
            {
                framePaths.Add(pcPath + "/" + mesh.name + ".ply");
            }

            // Opzionale: Carica Mesh statiche (se necessarie)
            string meshPath = $"PointClouds/{pcName}/q{q}/Meshes";
            GameObject[] meshPrefabs = Resources.LoadAll<GameObject>(meshPath);
            if (meshPrefabs.Length > 0)
            {
                List<Mesh> meshList = new();
                List<Material> matList = new();

                foreach (GameObject go in meshPrefabs)
                {
                    MeshFilter mf = go.GetComponent<MeshFilter>();
                    MeshRenderer mr = go.GetComponent<MeshRenderer>();
                    if (mf != null && mf.sharedMesh != null)
                        meshList.Add(mf.sharedMesh);
                    if (mr != null && mr.sharedMaterial != null)
                        matList.Add(mr.sharedMaterial);
                }

                meshes[q] = meshList.ToArray();
                meshMaterials[q] = matList.ToArray();
            }
        }
    }
}
