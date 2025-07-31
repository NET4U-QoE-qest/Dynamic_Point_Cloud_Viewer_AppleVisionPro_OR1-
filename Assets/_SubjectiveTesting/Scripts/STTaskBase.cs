using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class STTaskBase
{
    public List<STSequence> sequences = new List<STSequence>();
    public int currentSequenceIndex = -1;

    protected string qualityPrefix = "r";
    protected string distancePrefix = "d";

    public string currentSequenceString = "";

    private AnimatePointCloudBase animateComp;

    public STTaskBase(List<STSequence> sequences)
    {
        this.sequences = sequences;
    }

    protected void ActivateCurrentPointCloud()
    {
        animateComp.StartAnimation();
        Debug.Log("Activating point cloud object.");
    }

    protected string MakeSequenceString(string currentObjectString, string currentQualityString, string currentDistanceString)
    {
        return currentObjectString + "_" + currentQualityString + "_" + currentDistanceString;
    }

    public void SetupNextSequence()
    {
        currentSequenceIndex += 1;

        if (currentSequenceIndex == sequences.Count)
        {
            OnTaskEnded();
            return;
        }

        STSequence currSequence = sequences[currentSequenceIndex];
        string pcName = currSequence.ObjectType.ToString();
        PointCloudObject currPcObject = PointCloudsLoader.Instance.pcObjects.FirstOrDefault(p => p.pcName == pcName);

        if (currPcObject == null)
        {
            Debug.LogError($"PointCloudObject not found: {pcName}");
            return;
        }

        List<string> framePaths = currPcObject.framePaths;

        if (framePaths == null || framePaths.Count == 0)
        {
            Debug.LogWarning("[STTaskBase] No frame paths found for current PointCloudObject.");
            return;
        }

        animateComp.LoadQuality(framePaths,"");

        // Imposta il materiale corretto in base al tipo di rappresentazione
        var renderer = animateComp.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = STManager.Instance.GetMaterialFromRepresentation(currSequence.RepresentationType);
        }

        STManager.Instance.SetCurrentPCDistance(currSequence.Distance);

        if (currentSequenceIndex < sequences.Count)
        {
            string qualityString = $"{currSequence.FirstQuality}_{qualityPrefix}{currSequence.SecondQuality}";
            string distanceString = ((int)(currSequence.Distance * 100f)).ToString();

            string outTextFormatted = MakeSequenceString(
                pcName,
                qualityString,
                distancePrefix + distanceString);

            currentSequenceString = outTextFormatted;
            STManager.Instance.SetDisplayString(outTextFormatted);
            Debug.Log(outTextFormatted);
        }

        ActivateCurrentPointCloud();
    }

    public void SetupTask()
    {
        animateComp = STSecondaryManager.Instance.currentGameObject.GetComponent<AnimatePointCloudBase>();
        currentSequenceIndex = -1;
        SetupNextSequence();
    }

    private void OnTaskEnded()
    {
        STManager.Instance.OnFullTaskEnded();
        Debug.Log("Task ended.");
    }

    void SetSequences(List<STSequence> sequences)
    {
        this.sequences = sequences;
    }
}
