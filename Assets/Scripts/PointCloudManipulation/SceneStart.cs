using System.Collections.Generic;

using System.IO;

using UnityEngine;

public class SceneStart : MonoBehaviour

{

    void Start()

    {

        Debug.Log("[SceneStart] Inizializzazione della lista delle scene...");

        string runtimePath = Path.Combine(Application.persistentDataPath, "scenes_runtime.txt");

        // Se il file già esiste e non è vuoto, non fare nulla

        if (File.Exists(runtimePath))

        {

            string[] existingLines = File.ReadAllLines(runtimePath);

            if (existingLines.Length > 0)

            {

                Debug.Log($"[SceneStart] File scenes_runtime.txt già esistente con {existingLines.Length} scene. Nessuna rigenerazione necessaria.");

                return;

            }

            else

            {

                Debug.LogWarning("[SceneStart] File scenes_runtime.txt trovato ma vuoto. Procedo con rigenerazione...");

            }

        }

        else

        {

            Debug.Log("[SceneStart] File scenes_runtime.txt non trovato. Procedo con generazione...");

        }

        // Carica il file originale da Resources

        TextAsset file = Resources.Load<TextAsset>("scenes_list");

        if (file == null)

        {

            Debug.LogError("[SceneStart] ERRORE: Impossibile caricare scenes_list.txt da Resources.");

            return;

        }

        string[] lines = file.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)

        {

            Debug.LogError("[SceneStart] Il file scenes_list.txt è vuoto.");

            return;

        }

        List<string> scenes = new List<string>(lines);

        Shuffle(scenes);

        File.WriteAllLines(runtimePath, scenes.ToArray());

        Debug.Log($"[SceneStart] Lista scene mescolata salvata in {runtimePath}");

    }

    void Shuffle(List<string> list)

    {

        for (int i = 0; i < list.Count; i++)

        {

            int rnd = Random.Range(i, list.Count);

            (list[i], list[rnd]) = (list[rnd], list[i]);

        }

        Debug.Log("[SceneStart] Shuffle completato.");

    }

}

