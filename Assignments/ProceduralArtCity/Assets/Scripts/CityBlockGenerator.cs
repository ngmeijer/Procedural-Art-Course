using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CityBlockGenerator : MonoBehaviour
{
    [SerializeField] private List<CityBlock> cityBlocksData = new List<CityBlock>();
    private int currentSelectedIndex = -1;
    private Vector3 currentCentroidPoint;
    private bool generatorActive;
    [SerializeField] private float innerOffset = 3f;

    private void Start()
    {
        UIManager.onClickNewMode.AddListener(checkMode);
        NodeSelector.onNodeSelect.AddListener(addNodeToCityBlockCorners);
        UIManager.onCityBlockFinish.AddListener(createCityBlock);
        createEmptyCityBlock();
    }

    private void createCityBlock()
    {
        findCentroidOfBlock();
        calculateInnerCorners();
        createEmptyCityBlock();
        calculateSpawnpoints();
    }

    private void checkMode(string pNewMode)
    {
        if (pNewMode != "CityBlockGeneration") return;

        generatorActive = true; 
    }

    private void createEmptyCityBlock()
    {
        CityBlock cityBlock = new CityBlock();
        cityBlocksData.Add(cityBlock);
        currentSelectedIndex++;
    }

    private void addNodeToCityBlockCorners(Node pNode, Vector3 pMousePosition)
    {
        if (!generatorActive) return;
        if (pNode == null) return;

        if (!cityBlocksData[currentSelectedIndex].outerCorners.Contains(pNode.position))
            cityBlocksData[currentSelectedIndex].outerCorners.Add(pNode.position);
    }

    private void findCentroidOfBlock()
    {
        List<Vector3> outerCorners = cityBlocksData[currentSelectedIndex].outerCorners;

        Debug.Log(outerCorners.Count);
        
        Vector3 centroid = new Vector3();
        centroid.y = 0.5f;

        float sumX = 0;
        float sumZ = 0;
        for (int i = 0; i < outerCorners.Count; i++)
        {
            Debug.Log(outerCorners[i]);
            sumX += outerCorners[i].x;
            sumZ += outerCorners[i].z;
        }

        centroid.x = sumX / outerCorners.Count;
        centroid.z = sumZ / outerCorners.Count;

        cityBlocksData[currentSelectedIndex].centroid = centroid;
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

    private void calculateSpawnpoints()
    {
        int gridWidth = 0;
        int gridHeight = 0;
        
        int spawnpointCountX = 0;
        int spawnpointCountY = 0;
    }
    

    private void OnDrawGizmos()
    {
        for (int i = 0; i < cityBlocksData.Count; i++)
        {
            Gizmos.color = Color.yellow;

            Vector3 centroid = cityBlocksData[i].centroid;
            Mesh spawnAreaMesh = cityBlocksData[i].spawnAreaMesh;
            
            if(centroid != Vector3.zero) Gizmos.DrawSphere(centroid, 2f);
            Gizmos.color = Color.cyan;
            if(spawnAreaMesh != null) Gizmos.DrawMesh(spawnAreaMesh, centroid, Quaternion.identity);
            
            
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