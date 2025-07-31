using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System;
using System.Globalization;

public class QoESliderManager : MonoBehaviour
{
    public Slider qoeSlider;
    public TMP_Text numericValueText;
    public TMP_Text pcInfoText;

    public Button buttonNext;
    public Button buttonPrev;
    public Button buttonSubmit;

    public TextMeshProUGUI saveNotification;

    // Info PointCloud
    private string currentPCName = "N/A";
    private string currentQuality = "N/A";
    private string currentDistance = "N/A";
    private string currentUserCode = "N/A";
    private int currentRemaining = 0;

    private bool isSliderUpdating = false;
    private bool hasSubmittedRecently = false;

    private Coroutine notificationCoroutine;

    [Header("User Info")]
    public TMP_Text userCodeText;

    void Start()
    {
        // RESET all fields to N/A at startup
        Debug.Log("[QoESliderManager] (Start) -- Reset all to N/A via ForceResetPCInfoToNA");
        ForceResetPCInfoToNA();

        UpdateNumericValueText();
        qoeSlider.onValueChanged.AddListener(delegate { UpdateNumericValueText(); });

        buttonNext.onClick.RemoveAllListeners();
        buttonNext.onClick.AddListener(IncrementSlider);

        buttonPrev.onClick.RemoveAllListeners();
        buttonPrev.onClick.AddListener(DecrementSlider);

        buttonSubmit.onClick.RemoveAllListeners();
        buttonSubmit.onClick.AddListener(SubmitEvaluation);

        StartCoroutine(ForceInitialUpdate());

        Debug.Log($"[QoESliderManager] Submit button listeners: {buttonSubmit.onClick.GetPersistentEventCount()}");
    }

    private IEnumerator ForceInitialUpdate()
    {
        yield return null;
        UpdateNumericValueText();
    }

    private void UpdateNumericValueText()
    {
        numericValueText.text = $"{qoeSlider.value}/5";
    }

    /// <summary>
    /// RESET all fields to N/A and update UI (call ONLY at startup or on full reset)
    /// </summary>
    public void ForceResetPCInfoToNA()
    {
        currentUserCode = "N/A";
        currentPCName = "N/A";
        currentQuality = "N/A";
        currentDistance = "N/A";
        currentRemaining = 0;

        Debug.Log("[QoESliderManager] (ForceResetPCInfoToNA) All values set to N/A");
        RefreshPCInfoText();
    }

    /// <summary>
    /// Redraws the PC info UI with current fields
    /// </summary>
    private void RefreshPCInfoText()
    {
        string remStr = currentRemaining <= 0 ? "N/A" : $"{currentRemaining}/48";
        if (pcInfoText != null)
            pcInfoText.text = $"PC: {currentPCName}\n"
                            + $"Quality: {currentQuality}\n"
                            + $"Distance: {currentDistance}\n"
                            + $"Remaining: {remStr}";

        Debug.Log($"[QoESliderManager] --> RefreshPCInfoText: PC={currentPCName}, Quality={currentQuality}, Distance={currentDistance}, Remaining={remStr}\n---Stack---\n{Environment.StackTrace}");
    }


    /// <summary>
    /// Updates PC info with new values and redraws UI
    /// </summary>
    public void UpdatePCInfo(string pcName, string quality, string distance)
    {
        Debug.Log($"[QoESliderManager] CHIAMATO UpdatePCInfo: pcName={pcName}, quality={quality}, distance={distance}\n---Stack---\n{Environment.StackTrace}");

        if (pcInfoText == null)
        {
            Debug.LogError("[QoESliderManager] pcInfoText non è assegnato!");
            return;
        }

        // Only use GoalManager.UserCode!
        currentUserCode = GoalManager.UserCode;

        currentPCName = pcName;
        currentQuality = quality;
        currentDistance = distance;

        var randomPlayer = FindObjectOfType<RandomPointCloudPlayer>();
        currentRemaining = randomPlayer != null ? randomPlayer.RemainingPCCount : 0;

        RefreshPCInfoText();
        Debug.Log("[QoESliderManager] (UpdatePCInfo) Forcing refresh of TMP text for PC Info.");
        pcInfoText.ForceMeshUpdate(); // <-- FORZA IL REFRESH TMP
    }

    public void UpdateUserCodeInfo(string userCode)
    {
        if (userCodeText != null)
            userCodeText.text = $"User: {userCode}";
        Debug.Log($"[QoESliderManager] UpdateUserCodeInfo: {userCode}");
    }

    public void UpdatePCInfoFromController(PointCloudController pcController, float distance)
    {
        if (pcController == null || pcController.data == null)
        {
            Debug.Log("[QoESliderManager] UpdatePCInfoFromController: pcController o data è null!");
            return;
        }
        string pcName = pcController.data.pcName ?? "N/A";
        string quality = $"q{pcController.CurrentQuality}";
        string distStr = $"{distance:F2}m";
        Debug.Log($"[QoESliderManager] UpdatePCInfoFromController: pcName={pcName}, quality={quality}, dist={distStr}");
        UpdatePCInfo(pcName, quality, distStr);
    }


