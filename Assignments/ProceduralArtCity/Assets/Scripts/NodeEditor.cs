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

public class NodeEditor : MonoBehaviour
{
    private Camera cam;
    private Vector3 currentPoint;
    [SerializeField] private GameObject nodePrefab;
    private string currentMode;

    [SerializeField] private List<Node> allNodes = new List<Node>();
    private Node currentlySelectedNode;
    private Node firstNode;
    private Node secondNode;
    private Vector3 mousePositionOnGround;
    public Event_TransferNodeData eventTransferToNewFile { get; } = new Event_TransferNodeData();
    public Event_TransferNodeData eventTransferToExistingFile { get; } = new Event_TransferNodeData();
    public Event_TransferNodeData eventTransferToRoadGenerator = new Event_TransferNodeData();
    public static Event_OnResetSelection onResetSelection = new Event_OnResetSelection();

    private bool currentlyMovingNode;

    [SerializeField] private List<Vector3> outerCorners = new List<Vector3>();
    List<Vector3> positions = new List<Vector3>();
    private Vector3 cityCentroid;
    private float mostLeft;
    private float mostRight;
    private float mostTop;
    private float mostBottom;

    private void Start()
    {
        NodeSelector.onNodeSelect.AddListener(determineAction);
        UIManager.onClickNewMode.AddListener(ListenToModeChange);
        UIManager.onClickGenerateRoads.AddListener(ListenToRoadGenerationCommand);
    }

    public void ListenToModeChange(string pNewMode)
    {
        currentMode = pNewMode;

        if (currentMode == "StreetGeneration")
        {
            calculateOuterCorners();
            alignCornersToRectangle();
        }
    }

    public void ListenToRoadGenerationCommand()
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
        currentlySelectedNode = pNode;
        mousePositionOnGround = pMousePosition;

        switch (currentMode)
        {
            case "Place":
                createNode();
                break;
            case "Remove":
                removeNode();
                break;
            case "Move":
                moveNode();
                break;
            case "Connect":
                connectNode();
                break;
            case "Disconnect":
                disconnectNode();
                break;
            case "Default":
                Debug.Log("Not a valid mode for Node Editing.");
                break;
        }
    }

    private void calculateOuterCorners()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            positions.Add(allNodes[i].position);
        }

        cityCentroid = GridHelperClass.GetCentroidOfArea(positions);

        Dictionary<Vector3, float> distancesToCentroid = new Dictionary<Vector3, float>();
        for (int i = 0; i < positions.Count; i++)
        {
            float distance = Vector3.Distance(cityCentroid, positions[i]);
            distancesToCentroid.Add(positions[i], distance);
        }

        Dictionary<Vector3, float> highestDistances = distancesToCentroid.OrderByDescending(pair => pair.Value).Take(4)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        outerCorners = highestDistances.Keys.ToList();

        //Now the points are still sorted on distance. We want them sorted clockwise based on position, starting top-left.
        Vector3 currentTopLeft = cityCentroid;
        Vector3 currentTopRight = cityCentroid;
        Vector3 currentBottomRight = cityCentroid;
        Vector3 currentBottomLeft = cityCentroid;

        for (int i = 0; i < positions.Count; i++)
        {
            if (positions[i].x > currentTopRight.x && positions[i].z > currentTopRight.z)
                currentTopRight = positions[i];
            if (positions[i].x > currentBottomRight.x && positions[i].z < currentBottomRight.z)
            {
                currentBottomRight = positions[i];
                Debug.Log($"{currentBottomRight}");
            }

            if (positions[i].x < currentBottomLeft.x && positions[i].z < currentBottomLeft.z)
                currentBottomLeft = positions[i];
            if (positions[i].x < currentTopLeft.x && positions[i].z > currentTopLeft.z)
                currentTopLeft = positions[i];
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

        for (int i = 0; i < positions.Count; i++)
        {
            if (positions[i].x < mostLeft) mostLeft = positions[i].x;
            if (positions[i].x > mostRight) mostRight = positions[i].x;
            if (positions[i].z > mostTop) mostTop = positions[i].z;
            if (positions[i].z < mostBottom) mostBottom = positions[i].z;
        }

        //HORIZONTAL ALIGNMENT
        //Is the top left corner "less left" than the bottom left? Then relocate topleft.x to the bottomleft.x
        if (outerCorners[0].x < outerCorners[3].x)
            outerCorners[3] = new Vector3(outerCorners[0].x, 0.5f, outerCorners[3].z);
        //Else, relocate bottomleft.x to topleft.x
        else outerCorners[3] = new Vector3(outerCorners[0].x, 0.5f, outerCorners[3].z);

        if (outerCorners[0].x > mostLeft)
        {
            outerCorners[0] = new Vector3(mostLeft, 0.5f, outerCorners[0].z);
            outerCorners[3] = new Vector3(mostLeft, 0.5f, outerCorners[3].z);
        }
        
        if (outerCorners[1].x > mostRight)
        {
            outerCorners[1] = new Vector3(mostRight, 0.5f, outerCorners[1].z);
            outerCorners[2] = new Vector3(mostRight, 0.5f, outerCorners[2].z);
        }
        
        if (outerCorners[2].z < mostTop)
        {
            outerCorners[1] = new Vector3(outerCorners[1].x, 0.5f, mostTop);
            outerCorners[2] = new Vector3(outerCorners[2].x, 0.5f, mostTop);
        }
        
        if (outerCorners[3].z > mostBottom)
        {
            outerCorners[2] = new Vector3(outerCorners[2].x, 0.5f, mostBottom);
            outerCorners[3] = new Vector3(outerCorners[3].x, 0.5f, mostBottom);
        }

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
    }

    private void createNode()
    {
        GameObject GO_newNode = Instantiate(nodePrefab, mousePositionOnGround, Quaternion.identity, transform);
        GO_newNode.name = "Node " + allNodes.Count;

        Node node = GO_newNode.GetComponent<Node>();
        node.position = GO_newNode.transform.position;
        allNodes.Add(node);
    }

    private void removeNode()
    {
        if (currentlySelectedNode == null) return;

        List<Node> connectedNodes = currentlySelectedNode.connectedNodes;
        for (int i = 0; i < connectedNodes.Count; i++)
        {
            connectedNodes[i].connectedNodes.Remove(currentlySelectedNode);
        }

        allNodes.Remove(currentlySelectedNode);
        Destroy(currentlySelectedNode.gameObject);
    }

    private void moveNode()
    {
        if (currentlySelectedNode == null) return;

        currentlyMovingNode = true;
    }

    private void connectNode()
    {
        if (currentlySelectedNode == null) return;
        if (firstNode == null) firstNode = currentlySelectedNode;
        else
        {
            firstNode.connectedNodes.Add(currentlySelectedNode);
            currentlySelectedNode.connectedNodes.Add(firstNode);
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

    private void resetNodeSelection()
    {
        firstNode = null;
        secondNode = null;
        currentlySelectedNode = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (currentlySelectedNode != null) Handles.Label(currentlySelectedNode.position, "Currently selected node");

        for (int nodeInListIndex = 0; nodeInListIndex < allNodes.Count; nodeInListIndex++)
        {
            Vector3 currentNodePos = allNodes[nodeInListIndex].position;

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
            Vector3 labelPosition = outerCorners[i] + new Vector3(0, 10, 0);
            Handles.Label(labelPosition, $"index: {i}");
        }
    }
}