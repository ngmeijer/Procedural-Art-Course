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
    None,
    GenerateNodes,
    GenerateRoads,
    GenerateCityBlocks
}


[ExecuteAlways]
public class GeneratorFSM : MonoBehaviour
{
    public static UnityEvent<Node_EditModes> broadcastNodeEditModeChange = new UnityEvent<Node_EditModes>();
    public static UnityEvent<FSM_States> broadcastGenerationModeChange = new UnityEvent<FSM_States>();

    private NodeEditor nodeEditor;
    private RoadGenerator roadGenerator;
    private CityBlockGenerator cityBlockGenerator;
    private FSM_State currentGenerator;
    private FSM_States currentState;
    public Node_EditModes currentEditMode = Node_EditModes.NoneSelected;
    private Node_EditModes oldEditMode;
    
    private void Awake()
    {
        nodeEditor = FindObjectOfType<NodeEditor>();
        roadGenerator = FindObjectOfType<RoadGenerator>();
        cityBlockGenerator = FindObjectOfType<CityBlockGenerator>();
        
        nodeEditor.onModeExit.AddListener(ProcessNewGenerationModeRequest);
        roadGenerator.onModeExit.AddListener(ProcessNewGenerationModeRequest);
        cityBlockGenerator.onModeExit.AddListener(ProcessNewGenerationModeRequest);
    }

    private void Start()
    {
        ProcessNewNodeEditModeRequest(Node_EditModes.PlaceNode);
        ProcessNewGenerationModeRequest(FSM_States.None);
    }

    public void ProcessNewNodeEditModeRequest(Node_EditModes pNewMode)
    {
        oldEditMode = currentEditMode;

        if (pNewMode != oldEditMode || oldEditMode != Node_EditModes.NoneSelected)
        {
            currentEditMode = pNewMode;
            broadcastNodeEditModeChange.Invoke(pNewMode);
            Debug.Log("Invoking node mode change");
        }
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

    public void ProcessCityBlockPreferredBuildingType(BuildingType pType)
    {
        cityBlockGenerator.currentPreferredBuildingType = pType;
    }

    public void ProcessNewGenerationModeRequest(FSM_States pOldState)
    {
        if (currentGenerator != null)
        {
            Debug.Log($"Exiting {pOldState}. Current generator: {currentGenerator.name}");

            currentGenerator.ExitState();
        }
        
        switch (pOldState)
        {
            case FSM_States.None:
                currentGenerator = nodeEditor;
                currentState = FSM_States.GenerateNodes;
                break;
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