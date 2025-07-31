using UnityEngine;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(PointCloudMeshGenerator))]
public class PlyLoader : MonoBehaviour
{
    public string fileName = "PointClouds/PointClouds/BlueSpin_UVG_vox10_25_0_250/q1/PointClouds/BlueSpin_UVG_vox10_25_0_250_0001";
    void Start()
    {
        TextAsset plyAsset = Resources.Load<TextAsset>(fileName);
        if (plyAsset == null)
        {
            Debug.LogError("Ply file not found in Resources: " + fileName);
            return;
        }

        using (BinaryReader reader = new BinaryReader(new MemoryStream(plyAsset.bytes)))
        {
            var points = new List<Vector3>();
            var colors = new List<Color>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();

                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte(); // anche se spesso è ignorato

                points.Add(new Vector3(x, y, z));
                colors.Add(new Color32(r, g, b, a));
            }

            var generator = GetComponent<PointCloudMeshGenerator>();
            generator.Points = points;
            generator.Colors = colors;

            // Chiama manualmente il metodo per generare la mesh
            generator.SendMessage("GenerateMesh");
        }
    }
}
