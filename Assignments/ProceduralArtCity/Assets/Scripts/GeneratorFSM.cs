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

public enum CityBlockActions
{
    Create,
    Finish,
    Discard
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

        
        currentGenerator = nodeEditor;

        currentGenerator.EnterState();

        currentState = FSM_States.GenerateNodes;
        broadcastGenerationModeChange.Invoke(currentState);
    }

    public void ProcessNewNodeEditModeRequest(Node_EditModes pNewMode)
    {
        broadcastNodeEditModeChange.Invoke(pNewMode);
    }

    public void ProcessSpawnpointRegenerationRequest()
    {
        nodeEditor.RecalculateSpawnpoints();
    }

    public void ProcessRoadGenerationRequest()
    {
        roadGenerator.InitializeRoads();
    }

    public void ProcessCityBlockActionRequest(CityBlockActions pAction)
    {
        switch (pAction)
        {
            case CityBlockActions.Create:
                cityBlockGenerator.CreateEmptyCityBlock();
                break;
            case CityBlockActions.Finish:
                cityBlockGenerator.FinishCityBlock();
                break;
            case CityBlockActions.Discard:
                cityBlockGenerator.DiscardCurrentCityBlock();
                break;
        }
    }

    public void ProcessNewGenerationModeRequest(FSM_States pOldState)
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