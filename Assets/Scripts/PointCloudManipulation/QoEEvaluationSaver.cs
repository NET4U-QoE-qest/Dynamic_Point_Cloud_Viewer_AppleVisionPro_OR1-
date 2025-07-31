using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class QoEEvaluationSaver : MonoBehaviour
{
    [Header("UI References")]
    public TMPro.TMP_Text saveNotification;
    public UnityEngine.UI.Slider qoeSlider;
    public TextMeshProUGUI sliderValueText;

    [Header("Distanza dinamica (calcolata solo al Submit)")]
    public Transform pointCloudTransform;
    public Transform xrRigTransform;

    [Header("Submit cooldown (in sec)")]
    public float submitCooldown = 1f;

    private bool hasSubmittedRecently = false;
    private Coroutine notificationCoroutine;
    private int lastShownValue = -1;

    void Start()
    {
        if (qoeSlider == null)
            Debug.LogError("[QoEEvaluationSaver] QoE slider not assigned.");
    }

    void Update()
    {
        // Aggiorna il testo dello slider solo se cambia il valore
        if (sliderValueText != null && qoeSlider != null)
        {
            int currentValue = Mathf.RoundToInt(qoeSlider.value);
            if (currentValue != lastShownValue)
            {
                sliderValueText.text = currentValue.ToString();
                lastShownValue = currentValue;
            }
        }
    }

    public void SubmitEvaluation()
    {
        if (hasSubmittedRecently) return;
        hasSubmittedRecently = true;
        StartCoroutine(ResetSubmitCooldown());

        // === Info utente ===
        string userCode = !string.IsNullOrEmpty(GoalManager.UserCode)
            ? GoalManager.UserCode
            : PlayerPrefs.GetString("UserCode", "N/A");

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        int score = Mathf.RoundToInt(qoeSlider.value);

        // === Info scena ===
        string sceneName = SceneManager.GetActiveScene().name;
        string pcName = "N/A";
        string compressionLevel = "N/A";

        if (sceneName.Contains("_q"))
        {
            int qIndex = sceneName.LastIndexOf("_q");
            pcName = sceneName.Substring(0, qIndex);
            compressionLevel = sceneName.Substring(qIndex + 1);
        }

        // === Scene rimanenti ===
        string txtPath = Path.Combine(Application.persistentDataPath, "scenes_runtime_prova.txt");
        int remaining = File.Exists(txtPath) ? File.ReadAllLines(txtPath).Length : 0;
        string remainingStr = remaining > 0 ? remaining.ToString() : "N/A";

        // === Calcola distanza al momento del Submit ===
        Transform cam = xrRigTransform != null ? xrRigTransform : Camera.main?.transform;

        float distance = (pointCloudTransform != null && cam != null)
            ? Vector3.Distance(cam.position, pointCloudTransform.position)
            : -1f;

        string distanceStr = (distance < 0)
            ? "N/A"
            : distance.ToString("F2", CultureInfo.InvariantCulture) + "m";

        // === Riga CSV ===
        string header = "timestamp,User,PC,livello_di_compressione,distanza,qualità,Point_cloud_restanti";
        string line = $"{timestamp},{userCode},{pcName},{compressionLevel},{distanceStr},{score},{remainingStr}";

        string filename = "QoE_evaluation.csv";
        string path = Path.Combine(Application.persistentDataPath, filename);

        try
        {
            if (!File.Exists(path))
                File.AppendAllText(path, header + Environment.NewLine);

            File.AppendAllText(path, line + Environment.NewLine);
            Debug.Log($"[QoEEvaluationSaver] Evaluation saved:\n{line}\nPath: {path}");

            if (saveNotification != null)
            {
                saveNotification.text = "Evaluation saved!";
                if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
                notificationCoroutine = StartCoroutine(HideNotificationRoutine(1f));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[QoEEvaluationSaver] Failed to save evaluation: {e.Message}");
        }
    }

    private System.Collections.IEnumerator ResetSubmitCooldown()
    {
        yield return new WaitForSeconds(submitCooldown);
        hasSubmittedRecently = false;
    }

    private System.Collections.IEnumerator HideNotificationRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (saveNotification != null)
            saveNotification.text = "";
    }
}
