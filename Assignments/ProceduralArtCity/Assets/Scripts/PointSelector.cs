using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Event_OnSelectNode : UnityEvent<Node, Vector3>
{
}

[Serializable]
public class Event_OnSelectSpawnpoint : UnityEvent<Spawnpoint, Vector3>
{
}

public class PointSelector : MonoBehaviour
{
    private Camera cam;
    private Node currentlySelectedNode;
    private Spawnpoint currentlySelectedSpawnpoint;
    private Node_EditModes currentEditorMode;
    private FSM_States currentGenerationState;
    private bool currentlyMovingNode;
    private Node firstNode;
    private Node secondNode;
    public static Event_OnSelectNode onNodeSelect = new Event_OnSelectNode();
    public static Event_OnSelectSpawnpoint onSpawnpointSelect = new Event_OnSelectSpawnpoint();
    private ClickablePoint currentSelectedPointType;

    void Start()
    {
        cam = Camera.main;
        NodeEditor.onResetSelection.AddListener(resetNodeSelection);
        GeneratorFSM.broadcastNodeEditModeChange.AddListener(changeNodeMode);
        GeneratorFSM.broadcastGenerationModeChange.AddListener(changeGenerationMode);
    }

    private void Update()
    {
        castRay();

        if (Input.GetMouseButtonDown(1)) resetNodeSelection();
    }

    private void changeGenerationMode(FSM_States pNewState)
    {
        currentGenerationState = pNewState;
    }

    private void changeNodeMode(Node_EditModes pNewMode)
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
            if (Input.GetMouseButtonDown(0)) currentSelectedPointType = hit.collider.GetComponent<ClickablePoint>();
            if (!(currentSelectedPointType is Spawnpoint || currentSelectedPointType is Node) &&
                currentEditorMode != Node_EditModes.PlaceNode) return;

            if (currentSelectedPointType is Node || currentEditorMode == Node_EditModes.PlaceNode)
            {
                if (Input.GetMouseButtonDown(0) && currentEditorMode != Node_EditModes.MoveNode) selectNode(hit);
                else if (Input.GetMouseButton(0) && currentEditorMode == Node_EditModes.MoveNode) selectNode(hit);
            }

            if (currentGenerationState == FSM_States.GenerateCityBlocks && currentSelectedPointType is Spawnpoint)
            {
                if (Input.GetMouseButtonDown(0)) selectSpawnpoint(hit);
            }
        }
    }

    private void selectNode(RaycastHit pHit)
    {
        Vector3 mousePositionOnGround = new Vector3(pHit.point.x, 0, pHit.point.z);

        //Refactor to cache node gameobject position and pass the cached Node component.
        currentlySelectedNode = pHit.collider.gameObject.GetComponent<Node>();

        onNodeSelect.Invoke(currentlySelectedNode, mousePositionOnGround);
    }

    private void selectSpawnpoint(RaycastHit pHit)
    {
        Vector3 mousePositionOnGround = new Vector3(pHit.point.x, 0, pHit.point.z);

        currentlySelectedSpawnpoint = pHit.collider.gameObject.GetComponent<Spawnpoint>();

        MeshRenderer renderer = pHit.collider.gameObject.GetComponent<MeshRenderer>();
        renderer.material.color = Color.red;

        onSpawnpointSelect.Invoke(currentlySelectedSpawnpoint, mousePositionOnGround);
    }
}