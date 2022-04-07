using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoadGenerator : FSM_State
{
    [SerializeField] private GameObject IntersectionPrefab;
    [SerializeField] private Material roadMaterial;

    public List<Node> allNodes;

    private Dictionary<string, Vector3> coordinates = new Dictionary<string, Vector3>();

    private void Start()
    {
        NodeEditor.eventTransferToRoadGenerator.AddListener(ReceiveNodeData);
    }

    public override void EnterState()
    {
        isActive = true;
    }

    public override void ExitState()
    {
        isActive = false;
    }

    public void ReceiveNodeData(List<Node> pNodes)
    {
        allNodes = pNodes;
    }

    public void InitializeRoads()
    {
        if (!isActive) return;

        CreateIntersections();
        createRoads();
        onModeExit.Invoke(FSM_States.GenerateRoads);
    }

    public void CreateIntersections()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node currentNode = allNodes[i];
            Vector3 spawnPosition = new Vector3(currentNode.position.x, currentNode.position.y + 0.5f,
                currentNode.position.z);

            GameObject intersection =
                Instantiate(IntersectionPrefab, spawnPosition, Quaternion.identity, currentNode.transform.GetChild(1));

            currentNode.intersectionRotation = intersection.transform.rotation;
            currentNode.InterSection = intersection;
            currentNode.UpdateVertexCoordinates();
        }
    }

    private void createRoads()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node currentNode = allNodes[i];

            for (int j = 0; j < currentNode.connectedNodes.Count; j++)
            {
                createPlaneMesh(currentNode, currentNode.connectedNodes[j]);
            }
        }
    }

    private void createPlaneMesh(Node pMainNode, Node pConnectedNode)
    {
        if (pConnectedNode.alreadyConnectedNodes.Contains(pMainNode)) return;

        coordinates.Clear();

        string otherNodeDirection = "";
        Vector3 direction = pConnectedNode.position - pMainNode.position;

        float forwardAngle = Vector3.Angle(direction, pMainNode.transform.forward);
        if (forwardAngle <= 45f)
        {
            otherNodeDirection = "North";
        }

        float rightAngle = Vector3.Angle(direction, pMainNode.transform.right);
        if (rightAngle <= 45f)
        {
            otherNodeDirection = "East";
        }

        float backAngle = Vector3.Angle(direction, pMainNode.transform.forward * -1);
        if (backAngle <= 45f)
        {
            otherNodeDirection = "South";
        }

        float leftAngle = Vector3.Angle(direction, pMainNode.transform.right * -1);
        if (leftAngle <= 45f)
        {
            otherNodeDirection = "West";
        }

        Vector3 node0Vertex1 = new Vector3();
        Vector3 node0Vertex2 = new Vector3();
        Vector3 node1Vertex1 = new Vector3();
        Vector3 node1Vertex2 = new Vector3();

        switch (otherNodeDirection)
        {
            case "North":
                node0Vertex1 = pMainNode.vertexCoordinates[0];
                node0Vertex2 = pMainNode.vertexCoordinates[1];

                node1Vertex1 = pConnectedNode.vertexCoordinates[3];
                node1Vertex2 = pConnectedNode.vertexCoordinates[2];
                break;
            case "East":
                node0Vertex1 = pMainNode.vertexCoordinates[2];
                node0Vertex2 = pMainNode.vertexCoordinates[0];

                node1Vertex1 = pConnectedNode.vertexCoordinates[1];
                node1Vertex2 = pConnectedNode.vertexCoordinates[3];
                break;
            case "South":
                node0Vertex1 = pMainNode.vertexCoordinates[3];
                node0Vertex2 = pMainNode.vertexCoordinates[2];

                node1Vertex1 = pConnectedNode.vertexCoordinates[0];
                node1Vertex2 = pConnectedNode.vertexCoordinates[1];
                break;
            case "West":
                node0Vertex1 = pMainNode.vertexCoordinates[1];
                node0Vertex2 = pMainNode.vertexCoordinates[3];

                node1Vertex1 = pConnectedNode.vertexCoordinates[2];
                node1Vertex2 = pConnectedNode.vertexCoordinates[0];
                break;
        }

        coordinates.Add("Node 0, vertex 1", node0Vertex1);
        coordinates.Add("Node 0, vertex 2", node0Vertex2);

        coordinates.Add("Node 1, vertex 1", node1Vertex1);
        coordinates.Add("Node 1, vertex 2", node1Vertex2);

        GameObject go = new GameObject("Road mesh for: " + pMainNode.name);
        MeshFilter meshFilter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer meshRenderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        meshRenderer.material = roadMaterial;

        Mesh mesh = new Mesh
        {
            vertices = coordinates.Values.ToArray(),
            uv = new[] {new Vector2(), new Vector2(), new Vector2(), new Vector2(),},
            triangles = new[] {0, 1, 2, 0, 2, 3}
        };

        meshFilter.mesh = mesh;

        mesh.name = "RoadMesh";
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        go.transform.SetParent(pMainNode.transform.GetChild(0));
        pConnectedNode.alreadyConnectedNodes.Add(pMainNode);
        pMainNode.alreadyConnectedNodes.Add(pConnectedNode);
    } 
}