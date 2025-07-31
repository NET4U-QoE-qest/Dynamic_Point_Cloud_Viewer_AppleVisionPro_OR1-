using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.IO;

// [... le altre using ...]

public class AnimatePointCloudBase : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Material vertexColorMaterial;

    public bool showProgressBar = false;
    public Slider progressBar;
    public bool showDebugInfo = true; // ✅ Abilita/disabilita log FPS/tempo

    public int initialPreloadCount = 0;

    private List<string> framePaths;
    private int currentIndex = 0;
    private float frameRate = 13f;
    private bool animate = false;
    private float lastFrameTime = 0f;
    private GameObject currentFrameObject;

    private Dictionary<int, Mesh> frameBuffer = new();
    private int bufferSize = 1;

    public Mesh[] CurrentMeshes { get; private set; }
    public Material[] meshMaterials { get; set; }
    public bool IsAnimating => animate;
    public bool IsMesh { get; private set; } = false;
    public PCMaterialType CurrentMaterial { get; set; }

    private Coroutine preloadCoroutine = null;

    private float animationStartTime = 0f;
    private int displayedFrameCount = 0;
    private float lastLogTime = 0f;

    private void Start()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
        if (vertexColorMaterial == null)
        {
            vertexColorMaterial = Resources.Load<Material>("Materials/PCX_VertexColor");
            if (vertexColorMaterial == null)
                Debug.LogWarning("Material PCX_VertexColor not found in Resources/Materials/");
        }

        if (meshRenderer != null && vertexColorMaterial != null)
        {
            meshRenderer.material = vertexColorMaterial;
            Debug.Log("[AnimatePCBase] Material applied to ROOT on Start: " + meshRenderer.material?.name);
        }
    }

    private void Update()
    {
        if (!animate || framePaths == null || framePaths.Count == 0)
            return;

        float elapsed = Time.time - lastFrameTime;
        if (elapsed >= (1.0f / frameRate))
        {
            lastFrameTime = Time.time;
            int nextIndex = currentIndex + 1;
            if (nextIndex >= framePaths.Count)
            {
                StopAnimation();
                Debug.Log("[AnimatePCBase] Fine sequenza frame!");
                return;
            }
            ShowFrame(nextIndex);

            displayedFrameCount++;

            if (showDebugInfo && Time.time - lastLogTime >= 2f)
            {
                float realElapsed = Time.time - animationStartTime;
                float realFps = displayedFrameCount / realElapsed;
                Debug.Log($"[AnimatePCBase] FPS reale: {realFps:F2}, tempo trascorso: {realElapsed:F2}s");
                lastLogTime = Time.time;
            }
        }
    }

    public void LoadQuality(List<string> plyFramePaths, string folderpath ,bool isHeavy = false)
    {
        StopAllCoroutines();
        ClearCurrentFrame();
        frameBuffer.Clear();
        CurrentMeshes = (plyFramePaths != null) ? new Mesh[plyFramePaths.Count] : null;

        Debug.Log("[AnimatePCBase] Caricamento frame...");

        if (plyFramePaths == null || plyFramePaths.Count == 0)
        {
            Debug.LogWarning("[AnimatePCBase] plyFramePaths è NULL o vuoto.");
            return;
        }

        // --- AUTO STEP
        int step = 1;
        frameRate = 13f;

        string pcName = Path.GetFileNameWithoutExtension(folderpath);
        Debug.Log($"[simone] {pcName}");

        if (pcName.Contains("Octree", StringComparison.OrdinalIgnoreCase))
        {
            step = 2;
            frameRate = 10f;
            Debug.Log("[AutoStep] Octree → step=1, fps=12");
        }
        else if (pcName.Contains("Trisoup", StringComparison.OrdinalIgnoreCase))
        {
            step = 2;
            frameRate = 20f;
            Debug.Log("[AutoStep] Trisoup → step=2, fps=20");
        }
        else if (pcName.Contains("VPCC", StringComparison.OrdinalIgnoreCase))
        {
            step = 2;
            frameRate = 20f;
            Debug.Log("[AutoStep] VPCC → step=2, fps=20");
        }
        else
        {
            step = 2;
            frameRate = 20f;
            Debug.Log("[AutoStep] Tipo sconosciuto → step=1, fps=20");
        }

        // --- FILTRO FRAME
        List<string> filtered = new();
        for (int i = 0; i < plyFramePaths.Count; i += step)
            filtered.Add(plyFramePaths[i]);

        if (filtered.Count == 0 && plyFramePaths.Count > 0)
        {
            filtered.Add(plyFramePaths[0]);
            Debug.LogWarning("[AnimatePCBase] Nessun frame dopo filtro. Uso primo frame come fallback.");
        }

        framePaths = filtered;
        currentIndex = 0;

        if (preloadCoroutine != null)
        {
            StopCoroutine(preloadCoroutine);
            preloadCoroutine = null;
        }

        if (isHeavy)
        {
            ShowFrame(0);
            preloadCoroutine = StartCoroutine(PreloadInitialFramesAndStart());
        }
        else
        {
            ShowFrame(0);
            StartAnimation();
        }
    }

    private IEnumerator PreloadInitialFramesAndStart()
    {
        int preloadCount = Mathf.Min(initialPreloadCount, framePaths.Count);

        if (showProgressBar && progressBar != null)
        {
            progressBar.value = 0f;
            progressBar.maxValue = preloadCount;
            progressBar.gameObject.SetActive(true);
        }

        for (int i = 0; i < preloadCount; i++)
        {
            if (!frameBuffer.ContainsKey(i))
                frameBuffer[i] = RuntimePLYLoader.LoadMeshFromPLY(framePaths[i]);
            if (showProgressBar && progressBar != null)
                progressBar.value = i + 1;
            yield return null;
        }

        if (showProgressBar && progressBar != null)
            progressBar.gameObject.SetActive(false);

        StartAnimation();

        for (int i = preloadCount; i < framePaths.Count; i++)
        {
            if (!frameBuffer.ContainsKey(i))
                frameBuffer[i] = RuntimePLYLoader.LoadMeshFromPLY(framePaths[i]);
            if (i % 10 == 0)
                yield return null;
        }
    }

    public void ShowFrame(int idx)
    {
        if (framePaths == null || framePaths.Count == 0) return;
        ClearCurrentFrame();

        if (idx < 0 || idx >= framePaths.Count)
            idx = 0;

        Mesh mesh = RuntimePLYLoader.LoadMeshFromPLY(framePaths[idx]);
        if (mesh == null)
        {
            Debug.LogWarning($"[AnimatePCBase] Mesh null for frame {idx} at {framePaths[idx]}");
            return;
        }

        currentFrameObject = new GameObject("PointCloudFrame");
        currentFrameObject.transform.SetParent(this.transform, false);
        var mf = currentFrameObject.AddComponent<MeshFilter>();
        var mr = currentFrameObject.AddComponent<MeshRenderer>();
        mf.mesh = mesh;

        if (vertexColorMaterial != null)
            mr.material = vertexColorMaterial;

        currentIndex = idx;

        if (CurrentMeshes != null && idx < CurrentMeshes.Length)
            CurrentMeshes[idx] = null;

        frameBuffer.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private void ClearCurrentFrame()
    {
        if (currentFrameObject != null)
        {
            Destroy(currentFrameObject);
            currentFrameObject = null;
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }

    public void StartAnimation()
    {
        animate = true;
        lastFrameTime = Time.time;
        animationStartTime = Time.time;
        displayedFrameCount = 0;
        lastLogTime = Time.time;

        if (showDebugInfo)
        {
            float estimatedDuration = framePaths.Count / frameRate;
            Debug.Log($"[AnimatePCBase] Animazione avviata: {framePaths.Count} frame, {frameRate} fps → durata stimata: {estimatedDuration:F2}s");
        }
    }

    public void StopAnimation() => animate = false;

    public void SetAnimate(bool active)
    {
        if (active) StartAnimation();
        else StopAnimation();
    }

    public void SetIsMesh(bool useMesh) => IsMesh = useMesh;

    public void SetCurrentQuality(int quality) { /* opzionale */ }
}
