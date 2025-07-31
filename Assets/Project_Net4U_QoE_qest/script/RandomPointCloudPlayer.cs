using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RandomPointCloudPlayer : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject pointCloudPrefab;
    public Transform spawnAnchor;
    public TextMeshProUGUI randomStatusBanner;

    [Header("Settings")]
    public float distanceFromCamera = 2f;

    // PRIVATE
    private List<(string pcName, int quality)> remainingCombos = new();
    private string basePath;
    private string queueFilePath;
    private System.Random random = new();
    private GameObject currentInstance;

    private bool isRunning = false;
    private bool cleanedOldPC = false; // Only once at random mode start

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        basePath = Path.Combine(Application.persistentDataPath, "PointClouds");
#else
        basePath = Path.Combine(Application.dataPath, "PointClouds");
#endif
        queueFilePath = Path.Combine(Application.persistentDataPath, "random_eval_queue.txt");
    }

    public bool IsRunning() => isRunning;

    /// <summary>
    /// Avvia la sequenza random (o la riprende da file se presente)
    /// </summary>
    public void StartRandomSequence()
    {
        // Blocca avvio random se user code non valido
        if (string.IsNullOrEmpty(GoalManager.UserCode) || GoalManager.UserCode == "N/A")
        {
            ShowBanner("Insert your user code first!", 2f);
            Debug.LogWarning("[RandomPlayer] Tried to start random sequence, but user code not set!");
            return;
        }

        if (isRunning) return;

        // Destroy all point clouds in scene (just once)
        if (!cleanedOldPC)
        {
            foreach (var pc in FindObjectsOfType<PointCloudController>())
                Destroy(pc.gameObject);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();

            cleanedOldPC = true;
        }

        if (!Directory.Exists(basePath))
        {
            Debug.LogError("[RandomPlayer] PointClouds directory not found!");
            return;
        }

        // --- Load from file, or create/shuffle and save new list ---
        if (File.Exists(queueFilePath))
        {
            LoadQueueFromFile();
            Debug.Log("[RandomPlayer] Loaded evaluation queue from file.");
        }
        else
        {
            remainingCombos.Clear();

            // 1. Aggiungi SEMPRE BlueSpin q2 come primo (nome completo)
            remainingCombos.Add(("GPCC_Octree_BlueSpin", 3));

            // 2. Crea la lista completa di tutti i PC/quality (compresi altri BlueSpin q2)
            var tempCombos = new List<(string pcName, int quality)>();
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                string pcName = Path.GetFileName(dir);
                for (int q = 1; q <= 5; q++)
                {
                    string qDir = Path.Combine(dir, $"q{q}", "PointClouds");
                    if (Directory.Exists(qDir) && Directory.GetFiles(qDir, "*.ply").Length > 0)
                        tempCombos.Add((pcName, q));
                }
            }
            Shuffle(tempCombos);
            remainingCombos.AddRange(tempCombos);

            if (remainingCombos.Count == 0)
            {
                ShowBanner("No point clouds available.", 3f);
                return;
            }
            SaveQueueToFile(); // salva la lista finale
        }

        // Disattiva i dropdown e il bottone Start solo nella mano destra
        var drop = FindObjectOfType<PointCloudDropdownManager>();
        if (drop != null)
        {
            drop.DisableDropdownListeners(); // 
            drop.HideDropdownsAndStartButton(); // 
        }

        isRunning = true;
        ShowBanner("Starting random PC mode...", 2f);

        // Carica il PRIMO della lista SOLO QUI
        LoadNextRandomPointCloud();
        ShowBanner("Try a review of this test PC, then click Submit.", 5f);
    }


    /// <summary>
    /// Chiamato dal QoE manager dopo submit.
    /// </summary>
    public void SubmitAndContinue()
    {
        if (!isRunning) return;

        if (currentInstance != null)
            Destroy(currentInstance);
        Resources.UnloadUnusedAssets();
        System.GC.Collect();


        // RIMUOVI SOLO il PRIMO dopo submit
        if (remainingCombos.Count > 0)
            remainingCombos.RemoveAt(0);

        SaveQueueToFile();

        // Carica il prossimo SOLO SE esiste
        if (remainingCombos.Count > 0)
        {
            //ShowBanner(" Loading next...", 2f);
            LoadNextRandomPointCloud();
        }
        else
        {
            ShowBanner("Sequence complete.", 3f);
            isRunning = false;
            cleanedOldPC = false;
            if (File.Exists(queueFilePath))
                File.Delete(queueFilePath);
        }
    }


    /// <summary>
    /// Carica il prossimo point cloud random e aggiorna la UI.
    /// </summary>
    private void LoadNextRandomPointCloud()
    {
        if (currentInstance != null)
        {
            Destroy(currentInstance);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            currentInstance = null;
        }

        if (remainingCombos.Count == 0)
        {
            ShowBanner("Sequence complete.", 3f);
            isRunning = false;
            cleanedOldPC = false;
            if (File.Exists(queueFilePath))
                File.Delete(queueFilePath);
            return;
        }

        var combo = remainingCombos[0];

        // Instanzia prefab e carica PC
        currentInstance = Instantiate(pointCloudPrefab);
        var controller = currentInstance.GetComponent<PointCloudController>();
        if (controller != null)
        {
            controller.LoadPointCloud(combo.pcName, combo.quality);
            Debug.Log("[RandomPlayer] LoadPointCloud() chiamato.");
            Debug.Log($"[RandomPlayer] Dopo LoadPointCloud: controller.data?.pcName={controller.data?.pcName}, CurrentQuality={controller.CurrentQuality}");

            controller.transform.position = GetSpawnPosition();

            Debug.Log($"[RandomPlayer] Aggiorno UI: {combo.pcName}, q{combo.quality}, {distanceFromCamera}m");

            // Aggiorna SUBITO la UI, così la QoE è allineata per la prima valutazione
            var qoe = FindObjectOfType<QoESliderManager>();
            if (qoe != null)
            {
                float dist = Vector3.Distance(Camera.main.transform.position, controller.transform.position);
                qoe.UpdatePCInfoFromController(controller, dist);
            }

            // Poi comunque tieni la coroutine per gestire eventuali update di posizione dopo un frame
            StartCoroutine(DelayedQoEInfoUpdate(controller));
        }
        else
        {
            Debug.LogError("[RandomPlayer] PointCloudController missing on prefab!");
        }
    }

    // Nuova routine per aggiornare la UI dopo un frame
    private IEnumerator DelayedQoEInfoUpdate(PointCloudController controller)
    {
        yield return null; // aspetta un frame, ora la posizione è aggiornata
        var qoe = FindObjectOfType<QoESliderManager>();
        if (qoe != null && controller != null)
        {
            float dist = Vector3.Distance(Camera.main.transform.position, controller.transform.position);
            string distStr = $"{dist:F2}m";
            Debug.Log($"[RandomPlayer] (DelayedQoEInfoUpdate) PC={controller.data?.pcName} Quality=q{controller.CurrentQuality} Distance={distStr}");
            qoe.UpdatePCInfoFromController(controller, dist);
        }
    }

    // ----------------- FILE MANAGEMENT ------------------

    private void SaveQueueToFile()
    {
        var lines = new List<string>();
        lines.Add("GPCC_Octree_BlueSpin,3"); // sempre in cima
        foreach (var (pcName, quality) in remainingCombos)
            lines.Add($"{pcName},{quality}");

        File.WriteAllLines(queueFilePath, lines);
    }

    private void LoadQueueFromFile()
    {
        remainingCombos.Clear();
        if (File.Exists(queueFilePath))
        {
            var lines = File.ReadAllLines(queueFilePath);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[1], out int q))
                    remainingCombos.Add((parts[0], q));
            }
        }
    }

    // ---------------------------------------------------------

    public void UpdateQoEPCInfoForCurrent()
    {
        var qoe = FindObjectOfType<QoESliderManager>();
        if (qoe == null)
        {
            Debug.Log("[RandomPlayer] QoESliderManager NON trovato!");
            return;
        }
        if (CurrentInstance == null)
        {
            Debug.Log("[RandomPlayer] CurrentInstance è NULL, nessun PC visualizzato.");
            return;
        }

        var controller = CurrentInstance.GetComponent<PointCloudController>();
        if (controller != null)
        {
            string pcName = controller.data?.pcName ?? "N/A";
            string quality = $"q{controller.CurrentQuality}";
            string distStr = $"{distanceFromCamera:F2}m";
            Debug.Log($"[RandomPlayer] Aggiorno QoE: {pcName}, {quality}, {distStr}");
            qoe.UpdatePCInfo(pcName, quality, distStr);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return Vector3.zero;
        Vector3 forward = camera.transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 position = camera.transform.position + forward * distanceFromCamera;
        position.y = 0f; //height from the floor
        return position;
    }

    private void ShowBanner(string message, float duration)
    {
        if (randomStatusBanner == null) return;
        StopAllCoroutines();
        StartCoroutine(BannerRoutine(message, duration));
    }

    private IEnumerator BannerRoutine(string message, float duration)
    {
        randomStatusBanner.text = message;
        randomStatusBanner.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        randomStatusBanner.gameObject.SetActive(false);
    }

    // Fisher–Yates shuffle
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public GameObject CurrentInstance => currentInstance;

    public int RemainingPCCount
    {
        get { return remainingCombos.Count; }
    }
}
