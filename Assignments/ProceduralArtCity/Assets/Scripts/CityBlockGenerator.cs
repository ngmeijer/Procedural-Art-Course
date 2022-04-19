using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[Serializable]
public class Event_OnBuildingSettingsChanged : UnityEvent<Vector3, Vector3>
{
}

public struct UtilitySettings
{
    public float HouseDistanceFactor;
    public float SkyScraperDistanceFactor;

    public float HouseWeightFactor;
    public float SkyscraperWeightFactor;

    public float MinRandomValue;
    public float MaxRandomValue;
}

public class CityBlockGenerator : FSM_State
{
    public static Event_OnBuildingSettingsChanged onBuildingSettingsChanged = new Event_OnBuildingSettingsChanged();
    [SerializeField] private List<CityBlock> cityBlocksData = new List<CityBlock>();
    public int CityBlockCount;
    public int BuildingCount;
    public int StackCount;
    public int StackPrefabCount;
    private int blockInEditMode = -1;
    private Vector3 currentCentroidPoint;

    private Dictionary<int, Vector3> planeClosestVertices = new Dictionary<int, Vector3>();
    private bool currentlyEditingBlock;
    private Transform cityBlockParent;
    private float currentHouseValue;
    private float currentSkyscraperValue;
    private float maxDistanceToCenter;
    private CityBlock editorSelectedCityBlock;
    private ProceduralBuilding editorSelectedBuilding;
    private GameObject editorSelectedStack;

    [HideInInspector] public int selectedBuildingIndex;
    [HideInInspector] public int selectedCityBlockIndex;
    public int selectedNewStackPrefabIndex;
    public int selectedStackIndex;

    [SerializeField] private List<GameObject> availableSkyscrapers;
    [SerializeField] private List<GameObject> availableHouses;
    private BuildingType currentBuildingType;
    public UtilitySettings utilitySettings;

    private void Awake()
    {
        PointSelector.onNodeSelect.AddListener(addNodeToCityBlockCorners);
        PointSelector.onSpawnpointSelect.AddListener(determineSpawnpointAction);
    }

    public override void EnterState()
    {
        isActive = true;
    }

