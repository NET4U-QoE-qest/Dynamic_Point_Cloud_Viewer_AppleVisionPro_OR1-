using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public TMP_Dropdown pointCloudDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle meshToggle;
    public Button playButton;
    public Button resetButton;

    private PointCloudController currentPC;
    private PointCloudsLoader loader;
    private Dictionary<string, PointCloudObject> nameToData = new Dictionary<string, PointCloudObject>();

    private void Start()
    {
        loader = PointCloudsLoader.Instance;

        if (loader == null)
        {
            Debug.LogError("PointCloudsLoader.Instance is null! Make sure it exists in the scene.");
            return;
        }

        if (loader.pcObjects.Count > 0)
        {
            Debug.Log("[UIManager] Data already present, populating dropdowns immediately.");
            PopulateDropdowns();
        }
        else
        {
            Debug.Log("[UIManager] No data loaded yet, subscribing to OnLoaded event.");
            PointCloudsLoader.OnLoaded += PopulateDropdowns;
        }
    }

    void PopulateDropdowns()
    {
        Debug.Log("[UIManager] Populating dropdowns.");
        nameToData.Clear();
        List<string> names = new List<string>();

        foreach (var pc in PointCloudsLoader.Instance.pcObjects)
        {
            Debug.Log($"[UIManager] Found object: {(pc == null ? "null" : pc.ToString())}");
            Debug.Log($"[UIManager] Type: {pc.GetType().Name} | Name: '{pc.pcName}'");

            if (!string.IsNullOrEmpty(pc.pcName))
            {
                Debug.Log($"Adding point cloud '{pc.pcName}' to dropdown.");
                names.Add(pc.pcName);
                nameToData[pc.pcName] = pc;
            }
            else
            {
                Debug.LogWarning("Point cloud with null or empty name ignored.");
            }
        }

        Debug.Log($"[UIManager] Found names: {string.Join(", ", names)}");

        pointCloudDropdown.ClearOptions();
        pointCloudDropdown.AddOptions(names);

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "1", "2", "3" });

        pointCloudDropdown.onValueChanged.AddListener(OnPointCloudChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        meshToggle.onValueChanged.AddListener(OnMeshToggle);
        playButton.onClick.AddListener(OnPlay);
        resetButton.onClick.AddListener(OnReset);

        if (names.Count > 0)
        {
            LoadPointCloud(names[0]);
        }
        else
        {
            Debug.LogWarning("[UIManager] No valid names found. Dropdown is empty.");
        }
    }

    void LoadPointCloud(string pcName)
    {
        if (!nameToData.TryGetValue(pcName, out PointCloudObject data))
        {
            Debug.LogError($"Cannot find PointCloudObject with name '{pcName}'.");
            return;
        }

        // IMPORTANT: Make sure the prefab path/name matches your new prefab!
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Preview_PointCloudPrefab_NET4U");
        if (prefab == null)
        {
            Debug.LogError("Preview_PointCloudPrefab_NET4U not found in Resources/Prefabs.");
            return;
        }

        GameObject go = Instantiate(prefab);
        var controller = go.GetComponent<PointCloudController>();
        controller.LoadPointCloud(data.pcName, 1);
        currentPC = controller;
    }

    void OnPointCloudChanged(int index)
    {
        string selected = pointCloudDropdown.options[index].text;
        LoadPointCloud(selected);
    }

    void OnQualityChanged(int index)
    {
        if (currentPC != null)
            currentPC.SetQuality(qualityDropdown.options[index].text);
    }

    void OnMeshToggle(bool isMesh)
    {
        if (currentPC != null)
            currentPC.SetIsMesh(isMesh);
    }

    public void OnPlay()
    {
        string fixedName = "BlueSpin_UVG_vox10_25_0_250"; // Name of the main folder
        string fixedQuality = "1"; // Fixed quality q1

        Debug.Log($"[UIManager] PLAY fixed | {fixedName} - q{fixedQuality}");

        currentPC = PointCloudsLoader.Instance.Spawn(fixedName, fixedQuality);

        if (currentPC != null)
        {
            currentPC.SetIsMesh(meshToggle.isOn); // You can disable this if you always want point cloud
            currentPC.SetAnimate(true);
        }
    }

    void OnReset()
    {
        if (currentPC != null)
        {
            currentPC.SetAnimate(false);
            currentPC.ResetView();
        }
    }
}
