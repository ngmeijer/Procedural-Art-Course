using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Event_TransferNodeData : UnityEvent<List<Node>>
{
}

[Serializable]
public class Event_OnPlaceNode : UnityEvent<Node>
{
}

[Serializable]
public class Event_OnRemoveNode : UnityEvent<Node>
{
}

[Serializable]
public class Event_OnResetSelection : UnityEvent
{
}

public class NodeEditor : FSM_State
{
    private Camera cam;
    private Vector3 currentPoint;
    private GameObject nodePrefab;
    private GameObject spawnpointPrefab;
    public Node_EditModes CurrentMode;

    [SerializeField] private List<Node> allNodes = new List<Node>();
    private Node currentlySelectedNode;
    private Node firstNode;
    private Node secondNode;
    private Vector3 mousePositionOnGround;
    public Event_TransferNodeData eventTransferToNewFile { get; } = new Event_TransferNodeData();
    public Event_TransferNodeData eventTransferToExistingFile { get; } = new Event_TransferNodeData();
    public static Event_TransferNodeData eventTransferToRoadGenerator = new Event_TransferNodeData();
    public static Event_OnResetSelection onResetSelection = new Event_OnResetSelection();
    [HideInInspector] public UnityEvent<Node_EditModes> onSelectNewMode = new UnityEvent<Node_EditModes>();

    private bool currentlyMovingNode;

    private List<Vector3> outerCorners = new List<Vector3>();
    private List<Vector3> nodePositions = new List<Vector3>();
    private Vector3 cityCentroid;
    private float mostLeft;
    private float mostRight;
    private float mostTop;
    private float mostBottom;
    private List<Spawnpoint> spawnPointsList = new List<Spawnpoint>();

    [SerializeField] private Vector3 buildingSize;
    [SerializeField] private Vector3 buildingOffset;
    public bool HasCalculatedSpawnpoints;

    private void Start()
    {
        PointSelector.onNodeSelect.AddListener(determineAction);

        nodePrefab = Resources.Load<GameObject>("Prefabs/NodeInstance");
        spawnpointPrefab = Resources.Load<GameObject>("Prefabs/SpawnpointInstance");
    }

    public override void EnterState()
    {
        isActive = true;
    }

    public override void ExitState()
    {
        isActive = false;
    }

    public void RecalculateSpawnpoints()
    {
        clearOldData();
        destroyUnconnectedNodes();
        calculateOuterCorners();
        alignCornersToRectangle();
        createSpawnpoints();

        HasCalculatedSpawnpoints = true;
    }

    private void Update()
    {
        if (!isActive) return;

        if (currentlyMovingNode && Input.GetMouseButtonUp(0))
        {
            currentlyMovingNode = false;
            onResetSelection.Invoke();
            resetNodeSelection();
        }
    }

    private void clearOldData()
    {
        nodePositions.Clear();
        outerCorners.Clear();
        spawnPointsList.Clear();
    }

    public void transferNodes()
    {
        eventTransferToRoadGenerator.Invoke(allNodes);
    }

    private void transferToNewFile()
    {
        eventTransferToNewFile.Invoke(allNodes);
    }

    private void transferToExistingFile()
    {
        eventTransferToExistingFile.Invoke(allNodes);
    }

    private void determineAction(Node pNode, Vector3 pMousePosition)
    {
        if (!isActive) return;
        
        currentlySelectedNode = pNode;
        mousePositionOnGround = pMousePosition;

        if (pMousePosition == Vector3.zero) return;
        if (pNode == null && CurrentMode != Node_EditModes.PlaceNode) return;

        switch (CurrentMode)
        {
            case Node_EditModes.PlaceNode:
                createNode();
                break;
            case Node_EditModes.RemoveNode:
                removeNode();
                allNodes.Remove(pNode);
                Destroy(pNode.gameObject);
                break;
            case Node_EditModes.MoveNode:
                currentlyMovingNode = true;
                moveNode();
                break;
            case Node_EditModes.ConnectNode:
                connectNode();
                break;
            case Node_EditModes.DisconnectNode:
                disconnectNode();
                break;
        }
    }

