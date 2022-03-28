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

    private void OnEnable()
    {
        nodeEditMode = true;
        labelStyle.fontSize = 30;
        
        GeneratorFSM.broadcastNodeEditModeChange.AddListener(listenToNewMode);
    }

    private void OnGUI()
    {
        PrepareGameGUI();
        if (cityBlockEditMode) PrepareCityBlockGUI();
    }

    private void listenToNewMode(NodeEditModes pNewMode)
    {
        switch (pNewMode)
        {
            case NodeEditModes.PlaceNode:
                currentMode = "Place";
                modeInstructions = "Click somewhere to place a node.";
                break;
            case NodeEditModes.RemoveNode:
                currentMode = "Remove";
                modeInstructions = "Click node to delete.";
                break;
            case NodeEditModes.MoveNode:
                currentMode = "Move";
                modeInstructions = "Click a node to select it. \nUse mouse to move node. \nClick again to release.";
                break;
            case NodeEditModes.ConnectNode:
                currentMode = "Connect";
                modeInstructions = "Click 2 different nodes to connect them";
                break;
            case NodeEditModes.DisconnectNode:
                currentMode = "Disconnect";
                modeInstructions = "Click one of the connected \nnodes to disconnect";
                break; 
        }
    }

    private void PrepareCityBlockGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Confirm city block"))
        {
            onCityBlockFinish.Invoke();
        }
    }

    private void PrepareGameGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 50), "Mode:", labelStyle);
        GUI.Label(new Rect(100, 10, 100, 50), currentMode, labelStyle);
        GUI.Label(new Rect(10, 360, 100, 50), modeInstructions, labelStyle);
        GUI.Label(new Rect(2000, 150, 100, 50), "Press TAB to \nlock/unlock \ncamera movement", labelStyle);
    }
}