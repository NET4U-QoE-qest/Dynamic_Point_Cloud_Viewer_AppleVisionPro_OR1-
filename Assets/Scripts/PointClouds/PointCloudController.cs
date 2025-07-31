using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Pcx;

[RequireComponent(typeof(AnimatePointCloudBase))]
public class PointCloudController : MonoBehaviour
{
    public PointCloudObject data;

    private AnimatePointCloudBase animator;

    private bool isMesh = false;
    private int currentQuality = 1;

    void Awake()
    {
        animator = GetComponent<AnimatePointCloudBase>();
    }

    /// <summary>
    /// Imposta la qualità: trova tutti i frame (solo path!), ordina e passa la lista al tuo animator
    /// </summary>
    public void SetQuality(string qualityString)
    {
        if (data == null) return;

        int quality = int.Parse(qualityString.Replace("q", ""));
        currentQuality = quality;
        Debug.Log($"[PCController] Changing quality to: q{quality}");

        data.framePaths.Clear();

#if UNITY_ANDROID && !UNITY_EDITOR
        string basePath = Application.persistentDataPath;
#else
        string basePath = Path.Combine(Application.dataPath, "PointClouds");
#endif

        string folderPath = Path.Combine(basePath, "PointClouds", data.pcName, $"q{quality}", "PointClouds");

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"[PCController] Folder not found: {folderPath}");
            return;
        }

        string[] files = Directory.GetFiles(folderPath, "*.ply");
        System.Array.Sort(files);

        data.framePaths.AddRange(files);
        Debug.Log($"[PCController] Loaded {files.Length} frame paths from {folderPath}");
        Debug.Log($"[simone] il path è {data.framePaths}");

        animator.LoadQuality(data.framePaths, data.pcName);
        animator.StopAnimation();
        animator.StartAnimation();
        Debug.Log("[PCController] Animation restarted with new quality (on-demand)");
    }

    public void SetIsMesh(bool mesh)
    {
        isMesh = mesh;
        Debug.Log($"[PCController] Mesh mode: {isMesh}");
    }

    public void SetAnimate(bool active)
    {
        if (active)
            animator.StartAnimation();
        else
            animator.StopAnimation();

        Debug.Log($"[PCController] Animate: {active}");
    }

    public void ResetView()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        SetQuality("q3");
        SetIsMesh(false);
        SetAnimate(false);

        Debug.Log($"[PCController] View reset");
    }

    /// <summary>
    /// Chiama sempre questa per posizionare e caricare il point cloud richiesto
    /// </summary>
    public void LoadPointCloud(string name, int quality)
    {
        if (data != null)
            data.pcName = name;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 forward = cam.transform.forward.normalized;
            Vector3 pos = cam.transform.position + forward * 3.0f;
            pos.y -= 1.0f;
            transform.position = pos;
            Debug.Log($"[PCController] Position set dynamically in front of camera at {transform.position}");
        }
        else
        {
            transform.position = new Vector3(0, 0, 3.0f);
            Debug.LogWarning("[PCController] Main camera not found, using default position.");
        }

        transform.localScale = Vector3.one * 0.002f;

        SetQuality($"q{quality}");

        // Non serve più caricare tutti i materiali qui: lo farà AnimatePointCloudBase on demand!
    }

    public int CurrentQuality => currentQuality;
}