    private void calculateOuterCorners()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            nodePositions.Add(allNodes[i].position);
        }

        cityCentroid = GridHelperClass.GetCentroidOfArea(nodePositions);

        Dictionary<Vector3, float> distancesToCentroid = new Dictionary<Vector3, float>();
        for (int i = 0; i < nodePositions.Count; i++)
        {
            float distance = Vector3.Distance(cityCentroid, nodePositions[i]);
            distancesToCentroid.Add(nodePositions[i], distance);
        }

        Dictionary<Vector3, float> highestDistances = distancesToCentroid.OrderByDescending(pair => pair.Value).Take(4)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        outerCorners = highestDistances.Keys.ToList();

        //Now the points are still sorted on distance. We want them sorted clockwise based on position, starting top-left.
        Vector3 currentTopLeft = cityCentroid;
        Vector3 currentTopRight = cityCentroid;
        Vector3 currentBottomRight = cityCentroid;
        Vector3 currentBottomLeft = cityCentroid;

        for (int i = 0; i < nodePositions.Count; i++)
        {
            if (nodePositions[i].x > currentTopRight.x && nodePositions[i].z > currentTopRight.z)
                currentTopRight = nodePositions[i];
            if (nodePositions[i].x > currentBottomRight.x && nodePositions[i].z < currentBottomRight.z)
            {
                currentBottomRight = nodePositions[i];
            }

            if (nodePositions[i].x < currentBottomLeft.x && nodePositions[i].z < currentBottomLeft.z)
                currentBottomLeft = nodePositions[i];
            if (nodePositions[i].x < currentTopLeft.x && nodePositions[i].z > currentTopLeft.z)
                currentTopLeft = nodePositions[i];
        }

        outerCorners.Clear();
        outerCorners.Insert(0, currentTopLeft);
        outerCorners.Insert(1, currentTopRight);
        outerCorners.Insert(2, currentBottomRight);
        outerCorners.Insert(3, currentBottomLeft);
    }

    private void alignCornersToRectangle()
    {
        mostLeft = cityCentroid.x;
        mostRight = cityCentroid.x;
        mostTop = cityCentroid.z;
        mostBottom = cityCentroid.z;

        for (int i = 0; i < nodePositions.Count; i++)
        {
            if (nodePositions[i].x < mostLeft) mostLeft = nodePositions[i].x;
            if (nodePositions[i].x > mostRight) mostRight = nodePositions[i].x;
            if (nodePositions[i].z > mostTop) mostTop = nodePositions[i].z;
            if (nodePositions[i].z < mostBottom) mostBottom = nodePositions[i].z;
        }

        //HORIZONTAL ALIGNMENT
        //Is the top left corner "less left" than the bottom left? Then relocate topleft.x to the bottomleft.x
        if (outerCorners[0].x < outerCorners[3].x)
            outerCorners[3] = new Vector3(outerCorners[0].x, 0.5f, outerCorners[3].z);
        //Else, relocate bottomleft.x to topleft.x
        else outerCorners[3] = new Vector3(outerCorners[0].x, 0.5f, outerCorners[3].z);

        //Is the top right corner "less right" than the bottom right? Then relocate topright.x to bottomright.x
        if (outerCorners[1].x < outerCorners[2].x)
            outerCorners[1] = new Vector3(outerCorners[2].x, 0.5f, outerCorners[1].z);
        //Else, relocate bottomright.x to topright.x
        else outerCorners[2] = new Vector3(outerCorners[1].x, 0.5f, outerCorners[2].z);

        //VERTICAL ALIGNMENT
        //Is the top left corner lower than the top right corner? Then relocate topleft.z to topright.z
        if (outerCorners[0].z > outerCorners[1].z)
            outerCorners[1] = new Vector3(outerCorners[1].x, 0.5f, outerCorners[0].z);
        //Else, relocate topright.z to topleft.z
        else outerCorners[0] = new Vector3(outerCorners[0].x, 0.5f, outerCorners[1].z);

        if (outerCorners[2].z > outerCorners[3].z)
            outerCorners[3] = new Vector3(outerCorners[3].x, 0.5f, outerCorners[2].z);
        else outerCorners[2] = new Vector3(outerCorners[2].x, 0.5f, outerCorners[3].z);

        ///////
        if (outerCorners[0].x > mostLeft || outerCorners[3].x > mostLeft)
        {
            outerCorners[0] = new Vector3(mostLeft, 0.5f, outerCorners[0].z);
            outerCorners[3] = new Vector3(mostLeft, 0.5f, outerCorners[3].z);
        }

        if (outerCorners[1].x < mostRight || outerCorners[2].x < mostRight)
        {
            outerCorners[1] = new Vector3(mostRight, 0.5f, outerCorners[1].z);
            outerCorners[2] = new Vector3(mostRight, 0.5f, outerCorners[2].z);
        }

        if (outerCorners[0].z < mostTop || outerCorners[1].z < mostTop)
        {
            outerCorners[0] = new Vector3(outerCorners[0].x, 0.5f, mostTop);
            outerCorners[1] = new Vector3(outerCorners[1].x, 0.5f, mostTop);
        }

        if (outerCorners[2].z > mostBottom || outerCorners[3].z > mostBottom)
        {
            outerCorners[2] = new Vector3(outerCorners[2].x, 0.5f, mostBottom);
            outerCorners[3] = new Vector3(outerCorners[3].x, 0.5f, mostBottom);
        }
    }

    private void createSpawnpoints()
    {
        int gridWidth = Mathf.CeilToInt(outerCorners[1].x - outerCorners[0].x);
        int gridHeight = Mathf.CeilToInt(outerCorners[1].z - outerCorners[2].z);

        int xDivision = Mathf.CeilToInt(buildingSize.x + buildingOffset.x);
        int zDivision = Mathf.CeilToInt(buildingSize.z + buildingOffset.z);

        int countX = gridWidth / xDivision;
        int countZ = gridHeight / zDivision;

        Vector3 startPosition = outerCorners[0];

        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                Vector3 position = new Vector3(startPosition.x + (buildingSize.x + buildingOffset.x) * x,
                    0f,
                    startPosition.z - (buildingSize.z + buildingOffset.z) * z);
                GameObject newSpawnpointGO = Instantiate(spawnpointPrefab, position, Quaternion.identity, this.transform);
                Spawnpoint newSpawnpointData = newSpawnpointGO.GetComponent<Spawnpoint>();
                
                newSpawnpointData.position = position;

                spawnPointsList.Add(newSpawnpointData);
            }
        }
    }

    private void createNode()
    {
        GameObject GO_newNode = Instantiate(nodePrefab, mousePositionOnGround, Quaternion.identity, transform);
        GO_newNode.name = "Node " + allNodes.Count;

        Node node = GO_newNode.GetComponent<Node>();
        node.position = GO_newNode.transform.position;
        allNodes.Add(node);
    }

    private void removeNode(Node pNode = null)
    {
        if (currentlySelectedNode == null) return;

        if (pNode != null) currentlySelectedNode = pNode;

        List<Node> connectedNodes = currentlySelectedNode.connectedNodes;
        for (int i = 0; i < connectedNodes.Count; i++)
        {
            connectedNodes[i].connectedNodes.Remove(currentlySelectedNode);
        }
    }

    private void moveNode()
    {
        if (!currentlyMovingNode) return;
        if (currentlySelectedNode == null) return;

        currentlySelectedNode.position = mousePositionOnGround;
        currentlySelectedNode.gameObject.transform.position = mousePositionOnGround;
    }

    private void connectNode()
    {
        if (firstNode == null)
        {
            firstNode = currentlySelectedNode;
            return;
        }

        if (firstNode != null && secondNode == null) secondNode = currentlySelectedNode;

        if (firstNode != null && secondNode != null)
        {
            firstNode.connectedNodes.Add(currentlySelectedNode);
            secondNode.connectedNodes.Add(firstNode);

            resetNodeSelection();
        }
    }

    private void disconnectNode()
    {
        if (currentlySelectedNode == null) return;
        if (firstNode == null) firstNode = currentlySelectedNode;
        else
        {
            firstNode.connectedNodes.Remove(currentlySelectedNode);
            currentlySelectedNode.connectedNodes.Remove(firstNode);
            resetNodeSelection();
        }
    }

    private void destroyUnconnectedNodes()
    {
        List<Node> nodesToRemove = new List<Node>();
        for (int i = 0; i < allNodes.Count; i++)
        {
            if (allNodes[i].connectedNodes.Count <= 1)
            {
                nodesToRemove.Add(allNodes[i]);
            }
        }

        for (int i = 0; i < nodesToRemove.Count; i++)
        {
            allNodes[i].connectedNodes.Remove(nodesToRemove[i]);
            allNodes.Remove(nodesToRemove[i]);
            Destroy(nodesToRemove[i].gameObject);
        }
    }

    private void resetNodeSelection()
    {
        firstNode = null;
        secondNode = null;
        currentlySelectedNode = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int nodeInListIndex = 0; nodeInListIndex < allNodes.Count; nodeInListIndex++)
        {
            Vector3 currentNodePos = allNodes[nodeInListIndex].position;

            if (allNodes[nodeInListIndex] == currentlySelectedNode) Gizmos.color = Color.magenta;
            else Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentNodePos, 1.5f);

            if (allNodes[nodeInListIndex].connectedNodes.Count == 0) continue;

            for (int connectionsInNode = 0;
                connectionsInNode < allNodes[nodeInListIndex].connectedNodes.Count;
                connectionsInNode++)
            {
                Vector3 connectedNodePos =
                    allNodes[nodeInListIndex].connectedNodes[connectionsInNode].position;


                Debug.DrawLine(currentNodePos, connectedNodePos);
            }
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < outerCorners.Count; i++)
        {
            Gizmos.DrawSphere(outerCorners[i], 2f);
            Vector3 labelPosition = outerCorners[i] + new Vector3(0, 10, 5);
            Handles.Label(labelPosition, $"index: {i}");

            int previousIndex = i - 1;
            if (previousIndex < 0) previousIndex = outerCorners.Count - 1;

            int currentIndex = i;

            int nextIndex = i + 1;
            if (nextIndex > outerCorners.Count - 1) nextIndex = 0;

            Gizmos.DrawLine(outerCorners[currentIndex], outerCorners[nextIndex]);
            Gizmos.DrawLine(outerCorners[currentIndex], outerCorners[previousIndex]);
        }
    }
}