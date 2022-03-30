using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Event_OnSelectNode : UnityEvent<Node, Vector3>
{
}

public class NodeSelector : MonoBehaviour
{
    private Camera cam;
    private Node currentlySelectedNode;
    private NodeEditModes currentEditorMode;
    private bool currentlyMovingNode;
    private Node firstNode;
    private Node secondNode;
    public static Event_OnSelectNode onNodeSelect = new Event_OnSelectNode();


    void Start()
    {
        cam = Camera.main;
        NodeEditor.onResetSelection.AddListener(resetNodeSelection);
        GeneratorFSM.broadcastNodeEditModeChange.AddListener(changeMode);
    }

    private void Update()
    {
        castRay();
        
        if(Input.GetMouseButtonDown(1)) resetNodeSelection();
    }

    private void changeMode(NodeEditModes pNewMode)
    {
        currentEditorMode = pNewMode;
    }

    private void resetNodeSelection()
    {
        firstNode = null;
        secondNode = null;
        currentlySelectedNode = null;
        
        onNodeSelect.Invoke(null, Vector3.zero);
    }

    private void castRay()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (Input.GetMouseButtonDown(0) && currentEditorMode != NodeEditModes.MoveNode)
            {
                selectNode(hit);
            }
            else if (Input.GetMouseButton(0) && currentEditorMode == NodeEditModes.MoveNode) selectNode(hit);
        }
    }

    private void selectNode(RaycastHit pHit)
    {
        Vector3 mousePositionOnGround = new Vector3(pHit.point.x, 0, pHit.point.z);

        //Refactor to cache node gameobject position and pass the cached Node component.
        currentlySelectedNode = pHit.collider.gameObject.GetComponent<Node>();
        
        onNodeSelect.Invoke(currentlySelectedNode, mousePositionOnGround);
    }
}