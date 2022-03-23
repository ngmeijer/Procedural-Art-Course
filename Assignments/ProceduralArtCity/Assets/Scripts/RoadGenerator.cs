using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public GameObject IntersectionPrefab;
    public GameObject RoadPrefab;

    public List<Node> allNodes;
    public bool faultDetected;

    public List<Node> faultyIntersections = new List<Node>();

    public GameObject test1;
    public GameObject test2;

    public Vector3[] test1Coordinates;

    //public Vector3[] test2Coordinates;
    Dictionary<string, Vector3> coordinates = new Dictionary<string, Vector3>();
    public bool alreadyRan;

    public void InitializeStreets(List<Node> pNodes)
    {
        allNodes = pNodes;

        CreateIntersections();
        CreateRoads();
    }

    public void CreateIntersections()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node currentNode = allNodes[i];
            Vector3 spawnPosition = new Vector3(currentNode.position.x, currentNode.position.y + 0.5f,
                currentNode.position.z);

            GameObject intersection =
                Instantiate(IntersectionPrefab, spawnPosition, Quaternion.identity, currentNode.gameObject.transform);

            currentNode.intersectionRotation = intersection.transform.rotation;
            currentNode.InterSection = intersection;

            currentNode.UpdateVertexCoordinates();
        }

        faultyIntersections.Add(allNodes[1]);
    }

    public void CreateRoads()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node currentNode = allNodes[i];

            for (int j = 0; j < currentNode.connectedNodes.Count; j++)
            {
                Debug.Log("running loop");
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
        
        Debug.Log("Created road");

        GameObject go = new GameObject("Road mesh for: " + pMainNode.name);
        MeshFilter meshFilter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer meshRenderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        Mesh mesh = new Mesh
        {
            vertices = coordinates.Values.ToArray(),
            uv = new Vector2[] {new Vector2(), new Vector2(), new Vector2(), new Vector2(),},
            triangles = new int[] {0, 1, 2, 0, 2, 3}
        };

        meshFilter.mesh = mesh;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        
        go.transform.SetParent(pMainNode.transform);
        pConnectedNode.alreadyConnectedNodes.Add(pMainNode);
        pMainNode.alreadyConnectedNodes.Add(pConnectedNode);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < allNodes.Count; i++)
        {
            Node currentNode = allNodes[i];

            Vector3 labelPosition = new Vector3(currentNode.position.x, currentNode.position.y + 0.5f,
                currentNode.position.z);
            if (currentNode.connectedNodes.Count != 0)
                Handles.Label(labelPosition,
                    $"Node ID: {currentNode.name}\nDistance to {currentNode.connectedNodes[0]}: " +
                    currentNode.distanceToNextNode);
        }

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;

        foreach (KeyValuePair<string, Vector3> vertexPos in coordinates)
        {
            Handles.Label(vertexPos.Value, $"Vertex {vertexPos.Value}, ID: {vertexPos.Key}", style);
        }
    }
}