using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CityBlockGenerator : FSM_State
{
    [SerializeField] private List<CityBlock> cityBlocksData = new List<CityBlock>();
    private int currentSelectedIndex = -1;
    private Vector3 currentCentroidPoint;
    private bool generatorActive;
    [SerializeField] private float innerOffset = 3f;

    private void Start()
    {
        NodeSelector.onNodeSelect.AddListener(addNodeToCityBlockCorners);
        UIManager.onCityBlockFinish.AddListener(FinishCityBlock);
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
        createMesh();
        calculateSpawnpoints();
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

    private void findCentroidOfBlock()
    {
        List<Vector3> outerCorners = cityBlocksData[currentSelectedIndex].outerCorners;
        
        currentCentroidPoint = GridHelperClass.GetCentroidOfArea(outerCorners);

        cityBlocksData[currentSelectedIndex].centroid = currentCentroidPoint;
    }

    private void createMesh()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = currentCentroidPoint;
        plane.transform.localScale = new Vector3(3, 1, 3);

        Mesh mesh = plane.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] vertices = mesh.vertices;

        Dictionary<int, Vector3> verticesClosestToNodes = new Dictionary<int, Vector3>();

        List<Vector3> innerCorners = cityBlocksData[currentSelectedIndex].innerCorners;
        List<int> vertexIndices = new List<int>();

        for (int i = 0; i < innerCorners.Count; i++)
        {
            Dictionary<Vector3, float> distancesVertexToNode = new Dictionary<Vector3, float>();
            for (int j = 0; j < vertices.Length; j++)
            {
                float distance = Vector3.Distance(vertices[j], innerCorners[i]);
                distancesVertexToNode.Add(vertices[j], distance);
            }
            //
            // Vector3 closestVertex = distancesVertexToNode.Min().Key;
            
        }
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
        
        //Start from centroid, go left/right until the x-position of the top node has been reached. 
        //
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