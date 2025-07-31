using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PointCloudsLoader : MonoBehaviour
{
    public static PointCloudsLoader Instance;
    public bool loadMeshes = true;
    public string pcPathPrefix = "PointClouds/";

    public List<PointCloudObject> pcObjects = new List<PointCloudObject>();
    public delegate void LoaderEvent();
    public static event LoaderEvent OnLoaded;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        LoadAvailablePointClouds();
    }

    private void LoadAvailablePointClouds()
    {
        pcObjects.Clear();

        // Questa parte resta SOLO se vuoi supportare mesh statiche da Resources.
        // Se usi solo pipeline "frame-on-demand" dai file esterni, puoi anche eliminarla.
        var meshes = Resources.LoadAll<Mesh>("PointClouds");
        Debug.Log($"[Loader] Found {meshes.Length} Meshes in Resources/PointClouds");

        // Group meshes by base name (removing numeric index like _0000)
        var grouped = meshes.GroupBy(mesh =>
        {
            string name = mesh.name;
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore > 0)
                return name.Substring(0, lastUnderscore); // e.g. BlueSpin_UVG_vox10_25_0_250
            return name;
        });

        foreach (var kv in grouped)
        {
            var name = kv.Key;
            if (string.IsNullOrEmpty(name) || name == "Unknown")
            {
                Debug.LogWarning($"Invalid group name: {name}. Skipping...");
                continue;
            }

            var obj = new PointCloudObject
            {
                pcName = name,
                objectType = PCObjectTypeExtensions.FromString(name) ?? PCObjectType.Unknown
            };

            Debug.Log($"[Loader] Loading object {obj.pcName}");

            obj.LoadAssetsFromResources();

            // -- NUOVA LOGICA: aggiungi all'elenco solo se framePaths esiste --
            if (obj.framePaths != null && obj.framePaths.Count > 0)
            {
                pcObjects.Add(obj);
                Debug.Log($"[Loader] PointCloud {obj.pcName} loaded with {obj.framePaths.Count} frame paths.");
            }
            else
            {
                Debug.LogWarning($"[PCObject] No frame paths found for {obj.pcName}");
            }
        }

        Debug.Log($"[Loader] Total point clouds loaded: {pcObjects.Count}");
        OnLoaded?.Invoke();
    }

    public PointCloudController Spawn(string pcName, string quality)
    {
        PointCloudObject data = pcObjects.FirstOrDefault(p => p.pcName == pcName);
        if (data == null)
        {
            Debug.LogError($"❌ PointCloud {pcName} not found!");
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>("Prefabs/Preview_PointCloudPrefab_NET4U");
        if (prefab == null)
        {
            Debug.LogError("Prefab 'Preview_PointCloudPrefab_NET4U' not found!");
            return null;
        }

        GameObject go = Instantiate(prefab);
        PointCloudController controller = go.GetComponent<PointCloudController>();
        controller.data = data;
        controller.LoadPointCloud(pcName, int.Parse(quality));
        return controller;
    }

    private string AssetPathFrom(string meshName)
    {
        // In Resources/PointClouds/{NAME}/qX/PointClouds/{frame}
        if (meshName.Contains("_q")) return meshName.Split('_')[0];
        return "Unknown";
    }

    public PointCloudObject GetPCObjectFromType(PCObjectType type)
    {
        return pcObjects.FirstOrDefault(p => p.objectType == type);
    }

    public string GetBasePCPathFromType(PCObjectType type)
    {
        return pcPathPrefix + GetPCNameFromType(type) + "/";
    }

    public string GetPCNameFromType(PCObjectType type)
    {
        foreach (var obj in pcObjects)
        {
            if (obj.objectType == type)
                return obj.pcName;
        }
        return null;
    }

    // Method kept for backward compatibility with older scripts
    public void LoadPointCloudsAndMeshes()
    {
        foreach (var obj in pcObjects)
        {
            obj.LoadAssetsFromResources();
        }
    }
}
