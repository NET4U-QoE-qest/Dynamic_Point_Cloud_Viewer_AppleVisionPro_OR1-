using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandMenuActivator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject leftHandMenu;   // Assign in Inspector
    public GameObject rightHandMenu;  // Assign in Inspector
    public float holdDuration = 1.5f; // Seconds to hold pinch

    private float leftPinchTime = 0f;
    private float rightPinchTime = 0f;
    private bool leftMenuVisible = false;
    private bool rightMenuVisible = false;

    void Update()
    {
        // LEFT HAND
        if (IsPinching(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left))
        {
            leftPinchTime += Time.deltaTime;
            if (!leftMenuVisible && leftPinchTime > holdDuration)
            {
                ToggleMenu(leftHandMenu, true);
                leftMenuVisible = true;
            }
        }
        else
        {
            leftPinchTime = 0f;
            leftMenuVisible = false;
        }

        // RIGHT HAND
        if (IsPinching(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right))
        {
            rightPinchTime += Time.deltaTime;
            if (!rightMenuVisible && rightPinchTime > holdDuration)
            {
                ToggleMenu(rightHandMenu, true);
                rightMenuVisible = true;
            }
        }
        else
        {
            rightPinchTime = 0f;
            rightMenuVisible = false;
        }
    }

    bool IsPinching(InputDeviceCharacteristics handedness)
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(handedness, devices);

        foreach (var device in devices)
        {
            if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal))
            {
                if (triggerVal > 0.8f) // Considered a pinch
                    return true;
            }
        }
        return false;
    }

    void ToggleMenu(GameObject menu, bool state)
    {
        if (menu != null)
            menu.SetActive(state);
    }
}
