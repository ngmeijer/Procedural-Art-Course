using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class CityBlockGenerator : FSM_State
{
    [SerializeField] private List<CityBlock> cityBlocksData = new List<CityBlock>();
    private int currentSelectedIndex = -1;
    private Vector3 currentCentroidPoint;
    private bool generatorActive;
    private Vector3 mousePositionOnGround;
    [SerializeField] private float innerOffset = 3f;

    [SerializeField] private List<Vector3> planeOuterVertices = new List<Vector3>();
    [SerializeField] private Vector3 buildingSize = new Vector3(5,5,5);
    [SerializeField] private Vector3 buildingOffset = new Vector3(2, 2, 2);

    [SerializeField] private GameObject buildingPrefab;
    
    private int[] vertexIndices = new int[]
    {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 21, 32, 43, 54, 65, 76, 87, 98, 109, 120,
        11, 22, 33, 44, 55, 66, 77, 88, 99, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119
    };

    private Dictionary<int, Vector3> planeClosestVertices = new Dictionary<int, Vector3>();

    private void Start()
    {
        PointSelector.onNodeSelect.AddListener(addNodeToCityBlockCorners);
        PointSelector.onSpawnpointSelect.AddListener(addSpawnpointToCityBlock);
        UIManager.onCityBlockFinish.AddListener(FinishCityBlock);
        
        buildingPrefab = Resources.Load<GameObject>("Prefabs/Building");
    }

    public override void EnterState()
    {
        isActive = true;
    }

    public override void ExitState()
    {
        isActive = false;
    }

    public void FinishCityBlock()
    {
        findCentroidOfBlock();
        calculateInnerCorners();

        CityBlock currentBlock = cityBlocksData[currentSelectedIndex];
        for (int i = 0; i < currentBlock.spawnPoints.Count; i++)
        {
            GameObject testBuilding = Instantiate(buildingPrefab, currentBlock.spawnPoints[i].position,
                Quaternion.identity);

            testBuilding.transform.localScale = buildingSize;
        }
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