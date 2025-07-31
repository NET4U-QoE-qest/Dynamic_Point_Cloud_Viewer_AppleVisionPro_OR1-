using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    // Questo metodo può essere assegnato al bottone tramite l’Inspector
    public void CaricaSceneEmpty()
    {
        SceneManager.LoadScene("scene_empty_my", LoadSceneMode.Single);
    }

    public void CaricaTestScene()
    {
        SceneManager.LoadScene("Octree_prova_q10", LoadSceneMode.Single);
    }
}
