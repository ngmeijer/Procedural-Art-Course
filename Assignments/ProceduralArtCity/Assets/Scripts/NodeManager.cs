using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]public class Event_TransferNodeData : UnityEvent<List<Node>> { }

public class NodeManager : MonoBehaviour
{
    private Camera cam;
    private Vector3 currentPoint;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private Transform nodeParent;
    private string currentMode;

    [SerializeField] private List<Node> allNodes = new List<Node>();
    private Node currentlySelectedNode;
    private GameObject currentlySelectedNodeGO;
    private Node firstNode;
    private Node secondNode;
    private Vector3 mousePositionOnGround;
    public Event_TransferNodeData eventTransferToNewFile { get; } = new Event_TransferNodeData();
    public Event_TransferNodeData eventTransferToExistingFile { get; } = new Event_TransferNodeData();
    public Event_TransferNodeData eventTransferToRoadGenerator = new Event_TransferNodeData();
    
    private bool currentlyMovingNode;

    void Start()
    {
        cam = Camera.main;
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

    private void Update()
    {
        castRay();
        
        if(Input.GetMouseButtonDown(1)) resetNodeSelection();

        if (currentMode == "Move" && currentlyMovingNode)
        {
            currentlySelectedNode.position = mousePositionOnGround;

            if (Input.GetMouseButtonUp(0)) currentlyMovingNode = false;
        }
    }

    private void resetNodeSelection()
    {
        firstNode = null;
        secondNode = null;
        currentlySelectedNode = null;
    }

    private void castRay()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            currentPoint = hit.point;

            mousePositionOnGround = new Vector3(currentPoint.x, 0, currentPoint.z);
            if (Input.GetMouseButtonDown(0))
            {
                determineAction(hit);
            }
        }
    }

    private void determineAction(RaycastHit pHit)
    {
        currentlyMovingNode = false;
        currentlySelectedNode = pHit.collider.gameObject.GetComponent<Node>();
        currentlySelectedNodeGO = pHit.collider.gameObject;
        
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
        }
    }

    private void createNode()
    {
        GameObject GO_newNode = Instantiate(nodePrefab, mousePositionOnGround, Quaternion.identity, nodeParent);
        GO_newNode.name = "Node" + allNodes.Count;

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