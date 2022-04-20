using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

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
    Discard,
    RegenerateBuilding,
    Destroy,
    ReplaceStack
}

public enum FSM_States
{
    None,
    GenerateNodes,
    GenerateRoads,
    GenerateCityBlocks
}

public enum ScreenshotMode
{
    Enable,
    Disable,
}

[ExecuteAlways]
public class GeneratorFSM : MonoBehaviour
{
    public static UnityEvent<Node_EditModes> broadcastNodeEditModeChange = new UnityEvent<Node_EditModes>();
    public static UnityEvent<FSM_States> broadcastGenerationModeChange = new UnityEvent<FSM_States>();
    public static Event_HandleScreenshotMode onClickScreenshotMode = new Event_HandleScreenshotMode();
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
    public float stackBuildDelay;
    public int selectedBuildingIndex;
    public int selectedCityBlockIndex;
    public UtilitySettings utilitySettings = new();
    public int CityBlockCount;
    public int BuildingCount;
    public int StackCount;
    public int StackPrefabCount;
    public int selectedNewStackPrefabIndex;
    public int selectedStackIndex;

    private void Start()
    {
        ProcessNewNodeEditModeRequest(Node_EditModes.PlaceNode);
        ProcessNewGenerationModeRequest(FSM_States.None);
    }

    private void Update()
    {
        CityBlockCount = cityBlockGenerator.CityBlockCount;
        BuildingCount = cityBlockGenerator.BuildingCount;
        StackCount = cityBlockGenerator.StackCount;
        StackPrefabCount = cityBlockGenerator.StackPrefabCount;
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

    public void ProcessScreenshotModeRequest(bool pState)
    {
        onClickScreenshotMode.Invoke(pState);
    }

    public void ProcessCityBlockActionRequest(CityBlockActions pAction)
    {
        switch (pAction)
        {
            case CityBlockActions.Create:
                cityBlockGenerator.CreateEmptyCityBlock();
                break;
            case CityBlockActions.Finish:
                cityBlockGenerator.FillCityBlock();
                break;
            case CityBlockActions.Discard:
                cityBlockGenerator.DiscardCurrentCityBlock();
                break;
            case CityBlockActions.Destroy:
                cityBlockGenerator.DestroyIndexSelectedCityBlock();
                break;
            case CityBlockActions.RegenerateBuilding:
                cityBlockGenerator.RegenerateBuilding();
                break;
            case CityBlockActions.ReplaceStack:
                cityBlockGenerator.ReplaceBuildingStack();
                break;
        }
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

    public void UpdateDependencies()
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
            cityBlockGenerator.selectedStackIndex = selectedStackIndex;
            cityBlockGenerator.selectedNewStackPrefabIndex = selectedNewStackPrefabIndex;
            cityBlockGenerator.utilitySettings = utilitySettings;
        }

        if(stackHeight != 0) ProceduralBuilding.StackHeight = stackHeight;
        ProceduralBuilding.BuildDelay = stackBuildDelay;
    }
}