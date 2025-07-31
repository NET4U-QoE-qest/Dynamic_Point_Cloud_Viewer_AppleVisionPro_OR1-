using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManagerFromList : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(TransizioneCompleta());
    }

    IEnumerator TransizioneCompleta()
    {
        // 1. Pulisce la memoria
        Debug.Log("[TRANSITION] Pulizia memoria in corso...");
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return null; // aspetta un frame

        // 2. Legge la lista da persistentDataPath
        string path = Path.Combine(Application.persistentDataPath, "scenes_runtime.txt");
        if (!File.Exists(path))
        {
            Debug.LogError("[TRANSITION] Il file scenes_runtime.txt non esiste.");
            yield break;
        }

        List<string> sceneList = new List<string>(File.ReadAllLines(path));
        if (sceneList.Count == 0)
        {
            Debug.Log("[TRANSITION] Nessuna scena rimasta. Fine esperienza.");
            yield break;
        }

        // 3. Prende e rimuove la prima scena
        string scenaDaCaricare = sceneList[0];
        sceneList.RemoveAt(0);
        File.WriteAllLines(path, sceneList.ToArray());

        Debug.Log($"[TRANSITION] Carico scena: {scenaDaCaricare}");

        // 4. Carica la scena
        yield return SceneManager.LoadSceneAsync(scenaDaCaricare, LoadSceneMode.Single);
    }
}
