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
    private int currentSelectedIndex = -1;
    private Vector3 currentCentroidPoint;
    
    [SerializeField] private float innerOffset = 3f;
    [SerializeField] private Vector3 buildingSize = new Vector3(5,5,5);
    [SerializeField] private Vector3 buildingOffset = new Vector3(2, 2, 2);
    
    private Dictionary<int, Vector3> planeClosestVertices = new Dictionary<int, Vector3>();

    private void Awake()
    {
        PointSelector.onNodeSelect.AddListener(addNodeToCityBlockCorners);
        PointSelector.onSpawnpointSelect.AddListener(addSpawnpointToCityBlock);
        UIManager.onCityBlockFinish.AddListener(FinishCityBlock);
    }

    private void Start()
    {
        onBuildingSettingsChanged.Invoke(buildingSize, buildingOffset);
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
        findCentroidOfBlock();
        calculateInnerCorners();
    }

    public void CreateEmptyCityBlock()
    {
        CityBlock cityBlock = new CityBlock();
        cityBlocksData.Add(cityBlock);
        currentSelectedIndex++;
    }

    public void DiscardCurrentCityBlock()
    {
        cityBlocksData.RemoveAt(cityBlocksData.Count - 1);
        currentSelectedIndex--;
    }

    private void addNodeToCityBlockCorners(Node pNode, Vector3 pMousePosition)
    {
        if (!isActive) return;
        if (pNode == null) return;

        if (!cityBlocksData[currentSelectedIndex].outerCorners.Contains(pNode.position))
            cityBlocksData[currentSelectedIndex].outerCorners.Add(pNode.position);
    }

    private void addSpawnpointToCityBlock(Spawnpoint pSpawnpoint, Vector3 pMousePos)
    {
        if (!isActive) return;
        if (pSpawnpoint == null) return;
        
        if (!cityBlocksData[currentSelectedIndex].spawnPoints.Contains(pSpawnpoint))
            cityBlocksData[currentSelectedIndex].spawnPoints.Add(pSpawnpoint);
    }

    private void findCentroidOfBlock()
    {
        List<Vector3> outerCorners = cityBlocksData[currentSelectedIndex].outerCorners;

        currentCentroidPoint = GridHelperClass.GetCentroidOfArea(outerCorners);

        cityBlocksData[currentSelectedIndex].centroid = currentCentroidPoint;
    }

    private void calculateInnerCorners()
    {
        List<Vector3> outerCorners = cityBlocksData[currentSelectedIndex].outerCorners;
        Vector3 centroid = cityBlocksData[currentSelectedIndex].centroid;
        for (int i = 0; i < outerCorners.Count; i++)
        {
            Vector3 directionVector = outerCorners[i] - centroid;
            float originalLength = directionVector.magnitude;
            directionVector.Normalize();

            float relativeOffset = innerOffset / originalLength;

            float newLength = originalLength - (originalLength * relativeOffset);
            directionVector *= newLength;

            Vector3 innerCorner = directionVector + centroid;
            cityBlocksData[currentSelectedIndex].innerCorners.Add(innerCorner);
        }
    }

    private void OnValidate()
    {
        onBuildingSettingsChanged.Invoke(buildingSize, buildingOffset);
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

            List<Vector3> innerCorners = cityBlocksData[i].innerCorners;
            for (int j = 0; j < innerCorners.Count; j++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(innerCorners[j], 1f);

                int previousIndex = j - 1;
                if (previousIndex < 0) previousIndex = innerCorners.Count - 1;

                int currentIndex = j;

                int nextIndex = j + 1;
                if (nextIndex > innerCorners.Count - 1) nextIndex = 0;

                Gizmos.DrawLine(innerCorners[currentIndex], innerCorners[nextIndex]);
                Gizmos.DrawLine(innerCorners[currentIndex], innerCorners[previousIndex]);
            }
        }
    }
}