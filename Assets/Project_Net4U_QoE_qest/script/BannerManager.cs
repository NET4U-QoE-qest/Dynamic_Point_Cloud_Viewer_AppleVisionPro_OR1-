using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class WelcomeBannerManager : MonoBehaviour
{
    public Canvas bannerCanvas;
    public TMP_InputField codeInputField;
    public Button confirmButton;
    public TMP_Text errorText;
    public TMP_Text instructionsText;
    public Image logoImage;

    [Header("Instructions")]
    [TextArea(2, 8)]
    public string appInstructions;

    public static string UserCode { get; private set; } = "";

    void Start()
    {
        // Set instructions
        if (instructionsText != null)
            instructionsText.text = appInstructions;

        // Center the banner in front of the camera
        PlaceBannerInFrontOfCamera();

        // Clear error and code
        errorText.text = "";
        codeInputField.text = "";

        // Disattiva interazione col resto dell'app
        DisableMainUI();

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmPressed);
    }

    private void PlaceBannerInFrontOfCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float dist = 1.5f;
            Vector3 pos = cam.transform.position + cam.transform.forward * dist;
            pos.y = cam.transform.position.y + 0.5f; // leggermente sopra la vista
            bannerCanvas.transform.position = pos;
            bannerCanvas.transform.LookAt(cam.transform);
            bannerCanvas.transform.Rotate(0, 180, 0); // per orientare verso l’utente
        }
    }

    private void OnConfirmPressed()
    {
        string input = codeInputField.text.Trim();
        // Only numbers, 5 digits
        if (!Regex.IsMatch(input, @"^\d{5}$"))
        {
            errorText.text = "Please enter a 5-digit code (numbers only).";
            return;
        }

        UserCode = input;
        errorText.text = "";
        HideBannerAndEnableApp();
    }

    private void HideBannerAndEnableApp()
    {
        bannerCanvas.gameObject.SetActive(false);
        EnableMainUI();
    }

    private void DisableMainUI()
    {
        // Qui puoi disabilitare altri canvas/UI/tasti mentre il banner è attivo.
        // Ad esempio, puoi trovare la Random UI e metterla inactive.
        // GameObject.Find("RandomButton").SetActive(false);
    }

    private void EnableMainUI()
    {
        // Qui riattivi i controlli principali dell’app.
        // GameObject.Find("RandomButton").SetActive(true);
    }
}
