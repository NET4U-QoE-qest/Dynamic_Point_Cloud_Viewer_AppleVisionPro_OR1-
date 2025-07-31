using UnityEngine;
using System.IO;

public class DeleteAlternateFiles : MonoBehaviour
{
    // Inserisci qui il percorso completo o relativo della cartella
    public string folderPath = "Assets/Project_Net4U_QoE_qest/VisionPro/pc/CPCC_Octree_Ready_for_Winter_q5";

    void Start()
    {
        DeleteOneYesOneNo();
    }

    void DeleteOneYesOneNo()
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("La cartella specificata non esiste: " + folderPath);
            return;
        }

        string[] files = Directory.GetFiles(folderPath);
        System.Array.Sort(files); // Ordina per avere un comportamento deterministico

        for (int i = 1; i < files.Length; i += 2)
        {
            try
            {
                File.Delete(files[i]);
                Debug.Log("File eliminato: " + files[i]);
            }
            catch (IOException e)
            {
                Debug.LogError("Errore nell'eliminazione del file: " + files[i] + "\n" + e.Message);
            }
        }
    }
}
