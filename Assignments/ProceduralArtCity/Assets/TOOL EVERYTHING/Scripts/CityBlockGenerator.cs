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
    [SerializeField] private List<GameObject> availableSkyscrapers;
    [SerializeField] private List<GameObject> availableHouses;
    private CityBlock newCityBlock;
    private Vector3 currentCentroidPoint;

    private bool currentlyEditingBlock;
    private float currentHouseValue;
    private float currentSkyscraperValue;
    private float maxDistanceToCenter;
    private CityBlock editorSelectedCityBlock;
    private ProceduralBuilding editorSelectedBuilding;
    private GameObject editorSelectedStack;
    
    [HideInInspector] public int CityBlockCount;
    [HideInInspector] public int BuildingCount;
    [HideInInspector] public int StackCount;
    [HideInInspector] public int StackPrefabCount;
    [HideInInspector] public int selectedBuildingIndex;
    [HideInInspector] public int selectedCityBlockIndex;
    [HideInInspector] public int selectedNewStackPrefabIndex;
    [HideInInspector] public int selectedStackIndex;

    private BuildingType currentBuildingType;
    private Spawnpoint currentSpawnpoint;
    public UtilitySettings utilitySettings;

    private void Awake()
    {
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

        CityBlockCount = cityBlocksData.Count;

        if (cityBlocksData.Count > 0 && selectedCityBlockIndex >= 0 && selectedCityBlockIndex < cityBlocksData.Count)
            editorSelectedCityBlock = cityBlocksData[selectedCityBlockIndex];

        selectTemporaryBuilding();
        selectTemporaryStack();
    }

    private void selectTemporaryBuilding()
    {
        if (editorSelectedCityBlock != null)
        {
            if (selectedBuildingIndex >= 0 && selectedBuildingIndex < editorSelectedCityBlock.spawnedBuildings.Count)
                editorSelectedBuilding = editorSelectedCityBlock.spawnedBuildings[selectedBuildingIndex];

            BuildingCount = editorSelectedCityBlock.spawnedBuildings.Count;
        }
    }

    private void selectTemporaryStack()
    {
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
        
        maxDistanceToCenter = calculateMaxDistance();

        for (int i = 0; i < newCityBlock.spawnPoints.Count; i++)
        {
            currentSpawnpoint = newCityBlock.spawnPoints[i];
            GameObject selectedBuilding = chooseRandomBuilding();

            //Let ProceduralBuilding.cs do the rest of the work
            if (selectedBuilding == null)
            {
                Debug.LogError("Couldn't find any building prefabs!");
                return;
            }

            GameObject building = Instantiate(selectedBuilding, newCityBlock.spawnPoints[i].position,
                Quaternion.identity, newCityBlock.parent);

            ProceduralBuilding buildingScript = building.GetComponent<ProceduralBuilding>();
            buildingScript.buildingType = currentBuildingType;
            buildingScript.utilityValueHouse = currentHouseValue;
            buildingScript.utilityValueSkyscraper = currentSkyscraperValue;
            newCityBlock.spawnedBuildings.Add(buildingScript);
        }

        currentlyEditingBlock = false;
    }

    private GameObject chooseRandomBuilding()
    {
        currentBuildingType = determineBuildingType();

        int randomIndex = 0;
        GameObject selectedBuilding = null;

        switch (currentBuildingType)
        {
            case BuildingType.House:
                randomIndex = Random.Range(0, availableHouses.Count);
                selectedBuilding = availableHouses[randomIndex];
                break;
            case BuildingType.Skyscraper:
                randomIndex = Random.Range(0, availableSkyscrapers.Count);
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
        currentlyEditingBlock = true;
        Transform cityBlockBuildingParent = new GameObject().transform;
        cityBlockBuildingParent.parent = transform;
        cityBlockBuildingParent.name = $"[BLOCK {CityBlockCount}] building parent";
        cityBlock.parent = cityBlockBuildingParent;
        newCityBlock = cityBlock;
    }

    public void DiscardCurrentCityBlock()
    {
        newCityBlock = null;
        cityBlocksData.Remove(newCityBlock);
    }

    private void destroyIndexSelectedCityBlockBuildings()
    {
        foreach (var buildingData in editorSelectedCityBlock.spawnedBuildings)
        {
            if(buildingData != null) Destroy(buildingData.gameObject);
        }
        
        Destroy(editorSelectedCityBlock.parent.gameObject);

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
        selectedCityBlockIndex = 0;
    }

    public void ReplaceBuildingStack()
    {
        if (editorSelectedBuilding != null)
            editorSelectedBuilding.ReplaceStack(selectedStackIndex, selectedNewStackPrefabIndex);
    }

    public void RegenerateBuilding()
    {
        if (editorSelectedBuilding != null) editorSelectedBuilding.RegenerateBuilding();
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

    private float calculateDistancerToCenter() => Vector3.Distance(currentSpawnpoint.position, NodeEditor.Centroid);

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

    private void determineSpawnpointAction(Spawnpoint pSpawnpoint, Vector3 pMousePos,
        SpawnpointSelection pSelectionMode)
    {
        if (!isActive) return;
        if (pSpawnpoint == null) return;
        if (cityBlocksData.Count == 0) return;

        switch (pSelectionMode)
        {
            case SpawnpointSelection.Select:
                if (!newCityBlock.spawnPoints.Contains(pSpawnpoint))
                    newCityBlock.spawnPoints.Add(pSpawnpoint);
                break;
            case SpawnpointSelection.Deselect:
                if (newCityBlock.spawnPoints.Contains(pSpawnpoint))
                    newCityBlock.spawnPoints.Remove(pSpawnpoint);
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (DisableGizmos) return;

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