using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VirtualKeypad : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField codeInput;
    public TMP_Text errorText;

    // Richiamato dai bottoni numerici 0-9
    public void AddDigit(string digit)
    {
        codeInput.text += digit;
        errorText.text = ""; // pulisci eventuali errori
    }

    // Cancella l'ultimo carattere
    public void Backspace()
    {
        if (codeInput.text.Length > 0)
            codeInput.text = codeInput.text.Substring(0, codeInput.text.Length - 1);
    }

    // Cancella tutto
    public void ClearAll()
    {
        codeInput.text = "";
        errorText.text = "";
    }
}
