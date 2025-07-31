using UnityEngine;
[System.Flags]
public enum PCObjectType
{
[InspectorName("Deselect All")] DeselectAll = 0,
[InspectorName("Select All")] SelectAll = ~0,
[InspectorName("Unknown")] Unknown = -1,
    PointClouds = 1 <<0,
};
