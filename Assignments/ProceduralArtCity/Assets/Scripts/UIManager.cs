using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Event_OnClickNewMode : UnityEvent<string>
{
}

[Serializable] public class Event_OnCityBlockAction : UnityEvent{}

public class UIManager : MonoBehaviour
{
    private GUIStyle labelStyle = new GUIStyle();
    public string currentMode = "none selected";
    private string modeInstructions;

    public static UnityEvent onClickCreateSaveFile = new UnityEvent();
    public static UnityEvent onClickSaveToFile = new UnityEvent();
    public static Event_OnClickNewMode onClickNewMode = new Event_OnClickNewMode();
    public static UnityEvent onClickGenerateRoads = new UnityEvent();
    public static Event_OnCityBlockAction onCityBlockInitialize = new Event_OnCityBlockAction();
    public static Event_OnCityBlockAction onCityBlockFinish = new Event_OnCityBlockAction();

    public bool nodeEditMode;
    public bool streetGenerationMode;
    public bool cityBlockEditMode;

    private void Start()
    {
        nodeEditMode = true;
        labelStyle.fontSize = 30;
    }

    private void OnGUI()
    {
        if (nodeEditMode) PrepareNodeGUI();
        if (streetGenerationMode) PrepareStreetGenerationGUI();
        if (cityBlockEditMode) PrepareCityBlockGUI();
    }


    private void PrepareStreetGenerationGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Return to node \nedit mode"))
        {
            nodeEditMode = true;
            streetGenerationMode = false;
        }

        if (GUI.Button(new Rect(10, 70, 150, 50), "Generate roads"))
        {
            onClickGenerateRoads.Invoke();
            streetGenerationMode = false;
            cityBlockEditMode = true;
            currentMode = "CityBlockGeneration";
            onClickNewMode.Invoke(currentMode);
        }
    }

    private void PrepareCityBlockGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Confirm city block"))
        {
            onCityBlockFinish.Invoke();
        }
    }

    private void PrepareNodeGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 50), "Mode:", labelStyle);
        GUI.Label(new Rect(100, 10, 100, 50), currentMode, labelStyle);
        GUI.Label(new Rect(10, 360, 100, 50), modeInstructions, labelStyle);
        GUI.Label(new Rect(2000, 150, 100, 50), "Press TAB to \nlock/unlock \ncamera movement", labelStyle);

        if (GUI.Button(new Rect(10, 70, 150, 50), "Place node"))
        {
            currentMode = "Place";
            onClickNewMode.Invoke(currentMode);
            modeInstructions = "Click somewhere to place a node.";
        }

        if (GUI.Button(new Rect(10, 130, 150, 50), "Remove node"))
        {
            currentMode = "Remove";
            onClickNewMode.Invoke(currentMode);
            modeInstructions = "Click node to delete.";
        }

        if (GUI.Button(new Rect(10, 190, 150, 50), "Move node"))
        {
            currentMode = "Move";
            onClickNewMode.Invoke(currentMode);
            modeInstructions = "Click a node to select it. \nUse mouse to move node. \nClick again to release.";
        }

        if (GUI.Button(new Rect(10, 250, 150, 50), "Connect node"))
        {
            currentMode = "Connect";
            onClickNewMode.Invoke(currentMode);
            modeInstructions = "Click another node to connect";
        }

        if (GUI.Button(new Rect(10, 310, 150, 50), "Disconnect node"))
        {
            currentMode = "Disconnect";
            onClickNewMode.Invoke(currentMode);
            modeInstructions = "Click one of the connected \nnodes to disconnect";
        }

        if (GUI.Button(new Rect(10, 500, 150, 50), "Confirm map"))
        {
            nodeEditMode = false;
            streetGenerationMode = true;
            currentMode = "StreetGeneration";
            onClickNewMode.Invoke(currentMode);
        }

        if (GUI.Button(new Rect(2100, 10, 150, 50), "Create new save file"))
        {
            onClickCreateSaveFile.Invoke();
        }

        if (GUI.Button(new Rect(2100, 70, 150, 80), "Save to currently \nselected file"))
        {
            onClickSaveToFile.Invoke();
        }
    }
}