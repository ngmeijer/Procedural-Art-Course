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

            if (i != allNodes.Count - 1) currentNode.transform.LookAt(allNodes[i + 1].transform.position);

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
                createPlaneMesh(currentNode, currentNode.connectedNodes[j]);
            }
        }
    }

    private void createPlaneMesh(Node pMainNode, Node pConnectedNode)
    {
        if (alreadyRan) return;
        alreadyRan = true;
        coordinates.Add("Node 0, vertex 1", pMainNode.vertexCoordinates[0]);
        coordinates.Add("Node 0, vertex 2", pMainNode.vertexCoordinates[1]);

        coordinates.Add("Node 1, vertex 1", pConnectedNode.vertexCoordinates[0]);
        coordinates.Add("Node 1, vertex 2", pConnectedNode.vertexCoordinates[1]);

        // Vector3[] verticesTest1 = test1.GetComponent<MeshFilter>().sharedMesh.vertices;
        //
        // test1Coordinates = new[]
        // {
        //     test1.transform.TransformPoint(verticesTest1[0]),
        //     test1.transform.TransformPoint(verticesTest1[10]),
        //     test1.transform.TransformPoint(verticesTest1[110]),
        //     test1.transform.TransformPoint(verticesTest1[120]),
        // };
        //
        // test2Coordinates = new[]
        // {
        //     test2.transform.TransformPoint(verticesTest1[0]),
        //     test2.transform.TransformPoint(verticesTest1[10]),
        //     test2.transform.TransformPoint(verticesTest1[110]),
        //     test2.transform.TransformPoint(verticesTest1[120]),
        // };

        GameObject go = new GameObject("Plane");
        MeshFilter meshFilter = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer meshRenderer = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        Mesh mesh = new Mesh
        {
            vertices = coordinates.Values.ToArray(),
            uv = new Vector2[] {new Vector2(), new Vector2(), new Vector2(), new Vector2(),},
            triangles = new int[] {0, 1, 2, 0, 2, 3}
        };

        // Mesh mesh = new Mesh
        // {
        //     vertices = new Vector3[]
        //     {
        //         test1Coordinates[3], test1Coordinates[2], test2Coordinates[0], test2Coordinates[1]
        //     },
        //     uv = new Vector2[] {new Vector2(), new Vector2(), new Vector2(), new Vector2(),},
        //     triangles = new int[] {0, 1, 2, 0, 2, 3}
        // };


        meshFilter.mesh = mesh;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
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
        style.normal.textColor = Color.red;

        foreach (KeyValuePair<string, Vector3> vertexPos in coordinates)
        {
            Handles.Label(vertexPos.Value, $"Vertex {vertexPos.Value}, ID: {vertexPos.Key}", style);
        }
    }
}