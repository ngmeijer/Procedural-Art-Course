using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]public class Event_TransferNodeData : UnityEvent<List<Node>> { }
[Serializable]public class Event_OnPlaceNode : UnityEvent<Node>{}
[Serializable]public class Event_OnRemoveNode : UnityEvent<Node>{}
[Serializable]public class Event_OnResetSelection : UnityEvent{}

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

    private void Start()
    {
        NodeSelector.onNodeSelect.AddListener(determineAction);
        UIManager.onClickNewMode.AddListener(ListenToModeChange);
        UIManager.onClickGenerateRoads.AddListener(ListenToRoadGenerationCommand);
    }

    public void ListenToModeChange(string pNewMode)
    {
        currentMode = pNewMode;
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
        
        if(currentlySelectedNode != null) Handles.Label(currentlySelectedNode.position, "Currently selected node");
        
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
    }
}