    public void IncrementSlider()
    {
        if (isSliderUpdating) return;
        StartCoroutine(SliderCooldown());

        if (qoeSlider.value < qoeSlider.maxValue)
            qoeSlider.value++;
    }

    public void DecrementSlider()
    {
        if (isSliderUpdating) return;
        StartCoroutine(SliderCooldown());

        if (qoeSlider.value > qoeSlider.minValue)
            qoeSlider.value--;
    }

    private IEnumerator SliderCooldown()
    {
        isSliderUpdating = true;
        yield return new WaitForSeconds(0.25f);
        isSliderUpdating = false;
    }

    private float GetCurrentDistanceToPC()
    {
        var randomPlayer = FindObjectOfType<RandomPointCloudPlayer>();
        if (randomPlayer == null || randomPlayer.CurrentInstance == null)
            return -1f;
        Vector3 camPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        Vector3 pcPos = randomPlayer.CurrentInstance.transform.position;
        return Vector3.Distance(camPos, pcPos);
    }

    // Aggiorna SOLO il campo distanza nel menu, ogni frame
    void Update()
    {
        // Always refresh user code field
        if (userCodeText != null)
            userCodeText.text = $"User: {GoalManager.UserCode}";

        var randomPlayer = FindObjectOfType<RandomPointCloudPlayer>();
        if (randomPlayer != null && randomPlayer.IsRunning() && randomPlayer.CurrentInstance != null)
        {
            float d = GetCurrentDistanceToPC();
            string newDist = d < 0 ? "N/A" : $"{d:F2}m";
            if (newDist != currentDistance)
            {
                currentDistance = newDist;
                // Aggiorna solo la linea della distanza
                if (pcInfoText != null)
                {
                    string[] lines = pcInfoText.text.Split('\n');
                    if (lines.Length == 4)
                    {
                        lines[2] = $"Distance: {currentDistance}";
                        pcInfoText.text = string.Join("\n", lines);
                    }
                }
                Debug.Log($"[QoESliderManager] (Update) Distance dynamically updated: {currentDistance}");
            }
        }
    }

    // Utility per forzare aggiornamento UI a inizio random/inizio sessione
    public void ForceRefreshPCInfo()
    {
        Debug.Log("[QoESliderManager] (ForceRefreshPCInfo) Refreshing UI with current fields");
        RefreshPCInfoText();
    }

    public void SubmitEvaluation()
    {
        if (hasSubmittedRecently) return;
        hasSubmittedRecently = true;
        StartCoroutine(ResetSubmitCooldown());

        int score = Mathf.RoundToInt(qoeSlider.value);
        float distanceToPC = GetCurrentDistanceToPC();
        //string distStr = (distanceToPC < 0) ? "N/A" : $"{distanceToPC:F2}m";
        //impostiamo il . al posto della virgola nel csv
        string distStr = (distanceToPC < 0)
            ? "N/A"
            : distanceToPC.ToString("F2", CultureInfo.InvariantCulture) + "m";
        currentDistance = distStr;

        string userCode = !string.IsNullOrEmpty(GoalManager.UserCode)
            ? GoalManager.UserCode
            : PlayerPrefs.GetString("UserCode", "N/A");

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Pulizia e fallback campi
        string pcName = string.IsNullOrEmpty(currentPCName) ? "N/A" : currentPCName;
        string compressionLevel = string.IsNullOrEmpty(currentQuality) ? "N/A" : currentQuality;
        string distance = string.IsNullOrEmpty(currentDistance) ? "N/A" : currentDistance;
        string remainingStr = currentRemaining > 0 ? currentRemaining.ToString() : "N/A";
        string vote = score.ToString(); // voto da 1 a 5

        // Intestazione corretta (solo se nuovo file)
        string header = "timestamp,User,PC,livello_di_compressione,distanza,qualità,Point_cloud_restanti";
        string line = $"{timestamp},{userCode},{pcName},{compressionLevel},{distance},{vote},{remainingStr}";

        string filename = "QoE_evaluation.csv";
        string path = Path.Combine(Application.persistentDataPath, filename);

        try
        {
            if (!File.Exists(path))
                File.AppendAllText(path, header + Environment.NewLine);

            File.AppendAllText(path, line + Environment.NewLine);
            Debug.Log($"[QoESliderManager] Evaluation saved:\n{line}\nPath: {path}");

            if (saveNotification != null)
            {
                saveNotification.text = "Evaluation saved!";
                if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
                notificationCoroutine = StartCoroutine(HideNotificationRoutine(1f));
            }

            var randomPlayer = FindObjectOfType<RandomPointCloudPlayer>();
            if (randomPlayer != null && randomPlayer.IsRunning())
                randomPlayer.SubmitAndContinue();
        }
        catch (Exception e)
        {
            Debug.LogError($"[QoESliderManager] Failed to save evaluation: {e.Message}");
        }
    }


    private IEnumerator ResetSubmitCooldown()
    {
        yield return new WaitForSeconds(0.3f);
        hasSubmittedRecently = false;
    }

    private IEnumerator HideNotificationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        Debug.Log("[QoESliderManager] Hiding save notification");
        if (saveNotification != null)
        {
            saveNotification.text = "";
        }
        notificationCoroutine = null;
    }
}
