using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class Event_TransferNodeData : UnityEvent<List<Node>> { }

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
    
    private bool currentlyMovingNode;
    
    private static NodeManager _instance;
    public static NodeManager Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    void Start()
    {
        cam = Camera.main;
        UIManager.Instance.onClickCreateSaveFile.AddListener(transferToNewFile);
        UIManager.Instance.onClickSaveToFile.AddListener(transferToExistingFile);
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
        currentMode = UIManager.Instance.currentMode;

        castRay();
        
        if(Input.GetMouseButtonDown(1)) resetNodeSelection();

        if (currentMode == "Move" && currentlyMovingNode)
        {
            currentlySelectedNode.thisNodePosition = mousePositionOnGround;

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
        if (Physics.Raycast(ray, out hit, 100))
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
        node.thisNodePosition = GO_newNode.transform.position;
        allNodes.Add(node);
    }

    private void removeNode()
    {
        //Before destroying, remove itself from each connected nodes' list.
        if (currentlySelectedNode == null) return;

        List<Node> connectedNodes = currentlySelectedNode.connectedNodes;
        for (int i = 0; i < connectedNodes.Count; i++)
        {
            connectedNodes[i].connectedNodes.Remove(currentlySelectedNode);
        }
        
        allNodes.Remove(currentlySelectedNode);
        Destroy(currentlySelectedNode);
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
        
        if(currentlySelectedNode != null) Handles.Label(currentlySelectedNode.thisNodePosition, "Currently selected node");

        for (int nodeInListIndex = 0; nodeInListIndex < allNodes.Count; nodeInListIndex++)
        {
            Vector3 currentNodePos = allNodes[nodeInListIndex].thisNodePosition;

            Gizmos.DrawSphere(allNodes[nodeInListIndex].thisNodePosition, 0.05f);

            if (allNodes[nodeInListIndex].connectedNodes.Count == 0) continue;

            for (int connectionsInNode = 0;
                connectionsInNode < allNodes[nodeInListIndex].connectedNodes.Count;
                connectionsInNode++)
            {
                Vector3 connectedNodePos =
                    allNodes[nodeInListIndex].connectedNodes[connectionsInNode].thisNodePosition;
            
                Debug.DrawLine(currentNodePos, connectedNodePos);
            }
        }
    }
}