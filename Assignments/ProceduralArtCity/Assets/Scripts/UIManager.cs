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
    public string currentNodeEditMode = "none selected";
    private string nodeInstructions;

    public static UnityEvent onClickCreateSaveFile = new UnityEvent();
    public static UnityEvent onClickSaveToFile = new UnityEvent();
    public static Event_OnClickNewMode onClickNewMode = new Event_OnClickNewMode();
    public static UnityEvent onClickGenerateRoads = new UnityEvent();
    public static Event_OnCityBlockAction onCityBlockInitialize = new Event_OnCityBlockAction();
    public static Event_OnCityBlockAction onCityBlockFinish = new Event_OnCityBlockAction();
    private FSM_States currentState;
    private string currentGenerationMode;

    private void OnEnable()
    {
        labelStyle.fontSize = 30;
        
        GeneratorFSM.broadcastNodeEditModeChange.AddListener(listenToNewNodeMode);
        GeneratorFSM.broadcastGenerationModeChange.AddListener(listenToNewGenerationMode);
    }

    private void OnGUI()
    {
        PrepareGameGUI();
        PrepareCityBlockGUI();
    }

    private void listenToNewGenerationMode(FSM_States pNewState)
    {
        switch (pNewState)
        {
            case FSM_States.GenerateNodes:
                break;
            case FSM_States.GenerateRoads:
                break;
            case FSM_States.GenerateCityBlocks:
                break;
        }

        currentGenerationMode = pNewState.ToString();
    }

    private void listenToNewNodeMode(Node_EditModes pNewMode)
    {
        switch (pNewMode)
        {
            case Node_EditModes.PlaceNode:
                nodeInstructions = "Click somewhere to place a node.";
                break;
            case Node_EditModes.RemoveNode:
                nodeInstructions = "Click node to delete.";
                break;
            case Node_EditModes.MoveNode:
                nodeInstructions = "Click a node to select it. \nUse mouse to move node. \nClick again to release.";
                break;
            case Node_EditModes.ConnectNode:
                nodeInstructions = "Click 2 different nodes to connect them";
                break;
            case Node_EditModes.DisconnectNode:
                nodeInstructions = "Click one of the connected \nnodes to disconnect";
                break; 
        }

        currentNodeEditMode = pNewMode.ToString();
    }

    private void PrepareCityBlockGUI()
    {
        
    }

    private void PrepareGameGUI()
    {
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 30;
        GUI.Label(new Rect(10, 10, 100, 50), "Generation mode:", labelStyle);
        GUI.Label(new Rect(265, 10, 100, 50), currentGenerationMode, labelStyle);
        
        GUI.Label(new Rect(10, 70, 100, 50), "Node edit mode:", labelStyle);
        GUI.Label(new Rect(265, 70, 100, 50), currentNodeEditMode, labelStyle);

        labelStyle.normal.textColor = Color.black;
        labelStyle.fontSize = 25;
        GUI.Label(new Rect(10, 130, 100, 50), nodeInstructions, labelStyle);
        GUI.Label(new Rect(10, 180, 100, 50), "Press TAB to \nlock/unlock \ncamera movement", labelStyle);
    }
}