using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneInfoToText : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI pointCloudInfoText;

    [Header("Transforms")]
    public Transform pointCloudTransform;
    public Transform xrRigTransform;  // fallback se Camera.main è null

    private string pcName = "N/A";
    private string quality = "N/A";
    private int remaining = -1;

    void Start()
    {
        if (pointCloudInfoText == null)
        {
            Debug.LogError("[SceneInfoToText] TextMeshProUGUI not assigned.");
            return;
        }

        // === 1. Extract info from scene name ===
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("_q"))
        {
            int qIndex = sceneName.LastIndexOf("_q");
            pcName = sceneName.Substring(0, qIndex);
            quality = sceneName.Substring(qIndex + 1);
        }

        // === 2. Read how many scenes are left ===
        string path = Path.Combine(Application.persistentDataPath, "scenes_runtime.txt");
        remaining = File.Exists(path) ? File.ReadAllLines(path).Length : 0;

        Debug.Log("[SceneInfoToText] Scene info loaded.");
    }

    void Update()
    {
        if (pointCloudInfoText == null) return;

        // === 3. Use Camera.main (head position), fallback to xrRigTransform ===
        Transform cam = Camera.main != null ? Camera.main.transform : xrRigTransform;
        float dist = (pointCloudTransform != null && cam != null)
            ? Vector3.Distance(cam.position, pointCloudTransform.position)
            : -1f;

        string distStr = dist >= 0
            ? dist.ToString("F2", CultureInfo.InvariantCulture) + " m"
            : "N/A";

        // === 4. Update UI Text dynamically ===
        pointCloudInfoText.text =
            $"PC: {pcName}\n" +
            $"Quality: {quality}\n" +
            $"Remaining: {remaining}\n" +
            $"Distance: {distStr}";

#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log($"[SceneInfoToText] Camera position: {(cam != null ? cam.position.ToString("F2") : "null")}, Distance: {distStr}");
#endif
    }
}
