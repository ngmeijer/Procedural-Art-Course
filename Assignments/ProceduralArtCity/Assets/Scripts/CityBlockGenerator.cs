using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Event_OnBuildingSettingsChanged : UnityEvent<Vector3, Vector3>
{
}

public class CityBlockGenerator : FSM_State
{
    public static Event_OnBuildingSettingsChanged onBuildingSettingsChanged = new Event_OnBuildingSettingsChanged();
    [SerializeField] private List<CityBlock> cityBlocksData = new List<CityBlock>();
    private int currentSelectedCityBlock = -1;
    private Vector3 currentCentroidPoint;
    
    private Dictionary<int, Vector3> planeClosestVertices = new Dictionary<int, Vector3>();
    private GameObject buildingContainer;
    private bool currentlyEditingBlock;
    private Transform cityBlockParent;
    public BuildingType currentPreferredBuildingType;
    public int selectedBuildingIndex = -1;
    public int selectedCityBlockIndex = -1;

    private void Awake()
    {
        PointSelector.onNodeSelect.AddListener(addNodeToCityBlockCorners);
        PointSelector.onSpawnpointSelect.AddListener(determineSpawnpointAction);

        buildingContainer = Resources.Load<GameObject>("Prefabs/Building");
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
    }

    public void FinishCityBlock()
    {
        if (!currentlyEditingBlock) return;
        findCentroidOfBlock();
        
        CityBlock currentBlock = cityBlocksData[currentSelectedCityBlock];

        for (int i = 0; i < currentBlock.spawnPoints.Count; i++)
        {
            GameObject building = Instantiate(buildingContainer, currentBlock.spawnPoints[i].position, Quaternion.identity, currentBlock.parent);
            currentBlock.spawnedBuildings.Add(building.GetComponent<ProceduralBuilding>());
        }

        currentPreferredBuildingType = BuildingType.NoPreference;
        currentlyEditingBlock = false;
    }

    public void CreateEmptyCityBlock()
    {
        if (currentlyEditingBlock) return;
        
        CityBlock cityBlock = new CityBlock();
        cityBlocksData.Add(cityBlock);
        currentSelectedCityBlock++;
        currentlyEditingBlock = true;
        Transform cityBlockBuildingParent = new GameObject().transform;
        cityBlockBuildingParent.parent = transform;
        cityBlockBuildingParent.name = $"[BLOCK {currentSelectedCityBlock}] building parent";
        cityBlock.parent = cityBlockBuildingParent;
        cityBlockParent = cityBlockBuildingParent;
    }

    public void DiscardCurrentCityBlock()
    {
        cityBlocksData[currentSelectedCityBlock] = null;
        cityBlocksData.RemoveAt(currentSelectedCityBlock);
        currentSelectedCityBlock--;
    }

    private void addNodeToCityBlockCorners(Node pNode, Vector3 pMousePosition)
    {
        if (!isActive) return;
        if (pNode == null) return;

        if (!cityBlocksData[currentSelectedCityBlock].outerCorners.Contains(pNode.position))
            cityBlocksData[currentSelectedCityBlock].outerCorners.Add(pNode.position);
    }

    private void determineSpawnpointAction(Spawnpoint pSpawnpoint, Vector3 pMousePos, SpawnpointSelection pSelectionMode)
    {
        if (!isActive) return;
        if (pSpawnpoint == null) return;
        if (cityBlocksData.Count == 0) return;

        switch (pSelectionMode)
        {
            case SpawnpointSelection.Select:
                if (!cityBlocksData[currentSelectedCityBlock].spawnPoints.Contains(pSpawnpoint))
                    cityBlocksData[currentSelectedCityBlock].spawnPoints.Add(pSpawnpoint);
                break;
            case SpawnpointSelection.Deselect:
                if (cityBlocksData[currentSelectedCityBlock].spawnPoints.Contains(pSpawnpoint))
                    cityBlocksData[currentSelectedCityBlock].spawnPoints.Remove(pSpawnpoint);
                break;
        }
    }

    private void findCentroidOfBlock()
    {
        List<Vector3> outerCorners = cityBlocksData[currentSelectedCityBlock].outerCorners;

        currentCentroidPoint = GridHelperClass.GetCentroidOfArea(outerCorners);

        cityBlocksData[currentSelectedCityBlock].centroid = currentCentroidPoint;
        cityBlocksData[currentSelectedCityBlock].parent.position = currentCentroidPoint;
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
            Mesh spawnAreaMesh = cityBlocksData[i].spawnAreaMesh;

            if (centroid != Vector3.zero) Gizmos.DrawWireSphere(centroid, 2f);
            Gizmos.color = Color.cyan;
            if (spawnAreaMesh != null) Gizmos.DrawMesh(spawnAreaMesh, centroid, Quaternion.identity);
        }

        if (selectedCityBlockIndex > -1 && cityBlocksData.Count > 0)
        {
            if (selectedBuildingIndex > -1 && cityBlocksData[selectedCityBlockIndex].spawnedBuildings.Count > 0)
            {
                Gizmos.color = Color.green;

                ProceduralBuilding currentBuilding =
                    cityBlocksData[selectedCityBlockIndex].spawnedBuildings[selectedBuildingIndex];
                
                Vector3 buildingPosition = currentBuilding.gameObject.transform.position;
                Gizmos.DrawWireCube(buildingPosition, currentBuilding.size);
            }
        }
    }
}