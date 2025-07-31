using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Scena da caricare dopo la pulizia")]
    public string scenaDaCaricare;

    void Start()
    {
        StartCoroutine(TransizioneCompleta());
    }

    IEnumerator TransizioneCompleta()
    {
        // 1. Pulisce memoria
        Debug.Log("[TRANSITION] Pulizia memoria in corso...");
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return null; // aspetta un frame

        // 2. Carica la nuova scena
        if (!string.IsNullOrEmpty(scenaDaCaricare))
        {
            Debug.Log($"[TRANSITION] Carico nuova scena: {scenaDaCaricare}");
            yield return SceneManager.LoadSceneAsync(scenaDaCaricare, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("[TRANSITION] Nessun nome scena specificato in 'scenaDaCaricare'");
        }
    }
}
