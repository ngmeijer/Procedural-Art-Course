using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class Event_OnSelectNode : UnityEvent<Node, Vector3> { }

public class NodeSelector : MonoBehaviour
{
    private Camera cam;
    private Node currentlySelectedNode;
    private string currentEditorMode;
    private bool currentlyMovingNode;
    private Node firstNode;
    private Node secondNode;
    public static Event_OnSelectNode onNodeSelect = new Event_OnSelectNode();

    void Start()
    {
        cam = Camera.main;
        NodeEditor.onResetSelection.AddListener(resetNodeSelection);
    }

    private void Update()
    {
        castRay();
        
        if (currentEditorMode == "Move" && currentlyMovingNode)
        {
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
            if (Input.GetMouseButtonDown(0))
            {
                selectNode(hit);
            }
        }
    }

    private void selectNode(RaycastHit pHit)
    {
        Vector3 mousePositionOnGround = new Vector3(pHit.point.x, 0, pHit.point.z);
        
        //Refactor to cache node gameobject position and pass the cached Node component.
        Node currentlySelectedNode = pHit.collider.gameObject.GetComponent<Node>();
        
        onNodeSelect.Invoke(currentlySelectedNode, mousePositionOnGround);
    }
}