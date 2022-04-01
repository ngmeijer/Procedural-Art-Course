using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Node_EditModes
{
    PlaceNode,
    RemoveNode,
    MoveNode,
    ConnectNode,
    DisconnectNode,
    NoneSelected,
}

public enum FSM_States
{
    GenerateNodes,
    GenerateRoads,
    GenerateCityBlocks
}


public class GeneratorFSM : MonoBehaviour
{
    public static UnityEvent<Node_EditModes> broadcastNodeEditModeChange = new UnityEvent<Node_EditModes>();
    public static UnityEvent<FSM_States> broadcastGenerationModeChange = new UnityEvent<FSM_States>();

    private NodeEditor nodeEditor;
    private RoadGenerator roadGenerator;
    private CityBlockGenerator cityBlockGenerator;
    private FSM_State currentGenerator;
    private FSM_States currentState;

    private void OnEnable()
    {
        nodeEditor = FindObjectOfType<NodeEditor>();
        roadGenerator = FindObjectOfType<RoadGenerator>();
        cityBlockGenerator = FindObjectOfType<CityBlockGenerator>();

        nodeEditor.onSelectNewMode.AddListener(listenToNewNodeEditMode);

        nodeEditor.onModeExit.AddListener(listenToNewGenerationMode);
        roadGenerator.onModeExit.AddListener(listenToNewGenerationMode);
        cityBlockGenerator.onModeExit.AddListener(listenToNewGenerationMode);
        currentGenerator = nodeEditor;

        currentGenerator.EnterState();

        currentState = FSM_States.GenerateNodes;
        broadcastGenerationModeChange.Invoke(currentState);
    }

    private void listenToNewNodeEditMode(Node_EditModes pNewMode)
    {
        broadcastNodeEditModeChange.Invoke(pNewMode);
    }

    private void listenToNewGenerationMode(FSM_States pOldState)
    {
        currentGenerator.ExitState();
        
        switch (pOldState)
        {
            case FSM_States.GenerateNodes:
                nodeEditor.transferNodes();
                currentGenerator = roadGenerator;
                currentState = FSM_States.GenerateRoads;
                break;
            case FSM_States.GenerateRoads:
                currentGenerator = cityBlockGenerator;
                currentState = FSM_States.GenerateCityBlocks;
                break;
            case FSM_States.GenerateCityBlocks:
                break;
        }

        currentGenerator.EnterState();
        
        broadcastGenerationModeChange.Invoke(currentState);
    }
}