    public override void ExitState()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive) return;

        if (cityBlocksData.Count > 0 && selectedCityBlockIndex >= 0)
            editorSelectedCityBlock = cityBlocksData[selectedCityBlockIndex];

        if (editorSelectedCityBlock != null)
        {
            if (selectedBuildingIndex >= 0 && selectedBuildingIndex < editorSelectedCityBlock.spawnedBuildings.Count)
                editorSelectedBuilding = editorSelectedCityBlock.spawnedBuildings[selectedBuildingIndex];

            BuildingCount = editorSelectedCityBlock.spawnedBuildings.Count;
        }

        if (editorSelectedBuilding != null)
        {
            StackCount = editorSelectedBuilding.spawnedStacks.Count;
            if (selectedStackIndex >= 0 && selectedStackIndex < editorSelectedBuilding.spawnedStacks.Count)
            {
                if (selectedStackIndex == 0)
                {
                    StackPrefabCount = editorSelectedBuilding.floorStacksCount;
                    if (selectedNewStackPrefabIndex > editorSelectedBuilding.floorStacksCount) return;
                }
                else if (selectedStackIndex == editorSelectedBuilding.spawnedStacks.Count - 1)
                {
                    StackPrefabCount = editorSelectedBuilding.roofStacksCount;
                    if (selectedNewStackPrefabIndex > editorSelectedBuilding.roofStacksCount) return;
                }
                else
                {
                    StackPrefabCount = editorSelectedBuilding.middleStacksCount;
                    if (selectedNewStackPrefabIndex > editorSelectedBuilding.middleStacksCount) return;
                }

                editorSelectedStack = editorSelectedBuilding.spawnedStacks[selectedStackIndex];

                editorSelectedBuilding.ShowPotentialStack(selectedStackIndex, selectedNewStackPrefabIndex);
            }
        }
    }

    public void FillCityBlock()
    {
        if (!currentlyEditingBlock) return;

        CityBlock currentBlock = cityBlocksData[blockInEditMode];

        maxDistanceToCenter = calculateMaxDistance();

        for (int i = 0; i < currentBlock.spawnPoints.Count; i++)
        {
            GameObject selectedBuilding = chooseRandomBuilding();

            //Let ProceduralBuilding.cs do the rest of the work
            if (selectedBuilding == null)
            {
                Debug.LogError("Couldn't find any building prefabs!");
                return;
            }

            GameObject building = Instantiate(selectedBuilding, currentBlock.spawnPoints[i].position,
                Quaternion.identity, currentBlock.parent);

            ProceduralBuilding buildingScript = building.GetComponent<ProceduralBuilding>();
            buildingScript.buildingType = currentBuildingType;
            buildingScript.utilityValueHouse = currentHouseValue;
            buildingScript.utilityValueSkyscraper = currentSkyscraperValue;
            currentBlock.spawnedBuildings.Add(buildingScript);
        }

        currentlyEditingBlock = false;
    }

    private GameObject chooseRandomBuilding()
    {
        //Determine building type

        currentBuildingType = determineBuildingType();

        int randomIndex = 0;
        GameObject selectedBuilding = null;

        //If skyscraper, choose random from skyscraper list
        switch (currentBuildingType)
        {
            case BuildingType.House:
                randomIndex = Random.Range(0, availableHouses.Count - 1);
                selectedBuilding = availableHouses[randomIndex];
                break;
            case BuildingType.Skyscraper:
                randomIndex = Random.Range(0, availableSkyscrapers.Count - 1);
                selectedBuilding = availableSkyscrapers[randomIndex];
                break;
        }

        return selectedBuilding;
    }

    public void CreateEmptyCityBlock()
    {
        if (currentlyEditingBlock) return;

        CityBlock cityBlock = new CityBlock();
        cityBlocksData.Add(cityBlock);
        blockInEditMode++;
        currentlyEditingBlock = true;
        Transform cityBlockBuildingParent = new GameObject().transform;
        cityBlockBuildingParent.parent = transform;
        cityBlockBuildingParent.name = $"[BLOCK {blockInEditMode}] building parent";
        cityBlock.parent = cityBlockBuildingParent;
        cityBlockParent = cityBlockBuildingParent;
        CityBlockCount++;
    }

    public void DiscardCurrentCityBlock()
    {
        cityBlocksData[blockInEditMode] = null;
        cityBlocksData.RemoveAt(blockInEditMode);
        blockInEditMode--;
        CityBlockCount--;
    }

    private void destroyIndexSelectedCityBlockBuildings()
    {
        foreach (var buildingData in editorSelectedCityBlock.spawnedBuildings)
        {
            Destroy(buildingData.gameObject);
        }

        cityBlocksData.Remove(editorSelectedCityBlock);
        if (selectedCityBlockIndex > cityBlocksData.Count - 1) selectedCityBlockIndex--;
    }

    private void deselectIndexSelectedCityBlockSpawnpoints()
    {
        foreach (var spawnpoint in editorSelectedCityBlock.spawnPoints)
        {
            spawnpoint.ResetToDefaultColour();
        }

        editorSelectedCityBlock.spawnPoints.Clear();
    }

    public void DestroyIndexSelectedCityBlock()
    {
        destroyIndexSelectedCityBlockBuildings();
        deselectIndexSelectedCityBlockSpawnpoints();
        CityBlockCount--;
    }

    public void ReplaceBuildingStack()
    {
        if(editorSelectedBuilding != null) editorSelectedBuilding.ReplaceStack(selectedStackIndex, selectedNewStackPrefabIndex);
    }

    public void RegenerateBuilding()
    {
        if(editorSelectedBuilding != null) editorSelectedBuilding.RegenerateBuilding();
    }

    private BuildingType determineBuildingType()
    {
        currentHouseValue = calculateUtilityValue(utilitySettings.HouseDistanceFactor,
            utilitySettings.HouseWeightFactor, utilitySettings.MinRandomValue,
            utilitySettings.MaxRandomValue);
        currentSkyscraperValue = calculateUtilityValue(utilitySettings.SkyScraperDistanceFactor,
            utilitySettings.SkyscraperWeightFactor,
            utilitySettings.MinRandomValue, utilitySettings.MaxRandomValue);

        return currentHouseValue > currentSkyscraperValue ? BuildingType.House : BuildingType.Skyscraper;
    }

    private float calculateDistancerToCenter() => Vector3.Distance(transform.position, NodeEditor.Centroid);

    private float calculateMaxDistance() => Vector3.Distance(NodeEditor.Centroid, NodeEditor.TopLeftCorner);

    private float calculateUtilityValue(float pDistanceFactor, float pWeightFactor, float pMinRandomValue = 0,
        float pMaxRandomValue = 0)
    {
        float distanceToCentroid = calculateDistancerToCenter();
        float randomValue = Random.Range(pMinRandomValue, pMaxRandomValue);
        float utilityValue = (1 - (distanceToCentroid - maxDistanceToCenter)) * pDistanceFactor + pWeightFactor +
                             randomValue;

        return utilityValue;
    }

    private void addNodeToCityBlockCorners(Node pNode, Vector3 pMousePosition)
    {
        if (!isActive) return;
        if (pNode == null) return;

        if (!cityBlocksData[blockInEditMode].outerCorners.Contains(pNode.position))
            cityBlocksData[blockInEditMode].outerCorners.Add(pNode.position);
    }

    private void determineSpawnpointAction(Spawnpoint pSpawnpoint, Vector3 pMousePos,
        SpawnpointSelection pSelectionMode)
    {
        if (!isActive) return;
        if (pSpawnpoint == null) return;
        if (cityBlocksData.Count == 0) return;

        switch (pSelectionMode)
        {
            case SpawnpointSelection.Select:
                if (!cityBlocksData[blockInEditMode].spawnPoints.Contains(pSpawnpoint))
                    cityBlocksData[blockInEditMode].spawnPoints.Add(pSpawnpoint);
                break;
            case SpawnpointSelection.Deselect:
                if (cityBlocksData[blockInEditMode].spawnPoints.Contains(pSpawnpoint))
                    cityBlocksData[blockInEditMode].spawnPoints.Remove(pSpawnpoint);
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        if (planeClosestVertices.Count != 0)
        {
            foreach (KeyValuePair<int, Vector3> vertex in planeClosestVertices)
            {
                Gizmos.DrawSphere(vertex.Value, 0.5F);
                Handles.Label(vertex.Value, $"index: {vertex.Key}");
            }
        }

        for (int i = 0; i < cityBlocksData.Count; i++)
        {
            Gizmos.color = Color.yellow;
            Vector3 centroid = cityBlocksData[i].centroid;

            if (centroid != Vector3.zero) Gizmos.DrawWireSphere(centroid, 2f);
        }

        if (editorSelectedCityBlock != null)
        {
            if (editorSelectedBuilding != null)
            {
                Gizmos.color = Color.magenta;

                Vector3 buildingPosition = editorSelectedBuilding.gameObject.transform.position;

                Vector3 drawPosition = new Vector3(buildingPosition.x, editorSelectedBuilding.size.y / 2,
                    buildingPosition.z);
                Gizmos.DrawWireCube(drawPosition, editorSelectedBuilding.size);

                if (editorSelectedStack != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(editorSelectedStack.transform.position,
                        new Vector3(editorSelectedBuilding.size.x + 1, ProceduralBuilding.StackHeight,
                            editorSelectedBuilding.size.z + 1));
                }
            }
        }
    }
}