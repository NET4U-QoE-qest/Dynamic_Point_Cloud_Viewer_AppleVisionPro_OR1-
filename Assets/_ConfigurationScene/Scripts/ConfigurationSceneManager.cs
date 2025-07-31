using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ConfigurationSceneManager : MonoBehaviour
{
    public static ConfigurationSceneManager Instance { get; private set; }

    public GameObject firstPointCloud;
    public GameObject pointCloudPrefab;
    public TextMeshPro currentPcText;

    private GameObject currentWorkingPointCloud;
    private List<GameObject> pointCloudGameObjects;

    public float objectsDistanceOffset = 2.0f;
    public int maxNumPointClouds = 4;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        pointCloudGameObjects = new List<GameObject> { firstPointCloud };
        currentWorkingPointCloud = firstPointCloud;
        SetupCurrentWorkingObject();
    }

    private void InstantiateNewObject()
    {
        GameObject refObj = pointCloudGameObjects.Last();

        GameObject tempPC = Instantiate(pointCloudPrefab, refObj.transform.parent);
        tempPC.transform.localPosition = new Vector3(refObj.transform.localPosition.x + objectsDistanceOffset,
                                                     refObj.transform.localPosition.y,
                                                     refObj.transform.localPosition.z);

        pointCloudGameObjects.Add(tempPC);
        currentWorkingPointCloud = tempPC;
        SetupCurrentWorkingObject();
    }

    private void SetupCurrentWorkingObject()
    {
        string firstPC = PointCloudsLoader.Instance.pcObjects.FirstOrDefault()?.pcName;
        if (!string.IsNullOrEmpty(firstPC))
        {
            ChangeCurrentPCObject(firstPC);
        }

        MaterialChanger.Instance.OnPointButtonPressed();
        DistanceChanger.Instance.OnDistanceSliderUpdated();
    }

    private void ChangeConfigurationFrontend()
    {
        AnimatePointCloudBase anim = currentWorkingPointCloud.GetComponent<AnimatePointCloudBase>();
        ObjectChanger.Instance.HighlightSelectedObject(anim.name);

        // Non abbiamo più CurrentMaterial come property personalizzata,
        // possiamo solo passare il materiale attualmente assegnato.
        var meshRenderer = currentWorkingPointCloud.GetComponent<MeshRenderer>();
        Material mat = meshRenderer != null && meshRenderer.sharedMaterial != null ? meshRenderer.sharedMaterial : null;
        MaterialChanger.Instance.HighlightSelectedMaterial(PCMaterialType.PointCloud);



        // Non abbiamo più IsAnimating come property personalizzata,
        // quindi forziamo a true se serve aggiornare l'interfaccia.
        PlayPauseChanger.Instance.UpdatePausedStateOfNewSelectedObject(true);

        // Interactable
        InteractionChanger.Instance.UpdateInteractableStateOfNewSelectedObject(currentWorkingPointCloud.GetComponent<BoxCollider>().enabled);
        ChangeCurrentPCText();
    }

    public void OnAddPointCloudButtonPressed()
    {
        if (pointCloudGameObjects.Count >= maxNumPointClouds)
            return;

        InstantiateNewObject();
    }

    public void OnRemovePointCloudButtonPressed()
    {
        if (pointCloudGameObjects.Count <= 1)
            return;

        pointCloudGameObjects.Remove(currentWorkingPointCloud);
        Destroy(currentWorkingPointCloud);
        currentWorkingPointCloud = pointCloudGameObjects.Last();
        ChangeConfigurationFrontend();
    }

    public void OnChangeActivePointCloudButtonsPressed(bool increment)
    {
        int currentIndex = pointCloudGameObjects.IndexOf(currentWorkingPointCloud);

        if (increment)
        {
            currentIndex += 1;
            currentWorkingPointCloud = pointCloudGameObjects.ElementAtOrDefault(currentIndex) ?? pointCloudGameObjects.First();
        }
        else
        {
            currentIndex -= 1;
            currentWorkingPointCloud = pointCloudGameObjects.ElementAtOrDefault(currentIndex) ?? pointCloudGameObjects.Last();
        }

        ChangeConfigurationFrontend();
    }

    private void ChangeCurrentPCText()
    {
        AnimatePointCloudBase anim = currentWorkingPointCloud.GetComponent<AnimatePointCloudBase>();
        currentPcText.text = anim.name;
    }

    public void ChangePCAnimationPaused(bool paused)
    {
        if (currentWorkingPointCloud != null)
        {
            currentWorkingPointCloud.GetComponent<AnimatePointCloudBase>().SetAnimate(!paused);
        }
    }

    public void ChangePCInteractable(bool interactable)
    {
        currentWorkingPointCloud.GetComponent<BoxCollider>().enabled = interactable;
    }

    public void ChangeCurrentPCQuality(int quality)
    {
        if (currentWorkingPointCloud != null)
        {
            // Se serve gestire la qualità, chiama semplicemente una funzione custom che hai messo in AnimatePointCloudBase
            // oppure aggiorna la qualità come preferisci.
            // Se hai rimosso la funzione SetCurrentQuality, questa riga puoi toglierla.
            // currentWorkingPointCloud.GetComponent<AnimatePointCloudBase>().SetCurrentQuality(quality);
        }
    }

    public void ChangeCurrentPCDistance(float distance)
    {
        if (currentWorkingPointCloud != null)
        {
            currentWorkingPointCloud.transform.localRotation = Quaternion.Euler(0f, -180f, 0f);
            currentWorkingPointCloud.transform.localPosition = new Vector3(currentWorkingPointCloud.transform.localPosition.x,
                                                                           currentWorkingPointCloud.transform.localPosition.y,
                                                                           distance);
        }
    }

    public void ChangeCurrentPCObject(string pcName)
    {
        if (currentWorkingPointCloud != null)
        {
            AnimatePointCloudBase anim = currentWorkingPointCloud.GetComponent<AnimatePointCloudBase>();
            anim.name = pcName;
            ChangeConfigurationFrontend();
        }
    }

    public void ChangeCurrentPCMaterial(PCMaterialType type, Material currMat = null)
    {
        if (currentWorkingPointCloud != null)
        {
            var meshRenderer = currentWorkingPointCloud.GetComponent<MeshRenderer>();
            if (type == PCMaterialType.Mesh)
            {
                // Se vuoi gestire la modalità mesh, puoi aggiungere una flag custom.
                // Altrimenti lascia solo il cambio materiale qui:
                // (nessun SetIsMesh necessario)
            }
            else
            {
                // Cambia materiale solo se fornito
                if (currMat != null && meshRenderer != null)
                {
                    meshRenderer.materials = new Material[] { currMat };
                }
            }
        }
    }
}
