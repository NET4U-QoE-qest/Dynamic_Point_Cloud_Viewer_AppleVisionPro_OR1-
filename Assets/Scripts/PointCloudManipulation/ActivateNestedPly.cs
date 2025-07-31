using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActivateNestedPly : MonoBehaviour
{
    [Header("Durata visibilità di ciascun PLY (in secondi)")]
    public float frameDuration = 1.0f;

    [Header("Nome della scena successiva (vuoto = nessuna)")]
    public string scenaSuccessiva = "";

    private List<GameObject> plyFrames = new();

    void Start()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(includeInactive: true);

        foreach (MeshFilter mf in filters)
        {
            if (mf.gameObject == gameObject) continue;
            GameObject go = mf.gameObject;
            if (!plyFrames.Contains(go))
            {
                plyFrames.Add(go);
                go.SetActive(false);
            }
        }

        if (plyFrames.Count == 0)
        {
            Debug.LogWarning("[PLY] Nessun frame trovato.");
            return;
        }

        Debug.Log($"[PLY] Trovati {plyFrames.Count} frame. Avvio sequenza.");
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        for (int i = 0; i < plyFrames.Count; i++)
        {
            GameObject current = plyFrames[i];
            current.SetActive(true);
            Debug.Log($"[PLY] Frame {i + 1}/{plyFrames.Count}: {current.name}");
            yield return new WaitForSeconds(frameDuration);
            current.SetActive(false);
        }

        // Al termine, carica la scena vuota per gestire il cambio
        if (!string.IsNullOrEmpty(scenaSuccessiva))
        {
            SceneTransitionState.scenaSuccessiva = scenaSuccessiva;
            SceneManager.LoadScene("Scene_Empty", LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("[PLY] Sequenza completata. Nessuna scena successiva specificata.");
        }
    }
}
