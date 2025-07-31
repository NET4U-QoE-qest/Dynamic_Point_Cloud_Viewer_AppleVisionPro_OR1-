using UnityEngine;

public class AutoPointCloudPlayer : MonoBehaviour
{
    public string pointCloudName = "BlueSpin_UVG_vox10_25_0_250";
    public int quality = 3;

    void Start()
    {
        Debug.Log($"▶ AutoPlay: {pointCloudName} - q{quality}");

        GameObject prefab = Resources.Load<GameObject>("Prefabs/Preview_PointCloudPrefab_NET4U");
        if (prefab == null)
        {
            Debug.LogError(" Prefab 'Preview_PointCloudPrefab_NET4U' not found!");
            return;
        }

        // Instantiate the prefab in the scene
        GameObject go = Instantiate(prefab);

        go.transform.localPosition = new Vector3(0, 1f, 2f);
        go.transform.localRotation = Quaternion.Euler(0, 180, 0);
        go.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f); // or 0.005f for smaller scale

        // Load point cloud data
        var controller = go.GetComponent<PointCloudController>();
        controller.LoadPointCloud(pointCloudName, quality);

        // Start animation
        controller.SetAnimate(true);
    }
}
