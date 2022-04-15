using System;
using System.Collections;
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

    public static UnityEvent onClickRegenerateSpawnpoints = new UnityEvent();
    
    [SerializeField] private NodeEditor nodeEditor;
    [SerializeField] private RoadGenerator roadGenerator;
    [SerializeField] private CityBlockGenerator cityBlockGenerator;
    public FSM_State currentGenerator;
    public FSM_States currentState;
    public Node_EditModes currentEditMode = Node_EditModes.NoneSelected;
    private Node_EditModes oldEditMode;

    public Vector3 buildingSize;
    public Vector3 buildingOffset;
    public float stackHeight;
    public bool enableBillboards;
    public int selectedBuildingIndex;
    public int selectedCityBlockIndex;

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
        }
    }

    public void ProcessSpawnpointRegenerationRequest()
    {
        onClickRegenerateSpawnpoints.Invoke();
    }

    public void ProcessRoadGenerationRequest()
    {
        roadGenerator.InitializeRoads();
        ProcessNewGenerationModeRequest(FSM_States.GenerateRoads);
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
        if (currentGenerator != null) currentGenerator.ExitState();

        switch (pOldState)
        {
            case FSM_States.None:
                currentGenerator = nodeEditor;
                currentState = FSM_States.GenerateNodes;
                break;
            case FSM_States.GenerateNodes:
                nodeEditor.SaveToFile();
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

    public void UpdateVariables()
    {
        if (nodeEditor != null)
        {
            nodeEditor.buildingSize = buildingSize;
            nodeEditor.buildingOffset = buildingOffset;
        }

        if (cityBlockGenerator != null)
        {
            cityBlockGenerator.selectedCityBlockIndex = selectedCityBlockIndex;
            cityBlockGenerator.selectedBuildingIndex = selectedBuildingIndex;
        }

        if(stackHeight != 0) ProceduralBuilding.StackHeight = stackHeight;
        ProceduralBuilding.EnableBillboards = enableBillboards;
    }
}